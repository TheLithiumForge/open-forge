using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMovePlanningIntegrationTests
{
    [Theory(DisplayName = "Route Move resolves complete subjects destinations ownership references and navigation without writes"),
        InlineData("leaf-id", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("leaf-base-path", RouteMoveIntegrationWorkspace.LeafPath, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("leaf-overwrite-path", RouteMoveIntegrationWorkspace.OverwritePath, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("same-parent", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.Apply, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("cross-route", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination, (int)RouteMoveMode.Apply, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("category", RouteMoveIntegrationWorkspace.CategoryPath, RouteMoveIntegrationWorkspace.CategoryDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Category),
        InlineData("compatibility-category", ".agents/guidance/topics/index.md", ".agents/archive/topics/index.md", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Category),
        InlineData("incoming-reference", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("outgoing-reference", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("unicode-definition-fragment", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("external-uri", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("loader", ".agents/loader.md", RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSubject, -1),
        InlineData("workspace-root", ".agents/", RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSource, -1),
        InlineData("native-source", ".agents/guidance/topics/native/SKILL.md", RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSubject, -1),
        InlineData("resource-source", ".agents/guidance/topics/image.bin", RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSubject, -1),
        InlineData("unsupported-source", ".agents/guidance/topics/notes.md", RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSubject, -1),
        InlineData("orphan-overwrite", RouteMoveIntegrationWorkspace.OverwritePath, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidSubject, -1),
        InlineData("ambiguous-route", RouteMoveIntegrationWorkspace.CategoryPath, RouteMoveIntegrationWorkspace.CategoryDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.RouteAmbiguous, -1),
        InlineData("identity-collision", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.IdentityCollision, -1),
        InlineData("missing-parent", RouteMoveIntegrationWorkspace.LeafId, ".agents/missing/new guide.md", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationParentMissing, -1),
        InlineData("id-like-destination", RouteMoveIntegrationWorkspace.LeafId, "archive/new-guide", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidDestination, -1),
        InlineData("invalid-leaf-extension", RouteMoveIntegrationWorkspace.LeafId, ".agents/guidance/new guide.txt", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidDestination, -1),
        InlineData("invalid-category-form", RouteMoveIntegrationWorkspace.CategoryPath, ".agents/archive/topics/index.md", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidDestination, -1),
        InlineData("occupied-destination", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationOccupied, -1),
        InlineData("self-move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafPath, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.SelfMove, -1),
        InlineData("inside-source", RouteMoveIntegrationWorkspace.CategoryPath, ".agents/guidance/topics/nested/_nested.md", (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationInsideSource, -1),
        InlineData("overwrite-collision", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationOccupied, -1),
        InlineData("ownership-claim", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.OwnershipClaimed, -1),
        InlineData("ownership-malformed", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.OwnershipUnavailable, -1),
        InlineData("ownership-stale", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.OwnershipUnavailable, -1),
        InlineData("ownership-incomplete", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.OwnershipUnavailable, -1),
        InlineData("ownership-conflicting", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.OwnershipUnavailable, -1),
        InlineData("unsupported-reference", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Complete, -1, (int)RouteMoveSubjectKind.Leaf),
        InlineData("unsafe-reference", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.ReferenceUnsafe, -1),
        InlineData("invalid-utf8", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Incomplete, (int)RouteMoveFindingCode.ReferenceCoverageIncomplete, -1),
        InlineData("unsafe-generated-region", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.GeneratedRegionUnsafe, -1),
        InlineData("aliased-destination", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationUnsafe, -1),
        InlineData("category-unsafe-alias", RouteMoveIntegrationWorkspace.CategoryPath, RouteMoveIntegrationWorkspace.CategoryDestination, (int)RouteMoveMode.DryRun, (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.CategoryUnsafe, -1)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task PlanningCoversTheCompleteAcceptedBoundary(
        string scenario,
        string source,
        string destination,
        int modeValue,
        int expectedStatusValue,
        int expectedFindingValue,
        int expectedSubjectKindValue)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-plan-{scenario}");
        workspace.SeedScenario(scenario);
        var before = workspace.SnapshotHashes();

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(source, destination, (RouteMoveMode)modeValue),
            TestContext.Current.CancellationToken);

        var result = new RouteMoveResultBuilder().Build(build.Formation);
        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        if (expectedFindingValue >= 0)
        {
            Assert.Contains(
                result.Findings,
                finding => finding.Code == (RouteMoveFindingCode)expectedFindingValue);
            Assert.Null(build.Plan);
        }
        else
        {
            var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
            Assert.Equal((RouteMoveSubjectKind)expectedSubjectKindValue, result.Subject.Kind);
            Assert.Equal(RouteMoveCoverage.Complete, result.References.Coverage);
            Assert.Equal(RouteMoveCoverage.Complete, result.GeneratedNavigation.Coverage);
            Assert.Equal(RouteMovePlanCompleteness.Complete, result.Plan.Completeness);
            Assert.Equal(RouteMovePlanSafety.Safe, result.Plan.Safety);
            Assert.NotEmpty(result.Effects);
            Assert.NotNull(plan.Preview);
            if (result.Subject.Kind == RouteMoveSubjectKind.Leaf)
            {
                Assert.Empty(result.Subject.Items);
            }
            else
            {
                Assert.NotEmpty(result.Subject.Items);
            }
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move dry-run and apply planning are semantically identical and write-free")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task DryRunAndApplyShareOneExactObservationAndPlan()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-plan-parity");
        var before = workspace.SnapshotHashes();
        var builder = workspace.CreatePlanBuilder();

        var dryRun = await builder.BuildAsync(
            workspace.Request(mode: RouteMoveMode.DryRun),
            TestContext.Current.CancellationToken);
        var apply = await builder.BuildAsync(
            workspace.Request(mode: RouteMoveMode.Apply),
            TestContext.Current.CancellationToken);

        var dryProjection = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(dryRun.Plan).Projection;
        var applyProjection = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(apply.Plan).Projection;
        Assert.Equal(
            dryProjection.DirectoryCreations.Select(creation => creation.Expectation),
            applyProjection.DirectoryCreations.Select(creation => creation.Expectation));
        AssertFileChangesEqual(dryProjection.FileChanges, applyProjection.FileChanges);
        Assert.Equal(
            dryProjection.DirectoryDeletions.Select(deletion => deletion.Expectation),
            applyProjection.DirectoryDeletions.Select(deletion => deletion.Expectation));
        AssertRecoveryTargetsEqual(dryProjection.RecoveryTargets, applyProjection.RecoveryTargets);
        Assert.Equal(dryRun.Formation.Source, apply.Formation.Source);
        Assert.Equal(dryRun.Formation.Destination, apply.Formation.Destination);
        AssertSubjectsEqual(dryRun.Formation.Subject, apply.Formation.Subject);
        AssertReferencesEqual(dryRun.Formation.References, apply.Formation.References);
        AssertGeneratedNavigationEqual(
            dryRun.Formation.GeneratedNavigation,
            apply.Formation.GeneratedNavigation);
        Assert.Equal(dryRun.Formation.Effects, apply.Formation.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move category inventory retains every item kind layer and relative path")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task CategoryInventoryIsCompleteAndPreservesRelativeLayout()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-category-inventory");

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(
                RouteMoveIntegrationWorkspace.CategoryPath,
                RouteMoveIntegrationWorkspace.CategoryDestination),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        var items = plan.Preview.Subject.Items;
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.Directory
            && item.SourcePath == ".agents/guidance/topics/native"
            && item.DestinationPath == ".agents/archive/topics/native");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.Directory
            && item.SourcePath == ".agents/guidance/topics/assets"
            && item.DestinationPath == ".agents/archive/topics/assets");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.Entrypoint
            && item.SourcePath == RouteMoveIntegrationWorkspace.CategoryPath
            && item.DestinationPath == RouteMoveIntegrationWorkspace.CategoryDestination);
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.RoutedMarkdown
            && item.Layer == RouteMoveLayerKind.Base
            && item.SourcePath == ".agents/guidance/topics/child.md"
            && item.DestinationPath == ".agents/archive/topics/child.md");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.RoutedMarkdown
            && item.Layer == RouteMoveLayerKind.Overwrite
            && item.SourcePath == ".agents/guidance/topics/child.overwrite.md"
            && item.DestinationPath == ".agents/archive/topics/child.overwrite.md");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.UnroutedMarkdown
            && item.SourcePath == ".agents/guidance/topics/notes.md"
            && item.DestinationPath == ".agents/archive/topics/notes.md");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.NativeSource
            && item.SourcePath == ".agents/guidance/topics/native/SKILL.md"
            && item.DestinationPath == ".agents/archive/topics/native/SKILL.md");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.Resource
            && item.SourcePath == ".agents/guidance/topics/image.bin"
            && item.DestinationPath == ".agents/archive/topics/image.bin");
        Assert.Contains(items, item =>
            item.Kind == RouteMoveItemKind.Resource
            && item.SourcePath == ".agents/guidance/topics/assets/settings.json"
            && item.DestinationPath == ".agents/archive/topics/assets/settings.json");
        var effects = plan.Preview.Effects;
        Assert.Equal(
            effects.Select(EffectPhase).Order(),
            effects.Select(EffectPhase));
        AssertOrderedByDepthThenPath(
            effects
                .Where(effect => effect.Kind == RouteMoveEffectKind.Directory
                    && effect.Action == RouteMoveEffectAction.Create)
                .Select(effect => effect.Path),
            deepestFirst: false);
        Assert.Equal(
            effects
                .Where(effect => effect.Kind == RouteMoveEffectKind.MovedFile
                    && effect.Action == RouteMoveEffectAction.Create)
                .Select(effect => effect.Path)
                .Order(StringComparer.Ordinal),
            effects
                .Where(effect => effect.Kind == RouteMoveEffectKind.MovedFile
                    && effect.Action == RouteMoveEffectAction.Create)
                .Select(effect => effect.Path));
        Assert.Equal(
            effects
                .Where(effect => effect.Kind is RouteMoveEffectKind.ReferenceSource
                    or RouteMoveEffectKind.GeneratedRegion)
                .Select(effect => effect.Path)
                .Order(StringComparer.Ordinal),
            effects
                .Where(effect => effect.Kind is RouteMoveEffectKind.ReferenceSource
                    or RouteMoveEffectKind.GeneratedRegion)
                .Select(effect => effect.Path));
        AssertOrderedByDepthThenPath(
            effects
                .Where(effect => effect.Kind == RouteMoveEffectKind.MovedFile
                    && effect.Action == RouteMoveEffectAction.Delete)
                .Select(effect => effect.Path),
            deepestFirst: true);
        AssertOrderedByDepthThenPath(
            effects
                .Where(effect => effect.Kind == RouteMoveEffectKind.Directory
                    && effect.Action == RouteMoveEffectAction.Delete)
                .Select(effect => effect.Path),
            deepestFirst: true);
    }

    [Fact(DisplayName = "Route Move rewrites exact destination spans coalesces files and preserves unrelated bytes")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task ReferencePlanningUsesExactSpansAndCompleteFileCoalescing()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-reference-spans");
        const string oldTarget = ".agents/guidance/old%20guide.md#section";
        const string newTarget = ".agents/guidance/new%20guide.md#section";

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        Assert.Equal(5, plan.Preview.References.Rewrites.Length);
        var definition = Assert.Single(
            plan.Preview.References.Rewrites,
            rewrite => rewrite.SourcePath == "notes.md");
        Assert.Equal(oldTarget, definition.Before);
        Assert.Equal(newTarget, definition.Expected);
        Assert.Equal(
            Utf8Offset(workspace.ReadText("notes.md"), oldTarget),
            definition.Location.ByteOffset);
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(oldTarget), definition.Location.ByteLength);
        var notesPath = workspace.Absolute("notes.md");
        var readmePath = workspace.Absolute("README.md");
        var destinationPath = workspace.Absolute(RouteMoveIntegrationWorkspace.LeafDestination);
        Assert.Equal(
            1,
            plan.Projection.FileChanges.Count(change => change.LogicalPath == notesPath));
        Assert.Equal(
            workspace.ReadText("notes.md").Replace(oldTarget, newTarget, StringComparison.Ordinal),
            IntendedText(plan.Projection.FileChanges, notesPath));
        Assert.Equal(
            workspace.ReadText("README.md")
                .Replace(oldTarget, newTarget, StringComparison.Ordinal)
                .Replace(
                    "<.agents/guidance/old%20guide.md#caf%C3%A9>",
                    "<.agents/guidance/new%20guide.md#caf%C3%A9>",
                    StringComparison.Ordinal),
            IntendedText(plan.Projection.FileChanges, readmePath));
        Assert.Equal(
            workspace.ReadText(RouteMoveIntegrationWorkspace.LeafPath),
            IntendedText(plan.Projection.FileChanges, destinationPath));
        Assert.Contains(
            "https://example.com/a?b=c#d",
            IntendedText(plan.Projection.FileChanges, destinationPath),
            StringComparison.Ordinal);
        Assert.Contains(
            "../archive/_archive.md#entries",
            IntendedText(plan.Projection.FileChanges, destinationPath),
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Move preserves fragment-only and noncanonical relative literals that keep their intended meaning")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task ReferencePlanningPreservesStillValidAuthoredLiterals()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-reference-literal-preservation");
        var child = workspace.ReadText(".agents/guidance/topics/child.md");
        workspace.WriteText(
            ".agents/guidance/topics/child.md",
            child.Replace(
                "# Child\n",
                "# Child\n\n[Self](#child)\n\n[Notes](./notes.md)\n",
                StringComparison.Ordinal));

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(
                RouteMoveIntegrationWorkspace.CategoryId,
                RouteMoveIntegrationWorkspace.CategoryDestination),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        var destinationPath = workspace.Absolute(".agents/archive/topics/child.md");
        var intended = IntendedText(plan.Projection.FileChanges, destinationPath);
        Assert.Contains("[Self](#child)", intended, StringComparison.Ordinal);
        Assert.Contains("[Notes](./notes.md)", intended, StringComparison.Ordinal);
        Assert.Contains(
            "[Old guide](../../guidance/old%20guide.md#section)",
            intended,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            plan.Preview.References.Rewrites,
            rewrite => rewrite.SourcePath == ".agents/guidance/topics/child.md"
                && rewrite.Before is "#child" or "./notes.md");
    }

    [Fact(DisplayName = "Route Move composes authored and generated changes in one document")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task OneDocumentRetainsAuthoredRewriteAndGeneratedProjection()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-coalesced-document");
        var guidance = workspace.ReadText(".agents/guidance/_guidance.md");
        workspace.WriteText(
            ".agents/guidance/_guidance.md",
            guidance.Replace(
                "# Guidance\n",
                "# Guidance\n\n[Authored old guide](old%20guide.md#section)\n",
                StringComparison.Ordinal));

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        var guidancePath = workspace.Absolute(".agents/guidance/_guidance.md");
        Assert.Equal(
            1,
            plan.Projection.FileChanges.Count(change => change.LogicalPath == guidancePath));
        var intended = IntendedText(plan.Projection.FileChanges, guidancePath);
        Assert.Contains(
            "[Authored old guide](new%20guide.md#section)",
            intended,
            StringComparison.Ordinal);
        Assert.Contains(
            "- [Old guide](new%20guide.md) - #Route",
            intended,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "- [Old guide](old%20guide.md) - #Guide",
            intended,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Move ignores moved-path prose while rewriting supported links")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task PlainProseDoesNotReduceSupportedReferenceCoverage()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-reference-prose");
        workspace.WriteText(
            "README.md",
            "# Reference coverage\n\nPlain .agents/guidance/old guide.md prose.\n\n"
            + "Code: `.agents/guidance/old guide.md`.\n\n"
            + "[Supported](.agents/guidance/old%20guide.md#section)\n");

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        Assert.Equal(RouteMoveCoverage.Complete, plan.Preview.References.Coverage);
        var intended = IntendedText(plan.Projection.FileChanges, workspace.Absolute("README.md"));
        Assert.Contains("Plain .agents/guidance/old guide.md prose.", intended, StringComparison.Ordinal);
        Assert.Contains("`.agents/guidance/old guide.md`", intended, StringComparison.Ordinal);
        Assert.Contains(
            "[Supported](.agents/guidance/new%20guide.md#section)",
            intended,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Move validates generated markers in a moved entrypoint")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task MovedEntrypointWithUnsafeGeneratedMarkersIsBlocked()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-entrypoint-markers");
        workspace.WriteText(
            ".agents/guidance/_guidance.md",
            OpenForgeDocumentSeed.Metadata(
                "Guidance",
                ["Route"],
                "# Guidance\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n"));

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(
                ".agents/guidance/_guidance.md",
                ".agents/archive/guidance/_guidance.md"),
            TestContext.Current.CancellationToken);
        var result = new RouteMoveResultBuilder().Build(build.Formation);

        Assert.Null(build.Plan);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteMoveFindingCode.GeneratedRegionUnsafe);
    }

    [Fact(DisplayName = "Route Move generated navigation reports changed and unchanged applicable regions without hidden indexing")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task GeneratedNavigationRetainsEveryApplicableProjection()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-generated-navigation");

        var build = await workspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(
                ".agents/guidance/_guidance.md",
                ".agents/archive/guidance/_guidance.md"),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        Assert.Contains(plan.Preview.GeneratedNavigation.Regions, region =>
            region.Path == ".agents/loader.md"
            && region.State == RouteMoveGeneratedState.Changed
            && region.Reasons.SequenceEqual(
                [RouteMoveGeneratedReason.OldParent, RouteMoveGeneratedReason.Loader]));
        Assert.Contains(plan.Preview.GeneratedNavigation.Regions, region =>
            region.Path == ".agents/archive/_archive.md"
            && region.State == RouteMoveGeneratedState.Changed
            && region.Reasons.SequenceEqual([RouteMoveGeneratedReason.NewParent]));
        Assert.Contains(plan.Preview.GeneratedNavigation.Regions, region =>
            region.Path == ".agents/archive/guidance/_guidance.md"
            && region.State == RouteMoveGeneratedState.Changed
            && region.Reasons.SequenceEqual([RouteMoveGeneratedReason.MovedEntrypoint]));
        Assert.DoesNotContain(
            plan.Projection.FileChanges,
            change => change.LogicalPath == workspace.Absolute(".agents/archive/guidance/_guidance.md")
                && change.Kind == PlannedFileChangeKind.ReplaceGeneratedRegion);
    }

    private static long Utf8Offset(string contents, string literal)
    {
        var characterOffset = contents.IndexOf(literal, StringComparison.Ordinal);
        Assert.True(characterOffset >= 0);
        return System.Text.Encoding.UTF8.GetByteCount(contents.AsSpan(0, characterOffset));
    }

    private static int EffectPhase(RouteMoveEffect effect)
        => (effect.Kind, effect.Action) switch
        {
            (RouteMoveEffectKind.Directory, RouteMoveEffectAction.Create) => 0,
            (RouteMoveEffectKind.MovedFile, RouteMoveEffectAction.Create) => 1,
            (RouteMoveEffectKind.ReferenceSource or RouteMoveEffectKind.GeneratedRegion, _) => 2,
            (RouteMoveEffectKind.MovedFile, RouteMoveEffectAction.Delete) => 3,
            (RouteMoveEffectKind.Directory, RouteMoveEffectAction.Delete) => 4,
            _ => throw new InvalidOperationException(
                $"Unexpected accepted Route Move effect phase: {effect.Kind}/{effect.Action}."),
        };

    private static void AssertOrderedByDepthThenPath(
        IEnumerable<string> paths,
        bool deepestFirst)
    {
        var actual = paths.ToArray();
        var expected = deepestFirst
            ? actual.OrderByDescending(PathDepth).ThenBy(path => path, StringComparer.Ordinal)
            : actual.OrderBy(PathDepth).ThenBy(path => path, StringComparer.Ordinal);
        Assert.Equal(expected, actual);
    }

    private static int PathDepth(string path) => path.Count(character => character == '/');

    private static string IntendedText(
        IEnumerable<PlannedFileChange> changes,
        string path)
    {
        var change = Assert.Single(changes, candidate => candidate.LogicalPath == path);
        return System.Text.Encoding.UTF8.GetString(change.IntendedBytes.AsSpan());
    }

    private static void AssertFileChangesEqual(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        foreach (var (expectedChange, actualChange) in expected.Zip(actual))
        {
            Assert.Equal(expectedChange.Kind, actualChange.Kind);
            Assert.Equal(expectedChange.Expectation, actualChange.Expectation);
            Assert.Equal(expectedChange.IntendedBytes, actualChange.IntendedBytes);
        }
    }

    private static void AssertSubjectsEqual(
        RouteMoveSubject expected,
        RouteMoveSubject actual)
    {
        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Layers.Length, actual.Layers.Length);
        foreach (var (expectedLayer, actualLayer) in expected.Layers.Zip(actual.Layers))
        {
            Assert.Equal(expectedLayer.Layer, actualLayer.Layer);
            Assert.Equal(expectedLayer.SourcePath, actualLayer.SourcePath);
            Assert.Equal(expectedLayer.DestinationPath, actualLayer.DestinationPath);
        }

        Assert.Equal(expected.Items.Length, actual.Items.Length);
        foreach (var (expectedItem, actualItem) in expected.Items.Zip(actual.Items))
        {
            Assert.Equal(expectedItem.Kind, actualItem.Kind);
            Assert.Equal(expectedItem.Layer, actualItem.Layer);
            Assert.Equal(expectedItem.SourceId, actualItem.SourceId);
            Assert.Equal(expectedItem.SourcePath, actualItem.SourcePath);
            Assert.Equal(expectedItem.DestinationPath, actualItem.DestinationPath);
        }
    }

    private static void AssertRecoveryTargetsEqual(
        IReadOnlyList<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundleTarget> expected,
        IReadOnlyList<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundleTarget> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        foreach (var (expectedTarget, actualTarget) in expected.Zip(actual))
        {
            AssertFileChangesEqual([expectedTarget.Change], [actualTarget.Change]);
            Assert.Equal(expectedTarget.Before.Expectation, actualTarget.Before.Expectation);
            Assert.Equal(expectedTarget.Before.Bytes, actualTarget.Before.Bytes);
        }
    }

    private static void AssertReferencesEqual(
        RouteMoveReferences expected,
        RouteMoveReferences actual)
    {
        Assert.Equal(expected.Coverage, actual.Coverage);
        Assert.Equal(expected.ScannedSourceCount, actual.ScannedSourceCount);
        Assert.Equal(expected.InspectedSourceCount, actual.InspectedSourceCount);
        Assert.Equal(expected.OccurrenceCount, actual.OccurrenceCount);
        Assert.Equal(expected.Rewrites.Length, actual.Rewrites.Length);
        foreach (var (expectedRewrite, actualRewrite) in expected.Rewrites.Zip(actual.Rewrites))
        {
            Assert.Equal(expectedRewrite.SourcePath, actualRewrite.SourcePath);
            Assert.Equal(expectedRewrite.DestinationSourcePath, actualRewrite.DestinationSourcePath);
            Assert.Equal(expectedRewrite.Layer, actualRewrite.Layer);
            Assert.Equal(expectedRewrite.Location.Line, actualRewrite.Location.Line);
            Assert.Equal(expectedRewrite.Location.Column, actualRewrite.Location.Column);
            Assert.Equal(expectedRewrite.Location.ByteOffset, actualRewrite.Location.ByteOffset);
            Assert.Equal(expectedRewrite.Location.ByteLength, actualRewrite.Location.ByteLength);
            Assert.Equal(expectedRewrite.Before, actualRewrite.Before);
            Assert.Equal(expectedRewrite.Expected, actualRewrite.Expected);
            Assert.Equal(expectedRewrite.OldTarget.Id, actualRewrite.OldTarget.Id);
            Assert.Equal(expectedRewrite.OldTarget.Path, actualRewrite.OldTarget.Path);
            Assert.Equal(expectedRewrite.ExpectedTarget.Id, actualRewrite.ExpectedTarget.Id);
            Assert.Equal(expectedRewrite.ExpectedTarget.Path, actualRewrite.ExpectedTarget.Path);
        }
    }

    private static void AssertGeneratedNavigationEqual(
        RouteMoveGeneratedNavigation expected,
        RouteMoveGeneratedNavigation actual)
    {
        Assert.Equal(expected.Coverage, actual.Coverage);
        Assert.Equal(expected.Regions.Length, actual.Regions.Length);
        foreach (var (expectedRegion, actualRegion) in expected.Regions.Zip(actual.Regions))
        {
            Assert.Equal(expectedRegion.Path, actualRegion.Path);
            Assert.Equal(expectedRegion.Reasons.Length, actualRegion.Reasons.Length);
            foreach (var (expectedReason, actualReason) in expectedRegion.Reasons.Zip(actualRegion.Reasons))
            {
                Assert.Equal(expectedReason, actualReason);
            }

            Assert.Equal(expectedRegion.State, actualRegion.State);
        }
    }
}
