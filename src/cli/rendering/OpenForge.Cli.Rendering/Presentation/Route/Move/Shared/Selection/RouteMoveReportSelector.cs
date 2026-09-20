using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Move.Models;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Selection;

internal static class RouteMoveReportSelector
{
    internal static CliReport<RouteMoveData> Select(
        RouteMoveResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        return new CliReport<RouteMoveData>
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

    private static RouteMoveData Data(RouteMoveResult result, CliDetail detail)
    {
        var standard = detail >= CliDetail.Standard;
        var full = detail >= CliDetail.Full;
        var moved = MovedPairs(result);
        return new RouteMoveData
        {
            Mode = RouteMoveWireVocabulary.Name(result.Mode),
            Subject = result.Subject.Kind == RouteMoveSubjectKind.Category ? "route" : "file",
            Source = new RouteMoveDataSource
            {
                Id = result.Source.Id,
                Path = result.Source.Path,
            },
            Destination = new RouteMoveDataDestination
            {
                Id = result.Destination.Id,
                Path = result.Destination.Path,
            },
            Moved = moved.Select(pair => new RouteMoveDataMoved
            {
                From = pair.From,
                To = pair.To,
                Overwrite = standard && pair.OverwriteFrom is not null && pair.OverwriteTo is not null
                    ? new RouteMoveDataOverwrite
                    {
                        From = pair.OverwriteFrom,
                        To = pair.OverwriteTo,
                    }
                    : null,
            }).ToArray(),
            RewrittenLinks = result.References.Rewrites.Select(rewrite => new RouteMoveDataRewrittenLink
            {
                Path = rewrite.DestinationSourcePath,
                Location = Location(rewrite),
                From = standard
                    ? Target(rewrite.OldTarget)
                    : null,
                To = standard
                    ? Target(rewrite.ExpectedTarget)
                    : null,
            }).ToArray(),
            Scan = full
                ? new RouteMoveDataScan
                {
                    FilesScanned = result.References.ScannedSourceCount,
                    Occurrences = result.References.OccurrenceCount,
                }
                : null,
            TextRows = TextRows(result, detail, moved),
        };
    }

    private static IReadOnlyList<string> TextRows(
        RouteMoveResult result,
        CliDetail detail,
        IReadOnlyList<MovedPair> moved)
    {
        var standard = detail >= CliDetail.Standard;
        var full = detail >= CliDetail.Full;
        var rows = new List<string>();
        var category = result.Subject.Kind == RouteMoveSubjectKind.Category;
        var dryRun = result.Mode == RouteMoveMode.DryRun;

        if (category)
        {
            foreach (var pair in moved)
            {
                rows.Add(RouteMoveWording.Moved(pair.From, pair.To, dryRun));
                if (pair.OverwriteFrom is not null && pair.OverwriteTo is not null)
                {
                    rows.Add(RouteMoveWording.Moved(pair.OverwriteFrom, pair.OverwriteTo, dryRun));
                }
            }
        }
        else
        {
            var primary = moved.FirstOrDefault();
            var headlineCoversPrimary = primary is not null
                && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention;
            if (primary is not null && !headlineCoversPrimary)
            {
                rows.Add(RouteMoveWording.Moved(primary.From, primary.To, dryRun));
            }

            if (standard && primary?.OverwriteFrom is not null && primary.OverwriteTo is not null)
            {
                rows.Add(RouteMoveWording.Moved(primary.OverwriteFrom, primary.OverwriteTo, dryRun));
            }
        }

        rows.AddRange(NavigationRows(result, category));

        if (result.References.Rewrites.Length > 0)
        {
            rows.Add(RouteMoveWording.RewriteHeading(result.References.Rewrites.Length, category));
            foreach (var rewrite in result.References.Rewrites)
            {
                var location = RouteMoveWording.RewriteLocation(
                    rewrite.DestinationSourcePath,
                    rewrite.LocationView.Line,
                    rewrite.LocationView.Column);
                if (standard)
                {
                    location += $"  {RouteMoveWording.RewriteDestination(rewrite.OldTarget.Path, rewrite.ExpectedTarget.Path)}";
                }

                rows.Add(location);
            }
        }

        if (full)
        {
            rows.Add(global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.HeadingEffects());
            rows.AddRange(result.Effects.Select(EffectRow));
            rows.Add(RouteMoveWording.ScanSummary(
                result.References.ScannedSourceCount,
                result.References.OccurrenceCount));
        }

        return rows;
    }

    private static IReadOnlyList<string> NavigationRows(RouteMoveResult result, bool category)
    {
        var rows = new List<string>();
        foreach (var region in result.GeneratedNavigation.Regions
                     .Where(region => region.State == RouteMoveGeneratedState.Changed))
        {
            if (!category)
            {
                rows.Add(RouteMoveWording.EntryUpdated(region.Path));
                continue;
            }

            foreach (var reason in region.Reasons)
            {
                var row = reason switch
                {
                    RouteMoveGeneratedReason.OldParent => RouteMoveWording.EntryRemoved(region.Path),
                    RouteMoveGeneratedReason.NewParent => RouteMoveWording.EntryAdded(region.Path),
                    RouteMoveGeneratedReason.Loader
                        or RouteMoveGeneratedReason.MovedEntrypoint => RouteMoveWording.EntryUpdated(region.Path),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(region),
                        reason,
                        "The Route Move generated-navigation reason is not defined."),
                };
                if (!rows.Contains(row, StringComparer.Ordinal))
                {
                    rows.Add(row);
                }
            }
        }

        return rows;
    }

