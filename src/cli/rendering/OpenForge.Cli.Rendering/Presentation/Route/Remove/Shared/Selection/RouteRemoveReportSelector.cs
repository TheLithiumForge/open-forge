using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Selection;

internal static class RouteRemoveReportSelector
{
    internal static CliReport<RouteRemoveData> Select(
        RouteRemoveResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        return new CliReport<RouteRemoveData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(result, finding)).ToArray(),
            Effects = result.Effects.Select(effect => Effect(effect, selection.Detail)).ToArray(),
            Counts = Counts(result),
            Data = Data(result, selection.Detail),
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static RouteRemoveData Data(RouteRemoveResult result, CliDetail detail)
    {
        var removedEffects = result.Effects
            .Where(effect => effect.Kind == RouteRemoveEffectKind.RemovedFile)
            .ToArray();
        var category = IsCategory(result);
        var headlinePath = result.Source.Path;
        var hideHeadlineFile = !category
            && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && headlinePath is not null
            && removedEffects.Any(effect => effect.Path == headlinePath && IsProgressed(effect, result.Mode));
        var rows = new List<string>();
        if (category)
        {
            rows.AddRange(removedEffects.Select(effect => FileRow(effect)));
        }
        else
        {
            rows.AddRange(removedEffects
                .Where(effect => !hideHeadlineFile || effect.Path != headlinePath)
                .Select(FileRow));
        }

        rows.AddRange(result.Effects
            .Where(effect => effect.Kind == RouteRemoveEffectKind.GeneratedRegion)
            .Select(GeneratedRow));

        if (result.Persistence.Settings.Outcome != RouteRemovePersistenceOutcome.NotEstablished)
        {
            rows.Add(RouteRemoveWording.PersistenceSettings(result.Persistence.Settings));
        }

        if (result.Persistence.Ownership.Outcome != RouteRemovePersistenceOutcome.NotEstablished)
        {
            rows.Add(RouteRemoveWording.PersistenceOwnership(result.Persistence.Ownership));
        }

        var details = new List<string>();
        foreach (var group in DetachmentGroups(result))
        {
            rows.Add(group.Summary);
            foreach (var detachment in group.Detachments)
            {
                details.Add($"  {detachment.SourcePath}:{Location(detachment)}");
                if (detail >= CliDetail.Standard)
                {
                    details.Add($"  {LinkText(detachment)}");
                }
            }
        }

        if (detail >= CliDetail.Full)
        {
            foreach (var effect in result.Effects)
            {
                if (effect.Before.ContentSha256 is { } before)
                {
                    details.Add($"  {CliFindingWording.BeforeHash(before)}");
                }

                if (effect.Expected.ContentSha256 is { } after)
                {
                    details.Add($"  {CliFindingWording.AfterHash(after)}");
                }
            }

            details.Add(RouteRemoveWording.Scan(
                result.References.ScannedSourceCount,
                result.References.OccurrenceCount));
        }

        if (result.Recovery.ResidualPath is { } recovery)
        {
            details.Add(RouteRemoveWording.Recovery(removedEffects.Length, recovery));
        }

        if (detail >= CliDetail.Full)
        {
            details.AddRange(result.Persistence.Ownership.Claims.Select(claim =>
                $"  {RouteRemoveWording.OwnershipClaim(claim)}"));
        }

        return new RouteRemoveData
        {
            Mode = ModeName(result.Mode),
            Subject = Subject(result),
            Source = new RouteRemoveDataSource
            {
                Id = result.Source.Id,
                Path = result.Source.Path,
            },
            Removed = removedEffects.Select(effect => effect.Path).ToArray(),
            Settings = new RouteRemoveDataSettingsRemoval
            {
                Outcome = CliReportVocabulary.Name(result.Persistence.Settings.Outcome),
                Path = result.Persistence.Settings.Path,
                Categories = result.Persistence.Settings.Categories,
                Files = result.Persistence.Settings.Files,
                Directories = result.Persistence.Settings.Directories,
            },
            OwnershipRelease = new RouteRemoveDataOwnershipRelease
            {
                Outcome = CliReportVocabulary.Name(result.Persistence.Ownership.Outcome),
                Claims = result.Persistence.Ownership.Claims.Select(claim => new RouteRemoveDataOwnershipClaim
                {
                    Path = claim.Path,
                    Manager = claim.Manager == RouteRemoveOwnershipManager.Framework ? "framework" : "extension",
                    Owner = claim.Owner,
                }).ToArray(),
            },
            DetachedLinks = result.References.Detachments
                .Select(detachment => new RouteRemoveDataDetachedLink
                {
                    Path = detachment.SourcePath,
                    Location = Location(detachment),
                    Before = detail >= CliDetail.Standard ? detachment.Before : null,
                    After = detail >= CliDetail.Standard ? detachment.Expected : null,
                })
                .ToArray(),
            Scan = detail >= CliDetail.Full
                ? new RouteRemoveDataScan
                {
                    FilesScanned = result.References.ScannedSourceCount,
                    Occurrences = result.References.OccurrenceCount,
                }
                : null,
            TextRows = rows,
            TextDetailLines = details,
            ShowNoChanges = result.Mode == RouteRemoveMode.DryRun,
        };
    }

