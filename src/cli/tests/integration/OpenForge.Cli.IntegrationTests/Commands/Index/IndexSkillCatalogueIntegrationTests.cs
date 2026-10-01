using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexSkillCatalogueIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Default Index updates rooted Skill catalogues without changing native Skill or recipe bytes")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task DefaultSelectionUpdatesOnlySkillCatalogueGeneratedInteriorsAndRepeatsAsNoOp()
    {
        using var workspace = IndexOperationWorkspace.CreateSkillCatalogue("index-skill-catalogue");
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);
        var requestBefore = workspace.SnapshotHashes();
        var skillBytes = workspace.ReadBytes(IndexOperationWorkspace.NativeSkillPath);
        var skillOverwriteBytes = workspace.ReadBytes(IndexOperationWorkspace.NativeSkillOverwritePath);
        var recipeBytes = workspace.ReadBytes(IndexOperationWorkspace.RecipePath);
        var recipeOverwriteBytes = workspace.ReadBytes(IndexOperationWorkspace.RecipeOverwritePath);
        Assert.Contains("## Entries", System.Text.Encoding.UTF8.GetString(skillBytes), StringComparison.Ordinal);

        var dryRun = await operation.ExecuteAsync(
            workspace.AutomaticRequest(IndexMode.DryRun),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Equal(IndexSelectionOrigin.AutomaticLoader, dryRun.Selection.Origin);
        AssertCollectionRegions(
            dryRun,
            IndexRegionAction.Update,
            IndexRegionOutcome.NotRequested);
        Assert.Equal(requestBefore, workspace.SnapshotHashes());
        Assert.Equal(skillBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillPath));
        Assert.Equal(skillOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillOverwritePath));
        Assert.Equal(recipeBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipePath));
        Assert.Equal(recipeOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipeOverwritePath));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));

        var applied = await operation.ExecuteAsync(
            workspace.AutomaticRequest(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        AssertCollectionRegions(applied, IndexRegionAction.Update, IndexRegionOutcome.Verified);
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = IndexOperationWorkspace.LoaderExpectedEntry,
                Prefix = "# Open Forge Loader",
            }),
            await workspace.ReadTargetAsync(IndexOperationWorkspace.LoaderPath, TestContext.Current.CancellationToken));
        Assert.Equal(
            OpenForgeDocumentSeed.Metadata(
                description: "Skills",
                tags: ["Skill"],
                body: OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = IndexOperationWorkspace.SkillsExpectedEntry,
                    Prefix = IndexOperationWorkspace.SkillsPrefix,
                })),
            await workspace.ReadTargetAsync(IndexOperationWorkspace.SkillsPath, TestContext.Current.CancellationToken));
        Assert.Equal(
            OpenForgeDocumentSeed.Metadata(
                description: "References",
                tags: ["Docs"],
                body: OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = $"{IndexOperationWorkspace.GuidesExpectedEntry}\n{IndexOperationWorkspace.RecipeExpectedEntry}",
                    Prefix = IndexOperationWorkspace.ReferencesPrefix,
                })),
            await workspace.ReadTargetAsync(IndexOperationWorkspace.ReferencesPath, TestContext.Current.CancellationToken));
        Assert.Equal(
            OpenForgeDocumentSeed.Metadata(
                description: "Guides",
                tags: ["Docs"],
                body: OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = IndexOperationWorkspace.GuideExpectedEntry,
                    Prefix = IndexOperationWorkspace.GuidesPrefix,
                })),
            await workspace.ReadTargetAsync(IndexOperationWorkspace.GuidesPath, TestContext.Current.CancellationToken));
        Assert.Equal(skillBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillPath));
        Assert.Equal(skillOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillOverwritePath));
        Assert.Equal(recipeBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipePath));
        Assert.Equal(recipeOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipeOverwritePath));

        var afterApply = workspace.SnapshotHashes();
        Assert.Equal(
            [
                IndexOperationWorkspace.LoaderPath,
                IndexOperationWorkspace.SkillsPath,
                IndexOperationWorkspace.ReferencesPath,
                IndexOperationWorkspace.GuidesPath,
            ],
            requestBefore.Keys
                .Where(path => !string.Equals(requestBefore[path], afterApply[path], StringComparison.Ordinal))
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));

        var repeated = await operation.ExecuteAsync(
            workspace.AutomaticRequest(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        AssertCollectionRegions(repeated, IndexRegionAction.Unchanged, IndexRegionOutcome.AlreadyCurrent);
        Assert.Equal(afterApply, workspace.SnapshotHashes());
        Assert.Equal(skillBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillPath));
        Assert.Equal(skillOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.NativeSkillOverwritePath));
        Assert.Equal(recipeBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipePath));
        Assert.Equal(recipeOverwriteBytes, workspace.ReadBytes(IndexOperationWorkspace.RecipeOverwritePath));
    }

    private static void AssertCollectionRegions(
        IndexResult result,
        IndexRegionAction action,
        IndexRegionOutcome outcome)
    {
        Assert.Collection(
            result.Regions,
            region => AssertRegion(region, IndexOperationWorkspace.LoaderPath, action, outcome),
            region => AssertRegion(region, IndexOperationWorkspace.SkillsPath, action, outcome),
            region => AssertRegion(region, IndexOperationWorkspace.ReferencesPath, action, outcome),
            region => AssertRegion(region, IndexOperationWorkspace.GuidesPath, action, outcome));
        Assert.Equal(4, result.Counts.Regions);
        Assert.DoesNotContain(
            result.Regions,
            region => region.Source.Path == IndexOperationWorkspace.NativeSkillPath);
    }

    private static void AssertRegion(
        IndexRegion region,
        string path,
        IndexRegionAction action,
        IndexRegionOutcome outcome)
    {
        Assert.Equal(path, region.Source.Path);
        Assert.Equal(action, region.Action);
        Assert.Equal(outcome, region.Outcome);
    }
}