    private static string EffectRow(RouteMoveEffect effect)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{effect.Path}: {Action(effect.Action)} {Kind(effect.Kind)} / {PathState(effect.Before)} -> {PathState(effect.Expected)} / {Outcome(effect.Outcome)}");

    private static string Action(RouteMoveEffectAction action)
        => action switch
        {
            RouteMoveEffectAction.Create => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreate(),
            RouteMoveEffectAction.Replace => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelRewrite(),
            RouteMoveEffectAction.Delete => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDelete(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Route Move effect action is not defined."),
        };

    private static string Kind(RouteMoveEffectKind kind)
        => kind switch
        {
            RouteMoveEffectKind.Directory => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDirectory(),
            RouteMoveEffectKind.MovedFile => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFile(),
            RouteMoveEffectKind.ReferenceSource => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelLinkSource(),
            RouteMoveEffectKind.GeneratedRegion => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleEntriesSection(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Route Move effect kind is not defined."),
        };

    private static string Outcome(RouteMoveEffectOutcome outcome)
        => outcome switch
        {
            RouteMoveEffectOutcome.Planned => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelPlanned(),
            RouteMoveEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            RouteMoveEffectOutcome.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerified(),
            RouteMoveEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            RouteMoveEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Route Move effect outcome is not defined."),
        };

    private static string PathState(RouteMovePathState state)
        => RouteMoveWireVocabulary.Name(state.Kind) switch
        {
            "file" when state.ContentSha256 is { } hash => $"file:{hash}",
            var value => value,
        };

    private static CliHeadline Headline(RouteMoveResult result)
    {
        var id = result.Source.Id ?? result.Source.Requested;
        var path = result.Destination.Path ?? result.Destination.Requested;
        var first = FirstFinding(result);
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Mode == RouteMoveMode.DryRun
                => new(RouteMoveWording.WouldMove(id, DestinationForHeadline(result)), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => CompleteHeadline(result, id, path, CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => CompleteHeadline(result, id, path, CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteMoveWording.Incomplete(id, Message(result, first)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(RouteMoveWording.Invalid(result.Source.Requested, Problem(result, first)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RouteMoveWording.Blocked(id, Problem(result, first)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteMoveWording.Failed(ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RouteMoveWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Move status is not defined."),
        };
    }

    private static CliHeadline CompleteHeadline(
        RouteMoveResult result,
        string id,
        string path,
        CliHeadlineKind kind)
        => result.Subject.Kind == RouteMoveSubjectKind.Category
            ? new(RouteMoveWording.MovedCategory(id, ParentFolder(path), MovedFileCount(result)), kind)
            : new(RouteMoveWording.MovedFile(id, path), kind);

    private static string? HeadlineFindingCode(RouteMoveResult result)
    {
        if (result.Findings.Length != 1)
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            ? RouteMoveWireVocabulary.Name(result.Findings[0].Code)
            : null;
    }

    private static CliFinding Finding(RouteMoveResult result, RouteMoveFinding finding)
        => new()
        {
            Severity = finding.Code == RouteMoveFindingCode.OwnershipUnavailable
                ? CliSeverity.Warning
                : CliReportVocabulary.Severity(finding.Status),
            Code = RouteMoveWireVocabulary.Name(finding.Code),
            Title = RouteMoveWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = Subject(result, finding),
            Resolution = finding.Code is RouteMoveFindingCode.OverwriteAmbiguous
                or RouteMoveFindingCode.ReferenceUnsafe
                ? CliResolution.ManualDecision
                : null,
            Actions = Action(result, finding) is { } action ? [action] : [],
        };

    private static CliSubject Subject(RouteMoveResult result, RouteMoveFinding finding)
    {
        var target = finding.Target
            ?? result.Source.Path
            ?? result.Source.Id
            ?? result.Source.Requested;
        if (finding.Code is RouteMoveFindingCode.WorkspaceUnavailable
            or RouteMoveFindingCode.WorkspaceUnsafe)
        {
            return new CliSubject(
                CliSubjectKind.Workspace,
                Path: result.WorkspacePhysicalPath ?? target);
        }

        if (finding.Code is RouteMoveFindingCode.InvalidInput
            or RouteMoveFindingCode.InvalidSource
            or RouteMoveFindingCode.SourceNotFound
            or RouteMoveFindingCode.InvalidSubject
            or RouteMoveFindingCode.InvalidDestination
            or RouteMoveFindingCode.RouteAmbiguous
            or RouteMoveFindingCode.IdentityCollision
            or RouteMoveFindingCode.OverwriteAmbiguous)
        {
            return new CliSubject(CliSubjectKind.Identifier, Id: target);
        }

        if (finding.Code is RouteMoveFindingCode.CategoryUnsafe
            or RouteMoveFindingCode.DestinationParentMissing)
        {
            return new CliSubject(CliSubjectKind.Directory, Path: ParentFolder(target));
        }

        if (finding.Code is RouteMoveFindingCode.SourceUnsafe
            or RouteMoveFindingCode.DestinationUnsafe)
        {
            return new CliSubject(CliSubjectKind.Source, Path: target);
        }

        return new CliSubject(CliSubjectKind.File, Path: target);
    }

    private static string Message(RouteMoveResult result, RouteMoveFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelTheBlockingConditionIsNotDefined();
        }

        var target = finding.Target
            ?? result.Source.Path
            ?? result.Source.Id
            ?? result.Source.Requested;
        var workspace = result.WorkspacePhysicalPath ?? target;
        return finding.Code switch
        {
            RouteMoveFindingCode.InvalidInput => RouteMoveWording.TrimSentence(finding.Cause),
            RouteMoveFindingCode.InvalidSource => RouteMoveWording.InvalidSource(result.Source.Requested),
            RouteMoveFindingCode.SourceNotFound => CliFindingWording.UnknownSource(result.Source.Requested),
            RouteMoveFindingCode.InvalidSubject => RouteMoveWording.InvalidSubject(
                result.Source.Requested,
                IsOverwrite(target)),
            RouteMoveFindingCode.InvalidDestination => RouteMoveWording.InvalidDestination(
                result.Destination.Requested,
                result.Subject.Kind == RouteMoveSubjectKind.Category),
            RouteMoveFindingCode.SelfMove => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageTheSourceAndTheDestinationAreTheSame(),
            RouteMoveFindingCode.DestinationInsideSource => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageTheDestinationIsInsideTheFolderBeingMoved(),
            RouteMoveFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(workspace),
            RouteMoveFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(
                workspace,
                WorkspaceReason(finding.Cause)),
            RouteMoveFindingCode.SourceUnsafe => CliFindingWording.SourceUnsafe(target),
            RouteMoveFindingCode.DestinationUnsafe => CliFindingWording.TargetUnsafe(
                target,
                RouteMoveWording.TrimSentence(finding.Cause)),
            RouteMoveFindingCode.DestinationOccupied => RouteMoveWording.DestinationOccupied(target),
            RouteMoveFindingCode.DestinationParentMissing => RouteMoveWording.DestinationParentMissing(
                result.Destination.ParentPath ?? ParentFolder(result.Destination.Requested)),
            RouteMoveFindingCode.CategoryUnsafe => RouteMoveWording.CategoryUnsafe(
                ParentFolder(target),
                target,
                finding.Cause),
            RouteMoveFindingCode.RouteAmbiguous => CliFindingWording.RouteAmbiguous(
                result.Source.Id ?? result.Source.Requested),
            RouteMoveFindingCode.IdentityCollision => CliFindingWording.IdentityCollision(
                result.Destination.Id ?? result.Source.Id ?? result.Source.Requested),
            RouteMoveFindingCode.OverwriteAmbiguous => RouteMoveWording.OverwriteAmbiguous(
                OverwriteName(target)),
            RouteMoveFindingCode.OwnershipClaimed => OwnershipMessage(result, target),
            RouteMoveFindingCode.ReferenceUnsafe => RouteMoveWording.ReferenceUnsafe(
                target,
                finding.Cause),
            RouteMoveFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(
                target,
                GeneratedReason(finding.Cause)),
            RouteMoveFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            RouteMoveFindingCode.TargetChanged => CliFindingWording.TargetChanged(target),
            RouteMoveFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(target),
            RouteMoveFindingCode.OwnershipUnavailable => CliFindingWording.LifecycleUnavailable(),
            RouteMoveFindingCode.InspectionIncomplete => CliFindingWording.InspectionIncomplete(target),
            RouteMoveFindingCode.CategoryInventoryIncomplete => RouteMoveWording.CategoryInventoryIncomplete(
                ParentFolder(target)),
            RouteMoveFindingCode.ReferenceCoverageIncomplete => RouteMoveWording.ReferenceCoverageIncomplete(target),
            RouteMoveFindingCode.ProjectionIncomplete => CliFindingWording.ProjectionUnavailable(
                target,
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelChildMetadataCouldNotBeRead()),
            RouteMoveFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(
                finding.Target ?? workspace),
            RouteMoveFindingCode.RecoveryArtifactRetained => CliFindingWording.RecoveryRetained(
                result.Recovery.ResidualPath ?? target),
            RouteMoveFindingCode.TargetChangedDuringApply => CliFindingWording.TargetChangedDuringApply(
                target,
                ProgressedCount(result),
                result.Effects.Length),
            RouteMoveFindingCode.WriteFailed => CliFindingWording.WriteFailed(
                target,
                ProgressedCount(result),
                result.Effects.Length,
                result.Recovery.ResidualPath ?? "unknown"),
            RouteMoveFindingCode.VerificationFailed => CliFindingWording.VerificationFailed(
                target,
                result.Recovery.ResidualPath ?? "unknown"),
            RouteMoveFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            RouteMoveFindingCode.OperationFailed => CliFindingWording.OperationFailed(
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.ActionTitleRouteMove(),
                RouteMoveWording.TrimSentence(finding.Cause)),
            RouteMoveFindingCode.Interrupted => ProgressedCount(result) == 0
                ? CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.ActionTitleRouteMove())
                : CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.ActionTitleRouteMove(), ProgressedCount(result), result.Effects.Length),
            _ => throw new ArgumentOutOfRangeException(
                nameof(finding),
                finding.Code,
                "The Route Move finding code is not defined."),
        };
    }

    private static string Problem(RouteMoveResult result, RouteMoveFinding? finding)
        => finding is null
            ? global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelTheBlockingConditionIsNotDefined()
            : RouteMoveWording.TrimSentence(Message(result, finding));

    private static CliNextAction? Action(RouteMoveResult result, RouteMoveFinding finding)
        => finding.Code switch
        {
            RouteMoveFindingCode.InvalidInput
                => new CliNextAction(
                    "open-forge route move --help",
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageCorrectTheNamedRouteMoveInputThenRerunTheRequest()),
            RouteMoveFindingCode.InvalidSource
                => new CliNextAction(
                    "open-forge route list --depth=all",
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageListAllRoutesThenRerunRouteMoveWithAnExactSource()),
            RouteMoveFindingCode.InvalidDestination
                => new CliNextAction(
                    "open-forge route move --help",
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageCorrectTheNamedRouteMoveInputThenRerunTheRequest()),
            RouteMoveFindingCode.DestinationOccupied
                => new CliNextAction(
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelChooseAnotherDestination(),
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageChooseAnotherDestinationThenRerunRouteMove())
                {
                    Kind = CliNextActionKind.Sentence,
                },
            RouteMoveFindingCode.DestinationParentMissing
                => new CliNextAction(
                    $"open-forge route init {ParentId(result)}",
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageInitializeTheExactMissingDestinationParentRouteThenRerunRouteMove()),
            RouteMoveFindingCode.OwnershipClaimed
                => OwnershipAction(result, finding),
            RouteMoveFindingCode.ReferenceUnsafe
                or RouteMoveFindingCode.OverwriteAmbiguous
                => new CliNextAction(
                    global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFixByHand(),
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageFixTheNamedBoundaryByHandThenRerunRouteMove())
                {
                    Kind = CliNextActionKind.Sentence,
                },
            _ => null,
        };

    private static CliNextAction? OwnershipAction(RouteMoveResult result, RouteMoveFinding finding)
    {
        var claim = result.Ownership.Claims.FirstOrDefault(claim =>
            string.Equals(claim.Path, finding.Target, StringComparison.Ordinal));
        if (claim is null)
        {
            return new CliNextAction(
                "open-forge update",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageReconcileTheManagedSourceBeforeRerunningRouteMove());
        }

        return claim.Manager == RouteMoveOwnershipManager.Framework
            ? new CliNextAction(
                "open-forge update",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageReconcileTheManagedSourceBeforeRerunningRouteMove())
            : new CliNextAction(
                $"open-forge extension update {claim.Owner}",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageUpdateTheOwningExtensionBeforeRerunningRouteMove());
    }

    private static CliNextAction? Next(RouteMoveResult result)
    {
        var first = FirstFinding(result);
        if (first is not null && Action(result, first) is { } action)
        {
            return action;
        }

        if (first is not null && HasNoNext(first.Code))
        {
            return null;
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => new CliNextAction(
                "open-forge cleanup",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteMoveResult()),
            CliSemanticStatus.Incomplete => new CliNextAction(
                "open-forge doctor",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageInspectTheUnavailableInventoryReferenceProjectionOrRecoveryFactsBeforeRelyingOnThisRouteMoveResult()),
            CliSemanticStatus.Blocked when first?.Code is RouteMoveFindingCode.WorkspaceLockUnavailable
                or RouteMoveFindingCode.TargetChanged
                or RouteMoveFindingCode.TargetChangedDuringApply
                => new CliNextAction(
                    "open-forge route move",
                    global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteMoveFromAFreshPlan()),
            CliSemanticStatus.Blocked => new CliNextAction(
                "open-forge doctor",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageInspectTheBlockedWorkspaceRouteOwnershipReferenceGeneratedRegionDestinationOrRecoveryBoundaryBeforeRerunningRouteMove()),
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge route move --help",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageCorrectTheNamedRouteMoveInputThenRerunTheRequest()),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge route move --detail debug",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageReportTheFailureAndRetryTheSameRouteMoveRequestWithBoundedDiagnostics()),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge route move",
                global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageRerunTheSameRouteMoveRequest()),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Move status is not defined."),
        };
    }

    private static bool HasNoNext(RouteMoveFindingCode code)
        => code is RouteMoveFindingCode.SourceNotFound
            or RouteMoveFindingCode.InvalidSubject
            or RouteMoveFindingCode.SelfMove
            or RouteMoveFindingCode.DestinationInsideSource
            or RouteMoveFindingCode.WorkspaceUnavailable
            or RouteMoveFindingCode.WorkspaceUnsafe
            or RouteMoveFindingCode.SourceUnsafe
            or RouteMoveFindingCode.DestinationUnsafe
            or RouteMoveFindingCode.RouteAmbiguous
            or RouteMoveFindingCode.IdentityCollision
            or RouteMoveFindingCode.GeneratedRegionUnsafe
            or RouteMoveFindingCode.RecoveryConflict;

    private static CliEffect Effect(RouteMoveEffect effect, CliDetail detail)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                RouteMoveEffectKind.Directory => CliEffectKind.Directory,
                RouteMoveEffectKind.MovedFile => CliEffectKind.File,
                RouteMoveEffectKind.ReferenceSource => CliEffectKind.Link,
                RouteMoveEffectKind.GeneratedRegion => CliEffectKind.Section,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Route Move effect kind is not defined."),
            },
            Action = effect.Action switch
            {
                RouteMoveEffectAction.Create => CliEffectAction.Created,
                RouteMoveEffectAction.Replace => CliEffectAction.Rewritten,
                RouteMoveEffectAction.Delete => CliEffectAction.Deleted,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Action, "The Route Move effect action is not defined."),
            },
            Outcome = effect.Outcome switch
            {
                RouteMoveEffectOutcome.Planned => CliEffectOutcome.Planned,
                RouteMoveEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                RouteMoveEffectOutcome.Verified => CliEffectOutcome.Done,
                RouteMoveEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                RouteMoveEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Move effect outcome is not defined."),
            },
            Before = detail >= CliDetail.Full ? PathState(effect.Before) : null,
            After = detail >= CliDetail.Full ? PathState(effect.Expected) : null,
        };

    private static CliRecovery Recovery(RouteMoveResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                RouteMoveRecoveryState.NotRequired or RouteMoveRecoveryState.NotCreated
                    => CliRecoveryDisposition.NotRequired,
                RouteMoveRecoveryState.Removed => CliRecoveryDisposition.Removed,
                RouteMoveRecoveryState.Retained => CliRecoveryDisposition.Retained,
                RouteMoveRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State, "The Route Move recovery state is not defined."),
            });

    private static IReadOnlyList<CliCount> Counts(RouteMoveResult result)
    {
        int? scanned = result.References.Coverage is RouteMoveCoverage.NotEstablished
            or RouteMoveCoverage.Blocked
            or RouteMoveCoverage.Interrupted
            ? null
            : result.References.ScannedSourceCount;
        return
        [
            new CliCount("filesMoved", global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelFilesMoved(), MovedFileCount(result)),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), SectionsUpdated(result)),
            new CliCount("linksRewritten", global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelLinksRewritten(), result.References.Rewrites.Length),
            new CliCount(
                "filesScanned",
                global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelFilesScanned(),
                scanned,
                scanned is null ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelTheReferenceScanWasNotCompleted() : null),
        ];
    }

    private static int MovedFileCount(RouteMoveResult result)
    {
        var destinationCreates = result.Effects.Count(effect =>
            effect.Kind == RouteMoveEffectKind.MovedFile &&
            effect.Action == RouteMoveEffectAction.Create &&
            IsProgressed(effect, result.Mode));
        return destinationCreates > 0 || result.Effects.Any(effect =>
                effect.Kind == RouteMoveEffectKind.MovedFile &&
                effect.Action == RouteMoveEffectAction.Create)
            ? destinationCreates
            : result.Status == CliSemanticStatus.Complete
                ? MovedPairs(result).Count + MovedPairs(result).Count(pair => pair.OverwriteFrom is not null)
                : 0;
    }

    private static int SectionsUpdated(RouteMoveResult result)
        => result.Effects.Count(effect =>
            effect.Kind == RouteMoveEffectKind.GeneratedRegion && IsProgressed(effect, result.Mode));

    private static int ProgressedCount(RouteMoveResult result)
        => result.Effects.Count(effect => IsProgressed(effect, result.Mode));

    private static bool IsProgressed(RouteMoveEffect effect, RouteMoveMode mode)
        => mode == RouteMoveMode.DryRun
            ? effect.Outcome == RouteMoveEffectOutcome.Planned
            : effect.Outcome == RouteMoveEffectOutcome.Verified;

    private static IReadOnlyList<MovedPair> MovedPairs(RouteMoveResult result)
    {
        if (result.Subject.Kind == RouteMoveSubjectKind.Category)
        {
            var items = result.Subject.Items
                .Where(item => item.Kind != RouteMoveItemKind.Directory)
                .ToArray();
            var overwrites = items
                .Where(item => item.Layer == RouteMoveLayerKind.Overwrite)
                .ToDictionary(item => item.SourceId ?? item.SourcePath, StringComparer.Ordinal);
            return items
                .Where(item => item.Layer != RouteMoveLayerKind.Overwrite)
                .Select(item => new MovedPair(
                    item.SourcePath,
                    item.DestinationPath,
                    overwrites.GetValueOrDefault(item.SourceId ?? item.SourcePath)?.SourcePath,
                    overwrites.GetValueOrDefault(item.SourceId ?? item.SourcePath)?.DestinationPath))
                .Concat(items
                    .Where(item => item.Layer == RouteMoveLayerKind.Overwrite)
                    .Where(item => !items.Any(baseItem =>
                        baseItem.Layer != RouteMoveLayerKind.Overwrite
                        && baseItem.SourceId is not null
                        && string.Equals(baseItem.SourceId, item.SourceId, StringComparison.Ordinal)))
                    .Select(item => new MovedPair(item.SourcePath, item.DestinationPath, null, null)))
                .OrderBy(pair => pair.From, StringComparer.Ordinal)
                .ToArray();
        }

        var layers = result.Subject.Layers;
        var baseLayer = layers.FirstOrDefault(layer => layer.Layer == RouteMoveLayerKind.Base);
        var overwrite = layers.FirstOrDefault(layer => layer.Layer == RouteMoveLayerKind.Overwrite);
        if (baseLayer is not null)
        {
            return
            [
                new MovedPair(
                    baseLayer.SourcePath,
                    baseLayer.DestinationPath,
                    overwrite?.SourcePath,
                    overwrite?.DestinationPath),
            ];
        }

        return layers.Select(layer => new MovedPair(layer.SourcePath, layer.DestinationPath, null, null)).ToArray();
    }

    private static RouteMoveFinding? FirstFinding(RouteMoveResult result)
        => result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();

    private static string DestinationForHeadline(RouteMoveResult result)
        => result.Subject.Kind == RouteMoveSubjectKind.Category
            ? ParentFolder(result.Destination.Path ?? result.Destination.Requested)
            : result.Destination.Path ?? result.Destination.Requested;

    private static string ParentFolder(string path)
    {
        var normalized = path.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        return slash > 0 ? normalized[..slash] : normalized;
    }

    private static string ParentId(RouteMoveResult result)
    {
        if (result.Destination.ParentId is { } parentId)
        {
            return parentId;
        }

        var folder = ParentFolder(result.Destination.Requested);
        return folder.StartsWith(".agents/", StringComparison.Ordinal)
            ? folder[".agents/".Length..]
            : folder;
    }

    private static bool IsOverwrite(string value)
        => value.EndsWith(".overwrite.md", StringComparison.Ordinal);

    private static string OverwriteName(string value)
    {
        var normalized = value.Replace('\\', '/');
        var fileName = normalized[(normalized.LastIndexOf('/') + 1)..];
        return fileName.EndsWith(".overwrite.md", StringComparison.Ordinal)
            ? fileName[..^".overwrite.md".Length]
            : fileName;
    }

    private static string WorkspaceReason(string cause)
        => cause.Contains("link", StringComparison.OrdinalIgnoreCase)
            ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelALinkLeavesIt()
            : cause.Contains("ambiguous", StringComparison.OrdinalIgnoreCase)
                ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelItsIdentityIsAmbiguous()
                : RouteMoveWording.TrimSentence(cause);

    private static string GeneratedReason(string cause)
        => cause.Contains("missing", StringComparison.OrdinalIgnoreCase)
            ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelItIsMissing()
            : cause.Contains("more than one", StringComparison.OrdinalIgnoreCase)
                ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelThereIsMoreThanOne()
                : cause.Contains("malformed", StringComparison.OrdinalIgnoreCase)
                    ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelItIsMalformed()
                    : RouteMoveWording.TrimSentence(cause);

    private static string OwnershipMessage(RouteMoveResult result, string target)
    {
        var claim = result.Ownership.Claims.FirstOrDefault(claim =>
            string.Equals(claim.Path, target, StringComparison.Ordinal));
        var owner = claim is null
            ? global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelAnotherManager()
            : claim.Manager == RouteMoveOwnershipManager.Framework
                ? global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelTheFramework()
                : global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatTheExtension($"{claim.Owner}");
        return CliFindingWording.OwnershipClaimed(target, owner, global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleRouteMove(), global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.LabelMove());
    }

    private static RouteMoveDataDestination Target(RouteMoveReferenceTarget target)
        => new()
        {
            Id = target.Id,
            Path = target.Path,
        };

    private static string Location(RouteMoveReferenceRewrite rewrite)
        => string.Create(
            CultureInfo.InvariantCulture,
            $":{rewrite.LocationView.Line}:{rewrite.LocationView.Column}");

    private static IReadOnlyList<string> Diagnostics(RouteMoveResult result)
        =>
        new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteMoveWireVocabulary.Name(result.Mode)}",
            $"source={result.Source.Id ?? result.Source.Requested}",
            $"destination={result.Destination.Id ?? result.Destination.Requested}",
            $"subject={(result.Subject.Kind is { } kind ? RouteMoveWireVocabulary.Name(kind) : "none")}",
            $"completeness={RouteMoveWireVocabulary.Name(result.Plan.Completeness)}",
            $"safety={RouteMoveWireVocabulary.Name(result.Plan.Safety)}",
            $"effects={result.Effects.Length.ToString(CultureInfo.InvariantCulture)}",
            $"recovery={HumanState(RouteMoveWireVocabulary.Name(result.Recovery.State))}",
            $"verification={HumanState(RouteMoveWireVocabulary.Name(result.Verification))}",
            $"findings={result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
        }.Concat(result.Findings.Select(finding =>
            $"finding={RouteMoveWireVocabulary.Name(finding.Code)}:target={finding.Target ?? "none"}:cause={finding.Cause}"))
        .ToArray();

    private static string HumanState(string value)
        => value switch
        {
            "not-requested" => "not run",
            "not-started" => "not started",
            _ => value.Replace('-', ' '),
        };

    private sealed record MovedPair(
        string From,
        string To,
        string? OverwriteFrom,
        string? OverwriteTo);
}
