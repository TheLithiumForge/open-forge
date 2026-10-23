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
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    private const string CaseRuntimeFile = ".OPEN-FORGE-STATE/notes.md";

    [Theory(DisplayName = "Include refuses case-variant runtime targets before decoding their content")]
    [InlineData(false), InlineData(true)]
    public void IncludeRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-include");
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var target = CaseRuntimeFile;
        if (linked)
        {
            target = "alias.md";
            Assert.True(files.TryCreateFileSymbolicLink(target, files.Combine(CaseRuntimeFile), out var cause), cause);
        }
        var request = new RouteInclusionRequest { Workspace = Workspace(files), File = target, Action = RouteInclusionAction.Include };

        var exception = Assert.Throws<ArgumentException>(() => RouteInclusionTargetResolver.Resolve(request));

        Assert.Contains("reserved workspace runtime directory", exception.Message, StringComparison.Ordinal);
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        Assert.False(File.Exists(files.Combine(".OPEN-FORGE-STATE/.gitignore")));
    }

    [Theory(DisplayName = "Link destinations refuse case-variant runtime paths without invoking the content reader")]
    [InlineData(false), InlineData(true)]
    public async Task LinkDestinationRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-link");
        SeedWorkspace(files);
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        files.CreateFile("safe.md", "# Safe\n");
        var target = CaseRuntimeFile;
        if (linked)
        {
            target = "alias.md";
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
            SourceCanonicalPath = ".agents/root/_root.md",
            RawDestination = $"../../{target}#history",
        };

        var result = await resolver.ResolveAsync(input, Token);

        Assert.Equal(SourceLinkTargetResolution.Unreadable, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, result.Finding?.Code);
        Assert.Equal("The local destination is inside the reserved runtime directory.", result.Finding?.Cause);
        Assert.Empty(reads);
        var safe = await resolver.ResolveAsync(input with { RawDestination = "../../safe.md#safe" }, Token);
        Assert.Equal(SourceLinkTargetResolution.Complete, safe.Target.Resolution);
        Assert.Equal(["safe.md"], reads);
        Assert.False(File.Exists(files.Combine(".OPEN-FORGE-STATE/.gitignore")));
    }

    [Theory(DisplayName = "Package destinations refuse existing and prospective case-variant runtime paths")]
    [InlineData(false), InlineData(true)]
    public void PackageDestinationRejectsRuntimeCaseAlias(bool existing)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-package");
        if (existing) files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var destination = existing ? CaseRuntimeFile : ".OPEN-FORGE-STATE/new/file.md";
        var workspace = Workspace(files);
        var registration = LibraryRegistration.Create(LibraryId.Create("team"), WorkspaceRelativeDirectory.Create("shared/team"),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);

        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, destination));
        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(workspace, registration, registered: null, generatedTargets: []));

        if (existing) Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        else Assert.False(Directory.Exists(files.Combine(".OPEN-FORGE-STATE")));
        Assert.True(ExtensionDestinationPolicy.IsAllowed(workspace, ".OPEN-FORGE-STATE-sibling/file.md"));
        Assert.False(File.Exists(files.Combine(".OPEN-FORGE-STATE/.gitignore")));
    }

    [Theory(DisplayName = "Remove refuses case-variant runtime targets before capturing file content")]
    [InlineData(false), InlineData(true)]
    public async Task RemovalRejectsRuntimeCaseAlias(bool linked)
    {
        using var files = TemporaryWorkspace.Create("runtime-case-remove");
        files.CreateFile(CaseRuntimeFile, new byte[] { 255 });
        var root = ".OPEN-FORGE-STATE";
        if (linked)
        {
            root = "alias";
            Assert.True(files.TryCreateDirectorySymbolicLink(root, files.Combine(".OPEN-FORGE-STATE"), out var cause), cause);
        }
        var workspace = Workspace(files);
        var target = $"{root}/notes.md";
        var selected = await RemoveTargetResolver.ResolveAsync(workspace, RemoveSelection.Selected(RemoveKind.Path, target), Token);
        Assert.NotNull(selected.Cause);
        Assert.Contains("reserved workspace runtime directory", selected.Cause, StringComparison.Ordinal);
        var inventory = new RemovePathInventory(new PhysicalPathResolver());

        var captured = Assert.IsType<RemovePathFileCapture.Unavailable>(await inventory.CaptureFileAsync(workspace, files.Combine(target), Token));
        var directory = Assert.IsType<RemovePathDirectoryInventory.Unavailable>(await inventory.InventoryDirectoryAsync(workspace, files.Combine(root), null, Token));

        Assert.Contains("reserved workspace runtime directory", captured.Cause, StringComparison.Ordinal);
        Assert.Contains("reserved workspace runtime state", directory.Cause, StringComparison.Ordinal);
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(CaseRuntimeFile)));
        Assert.False(File.Exists(files.Combine(".OPEN-FORGE-STATE/.gitignore")));
    }
}
