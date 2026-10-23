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
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

[Trait("Boundary", "OS"), Trait("Feature", "workspace-runtime-reservation"), Trait("Evidence", "Integration")]
public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    private const string RuntimeFile = ".open-forge-state/recovery/v1/history.md";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Source catalogues skip runtime directory and file aliases without ignore rules")]
    [InlineData(".agents"), InlineData(".agents/alias")]
    public async Task SourceCatalogueSkipsRuntimeAliases(string alias)
    {
        using var files = TemporaryWorkspace.Create("runtime-catalogue");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(alias, files.Combine(".open-forge-state"), out var cause), cause);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(Workspace(files), [alias]), Token);
        Assert.Empty(catalogue.Candidates);
        Assert.Empty(catalogue.Issues);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
    }

    [Fact(DisplayName = "Source traversal omits linked runtime files and keeps ordinary neighbors")]
    public async Task SourceCatalogueSkipsRuntimeFileAlias()
    {
        using var files = TemporaryWorkspace.Create("runtime-source-file");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.CreateFile(".agents/safe.md", "safe");
        Assert.True(files.TryCreateFileSymbolicLink(".agents/hidden.md", files.Combine(RuntimeFile), out var cause), cause);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(Workspace(files), [".agents"]), Token);
        Assert.Equal([".agents/safe.md"], catalogue.Candidates.Select(candidate => candidate.CanonicalPath));
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
            path = ".agents/alias.md";
            Assert.True(files.TryCreateFileSymbolicLink(path, files.Combine(RuntimeFile), out var cause), cause);
        }
        var layer = new SourceLayer(path, files.Combine(path), SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var read = await new SourceDocumentReader(Workspace(files)).ReadAsync(layer, Token);
        Assert.Equal(SourceLayerVerificationState.Unsafe, read.Verification.State);
        Assert.Null(read.Read);
    }

    [Theory(DisplayName = "Markdown scans skip runtime roots files and aliases before UTF-8 inspection")]
    [InlineData("."), InlineData(".open-forge-state"), InlineData(RuntimeFile)]
    [InlineData("alias"), InlineData("alias/recovery/v1/history.md"), InlineData("alias.md")]
    public async Task MarkdownScansSkipRuntime(string root)
    {
        using var files = TemporaryWorkspace.Create("runtime-markdown");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.CreateFile(".open-forge-state-sibling/safe.md", "safe");
        Assert.True(files.TryCreateDirectorySymbolicLink("alias", files.Combine(".open-forge-state"), out var directoryCause), directoryCause);
        Assert.True(files.TryCreateFileSymbolicLink("alias.md", files.Combine(RuntimeFile), out var fileCause), fileCause);
        var result = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver()).ReadAsync(
            new RouteMarkdownCatalogueRequest(Workspace(files), [root], [], new RouteMarkdownCatalogueFilters([".md"])), Token);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, result.Coverage);
        Assert.Empty(result.Findings);
        Assert.Equal(root == "." ? [".open-forge-state-sibling/safe.md"] : Array.Empty<string>(), result.SelectedPaths);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
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
            path = "alias.md";
            Assert.True(files.TryCreateFileSymbolicLink(path, files.Combine(RuntimeFile), out var cause), cause);
        }
        var request = new RouteInclusionRequest { Workspace = Workspace(files), File = path, Action = RouteInclusionAction.Include };
        var exception = Assert.Throws<ArgumentException>(() => RouteInclusionTargetResolver.Resolve(request));
        Assert.Contains("reserved workspace runtime directory", exception.Message, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Package destinations reject runtime aliases including missing descendants")]
    [InlineData(".open-forge-state/new/file.md"), InlineData("alias/new/file.md"), InlineData("alias/recovery/v1/history.md")]
    public void PackageDestinationsRejectRuntime(string destination)
    {
        using var files = TemporaryWorkspace.Create("runtime-packages");
        files.CreateFile(RuntimeFile, "history");
        Assert.True(files.TryCreateDirectorySymbolicLink("alias", files.Combine(".open-forge-state"), out var cause), cause);
        var workspace = Workspace(files);
        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, destination));
        var registration = LibraryRegistration.Create(LibraryId.Create("team"), WorkspaceRelativeDirectory.Create("shared/team"),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);
        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(workspace, registration, registered: null, generatedTargets: []));
        Assert.True(ExtensionDestinationPolicy.IsAllowed(workspace, ".open-forge-state-sibling/file.md"));
    }

    [Fact(DisplayName = "Lexical runtime reservation survives a linked workspace and an outward runtime alias")]
    public async Task LexicalReservationUsesPhysicalWorkspaceRoot()
    {
        using var files = TemporaryWorkspace.Create("runtime-lexical-root");
        files.CreateDirectory("workspace");
        files.CreateFile("workspace/safe/history.md", "safe bytes");
        Assert.True(files.TryCreateDirectorySymbolicLink("workspace/.open-forge-state", files.Combine("workspace/safe"), out var runtimeCause), runtimeCause);
        Assert.True(files.TryCreateDirectorySymbolicLink("workspace-alias", files.Combine("workspace"), out var workspaceCause), workspaceCause);
        var workspace = new CliWorkspace(files.Combine("workspace-alias"), files.Combine("workspace"), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var layer = new SourceLayer(".open-forge-state/history.md", files.Combine("workspace/safe/history.md"), SourceDocumentForm.Markdown, SourceLayerKind.Base);
        Assert.Equal(SourceLayerVerificationState.Unsafe, (await new SourceDocumentReader(workspace).ReadAsync(layer, Token)).Verification.State);
        Assert.False(ExtensionDestinationPolicy.IsAllowed(workspace, ".open-forge-state/new.md"));
    }

    private static CliWorkspace Workspace(TemporaryWorkspace files)
        => new(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
