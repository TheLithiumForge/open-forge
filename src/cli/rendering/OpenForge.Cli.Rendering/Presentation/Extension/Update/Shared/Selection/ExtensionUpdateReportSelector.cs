using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Selection;

internal static class ExtensionUpdateReportSelector
{
    internal static CliReport<ExtensionUpdateData> Select(
        ExtensionUpdateResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var selectedIds = SelectedIds(result).ToArray();
        var primaryId = selectedIds.FirstOrDefault()
            ?? result.Findings.FirstOrDefault(finding => finding.Target is not null)?.Target
            ?? result.Packages.FirstOrDefault()?.Id
            ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelected();
        var sourcePath = result.Source?.Path;
        var full = selection.Detail >= CliDetail.Full;
        var standard = selection.Detail >= CliDetail.Standard;
        var projected = ProjectEffects(result, full);
        var kept = result.Comparisons
            .Where(comparison => comparison.IntendedState == ExtensionUpdateComparisonIntendedState.Retired)
            .Where(comparison => !result.Prune)
            .Where(comparison => projected.All(effect => !string.Equals(effect.Path, comparison.Path, StringComparison.Ordinal)))
            .Select(comparison => ProjectKept(comparison, result.Mode == ExtensionUpdateMode.DryRun, full))
            .ToArray();
        var navigation = result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            ? result.GeneratedNavigation?.Regions
                .Where(region => region.State != ExtensionUpdateGeneratedRegionState.Unchanged)
                .Where(region => projected.All(effect => !string.Equals(effect.Path, region.Path, StringComparison.Ordinal)))
                .Select(region => ProjectSection(region, result.Mode == ExtensionUpdateMode.DryRun, full))
                .ToArray() ?? []
            : [];
        var allEffects = projected
            .Concat(kept)
            .Concat(navigation)
            .OrderBy(effect => effect.Path, StringComparer.Ordinal)
            .ToArray();
        var rows = allEffects
            .Select(effect => new ExtensionUpdateDataTextRow(effect.Path, effect.Text, effect.Detail))
            .Concat(full
                ? result.Comparisons
                    .Where(comparison => comparison.CurrentState == ExtensionUpdateComparisonCurrentState.FormatOnly)
                    .Select(comparison => new ExtensionUpdateDataTextRow(
                        comparison.Path,
                        global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnchangedLineEndingsDiffer(),
                        Detail(comparison, null, result)))
                : [])
            .OrderBy(row => row.Path, StringComparer.Ordinal)
            .ToArray();
        var unchanged = result.Comparisons
            .Where(comparison => comparison.IntendedState == ExtensionUpdateComparisonIntendedState.Same
                && comparison.CurrentState is ExtensionUpdateComparisonCurrentState.Same
                    or ExtensionUpdateComparisonCurrentState.FormatOnly)
            .Select(comparison => comparison.Path)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var sections = result.GeneratedNavigation?.Regions
            .Where(region => region.State != ExtensionUpdateGeneratedRegionState.Unchanged)
            .Select(region => region.Path)
            .Order(StringComparer.Ordinal)
            .ToArray() ?? [];
        var dependencies = result.Packages
            .Select(package => new ExtensionUpdateDataDependency
            {
                Id = package.Id,
                Requires = [.. package.Dependencies],
            })
            .ToArray();
        var data = new ExtensionUpdateData
        {
            Mode = Name(result.Mode),
            Force = result.Force,
            Prune = result.Prune,
            Automatic = result.Automatic,
            Source = result.Source is { } source
                ? new ExtensionUpdateDataSource
                {
                    Kind = Name(source.Kind),
                    Path = source.Path,
                }
                : null,
            Packages = result.Packages.Select(package => new ExtensionUpdateDataPackage
            {
                Id = package.Id,
                From = package.FromVersion,
                To = package.ToVersion,
            }).ToArray(),
            PreviousContent = result.Next?.Command == "git diff" ? "git-diff" : null,
            Permissions = Permissions(result),
            Unchanged = standard ? unchanged : null,
            Sections = standard ? sections : null,
            Dependencies = standard ? dependencies : null,
            Effects = full ? allEffects.Select(effect => effect.Data).ToArray() : null,
            TextRows = rows,
            TextDetails = TextDetails(result, selectedIds, unchanged.Length, sections.Length, allEffects.Length > 0, standard, full),
        };

        return new CliReport<ExtensionUpdateData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, selectedIds, primaryId, allEffects.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Preserve)),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspacePath
                ? new CliWorkspaceEcho(workspacePath, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings
                .Select(finding => Finding(result, finding, primaryId))
                .ToArray(),
            Effects = allEffects.Select(effect => effect.Effect).ToArray(),
            Counts = Counts(result, selectedIds, allEffects, unchanged.Length, sections.Length),
            Data = data,
            Recovery = result.Recovery.ResidualPath is { } path
                ? new CliRecovery(path, CliRecoveryDisposition.Retained)
                : null,
            Next = Next(result, selectedIds, primaryId, allEffects),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatStatus($"{CliStatusDefinitions.Read(result.Status).MachineName}"),
                    $"packages={result.Packages.Count}",
                    $"effects={allEffects.Length}",
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatFindings($"{result.Findings.Count}"),
                }
                .Concat(result.Findings.Select(finding => finding.Cause))
                .ToArray()
                : [],
        };
    }

    private static IReadOnlyList<ExtensionUpdateProjectedEffect> ProjectEffects(
        ExtensionUpdateResult result,
        bool full)
    {
        var effects = new List<ExtensionUpdateProjectedEffect>();
        foreach (var effect in result.Effects)
        {
            foreach (var change in effect.Changes)
            {
                var comparison = result.Comparisons.FirstOrDefault(candidate =>
                    string.Equals(candidate.Path, effect.Path, StringComparison.Ordinal)
                    && (candidate.PackageId == effect.PackageId || effect.PackageId is null));
                var planned = effect.Outcome == ExtensionUpdateEffectOutcome.Planned
                    || result.Mode == ExtensionUpdateMode.DryRun && effect.Outcome != ExtensionUpdateEffectOutcome.Verified;
                var text = effect.Outcome == ExtensionUpdateEffectOutcome.NotStarted
                    ? ExtensionUpdateWording.NotStarted()
                    : change.Kind == ExtensionUpdateComparisonTargetKind.GeneratedRegion
                        ? ExtensionUpdateWording.EntriesUpdated()
                        : Row(change.Action, comparison, planned);
                effects.Add(new ExtensionUpdateProjectedEffect
                {
                    Path = effect.Path,
                    ChangeAction = change.Action,
                    Text = text,
                    Detail = full ? Detail(comparison, change, result) : null,
                    Data = new ExtensionUpdateDataEffect
                    {
                        Path = effect.Path,
                        Before = comparison?.CurrentFingerprint,
                        After = comparison?.IntendedFingerprint,
                        Relation = Relation(comparison),
                        Verification = Verification(result.Verification),
                        Recovery = Recovery(result.Recovery),
                    },
                    Effect = new CliEffect
                    {
                        Path = effect.Path,
                        Kind = change.Kind == ExtensionUpdateComparisonTargetKind.GeneratedRegion
                            ? CliEffectKind.Section
                            : CliEffectKind.File,
                        Action = Action(change.Action),
                        Outcome = Outcome(effect.Outcome),
                        Reason = EffectReason(change.Action, change.Kind, comparison),
                        Owner = effect.PackageId,
                        Before = comparison?.CurrentFingerprint,
                        After = comparison?.IntendedFingerprint,
                    },
                });
            }
        }

        return effects;
    }

    private static ExtensionUpdateProjectedEffect ProjectKept(
        ExtensionUpdateComparison comparison,
        bool planned,
        bool full)
        => new()
        {
            Path = comparison.Path,
            ChangeAction = ExtensionUpdateChangeAction.Preserve,
            Text = ExtensionUpdateWording.KeepRow(planned),
            Detail = full ? Detail(comparison, null, null) : null,
            Data = new ExtensionUpdateDataEffect
            {
                Path = comparison.Path,
                Before = comparison.CurrentFingerprint,
                After = null,
                Relation = Relation(comparison),
                Verification = Verification(null),
                Recovery = Recovery(null),
            },
            Effect = new CliEffect
            {
                Path = comparison.Path,
                Kind = CliEffectKind.File,
                Action = CliEffectAction.Kept,
                Outcome = planned ? CliEffectOutcome.Planned : CliEffectOutcome.Done,
                Reason = global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelNoLongerPartOfThePackage(),
                Owner = comparison.PackageId,
                Before = comparison.CurrentFingerprint,
                After = null,
            },
        };

    private static ExtensionUpdateProjectedEffect ProjectSection(
        ExtensionUpdateGeneratedRegion region,
        bool planned,
        bool full)
        => new()
        {
            Path = region.Path,
            ChangeAction = ExtensionUpdateChangeAction.Replace,
            Text = ExtensionUpdateWording.EntriesUpdated(),
            Detail = full ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatState($"{Name(region.State)}") : null,
            Data = new ExtensionUpdateDataEffect
            {
                Path = region.Path,
                Before = null,
                After = null,
                Relation = new ExtensionUpdateDataRelation
                {
                    Current = "unknown",
                    Shipped = Name(region.State),
                },
                Verification = Verification(null),
                Recovery = Recovery(null),
            },
            Effect = new CliEffect
            {
                Path = region.Path,
                Kind = CliEffectKind.Section,
                Action = CliEffectAction.Rewritten,
                Outcome = planned ? CliEffectOutcome.Planned : CliEffectOutcome.Done,
                Reason = ExtensionUpdateWording.EntriesUpdated(),
                Owner = null,
            },
        };

    private static string Row(
        ExtensionUpdateChangeAction action,
        ExtensionUpdateComparison? comparison,
        bool planned)
        => action switch
        {
            ExtensionUpdateChangeAction.Replace => ExtensionUpdateWording.ReplaceRow(RelationText(comparison), planned),
            ExtensionUpdateChangeAction.Restore => ExtensionUpdateWording.RestoreRow(planned),
            ExtensionUpdateChangeAction.Create => ExtensionUpdateWording.CreateRow(planned),
            ExtensionUpdateChangeAction.Delete => ExtensionUpdateWording.DeleteRow(planned),
            ExtensionUpdateChangeAction.Preserve => ExtensionUpdateWording.KeepRow(planned),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Extension Update change action is not defined."),
        };

    private static string RelationText(ExtensionUpdateComparison? comparison)
        => comparison is null
            ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelTheSelectedPackageChangesIt()
            : comparison.CurrentState switch
            {
                ExtensionUpdateComparisonCurrentState.Changed when comparison.IntendedState == ExtensionUpdateComparisonIntendedState.Changed
                    => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelYouHadChangedItAndThisVersionChangesIt(),
                ExtensionUpdateComparisonCurrentState.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelYouHadChangedIt(),
                ExtensionUpdateComparisonCurrentState.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelItWasMissing(),
                _ when comparison.IntendedState == ExtensionUpdateComparisonIntendedState.Changed => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNewContentInThisVersion(),
                _ => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelTheSelectedPackageChangesIt(),
            };

    private static string EffectReason(
        ExtensionUpdateChangeAction action,
        ExtensionUpdateComparisonTargetKind kind,
        ExtensionUpdateComparison? comparison)
        => kind == ExtensionUpdateComparisonTargetKind.GeneratedRegion
            ? ExtensionUpdateWording.EntriesUpdated()
            : action switch
            {
                ExtensionUpdateChangeAction.Restore => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelItWasMissing(),
                ExtensionUpdateChangeAction.Create => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNewContentInThisVersion(),
                ExtensionUpdateChangeAction.Delete or ExtensionUpdateChangeAction.Preserve => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelNoLongerPartOfThePackage(),
                _ => RelationText(comparison),
            };

    private static ExtensionUpdateDataRelation Relation(ExtensionUpdateComparison? comparison)
        => comparison is null
            ? new() { Current = "unknown", Shipped = "unknown" }
            : new()
            {
                Current = Name(comparison.CurrentState),
                Shipped = Name(comparison.IntendedState),
            };

    private static string? Detail(
        ExtensionUpdateComparison? comparison,
        ExtensionUpdateLogicalChange? change,
        ExtensionUpdateResult? result)
    {
        if (comparison is null)
        {
            return change is null ? null : $"kind: {Name(change.Kind)}";
        }

        var parts = new List<string>
        {
            global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatBeforeSha256($"{comparison.CurrentFingerprint ?? "unavailable"}"),
            global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatAfterSha256($"{comparison.IntendedFingerprint ?? "unavailable"}"),
        };
        if (comparison.SourceAssetPath is { } source)
        {
            parts.Add($"source asset: {source}");
        }
        if (change is { Region: { } region })
        {
            parts.Add(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatRegion($"{region}"));
        }
        if (result is { } observed)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatVerification($"{VerificationText(observed.Verification)}"));
            parts.Add($"recovery: {RecoveryText(observed.Recovery)}");
        }

        return string.Join("; ", parts);
    }

    private static CliEffectAction Action(ExtensionUpdateChangeAction action) => action switch
    {
        ExtensionUpdateChangeAction.Create => CliEffectAction.Created,
        ExtensionUpdateChangeAction.Replace => CliEffectAction.Replaced,
        ExtensionUpdateChangeAction.Restore => CliEffectAction.Restored,
        ExtensionUpdateChangeAction.Delete => CliEffectAction.Deleted,
        ExtensionUpdateChangeAction.Preserve => CliEffectAction.Kept,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Extension Update change action is not defined."),
    };

    private static CliEffectOutcome Outcome(ExtensionUpdateEffectOutcome outcome) => outcome switch
    {
        ExtensionUpdateEffectOutcome.Planned => CliEffectOutcome.Planned,
        ExtensionUpdateEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
        ExtensionUpdateEffectOutcome.Verified => CliEffectOutcome.Done,
        ExtensionUpdateEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
        ExtensionUpdateEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Extension Update effect outcome is not defined."),
    };

    private static ExtensionUpdateDataPermissions Permissions(ExtensionUpdateResult result)
        => new()
        {
            Required = [.. result.RequiredPermissions],
            Missing = [.. result.MissingPermissions],
            Decision = Name(result.PermissionDecision),
            Action = Name(result.PermissionAction),
            Outcome = Name(result.PermissionOutcome),
        };

    private static ExtensionUpdateDataVerification Verification(ExtensionUpdateVerification? verification)
        => verification is null
            ? new() { Targets = "not-requested", Topology = "not-requested", ExtensionRecord = "not-requested" }
            : new()
            {
                Targets = Name(verification.Targets),
                Topology = Name(verification.Topology),
                ExtensionRecord = Name(verification.ExtensionsLifecycle),
            };

    private static ExtensionUpdateDataRecovery Recovery(ExtensionUpdateRecovery? recovery)
        => recovery is null
            ? new() { State = "not-requested", Path = null }
            : new()
            {
                State = Name(recovery.State),
                Path = recovery.ResidualPath,
            };

    private static string VerificationText(ExtensionUpdateVerification verification)
        => ExtensionUpdateWording.Verification(
            VerificationWord(verification.Targets),
            VerificationWord(verification.Topology),
            VerificationWord(verification.ExtensionsLifecycle));

    private static string RecoveryText(ExtensionUpdateRecovery recovery)
        => ExtensionUpdateWording.Recovery(
            RecoveryWord(recovery.State),
            recovery.ResidualPath);

    private static string VerificationWord(ExtensionUpdateVerificationState value) => value switch
    {
        ExtensionUpdateVerificationState.NotRequested => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNotRequested(),
        ExtensionUpdateVerificationState.Planned => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelPlannedButNotChecked(),
        ExtensionUpdateVerificationState.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerified(),
        ExtensionUpdateVerificationState.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
        ExtensionUpdateVerificationState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown(),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Extension Update verification state is not defined."),
    };

    private static string RecoveryWord(ExtensionUpdateRecoveryState value) => value switch
    {
        ExtensionUpdateRecoveryState.NotRequired => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNotRequired(),
        ExtensionUpdateRecoveryState.NotCreated => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNotCreated(),
        ExtensionUpdateRecoveryState.Removed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRemoved(),
        ExtensionUpdateRecoveryState.Retained => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelRetained(),
        ExtensionUpdateRecoveryState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown(),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Extension Update recovery state is not defined."),
    };

    private static IReadOnlyList<string> TextDetails(
        ExtensionUpdateResult result,
        IReadOnlyList<string> selectedIds,
        int unchanged,
        int sections,
        bool hasWork,
        bool standard,
        bool full)
    {
        var details = new List<string>();
        if (result.Next?.Command == "git diff")
        {
            details.Add(ExtensionUpdateWording.PreviousContent("git diff"));
        }
        else if (result.Recovery.ResidualPath is { } recoveryPath)
        {
            details.Add(ExtensionUpdateWording.PreviousContent(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatRecoveryBundleAt($"{recoveryPath}")));
        }

        if (GrantPaths(result) is { Count: > 0 } grants
            && (Name(result.PermissionOutcome) == "verified"
                || result.Mode == ExtensionUpdateMode.DryRun && Name(result.PermissionOutcome) == "planned"))
        {
            details.AddRange(grants.Select(path => ExtensionUpdateWording.SavedGrant(path, result.Mode == ExtensionUpdateMode.DryRun)));
        }

        if (standard)
        {
            if (result.Source is { } source)
            {
                details.Add(ExtensionUpdateWording.Source(source.Path ?? source.Identity));
            }

            var packageLines = result.Packages
                .Select(package => package.SelectedRoot
                    ? package.Id
                    : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatDependency($"{package.Id}"))
                .ToArray();
            details.Add(ExtensionUpdateWording.Packages(packageLines));
            details.Add(ExtensionUpdateWording.Unchanged(unchanged));
            if (sections > 0)
            {
                details.Add(ExtensionUpdateWording.Sections(sections));
            }

            details.Add(ExtensionUpdateWording.Grant(
                result.RequiredPermissions,
                result.MissingPermissions,
                Name(result.PermissionDecision),
                Name(result.PermissionAction),
                Name(result.PermissionOutcome)));
        }

        if (full)
        {
            if (hasWork)
            {
                details.Add(ExtensionUpdateWording.Verification(
                    VerificationWord(result.Verification.Targets),
                    VerificationWord(result.Verification.Topology),
                    VerificationWord(result.Verification.ExtensionsLifecycle)));
                details.Add(ExtensionUpdateWording.Recovery(
                    RecoveryWord(result.Recovery.State),
                    result.Recovery.ResidualPath));
            }
            else
            {
                details.Add(global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoVerificationWasNeeded());
                details.Add(ExtensionUpdateWording.Recovery(
                    RecoveryWord(result.Recovery.State),
                    result.Recovery.ResidualPath));
            }
        }

        return details;
    }

    private static CliHeadline Headline(
        ExtensionUpdateResult result,
        IReadOnlyList<string> selectedIds,
        string primaryId,
        int kept)
    {
        var finding = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        var changed = result.Effects.Count > 0;
        var count = selectedIds.Count == 0 ? result.Packages.Count : selectedIds.Count;
        var version = result.Packages.FirstOrDefault(package => package.Id == primaryId)?.ToVersion ?? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelTheSelectedVersion();
        if (result.Status == CliSemanticStatus.Complete && !changed && kept == 0)
        {
            return count > 1
                ? new(ExtensionUpdateWording.AllUpToDate(count), CliHeadlineKind.NothingToDo)
                : new(ExtensionUpdateWording.UpToDate(primaryId), CliHeadlineKind.NothingToDo);
        }

        if (result.Status == CliSemanticStatus.Complete && result.Mode == ExtensionUpdateMode.DryRun)
        {
            return new(ExtensionUpdateWording.WouldUpdate(primaryId, version), CliHeadlineKind.Preview);
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete when kept > 0
                => new(ExtensionUpdateWording.UpdatedWithKept(primaryId, version, kept), CliHeadlineKind.Warnings),
            CliSemanticStatus.Complete when count > 1
                => new(ExtensionUpdateWording.Updated(count), CliHeadlineKind.Done),
            CliSemanticStatus.Complete
                => new(ExtensionUpdateWording.Updated(primaryId, version), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when changed && result.Mode == ExtensionUpdateMode.DryRun
                => new(ExtensionUpdateWording.WouldUpdateWithKept(primaryId, version, kept), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when changed
                => new(ExtensionUpdateWording.UpdatedWithKept(primaryId, version, kept), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when result.Findings.Any(finding => finding.Code == ExtensionUpdateFindingCode.LifecycleObservation)
                => new(ExtensionUpdateWording.OwnershipUnknown(primaryId), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(ExtensionUpdateWording.UpToDateWithKept(primaryId, kept), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(ExtensionUpdateWording.Incomplete(primaryId, FindingMessage(result, finding, primaryId)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(ExtensionUpdateWording.CannotUpdate(HeadlineReason(result, finding, primaryId)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(ExtensionUpdateWording.Blocked(primaryId, BlockedReason(result, finding, primaryId)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(ExtensionUpdateWording.Failed(CompletedChanges(result), result.Effects.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(ExtensionUpdateWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Extension Update status is not defined."),
        };
    }

    private static string FindingMessage(
        ExtensionUpdateResult result,
        ExtensionUpdateFinding? finding,
        string id)
        => finding is null
            ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageTheExtensionUpdateCouldNotBeClassified()
            : ExtensionUpdateWording.FindingMessage(
                finding,
                id,
                result.Source?.Path,
                result.Recovery.ResidualPath);

    private static string HeadlineReason(
        ExtensionUpdateResult result,
        ExtensionUpdateFinding? finding,
        string id)
    {
        if (finding?.Code == ExtensionUpdateFindingCode.InvalidInput)
        {
            return finding.Cause;
        }

        return FindingMessage(result, finding, id);
    }

    private static string BlockedReason(
        ExtensionUpdateResult result,
        ExtensionUpdateFinding? finding,
        string id)
    {
        var message = FindingMessage(result, finding, id);
        const string prefix = "Cannot update: ";
        return message.StartsWith(prefix, StringComparison.Ordinal)
            ? message[prefix.Length..]
            : message;
    }

    private static string? HeadlineFindingCode(ExtensionUpdateResult result)
        => result.Status is (CliSemanticStatus.Invalid
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted)
            && result.Findings.Count(finding => finding.Status == result.Status) == 1
            ? ExtensionUpdateWording.FindingCode(result.Findings.First(finding => finding.Status == result.Status).Code)
            : null;

    private static CliFinding Finding(
        ExtensionUpdateResult result,
        ExtensionUpdateFinding finding,
        string id)
    {
        var kind = finding.Code switch
        {
            ExtensionUpdateFindingCode.SourceUnavailable
                or ExtensionUpdateFindingCode.SourceInvalid
                or ExtensionUpdateFindingCode.SourceOverlap
                or ExtensionUpdateFindingCode.SourceIdentityConflict => CliSubjectKind.Source,
            ExtensionUpdateFindingCode.TargetUnsafe
                or ExtensionUpdateFindingCode.ProjectionUnavailable
                or ExtensionUpdateFindingCode.GeneratedRegionUnsafe
                or ExtensionUpdateFindingCode.TargetChanged
                or ExtensionUpdateFindingCode.TopologyVerificationFailed => CliSubjectKind.File,
            _ when finding.Target is { } target
                && (target.Contains('/', StringComparison.Ordinal) || Path.IsPathRooted(target)) => CliSubjectKind.File,
            _ => CliSubjectKind.Identifier,
        };
        var subject = kind == CliSubjectKind.Source
            ? result.Source?.Path ?? finding.Target ?? id
            : finding.Target ?? id;
        var actions = finding.Code switch
        {
            ExtensionUpdateFindingCode.SelectionRequired => new[]
            {
                new CliNextAction(
                    "open-forge extension list --installed",
                    global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageListTheInstalledExtensionsThenRerunTheRequestWithAnExplicitSelection()),
            },
            ExtensionUpdateFindingCode.SourceIdentityConflict when finding.Target is { } conflictId => new[]
            {
                new CliNextAction(
                    $"open-forge extension inspect {conflictId}",
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageInspectTheInstalledExtensionIdentityBeforeRetryingTheUpdate()),
            },
            ExtensionUpdateFindingCode.LifecycleObservation => new[]
            {
                new CliNextAction(
                    "open-forge extension list --installed",
                    global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageListTheInstalledExtensionsThenRerunTheRequestWithAnExplicitSelection()),
            },
            ExtensionUpdateFindingCode.TopologyVerificationFailed
                or ExtensionUpdateFindingCode.RecoveryUnavailable
                or ExtensionUpdateFindingCode.RecoveryFailed
                or ExtensionUpdateFindingCode.PermissionWriteFailed => new[]
            {
                new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageInspectTheReportedExtensionState()),
            },
            ExtensionUpdateFindingCode.RecoveryArtifactRetained => new[]
            {
                new CliNextAction("open-forge cleanup", global::OpenForge.Cli.OutputText.Shared.SharedText.MessageReviewAndRemoveTheReportedRecoveryBundle()),
            },
            ExtensionUpdateFindingCode.SettingsInvalid or ExtensionUpdateFindingCode.SettingsUnavailable => new[]
            {
                new CliNextAction(
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageRepairSettingsThenRerunUpdate(),
                    "Repair workspace settings before updating an Extension.")
                { Kind = CliNextActionKind.Sentence },
            },
            ExtensionUpdateFindingCode.RemovedExtension => new[]
            {
                new CliNextAction(
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageRemoveExtensionFromSettingsThenRerunUpdate(),
                    "Workspace removal settings currently exclude this Extension.")
                { Kind = CliNextActionKind.Sentence },
            },
            ExtensionUpdateFindingCode.BulkExcluded => new[]
            {
                new CliNextAction(
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageRemoveExtensionFromSettingsThenRerunUpdate(),
                    "Workspace removal settings excluded this Extension from the bulk update.")
                { Kind = CliNextActionKind.Sentence },
            },
            ExtensionUpdateFindingCode.PathExcluded or ExtensionUpdateFindingCode.ExcludedAncestor => new[]
            {
                new CliNextAction(
                    global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageRemovePathFromSettingsThenRerunUpdate(),
                    "Workspace removal settings currently exclude this Extension path.")
                { Kind = CliNextActionKind.Sentence },
            },
            _ => [],
        };
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = ExtensionUpdateWording.FindingCode(finding.Code),
            Title = ExtensionUpdateWording.FindingTitle(finding.Code),
            Message = ExtensionUpdateWording.FindingMessage(
                finding,
                id,
                result.Source?.Path,
                result.Recovery.ResidualPath),
            Subject = new CliSubject(
                kind,
                kind is CliSubjectKind.File or CliSubjectKind.Source ? subject : null,
                kind == CliSubjectKind.Identifier ? subject : null),
            Resolution = FindingResolution(finding, actions),
            Actions = actions,
            Evidence = [new CliEvidence("cause", finding.Cause)],
            Provenance = result.Source is { } source
                ? new CliProvenance(source.Identity, source.Path)
                : null,
        };
    }

    private static CliResolution? FindingResolution(
        ExtensionUpdateFinding finding,
        IReadOnlyList<CliNextAction> actions)
    {
        if (finding.Code == ExtensionUpdateFindingCode.PathExcluded
            || finding.Status == CliSemanticStatus.Attention)
        {
            return CliResolution.Informational;
        }

        if (actions.Any(action => action.Kind == CliNextActionKind.Command))
        {
            return CliResolution.TargetedOperation;
        }

        return finding.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Failed
            ? CliResolution.ManualDecision
            : null;
    }

    private static CliNextAction? Next(
        ExtensionUpdateResult result,
        IReadOnlyList<string> selectedIds,
        string primaryId,
        IReadOnlyList<ExtensionUpdateProjectedEffect> effects)
    {
        if (result.Findings.Any(finding => finding.Code == ExtensionUpdateFindingCode.SelectionRequired))
        {
            return new CliNextAction(
                "open-forge extension list --installed",
                global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.MessageListTheInstalledExtensionsThenRerunTheRequestWithAnExplicitSelection());
        }

        if (result.Findings.Any(finding => finding.Code == ExtensionUpdateFindingCode.ConfirmationRequired))
        {
            return new CliNextAction(
                "open-forge extension update --automatic",
                global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageRerunExtensionUpdateWithAutomaticOrUseAnInteractiveTerminalToSupplyConfirmation());
        }

        var kept = effects.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Preserve);
        if (kept > 0)
        {
            return new CliNextAction(
                $"open-forge extension update {primaryId} --prune --dry-run",
                global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelPreviewDeletingTheKeptFile());
        }

        if (result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted
            && result.Recovery.ResidualPath is not null)
        {
            return new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageInspectThePartialUpdateAndItsRecoveryBundle());
        }

        return result.Next;
    }

    private static IReadOnlyList<CliCount> Counts(
        ExtensionUpdateResult result,
        IReadOnlyList<string> selectedIds,
        IReadOnlyList<ExtensionUpdateProjectedEffect> effects,
        int unchanged,
        int sections)
    {
        var counted = effects
            .Where(effect => effect.Effect.Kind == CliEffectKind.File)
            .Where(effect => effect.Effect.Outcome is CliEffectOutcome.Planned or CliEffectOutcome.Done)
            .ToArray();
        return
        [
            new CliCount("packagesUpdated", global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelPackagesUpdated(), selectedIds.Count == 0 ? result.Packages.Count : selectedIds.Count),
            new CliCount("filesReplaced", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesReplaced(), counted.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Replace)),
            new CliCount("filesRestored", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesRestored(), counted.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Restore)),
            new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), counted.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Create)),
            new CliCount("filesDeleted", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesDeleted(), counted.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Delete)),
            new CliCount("filesKept", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesKept(), counted.Count(effect => effect.ChangeAction == ExtensionUpdateChangeAction.Preserve)),
            new CliCount("filesUnchanged", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesUnchanged(), unchanged),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionsUpdated(), sections),
        ];
    }

    private static int CompletedChanges(ExtensionUpdateResult result)
        => result.Effects.Count(effect => effect.Outcome == ExtensionUpdateEffectOutcome.Verified);

    private static IReadOnlyList<string>? GrantPaths(ExtensionUpdateResult result)
    {
        var paths = result.RequiredPermissions.Length > 0
            ? result.RequiredPermissions
            : result.MissingPermissions;
        return paths.Length == 0 ? null : [.. paths.Order(StringComparer.Ordinal)];
    }

    private static IReadOnlyList<string> SelectedIds(ExtensionUpdateResult result)
        => result.Selection is { } selection
            ? selection.SelectedBy is ExtensionUpdateSelectionKind.ExplicitAll or ExtensionUpdateSelectionKind.InteractiveAll
                ? selection.FrozenIds ?? result.Packages.Where(package => package.SelectedRoot).Select(package => package.Id).ToArray()
                : selection.RootIds
            : [];

    private static string Name(Enum value)
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    private sealed record ExtensionUpdateProjectedEffect
    {
        internal required string Path { get; init; }
        internal required ExtensionUpdateChangeAction ChangeAction { get; init; }
        internal required string Text { get; init; }
        internal required string? Detail { get; init; }
        internal required CliEffect Effect { get; init; }
        internal required ExtensionUpdateDataEffect Data { get; init; }
    }
}
