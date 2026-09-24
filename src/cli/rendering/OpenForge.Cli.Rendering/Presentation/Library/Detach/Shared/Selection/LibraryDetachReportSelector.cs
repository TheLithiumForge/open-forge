using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Selection;

internal static class LibraryDetachReportSelector
{
    internal static CliReport<LibraryDetachData> Select(
        LibraryDetachResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var id = result.Result.Identity.LibraryId
            ?? result.Result.Findings.FirstOrDefault(finding => finding.LibraryId is not null)?.LibraryId;
        var effects = ProjectEffects(result, selection.Detail);
        var data = new LibraryDetachData
        {
            Mode = LibraryDetachWording.Mode(result.Result.Identity.Mode),
            Id = id,
            SourceFolder = result.Result.Identity.SourceRoot,
            DestinationFolder = result.Result.Identity.DestinationRoot,
            RegistrationRemoved = RegistrationRemoved(result),
            Permissions = ProjectPermissions(result.Result.Permissions),
            Effects = effects.Select(effect => effect.Data).ToArray(),
            ExpectedStates = selection.Detail >= CliDetail.Full
                ? ExpectedStates(result.Result.Plan)
                : null,
            Verification = selection.Detail >= CliDetail.Full
                ? new LibraryDetachDataVerification
                {
                    State = LibraryDetachWording.VerificationState(result.Result.Application.Verification),
                    Registration = LibraryDetachWording.WireState(result.Result.Application.RecordPublication.State),
                }
                : null,
            Recovery = selection.Detail >= CliDetail.Full
                ? ProjectRecovery(result.Result.Application)
                : null,
            TextRows = TextRows(result, effects, selection.Detail),
            TextDetails = TextDetails(result, effects, selection.Detail),
        };

        return new CliReport<LibraryDetachData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, id ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId(), effects),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Result.Findings.Select(finding => Finding(result, finding, id)).ToArray(),
            Effects = effects.Select(effect => effect.Effect).ToArray(),
            Counts =
            [
                new CliCount("linksRemoved", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksRemoved(), effects.Count(effect => effect.Data.Kind == "link" && effect.Outcome is EffectOutcome.Done or EffectOutcome.Planned)),
                new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionsUpdated(), effects.Count(effect => effect.Data.Kind == "section" && effect.Outcome is EffectOutcome.Done or EffectOutcome.Planned)),
            ],
            Data = data,
            Recovery = Recovery(result.Result.Application),
            Next = Next(result, id),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data, effects)
                : [],
        };
    }

    private static CliHeadline Headline(
        LibraryDetachResult result,
        string id,
        IReadOnlyList<ProjectedEffect> effects)
    {
        var finding = PrimaryFinding(result);
        var linkCount = effects.Count(effect => effect.Data.Kind == "link");
        var destination = result.Result.Identity.DestinationRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationFolder();
        var source = result.Result.Identity.SourceRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        if (result.Status == CliSemanticStatus.Complete
            && result.Result.Plan.SettingsChange is null
            && result.Result.Findings.Any(finding => finding.Code == LibraryDetachFindingCode.OwnershipObservation))
        {
            return new(LibraryDetachWording.NoOwnership(id), CliHeadlineKind.Done);
        }

        if (result.Status == CliSemanticStatus.Complete && result.Result.Identity.Mode == LibraryMode.DryRun)
        {
            if (linkCount == 0 && result.Result.Plan.SettingsChange is not null)
            {
                return new(LibraryDetachWording.WouldRecordRemoval(id), CliHeadlineKind.Preview);
            }
            return new(LibraryDetachWording.WouldDetach(id, linkCount, destination), CliHeadlineKind.Preview);
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Result.Identity.Mode == LibraryMode.DryRun)
        {
            return new(LibraryDetachWording.WouldDetach(id, linkCount, destination), CliHeadlineKind.Warnings);
        }

        if (result.Status == CliSemanticStatus.Complete && linkCount == 0 && result.Result.Plan.SettingsChange is null)
        {
            return new(LibraryDetachWording.DetachedNoLinks(id), CliHeadlineKind.NothingToDo);
        }

        if (result.Status == CliSemanticStatus.Complete && linkCount == 0 && result.Result.Plan.SettingsChange is not null)
        {
            return new(LibraryDetachWording.RemovalRecorded(id), CliHeadlineKind.Done);
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => new(LibraryDetachWording.Detached(id, linkCount, destination, source), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(LibraryDetachWording.Detached(id, linkCount, destination, source), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(
                LibraryDetachWording.Incomplete(id, finding is null ? global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelTheDetachCouldNotBeChecked() : Message(result, finding)),
                CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(
                LibraryDetachWording.Cannot(id, finding is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRequestIsInvalid() : Problem(result, finding)),
                CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(
                LibraryDetachWording.Blocked(id, BlockedReason(result, finding)),
                CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(
                LibraryDetachWording.Failed(CompletedLinks(effects), linkCount),
                CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(LibraryDetachWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Library Detach status is not defined."),
        };
    }

    private static string BlockedReason(LibraryDetachResult result, LibraryDetachFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelTheDetachIsBlocked();
        }

        return finding.Code switch
        {
            LibraryDetachFindingCode.PermissionRequired => LibraryDetachWording.PermissionBlockedReason(),
            LibraryDetachFindingCode.LockUnavailable => LibraryDetachWording.LockUnavailableReason(),
            _ => Message(result, finding),
        };
    }

    private static string? HeadlineFindingCode(LibraryDetachResult result)
    {
        if (result.Status == CliSemanticStatus.Attention)
        {
            return null;
        }

        var matching = result.Result.Findings.Where(finding => finding.Status == result.Status).ToArray();
        return matching.Length == 1 ? LibraryDetachWording.FindingCode(matching[0].Code) : null;
    }

    private static CliFinding Finding(
        LibraryDetachResult result,
        LibraryDetachFinding finding,
        string? fallbackId)
    {
        var path = DisplayPath(result, finding);
        var id = finding.LibraryId ?? fallbackId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        var action = Action(result, finding, id, path);
        var kind = path is null ? CliSubjectKind.Identifier : CliSubjectKind.File;
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = LibraryDetachWording.FindingCode(finding.Code),
            Title = LibraryDetachWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = kind == CliSubjectKind.Identifier
                ? new CliSubject(kind, Id: id)
                : new CliSubject(kind, path),
            Resolution = action is null ? null : CliResolution.TargetedOperation,
            Actions = action is null ? [] : [action],
        };
    }

    private static string Message(LibraryDetachResult result, LibraryDetachFinding finding)
    {
        var path = DisplayPath(result, finding) ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheRegisteredLink();
        var id = finding.LibraryId
            ?? result.Result.Identity.LibraryId
            ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        return finding.Code switch
        {
            LibraryDetachFindingCode.InvalidInput => LibraryDetachWording.InvalidInput(id, finding.Cause),
            LibraryDetachFindingCode.InvalidId => LibraryDetachWording.InvalidId(id),
            LibraryDetachFindingCode.UnknownId => LibraryDetachWording.UnknownId(id),
            LibraryDetachFindingCode.OwnershipObservation => LibraryDetachWording.OwnershipObservation(id),
            LibraryDetachFindingCode.RecordInvalid => LibraryDetachWording.RecordInvalid(finding.Cause),
            LibraryDetachFindingCode.RecordUnavailable => LibraryDetachWording.RecordUnavailable(),
            LibraryDetachFindingCode.RecordBlocked => LibraryDetachWording.RecordBlocked(finding.Cause),
            LibraryDetachFindingCode.RegisteredLinkMissing => LibraryDetachWording.RegisteredLinkMissing(path),
            LibraryDetachFindingCode.MappingBlocked => MappingBlockedMessage(path, finding),
            LibraryDetachFindingCode.DestinationProtected => DestinationProtectedMessage(path, finding),
            LibraryDetachFindingCode.MappingUnavailable => LibraryDetachWording.MappingUnavailable(path),
            LibraryDetachFindingCode.LinkCapabilityUnavailable => LibraryDetachWording.LinkCapabilityUnavailable(),
            LibraryDetachFindingCode.ConsumerBlocked => LibraryDetachWording.ConsumerBlocked(path),
            LibraryDetachFindingCode.OwnershipConflict => LibraryDetachWording.OwnershipConflict(path),
            LibraryDetachFindingCode.PermissionRequired => LibraryDetachWording.PermissionRequired(),
            LibraryDetachFindingCode.PermissionDeclined => LibraryDetachWording.PermissionDeclined(),
            LibraryDetachFindingCode.PermissionInvalid => LibraryDetachWording.PermissionInvalid(finding.Cause),
            LibraryDetachFindingCode.PermissionUnavailable => LibraryDetachWording.PermissionUnavailable(),
            LibraryDetachFindingCode.PermissionChanged => LibraryDetachWording.PermissionChanged(),
            LibraryDetachFindingCode.PermissionWriteFailed => LibraryDetachWording.PermissionWriteFailed(),
            LibraryDetachFindingCode.GeneratedNavigationBlocked => LibraryDetachWording.GeneratedNavigationBlocked(path, finding.Cause),
            LibraryDetachFindingCode.GeneratedNavigationIncomplete => LibraryDetachWording.GeneratedNavigationIncomplete(path, finding.Cause),
            LibraryDetachFindingCode.LockUnavailable => LibraryDetachWording.LockUnavailable(),
            LibraryDetachFindingCode.RecoveryUnavailable => LibraryDetachWording.RecoveryUnavailable(path),
            LibraryDetachFindingCode.RecoveryRetained => LibraryDetachWording.RecoveryRetained(path),
            LibraryDetachFindingCode.ApplicationFailed => LibraryDetachWording.ApplicationFailed(finding.Cause),
            LibraryDetachFindingCode.VerificationFailed => LibraryDetachWording.VerificationFailed(path, result.Result.Application.Recovery.Path, finding.Cause),
            LibraryDetachFindingCode.OperationFailed => LibraryDetachWording.OperationFailed(finding.Cause),
            LibraryDetachFindingCode.Interrupted => LibraryDetachWording.Interrupted(),
            LibraryDetachFindingCode.ConfirmationRequired => LibraryDetachWording.ConfirmationRequired(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Library Detach finding code is not defined."),
        };
    }

    private static string Problem(LibraryDetachResult result, LibraryDetachFinding finding)
        => finding.Code switch
        {
            LibraryDetachFindingCode.InvalidId => LibraryDetachWording.InvalidId(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedValue()),
            LibraryDetachFindingCode.UnknownId => LibraryDetachWording.UnknownId(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId()),
            LibraryDetachFindingCode.InvalidInput => Sentence(finding.Cause),
            _ => Message(result, finding),
        };

    private static CliNextAction? Action(
        LibraryDetachResult result,
        LibraryDetachFinding finding,
        string id,
        string? path)
        => finding.Code switch
        {
            LibraryDetachFindingCode.InvalidId or LibraryDetachFindingCode.UnknownId
                => new CliNextAction("open-forge library list", LibraryDetachWording.ListNextReason()),
            LibraryDetachFindingCode.RegisteredLinkMissing
                or LibraryDetachFindingCode.MappingBlocked when finding.LibraryId is not null
                => new CliNextAction(global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatOpenForgeLibraryInspect($"{id}"), LibraryDetachWording.InspectNextReason()),
            LibraryDetachFindingCode.DestinationProtected
                => new CliNextAction("open-forge library list", LibraryDetachWording.ListNextReason()),
            LibraryDetachFindingCode.RecordInvalid
                or LibraryDetachFindingCode.RecordUnavailable
                or LibraryDetachFindingCode.RecordBlocked
                or LibraryDetachFindingCode.MappingUnavailable
                or LibraryDetachFindingCode.GeneratedNavigationIncomplete
                or LibraryDetachFindingCode.RecoveryUnavailable
                => new CliNextAction("open-forge doctor", LibraryDetachWording.DoctorNextReason()),
            LibraryDetachFindingCode.PermissionRequired when result.Result.Permissions.Missing.FirstOrDefault() is { } missing
                => new CliNextAction(
                    $"open-forge library detach {id} --allow-path {missing} --automatic",
                    global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageGrantTheDestinationPathThenRerunTheDetachRequest()),
            LibraryDetachFindingCode.RecoveryRetained
                => new CliNextAction("open-forge cleanup", LibraryDetachWording.RecoveryNextReason()),
            LibraryDetachFindingCode.ConfirmationRequired
                => new CliNextAction("open-forge library detach --automatic", global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.MessageRerunTheSameLibraryDetachRequestWithExplicitAutomaticMode()),
            LibraryDetachFindingCode.Interrupted
                => new CliNextAction($"open-forge library detach {id}", LibraryDetachWording.RetryNextReason()),
            LibraryDetachFindingCode.OperationFailed
                or LibraryDetachFindingCode.ApplicationFailed
                or LibraryDetachFindingCode.VerificationFailed
                => new CliNextAction(global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachPhrases.FormatOpenForgeLibraryDetachDetailDebug($"{id}"), LibraryDetachWording.FailedNextReason()),
            LibraryDetachFindingCode.LockUnavailable
                => new CliNextAction("open-forge doctor", LibraryDetachWording.DoctorNextReason()),
            _ => null,
        };

    private static CliNextAction? Next(LibraryDetachResult result, string? id)
    {
        if (result.Next is not null)
        {
            return result.Next;
        }

        var finding = PrimaryFinding(result);
        if (finding is null)
        {
            return null;
        }

        var action = Action(result, finding, id ?? finding.LibraryId ?? "the supplied ID", DisplayPath(result, finding));
        return action;
    }

    private static LibraryDetachFinding? PrimaryFinding(LibraryDetachResult result)
        => result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();

    private static string? DisplayPath(LibraryDetachResult result, LibraryDetachFinding finding)
    {
        if (finding.Path is null)
        {
            return null;
        }

        if (finding.Code is LibraryDetachFindingCode.RegisteredLinkMissing
            or LibraryDetachFindingCode.MappingBlocked
            or LibraryDetachFindingCode.MappingUnavailable
            or LibraryDetachFindingCode.OwnershipConflict)
        {
            return result.Result.Projection.Mappings
                .FirstOrDefault(mapping => string.Equals(mapping.SourcePath, finding.Path, StringComparison.Ordinal))
                ?.DestinationPath ?? finding.Path;
        }

        return finding.Path;
    }

    private static string MappingBlockedMessage(string path, LibraryDetachFinding finding)
    {
        if (finding.Status == CliSemanticStatus.Attention
            && finding.OccupantKind == LibraryDetachOccupantKind.OrdinaryFile)
        {
            return LibraryDetachWording.RetainedDestination(path);
        }

        return finding.OccupantKind switch
        {
            LibraryDetachOccupantKind.OrdinaryFile
                => LibraryDetachWording.MappingBlocked(path, global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelAnOrdinaryFile()),
            LibraryDetachOccupantKind.Folder
                => LibraryDetachWording.MappingBlocked(path, global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelAFolder()),
            LibraryDetachOccupantKind.DifferentLink
                => LibraryDetachWording.MappingBlocked(path, global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelADifferentLink()),
            _ => LibraryDetachWording.MappingBlockedCause(finding.Cause),
        };
    }

    private static string DestinationProtectedMessage(string path, LibraryDetachFinding finding)
        => finding.Path is null
            ? LibraryDetachWording.MappingBlockedCause(finding.Cause)
            : LibraryDetachWording.DestinationProtected(path);

    private static LibraryDetachDataPermissions ProjectPermissions(LibraryPermissionView permissions)
        => new()
        {
            Required = [.. permissions.Required],
            Missing = [.. permissions.Missing],
            Decision = permissions.Decision,
            Action = permissions.Action,
            Outcome = permissions.Outcome,
        };

    private static IReadOnlyList<ProjectedEffect> ProjectEffects(
        LibraryDetachResult result,
        CliDetail detail)
    {
        var plan = result.Result.Plan;
        var planned = new List<PlannedEffect>();
        if (plan.SettingsChange is { } settingsChange)
        {
            planned.Add(new PlannedEffect(
                settingsChange.Path,
                "setting",
                settingsChange.Action,
                null,
                settingsChange.Expected));
        }
        planned.AddRange(plan.Links.Select(link => new PlannedEffect(
            link.Path,
            "link",
            "delete",
            link.RawRelativeTarget,
            link.Expected)));
        planned.AddRange(plan.GeneratedRegions.Select(region => new PlannedEffect(
            region.Path,
            "section",
            "rewrite",
            null,
            region.Expected)));
        if (plan.RecordEffect != LibraryRecordEffect.None && plan.RecordExpected is { } recordExpected)
        {
            planned.Add(new PlannedEffect(
                ".agents/open-forge.lock.json",
                "record",
                LibraryDetachWording.WireState(plan.RecordEffect),
                null,
                recordExpected));
        }

        var lastObserved = LastObservedIndex(planned, result.Result.Application.Residuals);
        var projected = new List<ProjectedEffect>(planned.Count);
        for (var index = 0; index < planned.Count; index++)
        {
            var effect = planned[index];
            var outcome = Outcome(result, effect.Path, index, lastObserved);
            var dryRun = result.Result.Identity.Mode == LibraryMode.DryRun;
            var text = effect.Kind switch
            {
                "link" => LinkText(outcome, dryRun),
                "section" => SectionText(effect.Path, outcome, dryRun),
                "record" => RecordText(outcome, dryRun),
                "setting" => SettingsText(outcome, dryRun),
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Library detach effect kind is not defined."),
            };
            projected.Add(new ProjectedEffect
            {
                Data = new LibraryDetachDataEffect
                {
                    Path = effect.Path,
                    Kind = effect.Kind,
                    Action = effect.Action,
                    Outcome = LibraryDetachWording.WireState(outcome),
                    Target = detail >= CliDetail.Standard ? effect.Target : null,
                    Text = text,
                },
                Effect = SharedEffect(effect, outcome),
                Expected = effect.Expected,
                Outcome = outcome,
            });
        }

        return projected;
    }

    private static string LinkText(EffectOutcome outcome, bool dryRun)
        => outcome switch
        {
            EffectOutcome.Planned => LibraryDetachWording.LinkRow(dryRun),
            EffectOutcome.Done => global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.LabelLinkRemoved(),
            EffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            EffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            EffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static string SectionText(string path, EffectOutcome outcome, bool dryRun)
        => outcome switch
        {
            EffectOutcome.Planned or EffectOutcome.Done => LibraryDetachWording.SectionRow(path, dryRun),
            EffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            EffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            EffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static string RecordText(EffectOutcome outcome, bool dryRun)
        => outcome switch
        {
            EffectOutcome.Planned or EffectOutcome.Done => LibraryDetachWording.LockRow(dryRun),
            EffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            EffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            EffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static string SettingsText(EffectOutcome outcome, bool dryRun)
        => outcome switch
        {
            EffectOutcome.Planned or EffectOutcome.Done => LibraryDetachWording.SettingsRow(dryRun),
            EffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            EffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            EffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static CliEffect SharedEffect(PlannedEffect effect, EffectOutcome outcome)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                "link" => CliEffectKind.Link,
                "section" => CliEffectKind.Section,
                "record" => CliEffectKind.Record,
                "setting" => CliEffectKind.Setting,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Library detach effect kind is not defined."),
            },
            Action = effect.Kind switch
            {
                "link" => CliEffectAction.Deleted,
                "section" => CliEffectAction.Rewritten,
                "record" => CliEffectAction.Detached,
                "setting" => effect.Action == "create" ? CliEffectAction.Created : CliEffectAction.Rewritten,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Library detach effect kind is not defined."),
            },
            Outcome = outcome switch
            {
                EffectOutcome.Planned => CliEffectOutcome.Planned,
                EffectOutcome.Done => CliEffectOutcome.Done,
                EffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                EffectOutcome.Unknown => CliEffectOutcome.Unknown,
                EffectOutcome.Failed => CliEffectOutcome.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
            },
        };

    private static EffectOutcome Outcome(
        LibraryDetachResult result,
        string path,
        int index,
        int lastObserved)
    {
        if (string.Equals(path, ".agents/open-forge.json", StringComparison.Ordinal))
        {
            return result.Result.Identity.Mode == LibraryMode.DryRun
                ? EffectOutcome.Planned
                : result.Result.Permissions.Outcome switch
                {
                    "verified" => EffectOutcome.Done,
                    "not-started" or "not-requested" => EffectOutcome.NotStarted,
                    "verification-failed" => EffectOutcome.Failed,
                    "completion-unknown" => EffectOutcome.Unknown,
                    "planned" => EffectOutcome.Planned,
                    _ => throw new ArgumentOutOfRangeException(nameof(result), result.Result.Permissions.Outcome, "The Library settings outcome is not defined."),
                };
        }
        if (result.Result.Identity.Mode == LibraryMode.DryRun)
        {
            return EffectOutcome.Planned;
        }

        var residual = result.Result.Application.Residuals.FirstOrDefault(residual =>
            string.Equals(residual.Path, path, StringComparison.Ordinal));
        if (residual is { State: LibraryResidualState.Unknown })
        {
            return EffectOutcome.Unknown;
        }

        if (residual is { State: LibraryResidualState.Retained })
        {
            return EffectOutcome.Done;
        }

        if (result.Result.Application.State is LibraryApplicationState.Applied or LibraryApplicationState.NoOp)
        {
            return EffectOutcome.Done;
        }

        if (index <= lastObserved)
        {
            return EffectOutcome.Done;
        }

        return EffectOutcome.NotStarted;
    }

    private static int LastObservedIndex(
        IReadOnlyList<PlannedEffect> planned,
        IReadOnlyList<LibraryResidualView> residuals)
    {
        var last = -1;
        foreach (var residual in residuals)
        {
            var index = -1;
            for (var position = 0; position < planned.Count; position++)
            {
                if (string.Equals(planned[position].Path, residual.Path, StringComparison.Ordinal))
                {
                    index = position;
                    break;
                }
            }

            if (index > last)
            {
                last = index;
            }
        }

        return last;
    }

    private static IReadOnlyList<LibraryDetachDataTextRow> TextRows(
        LibraryDetachResult result,
        IReadOnlyList<ProjectedEffect> effects,
        CliDetail detail)
    {
        var partial = result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        var showLinks = detail >= CliDetail.Standard
            || result.Status == CliSemanticStatus.Attention
                && effects.Any(effect => effect.Data.Kind == "link" && effect.Outcome is EffectOutcome.Done or EffectOutcome.Planned)
            || partial && effects.Any(effect => effect.Data.Kind == "link" && effect.Outcome is EffectOutcome.Done or EffectOutcome.Unknown);
        return showLinks
            ? effects
                .Where(effect => effect.Data.Kind is "link" or "setting")
                .Select(effect => new LibraryDetachDataTextRow(effect.Data.Path, effect.Data.Text))
                .ToArray()
            : [];
    }

    private static IReadOnlyList<string> TextDetails(
        LibraryDetachResult result,
        IReadOnlyList<ProjectedEffect> effects,
        CliDetail detail)
    {
        var lines = new List<string>();
        var sections = effects.Where(effect => effect.Data.Kind == "section").ToArray();
        var records = effects.Where(effect => effect.Data.Kind == "record").ToArray();
        var settings = effects.Where(effect => effect.Data.Kind == "setting").ToArray();
        if (detail == CliDetail.Minimal)
        {
            lines.AddRange(sections
                .Where(effect => effect.Outcome is EffectOutcome.Planned or EffectOutcome.Done)
                .Select(effect => effect.Data.Text));
        }
        else if (detail >= CliDetail.Standard)
        {
            lines.AddRange(sections.Select(effect => effect.Data.Text));
            lines.AddRange(records.Select(effect => $".agents/open-forge.lock.json  {effect.Data.Text}"));
            lines.AddRange(settings.Select(effect => $".agents/open-forge.json  {effect.Data.Text}"));
        }

        if (detail >= CliDetail.Full)
        {
            lines.AddRange(effects.Select(effect => LibraryDetachWording.ExpectedState(
                effect.Data.Path,
                effect.Expected is { } expected ? LibraryDetachWording.ExpectedKind(expected.Kind) : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
                effect.Expected?.Length,
                effect.Expected?.Sha256,
                effect.Expected?.RawRelativeTarget)));
            lines.Add(LibraryDetachWording.Verification(
                result.Result.Application.Verification,
                result.Result.Application.RecordPublication.State));
            var recovery = result.Result.Application.Recovery;
            lines.Add(LibraryDetachWording.RecoveryFacts(
                recovery.State,
                recovery.Path,
                result.Result.Application.Residuals.Length));
        }

        if (result.Result.Identity.Mode == LibraryMode.DryRun)
        {
            lines.Add(LibraryDetachWording.NoFilesChanged());
        }

        return lines;
    }

    private static IReadOnlyList<LibraryDetachDataExpectedState> ExpectedStates(LibraryMutationPlanView plan)
    {
        var values = new List<LibraryDetachDataExpectedState>();
        values.AddRange(plan.Links.Select(link => Expected(link.Path, link.Expected)));
        values.AddRange(plan.GeneratedRegions.Select(region => Expected(region.Path, region.Expected)));
        if (plan.RecordExpected is { } record)
        {
            values.Add(Expected(".agents/open-forge.lock.json", record));
        }
        if (plan.SettingsChange is { } settingsChange)
        {
            values.Add(Expected(settingsChange.Path, settingsChange.Expected));
        }

        return values;
    }

    private static LibraryDetachDataExpectedState Expected(string path, LibraryExpectedState expected)
        => new()
        {
            Path = path,
            Kind = LibraryDetachWording.ExpectedKind(expected.Kind),
            Length = expected.Length,
            Sha256 = expected.Sha256,
            RawRelativeTarget = expected.RawRelativeTarget,
        };

    private static LibraryDetachDataRecovery ProjectRecovery(LibraryMutationApplication application)
        => new()
        {
            State = LibraryDetachWording.RecoveryState(application.Recovery.State),
            Path = application.Recovery.Path,
            Residuals = application.Residuals.Select(residual => new LibraryDetachDataResidual
            {
                Path = residual.Path,
                Kind = LibraryDetachWording.ResidualKind(residual.Kind),
                State = LibraryDetachWording.ResidualState(residual.State),
            }).ToArray(),
        };

    private static CliRecovery Recovery(LibraryMutationApplication application)
        => new(
            application.Recovery.Path,
            application.Recovery.State switch
            {
                LibraryRecoveryState.NotRequested or LibraryRecoveryState.Prepared => CliRecoveryDisposition.NotRequired,
                LibraryRecoveryState.Removed => CliRecoveryDisposition.Removed,
                LibraryRecoveryState.Retained => CliRecoveryDisposition.Retained,
                LibraryRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(application), application.Recovery.State, "The Library recovery state is not defined."),
            });

    private static bool RegistrationRemoved(LibraryDetachResult result)
        => result.Result.Identity.Mode != LibraryMode.DryRun
            && result.Result.Plan.RecordEffect != LibraryRecordEffect.None
            && result.Result.Application.RecordPublication.State == LibraryRecordPublicationState.Verified;

    private static int CompletedLinks(IReadOnlyList<ProjectedEffect> effects)
        => effects.Count(effect => effect.Data.Kind == "link" && effect.Outcome == EffectOutcome.Done);

    private static IReadOnlyList<string> Diagnostics(
        LibraryDetachResult result,
        LibraryDetachData data,
        IReadOnlyList<ProjectedEffect> effects)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"id={data.Id ?? "unavailable"}",
            $"effects={effects.Count}",
            $"links={effects.Count(effect => effect.Data.Kind == "link")}",
            $"sections={effects.Count(effect => effect.Data.Kind == "section")}",
            $"registration-removed={data.RegistrationRemoved}",
            $"findings={result.Result.Findings.Length}",
        ];

    private static string Sentence(string value) => value.Trim().TrimEnd('.');

    private sealed record PlannedEffect(
        string Path,
        string Kind,
        string Action,
        string? Target,
        LibraryExpectedState Expected);

    private enum EffectOutcome
    {
        Planned,
        Done,
        NotStarted,
        Unknown,
        Failed,
    }

    private sealed record ProjectedEffect
    {
        internal required LibraryDetachDataEffect Data { get; init; }
        internal required CliEffect Effect { get; init; }
        internal required LibraryExpectedState Expected { get; init; }
        internal required EffectOutcome Outcome { get; init; }
    }
}
