using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateTargetObservationIntegrationTests
{
    [Theory(DisplayName = "Route Update selects one logical target by ID base path or overwrite path")]
    [InlineData(RouteUpdateIntegrationWorkspace.TargetId, (int)RouteUpdateTargetSelection.SourceId)]
    [InlineData(RouteUpdateIntegrationWorkspace.TargetPath, (int)RouteUpdateTargetSelection.BasePath)]
    [InlineData(RouteUpdateIntegrationWorkspace.OverwritePath, (int)RouteUpdateTargetSelection.OverwritePath)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task LogicalTargetSelectionNeverRedirectsMutation(
        string reference,
        int expectedSelectionValue)
    {
        var expectedSelection = (RouteUpdateTargetSelection)expectedSelectionValue;
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-selection-{expectedSelection.ToString().ToLowerInvariant()}");
        workspace.SeedOverwrite();
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(reference);

        var observation = Assert.IsType<RouteUpdateObservation>(build.Observation);
        Assert.Null(build.Boundary);
        Assert.Equal(expectedSelection, observation.Target.SelectedBy);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TargetId, observation.Target.Id);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TargetPath, observation.Target.Path);
        Assert.Equal(
            [RouteUpdateIntegrationWorkspace.OverwritePath],
            observation.Target.OverwritePaths);
        var overwrite = Assert.IsType<FileStateSnapshot>(observation.OverwriteSnapshot);
        Assert.Equal(
            workspace.Absolute(RouteUpdateIntegrationWorkspace.OverwritePath),
            overwrite.LogicalPath);
        Assert.Equal(
            RouteUpdateIntegrationWorkspace.OverwriteText,
            Encoding.UTF8.GetString(overwrite.Bytes.AsSpan()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Route Update preserves ordinary canonical and compatibility entrypoint identity")]
    [InlineData(RouteUpdateIntegrationWorkspace.TargetPath, (int)RouteUpdateTargetForm.OrdinaryMarkdown)]
    [InlineData(".agents/memory/project-alpha/overview/_overview.md", (int)RouteUpdateTargetForm.CanonicalEntrypoint)]
    [InlineData(".agents/memory/project-alpha/overview/index.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint)]
    [InlineData(".agents/memory/project-alpha/overview/_index.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint)]
    [InlineData(".agents/memory/project-alpha/overview/references.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint)]
    [InlineData(".agents/memory/project-alpha/overview/_references.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task SupportedFormsRetainTheirActualBasePath(
        string path,
        int expectedFormValue)
    {
        var expectedForm = (RouteUpdateTargetForm)expectedFormValue;
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-form-{Path.GetFileNameWithoutExtension(path)}");
        if (!string.Equals(
                path,
                RouteUpdateIntegrationWorkspace.TargetPath,
                StringComparison.Ordinal))
        {
            workspace.SeedTargetForm(path);
        }

        var build = await workspace.ObserveAsync(path);

        var observation = Assert.IsType<RouteUpdateObservation>(build.Observation);
        Assert.Equal(expectedForm, observation.Target.Form);
        Assert.Equal(path, observation.Target.Path);
        Assert.Equal(workspace.Absolute(path), observation.TargetSnapshot.LogicalPath);
        Assert.Null(observation.OverwriteSnapshot);
    }

    [Theory(DisplayName = "Route Update maps missing and unsafe workspace roots before target resolution")]
    [InlineData(false, (int)RouteUpdateFindingCode.WorkspaceUnavailable)]
    [InlineData(true, (int)RouteUpdateFindingCode.WorkspaceUnsafe)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task RootFailureIsTypedAndWriteFree(
        bool unsafeRoot,
        int expectedFindingValue)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.CreateUnseeded(
            $"route-update-root-{(unsafeRoot ? "unsafe" : "missing")}");
        using var outside = TemporaryWorkspace.Create("route-update-root-outside");
        if (unsafeRoot)
        {
            Assert.True(
                workspace.TrySeedUnsafeAgentsRoot(outside),
                "This integration case requires real symbolic-link support.");
        }
        var before = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var build = await workspace.ObserveAsync(RouteUpdateIntegrationWorkspace.TargetId);

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == (RouteUpdateFindingCode)expectedFindingValue);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update blocks a selected source physical alias before planning")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task SelectedPhysicalAliasIsBlockedBeforePlanning()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-physical-alias");
        Assert.True(
            workspace.TrySeedTargetPhysicalAlias(),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();

        var build = await workspace.BuildPlanAsync(workspace.Request(
            patch: RouteUpdateIntegrationWorkspace.ResponsibilityPatch("After")));

        Assert.Null(build.Plan);
        Assert.Contains(
            build.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.IdentityCollision);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update blocks an ambiguous logical target before mutation")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task AmbiguousTargetIsBlocked()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-ambiguous");
        workspace.SeedAmbiguousTarget();
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(RouteUpdateIntegrationWorkspace.TargetId);

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.RouteAmbiguous);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Route Update rejects unknown IDs and missing exact paths as invalid targets")]
    [InlineData("memory/project-alpha/missing")]
    [InlineData(".agents/memory/project-alpha/missing.md")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingTargetReferencesAreInvalidAndWriteFree(string reference)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-missing");
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(reference);

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        var result = new RouteUpdateResultBuilder().Build(boundary.Formation);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteUpdatePlanCompleteness.NotEstablished, result.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.Blocked, result.Plan.Safety);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.InvalidTarget);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update rejects detached routed forms instead of adopting them")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task DetachedTargetIsInvalid()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-detached");
        workspace.SeedDetachedTarget();
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(".agents/detached/_detached.md");

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        var result = new RouteUpdateResultBuilder().Build(boundary.Formation);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteUpdatePlanCompleteness.NotEstablished, result.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.Blocked, result.Plan.Safety);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.InvalidTarget);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update rejects an orphan overwrite selection instead of adopting its base")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task OrphanOverwriteTargetIsInvalid()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-orphan-overwrite");
        workspace.SeedOrphanTarget();
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(
            ".agents/orphan/leaf.overwrite.md");

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        var result = new RouteUpdateResultBuilder().Build(boundary.Formation);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteUpdatePlanCompleteness.NotEstablished, result.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.Blocked, result.Plan.Safety);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.InvalidTarget);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update keeps unavailable overwrite route facts incomplete")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnavailableOverwriteRouteFactsAreIncompleteAndWriteFree()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-overwrite-unavailable");
        workspace.SeedOverwrite();
        workspace.SeedUnavailableRouteFacts();
        var before = workspace.SnapshotHashes();

        var build = await workspace.ObserveAsync(RouteUpdateIntegrationWorkspace.OverwritePath);

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        var result = new RouteUpdateResultBuilder().Build(boundary.Formation);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteUpdatePlanCompleteness.Incomplete, result.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.NotEstablished, result.Plan.Safety);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.InspectionIncomplete);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update rejects protected native sources before patching")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task ProtectedNativeTargetIsInvalid()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-target-protected");
        workspace.SeedProtectedSkill();

        var build = await workspace.ObserveAsync(
            ".agents/memory/project-alpha/native/SKILL.md");

        Assert.Null(build.Observation);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.InvalidTarget);
    }
}
