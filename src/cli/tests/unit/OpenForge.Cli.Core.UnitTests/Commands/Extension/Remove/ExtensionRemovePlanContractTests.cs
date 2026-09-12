using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePlanContractTests
{
    [Theory(DisplayName = "Extension Remove planning binds changed-owner action to same-request prune"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    [InlineData(false, false, false, (int)ExtensionRemovePathAction.KeepAsUnmanaged)]
    [InlineData(true, true, false, (int)ExtensionRemovePathAction.Delete)]
    [InlineData(false, false, true, (int)ExtensionRemovePathAction.KeepAsUnmanaged)]
    public void PlanningBindsChangedOwnerActionToPrune(
        bool prune,
        bool automatic,
        bool allowInteraction,
        int expectedActionValue)
    {
        var expectedAction = (ExtensionRemovePathAction)expectedActionValue;
        var request = Request(prune, automatic, allowInteraction);
        var path = PathPlan(
            ".agents/changed.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            expectedAction);

        var plan = CreatePlan(
            request,
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, ["toolkit"]),
            Dependencies(),
            [path]);

        Assert.Equal(expectedAction, Assert.Single(plan.Planning.Decisions).Path.Action);
    }

    [Fact(DisplayName = "Extension Remove prompt-capable planning honors the wizard delete choice"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PromptPlanningHonorsDeleteChoice()
    {
        var request = Request(prune: false, automatic: false, allowInteraction: true);
        var path = PathPlan(
            ".agents/changed.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete);

        var plan = CreatePlan(
            request,
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.InteractiveIds, ["toolkit"]),
            Dependencies(),
            [path],
            ExtensionRemoveChangedContentPolicy.Delete);

        Assert.Equal(ExtensionRemovePathAction.Delete, Assert.Single(plan.Planning.Decisions).Path.Action);
        Assert.Equal(ExtensionRemoveSelectionKind.InteractiveIds, plan.Selection.SelectedBy);
    }

    [Fact(DisplayName = "Extension Remove noninteractive planning cannot select delete without prune"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void NoninteractivePlanningRejectsUnrequestedDelete()
    {
        var request = Request(prune: false, automatic: true, allowInteraction: false);
        var path = PathPlan(
            ".agents/changed.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete);

        Assert.Throws<ArgumentException>(() => CreatePlan(
            request,
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, ["toolkit"]),
            Dependencies(),
            [path],
            ExtensionRemoveChangedContentPolicy.Delete));
    }

    [Fact(DisplayName = "Extension Remove plan snapshots request selection, paths, topology, and effects"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanPreservesCohesiveFacts()
    {
        var request = Request(prune: false, automatic: true, allowInteraction: false);
        var selection = new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["toolkit"]);
        var dependencies = Dependencies();
        var paths = new[]
        {
            PathPlan(
                "z-path",
                ExtensionRemovePathClassification.UnchangedFinalOwner,
                ["toolkit"],
                [],
                ExtensionRemovePathAction.Delete),
            PathPlan(
                "a-path",
                ExtensionRemovePathClassification.Shared,
                ["toolkit"],
                ["other"],
                ExtensionRemovePathAction.RetainShared),
        };

        var plan = CreatePlan(request, selection, dependencies, paths);

        Assert.Same(request, plan.Request);
        Assert.Same(selection, plan.Selection);
        Assert.Same(dependencies, plan.Dependencies);
        Assert.Equal(["z-path", "a-path"], plan.Planning.Decisions.Select(decision => decision.Path.Path));
        Assert.Empty(plan.Topology.IntendedTargetBytes);
        Assert.Empty(plan.Topology.GeneratedEntries);
        Assert.Empty(plan.Topology.ProtectedPaths);
        Assert.True(plan.IsNoOp);
    }

    [Fact(DisplayName = "Extension Remove plan records an effect as non-no-op and retains its result identity"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRetainsPlannedEffects()
    {
        var effect = new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None);
        var planned = new ExtensionRemovePlannedEffect
        {
            Result = effect,
            FileChange = null,
            RecoveryTarget = null,
        };

        var plan = CreatePlan(
            Request(prune: false, automatic: true, allowInteraction: false),
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, ["toolkit"]),
            Dependencies(),
            [PathPlan(
                ".agents/toolkit.md",
                ExtensionRemovePathClassification.UnchangedFinalOwner,
                ["toolkit"],
                [],
                ExtensionRemovePathAction.Delete)],
            effects: [planned]);

        Assert.False(plan.IsNoOp);
        Assert.Same(effect, Assert.Single(plan.Effects).Result);
    }

    [Fact(DisplayName = "Extension Remove plan rejects mismatched selections owners duplicate paths and prune actions"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRejectsMismatchedAndDuplicateFacts()
    {
        var request = Request(prune: false, automatic: true, allowInteraction: false);
        var selection = new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["toolkit"]);
        var dependencies = Dependencies();
        var path = PathPlan(
            ".agents/changed.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.KeepAsUnmanaged);

        Assert.Throws<ArgumentException>(() => CreatePlan(
            request,
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, ["other"]),
            new ExtensionRemoveDependencyPlan(
                [new ExtensionRemovePackageFact("other", selectedForRemoval: true, [])],
                ["other"],
                [],
                []),
            [path]));
        Assert.Throws<ArgumentException>(() => CreatePlan(
            request,
            selection,
            dependencies,
            [PathPlan(
                ".agents/other.md",
                ExtensionRemovePathClassification.UnchangedFinalOwner,
                ["other"],
                [],
                ExtensionRemovePathAction.Delete)]));
        Assert.Throws<ArgumentException>(() => CreatePlan(
            request,
            selection,
            dependencies,
            [path, path]));
        Assert.Throws<ArgumentException>(() => CreatePlan(
            Request(prune: true, automatic: true, allowInteraction: false),
            selection,
            dependencies,
            [path]));
        Assert.Throws<ArgumentException>(() => CreatePlan(
            request,
            selection,
            dependencies,
            [PathPlan(
                ".agents/changed.md",
                ExtensionRemovePathClassification.ChangedFinalOwner,
                ["toolkit"],
                [],
                ExtensionRemovePathAction.Delete)]));
    }

    [Fact(DisplayName = "Extension Remove planning facts expose no-op and blocked decisions"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanningFactsExposeNoOpAndBlockedStates()
    {
        var retained = new ExtensionRemovePlanningPlan
        {
            Authority = new ExtensionRemovePlanningAuthority(
                ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged),
            Decisions =
            [
                new ExtensionRemovePlanningDecision
                {
                    Path = PathPlan(
                        ".agents/shared.md",
                        ExtensionRemovePathClassification.Shared,
                        ["toolkit"],
                        ["other"],
                        ExtensionRemovePathAction.RetainShared),
                    Disposition = ExtensionRemovePlanningDisposition.Retain,
                },
            ],
        };
        var blocked = new ExtensionRemovePlanningPlan
        {
            Authority = new ExtensionRemovePlanningAuthority(
                ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged),
            Decisions =
            [
                new ExtensionRemovePlanningDecision
                {
                    Path = PathPlan(
                        ".agents/missing.md",
                        ExtensionRemovePathClassification.Missing,
                        ["toolkit"],
                        [],
                        ExtensionRemovePathAction.ReleaseOwnership),
                    Disposition = ExtensionRemovePlanningDisposition.Blocked,
                },
            ],
        };

        Assert.True(retained.IsNoOp);
        Assert.False(retained.IsBlocked);
        Assert.False(blocked.IsNoOp);
        Assert.True(blocked.IsBlocked);
    }

    private static ExtensionRemovePlan CreatePlan(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection selection,
        ExtensionRemoveDependencyPlan dependencies,
        IEnumerable<ExtensionRemovePathPlan> paths,
        ExtensionRemoveChangedContentPolicy policy = ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged,
        IEnumerable<ExtensionRemovePlannedEffect>? effects = null)
    {
        var decisions = paths.Select(path => new ExtensionRemovePlanningDecision
        {
            Path = path,
            Disposition = path.Action switch
            {
                ExtensionRemovePathAction.RetainShared => ExtensionRemovePlanningDisposition.Retain,
                ExtensionRemovePathAction.KeepAsUnmanaged => ExtensionRemovePlanningDisposition.Retain,
                ExtensionRemovePathAction.Delete => ExtensionRemovePlanningDisposition.Delete,
                ExtensionRemovePathAction.ReleaseOwnership => ExtensionRemovePlanningDisposition.ReleaseOwnership,
                _ => throw new ArgumentOutOfRangeException(nameof(path), path.Action, "The test path action is not defined."),
            },
        }).ToArray();
        var input = new ExtensionRemovePlanInput
        {
            Request = request,
            Selection = selection,
            Dependencies = dependencies,
            Planning = new ExtensionRemovePlanningPlan
            {
                Authority = new ExtensionRemovePlanningAuthority(policy),
                Decisions = decisions,
            },
            Topology = new ExtensionRemoveTopology
            {
                IntendedTargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
                GeneratedEntries = new Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(StringComparer.Ordinal),
                ProtectedPaths = new HashSet<string>(StringComparer.Ordinal),
            },
            Effects = (effects ?? []).ToArray(),
            LifecycleChange = null,
            LifecycleRecoveryTarget = null,
        };
        return ExtensionRemovePlan.Create(input);
    }

    private static ExtensionRemoveRequest Request(
        bool prune,
        bool automatic,
        bool allowInteraction)
        => new(
            Workspace(),
            ExtensionRemoveMode.Apply,
            ["toolkit"],
            prune,
            automatic,
            allowInteraction);

    private static ExtensionRemoveDependencyPlan Dependencies()
        => new(
            [new ExtensionRemovePackageFact("toolkit", selectedForRemoval: true, [])],
            ["toolkit"],
            [],
            []);

    private static ExtensionRemovePathPlan PathPlan(
        string path,
        ExtensionRemovePathClassification classification,
        IEnumerable<string> selectedOwnerIds,
        IEnumerable<string> remainingOwnerIds,
        ExtensionRemovePathAction action)
        => new(path, classification, selectedOwnerIds, remainingOwnerIds, action);

    private static CliWorkspace Workspace()
        => new(
            "extension-remove-plan-workspace",
            "extension-remove-plan-workspace",
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
