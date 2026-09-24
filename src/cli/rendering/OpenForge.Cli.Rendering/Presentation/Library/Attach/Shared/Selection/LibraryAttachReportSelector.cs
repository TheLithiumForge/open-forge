using System.Globalization;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Presentation.Library.Attach.Models;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Selection;

internal static class LibraryAttachReportSelector
{
    internal static CliReport<LibraryAttachData> Select(
        LibraryAttachResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var payload = result.Result;
        var progress = Progress(payload);
        var data = Data(result, selection.Detail, progress);
        return new CliReport<LibraryAttachData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, data, progress),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = payload.Findings.Select(finding => Finding(result, finding, progress)).ToArray(),
            Effects = Effects(result, selection.Detail),
            Counts = Counts(payload, progress),
            Limitations = [],
            Data = data,
            Recovery = Recovery(payload.Application.Recovery),
            Next = Next(result, progress),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data, progress)
                : [],
        };
    }

    private static LibraryAttachData Data(
        LibraryAttachResult result,
        CliDetail detail,
        LinkProgress progress)
    {
        var payload = result.Result;
        var identity = payload.Identity;
        var preview = identity.Mode == LibraryMode.DryRun;
        var includeStandard = detail >= CliDetail.Standard;
        var includeFull = detail >= CliDetail.Full;
        var links = payload.Plan.Links
            .Select(link => new LibraryAttachDataLink
            {
                Path = link.Path,
                Target = link.RawRelativeTarget,
            })
            .ToArray();
        var permissionSaved = PermissionSaved(payload.Permissions);
        var grantFolders = GrantFolders(payload, includePlanned: preview);
        var summaryLines = SummaryLines(result, detail, progress, preview, grantFolders);
        var detailLines = includeFull
            ? DetailLines(payload, progress)
            : [];
        var recovery = includeFull
            ? new LibraryAttachDataRecovery
            {
                State = State(payload.Application.Recovery.State),
                Path = payload.Application.Recovery.Path,
                Remaining = payload.Application.Residuals.Select(residual => new LibraryAttachDataRemaining
                {
                    Path = residual.Path,
                    Kind = State(residual.Kind),
                    State = State(residual.State),
                }).ToArray(),
            }
            : null;

        return new LibraryAttachData
        {
            Mode = State(identity.Mode),
            Id = identity.LibraryId ?? payload.Findings.FirstOrDefault()?.LibraryId,
            SourceFolder = identity.SourceRoot,
            DestinationFolder = identity.DestinationRoot,
            Recorded = payload.Application.RecordPublication.State == LibraryRecordPublicationState.Verified,
            Permissions = new LibraryAttachDataPermissions
            {
                Decision = payload.Permissions.Decision,
                Required = payload.Permissions.Required,
                Missing = payload.Permissions.Missing,
                Saved = permissionSaved,
            },
            Links = includeStandard ? links : null,
            Inventory = includeStandard
                ? new LibraryAttachDataInventory
                {
                    Eligible = payload.Source.EligiblePaths.Length,
                    Excluded = payload.Source.ExcludedPaths.Length,
                }
                : null,
            ExpectedStates = includeFull ? ExpectedStates(payload) : null,
            Verification = includeFull
                ? new LibraryAttachDataVerification
                {
                    State = State(payload.Application.Verification),
                    LinksCreated = progress.Created,
                    LinksExpected = progress.Total,
                    Recorded = payload.Application.RecordPublication.State == LibraryRecordPublicationState.Verified,
                }
                : null,
            Recovery = recovery,
            TextRows = TextRows(result, detail, progress, links),
            TextSummaryLines = summaryLines,
            TextDetailLines = detailLines,
            ShowNoChanges = preview && payload.Plan.State == LibraryPlanState.Complete,
        };
    }

    private static IReadOnlyList<LibraryAttachDataTextRow> TextRows(
        LibraryAttachResult result,
        CliDetail detail,
        LinkProgress progress,
        IReadOnlyList<LibraryAttachDataLink> links)
    {
        var showTargets = result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && result.Result.Plan.State == LibraryPlanState.Complete;
        var partial = result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        if (!showTargets && !partial)
        {
            return [];
        }

        if (!partial && detail < CliDetail.Standard)
        {
            return [];
        }

        if (partial)
        {
            return links.Select(link => new LibraryAttachDataTextRow(
                link.Path,
                progress.Retained.Contains(link.Path, StringComparer.Ordinal)
                    ? LibraryAttachWording.LinkCreated()
                    : progress.Unknown.Contains(link.Path, StringComparer.Ordinal)
                        ? LibraryAttachWording.LinkUnknown()
                        : LibraryAttachWording.LinkNotStarted())).ToArray();
        }

        return links.Select(link => new LibraryAttachDataTextRow(
            link.Path,
                LibraryAttachWording.LinkTarget(link.Target))).ToArray();
    }

    private static IReadOnlyList<string> SummaryLines(
        LibraryAttachResult result,
        CliDetail detail,
        LinkProgress progress,
        bool preview,
        IReadOnlyList<string> grantFolders)
    {
        var payload = result.Result;
        var partial = result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        var lines = new List<string>();
        if (partial)
        {
            if (payload.Application.RecordPublication.State != LibraryRecordPublicationState.Verified
                && payload.Application.Recovery.Path is { } recoveryPath)
            {
                lines.Add(LibraryAttachWording.NotRecordedRecovery(recoveryPath));
            }

            return lines;
        }

        if (result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete)
        {
            return [];
        }

        if (preview)
        {
            if (payload.Plan.Links.Length > 0)
            {
                lines.Add(LibraryAttachWording.WouldCreateLinks(
                    payload.Plan.Links.Length,
                    LinkFolder(payload)));
            }
        }
        else if (progress.Created > 0)
        {
            lines.Add(LibraryAttachWording.CreatedLinks(progress.Created, LinkFolder(payload)));
        }

        foreach (var region in payload.Plan.GeneratedRegions)
        {
            lines.Add(preview
                ? LibraryAttachWording.WouldUpdateEntries(region.Path)
                : LibraryAttachWording.UpdatedEntries(region.Path));
        }

        foreach (var folder in grantFolders)
        {
            lines.Add(preview
                ? LibraryAttachWording.WouldSaveGrant(folder)
                : LibraryAttachWording.SavedGrant(folder));
        }

        if (detail >= CliDetail.Standard)
        {
            lines.Add(LibraryAttachWording.Inventory(
                payload.Source.EligiblePaths.Length,
                payload.Source.ExcludedPaths.Length));
            lines.Add(preview
                ? LibraryAttachWording.WouldRecord()
                : LibraryAttachWording.LockRecorded());
        }

        return lines;
    }

    private static IReadOnlyList<string> DetailLines(
        LibraryAttachPayload payload,
        LinkProgress progress)
    {
        var lines = new List<string>
        {
            LibraryAttachWording.PermissionEvaluation(
                payload.Permissions.Decision,
                payload.Permissions.Required,
                payload.Permissions.Missing,
                PermissionSaved(payload.Permissions)),
        };
        foreach (var expected in ExpectedStates(payload))
        {
            lines.Add(LibraryAttachWording.ExpectedState(
                expected.Path,
                expected.Kind,
                expected.Length,
                expected.Sha256,
                expected.Target));
        }

        lines.Add(LibraryAttachWording.Verification(
            State(payload.Application.Verification),
            progress.Created,
            progress.Total,
            payload.Application.RecordPublication.State == LibraryRecordPublicationState.Verified));
        lines.Add(LibraryAttachWording.Recovery(
            State(payload.Application.Recovery.State),
            payload.Application.Recovery.Path,
            payload.Application.Residuals.Length));
        return lines;
    }

    private static IReadOnlyList<LibraryAttachDataExpectedState> ExpectedStates(LibraryAttachPayload payload)
    {
        var states = new List<LibraryAttachDataExpectedState>();
        states.AddRange(payload.Plan.Directories.Select(directory => Expected(directory.Path, directory.Expected)));
        states.AddRange(payload.Plan.Links.Select(link => Expected(link.Path, link.Expected)));
        states.AddRange(payload.Plan.GeneratedRegions.Select(region => Expected(region.Path, region.Expected)));
        if (payload.Plan.RecordExpected is { } record)
        {
            states.Add(Expected(payload.Record.Path, record));
        }

        return states;
    }

    private static LibraryAttachDataExpectedState Expected(string path, LibraryExpectedState expected)
        => new()
        {
            Path = path,
            Kind = State(expected.Kind),
            Length = expected.Length,
            Sha256 = expected.Sha256,
            Target = expected.RawRelativeTarget,
        };

    private static CliHeadline Headline(
        LibraryAttachResult result,
        LibraryAttachData data,
        LinkProgress progress)
    {
        var payload = result.Result;
        var id = data.Id ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        var source = data.SourceFolder ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        var first = payload.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? payload.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when payload.Identity.Mode == LibraryMode.DryRun
                => new(LibraryAttachWording.WouldRegister(id, source), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete when payload.Source.InventoryState == LibraryMutationInventoryState.Complete
                && payload.Source.EligiblePaths.Length == 0
                => new(LibraryAttachWording.RegisteredEmpty(id, source), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete
                => new(LibraryAttachWording.Registered(id, source), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(LibraryAttachWording.Registered(id, source), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(LibraryAttachWording.Incomplete(id, FindingMessage(result, first, progress)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(LibraryAttachWording.CannotAttach(id, FindingMessage(result, first, progress)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(LibraryAttachWording.CannotAttach(id, FindingMessage(result, first, progress)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(LibraryAttachWording.Failed(progress.Created, progress.Total), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted when progress.Started == 0
                => new(LibraryAttachWording.Cancelled(), CliHeadlineKind.Cancelled),
            CliSemanticStatus.Interrupted
                => new(LibraryAttachWording.CancelledAfter(progress.Created, progress.Total), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Library Attach status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(LibraryAttachResult result)
    {
        if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return null;
        }

        var finding = result.Result.Findings.FirstOrDefault(candidate => candidate.Status == result.Status);
        return finding is null ? null : LibraryAttachWording.MachineCode(finding.Code);
    }

    private static CliFinding Finding(
        LibraryAttachResult result,
        LibraryAttachFinding finding,
        LinkProgress progress)
    {
        var action = Action(result, finding, progress);
        return new CliFinding
        {
            Severity = Severity(result, finding),
            Code = LibraryAttachWording.MachineCode(finding.Code),
            Title = LibraryAttachWording.FindingTitle(finding.Code),
            Message = FindingMessage(result, finding, progress),
            Subject = Subject(result, finding),
            Resolution = action is null
                ? null
                : action.Kind == CliNextActionKind.Command
                    ? CliResolution.TargetedOperation
                    : CliResolution.GuidedChoice,
            Actions = action is null ? [] : [action],
        };
    }

    private static CliSeverity Severity(
        LibraryAttachResult result,
        LibraryAttachFinding finding)
        => result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && finding.Code == LibraryAttachFindingCode.OwnershipObservation
            && result.Result.Record.State == LibraryMutationRecordState.Invalid
            ? CliSeverity.Warning
            : CliReportVocabulary.Severity(finding.Status);

    private static CliSubject Subject(LibraryAttachResult result, LibraryAttachFinding finding)
    {
        var payload = result.Result;
        var id = finding.LibraryId ?? payload.Identity.LibraryId ?? result.Command;
        if (finding.Code is LibraryAttachFindingCode.InvalidInput
            or LibraryAttachFindingCode.InvalidId
            or LibraryAttachFindingCode.DuplicateId
            or LibraryAttachFindingCode.LibraryRemoved
            or LibraryAttachFindingCode.ConfirmationRequired)
        {
            return new CliSubject(CliSubjectKind.Identifier, Id: id);
        }

        if (finding.Code is LibraryAttachFindingCode.SourceRootInvalid
            or LibraryAttachFindingCode.SourceRootUnavailable
            or LibraryAttachFindingCode.SourceRootBlocked
            or LibraryAttachFindingCode.InventoryIncomplete)
        {
            return new CliSubject(CliSubjectKind.Source, Path: finding.Path ?? payload.Identity.SourceRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder());
        }

        if (finding.Code is LibraryAttachFindingCode.PermissionRequired
            or LibraryAttachFindingCode.PermissionDeclined
            or LibraryAttachFindingCode.PermissionInvalid
            or LibraryAttachFindingCode.PermissionUnavailable
            or LibraryAttachFindingCode.PermissionChanged
            or LibraryAttachFindingCode.PermissionWriteFailed)
        {
            return new CliSubject(CliSubjectKind.Directory, Path: PermissionFolder(payload, finding.Path));
        }

        if (finding.Code == LibraryAttachFindingCode.OwnershipObservation
            && payload.Record.State == LibraryMutationRecordState.Invalid)
        {
            return new CliSubject(CliSubjectKind.File, Path: finding.Path ?? payload.Record.Path);
        }

        if (finding.Code is LibraryAttachFindingCode.RecordInvalid
            or LibraryAttachFindingCode.RecordUnavailable
            or LibraryAttachFindingCode.RecordBlocked
            or LibraryAttachFindingCode.LockUnavailable)
        {
            return new CliSubject(CliSubjectKind.File, Path: finding.Path ?? payload.Record.Path);
        }

        return new CliSubject(
            CliSubjectKind.File,
            Path: finding.Path ?? payload.Identity.DestinationRoot ?? payload.Record.Path);
    }

    private static string FindingMessage(
        LibraryAttachResult result,
        LibraryAttachFinding? finding,
        LinkProgress progress)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageTheLibraryAttachResultDidNotContainAFinding();
        }

        var payload = result.Result;
        var id = finding.LibraryId ?? payload.Identity.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        var source = payload.Identity.SourceRoot ?? finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        return finding.Code switch
        {
            LibraryAttachFindingCode.InvalidInput
                => TrimSentence(finding.Cause),
            LibraryAttachFindingCode.InvalidId
                => LibraryAttachWording.InvalidId(finding.LibraryId),
            LibraryAttachFindingCode.DuplicateId
                => LibraryAttachWording.DuplicateId(id),
            LibraryAttachFindingCode.LibraryRemoved
                or LibraryAttachFindingCode.PathExcluded
                => TrimSentence(finding.Cause),
            LibraryAttachFindingCode.SourceRootInvalid
                => LibraryAttachWording.SourceRootInvalid(finding.Path ?? source),
            LibraryAttachFindingCode.SourceRootUnavailable
                => LibraryAttachWording.SourceRootUnavailable(finding.Path ?? source),
            LibraryAttachFindingCode.SourceRootBlocked
                => LibraryAttachWording.SourceRootBlocked(finding.Path ?? source),
            LibraryAttachFindingCode.DestinationRootInvalid
                => LibraryAttachWording.DestinationRootInvalid(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedValue()),
            LibraryAttachFindingCode.DestinationCollision
                => LibraryAttachWording.DestinationCollision(finding.Path ?? payload.Identity.DestinationRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath()),
            LibraryAttachFindingCode.InventoryIncomplete
                => LibraryAttachWording.InventoryIncomplete(source),
            LibraryAttachFindingCode.MappingUnavailable
                => LibraryAttachWording.MappingUnavailable(source, finding.Cause),
            LibraryAttachFindingCode.MappingBlocked
                => LibraryAttachWording.MappingBlocked(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath(), finding.Cause),
            LibraryAttachFindingCode.LinkCapabilityUnavailable
                => global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.MessageThisSystemCannotCreateTheFileLinksLibrariesNeed(),
            LibraryAttachFindingCode.ConsumerBlocked
                => LibraryAttachWording.ConsumerBlocked(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath()),
            LibraryAttachFindingCode.OwnershipConflict
                => LibraryAttachWording.OwnershipConflict(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath(), id),
            LibraryAttachFindingCode.OwnershipObservation
                => payload.Record.State == LibraryMutationRecordState.Invalid
                    ? finding.Cause
                    : LibraryAttachWording.OwnershipObservation(id),
            LibraryAttachFindingCode.RecordInvalid
                => LibraryAttachWording.RecordInvalid(finding.Cause),
            LibraryAttachFindingCode.RecordUnavailable
                => CliFindingWording.LifecycleUnavailable(),
            LibraryAttachFindingCode.RecordBlocked
                => CliFindingWording.LifecycleBlocked(TrimSentence(finding.Cause)),
            LibraryAttachFindingCode.PermissionRequired
                => LibraryAttachWording.PermissionRequired(PermissionFolder(payload, finding.Path)),
            LibraryAttachFindingCode.PermissionDeclined
                => CliFindingWording.PermissionDeclined(),
            LibraryAttachFindingCode.PermissionInvalid
                => CliFindingWording.PermissionsInvalid(TrimSentence(finding.Cause)),
            LibraryAttachFindingCode.PermissionUnavailable
                => CliFindingWording.PermissionsUnavailable(),
            LibraryAttachFindingCode.PermissionChanged
                => CliFindingWording.PermissionsChanged(),
            LibraryAttachFindingCode.PermissionWriteFailed
                => CliFindingWording.PermissionWriteFailed(),
            LibraryAttachFindingCode.GeneratedNavigationBlocked
                => CliFindingWording.GeneratedRegionUnsafe(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheEntriesFile(), TrimSentence(finding.Cause)),
            LibraryAttachFindingCode.GeneratedNavigationIncomplete
                => CliFindingWording.ProjectionUnavailable(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheEntriesFile(), TrimSentence(finding.Cause)),
            LibraryAttachFindingCode.LockUnavailable
                => CliFindingWording.WorkspaceLockUnavailable(),
            LibraryAttachFindingCode.RecoveryUnavailable
                => CliFindingWording.RecoveryUnavailable(finding.Path ?? payload.Application.Recovery.Path ?? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheRecoveryStore()),
            LibraryAttachFindingCode.RecoveryRetained
                => CliFindingWording.RecoveryRetained(finding.Path ?? payload.Application.Recovery.Path ?? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheRecoveryBundle()),
            LibraryAttachFindingCode.ApplicationFailed
                => LibraryAttachWording.ApplicationFailed(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath(), progress.Created, progress.Total),
            LibraryAttachFindingCode.VerificationFailed
                => CliFindingWording.VerificationFailed(finding.Path ?? payload.Record.Path,
                    payload.Application.Recovery.Path ?? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelTheRecoveryStore()),
            LibraryAttachFindingCode.OperationFailed
                => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLibraryAttach(), TrimSentence(finding.Cause)),
            LibraryAttachFindingCode.Interrupted
                => progress.Started == 0
                    ? LibraryAttachWording.Cancelled()
                    : LibraryAttachWording.CancelledAfter(progress.Created, progress.Total),
            LibraryAttachFindingCode.ConfirmationRequired
                => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.TitleLibraryAttach()),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Library Attach finding code is not defined."),
        };
    }

    private static CliNextAction? Action(
        LibraryAttachResult result,
        LibraryAttachFinding finding,
        LinkProgress progress)
    {
        var payload = result.Result;
        var id = finding.LibraryId ?? payload.Identity.LibraryId ?? "the supplied ID";
        return finding.Code switch
        {
            LibraryAttachFindingCode.DuplicateId
                => new CliNextAction($"open-forge library inspect {id}", LibraryAttachWording.DuplicateNextReason()),
            LibraryAttachFindingCode.MappingUnavailable
                or LibraryAttachFindingCode.RecordInvalid
                or LibraryAttachFindingCode.GeneratedNavigationIncomplete
                or LibraryAttachFindingCode.RecoveryUnavailable
                => new CliNextAction("open-forge doctor", LibraryAttachWording.DoctorNextReason()),
            LibraryAttachFindingCode.PermissionRequired
                => new CliNextAction(PermissionCommand(payload), LibraryAttachWording.PermissionNextReason()),
            LibraryAttachFindingCode.DestinationCollision
                => new CliNextAction(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelChooseAnotherToFolder(), LibraryAttachWording.CollisionNextReason())
                {
                    Kind = CliNextActionKind.Sentence,
                },
            LibraryAttachFindingCode.RecoveryRetained
                => new CliNextAction("open-forge cleanup", LibraryAttachWording.CleanupNextReason()),
            LibraryAttachFindingCode.Interrupted when progress.Started > 0
                => new CliNextAction("open-forge doctor", LibraryAttachWording.DoctorNextReason()),
            _ => null,
        };
    }

    private static CliNextAction? Next(LibraryAttachResult result, LinkProgress progress)
    {
        if (result.Next is { } next)
        {
            return next;
        }

        var first = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();
        if (first?.Code == LibraryAttachFindingCode.LibraryRemoved)
        {
            return new CliNextAction(
                LibraryAttachWording.RemoveExcludedLibraryNext(first.LibraryId ?? result.Result.Identity.LibraryId ?? "the supplied ID"),
                LibraryAttachWording.WorkspaceSettingsNextReason())
            {
                Kind = CliNextActionKind.Sentence,
            };
        }
        return first is null ? null : Action(result, first, progress);
    }

    private static IReadOnlyList<CliEffect> Effects(LibraryAttachResult result, CliDetail detail)
    {
        var payload = result.Result;
        var effects = new List<CliEffect>();
        effects.AddRange(payload.Plan.Directories.Select(directory => Effect(
            result,
            directory.Path,
            CliEffectKind.Directory,
            CliEffectAction.Created,
            directory.Expected,
            detail)));
        effects.AddRange(payload.Plan.Links.Select(link => Effect(
            result,
            link.Path,
            CliEffectKind.Link,
            CliEffectAction.Created,
            link.Expected,
            detail)));
        effects.AddRange(payload.Plan.GeneratedRegions.Select(region => Effect(
            result,
            region.Path,
            CliEffectKind.Section,
            CliEffectAction.Rewritten,
            region.Expected,
            detail)));
        if (payload.Plan.RecordEffect != LibraryRecordEffect.None && payload.Plan.RecordExpected is { } record)
        {
            effects.Add(Effect(
                result,
                payload.Record.Path,
                CliEffectKind.Record,
                payload.Plan.RecordEffect switch
                {
                    LibraryRecordEffect.Create => CliEffectAction.Created,
                    LibraryRecordEffect.Replace => CliEffectAction.Replaced,
                    LibraryRecordEffect.Delete => CliEffectAction.Deleted,
                    _ => throw new ArgumentOutOfRangeException(nameof(result), payload.Plan.RecordEffect, "The Library record effect is not defined."),
                },
                record,
                detail));
        }

        var permissions = payload.Permissions;
        if (payload.Plan.SettingsChange is { } settingsChange)
        {
            effects.Add(new CliEffect
            {
                Path = settingsChange.Path,
                Kind = CliEffectKind.Setting,
                Action = settingsChange.Action == "replace" ? CliEffectAction.Rewritten : CliEffectAction.Created,
                Outcome = PermissionOutcome(result, permissions),
            });
        }

        return effects;
    }

    private static CliEffect Effect(
        LibraryAttachResult result,
        string path,
        CliEffectKind kind,
        CliEffectAction action,
        LibraryExpectedState expected,
        CliDetail detail)
    {
        var progress = Progress(result.Result);
        var residual = result.Result.Application.Residuals.FirstOrDefault(candidate =>
            candidate.Kind == Kind(kind) && string.Equals(candidate.Path, path, StringComparison.Ordinal));
        var outcome = result.Result.Identity.Mode == LibraryMode.DryRun
            ? result.Result.Plan.State == LibraryPlanState.Complete
                ? CliEffectOutcome.Planned
                : CliEffectOutcome.NotStarted
            : residual is { State: LibraryResidualState.Retained }
                ? CliEffectOutcome.Done
                : residual is { State: LibraryResidualState.Unknown }
                    ? CliEffectOutcome.Unknown
                    : result.Result.Application.State is LibraryApplicationState.Applied or LibraryApplicationState.NoOp
                        && result.Result.Application.Verification == LibraryVerificationState.Verified
                            ? CliEffectOutcome.Done
                            : result.Status is CliSemanticStatus.Failed
                                && result.Result.Application.Verification == LibraryVerificationState.Failed
                                    ? CliEffectOutcome.Failed
                                    : CliEffectOutcome.NotStarted;
        return new CliEffect
        {
            Path = path,
            Kind = kind,
            Action = action,
            Outcome = outcome,
            After = detail >= CliDetail.Full ? expected.Sha256 ?? expected.RawRelativeTarget : null,
            Reason = outcome == CliEffectOutcome.Unknown ? global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.MessageTheFinalStateCouldNotBeVerified() : null,
        };
    }

    private static CliEffectOutcome PermissionOutcome(
        LibraryAttachResult result,
        LibraryPermissionView permissions)
        => PermissionSaved(permissions)
            ? CliEffectOutcome.Done
            : result.Result.Identity.Mode == LibraryMode.DryRun && permissions.Action is not ("none" or "")
                ? CliEffectOutcome.Planned
                : CliEffectOutcome.NotStarted;

    private static IReadOnlyList<CliCount> Counts(
        LibraryAttachPayload payload,
        LinkProgress progress)
        =>
        [
            new CliCount("linksCreated", global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.LabelLinksCreated(), progress.Created),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), CountSections(payload)),
            new CliCount("grantsSaved", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelGrantsSaved(), PermissionSaved(payload.Permissions) ? GrantFolders(payload, includePlanned: false).Count : 0),
            new CliCount("sourceFiles", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelSourceFiles(), payload.Source.EligiblePaths.Length),
        ];

    private static int CountSections(LibraryAttachPayload payload)
        => payload.Identity.Mode == LibraryMode.DryRun
            ? 0
            : payload.Application.State is LibraryApplicationState.Applied or LibraryApplicationState.NoOp
                && payload.Application.Verification == LibraryVerificationState.Verified
                    ? payload.Plan.GeneratedRegions.Length
                    : payload.Application.Residuals.Count(residual => residual.Kind == LibraryResidualKind.GeneratedRegion
                        && residual.State == LibraryResidualState.Retained);

    private static CliRecovery Recovery(LibraryRecoveryView recovery)
        => new(
            recovery.Path,
            recovery.State switch
            {
                LibraryRecoveryState.NotRequested => CliRecoveryDisposition.NotRequired,
                LibraryRecoveryState.Prepared or LibraryRecoveryState.Retained => CliRecoveryDisposition.Retained,
                LibraryRecoveryState.Removed => CliRecoveryDisposition.Removed,
                LibraryRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(recovery), recovery.State, "The Library recovery state is not defined."),
            });

    private static IReadOnlyList<string> Diagnostics(
        LibraryAttachResult result,
        LibraryAttachData data,
        LinkProgress progress)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"library={data.Id ?? "unavailable"}",
            $"source-files={progress.SourceFiles.ToString(CultureInfo.InvariantCulture)}",
            $"links={progress.Total.ToString(CultureInfo.InvariantCulture)}",
            $"created={progress.Created.ToString(CultureInfo.InvariantCulture)}",
            $"sections={CountSections(result.Result).ToString(CultureInfo.InvariantCulture)}",
            $"grants={data.Permissions.Saved}",
            $"recorded={data.Recorded}",
            $"recovery={HumanState(result.Result.Application.Recovery.State)}",
            $"verification={HumanState(result.Result.Application.Verification)}",
            $"findings={result.Result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
        }.Concat(result.Result.Findings.Select(finding => finding.Cause)).ToArray();

    private static LinkProgress Progress(LibraryAttachPayload payload)
    {
        var total = payload.Plan.Links.Length;
        var residuals = payload.Application.Residuals
            .Where(residual => residual.Kind == LibraryResidualKind.Link)
            .ToArray();
        var retained = residuals
            .Where(residual => residual.State == LibraryResidualState.Retained)
            .Select(residual => residual.Path)
            .ToHashSet(StringComparer.Ordinal);
        var unknown = residuals
            .Where(residual => residual.State == LibraryResidualState.Unknown)
            .Select(residual => residual.Path)
            .ToHashSet(StringComparer.Ordinal);
        var allVerified = payload.Identity.Mode != LibraryMode.DryRun
            && payload.Application.State is LibraryApplicationState.Applied or LibraryApplicationState.NoOp
            && payload.Application.Verification == LibraryVerificationState.Verified;
        var created = allVerified ? total : retained.Count;
        var started = allVerified ? total : retained.Count + unknown.Count;
        return new LinkProgress(total, created, started, payload.Source.EligiblePaths.Length, retained, unknown);
    }

    private static bool PermissionSaved(LibraryPermissionView permissions)
        => permissions.Action is "create" or "replace"
            && permissions.Outcome == "verified";

    private static IReadOnlyList<string> GrantFolders(
        LibraryAttachPayload payload,
        bool includePlanned)
    {
        var permissions = payload.Permissions;
        if ((!includePlanned && !PermissionSaved(permissions))
            || permissions.Action is "none" or "")
        {
            return [];
        }

        var approved = permissions.ApprovedScopes
            .Select(scope => scope.Path)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (approved.Length > 0)
        {
            return approved;
        }

        return payload.Plan.Links
            .Select(link => link.Path.Replace('\\', '/'))
            .Where(path => !path.StartsWith(".agents/", StringComparison.Ordinal))
            .Select(path => path.LastIndexOf('/') > 0 ? path[..path.LastIndexOf('/')] : ".")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string PermissionFolder(
        LibraryAttachPayload payload,
        string? findingPath = null)
    {
        var scope = payload.Permissions.Required.FirstOrDefault()
            ?? payload.Permissions.Missing.FirstOrDefault()
            ?? payload.Permissions.ProposedScopes.FirstOrDefault()?.Path;
        if (scope is null)
        {
            scope = findingPath;
        }

        if (scope is null || scope is ".agents/open-forge.json" or ".agents/open-forge.lock.json")
        {
            return payload.Identity.DestinationRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationFolder();
        }

        scope = scope.Replace('\\', '/');
        var slash = scope.LastIndexOf('/');
        return slash > 0 ? scope[..slash] : scope;
    }

    private static string PermissionCommand(LibraryAttachPayload payload)
    {
        var id = payload.Identity.LibraryId ?? "<library-id>";
        var source = payload.Identity.SourceRoot ?? "<source-root>";
        var destination = payload.Identity.DestinationRoot ?? ".";
        return $"open-forge library attach {id} {source} --to {destination} --allow-path {PermissionFolder(payload)}";
    }

    private static string LinkFolder(LibraryAttachPayload payload)
    {
        var destination = payload.Identity.DestinationRoot;
        if (!string.IsNullOrWhiteSpace(destination) && destination != ".")
        {
            return destination;
        }

        var path = payload.Plan.Links.FirstOrDefault()?.Path.Replace('\\', '/');
        if (path is null)
        {
            return destination ?? ".";
        }

        var slash = path.LastIndexOf('/');
        return slash > 0 ? path[..slash] : ".";
    }

    private static LibraryResidualKind Kind(CliEffectKind kind) => kind switch
    {
        CliEffectKind.Directory => LibraryResidualKind.Directory,
        CliEffectKind.Link => LibraryResidualKind.Link,
        CliEffectKind.Section => LibraryResidualKind.GeneratedRegion,
        CliEffectKind.Record => LibraryResidualKind.Record,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library effect kind is not a mutation receipt kind."),
    };

    private static string State<T>(T value) where T : struct, Enum
        => CliReportVocabulary.Name(value);

    private static string HumanState<T>(T value) where T : struct, Enum
        => State(value) switch
        {
            "not-requested" => "not run",
            "not-started" => "not started",
            _ => State(value).Replace('-', ' '),
        };

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');

    private sealed record LinkProgress(
        int Total,
        int Created,
        int Started,
        int SourceFiles,
        IReadOnlySet<string> Retained,
        IReadOnlySet<string> Unknown);
}
