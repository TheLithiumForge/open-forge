using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class LibraryAttachBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<LibraryAttachResult> Renderers = CommandOutputRenderers<LibraryAttachResult>.From(LibraryAttachPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library attach output preserves cancellation between real link and record application stages")]
    public async Task CancelledBetweenRealApplicationStages()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var artifacts = new LibraryOutputArtifacts(workspace);
        using var locks = WorkspaceLockTestStore.Create("library-output-cancelled-stages");
        workspace.Source();
        workspace.Directory(".agents/directives");
        artifacts.OwnLink(LibraryMutationWorkspace.Leaf);
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}");
        var sourceBefore = File.ReadAllBytes(sourcePath);
        var request = workspace.Attach(LibraryMode.Apply);
        var observations = await ObservePlanAsync(workspace, request);
        var plan = LibraryAttachPlanner.Plan(observations, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Complete, plan.State);
        Assert.Empty(plan.Directories);
        Assert.Empty(plan.GeneratedRegions);
        var link = Assert.Single(plan.Links);
        var recordChange = Assert.IsType<PlannedFileChange>(plan.OwnershipChange);
        var permission = await workspace.Permissions.DetermineAsync(new LibraryPermissionRequest
        {
            Workspace = workspace.Workspace,
            LibraryId = request.LibraryId,
            SettingsObservation = observations.Settings,
            Targets = [new LibraryPermissionTarget(link.DestinationPath.Value, LibraryPermissionTargetUse.Live)
                { Effect = LibraryPermissionEffect.CreateLink }],
            AllowPrompt = false,
        }, TestContext.Current.CancellationToken);
        Assert.Null(permission.Failure);
        Assert.Null(permission.Change);
        plan = plan with { Permissions = permission };
        var acquired = await locks.AcquireAsync(new WorkspaceLockRequest(workspace.Workspace,
            LibraryAttachDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await LibraryMutationOperationSupport.PrepareRecoveryAsync(new LibraryRecoveryPreparationRequest
        {
            Lease = lease,
            Command = LibraryAttachDefinitions.CommandIdentity,
            Operation = RecoveryBundleOperation.Attach,
            Permissions = permission,
            Links = plan.Links,
            GeneratedRegions = plan.GeneratedRegions,
            Ownership = observations.Ownership,
            OwnershipChange = recordChange,
            Mappings = observations.Mappings,
        }, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var revalidator = new MutationRevalidator(validator);
            var validation = await revalidator.ValidateAsync(lease, [recordChange], TestContext.Current.CancellationToken);
            Assert.Equal(MutationValidationState.Valid, validation.State);
            var applied = await LibraryMutationApplicationRunner.ApplyAsync(new LibraryMutationApplicationRequest
            {
                Permissions = permission,
                Lease = lease,
                Directories = plan.Directories,
                Links = plan.Links,
                GeneratedRegions = plan.GeneratedRegions,
                OwnershipChange = null,
                RecoveryPreparation = preparation,
                ProtectedSourceRoots = [request.SourceRoot],
            }, TestContext.Current.CancellationToken);
            Assert.Null(applied.UnexpectedFailure);
            Assert.Null(applied.Cancellation);
            Assert.Equal(FilesystemVerificationState.Verified, Assert.Single(applied.Links).VerificationState);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var record = await new FileChangeApplier(revalidator, validator).ApplyAsync(
                lease, recordChange, Assert.Single(validation.Checks), preparation, cancellation.Token);
            Assert.Equal(FilesystemEffectState.NotStarted, record.EffectState);
            Assert.Equal(FilesystemNotStartedReason.Cancelled, record.NotStartedReason);
            var scope = Assert.IsType<LibrarySourceEffectScopeFacts>(applied.SourceEffectScope);

            // The collector has no between-effect hook. Preserve its actual receipts;
            // map the observed cancelled receipt at the same application boundary.
            var execution = applied with
            {
                Record = record,
                RecoveryPreparationOutcome = prepared,
                Cancellation = new LibraryCancellationFact(LibraryExecutionStage.Application),
                SourceEffectScope = scope with
                {
                    AttemptedMutationTargets = scope.AttemptedMutationTargets.Add(CanonicalRelativePath.Create(LibraryMutationWorkspace.RecordPath)),
                },
            };
            var result = LibraryAttachCompletion.Complete(new LibraryAttachCompletionInput
            {
                Request = request,
                Plan = plan,
                Observations = observations,
                Execution = execution,
            });
            artifacts.OwnRecovery(result.Result.Application.Recovery);
            Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
            Assert.Equal(link.RawRelativeTarget, new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            Assert.Equal(sourceBefore, File.ReadAllBytes(sourcePath));
            Assert.True(File.Exists(preparation.BundlePath));
            Renderers.MatchDetails(result, "interrupted-partial", result.Result.Application.Recovery.Path);
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }

    private static async Task<LibraryAttachPlanningInput> ObservePlanAsync(LibraryMutationWorkspace workspace, LibraryAttachRequest request)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var resolver = new PhysicalPathResolver();
        var ownership = await WorkspaceOwnershipReader.ReadAsync(resolver, workspace.Workspace, cancellation);
        var record = LibraryRegistrationReader.Read(ownership);
        var sourceRoot = LibrarySourceRootReader.Read(resolver,
            new LibrarySourceRootRequest { Workspace = workspace.Workspace, SourceRoot = request.SourceRoot }, cancellation);
        Assert.Equal(LibrarySourceRootState.Available, sourceRoot.State);
        var source = await LibraryInventoryReader.ReadAsync(resolver, sourceRoot, cancellation);
        var inventory = Assert.IsType<LibraryInventory>(source.Inventory);
        var mappings = LibraryMutationOperationSupport.ObserveMappings(resolver, new LibraryMappingSetRequest
        {
            Workspace = workspace.Workspace,
            SourceRoot = request.SourceRoot,
            DestinationRoot = request.DestinationRoot,
            Paths = [.. inventory.Entries.Select(entry => entry.SourcePath)],
        }, cancellation);
        var navigation = await LibraryGeneratedNavigationReader.ReadAsync(new LibraryGeneratedNavigationRequest
        {
            Workspace = workspace.Workspace,
            SelectedLibrary = LibraryRegistration.Create(request.LibraryId, request.SourceRoot, request.DestinationRoot,
                [.. inventory.Entries.Select(entry => entry.SourcePath)]),
            CurrentRecord = record.Record,
            IntendedEntries = inventory.Entries,
            Settings = OpenForge.Cli.Core.Framework.Settings.Models.Document.WorkspaceSettingsDocument.Empty,
        }, cancellation);
        Assert.Null(navigation.Issue);
        var ancestors = LibraryMutationOperationSupport.ReadAncestors(workspace.Workspace,
            mappings.Select(mapping => mapping.Mapping.DestinationPath.Value), navigation.Changes);
        var boundary = await LibraryConsumerBoundaryReader.ReadAsync(resolver, new LibraryConsumerBoundaryRequest
        {
            Workspace = workspace.Workspace,
            RequiredAncestorPaths = ancestors,
        }, cancellation);
        return new LibraryAttachPlanningInput
        {
            Request = request,
            ConsumerBoundary = boundary,
            Record = record,
            Source = source,
            Mappings = mappings,
            Ownership = ownership,
            Settings = OpenForge.Cli.Core.Framework.Settings.Models.Observation.WorkspaceSettingsRead.Absent(workspace.Absolute(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath)),
            GeneratedRegionChanges = navigation.Changes,
            GeneratedNavigationIssue = navigation.Issue,
        };
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library attach output preserves source projection, permissions and refusal boundaries")]
    [InlineData("attached-inside-agents", (int)CliSemanticStatus.Complete)]
    [InlineData("attached-outside-with-flag", (int)CliSemanticStatus.Complete)]
    [InlineData("permission-prompt", (int)CliSemanticStatus.Complete)]
    [InlineData("permission-required-non-interactive", (int)CliSemanticStatus.Blocked)]
    [InlineData("empty-source", (int)CliSemanticStatus.Complete)]
    [InlineData("duplicate-id", (int)CliSemanticStatus.Blocked)]
    [InlineData("source-missing", (int)CliSemanticStatus.Invalid)]
    [InlineData("destination-collision", (int)CliSemanticStatus.Blocked)]
    [InlineData("removed-library-id", (int)CliSemanticStatus.Blocked)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    public async Task Attach(string situation, int status)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var artifacts = new LibraryOutputArtifacts(workspace);
        var outside = situation is "attached-outside-with-flag" or "permission-prompt" or "permission-required-non-interactive";
        var target = outside ? "docs/review.md" : LibraryMutationWorkspace.Leaf;
        workspace.Directory(outside ? "docs" : ".agents/directives");
        if (situation != "empty-source") workspace.Source(target);
        if (situation == "duplicate-id") workspace.Record();
        if (situation == "removed-library-id") workspace.Write(".agents/open-forge.json", "{\"removedLibraries\":[\"team-knowledge\"]}");
        if (situation == "destination-collision") workspace.Write(target, "local occupant\n");
        artifacts.OwnLink(target);
        var prompted = situation == "permission-prompt";
        if (prompted || situation == "attached-outside-with-flag") artifacts.OwnSettings();
        using var input = new StringReader(prompted ? "always\n" : string.Empty);
        using var prompts = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, prompts, prompted));
        var request = workspace.Attach("team-knowledge", situation == "source-missing" ? "shared/missing" : LibraryMutationWorkspace.SourceRoot,
            ".", situation == "dry-run" ? LibraryMode.DryRun : LibraryMode.Apply) with
        {
            AllowPrompt = prompted,
            Automatic = !prompted && situation != "dry-run",
            Allow = situation == "attached-outside-with-flag" ? ["docs"] : [],
        };
        var before = workspace.Snapshot();
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{target}");
        var sourceExisted = File.Exists(sourcePath);
        var sourceBefore = sourceExisted ? File.ReadAllBytes(sourcePath) : [];
        LibraryAttachResult result;
        using (var held = situation == "lock-held" ? artifacts.HoldLock() : null)
        {
            result = await new LibraryAttachOperation(
                permissions,
                prompted ? LibraryPermissionTestPrompt.AttachConfirmation() : null)
                .ExecuteAsync(request, TestContext.Current.CancellationToken);
        }
        artifacts.OwnRecovery(result.Result.Application.Recovery);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "attached-inside-agents" or "attached-outside-with-flag" or "permission-prompt")
            Assert.NotNull(new FileInfo(workspace.Absolute(target)).LinkTarget);
        else if (situation != "empty-source") Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceExisted, File.Exists(sourcePath));
        if (sourceExisted) Assert.Equal(sourceBefore, File.ReadAllBytes(sourcePath));
        Renderers.MatchDetails(result, situation, result.Result.Application.Recovery.Path,
            testName: $"{nameof(Attach)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library attach output preserves invalid arguments without effects")]
    public async Task InvalidInput()
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["library", "attach", "INVALID ID"],
            ExitCode = 4,
        });
        Assert.Equal(before, workspace.Snapshot());
    }
}
