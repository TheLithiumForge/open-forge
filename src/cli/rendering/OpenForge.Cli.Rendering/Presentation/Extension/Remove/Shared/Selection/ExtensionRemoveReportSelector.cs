using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Selection;

internal static class ExtensionRemoveReportSelector
{
    internal static CliReport<ExtensionRemoveData> Select(
        ExtensionRemoveResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var ids = result.Selection?.Ids.ToArray() ?? [];
        var primaryId = ids.FirstOrDefault()
            ?? result.Findings.FirstOrDefault(finding => finding.Target is not null)?.Target
            ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelected();
        var effects = result.Effects.ToLookup(effect => effect.Path, StringComparer.Ordinal);
        var hasPathEffects = result.Effects.Any(effect => effect.Kind is ExtensionRemoveEffectKind.PackageFile
            or ExtensionRemoveEffectKind.GeneratedRegion);
        var includePathRows = result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            || hasPathEffects;
        var rows = !includePathRows
            ? []
            : result.Paths
                .Select(path => ProjectPath(result, path, effects[path.Path], selection.Detail))
                .ToList();
        if (result.GeneratedNavigation is { } navigation
            && (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
                || result.Effects.Any(effect => effect.Kind == ExtensionRemoveEffectKind.GeneratedRegion)))
        {
            rows.AddRange(navigation.Regions
                .Where(region => selection.Detail >= CliDetail.Standard
                    || region.State != ExtensionRemoveGeneratedRegionState.Unchanged)
                .Select(region => ProjectNavigation(result, region.Path, region.State, selection.Detail)));
        }

        rows.InsertRange(
            0,
            result.Effects
                .Where(effect => effect.Kind == ExtensionRemoveEffectKind.Settings)
                .Select(effect => ProjectSettings(effect, selection.Detail)));
        rows.InsertRange(
            0,
            result.Effects
                .Where(effect => effect.Kind == ExtensionRemoveEffectKind.Directory)
                .Select(effect => ProjectDirectory(effect, selection.Detail)));

        var unchanged = result.GeneratedNavigation?.Regions
            .Where(region => region.State == ExtensionRemoveGeneratedRegionState.Unchanged)
            .Select(region => region.Path)
            .ToArray() ?? [];
        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var data = new ExtensionRemoveData
        {
            Mode = EnumName(result.Mode),
            Automatic = result.Automatic,
            Packages = ids.Select(id => new ExtensionRemoveDataPackage { Id = id }).ToArray(),
            Orphaned = result.Dependencies?.RetainedOrphanDependencyIds.ToArray() ?? [],
            Permissions = Permissions(result),
            Effects = rows.Select(row => row.Data).ToArray(),
            RemovalOrder = standard ? result.Dependencies?.RemovalOrder.ToArray() ?? [] : null,
            EntriesUnchanged = full ? unchanged : null,
            Verification = full ? Verification(result.Verification) : null,
            Recovery = full ? Recovery(result.Recovery) : null,
            TextRows = rows.Select(row => new ExtensionRemoveDataTextRow(row.Data.Path, row.Data.Text, row.Data.Detail)).ToArray(),
            TextDetails = TextDetails(result, unchanged, standard, full),
        };

        return new CliReport<ExtensionRemoveData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, primaryId),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(result, finding, primaryId)).ToArray(),
            Effects = [],
            Counts = Counts(result, rows),
            Data = data,
            Recovery = result.Recovery.ResidualPath is { } recovery
                ? new CliRecovery(recovery, CliRecoveryDisposition.Retained)
                : null,
            Next = Next(result, primaryId),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"packages={ids.Length}",
                    $"effects={rows.Count}",
                    $"findings={result.Findings.Count}",
                }.Concat(result.Findings.Select(finding => finding.Cause)).ToArray()
                : [],
        };
    }

    private static ExtensionRemoveProjectedRow ProjectPath(
        ExtensionRemoveResult result,
        ExtensionRemovePathPlan path,
        IEnumerable<ExtensionRemoveEffect> effects,
        CliDetail detail)
    {
        var effect = effects.FirstOrDefault();
        var outcome = effect?.Outcome ?? ReadUnpairedPathOutcome(result, path);
        var text = path.Action switch
        {
            ExtensionRemovePathAction.Delete => ExtensionRemoveWording.DeleteRow(outcome),
            ExtensionRemovePathAction.RetainShared => ExtensionRemoveWording.KeepRow(path.RemainingOwnerIds),
            ExtensionRemovePathAction.ReleaseOwnership => ExtensionRemoveWording.ReleaseRow(outcome),
            _ => throw new ArgumentOutOfRangeException(nameof(path), path.Action, "The Extension Remove path action is not defined."),
        };
        return new ExtensionRemoveProjectedRow
        {
            Data = new ExtensionRemoveDataEffect
            {
                Path = path.Path,
                Action = EnumName(path.Action),
                Outcome = ReadPathOutcomeName(path, effect, outcome),
                Owner = path.SelectedOwnerIds.FirstOrDefault(),
                KeptFor = path.RemainingOwnerIds.ToArray(),
                OwnersBefore = detail >= CliDetail.Standard ? path.SelectedOwnerIds.ToArray() : null,
                OwnersAfter = detail >= CliDetail.Standard ? path.RemainingOwnerIds.ToArray() : null,
                Text = text,
                Detail = detail >= CliDetail.Standard
                    ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatOwnersBeforeOwnersAfter($"{ExtensionRemoveWording.Owners(path.SelectedOwnerIds)}", $"{ExtensionRemoveWording.Owners(path.RemainingOwnerIds)}")
                    : null,
            },
        };
    }

    private static ExtensionRemoveEffectOutcome ReadUnpairedPathOutcome(
        ExtensionRemoveResult result,
        ExtensionRemovePathPlan path)
    {
        if (result.Mode == ExtensionRemoveMode.DryRun)
        {
            return ExtensionRemoveEffectOutcome.Planned;
        }

        if (path.Action == ExtensionRemovePathAction.ReleaseOwnership)
        {
            return result.Lifecycle.Outcome switch
            {
                ExtensionRemoveLifecycleOutcome.Planned => ExtensionRemoveEffectOutcome.Planned,
                ExtensionRemoveLifecycleOutcome.NotStarted => ExtensionRemoveEffectOutcome.NotStarted,
                ExtensionRemoveLifecycleOutcome.Verified or ExtensionRemoveLifecycleOutcome.AlreadyCurrent
                    => ExtensionRemoveEffectOutcome.Verified,
                ExtensionRemoveLifecycleOutcome.VerificationFailed => ExtensionRemoveEffectOutcome.VerificationFailed,
                ExtensionRemoveLifecycleOutcome.CompletionUnknown => ExtensionRemoveEffectOutcome.CompletionUnknown,
                ExtensionRemoveLifecycleOutcome.NotRequested => ExtensionRemoveEffectOutcome.NotStarted,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Lifecycle.Outcome, "The Extension Remove lifecycle outcome is not defined."),
            };
        }

        if (result.Status is CliSemanticStatus.Blocked
            or CliSemanticStatus.Invalid
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted)
        {
            return ExtensionRemoveEffectOutcome.NotStarted;
        }

        return ExtensionRemoveEffectOutcome.Verified;
    }

    private static string ReadPathOutcomeName(
        ExtensionRemovePathPlan path,
        ExtensionRemoveEffect? effect,
        ExtensionRemoveEffectOutcome outcome)
    {
        if (effect is not null || path.Action != ExtensionRemovePathAction.RetainShared)
        {
            return EnumName(outcome);
        }

        return EnumName(path.Action);
    }

    private static ExtensionRemoveProjectedRow ProjectNavigation(
        ExtensionRemoveResult result,
        string path,
        ExtensionRemoveGeneratedRegionState state,
        CliDetail detail)
    {
        var unchanged = state == ExtensionRemoveGeneratedRegionState.Unchanged;
        var effect = result.Effects.FirstOrDefault(candidate =>
            candidate.Kind == ExtensionRemoveEffectKind.GeneratedRegion
                && string.Equals(candidate.Path, path, StringComparison.Ordinal));
        var outcome = effect?.Outcome ?? (result.Mode == ExtensionRemoveMode.DryRun
            ? ExtensionRemoveEffectOutcome.Planned
            : ExtensionRemoveEffectOutcome.Verified);
        return new ExtensionRemoveProjectedRow
        {
            Data = new ExtensionRemoveDataEffect
            {
                Path = path,
                Action = unchanged ? "unchanged" : "updated",
                Outcome = unchanged ? EnumName(state) : EnumName(outcome),
                Owner = null,
                KeptFor = [],
                OwnersBefore = detail >= CliDetail.Standard ? [] : null,
                OwnersAfter = detail >= CliDetail.Standard ? [] : null,
                Text = unchanged
                    ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelUnchanged()
                    : ExtensionRemoveWording.EntriesUpdate(path, outcome),
            },
        };
    }

    private static ExtensionRemoveProjectedRow ProjectSettings(
        ExtensionRemoveEffect effect,
        CliDetail detail)
        => new()
        {
            Data = new ExtensionRemoveDataEffect
            {
                Path = effect.Path,
                Action = EnumName(effect.Action),
                Outcome = EnumName(effect.Outcome),
                Owner = null,
                KeptFor = [],
                OwnersBefore = detail >= CliDetail.Standard ? [] : null,
                OwnersAfter = detail >= CliDetail.Standard ? [] : null,
                Text = ExtensionRemoveWording.SettingsEffect(
                    effect.Outcome),
                Detail = null,
            },
        };

    private static ExtensionRemoveProjectedRow ProjectDirectory(
        ExtensionRemoveEffect effect,
        CliDetail detail)
        => new()
        {
            Data = new ExtensionRemoveDataEffect
            {
                Path = effect.Path,
                Action = EnumName(effect.Action),
                Outcome = EnumName(effect.Outcome),
                Owner = null,
                KeptFor = [],
                OwnersBefore = detail >= CliDetail.Standard ? [] : null,
                OwnersAfter = detail >= CliDetail.Standard ? [] : null,
                Text = ExtensionRemoveWording.DirectoryEffect(
                    effect.Outcome),
                Detail = null,
            },
        };

    private static CliHeadline Headline(ExtensionRemoveResult result, string id)
    {
        var finding = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        if (result.Status == CliSemanticStatus.Complete
            && ((result.Paths.Count == 0 && result.Effects.Count == 0)
                || result.Findings.Any(finding => finding.Code == ExtensionRemoveFindingCode.OwnershipObservation)))
        {
            return new(ExtensionRemoveWording.NoFilesRecorded(id), CliHeadlineKind.NothingToDo);
        }

        if (result.Status == CliSemanticStatus.Complete && result.Mode == ExtensionRemoveMode.DryRun)
        {
            return new(ExtensionRemoveWording.WouldRemove(id), CliHeadlineKind.Preview);
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => new(ExtensionRemoveWording.Removed(result.Selection?.Ids ?? [id]), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(AttentionHeadline(result, id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(
                finding is { Code: ExtensionRemoveFindingCode.LifecycleUnavailable } lifecycle
                    ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatOwnershipRecordUnavailable(lifecycle.Cause)
                    : ExtensionRemoveWording.Incomplete(id, finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRemovalCouldNotBeChecked()),
                CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(ExtensionRemoveWording.CannotRemove(finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheInputIsInvalid()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(ExtensionRemoveWording.Blocked(
                id,
                finding?.Code switch
                {
                    ExtensionRemoveFindingCode.DependencyBlocked
                        => ExtensionRemoveWording.DependencyBlockedReason(finding.Cause, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRemovalIsBlocked()),
                    ExtensionRemoveFindingCode.WorkspaceLockUnavailable
                        => ExtensionRemoveWording.LockUnavailable(),
                    _ => finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRemovalIsBlocked(),
                }), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(ExtensionRemoveWording.Failed(CompletedChanges(result), result.Effects.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(ExtensionRemoveWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Extension Remove status is not defined."),
        };
    }

    private static string AttentionHeadline(ExtensionRemoveResult result, string id)
    {
        var dependency = result.Findings.FirstOrDefault(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleObservation)?.Target;
        return dependency is null
            ? ExtensionRemoveWording.Removed(result.Selection?.Ids ?? [id])
            : ExtensionRemoveWording.Warning(id, dependency);
    }

    private static string? HeadlineFindingCode(ExtensionRemoveResult result)
        => result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted
            ? result.Findings.Count(finding => finding.Status == result.Status) == 1
                ? ExtensionRemoveWording.FindingCode(result.Findings.First(finding => finding.Status == result.Status).Code)
                : null
            : null;

    private static CliFinding Finding(ExtensionRemoveResult result, ExtensionRemoveFinding finding, string id)
    {
        var subject = finding.Target ?? id;
        var kind = finding.Target is null ? CliSubjectKind.Identifier : CliSubjectKind.File;
        var action = finding.Code switch
        {
            ExtensionRemoveFindingCode.SelectionRequired => new CliNextAction("open-forge extension list", global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageListTheInstalledExtensionsThenRerunTheRequestWithAnExplicitSelection()),
            ExtensionRemoveFindingCode.SettingsInvalid or ExtensionRemoveFindingCode.SettingsUnavailable => new CliNextAction(
                ExtensionRemoveWording.FindingMessage(finding, id),
                ExtensionRemoveWording.FindingMessage(finding, id))
            { Kind = CliNextActionKind.Sentence },
            ExtensionRemoveFindingCode.PathExcluded => new CliNextAction(
                global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageClearCoveringExclusionsThenRunIndex(),
                global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageClearCoveringExclusionsThenRunIndex())
            { Kind = CliNextActionKind.Sentence },
            ExtensionRemoveFindingCode.LifecycleObservation when finding.Target is { } dependency => new CliNextAction($"open-forge extension remove {dependency}", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageRemoveTheUnusedDependencyWhenItIsNoLongerNeeded()),
            ExtensionRemoveFindingCode.RecoveryArtifactRetained => new CliNextAction("open-forge cleanup", global::OpenForge.Cli.OutputText.Shared.SharedText.MessageReviewAndRemoveTheReportedRecoveryBundle()),
            _ => null,
        };
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = ExtensionRemoveWording.FindingCode(finding.Code),
            Title = ExtensionRemoveWording.FindingTitle(finding.Code),
            Message = ExtensionRemoveWording.FindingMessage(finding, id),
            Subject = new CliSubject(kind, kind == CliSubjectKind.File ? subject : null, kind == CliSubjectKind.Identifier ? subject : null),
            Resolution = finding.Status == CliSemanticStatus.Attention
                || finding.Code == ExtensionRemoveFindingCode.PathExcluded
                ? CliResolution.Informational
                : null,
            Actions = action is null ? [] : [action],
        };
    }

    private static CliNextAction? Next(ExtensionRemoveResult result, string primaryId)
    {
        var settingsFinding = result.Findings.FirstOrDefault(finding =>
            finding.Code is ExtensionRemoveFindingCode.SettingsInvalid
                or ExtensionRemoveFindingCode.SettingsUnavailable);
        if (settingsFinding is not null)
        {
            var instruction = ExtensionRemoveWording.FindingMessage(settingsFinding, primaryId);
            return new CliNextAction(instruction, instruction)
            { Kind = CliNextActionKind.Sentence };
        }

        if (result.Recovery.ResidualPath is not null)
        {
            if (result.Status != CliSemanticStatus.Attention
                || result.Findings.FirstOrDefault(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleObservation) is null)
            {
                return new CliNextAction("open-forge cleanup", ExtensionRemoveWording.NextCleanup());
            }
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Findings.FirstOrDefault(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleObservation) is { Target: { } dependency })
        {
            return new CliNextAction($"open-forge extension remove {dependency}", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageRemoveTheUnusedDependencyWhenItIsNoLongerNeeded());
        }

        return result.Status switch
        {
            CliSemanticStatus.Invalid => new CliNextAction("open-forge extension remove --help", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageCorrectTheExtensionRemoveInputThenRerunTheRequest()),
            CliSemanticStatus.Incomplete => new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageInspectTheReportedExtensionState()),
            _ => null,
        };
    }

    private static IReadOnlyList<CliCount> Counts(
        ExtensionRemoveResult result,
        IReadOnlyList<ExtensionRemoveProjectedRow> rows)
        =>
        [
            new CliCount("packagesRemoved", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelPackagesRemoved(), ReadRemovedPackageCount(result)),
            new CliCount("filesDeleted", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesDeleted(), rows.Count(row => row.Data.Action == "delete"
                && row.Data.Outcome is "planned" or "verified")),
            new CliCount("filesKept", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesKept(), rows.Count(row => row.Data.Action == "retain-shared")),
            new CliCount("filesReleased", global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelFilesReleased(), rows.Count(row => row.Data.Action == "release-ownership"
                && row.Data.Outcome is "planned" or "verified")),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionsUpdated(), rows.Count(row => row.Data.Action == "updated"
                && row.Data.Outcome is "planned" or "verified")),
        ];

    private static int ReadRemovedPackageCount(ExtensionRemoveResult result)
    {
        if (result.Selection is null)
        {
            return 0;
        }

        var removalCompleted = result.Lifecycle.Action == ExtensionRemoveLifecycleAction.Publish
            ? result.Lifecycle.Outcome == ExtensionRemoveLifecycleOutcome.Verified
            : result.Effects.Any(effect => effect.Kind == ExtensionRemoveEffectKind.Settings
                && effect.Outcome == ExtensionRemoveEffectOutcome.Verified);
        var removalPlanned = result.Mode == ExtensionRemoveMode.DryRun
            && (result.Lifecycle.Action == ExtensionRemoveLifecycleAction.Publish
                ? result.Lifecycle.Outcome == ExtensionRemoveLifecycleOutcome.Planned
                : result.Effects.Any(effect => effect.Kind == ExtensionRemoveEffectKind.Settings
                    && effect.Outcome == ExtensionRemoveEffectOutcome.Planned));
        return removalCompleted || removalPlanned ? result.Selection.Ids.Count : 0;
    }

    private static ExtensionRemoveDataPermissions Permissions(ExtensionRemoveResult result)
        => new()
        {
            Required = [.. result.RequiredPermissions],
            Missing = [.. result.MissingPermissions],
            Decision = EnumName(result.PermissionDecision),
            Action = EnumName(result.PermissionAction),
            Outcome = EnumName(result.PermissionOutcome),
        };

    private static ExtensionRemoveDataVerification Verification(ExtensionRemoveVerification verification)
        => new()
        {
            Targets = EnumName(verification.Targets),
            Entries = EnumName(verification.Topology),
            ExtensionRecord = EnumName(verification.ExtensionsLifecycle),
        };

    private static ExtensionRemoveDataRecovery Recovery(ExtensionRemoveRecovery recovery)
        => new()
        {
            State = EnumName(recovery.State),
            ProtectedPaths = [.. recovery.ProtectedPaths],
            Path = recovery.ResidualPath,
        };

    private static IReadOnlyList<string> TextDetails(
        ExtensionRemoveResult result,
        IReadOnlyList<string> unchanged,
        bool standard,
        bool full)
    {
        var details = new List<string>();
        if (standard)
        {
            if (result.Dependencies is { RemovalOrder.Count: > 0 } dependencies)
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatRemovalOrder($"{string.Join(", ", dependencies.RemovalOrder)}"));
            }

            details.Add($".agents/open-forge.lock.json  {ExtensionRemoveWording.Lock(result.Lifecycle)}");
        }
        if (full)
        {
            details.Add(ExtensionRemoveWording.EntriesUnchanged(unchanged));
            details.Add(ExtensionRemoveWording.Verification(result.Verification));
            details.Add(ExtensionRemoveWording.RecoveryFacts(result.Recovery));
        }

        if (result.Recovery.ResidualPath is { } path)
        {
            details.Add(ExtensionRemoveWording.Recovery(path));
        }

        return details;
    }

    private static int CompletedChanges(ExtensionRemoveResult result)
        => result.Effects.Count(effect => effect.Outcome == ExtensionRemoveEffectOutcome.Verified);

    private static string EnumName(Enum value)
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    private sealed record ExtensionRemoveProjectedRow
    {
        internal required ExtensionRemoveDataEffect Data { get; init; }
    }
}
