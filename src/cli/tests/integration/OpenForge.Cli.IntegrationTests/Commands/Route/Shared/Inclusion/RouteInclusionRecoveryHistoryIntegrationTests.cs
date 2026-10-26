using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Route.Include.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

[Trait("Boundary", "OS"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
public sealed partial class RouteInclusionRecoveryHistoryIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Include and Exclude warn only about draft and damaged artifacts on changes and no-ops")]
    [InlineData(false), InlineData(true)]
    public async Task HistoryDoesNotBlockChangesOrNoOps(bool exclude)
    {
        using var workspace = new RouteInclusionWorkspace();
        var include = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = "guide.md", Description = "Guide" };
        if (exclude) Assert.Equal(CliSemanticStatus.Complete, (await include.ExecuteAsync(request, Token)).Status);
        var history = await SeedHistoryAsync(workspace);
        var before = workspace.Bytes("guide.md");
        for (var run = 0; run < 2; run++)
        {
            RouteInclusionOutcome outcome;
            IReadOnlyList<CliFinding> findings;
            if (exclude)
            {
                var result = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(
                    new RouteExcludeRequest { Workspace = workspace.Workspace, File = "guide.md" }, Token);
                outcome = result.Outcome;
                findings = RouteExcludeReportSelector.Select(result, new CliSelection(CliDetail.Standard, null)).Findings;
            }
            else
            {
                var result = await include.ExecuteAsync(request, Token);
                outcome = result.Outcome;
                findings = RouteIncludeReportSelector.Select(result, new CliSelection(CliDetail.Standard, null)).Findings;
            }
            Assert.Equal(CliSemanticStatus.Complete, outcome.Status);
            Assert.Equal(run == 1, outcome.NoOp);
            Assert.Null(outcome.RecoveryPath);
            Assert.Equal(2, findings.Count);
            Assert.All(findings, finding => Assert.Equal(CliSeverity.Warning, finding.Severity));
            Assert.Equal(history.Keys.Skip(1).Order(StringComparer.Ordinal), findings.Select(finding => finding.Subject.Path).Order(StringComparer.Ordinal));
            Assert.All(history, item => Assert.Equal(item.Value, File.ReadAllBytes(item.Key)));
            Assert.Equal(before, workspace.Bytes("guide.md"));
        }
    }

    [Theory(DisplayName = "Include and Exclude still stop changed current targets before effects despite history")]
    [InlineData(false), InlineData(true)]
    public async Task CurrentChangesStillStop(bool exclude)
    {
        using var workspace = new RouteInclusionWorkspace(exclude ? "- [Guide](../guide.md)" : "- none - No entries - #Empty");
        var request = new RouteInclusionRequest
        {
            Workspace = workspace.Workspace,
            File = "guide.md",
            Action = exclude ? RouteInclusionAction.Exclude : RouteInclusionAction.Include,
            Description = "Guide",
        };
        var build = await new RouteInclusionPlanBuilder().BuildAsync(request, Token);
        Assert.NotNull(build.Plan);
        var history = await SeedHistoryAsync(workspace);
        workspace.Files.ReplaceText(SourceLogicalPath.LoaderPath, workspace.Read(SourceLogicalPath.LoaderPath) + "\nChanged after planning.\n");
        var before = workspace.Files.SnapshotHashes();

        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(build.Plan, Token);

        Assert.Equal(RouteInclusionFindingCode.TargetChanged, outcome.Code);
        Assert.Equal(CliSemanticStatus.Blocked, outcome.Status);
        Assert.All(outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.All(history, item => Assert.Equal(item.Value, File.ReadAllBytes(item.Key)));
    }

    private static async Task<Dictionary<string, byte[]>> SeedHistoryAsync(RouteInclusionWorkspace workspace)
    {
        var path = workspace.Files.Combine("guide.md");
        var snapshot = FileStateSnapshot.File(path, path, workspace.Bytes("guide.md"));
        var prepared = await RecoveryBundleStore.PrepareAsync(RecoveryBundleInput.Create(
            workspace.Workspace, RouteIncludeDefinitions.CommandIdentity,
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Route, RecoveryBundleOperation.Include, workspace.Workspace),
            Guid.NewGuid(), [RecoveryBundleTarget.Create(PlannedFileChange.Delete(snapshot.Expectation), snapshot)]), Token);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var directory = Path.GetDirectoryName(preparation.BundlePath)!;
        var draft = Path.Combine(directory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()));
        var damaged = Path.Combine(directory, RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()));
        await File.WriteAllTextAsync(draft, "unfinished history", Token);
        await File.WriteAllTextAsync(damaged, "damaged history", Token);
        return new[] { preparation.BundlePath, draft, damaged }.ToDictionary(item => item, File.ReadAllBytes, StringComparer.Ordinal);
    }
}
