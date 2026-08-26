using OpenForge.Cli.Core.Commands.Context.Shared.Links;
using OpenForge.Cli.Core.UnitTests.Commands.Context;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context.Shared.Links;

public sealed class ContextLinkCaseMismatchResolverTests
{
    [Theory(DisplayName = "Context detects exact target case mismatch after complete or missing neutral resolution"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData((int)SourceLinkTargetResolution.Complete)]
    [InlineData((int)SourceLinkTargetResolution.Missing)]
    public async Task DetectsCaseMismatchAcrossNeutralResolutionStates(int initialResolutionValue)
    {
        var initialResolution = (SourceLinkTargetResolution)initialResolutionValue;
        using var workspace = ContextOperationWorkspace.Create();
        var actualPath = Path.Combine(workspace.Root, ".agents/projects/Linked.md");
        File.Move(
            Path.Combine(workspace.Root, ".agents/projects/linked.md"),
            actualPath);
        var catalogue = new SourceCatalogue(
            workspace: workspace.Workspace,
            candidates: [],
            sources: [],
            issues: [],
            isCancelled: false);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace.Workspace,
            Catalogue = catalogue,
            LayerCanonicalPath = ".agents/projects/guide.md",
            RawDestination = "linked.md#details",
        };
        var initial = new SourceLinkDestinationFacts
        {
            Fragment = "details",
            Target = new SourceLinkTarget
            {
                Kind = SourceLinkTargetKind.Local,
                Id = null,
                Path = ".agents/projects/linked.md",
                PhysicalPath = initialResolution == SourceLinkTargetResolution.Complete ? actualPath : null,
                Layer = null,
                Resolution = initialResolution,
                Network = null,
            },
            Finding = initialResolution == SourceLinkTargetResolution.Missing
                ? new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetMissing,
                    Cause = "The local destination path does not exist.",
                    Candidates = [],
                }
                : null,
        };
        var physicalPathResolver = new PhysicalPathResolver();
        var markdownParser = new MarkdownDocumentParser();
        var resolver = new SourceLinkDestinationResolver(
            (selectedWorkspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
                selectedWorkspace.LexicalRoot,
                selectedWorkspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdownParser.Parse);

        var result = await ContextLinkCaseMismatchResolver.ResolveAsync(
            input,
            initial,
            resolver,
            CancellationToken.None);

        Assert.True(result.CaseMismatch);
        Assert.Equal(SourceLinkTargetResolution.Complete, result.Facts.Target.Resolution);
        Assert.Equal(".agents/projects/Linked.md", result.Facts.Target.Path);
        Assert.Null(result.Facts.Finding);
    }
}
