using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    private static readonly string CaseRuntimeDirectory = WorkspaceRuntimePaths.DirectoryName.ToUpperInvariant();
    private static readonly string CaseRuntimeFile = CaseRuntimeDirectory + "/" + NotesFileName;
    private static readonly string CaseRuntimeNewFile = CaseRuntimeDirectory + "/" + NewFile;
    private static readonly string CaseSiblingFile = CaseRuntimeDirectory + SiblingSuffix + "/" + FileName;

    [Theory(DisplayName = "Include refuses case-variant runtime targets before decoding their content")]
    [InlineData(false), InlineData(true)]
    public void IncludeRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-include");
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var target = CaseRuntimeFile;
        if (linked)
        {
            target = AliasFile;
            Assert.True(files.TryCreateFileSymbolicLink(target, files.Combine(CaseRuntimeFile), out var cause), cause);
        }
        var request = new RouteInclusionRequest { Workspace = Workspace(files), File = target, Action = RouteInclusionAction.Include };

        var exception = Assert.Throws<ArgumentException>(() => RouteInclusionTargetResolver.Resolve(request));

        MatchSnapshot(exception.Message, $"linked-{linked}.content");
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        Assert.False(File.Exists(Path.Combine(files.Path, CaseRuntimeDirectory, Path.GetFileName(WorkspaceRuntimePaths.IgnorePath(Workspace(files))))));
    }

    [Theory(DisplayName = "Link destinations refuse case-variant runtime paths without invoking the content reader")]
    [InlineData(false), InlineData(true)]
    public async Task LinkDestinationRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-link");
        SeedWorkspace(files);
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        files.CreateFile(SafeFile, SafeDocumentContent);
        var target = CaseRuntimeFile;
        if (linked)
        {
            target = AliasFile;
            Assert.True(files.TryCreateFileSymbolicLink(target, files.Combine(CaseRuntimeFile), out var cause), cause);
        }
        var workspace = Workspace(files);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(workspace, Token);
        var reads = new List<string>();
        var resolver = new SourceLinkDestinationResolver(new PhysicalPathResolver(), (physical, logical, token) =>
        {
            reads.Add(logical);
            return StrictUtf8FileReader.ReadAsync(physical, logical, token);
        }, new MarkdownDocumentParser().Parse);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = RootCatalogue,
            RawDestination = $"../../{target}{HistoryFragment}",
        };

        var result = await resolver.ResolveAsync(input, Token);

        Assert.Equal(SourceLinkTargetResolution.Unreadable, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, result.Finding?.Code);
        MatchSnapshot(Assert.IsType<string>(result.Finding?.Cause), "rejection.content");
        Assert.Empty(reads);
        var safe = await resolver.ResolveAsync(input with { RawDestination = SafeDestination }, Token);
        Assert.Equal(SourceLinkTargetResolution.Complete, safe.Target.Resolution);
        Assert.Equal([SafeFile], reads);
        Assert.False(File.Exists(Path.Combine(files.Path, CaseRuntimeDirectory, Path.GetFileName(WorkspaceRuntimePaths.IgnorePath(Workspace(files))))));
    }

    [Theory(DisplayName = "Package destinations refuse existing and prospective case-variant runtime paths")]
    [InlineData(false), InlineData(true)]
    public void PackageDestinationRejectsRuntimeCaseAlias(bool existing)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-package");
        if (existing) files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var destination = existing ? CaseRuntimeFile : CaseRuntimeNewFile;
        var workspace = Workspace(files);
        var registration = LibraryRegistration.Create(LibraryId.Create(LibraryName), WorkspaceRelativeDirectory.Create(LibrarySourceDirectory),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);

        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, destination));
        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(workspace, registration, registered: null, generatedTargets: []));

        if (existing) Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        else Assert.False(Directory.Exists(files.Combine(CaseRuntimeDirectory)));
        Assert.True(ExtensionDestinationPolicy.IsAllowed(workspace, CaseSiblingFile));
        Assert.False(File.Exists(Path.Combine(files.Path, CaseRuntimeDirectory, Path.GetFileName(WorkspaceRuntimePaths.IgnorePath(Workspace(files))))));
    }

    [Theory(DisplayName = "Remove refuses case-variant runtime targets before capturing file content")]
    [InlineData(false), InlineData(true)]
    public async Task RemovalRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-remove");
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var root = CaseRuntimeDirectory;
        if (linked)
        {
            root = AliasDirectory;
            Assert.True(files.TryCreateDirectorySymbolicLink(root, files.Combine(CaseRuntimeDirectory), out var cause), cause);
        }
        var workspace = Workspace(files);
        var target = $"{root}/{NotesFileName}";
        var selected = await RemoveTargetResolver.ResolveAsync(workspace, RemoveSelection.Selected(RemoveKind.Path, target), Token);
        Assert.NotNull(selected.Cause);
        MatchSnapshot(selected.Cause, $"selection-linked-{linked}.content");
        var inventory = new RemovePathInventory(new PhysicalPathResolver());

        var captured = Assert.IsType<RemovePathFileCapture.Unavailable>(await inventory.CaptureFileAsync(workspace, files.Combine(target), Token));
        var directory = Assert.IsType<RemovePathDirectoryInventory.Unavailable>(await inventory.InventoryDirectoryAsync(workspace, files.Combine(root), null, Token));

        MatchSnapshot(captured.Cause, $"capture-linked-{linked}.content");
        MatchSnapshot(directory.Cause, $"directory-linked-{linked}.content");
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        Assert.False(File.Exists(Path.Combine(files.Path, CaseRuntimeDirectory, Path.GetFileName(WorkspaceRuntimePaths.IgnorePath(Workspace(files))))));
    }
}
