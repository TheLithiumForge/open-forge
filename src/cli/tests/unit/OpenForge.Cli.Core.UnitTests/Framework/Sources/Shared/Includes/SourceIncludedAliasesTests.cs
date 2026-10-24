using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Includes;

public sealed class SourceIncludedAliasesTests
{
    [Fact(DisplayName = "Included alias operands resolve one retained source without a separate source ID")]
    [Trait("Boundary", "Processing"), Trait("Feature", "included-sources"), Trait("Evidence", "Unit")]
    public void AliasOperandsKeepOneIdentity()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "included-alias-model"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
        var source = new SourceLogicalSource(new SourceLogicalIdentity(automaticId: "./CONTRIBUTING.md", canonicalBasePath: "CONTRIBUTING.md"),
            new SourceLayer(canonicalPath: "CONTRIBUTING.md", physicalPath: Path.Combine(root, "CONTRIBUTING.md"),
                form: SourceDocumentForm.Markdown, kind: SourceLayerKind.Base));
        var catalogue = new SourceCatalogue(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [], [source], [], isCancelled: false,
            aliases: new Dictionary<string, string> { ["alias.md"] = "CONTRIBUTING.md" });
        var resolver = new SourceReferenceResolver((_, _) => throw new InvalidOperationException("A retained alias requires no extra physical lookup."));
        var resolution = resolver.Resolve("./alias.md", catalogue);

        Assert.Equal(SourceReferenceResolutionState.Resolved, resolution.State);
        Assert.Same(source, resolution.Source);
        Assert.Single(catalogue.Sources);
        Assert.Equal(SourceReferenceResolutionState.Unknown, resolver.Resolve("alias", catalogue).State);
        Assert.Null(SourceIdentity.DeriveId(DefaultWorkspaceLayout, "alias.md"));
    }

    [Fact(DisplayName = "Required alias rows select the first source once instead of reporting a missing file")]
    [Trait("Boundary", "Processing"), Trait("Feature", "included-sources"), Trait("Evidence", "Unit")]
    public void RequiredAliasRowsKeepOneRead()
    {
        var metadata = SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing);
        var entries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse("""
            # Loader

            ## Entries

            - [Contributing](../CONTRIBUTING.md) - #Contribution
            - [Alias](../alias.md) - #LoadNow
            """));
        var result = new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            Layout = WorkspaceLayout.Create(SourceInventoryTestData.Workspace(), WorkspaceLayout.DefaultMainRoot),
            WorkspaceEntryPath = "AGENTS.md",
            LoaderRootPaths = [],
            IncludeLinkedSources = true,
            Aliases = new Dictionary<string, string> { ["alias.md"] = "CONTRIBUTING.md" },
            Sources =
            [
                new SourceLoadingClosureSource
                {
                    Path = ".agents/loader.md", Form = SourceDocumentForm.Loader, RouteState = SourceRouteState.Unrouted,
                    ParentPath = null, Metadata = metadata, GeneratedEntries = entries,
                },
                new SourceLoadingClosureSource
                {
                    Path = "CONTRIBUTING.md", Form = SourceDocumentForm.Markdown, RouteState = SourceRouteState.Unrouted,
                    ParentPath = ".agents/loader.md", Metadata = metadata, GeneratedEntries = SourceGeneratedEntriesFacts.Absent, IsLinked = true,
                },
            ],
        });

        Assert.Empty(result.Issues);
        Assert.Equal(["AGENTS.md", ".agents/loader.md", "CONTRIBUTING.md"], result.Startup.Select(source => source.Path));
    }

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(SourceIncludedAliasesTests)),
                Path.GetFullPath(nameof(SourceIncludedAliasesTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
