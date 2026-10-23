using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Cleanup;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Presentation.Remove;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Presentation.Route.Include;
using OpenForge.Cli.Core.Presentation.Route.Exclude;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Cleanup;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.Core.UnitTests.Commands.Repair;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Update;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

[Trait("Feature", "workspace-lock"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed partial class WorkspaceLockFailurePresentationTests
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "lock-presentation"));
    private static readonly string LockPath = Path.Combine(Root, new string('p', 180), new string('q', 180), ".open-forge-state", "workspace.lock");
    private static readonly CliWorkspace Workspace = new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    [Theory(DisplayName = "Every lock cause preserves command coordinates and the full path in every text and JSON detail")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5)]
    public void CausesAcrossCommandFamilies(int cause)
    {
        var failure = WorkspaceLockFailureProjection.From(new WorkspaceLockFailure((WorkspaceLockFailureKind)cause, LockPath));
        var cancelled = failure.Reason == WorkspaceLockFailureReason.Cancelled;
        var status = cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked;
        var rawCause = new string('x', 300);
        VerifyOtherRoutes(failure, status, rawCause);
        VerifyOtherPackages(failure, status, rawCause);

        var indexCode = IndexMutationMapper.ReadLockFindingCode(WorkspaceLockResult.Failed(new((WorkspaceLockFailureKind)cause, LockPath)));
        Assert.NotNull(indexCode);
        var indexFinding = new IndexFinding(indexCode.Value,
            null, null, rawCause, [])
        { LockFailure = failure };
        Verify(new IndexResult(new IndexResultFormation
        {
            Workspace = Workspace,
            Mode = IndexMode.Apply,
            Selection = IndexSelection.NotEstablished(IndexSelectionOrigin.ExplicitSources),
            Regions = [],
            Findings = [indexFinding],
            Recovery = IndexRecovery.NotRequired,
        }), IndexPresentation.Rendering, failure, indexFinding.MachineCode, status);

        var installFinding = new InstallFinding(cancelled ? InstallFindingCode.Interrupted : InstallFindingCode.WorkspaceUnsafe, rawCause)
        { LockFailure = failure };
        Assert.Equal(256, installFinding.Cause.Length);
        Verify(new InstallResult(Workspace, new(false, true, InstallMode.Apply), [installFinding]),
            InstallPresentation.Rendering, failure, cancelled ? "install.interrupted" : "install.workspace-unsafe", status);

        Verify(new UpdateResult(new UpdateResultFormation
        {
            Workspace = Workspace,
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = true,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new()
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new() { State = UpdateRecoveryState.NotRequired, ProtectedPaths = [], ResidualPath = null },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [new UpdateFinding(cancelled ? UpdateFindingCode.Interrupted : UpdateFindingCode.WorkspaceUnsafe, null, rawCause)
            { LockFailure = failure }],
        }), UpdatePresentation.Rendering, failure, cancelled ? "update.interrupted" : "update.workspace-unsafe", status);

        Verify(RemoveResult.Refused(Workspace, ".agents/topic.md", "path",
            cancelled ? RemoveFindingCode.Interrupted : RemoveFindingCode.WorkspaceLockUnavailable,
            status, rawCause, lockFailure: failure), RemovePresentation.Rendering, failure,
            cancelled ? "remove.interrupted" : "remove.workspace-lock-unavailable", status);

        Verify(RepairTestData.Result([new RepairFinding(cancelled ? RepairFindingCode.Interrupted : RepairFindingCode.WorkspaceLockUnavailable, rawCause)
        { LockFailure = failure }]), RepairPresentation.Rendering, failure,
            cancelled ? "repair.interrupted" : "repair.workspace-lock-unavailable", status);

        var cleanupFinding = CleanupFinding.Create(cancelled ? CleanupFindingCode.Interrupted : CleanupFindingCode.WorkspaceLockUnavailable, rawCause)
            with
        { LockFailure = failure };
        Verify(CleanupTestData.Result(CleanupTestData.Facts(effects: [], findings: [cleanupFinding]), status),
            CleanupPresentation.Rendering, failure, cancelled ? "cleanup.interrupted" : "cleanup.workspace-lock-unavailable", status);

        Verify(RouteUpdateTestData.Result(RouteUpdateTestData.VerifiedNoOpFormation(findings:
            [new RouteUpdateFinding(cancelled ? RouteUpdateFindingCode.Interrupted : RouteUpdateFindingCode.WorkspaceLockUnavailable, rawCause)
            { LockFailure = failure }])), RouteUpdatePresentation.Rendering, failure,
            cancelled ? "route-update.interrupted" : "route-update.workspace-lock-unavailable", status);

        var inclusion = new RouteInclusionOutcome
        {
            Status = status,
            Code = cancelled ? RouteInclusionFindingCode.Interrupted : RouteInclusionFindingCode.WorkspaceLockUnavailable,
            Cause = rawCause,
            LockFailure = failure,
        };
        Verify(new RouteIncludeResult { Workspace = Workspace, Requested = "topic.md", Outcome = inclusion },
            RouteIncludePresentation.Rendering, failure, cancelled ? "route-include.interrupted" : "route-include.workspace-lock-unavailable", status);
        Verify(new RouteExcludeResult { Workspace = Workspace, Requested = "topic.md", Outcome = inclusion },
            RouteExcludePresentation.Rendering, failure, cancelled ? "route-exclude.interrupted" : "route-exclude.workspace-lock-unavailable", status);

        Verify(ExtensionInstallResult.Empty(Workspace, ExtensionInstallMode.Apply, false, true,
            new ExtensionInstallFinding(cancelled ? ExtensionInstallFindingCode.Interrupted : ExtensionInstallFindingCode.WorkspaceLockUnavailable, rawCause)
            { LockFailure = failure }), ExtensionInstallPresentation.Rendering, failure,
            cancelled ? "extension-install.interrupted" : "extension-install.workspace-lock-unavailable", status);

        Verify(new LibrarySyncResult
        {
            Status = status,
            Workspace = Workspace,
            Next = null,
            Result = new LibrarySyncPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationPresentationData.Identity(false),
                Record = LibraryMutationPresentationData.Record(),
                Source = LibraryMutationPresentationData.Source(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [new LibrarySyncFinding
                {
                    Code = cancelled ? LibrarySyncFindingCode.Interrupted : LibrarySyncFindingCode.LockUnavailable,
                    Status = status, Cause = rawCause, Path = null, LibraryId = "team-knowledge", LockFailure = failure,
                }],
            },
        }, LibrarySyncPresentation.Rendering, failure, cancelled ? "library-sync.interrupted" : "library-sync.lock-unavailable", status);
    }

    [Fact(DisplayName = "The six lock causes and history wording have reviewed exact messages")]
    public void CauseAndHistoryMessages()
    {
        Assert.Equal(6, Enum.GetValues<WorkspaceLockFailureReason>().Length);
        var messages = Enum.GetValues<WorkspaceLockFailureReason>().Select(reason =>
            $"{reason}: {CliFindingWording.WorkspaceLockUnavailable(new(reason, "<workspace>/.open-forge-state/workspace.lock"))}");
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n", messages), "causes");
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n",
            CliFindingWording.RecoveryHistory("<workspace>/.open-forge-state/recovery/v1/bundle.zip"),
            CliFindingWording.RecoveryCoverageUnavailable("<legacy>/recovery/v1", "Access was denied"),
            CliFindingWording.RecoveryCoverageUnavailable(null, "The legacy location is unavailable")), "history");
        Assert.Throws<ArgumentOutOfRangeException>(() => CliFindingWording.WorkspaceLockUnavailable(new((WorkspaceLockFailureReason)99, LockPath)));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockFailureProjection.From(new((WorkspaceLockFailureKind)99, LockPath)));
    }

    private static void Verify<TResult, TData>(TResult result, CliReportRendering<TResult, TData> rendering,
        WorkspaceLockFailureInfo failure, string code, CliSemanticStatus status)
        where TResult : ICliCommandResult where TData : class
    {
        Assert.Equal(status, result.Status);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            foreach (var format in new[] { CliFormat.Text, CliFormat.Json })
            {
                var output = CliRenderingStage.Render(new CliPresentationRequest<TResult>(result, new(format, detail, null)), rendering);
                Assert.Equal(status, output.Status);
                Assert.DoesNotContain("Nothing was changed. Nothing was changed.", output.PrimaryContent, StringComparison.Ordinal);
                Assert.Equal(format == CliFormat.Json ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError, output.PrimaryTarget);
                Assert.Equal(status == CliSemanticStatus.Interrupted ? 130 : 5,
                    CliProcessCompletionPolicy.Complete(output.Status, output.PrimaryTarget).ExitCode);
                if (format == CliFormat.Text)
                {
                    Assert.Contains(LockPath, output.PrimaryContent, StringComparison.Ordinal);
                }
                else
                {
                    using var json = JsonDocument.Parse(output.PrimaryContent);
                    var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
                    Assert.Equal(code, finding.GetProperty("code").GetString());
                    Assert.Equal("Workspace lock is unavailable", finding.GetProperty("title").GetString());
                    Assert.Contains(LockPath, finding.GetProperty("message").GetString(), StringComparison.Ordinal);
                }
                var message = CliFindingWording.WorkspaceLockUnavailable(failure);
                Assert.Equal(failure.Reason == WorkspaceLockFailureReason.Busy, message.Contains("Another Open Forge command", StringComparison.Ordinal));
                Assert.DoesNotContain("delete", message, StringComparison.OrdinalIgnoreCase);
                Assert.EndsWith("Nothing was changed.", message, StringComparison.Ordinal);
            }
        }
    }
}
