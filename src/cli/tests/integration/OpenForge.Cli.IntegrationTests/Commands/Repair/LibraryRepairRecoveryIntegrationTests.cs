using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class LibraryRepairRecoveryIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task PriorRecordRecoveryCannotOverwriteACurrentRegisteredSource()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-delete");
        File.WriteAllText(workspace.Files.Absolute(LibraryMutationWorkspace.OwnershipPath), """
            {"schemaVersion":1,"libraries":[{"id":"later","sourceRoot":".agents","destinationRoot":"docs","paths":[]}]}
            """);
        var current = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace.Files.Workspace, TestContext.Current.CancellationToken);
        var evidence = new LibraryResidualEvidence(workspace.Evidence.LibraryId, current, workspace.Evidence.VerifiedPriorRecord,
            workspace.Evidence.Residual, workspace.Evidence.Entry);
        var before = workspace.Files.Snapshot();
        var admission = await OpenForge.Cli.Core.Framework.Libraries.Shared.Permissions.LibraryRecoveryPermissionReader.ObserveAsync(
            workspace.Files.Workspace, evidence, TestContext.Current.CancellationToken);
        Assert.False(admission.IsAdmitted);
        Assert.Equal(before, workspace.Files.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public static async Task NewlyRegisteredSourceBlocksSelectedRecoveryBeforeEffects(bool generatedHost, bool afterPreflight)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create", includeUnselectedHost: generatedHost);
        var evidence = generatedHost
            ? new LibraryResidualEvidence(workspace.Evidence.LibraryId, workspace.Evidence.CurrentRecord, null,
                workspace.Evidence.Residual, workspace.Evidence.Residual.Entries[1])
            : workspace.Evidence;
        var plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
        {
            Request = new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false),
            References = [],
            Libraries = [new RepairLibraryRecoveryProposal(evidence)],
            PromptRelinks = [],
            PromptLibraries = null,
        });
        Assert.Single(plan.LibrarySteps);
        var application = RepairOperationFactory.CreateDefaultComponents().Application;
        if (afterPreflight)
        {
            var ready = await application.PreflightAsync(plan, TestContext.Current.CancellationToken);
            Assert.Equal(RepairPreflightState.Ready, ready.Outcome.Preflight.State);
        }
        File.WriteAllText(workspace.Files.Absolute(LibraryMutationWorkspace.OwnershipPath), """
            {"schemaVersion":1,"libraries":[
              {"id":"later","sourceRoot":".agents/directives","destinationRoot":"docs","paths":[]},
              {"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/review.md"]}]}
            """);
        if (!afterPreflight)
        {
            var current = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace.Files.Workspace, TestContext.Current.CancellationToken);
            Assert.Equal(OpenForge.Cli.Core.Framework.Libraries.Models.Observation.LibraryRegistrationReadState.Complete, current.State);
            var freshEvidence = new LibraryResidualEvidence(evidence.LibraryId, current, null, evidence.Residual, evidence.Entry);
            plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
            {
                Request = plan.Request,
                References = [],
                Libraries = [new RepairLibraryRecoveryProposal(freshEvidence)],
                PromptRelinks = [],
                PromptLibraries = null,
            });
            var heldOutcome = await application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken);
            Assert.Equal(RepairPreflightState.Blocked, heldOutcome.Preflight.State);
            Assert.Equal(0, heldOutcome.Application.AppliedEffects);
        }
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var outcome = afterPreflight
            ? await application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken)
            : (await application.PreflightAsync(plan, TestContext.Current.CancellationToken)).Outcome;
        Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.Equal(0, outcome.Application.AppliedEffects);
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }

    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create", false), InlineData("record-replace", false), InlineData("record-delete", false)]
    [InlineData("link-create", false), InlineData("link-delete", false), InlineData("link-delete", true)]
    public static async Task AutomaticRepairAppliesOnlyExactTypedInverseAndPreservesSource(string kind, bool dangling)
    {
        using var workspace = new LibraryResidualWorkspace(dangling);
        await workspace.PrepareAsync(kind);
        var source = workspace.Sources();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var run = await CliHostCapture.RunAsync(["repair", "--automatic", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode is 0 or 2, $"Expected completed recovery: {run.ExitCode}; {run.Error}; {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        // The recovery is reported as an effect on the envelope. The bundle path and the
        // verification state are operation facts the receipt below still proves; the command data
        // reports how the repair was selected and what it relinked.
        Assert.Equal("automatic", document.RootElement.GetProperty("data").GetProperty("selection").GetString());
        var effect = Assert.Single(
            document.RootElement.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("kind").GetString() is "link" or "record");
        Assert.Equal("done", effect.GetProperty("outcome").GetString());
        switch (kind)
        {
            case "record-create":
            case "link-create": Assert.Null(new FileInfo(workspace.TargetPath).LinkTarget); Assert.False(File.Exists(workspace.TargetPath)); break;
            case "link-delete":
                Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", new FileInfo(workspace.TargetPath).LinkTarget);
                break;
            default: Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath)); break;
        }

        Assert.Equal(source, workspace.Sources());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }

    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create"), InlineData("record-replace"), InlineData("record-delete")]
    [InlineData("link-create"), InlineData("link-delete")]
    public static async Task DryRunSelectsExactInverseWithoutEffects(string kind)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync(kind);
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var run = await CliHostCapture.RunAsync(["repair", "--automatic", "--dry-run", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode is 0 or 2, $"Expected valid Library recovery plan: {run.ExitCode}; {run.Error}; {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("data");

        // Library recovery is an effect on the envelope, not a member of the command data. The
        // command data reports how the repair was selected and what it would relink.
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("automatic", result.GetProperty("selection").GetString());
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public static async Task ThirdStateLibraryResidualBlocksBeforeAnySelectedReferenceWrite(bool link)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync(link ? "link-create" : "record-replace");
        workspace.Files.Write(".agents/directives/local.md", """
            ---
            open-forge:
              description: Local
              tags: [Directive]
            ---
            # Local

            [Directives](./_directives.md)
            """);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var diagnosis = await components.DiagnosisReader.ReadAsync(new DoctorRequest(workspace.Files.Workspace), TestContext.Current.CancellationToken);
        var catalogue = await components.CatalogueReader.ReadAsync(workspace.Files.Workspace,
            diagnosis.Observation.LocalReferences, TestContext.Current.CancellationToken);
        var plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
        {
            Request = new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false),
            References = [.. catalogue.Proposals],
            Libraries = [new RepairLibraryRecoveryProposal(workspace.Evidence)],
            PromptRelinks = [],
            PromptLibraries = null,
        });
        Assert.NotEmpty(plan.Effects);
        Assert.Single(plan.LibrarySteps);
        var preflight = await components.Application.PreflightAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RepairPreflightState.Ready, preflight.Outcome.Preflight.State);
        Assert.NotEmpty(preflight.Targets);
        if (link)
        {
            File.Delete(workspace.TargetPath);
            File.CreateSymbolicLink(workspace.TargetPath, "../../shared/changed.md");
        }
        else
        {
            File.WriteAllText(workspace.Files.Absolute(LibraryMutationWorkspace.OwnershipPath), LibraryResidualWorkspace.RecordText + "\n\n");
        }

        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var outcome = await components.Application.ExecuteAsync(plan, preflight.Targets, TestContext.Current.CancellationToken);
        Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.Equal(0, outcome.Application.AppliedEffects);
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MixedReferenceAndLibraryPlanRunsThroughRealOperationAndRetainsSeparateReceipts()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create");
        const string local = ".agents/directives/local.md";
        workspace.Files.Write(local, "---\nopen-forge:\n  description: Local\n  tags: [Directive]\n---\n# Local\n\n[Directives](./_directives.md)\n");
        var source = workspace.Sources();
        var run = await CliHostCapture.RunAsync(["repair", "--automatic", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode is 0 or 2, $"Expected mixed repair: {run.ExitCode}; {run.Error}; {run.Output}");
        Assert.Contains("[Directives](_directives.md)", File.ReadAllText(workspace.Files.Absolute(local)), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.TargetPath));
        Assert.Null(new FileInfo(workspace.TargetPath).LinkTarget);
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("data");
        Assert.Equal("automatic", result.GetProperty("selection").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("kind").GetString() is "link" or "record");
        Assert.Equal(source, workspace.Sources());
        Assert.True(File.Exists(workspace.Preparation.BundlePath));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ReceiptRejectsAnotherVerifiedOriginalEvenWithSameEntryShape()
    {
        using var selected = new LibraryResidualWorkspace();
        using var other = new LibraryResidualWorkspace();
        await selected.PrepareAsync("record-replace");
        await other.PrepareAsync("record-replace");
        var result = Ordinary(selected.Evidence.Entry);
        Assert.Throws<ArgumentException>(() => new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary,
            selected.Effect(), other.Preparation, result, relativeFileLink: null));
        var receipt = new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary,
            selected.Effect(), selected.Preparation, result, relativeFileLink: null);
        Assert.Same(selected.Preparation, receipt.OriginalResidual);
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public static async Task ReceiptRejectsIndependentWrongBeforeOrAfterContext(bool after)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var entry = workspace.Evidence.Entry.Input.Context.Entry;
        var other = RecoveryEntry.Create(entry.Ordinal, CanonicalRelativePath.Create(".agents/other.json"),
            entry.Kind, entry.Prior, entry.Intended, entry.PriorPayload);
        var context = new RecoveryEntryComparisonContext(workspace.Files.Workspace, other);
        var changed = workspace.Evidence.Entry with
        {
            Input = new RecoveryEntryComparisonInput(context, NoFollowLeafObservation.OrdinaryFile(context.LogicalPath),
                new RecoveryOrdinaryContentObservation(context.LogicalPath, other.Intended.OrdinaryFile, null)),
        };
        var result = after ? Ordinary(workspace.Evidence.Entry) with { After = changed } : Ordinary(changed);
        Assert.Throws<ArgumentException>(() => new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary,
            workspace.Effect(), workspace.Preparation, result, relativeFileLink: null));
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public static async Task ReceiptRejectsSameWorkspaceOtherOriginalOrForwardPreparation(bool forward)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var other = await workspace.PrepareOtherAsync(forward);
        var error = Assert.Throws<ArgumentException>(() => new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary,
            workspace.Effect(), other, Ordinary(workspace.Evidence.Entry), relativeFileLink: null));
        Assert.Equal("originalResidual", error.ParamName);
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ReceiptRejectsUndefinedKindWithRealOriginalPreparation()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        Assert.Throws<ArgumentOutOfRangeException>(() => new RepairLibraryRecoveryReceipt((RepairLibraryRecoveryKind)int.MaxValue,
            workspace.Effect(), workspace.Preparation, Ordinary(workspace.Evidence.Entry), relativeFileLink: null));
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("cancellation"), InlineData("failure"), InlineData("cleanup-unknown")]
    public static async Task CompletionRetainsVerifiedInverseWhenLaterExecutionCannotComplete(string later)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(workspace.Files.Workspace, "repair", Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, acquired.State);
        Assert.NotNull(acquired.Lease);
        await using var lease = acquired.Lease;
        var recovered = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
            workspace.Preparation, workspace.Effect().Entry, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemEffectState.Applied, recovered.Effect);
        Assert.Equal(FilesystemVerificationState.Verified, recovered.Verification);
        var effect = workspace.Effect();
        var receipt = new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary, effect,
            workspace.Preparation, recovered, relativeFileLink: null);
        var step = new RepairLibraryRecoveryStep
        {
            Ordinal = 0,
            Selection = effect.Selection,
            Dependency = new RepairDependency([RepairDependencyDomain.WorkspaceContainment, RepairDependencyDomain.LibraryRegistration, RepairDependencyDomain.LibraryResidual]),
            Verification = new RepairVerificationRequirement([RepairVerificationKind.NoFollowIdentity, RepairVerificationKind.PriorState]),
            Effect = effect,
            Outcome = RepairStepOutcome.Planned,
        };
        var plan = new RepairPlan(new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false),
            new RepairSelection(RepairSelectionMode.Automatic, [], [], new RepairLibrarySelection([effect.Selection], [])), [], [], [step]);
        var execution = new RepairLibraryExecution
        {
            ReferenceReceipts = [],
            LibraryReceipts = [receipt],
            ForwardPreparation = null,
            PostDiagnosis = null,
            ForwardCleanup = later == "cleanup-unknown"
                ? RecoveryBundleDeletionResult.FailedUnknown(Path.Combine(workspace.Files.Path, "independent-forward.zip"), "Cleanup state unknown.") : null,
            Cancellation = later == "cancellation" ? new RepairLibraryCancellation(RepairLibraryExecutionStage.PostDiagnosis, null) : null,
            UnexpectedFailure = later == "failure" ? new RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage.PostDiagnosis, null, "Post-diagnosis failed.") : null,
        };
        var outcome = RepairLibraryRecoveryApplication.Complete(plan, execution);
        Assert.Same(execution, outcome.LibraryExecution);
        Assert.Equal(1, outcome.Application.AppliedEffects);
        Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath));
        Assert.True(File.Exists(workspace.Preparation.BundlePath));
        Assert.NotNull(outcome.LibraryExecution);
        Assert.Same(receipt, Assert.Single(outcome.LibraryExecution.LibraryReceipts));
        if (later == "cleanup-unknown")
        {
            Assert.Equal(RepairRecoveryState.Unknown, outcome.Recovery.State);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public static async Task RelativeLinkReceiptRejectsWrongBeforeOrAfterLogicalIdentity(bool after)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create");
        var entry = workspace.Effect().Entry;
        var exact = workspace.Evidence.Entry.Input.Leaf;
        var foreign = NoFollowLeafObservation.Missing(workspace.Files.Absolute(".agents/other.md"));
        var result = RelativeFileLinkRecoveryResult.Classified(RelativeFileLinkRecoveryState.Mismatched, entry,
            after ? exact : foreign, after ? foreign : null, "Independent link identity mismatch.");
        Assert.Throws<ArgumentException>(() => new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.RelativeFileLink,
            workspace.Effect(), workspace.Preparation, ordinary: null, result));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task SelectedSubsetPreservesUnselectedEntryAndEntireOriginalArchive()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace", includeUnselectedHost: true);
        var selected = new RepairLibraryRecoveryProposal(workspace.Evidence);
        var unselected = new RepairLibraryRecoveryProposal(new LibraryResidualEvidence(workspace.Evidence.LibraryId,
            workspace.Evidence.CurrentRecord, null, workspace.Evidence.Residual, workspace.Evidence.Residual.Entries[1]));
        var plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
        {
            Request = new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: false, [], allowInteraction: true),
            References = [],
            Libraries = [selected, unselected],
            PromptRelinks = [],
            PromptLibraries = [selected],
        });
        Assert.Same(selected, Assert.Single(plan.Selection.Libraries.Selected).Proposal);
        Assert.Same(unselected, Assert.Single(plan.Selection.Libraries.Unselected));
        var original = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var hostPath = workspace.Files.Absolute(".agents/directives/_directives.md");
        var host = File.ReadAllBytes(hostPath);
        var sources = workspace.Sources();
        var outcome = await RepairOperationFactory.CreateDefaultComponents().Application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken);
        Assert.Equal(1, outcome.Application.AppliedEffects);
        Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath));
        Assert.Equal(host, File.ReadAllBytes(hostPath));
        Assert.Equal(original, File.ReadAllBytes(workspace.Preparation.BundlePath));
        Assert.Equal(sources, workspace.Sources());
    }

    private static OrdinaryFileRecoveryResult Ordinary(RecoveryEntryComparison before)
        => new()
        {
            Before = before,
            After = null,
            Effect = FilesystemEffectState.NotStarted,
            Verification = FilesystemVerificationState.NotStarted,
            Failure = null,
            Cause = null,
        };

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MatchingReceiptPrecedesAtomicFailureAttribution()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
        {
            Request = new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false),
            References = [],
            Libraries = [new RepairLibraryRecoveryProposal(workspace.Evidence)],
            PromptRelinks = [],
            PromptLibraries = null,
        });
        Assert.False(plan.IsBlocked);
        var step = Assert.Single(plan.LibrarySteps);
        Assert.Equal(1, step.Ordinal);
        Assert.NotNull(step.Effect);
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(workspace.Files.Workspace, "repair", Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, acquired.State);
        Assert.NotNull(acquired.Lease);
        await using var lease = acquired.Lease;
        var recovered = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
            workspace.Preparation, step.Effect.Entry, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemEffectState.Applied, recovered.Effect);
        Assert.Equal(FilesystemVerificationState.Verified, recovered.Verification);
        var receipt = new RepairLibraryRecoveryReceipt(RepairLibraryRecoveryKind.Ordinary, step.Effect,
            workspace.Preparation, recovered, relativeFileLink: null);
        var execution = new RepairLibraryExecution
        {
            ReferenceReceipts = [],
            LibraryReceipts = [receipt],
            ForwardPreparation = null,
            ForwardCleanup = null,
            PostDiagnosis = null,
            Cancellation = new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, 0),
            UnexpectedFailure = new RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage.Effect, 0, "Independent effect failure."),
        };
        var result = RepairLibraryResultFormation.Build(new RepairResultInput
        {
            Request = plan.Request,
            Plan = plan,
            LibraryExecution = execution,
            Diagnosis = new RepairDiagnosisCoverage(RepairCoverageState.Complete, RepairCoverageState.Complete, RepairCoverageState.Complete, RepairCoverageState.Complete),
            Findings = [],
            InitialFindings = [],
            Preflight = RepairPreflight.NotRequested,
            Application = RepairApplication.NotRequested,
            Verification = RepairVerification.NotRequested,
            Recovery = RepairRecovery.NotRequired,
            PostDiagnosis = RepairPostDiagnosis.NotRequested,
        });

        Assert.NotNull(result.Plan);
        Assert.Equal(RepairStepOutcome.Verified, Assert.Single(result.Plan.LibrarySteps).Outcome);
        Assert.Same(execution, result.LibraryExecution);
        Assert.Same(receipt, Assert.Single(execution.LibraryReceipts));
        Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath));
        Assert.True(File.Exists(workspace.Preparation.BundlePath));
    }
}