    private static CliHeadline Headline(RouteRemoveResult result)
    {
        var path = result.Source.Path ?? result.Source.Requested;
        var id = result.Source.Id ?? result.Source.Requested;
        var finding = PrimaryFinding(result);
        var removedCount = result.Effects.Count(effect => effect.Kind == RouteRemoveEffectKind.RemovedFile);
        var category = IsCategory(result);
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Effects.IsEmpty
                => new(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageNothingToDoForSource(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Mode == RouteRemoveMode.DryRun && category
                => new(RouteRemoveWording.WouldRemoveRoute(id, removedCount), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete when result.Mode == RouteRemoveMode.DryRun
                => new(RouteRemoveWording.WouldRemove(path), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete when category
                => new(RouteRemoveWording.RemovedRoute(id, removedCount), CliHeadlineKind.Done),
            CliSemanticStatus.Complete
                => new(RouteRemoveWording.Removed(path), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when result.Mode == RouteRemoveMode.DryRun && category
                => new(RouteRemoveWording.WouldRemoveRoute(id, removedCount), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when result.Mode == RouteRemoveMode.DryRun
                => new(RouteRemoveWording.WouldRemove(path), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when category
                => new(RouteRemoveWording.RemovedRoute(id, removedCount), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(RouteRemoveWording.Removed(path), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteRemoveWording.Incomplete(
                    id,
                    finding is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRemovalCouldNotBeChecked() : Message(result, finding)),
                    CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(RouteRemoveWording.CannotRemove(result.Source.Requested, HeadlineProblem(result, finding)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RouteRemoveWording.Blocked(id, BlockedReason(result, finding)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteRemoveWording.Failed(ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(ProgressedCount(result) == 0
                    ? RouteRemoveWording.Cancelled()
                    : CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.ActionTitleRouteRemove(), ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Remove status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(RouteRemoveResult result)
    {
        if (result.Status is not (CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted))
        {
            return null;
        }

        var findings = result.Findings.Where(finding => finding.Status == result.Status).ToArray();
        return findings.Length == 1
            ? RouteRemoveWording.FindingCode(findings[0].Code)
            : null;
    }

    private static CliFinding Finding(RouteRemoveResult result, RouteRemoveFinding finding)
    {
        var action = Action(result, finding);
        var subject = Subject(result, finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = RouteRemoveWording.FindingCode(finding.Code),
            Title = RouteRemoveWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = subject,
            Actions = action is null ? [] : [action],
        };
    }

    private static CliSubject Subject(RouteRemoveResult result, RouteRemoveFinding finding)
    {
        var value = finding.Target
            ?? result.Source.Path
            ?? result.Source.Id
            ?? result.Source.Requested;
        var kind = finding.Code switch
        {
            RouteRemoveFindingCode.WorkspaceUnavailable
                or RouteRemoveFindingCode.WorkspaceUnsafe => CliSubjectKind.Workspace,
            RouteRemoveFindingCode.InvalidInput
                or RouteRemoveFindingCode.InvalidSource
                or RouteRemoveFindingCode.SourceNotFound
                or RouteRemoveFindingCode.InvalidSubject
                or RouteRemoveFindingCode.RouteAmbiguous
                or RouteRemoveFindingCode.IdentityCollision
                or RouteRemoveFindingCode.OverwriteAmbiguous => CliSubjectKind.Identifier,
            RouteRemoveFindingCode.CategoryUnsafe
                or RouteRemoveFindingCode.ProtectedTarget => CliSubjectKind.Directory,
            _ => CliSubjectKind.File,
        };
        var location = finding.LocationView is { } sourceLocation
            ? new CliSourceLocation(sourceLocation.Line, sourceLocation.Column)
            : null;
        return kind switch
        {
            CliSubjectKind.Workspace => new CliSubject(kind, Path: result.WorkspacePath ?? value),
            CliSubjectKind.Identifier => new CliSubject(kind, Id: value),
            CliSubjectKind.Directory => new CliSubject(kind, Path: CategoryFolder(value)),
            _ => new CliSubject(kind, Path: value, Location: location),
        };
    }

    private static string Message(RouteRemoveResult result, RouteRemoveFinding finding)
    {
        var path = finding.Target
            ?? result.Source.Path
            ?? result.Source.Id
            ?? result.Source.Requested;
        var reference = result.Source.Requested;
        return finding.Code switch
        {
            RouteRemoveFindingCode.InvalidInput
                => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.LabelRemoveTheRoutedSource(), TrimSentence(finding.Cause)),
            RouteRemoveFindingCode.ConfirmationRequired
                => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.ActionTitleRouteRemove()),
            RouteRemoveFindingCode.InvalidSource
                => RouteRemoveWording.InvalidSource(reference),
            RouteRemoveFindingCode.SourceNotFound
                => CliFindingWording.UnknownSource(reference),
            RouteRemoveFindingCode.InvalidSubject
                => RouteRemoveWording.InvalidSubject(reference, finding.Cause),
            RouteRemoveFindingCode.WorkspaceUnavailable
                => CliFindingWording.WorkspaceUnavailable(result.WorkspacePath ?? path),
            RouteRemoveFindingCode.WorkspaceUnsafe
                => CliFindingWording.WorkspaceUnsafe(result.WorkspacePath ?? path, TrimSentence(finding.Cause)),
            RouteRemoveFindingCode.SourceUnsafe
                => CliFindingWording.SourceUnsafe(path),
            RouteRemoveFindingCode.RouteAmbiguous
                => CliFindingWording.RouteAmbiguous(result.Source.Id ?? reference),
            RouteRemoveFindingCode.IdentityCollision
                => CliFindingWording.IdentityCollision(result.Source.Id ?? reference),
            RouteRemoveFindingCode.OverwriteAmbiguous
                => RouteRemoveWording.OverwriteAmbiguous(FileName(path)),
            RouteRemoveFindingCode.CategoryUnsafe
                => CliFindingWording.CauseSentence(finding.Cause),
            RouteRemoveFindingCode.OwnershipUnavailable
                => CliFindingWording.LifecycleUnavailable(),
            RouteRemoveFindingCode.SettingsUnavailable
                => CliFindingWording.CauseSentence(finding.Cause),
            RouteRemoveFindingCode.ProtectedTarget
                => CliFindingWording.CauseSentence(finding.Cause),
            RouteRemoveFindingCode.OwnershipClaimed
                => OwnershipClaimed(result, finding),
            RouteRemoveFindingCode.ReferenceUnsafe
                => finding.LocationView is { } location
                    ? RouteRemoveWording.ReferenceUnsafe(path, location.Line, location.Column, finding.Cause)
                    : CliFindingWording.CauseSentence(finding.Cause),
            RouteRemoveFindingCode.GeneratedRegionUnsafe
                => CliFindingWording.GeneratedRegionUnsafe(path, TrimSentence(finding.Cause)),
            RouteRemoveFindingCode.WorkspaceLockUnavailable
                => CliFindingWording.WorkspaceLockUnavailable(),
            RouteRemoveFindingCode.TargetChanged
                => CliFindingWording.TargetChanged(path),
            RouteRemoveFindingCode.RecoveryConflict
                => CliFindingWording.RecoveryConflict(path),
            RouteRemoveFindingCode.CategoryInventoryIncomplete
                => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatSomeFilesUnderCouldNotBeListedSoTheRemovalWasNotPlanned($"{CategoryFolder(path)}"),
            RouteRemoveFindingCode.ReferenceCoverageIncomplete
                => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatSomeFilesCouldNotBeScannedForLinksToSoTheRemovalWasNotPlanned($"{result.Source.Path ?? path}"),
            RouteRemoveFindingCode.ProjectionIncomplete
                => CliFindingWording.ProjectionUnavailable(path, TrimSentence(finding.Cause)),
            RouteRemoveFindingCode.RecoveryUnavailable
                => CliFindingWording.RecoveryUnavailable(result.WorkspacePath ?? path),
            RouteRemoveFindingCode.InspectionIncomplete
                => CliFindingWording.InspectionIncomplete(path),
            RouteRemoveFindingCode.RecoveryArtifactRetained
                => CliFindingWording.RecoveryRetained(result.Recovery.ResidualPath ?? path),
            RouteRemoveFindingCode.TargetChangedDuringApply
                => CliFindingWording.TargetChangedDuringApply(path, ProgressedCount(result), result.Effects.Length),
            RouteRemoveFindingCode.WriteFailed
                => CliFindingWording.WriteFailed(path, ProgressedCount(result), result.Effects.Length, result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteRemoveFindingCode.VerificationFailed
                => CliFindingWording.VerificationFailed(path, result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteRemoveFindingCode.RecoveryFailed
                => CliFindingWording.RecoveryFailed(),
            RouteRemoveFindingCode.OperationFailed
                => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.ActionTitleRouteRemove(), TrimSentence(finding.Cause)),
            RouteRemoveFindingCode.Interrupted
                => ProgressedCount(result) == 0
                    ? RouteRemoveWording.Cancelled()
                    : CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.ActionTitleRouteRemove(), ProgressedCount(result), result.Effects.Length),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Route Remove finding code is not defined."),
        };
    }

    private static string HeadlineProblem(RouteRemoveResult result, RouteRemoveFinding? finding)
        => finding is null
            ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheInputIsInvalid()
            : finding.Code == RouteRemoveFindingCode.InvalidInput
                ? TrimSentence(finding.Cause)
                : Message(result, finding).Trim().TrimEnd('.');

    private static string BlockedReason(RouteRemoveResult result, RouteRemoveFinding? finding)
        => finding is null
            ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRemovalIsBlocked()
            : Message(result, finding).Trim().TrimEnd('.');

    private static string OwnershipClaimed(RouteRemoveResult result, RouteRemoveFinding finding)
    {
        var path = finding.Target ?? result.Source.Path ?? result.Source.Requested;
        var claim = result.Ownership.Claims.FirstOrDefault(candidate => candidate.Path == path)
            ?? result.Ownership.Claims.FirstOrDefault();
        if (claim is null)
        {
            return CliFindingWording.CauseSentence(finding.Cause);
        }

        var owner = claim.Manager == RouteRemoveOwnershipManager.Framework
            ? global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelTheFramework()
            : global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatTheExtension($"{claim.Owner}");
        return CliFindingWording.OwnershipClaimed(path, owner, global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleRouteRemove(), global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.LabelRemove());
    }

    private static CliNextAction? Action(RouteRemoveResult result, RouteRemoveFinding finding)
        => finding.Code switch
        {
            RouteRemoveFindingCode.InvalidSource
                => new CliNextAction("open-forge route list --depth=all", RouteRemoveWording.ListReason()),
            RouteRemoveFindingCode.ConfirmationRequired
                => new CliNextAction(
                    $"open-forge route remove \"{result.Source.Requested}\" --automatic",
                    global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageRerunRouteRemoveWithAutomatic()),
            RouteRemoveFindingCode.CategoryInventoryIncomplete
                or RouteRemoveFindingCode.ReferenceCoverageIncomplete
                or RouteRemoveFindingCode.ProjectionIncomplete
                or RouteRemoveFindingCode.InspectionIncomplete
                or RouteRemoveFindingCode.RecoveryUnavailable
                => new CliNextAction("open-forge doctor", RouteRemoveWording.DoctorReason()),
            RouteRemoveFindingCode.OwnershipClaimed
                => OwnershipAction(result, finding),
            RouteRemoveFindingCode.OverwriteAmbiguous
                or RouteRemoveFindingCode.ReferenceUnsafe
                => ManualAction(),
            RouteRemoveFindingCode.WorkspaceLockUnavailable
                or RouteRemoveFindingCode.TargetChanged
                or RouteRemoveFindingCode.TargetChangedDuringApply
                => new CliNextAction("open-forge route remove", RouteRemoveWording.RetryReason()),
            RouteRemoveFindingCode.OperationFailed
                => new CliNextAction(VerboseRouteRemoveCommand, RouteRemoveWording.DebugReason()),
            _ => null,
        };

    private static CliNextAction? Next(RouteRemoveResult result)
    {
        if (result.Recovery.ResidualPath is not null)
        {
            return new CliNextAction("open-forge cleanup", RouteRemoveWording.CleanupReason());
        }

        var finding = PrimaryFinding(result);
        if (finding is not null && Action(result, finding) is { } action)
        {
            return action;
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Invalid => new CliNextAction(RouteRemoveHelpCommand, RouteRemoveWording.HelpReason()),
            CliSemanticStatus.Incomplete => new CliNextAction("open-forge doctor", RouteRemoveWording.DoctorReason()),
            CliSemanticStatus.Blocked => null,
            CliSemanticStatus.Failed => new CliNextAction(VerboseRouteRemoveCommand, RouteRemoveWording.DebugReason()),
            CliSemanticStatus.Interrupted => new CliNextAction("open-forge route remove", RouteRemoveWording.RerunReason()),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Remove status is not defined."),
        };
    }

    private static CliNextAction? OwnershipAction(RouteRemoveResult result, RouteRemoveFinding finding)
    {
        var path = finding.Target ?? result.Source.Path ?? result.Source.Requested;
        var claim = result.Ownership.Claims.FirstOrDefault(candidate => candidate.Path == path)
            ?? result.Ownership.Claims.FirstOrDefault();
        return claim?.Manager switch
        {
            RouteRemoveOwnershipManager.Framework
                => new CliNextAction("open-forge update", RouteRemoveWording.UpdateReason()),
            RouteRemoveOwnershipManager.Extension
                => new CliNextAction($"open-forge extension remove {claim.Owner}", RouteRemoveWording.ExtensionReason()),
            _ => null,
        };
    }

    private static CliNextAction ManualAction()
        => new CliNextAction(global::OpenForge.Cli.OutputText.Shared.SharedText.MessageFixItByHand(), RouteRemoveWording.ManualReason())
        {
            Kind = CliNextActionKind.Sentence,
        };

    private static CliEffect Effect(RouteRemoveEffect effect, CliDetail detail)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                RouteRemoveEffectKind.Directory => CliEffectKind.Directory,
                RouteRemoveEffectKind.RemovedFile => CliEffectKind.File,
                RouteRemoveEffectKind.ReferenceSource => CliEffectKind.Link,
                RouteRemoveEffectKind.GeneratedRegion => CliEffectKind.Section,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Route Remove effect kind is not defined."),
            },
            Action = effect.Kind switch
            {
                RouteRemoveEffectKind.ReferenceSource => CliEffectAction.Detached,
                RouteRemoveEffectKind.GeneratedRegion => CliEffectAction.Rewritten,
                RouteRemoveEffectKind.Directory
                    or RouteRemoveEffectKind.RemovedFile => CliEffectAction.Deleted,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Route Remove effect kind is not defined."),
            },
            Outcome = effect.Outcome switch
            {
                RouteRemoveEffectOutcome.Planned => CliEffectOutcome.Planned,
                RouteRemoveEffectOutcome.Verified => CliEffectOutcome.Done,
                RouteRemoveEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                RouteRemoveEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                RouteRemoveEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Remove effect outcome is not defined."),
            },
            Before = detail >= CliDetail.Full ? effect.Before.ContentSha256 : null,
            After = detail >= CliDetail.Full ? effect.Expected.ContentSha256 : null,
        };

    private static IReadOnlyList<CliCount> Counts(RouteRemoveResult result)
        =>
        [
            new CliCount("filesRemoved", global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.LabelFilesRemoved(), CountProgressed(result, RouteRemoveEffectKind.RemovedFile)),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), CountProgressed(result, RouteRemoveEffectKind.GeneratedRegion)),
            new CliCount("linksDetached", global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.LabelLinksDetached(), result.References.Detachments.Length),
            new CliCount("filesScanned", global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelFilesScanned(), result.References.ScannedSourceCount),
        ];

    private static CliRecovery? Recovery(RouteRemoveResult result)
        => result.Recovery.ResidualPath is not { } path
            ? null
            : new CliRecovery(
                path,
                result.Recovery.State switch
                {
                    RouteRemoveRecoveryState.Retained => CliRecoveryDisposition.Retained,
                    RouteRemoveRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                    RouteRemoveRecoveryState.NotRequired
                        or RouteRemoveRecoveryState.NotCreated
                        or RouteRemoveRecoveryState.Removed => CliRecoveryDisposition.Failed,
                    _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State, "The Route Remove recovery state is not defined."),
                });

    private static IReadOnlyList<string> Diagnostics(RouteRemoveResult result)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={ModeName(result.Mode)}",
            $"source={result.Source.Id ?? result.Source.Requested}",
            $"subject={Subject(result)}",
            $"effects={result.Effects.Length.ToString(CultureInfo.InvariantCulture)}",
            $"removed={result.Effects.Count(effect => effect.Kind == RouteRemoveEffectKind.RemovedFile).ToString(CultureInfo.InvariantCulture)}",
            $"detached={result.References.Detachments.Length.ToString(CultureInfo.InvariantCulture)}",
            $"findings={result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
        }.Concat(result.Findings.Select(finding => finding.Cause)).ToArray();

    private static string FileRow(RouteRemoveEffect effect)
        => effect.Outcome switch
        {
            RouteRemoveEffectOutcome.Planned => RouteRemoveWording.WouldRemove(effect.Path),
            RouteRemoveEffectOutcome.Verified => RouteRemoveWording.Removed(effect.Path),
            RouteRemoveEffectOutcome.NotStarted => RouteRemoveWording.NotStarted(effect.Path),
            RouteRemoveEffectOutcome.CompletionUnknown => RouteRemoveWording.Unknown(effect.Path),
            RouteRemoveEffectOutcome.VerificationFailed => RouteRemoveWording.FailedEffect(effect.Path),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Remove effect outcome is not defined."),
        };

    private static string GeneratedRow(RouteRemoveEffect effect)
        => effect.Outcome switch
        {
            RouteRemoveEffectOutcome.Planned => RouteRemoveWording.WouldEntryRemoved(effect.Path),
            RouteRemoveEffectOutcome.Verified => RouteRemoveWording.EntryRemoved(effect.Path),
            RouteRemoveEffectOutcome.NotStarted => RouteRemoveWording.NotStarted(effect.Path),
            RouteRemoveEffectOutcome.CompletionUnknown => RouteRemoveWording.Unknown(effect.Path),
            RouteRemoveEffectOutcome.VerificationFailed => RouteRemoveWording.FailedEffect(effect.Path),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Remove effect outcome is not defined."),
        };

    private static IEnumerable<DetachmentGroup> DetachmentGroups(RouteRemoveResult result)
    {
        foreach (var group in result.References.Detachments.GroupBy(
                     detachment => detachment.SourcePath,
                     StringComparer.Ordinal))
        {
            var effect = result.Effects.FirstOrDefault(candidate =>
                candidate.Kind == RouteRemoveEffectKind.ReferenceSource
                && candidate.Path == group.Key);
            var planned = result.Mode == RouteRemoveMode.DryRun
                || effect?.Outcome == RouteRemoveEffectOutcome.Planned;
            yield return new DetachmentGroup(
                planned
                    ? RouteRemoveWording.WouldDetachLinks(group.Count())
                    : RouteRemoveWording.DetachedLinks(group.Count()),
                group.OrderBy(detachment => detachment.LocationView.ByteOffset));
        }
    }

    private static string LinkText(RouteRemoveReferenceDetachment detachment)
        => RouteRemoveWording.LinkText(
            $"[{detachment.VisibleLabel}]({detachment.OriginalDestination})",
            detachment.VisibleLabel);

    private static string Location(RouteRemoveReferenceDetachment detachment)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{detachment.LocationView.Line}:{detachment.LocationView.Column}");

    private static string Subject(RouteRemoveResult result)
        => IsCategory(result) ? "route" : "file";

    private static bool IsCategory(RouteRemoveResult result)
        => result.Subject.Kind == RouteRemoveSubjectKind.Category
            || result.Source.Form is RouteRemoveSourceForm.CanonicalEntrypoint
                or RouteRemoveSourceForm.CompatibilityEntrypoint;

    private static string CategoryFolder(string path)
    {
        var normalized = path.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        var fileName = slash < 0 ? normalized : normalized[(slash + 1)..];
        if (fileName.StartsWith('_') && fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            return slash < 0 ? "." : normalized[..slash];
        }

        return normalized;
    }

    private static string FileName(string path)
    {
        var normalized = path.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        return slash < 0 ? normalized : normalized[(slash + 1)..];
    }

    private static RouteRemoveFinding? PrimaryFinding(RouteRemoveResult result)
        => result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();

    private static int CountProgressed(RouteRemoveResult result, RouteRemoveEffectKind kind)
        => result.Effects.Count(effect => effect.Kind == kind && IsProgressed(effect, result.Mode));

    private static int ProgressedCount(RouteRemoveResult result)
        => result.Effects.Count(effect => IsProgressed(effect, result.Mode));

    private static bool IsProgressed(RouteRemoveEffect effect, RouteRemoveMode mode)
        => mode == RouteRemoveMode.DryRun
            ? effect.Outcome == RouteRemoveEffectOutcome.Planned
            : effect.Outcome == RouteRemoveEffectOutcome.Verified;

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');

    private static string ModeName(RouteRemoveMode mode)
        => mode switch
        {
            RouteRemoveMode.Apply => "apply",
            RouteRemoveMode.DryRun => "dry-run",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Route Remove mode is not defined."),
        };

    private const string RouteRemoveHelpCommand = "open-forge route remove --help";
    private const string VerboseRouteRemoveCommand = "open-forge route remove --detail debug";

    private sealed record DetachmentGroup(
        string Summary,
        IEnumerable<RouteRemoveReferenceDetachment> Detachments);
}
