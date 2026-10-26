using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

[Trait("Boundary", "OS"), Trait("Feature", "workspace-runtime-reservation"), Trait("Evidence", "Integration")]
public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    private const string LibraryName = "team";
    private const string LibrarySourceDirectory = "shared/" + LibraryName;
    private const string SafeDocumentContent = "# Safe\n";
    private const int FixtureSettingsVersion = 2;
    private const string MissingFile = "missing/" + FileName;
    private const string AliasDirectory = "alias";
    private const string AliasFile = AliasDirectory + ".md";
    private const string SafeFile = "safe.md";
    private const string SafeChildFile = "safe-child.md";
    private const string SafeCompanionFile = "safe.overwrite.md";
    private const string HistoryFileName = "history.md";
    private const string NotesFileName = "notes.md";
    private const string FileName = "file.md";
    private const string NewDirectory = "new";
    private const string NewLeaf = NewDirectory + ".md";
    private const string NewCatalogue = NewDirectory + "/_new.md";
    private const string NewFile = NewDirectory + "/" + FileName;
    private const string SiblingSuffix = "-sibling";
    private const string RecoveryHistory = RecoveryBundleFormatV1.RecoveryDirectoryName + "/" + RecoveryBundleFormatV1.VersionDirectoryName + "/" + HistoryFileName;
    private const string RuntimeFile = WorkspaceRuntimePaths.DirectoryName + "/" + RecoveryHistory;
    private const string RuntimeHistory = WorkspaceRuntimePaths.DirectoryName + "/" + HistoryFileName;
    private const string RuntimeNewFile = WorkspaceRuntimePaths.DirectoryName + "/" + NewFile;
    private const string RuntimeNewLeaf = WorkspaceRuntimePaths.DirectoryName + "/" + NewLeaf;
    private const string RuntimeNewCatalogue = WorkspaceRuntimePaths.DirectoryName + "/" + NewCatalogue;
    private const string SiblingFile = WorkspaceRuntimePaths.DirectoryName + SiblingSuffix + "/" + FileName;
    private const string SiblingSafeFile = WorkspaceRuntimePaths.DirectoryName + SiblingSuffix + "/" + SafeFile;
    private const string AliasHistory = AliasDirectory + "/" + RecoveryHistory;
    private const string AliasNewFile = AliasDirectory + "/" + NewFile;
    private const string SourceAliasDirectory = SourceLogicalPath.AgentsRoot + "/" + AliasDirectory;
    private const string SourceAliasFile = SourceLogicalPath.AgentsRoot + "/" + AliasFile;
    private const string SourceAliasHistory = SourceAliasDirectory + "/" + RecoveryHistory;
    private const string SourceAliasNewFile = SourceAliasDirectory + "/" + NewLeaf;
    private const string SourceAliasNewCatalogue = SourceAliasDirectory + "/" + NewCatalogue;
    private const string SourceSafeFile = SourceLogicalPath.AgentsRoot + "/" + SafeFile;
    private const string SourceHiddenFile = SourceLogicalPath.AgentsRoot + "/hidden.md";
    private const string RootDirectory = "root";
    private const string LeafName = "leaf";
    private const string LeafReference = RootDirectory + "/" + LeafName;
    private const string RootCatalogueRelative = RootDirectory + "/_root.md";
    private const string RootCatalogue = SourceLogicalPath.AgentsRoot + "/" + RootCatalogueRelative;
    private const string RootLeaf = SourceLogicalPath.AgentsRoot + "/" + LeafReference + ".md";
    private const string RootMoved = SourceLogicalPath.AgentsRoot + "/" + RootDirectory + "/moved.md";
    private const string RootScratch = SourceLogicalPath.AgentsRoot + "/" + RootDirectory + "/scratch.bin";
    private const string RootRuntime = SourceLogicalPath.AgentsRoot + "/" + RootDirectory + "/runtime";
    private const string RuntimeAliasDirectory = "runtime-alias";
    private const string RuntimeAliasFile = RuntimeAliasDirectory + ".md";
    private const string RuntimeAliasHistory = RuntimeAliasDirectory + "/" + RecoveryHistory;
    private const string RuntimeLinkedSafe = WorkspaceRuntimePaths.DirectoryName + "/linked-safe.md";
    private const string WorkspaceDirectory = "workspace";
    private const string WorkspaceAlias = WorkspaceDirectory + "-alias";
    private const string WorkspaceSafeDirectory = WorkspaceDirectory + "/safe";
    private const string WorkspaceSafeHistory = WorkspaceSafeDirectory + "/" + HistoryFileName;
    private const string WorkspaceRuntimeDirectory = WorkspaceDirectory + "/" + WorkspaceRuntimePaths.DirectoryName;
    private const string RetainedHistory = "retained history";
    private const string HistoryFragment = "#history";
    private const string SafeDestination = "../../" + SafeFile + "#safe";
    private static string JsonFormat => CliSyntaxDefinitions.Format.FiniteSpellings.Single(pair => pair.Value == CliFormat.Json).Key;

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Source catalogues skip runtime directory and file aliases without ignore rules")]
    [InlineData(SourceLogicalPath.AgentsRoot), InlineData(SourceAliasDirectory)]
    public async Task SourceCatalogueSkipsRuntimeAliases(string alias)
    {
        using var files = TemporaryWorkspace.Create("runtime-catalogue");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(alias, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(Workspace(files), [alias]), Token);
        Assert.Empty(catalogue.Candidates);
        Assert.Empty(catalogue.Issues);
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    [Fact(DisplayName = "Source traversal omits linked runtime files and keeps ordinary neighbors")]
    public async Task SourceCatalogueSkipsRuntimeFileAlias()
    {
        using var files = TemporaryWorkspace.Create("runtime-source-file");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.CreateFile(SourceSafeFile, "safe");
        Assert.True(files.TryCreateFileSymbolicLink(SourceHiddenFile, files.Combine(RuntimeFile), out var cause), cause);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(Workspace(files), [SourceLogicalPath.AgentsRoot]), Token);
        Assert.Equal([SourceSafeFile], catalogue.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Empty(catalogue.Issues);
    }

    [Theory(DisplayName = "Source document reads refuse lexical and resolved runtime layers before reading bytes")]
    [InlineData(false), InlineData(true)]
    public async Task SourceDocumentRejectsRuntime(bool alias)
    {
        using var files = TemporaryWorkspace.Create("runtime-document");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        var path = RuntimeFile;
        if (alias)
        {
            path = SourceAliasFile;
            Assert.True(files.TryCreateFileSymbolicLink(path, files.Combine(RuntimeFile), out var cause), cause);
        }
        var layer = new SourceLayer(path, files.Combine(path), SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var read = await new SourceDocumentReader(Workspace(files)).ReadAsync(layer, Token);
        Assert.Equal(SourceLayerVerificationState.Unsafe, read.Verification.State);
        Assert.Null(read.Read);
    }

    [Theory(DisplayName = "Markdown scans skip runtime roots files and aliases before UTF-8 inspection")]
    [InlineData("."), InlineData(WorkspaceRuntimePaths.DirectoryName), InlineData(RuntimeFile)]
    [InlineData(AliasDirectory), InlineData(AliasHistory), InlineData(AliasFile)]
    public async Task MarkdownScansSkipRuntime(string root)
    {
        using var files = TemporaryWorkspace.Create("runtime-markdown");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.CreateFile(SiblingSafeFile, "safe");
        Assert.True(files.TryCreateDirectorySymbolicLink(AliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var directoryCause), directoryCause);
        Assert.True(files.TryCreateFileSymbolicLink(AliasFile, files.Combine(RuntimeFile), out var fileCause), fileCause);
        var result = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver()).ReadAsync(
            new RouteMarkdownCatalogueRequest(Workspace(files), [root], [], new RouteMarkdownCatalogueFilters([".md"])), Token);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, result.Coverage);
        Assert.Empty(result.Findings);
        Assert.Equal(root == "." ? [SiblingSafeFile] : Array.Empty<string>(), result.SelectedPaths);
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    [Theory(DisplayName = "Route inclusion refuses direct and linked runtime files")]
    [InlineData(false), InlineData(true)]
    public void IncludeRejectsRuntime(bool alias)
    {
        using var files = TemporaryWorkspace.Create("runtime-include");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        var path = RuntimeFile;
        if (alias)
        {
            path = AliasFile;
            Assert.True(files.TryCreateFileSymbolicLink(path, files.Combine(RuntimeFile), out var cause), cause);
        }
        var request = new RouteInclusionRequest { Workspace = Workspace(files), File = path, Action = RouteInclusionAction.Include };
        var exception = Assert.Throws<ArgumentException>(() => RouteInclusionTargetResolver.Resolve(request));
        MatchSnapshot(exception.Message, $"alias-{alias}.content");
    }

    [Theory(DisplayName = "Package destinations reject runtime aliases including missing descendants")]
    [InlineData(RuntimeNewFile), InlineData(AliasNewFile), InlineData(AliasHistory)]
    public void PackageDestinationsRejectRuntime(string destination)
    {
        using var files = TemporaryWorkspace.Create("runtime-packages");
        files.CreateFile(RuntimeFile, "history");
        Assert.True(files.TryCreateDirectorySymbolicLink(AliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var workspace = Workspace(files);
        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, destination));
        var registration = LibraryRegistration.Create(LibraryId.Create(LibraryName), WorkspaceRelativeDirectory.Create(LibrarySourceDirectory),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);
        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(workspace, registration, registered: null, generatedTargets: []));
        Assert.True(ExtensionDestinationPolicy.IsAllowed(workspace, SiblingFile));
    }

    [Fact(DisplayName = "Lexical runtime reservation survives a linked workspace and an outward runtime alias")]
    public async Task LexicalReservationUsesPhysicalWorkspaceRoot()
    {
        using var files = TemporaryWorkspace.Create("runtime-lexical-root");
        files.CreateDirectory(WorkspaceDirectory);
        files.CreateFile(WorkspaceSafeHistory, "safe bytes");
        Assert.True(files.TryCreateDirectorySymbolicLink(WorkspaceRuntimeDirectory, files.Combine(WorkspaceSafeDirectory), out var runtimeCause), runtimeCause);
        Assert.True(files.TryCreateDirectorySymbolicLink(WorkspaceAlias, files.Combine(WorkspaceDirectory), out var workspaceCause), workspaceCause);
        var workspace = new CliWorkspace(files.Combine(WorkspaceAlias), files.Combine(WorkspaceDirectory), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var layer = new SourceLayer(RuntimeHistory, files.Combine(WorkspaceSafeHistory), SourceDocumentForm.Markdown, SourceLayerKind.Base);
        Assert.Equal(SourceLayerVerificationState.Unsafe, (await new SourceDocumentReader(workspace).ReadAsync(layer, Token)).Verification.State);
        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, RuntimeNewLeaf));
    }

    private static void MatchSnapshot(string actual, string name,
        [CallerFilePath] string sourceFile = "", [CallerMemberName] string testName = "")
        => CommandOutputSnapshot.MatchSnapshot(actual, $"{testName}-{name}", sourceFile, nameof(WorkspaceRuntimeReservationIntegrationTests));

    private static CliWorkspace Workspace(TemporaryWorkspace files)
        => new(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
