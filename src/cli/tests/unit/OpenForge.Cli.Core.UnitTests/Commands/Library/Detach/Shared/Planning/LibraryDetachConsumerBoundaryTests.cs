using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Planning;

public sealed class LibraryDetachConsumerBoundaryTests
{
    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("unobserved"), InlineData("ancestor-gap")]
    public void CoverageGapNeverMeansMissingDirectory(string scenario)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            ConsumerBoundary = input.ConsumerBoundary with
            {
                ConsumerRoot = scenario == "unobserved" ? null : input.ConsumerBoundary.ConsumerRoot,
                Ancestors = [],
                IsComplete = false,
            }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.Empty(plan.Directories);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("file-root"), InlineData("linked-parent")]
    public void PositiveUnsafeBoundaryBlocksCompletePlan(string scenario)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var absolute = LibraryMutationPlanningData.Absolute(
            scenario == "linked-parent" ? ".agents/directives" : ".agents");
        var leaf = scenario switch
        {
            "file-root" => NoFollowLeafObservation.OrdinaryFile(absolute),
            _ => NoFollowLeafObservation.Classified(absolute, NoFollowLeafState.ReparsePoint),
        };
        var observation = new LibraryConsumerDirectoryObservation(
            leaf,
            PhysicalPathResolution.Contained(absolute, absolute));
        input = input with
        {
            ConsumerBoundary = scenario == "linked-parent"
            ? input.ConsumerBoundary with { Ancestors = [observation] }
            : input.ConsumerBoundary with { ConsumerRoot = observation }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public async Task MissingSettingsRootPlansSettingsOnlyDetachAndRecordsExplicitLibraryId()
    {
        var input = LibraryMutationPlanningData.Detach() with
        {
            Record = LibraryMutationPlanningData.MissingRecord(),
            Mappings = [],
            Ownership = LibraryMutationPlanningData.WorkspaceOwnership(),
            ConsumerBoundary = LibraryMutationPlanningData.Boundary() with
            {
                Request = LibraryMutationPlanningData.Boundary().Request with
                {
                    RequiredAncestorPaths = [],
                },
                ConsumerRoot = LibraryMutationPlanningData.DirectoryObservation(".agents", missing: true),
                Ancestors = [],
                IsComplete = true,
            },
        };

        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Equal(LibraryMutationPlanningData.Id, plan.Input.Request.LibraryId.Value);
        Assert.Equal(
            LibraryMutationPlanningData.Absolute(".agents"),
            Assert.Single(plan.Directories).LogicalPath);
        Assert.Empty(plan.Links);
        Assert.Empty(plan.GeneratedRegions);
        Assert.Null(plan.OwnershipChange);
        Assert.Null(plan.IntendedRecord);

        var permissions = await new LibraryPermissionOperation(
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<CliPermissionChoice>.Unavailable()))
            .DetermineAsync(new LibraryPermissionRequest
            {
                Workspace = input.Request.Workspace,
                LibraryId = input.Request.LibraryId,
                SettingsObservation = input.Settings,
                Targets = [],
                RemovalSelection = new WorkspaceRemovalSelection
                {
                    Libraries = [input.Request.LibraryId.Value],
                },
                AllowPrompt = false,
            }, TestContext.Current.CancellationToken);

        Assert.Null(permissions.Failure);
        Assert.Equal(WorkspacePermissionAction.Create, permissions.Result.Action);
        Assert.Equal(WorkspacePermissionOutcome.Planned, permissions.Result.Outcome);
        var settingsChange = Assert.IsType<PlannedFileChange>(permissions.Change);
        Assert.Equal(PlannedFileChangeKind.Create, settingsChange.Kind);
        using var document = JsonDocument.Parse(settingsChange.IntendedBytes.ToArray());
        Assert.Equal(
            LibraryMutationPlanningData.Id,
            Assert.Single(document.RootElement.GetProperty("removedLibraries").EnumerateArray()).GetString());
    }
}
