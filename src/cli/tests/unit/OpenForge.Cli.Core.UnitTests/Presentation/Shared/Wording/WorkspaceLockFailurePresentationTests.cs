using OpenForge.Cli.TestSupport.Isolation;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Route.Include.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Install;
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
    private const string WorkspaceName = "lock-presentation";
    private const string WorkspacePlaceholder = "<workspace>";
    private const string TopicName = "topic.md";
    private const string TargetKind = "path";
    private const string BundleName = "bundle.zip";
    private const string DeniedCause = "Access was denied";
    private const string LegacyUnavailableCause = "The legacy location is unavailable";
    private const string HistoryUnavailableCause = "History is unavailable";
    private static readonly string Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), WorkspaceName));
    private static readonly CliWorkspace Workspace = new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    private static readonly string LockWorkspaceRoot = Path.Combine(Root, new string('p', 180), new string('q', 180));
    private static readonly CliWorkspace LockWorkspace = new(LockWorkspaceRoot, LockWorkspaceRoot, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    private static readonly string LockPath = WorkspaceRuntimePaths.LockPath(LockWorkspace);
    private readonly Dictionary<string, string> _captures = new(StringComparer.Ordinal);

    [Theory(DisplayName = "Every lock cause preserves command coordinates and the full path in every text and JSON detail")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5)]
    public void CausesAcrossCommandFamilies(int cause)
    {
        var failure = WorkspaceLockFailureProjection.From(new WorkspaceLockFailure((WorkspaceLockFailureKind)cause, LockPath));
        Assert.Equal((WorkspaceLockFailureReason)cause, failure.Reason);
        Assert.Equal(LockPath, failure.Path);
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
            InstallPresentation.Rendering, failure, InstallDefinitions.ReadMachineName(cancelled ? InstallFindingCode.Interrupted : InstallFindingCode.WorkspaceUnsafe), status);

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
        }), UpdatePresentation.Rendering, failure, UpdateDefinitions.ReadMachineName(cancelled ? UpdateFindingCode.Interrupted : UpdateFindingCode.WorkspaceUnsafe), status);

        Verify(RemoveResult.Refused(Workspace, $"{SourceLogicalPath.AgentsRoot}/{TopicName}", TargetKind,
            cancelled ? RemoveFindingCode.Interrupted : RemoveFindingCode.WorkspaceLockUnavailable,
            status, rawCause, lockFailure: failure), RemovePresentation.Rendering, failure,
            RemoveReportSelector.FindingCode(cancelled ? RemoveFindingCode.Interrupted : RemoveFindingCode.WorkspaceLockUnavailable), status);

        Verify(RepairTestData.Result([new RepairFinding(cancelled ? RepairFindingCode.Interrupted : RepairFindingCode.WorkspaceLockUnavailable, rawCause)
        { LockFailure = failure }]), RepairPresentation.Rendering, failure,
            RepairDefinitions.ReadMachineName(cancelled ? RepairFindingCode.Interrupted : RepairFindingCode.WorkspaceLockUnavailable), status);

        var cleanupFinding = CleanupFinding.Create(cancelled ? CleanupFindingCode.Interrupted : CleanupFindingCode.WorkspaceLockUnavailable, rawCause)
            with
        { LockFailure = failure };
        Verify(CleanupTestData.Result(CleanupTestData.Facts(effects: [], findings: [cleanupFinding]), status),
            CleanupPresentation.Rendering, failure, CleanupWording.FindingCode(cancelled ? CleanupFindingCode.Interrupted : CleanupFindingCode.WorkspaceLockUnavailable), status);

        Verify(RouteUpdateTestData.Result(RouteUpdateTestData.VerifiedNoOpFormation(findings:
            [new RouteUpdateFinding(cancelled ? RouteUpdateFindingCode.Interrupted : RouteUpdateFindingCode.WorkspaceLockUnavailable, rawCause)
            { LockFailure = failure }])), RouteUpdatePresentation.Rendering, failure,
            RouteUpdateDefinitions.ReadMachineName(cancelled ? RouteUpdateFindingCode.Interrupted : RouteUpdateFindingCode.WorkspaceLockUnavailable), status);

        var inclusionCode = cancelled ? RouteInclusionFindingCode.Interrupted : RouteInclusionFindingCode.WorkspaceLockUnavailable;
        var inclusion = new RouteInclusionOutcome
        {
            Status = status,
            Code = inclusionCode,
            Cause = rawCause,
            LockFailure = failure,
        };
        Verify(new RouteIncludeResult { Workspace = Workspace, Requested = TopicName, Outcome = inclusion },
            RouteIncludePresentation.Rendering, failure,
            RouteInclusionWireVocabulary.FindingCode(RouteIncludeReportSelector.FindingPrefix, inclusionCode), status);
        Verify(new RouteExcludeResult { Workspace = Workspace, Requested = TopicName, Outcome = inclusion },
            RouteExcludePresentation.Rendering, failure,
            RouteInclusionWireVocabulary.FindingCode(RouteExcludeReportSelector.FindingPrefix, inclusionCode), status);

        Verify(ExtensionInstallResult.Empty(Workspace, ExtensionInstallMode.Apply, false, true,
            new ExtensionInstallFinding(cancelled ? ExtensionInstallFindingCode.Interrupted : ExtensionInstallFindingCode.WorkspaceLockUnavailable, rawCause)
            { LockFailure = failure }), ExtensionInstallPresentation.Rendering, failure,
            ExtensionInstallDefinitions.ReadMachineName(cancelled ? ExtensionInstallFindingCode.Interrupted : ExtensionInstallFindingCode.WorkspaceLockUnavailable), status);

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
                    Status = status, Cause = rawCause, Path = null, LibraryId = LibraryMutationPlanningData.Id, LockFailure = failure,
                }],
            },
        }, LibrarySyncPresentation.Rendering, failure, LibrarySyncDefinitions.ReadFindingCode(cancelled ? LibrarySyncFindingCode.Interrupted : LibrarySyncFindingCode.LockUnavailable), status);
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n\n", _captures.Select(capture =>
            $"## {capture.Key}\n{capture.Value}")), $"commands-{failure.Reason}");
    }

    [Fact(DisplayName = "The six lock causes and history wording have reviewed exact messages")]
    public void CauseAndHistoryMessages()
    {
        WorkspaceLockFailureReason[] reasons =
        [
            WorkspaceLockFailureReason.Busy, WorkspaceLockFailureReason.AccessDenied,
            WorkspaceLockFailureReason.Unsupported, WorkspaceLockFailureReason.InvalidTarget,
            WorkspaceLockFailureReason.IoError, WorkspaceLockFailureReason.Cancelled,
        ];
        Assert.Equal(reasons, Enum.GetValues<WorkspaceLockFailureReason>());
        var messages = reasons.Select(reason =>
        {
            var failure = new WorkspaceLockFailureInfo(reason, WorkspaceRuntimePaths.LockPath(Workspace));
            return $"{reason}: {CliFindingWording.WorkspaceLockUnavailable(failure)}\nReason: {CliFindingWording.WorkspaceLockUnavailableReason(failure)}";
        });
        CommandOutputSnapshot.MatchSnapshot(Normalize(string.Join("\n", messages)), "causes");
        var legacyStore = TestDataHome.LegacyRecoveryWorkspaceDirectory(Root, Root);
        var history = string.Join("\n",
            CliFindingWording.RecoveryHistory(Path.Combine(WorkspaceRuntimePaths.RecoveryRoot(Workspace), BundleName)),
            CliFindingWording.RecoveryCoverageUnavailable(legacyStore, DeniedCause),
            CliFindingWording.RecoveryCoverageUnavailable(null, LegacyUnavailableCause));
        CommandOutputSnapshot.MatchSnapshot(Normalize(
            CommandOutputNormalization.ReplaceDelimitedPath(history, legacyStore, "<legacy-store>")), "history");
        Assert.Throws<ArgumentOutOfRangeException>(() => CliFindingWording.WorkspaceLockUnavailable(new((WorkspaceLockFailureReason)99, LockPath)));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockFailureProjection.From(new((WorkspaceLockFailureKind)99, LockPath)));
    }

    [Fact(DisplayName = "Recovery coverage adds one period after causes with or without terminal punctuation")]
    public void RecoveryCoveragePunctuation()
    {
        var captures = new List<string>();
        foreach (var punctuation in new[] { "", ".", "!", "?", ". " })
        {
            foreach (var path in new string?[] { null, WorkspaceRuntimePaths.LockPath(Workspace) })
            {
                captures.Add($"Punctuation [{punctuation}], path present {path is not null}: "
                    + CliFindingWording.RecoveryCoverageUnavailable(path, $"{HistoryUnavailableCause}{punctuation}"));
            }
        }
        CommandOutputSnapshot.MatchSnapshot(Normalize(string.Join("\n", captures)), "history-punctuation");
    }

    private void Verify<TResult, TData>(TResult result, CliReportRendering<TResult, TData> rendering,
        WorkspaceLockFailureInfo failure, string code, CliSemanticStatus status)
        where TResult : ICliCommandResult where TData : class
    {
        Assert.Equal(status, result.Status);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var report = rendering.Selector(result, new CliSelection(detail, null));
            var finding = Assert.Single(report.Findings);
            Assert.Equal(code, finding.Code);
            Assert.Equal(failure, finding.LockFailure);
            foreach (var format in new[] { CliFormat.Text, CliFormat.Json })
            {
                var output = CliRenderingStage.Render(new CliPresentationRequest<TResult>(result, new(format, detail, null)), rendering);
                Assert.Equal(status, output.Status);
                Assert.Equal(format == CliFormat.Json ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError, output.PrimaryTarget);
                Assert.Equal(status == CliSemanticStatus.Interrupted ? 130 : 5,
                    CliProcessCompletionPolicy.Complete(output.Status, output.PrimaryTarget).ExitCode);
                var projection = output.PrimaryContent;
                if (format == CliFormat.Json)
                {
                    using var json = JsonDocument.Parse(output.PrimaryContent);
                    var jsonFinding = Assert.Single(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
                    Assert.Equal(code, jsonFinding.GetProperty(CliJsonFields.Code).GetString());
                    projection = jsonFinding.GetRawText();
                }
                _captures.Add($"{result.Command} {detail} {format}", Normalize(projection, result.Workspace, finding.Subject.Path ?? finding.Subject.Id));
            }
        }
    }

    private static string Normalize(string output, CliWorkspace? workspace = null, string? subjectPath = null)
    {
        var lockRelative = Path.GetRelativePath(Root, LockPath).Replace('\\', '/');
        output = CommandOutputNormalization.ReplaceDelimitedPath(output, LockPath, $"{WorkspacePlaceholder}/{lockRelative}");
        var ordinaryLock = WorkspaceRuntimePaths.LockPath(Workspace);
        output = CommandOutputNormalization.ReplaceDelimitedPath(output, ordinaryLock,
            $"{WorkspacePlaceholder}/{Path.GetRelativePath(Root, ordinaryLock).Replace('\\', '/')}");
        var bundle = Path.Combine(WorkspaceRuntimePaths.RecoveryRoot(Workspace), BundleName);
        output = CommandOutputNormalization.ReplaceDelimitedPath(output, bundle,
            $"{WorkspacePlaceholder}/{Path.GetRelativePath(Root, bundle).Replace('\\', '/')}");
        if (workspace is not null)
        {
            if (subjectPath is not null && Path.IsPathFullyQualified(subjectPath))
            {
                var relativeSubject = Path.GetRelativePath(workspace.LexicalRoot, subjectPath).Replace('\\', '/');
                output = CommandOutputNormalization.ReplaceDelimitedPath(output, subjectPath, $"{WorkspacePlaceholder}/{relativeSubject}");
            }
            output = CommandOutputNormalization.ReplaceDelimitedPath(output, workspace.LexicalRoot, WorkspacePlaceholder);
        }
        return CommandOutputNormalization.ReplaceDelimitedPath(output, Root, WorkspacePlaceholder).Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}
