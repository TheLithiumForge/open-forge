using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Presentation.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Selection;

internal static class UpdateReportSelector
{
    internal static CliReport<UpdateData> Select(UpdateResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var preview = result.Mode == UpdateMode.DryRun;
        var includeDetails = selection.Detail >= CliDetail.Full;
        var effects = result.Effects
            .Select(effect => Project(effect, result, includeDetails))
            .ToArray();
        var unchanged = Unchanged(result, effects);
        var entries = Entries(result);
        var migrations = result.Migrations;
        var keptFiles = result.Findings.Count(finding => IsCode(finding, "RetiredContentPreserved"));
        var userOwnedSourcePaths = migrations
            .Where(migration => migration.IsUserOwnedSource)
            .Select(migration => migration.Path)
            .ToHashSet(StringComparer.Ordinal);
        var fileEffects = effects
            .Where(effect => !effect.IsDirectory && !userOwnedSourcePaths.Contains(effect.Path))
            .ToArray();
        var countedFileEffects = fileEffects.Where(effect => IsCounted(effect, preview)).ToArray();
        var changedFiles = countedFileEffects.Length;
        var replacedFiles = countedFileEffects.Count(effect => effect.IsWholeFileReplacement);
        var restoredFiles = countedFileEffects.Count(effect => effect.IsRestore);
        var createdFiles = countedFileEffects.Count(effect => effect.ResultAction == UpdatePhysicalEffectAction.Create);
        var deletedFiles = countedFileEffects.Count(effect => effect.ResultAction == UpdatePhysicalEffectAction.Delete);
        var updatedSections = entries.Count(entry => effects.Any(effect => effect.Path == entry.Path && IsCounted(effect, preview)));
        var completedChanges = effects.Count(effect => effect.ResultOutcome == UpdatePhysicalEffectOutcome.Verified);
        var suppressInterruptedFinding = result.Status == CliSemanticStatus.Interrupted && completedChanges == 0;
        var previousContent = result.PreviousContentAvailable ? "git-diff" : null;
        var migrationSummaryInHeadline = result.Status == CliSemanticStatus.Complete
            && migrations.Count > 0
            && changedFiles == 0;
        var data = new UpdateData
        {
            Mode = CliReportVocabulary.Name(result.Mode),
            Force = result.Force,
            Prune = result.Prune,
            Automatic = result.Automatic,
            PreviousContent = previousContent,
            LockPath = UpdateWording.OwnershipRecordPath,
            Unchanged = selection.Detail >= CliDetail.Standard ? unchanged : null,
            EntriesSections = selection.Detail >= CliDetail.Standard ? entries : null,
            Effects = includeDetails ? effects : null,
            Verification = includeDetails ? CliReportVocabulary.Name(result.Verification) : null,
            Migrations = migrations.Count == 0 ? null : migrations.Select(ProjectMigration).ToArray(),
            TextRows = TextRows(result, effects, unchanged, entries, selection.Detail, preview),
            TextSummaryLines = TextSummaryLines(
                result,
                effects,
                unchanged,
                previousContent,
                selection.Detail,
                preview,
                includeMigrationSummary: !migrationSummaryInHeadline),
            TextDetailLines = TextDetailLines(result, effects, selection.Detail),
            ShowNoChanges = preview && effects.Length > 0,
            SuppressInterruptedFinding = suppressInterruptedFinding,
            ChangedFiles = changedFiles,
            ReplacedFiles = replacedFiles,
            RestoredFiles = restoredFiles,
            CreatedFiles = createdFiles,
            DeletedFiles = deletedFiles,
            KeptFiles = keptFiles,
            UnchangedFiles = unchanged.Length,
            UpdatedSections = updatedSections,
            CompletedChanges = completedChanges,
            TotalChanges = effects.Length,
            Status = result.Status,
            ResultMode = result.Mode,
            RecoveryState = result.Recovery.State,
            RecoveryPath = result.Recovery.ResidualPath,
            VerificationState = result.Verification,
            IsNoOp = effects.Length == 0 && keptFiles == 0 && migrations.Count == 0,
        };

        return new CliReport<UpdateData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, data),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = Workspace(result),
            Findings = result.Findings
                .Select(finding => Finding(result, finding, data))
                .ToArray(),
            Effects = effects.Select(Effect).ToArray(),
            Counts =
            [
                new CliCount("filesReplaced", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesReplaced(), replacedFiles),
                new CliCount("filesRestored", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesRestored(), restoredFiles),
                new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), createdFiles),
                new CliCount("filesDeleted", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesDeleted(), deletedFiles),
                new CliCount("filesKept", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesKept(), keptFiles),
                new CliCount("filesUnchanged", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesUnchanged(), unchanged.Length),
                new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), updatedSections),
            ],
            Data = data,
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data)
                : [],
        };
    }

    private static UpdateDataEffect Project(UpdatePhysicalEffect effect, UpdateResult result, bool includeDetails)
    {
        var isUserOwnedMigration = result.Migrations.Any(migration =>
            migration.Path == effect.Path && migration.IsUserOwnedSource);
        var comparison = isUserOwnedMigration ? null : Comparison(effect, result);
        var isRestore = effect.Changes.Any(change => change.Action == UpdateLogicalChangeAction.Restore);
        var isSection = !isUserOwnedMigration
            && effect.Changes.Any(change => change.Kind != UpdateComparisonTargetKind.File);
        var source = !isUserOwnedMigration && result.Source is { } sourceFacts
            ? new UpdateDataSource
            {
                Id = sourceFacts.Id,
                Version = sourceFacts.Version,
                Fingerprint = sourceFacts.InventoryFingerprint,
            }
            : null;

        return new UpdateDataEffect
        {
            Path = effect.Path,
            Before = includeDetails ? comparison?.CurrentFingerprint : null,
            After = includeDetails ? comparison?.IntendedFingerprint : null,
            SourceAssetPath = includeDetails && !isUserOwnedMigration
                ? comparison?.SourceAssetPath ?? effect.Changes.Select(change => change.SourceAssetPath).FirstOrDefault(path => path is not null)
                : null,
            Relation = new UpdateDataRelation
            {
                Current = comparison is null ? "unknown" : CliReportVocabulary.Name(comparison.CurrentState),
                Shipped = comparison is null ? "unknown" : CliReportVocabulary.Name(comparison.IntendedState),
            },
            Source = source,
            Verification = CliReportVocabulary.Name(result.Verification),
            ResultAction = effect.Action,
            ResultOutcome = effect.Outcome,
            ResultResidual = effect.Residual,
            IsRestore = isRestore,
            IsSection = isSection,
            IsWholeFileReplacement = effect.Action == UpdatePhysicalEffectAction.Replace
                && effect.Changes.Any(change => change.Kind == UpdateComparisonTargetKind.File
                    && change.Action == UpdateLogicalChangeAction.Replace),
            HasManagedSection = effect.Changes.Any(change => change.Kind == UpdateComparisonTargetKind.ManagedRegion),
            AddsManagedSection = effect.Changes.Any(change => change.Kind == UpdateComparisonTargetKind.ManagedRegion
                && change.Action is UpdateLogicalChangeAction.Create or UpdateLogicalChangeAction.Restore),
            IsDirectory = effect.Kind == UpdatePhysicalEffectKind.Directory,
            Reason = isUserOwnedMigration ? null : Reason(effect, comparison),
        };
    }

    private static UpdateComparison? Comparison(UpdatePhysicalEffect effect, UpdateResult result)
    {
        foreach (var change in effect.Changes)
        {
            var exact = result.Comparisons.FirstOrDefault(comparison =>
                comparison.RelativePath == effect.Path
                && comparison.Kind == change.Kind
                && comparison.RegionIdentity == change.Region
                && (change.SourceAssetPath is null || comparison.SourceAssetPath == change.SourceAssetPath));
            if (exact is not null)
            {
                return exact;
            }
        }

        return result.Comparisons.FirstOrDefault(comparison => comparison.RelativePath == effect.Path);
    }

    private static string? Reason(UpdatePhysicalEffect effect, UpdateComparison? comparison)
    {
        if (effect.Kind == UpdatePhysicalEffectKind.Directory)
        {
            return null;
        }

        var change = effect.Changes.FirstOrDefault();
        if (change?.Action == UpdateLogicalChangeAction.Restore
            || comparison?.CurrentState == UpdateComparisonCurrentState.Missing)
        {
            return UpdateWording.MissingReason();
        }

        if (change?.Action == UpdateLogicalChangeAction.Create
            || comparison?.IntendedState == UpdateComparisonIntendedState.New)
        {
            return UpdateWording.NewReason();
        }

        if (change?.Action == UpdateLogicalChangeAction.Delete
            || comparison?.IntendedState == UpdateComparisonIntendedState.Retired)
        {
            return UpdateWording.RetiredReason();
        }

        if (comparison is { CurrentState: UpdateComparisonCurrentState.Changed, IntendedState: UpdateComparisonIntendedState.Changed })
        {
            return UpdateWording.ChangedBothReason();
        }

        if (comparison is { CurrentState: UpdateComparisonCurrentState.Changed })
        {
            return UpdateWording.ChangedReason();
        }

        if (comparison is { IntendedState: UpdateComparisonIntendedState.Changed })
        {
            return UpdateWording.NewReleaseReason();
        }

        if (effect.Changes.Any(change => change.Kind == UpdateComparisonTargetKind.GeneratedRegion))
        {
            return UpdateWording.EntriesSectionUpdated();
        }

        return null;
    }

    private static UpdateDataPath[] Unchanged(UpdateResult result, IReadOnlyList<UpdateDataEffect> effects)
    {
        var changed = effects.Select(effect => effect.Path).ToHashSet(StringComparer.Ordinal);
        return result.Comparisons
            .Where(comparison => !changed.Contains(comparison.RelativePath)
                && comparison.IntendedState == UpdateComparisonIntendedState.Same
                && comparison.CurrentState is UpdateComparisonCurrentState.Same or UpdateComparisonCurrentState.FormatOnly)
            .Select(comparison => comparison.RelativePath)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(path => new UpdateDataPath { Path = path })
            .ToArray();
    }

    private static UpdateDataEntrySection[] Entries(UpdateResult result)
        => result.GeneratedNavigation?.Regions
            .Where(region => region.State != UpdateGeneratedNavigationRegionState.Unchanged)
            .Select(region => new UpdateDataEntrySection
            {
                Path = region.Path,
                State = CliReportVocabulary.Name(region.State),
            })
            .ToArray()
            ?? [];

    private static UpdateDataMigration ProjectMigration(UpdateMigration migration)
        => new()
        {
            Path = migration.Path,
            Actions = migration.Actions.Select(ActionMachineName).ToArray(),
            Fields = migration.Fields.ToArray(),
            Derivation = migration.Derivation.Select(value => new UpdateDataMigrationDerivation
            {
                Field = value.Field,
                Source = DerivationSourceMachineName(value.Source),
            }).ToArray(),
            Outcome = global::OpenForge.Cli.Core.Commands.Update.UpdateDefinitions.ReadMachineName(migration.Outcome),
        };

    private static UpdateDataTextRow MigrationTextRow(UpdateMigration migration)
    {
        var actions = migration.Actions.Select(ActionMachineName);
        var formattedActions = global::OpenForge.Cli.OutputText.Update.UpdateText.FormatMigrationActions(actions);
        return new UpdateDataTextRow(
            migration.Path,
            global::OpenForge.Cli.OutputText.Update.UpdateText.FormatMigrationRow(
                migration.Outcome == UpdateMigrationOutcome.Applied,
                formattedActions));
    }

    private static IReadOnlyList<string> MigrationSummaryLines(IReadOnlyList<UpdateMigration> migrations)
    {
        var lines = new List<string>();
        foreach (var outcome in new[] { UpdateMigrationOutcome.Applied, UpdateMigrationOutcome.Planned })
        {
            var count = migrations.Count(migration => migration.Outcome == outcome);
            if (count > 0)
            {
                lines.Add(global::OpenForge.Cli.OutputText.Update.UpdateText.FormatMigrationSummary(
                    outcome == UpdateMigrationOutcome.Applied,
                    count));
            }
        }

        return lines;
    }

    private static CliHeadline MigrationHeadline(IReadOnlyList<UpdateMigration> migrations)
    {
        var applied = migrations.Count(migration => migration.Outcome == UpdateMigrationOutcome.Applied);
        var planned = migrations.Count(migration => migration.Outcome == UpdateMigrationOutcome.Planned);
        var wording = global::OpenForge.Cli.OutputText.Update.UpdateText.FormatMigrationHeadline(applied, planned);
        return new(wording, planned > 0 && applied == 0 ? CliHeadlineKind.Preview : CliHeadlineKind.Done);
    }

    private static string ActionMachineName(WorkspaceAdoptionAction action)
        => action switch
        {
            WorkspaceAdoptionAction.MetadataCompleted => "metadata-completed",
            WorkspaceAdoptionAction.EntrypointCreated => "entrypoint-created",
            WorkspaceAdoptionAction.EntriesSectionAdded => "entries-section-added",
            WorkspaceAdoptionAction.NavigationUpdated => "navigation-updated",
            WorkspaceAdoptionAction.ContentPreserved => "content-preserved",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Update migration action is not defined."),
        };

    private static string DerivationSourceMachineName(WorkspaceAdoptionDerivationSource source)
        => source switch
        {
            WorkspaceAdoptionDerivationSource.ExistingDescription => "existing-description",
            WorkspaceAdoptionDerivationSource.ExistingTitle => "existing-title",
            WorkspaceAdoptionDerivationSource.Heading => "heading",
            WorkspaceAdoptionDerivationSource.RelativePath => "relative-path",
            WorkspaceAdoptionDerivationSource.DirectoryName => "directory-name",
            WorkspaceAdoptionDerivationSource.RequiredTag => "required-tag",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "The Update migration derivation source is not defined."),
        };

    private static IReadOnlyList<UpdateDataTextRow> TextRows(
        UpdateResult result,
        IReadOnlyList<UpdateDataEffect> effects,
        IReadOnlyList<UpdateDataPath> unchanged,
        IReadOnlyList<UpdateDataEntrySection> entries,
        CliDetail detail,
        bool preview)
    {
        var suppressEffectRows = result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete;
        if (suppressEffectRows && result.Migrations.Count == 0)
        {
            return [];
        }

        var migrationsByPath = result.Migrations.ToDictionary(migration => migration.Path, StringComparer.Ordinal);
        var rows = new List<UpdateDataTextRow>();
        if (suppressEffectRows)
        {
            rows.AddRange(result.Migrations.Select(MigrationTextRow));
        }
        else
        {
            foreach (var effect in effects)
            {
                if (migrationsByPath.TryGetValue(effect.Path, out var migration))
                {
                    rows.Add(MigrationTextRow(migration));
                    continue;
                }

                rows.Add(new UpdateDataTextRow(effect.Path, RowWording(effect, preview, result.Recovery.State)));
            }

            var effectPaths = effects.Select(effect => effect.Path).ToHashSet(StringComparer.Ordinal);
            rows.AddRange(result.Migrations
                .Where(migration => !effectPaths.Contains(migration.Path))
                .Select(MigrationTextRow));
            if (result.Status == CliSemanticStatus.Attention)
            {
                rows.AddRange(result.Findings
                    .Where(finding => IsCode(finding, "RetiredContentPreserved") && finding.Target is not null)
                    .Select(finding => new UpdateDataTextRow(finding.Target!, UpdateWording.RetainedRow())));
            }

            if (detail >= CliDetail.Standard)
            {
                rows.AddRange(entries
                    .Where(entry => !migrationsByPath.ContainsKey(entry.Path) && !effectPaths.Contains(entry.Path))
                    .Select(entry => new UpdateDataTextRow(
                        entry.Path,
                        global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted())));

                if (result.Lifecycle.Action == UpdateLifecycleAction.Publish && effects.Count > 0)
                {
                    rows.Add(new UpdateDataTextRow(UpdateWording.OwnershipRecordPath, OwnershipWording(result.Lifecycle.Outcome)));
                }

                if (detail == CliDetail.Standard && unchanged.Count > 0)
                {
                    rows.Add(new UpdateDataTextRow(string.Empty, UpdateWording.UnchangedCount(unchanged.Count)));
                }
            }

            if (detail >= CliDetail.Full)
            {
                rows.AddRange(result.Comparisons
                    .Where(comparison => comparison.CurrentState == UpdateComparisonCurrentState.FormatOnly
                        && comparison.IntendedState == UpdateComparisonIntendedState.Same)
                    .Select(comparison => new UpdateDataTextRow(
                        comparison.RelativePath,
                        UpdateWording.FormatOnlyRow())));
            }
        }

        return rows;
    }

    private static bool IsCounted(UpdateDataEffect effect, bool preview)
        => preview
            ? effect.ResultOutcome == UpdatePhysicalEffectOutcome.Planned
            : effect.ResultOutcome == UpdatePhysicalEffectOutcome.Verified;

    private static string RowWording(UpdateDataEffect effect, bool preview, UpdateRecoveryState recoveryState)
    {
        if (effect.ResultOutcome == UpdatePhysicalEffectOutcome.NotStarted)
        {
            return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();
        }
        if (effect.ResultOutcome == UpdatePhysicalEffectOutcome.CompletionUnknown)
        {
            return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown();
        }
        if (effect.ResultOutcome == UpdatePhysicalEffectOutcome.VerificationFailed)
        {
            return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed();
        }
        preview = preview || effect.ResultOutcome == UpdatePhysicalEffectOutcome.Planned;
        if (effect.IsWholeFileReplacement)
        {
            return CliChangeWording.Replaced(preview, recoveryKept: preview || recoveryState == UpdateRecoveryState.Retained);
        }
        if (effect.HasManagedSection)
        {
            if (effect.ResultAction == UpdatePhysicalEffectAction.Create)
            {
                return global::OpenForge.Cli.OutputText.Update.UpdateText.CreatedWithSection(preview);
            }
            return effect.AddsManagedSection
                ? CliChangeWording.AddedOpenForgeSection(preview)
                : CliChangeWording.OpenForgeSection(preview);
        }
        if (effect.ResultAction == UpdatePhysicalEffectAction.Create)
        {
            return effect.IsRestore
                ? global::OpenForge.Cli.OutputText.Update.UpdateText.Restored(preview)
                : CliChangeWording.Created(preview);
        }
        if (effect.IsSection)
        {
            return CliChangeWording.Entries(preview);
        }
        if (effect.IsRestore)
        {
            return global::OpenForge.Cli.OutputText.Update.UpdateText.Restored(preview);
        }
        var action = effect.ResultAction switch
        {
            UpdatePhysicalEffectAction.Delete => global::OpenForge.Cli.OutputText.Update.UpdateText.Deleted(preview),
            UpdatePhysicalEffectAction.Replace => CliChangeWording.Replaced(preview,
                recoveryKept: preview || recoveryState == UpdateRecoveryState.Retained),
            _ => throw new ArgumentOutOfRangeException(nameof(effect)),
        };
        return effect.Reason is null ? action : $"{action} ({effect.Reason})";
    }

    private static string OwnershipWording(UpdateLifecycleOutcome outcome)
        => outcome switch
        {
            UpdateLifecycleOutcome.Planned => CliChangeWording.OwnershipRecord(preview: true),
            UpdateLifecycleOutcome.Verified => CliChangeWording.OwnershipRecord(preview: false),
            UpdateLifecycleOutcome.AlreadyCurrent => CliChangeWording.OwnershipRecordUnchanged(),
            UpdateLifecycleOutcome.NotRequested or UpdateLifecycleOutcome.NotStarted
                or UpdateLifecycleOutcome.VerificationFailed => CliChangeWording.OwnershipRecordNotUpdated(),
            UpdateLifecycleOutcome.CompletionUnknown => CliChangeWording.OwnershipRecordUnconfirmed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static IReadOnlyList<string> TextSummaryLines(
        UpdateResult result,
        IReadOnlyList<UpdateDataEffect> effects,
        IReadOnlyList<UpdateDataPath> unchanged,
        string? previousContent,
        CliDetail detail,
        bool preview,
        bool includeMigrationSummary)
    {
        if (result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete)
        {
            return MigrationSummaryLines(result.Migrations);
        }

        var lines = new List<string>();
        if (includeMigrationSummary)
        {
            lines.AddRange(MigrationSummaryLines(result.Migrations));
        }
        if (previousContent == "git-diff")
        {
            lines.Add(UpdateWording.PreviousContent());
        }

        if (detail >= CliDetail.Full && unchanged.Count > 0)
        {
            lines.Add(UpdateWording.UnchangedCount(unchanged.Count));
        }

        if (result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted
            && result.Recovery.State == UpdateRecoveryState.Retained
            && result.Recovery.ResidualPath is { } recovery)
        {
            lines.Add(UpdateWording.RecoveryData(recovery));
        }

        return lines;
    }

    private static IReadOnlyList<string> TextDetailLines(UpdateResult result, IReadOnlyList<UpdateDataEffect> effects, CliDetail detail)
    {
        if (detail < CliDetail.Full)
        {
            return [];
        }

        var lines = new List<string>();
        foreach (var effect in effects)
        {
            if (effect.Before is { } before)
            {
                lines.Add(UpdateWording.CurrentHash(effect.Path, before));
            }

            if (effect.After is { } after)
            {
                lines.Add(UpdateWording.ShippedHash(effect.Path, after));
            }

            if (effect.SourceAssetPath is { } source)
            {
                lines.Add(UpdateWording.SourceAsset(effect.Path, source));
            }

            if (effect.Source is { } sourceFacts)
            {
                lines.Add(UpdateWording.SourceFacts(effect.Path, sourceFacts));
            }
        }

        lines.Add(UpdateWording.Verification(result.Verification));
        if (result.Recovery.ResidualPath is { } recovery)
        {
            lines.Add(UpdateWording.RecoveryData(recovery));
        }

        return lines;
    }

    private static CliHeadline Headline(UpdateResult result, UpdateData data)
    {
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        var noOwnership = result.Findings.Any(finding => IsCode(finding, "OwnershipObservation"));
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Migrations.Count > 0 && data.ChangedFiles == 0
                => MigrationHeadline(result.Migrations),
            CliSemanticStatus.Complete when noOwnership => new(UpdateWording.NoOwnership(), CliHeadlineKind.Done),
            CliSemanticStatus.Complete when data.IsNoOp => new(UpdateWording.UpToDate(), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when data.ResultMode == UpdateMode.DryRun => new(UpdateWording.WouldUpdate(data.ChangedFiles), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete => new(UpdateWording.Updated(data.ChangedFiles), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when data.ChangedFiles == 0 => new(UpdateWording.UpToDateWithKept(data.KeptFiles), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention => new(UpdateWording.UpdatedWithKept(data.ChangedFiles, data.KeptFiles), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(UpdateWording.Incomplete(first?.Cause ?? global::OpenForge.Cli.OutputText.Update.UpdateText.LabelRequiredInformationWasUnavailable()), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid when first is not null && IsCode(first, "ConfirmationRequired") => new(CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Update.UpdateText.TitleUpdate()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Invalid => new(UpdateWording.Blocked(first?.Cause ?? global::OpenForge.Cli.OutputText.Update.UpdateText.LabelTheInputWasInvalid()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(UpdateWording.Blocked(first?.Cause ?? global::OpenForge.Cli.OutputText.Update.UpdateText.LabelTheWorkspaceIsBlocked()), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(UpdateWording.Failed(data.CompletedChanges, data.TotalChanges), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted when data.SuppressInterruptedFinding => new(UpdateWording.Cancelled(), CliHeadlineKind.Cancelled),
            CliSemanticStatus.Interrupted => new(UpdateWording.CancelledAfter(data.CompletedChanges, data.TotalChanges), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Update status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(UpdateResult result)
    {
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status == CliSemanticStatus.Invalid
            && first is not null && IsCode(first, "ConfirmationRequired")
            ? UpdateWording.MachineCode(first.Code)
            : null;
    }

    private static CliFinding Finding(UpdateResult result, UpdateFinding finding, UpdateData data)
    {
        var path = finding.Target ?? UpdateWording.OwnershipRecordPath;
        var subjectKind = SubjectKind(finding);
        var action = Action(result, finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = UpdateWording.MachineCode(finding.Code),
            Title = UpdateWording.FindingTitle(finding.Code),
            Message = Message(result, finding, data, path),
            Subject = new CliSubject(subjectKind, subjectKind == CliSubjectKind.Identifier ? null : path, subjectKind == CliSubjectKind.Identifier ? path : null),
            Resolution = Resolution(finding),
            Actions = action is null ? [] : [action],
            Evidence = CodeName(finding) == "WriteFailed"
                ? [new CliEvidence("cause", finding.Cause)]
                : [],
        };
    }

    private static CliSubjectKind SubjectKind(UpdateFinding finding)
        => CodeName(finding) switch
        {
            "InvalidInput" or "ConfirmationRequired" => CliSubjectKind.Identifier,
            "TargetUnavailable"
                or "TargetUnsafe"
                or "SourceProvenanceInvalid"
                or "FingerprintUnsupported"
                or "RetiredContentPreserved"
                or "RetirementIneligible"
                or "GeneratedRegionUnsafe"
                or "PlanBlocked"
                or "RecoveryConflict"
                or "VerificationFailed"
                or "WriteFailed" => CliSubjectKind.File,
            _ => CliSubjectKind.Workspace,
        };

    private static string Message(UpdateResult result, UpdateFinding finding, UpdateData data, string path)
        => CodeName(finding) switch
        {
            "InvalidInput" => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdate(), TrimSentence(finding.Cause)),
            "ConfirmationRequired" => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Update.UpdateText.TitleUpdate()),
            "WorkspaceUnavailable" => CliFindingWording.WorkspaceUnavailable(path),
            "WorkspaceUnsafe" => CliFindingWording.WorkspaceUnsafe(path, TrimSentence(finding.Cause)),
            "PayloadUnavailable" => CliFindingWording.PayloadUnavailable(),
            "PayloadInvalid" => CliFindingWording.PayloadInvalid(),
            "LifecycleMissing" => UpdateWording.LifecycleMissing(),
            "LifecycleUnavailable" => CliFindingWording.LifecycleUnavailable(),
            "LifecycleBlocked" => CliFindingWording.LifecycleBlocked(TrimSentence(finding.Cause)),
            "OwnershipObservation" => UpdateWording.OwnershipObservation(),
            "OwnershipConflict" => CliFindingWording.CauseSentence(finding.Cause),
            "TargetUnavailable" => UpdateWording.TargetUnavailable(path),
            "TargetUnsafe" => CliFindingWording.TargetUnsafe(path, TrimSentence(finding.Cause)),
            "SourceProvenanceInvalid" => UpdateWording.SourceProvenanceInvalid(path),
            "FingerprintUnsupported" => UpdateWording.FingerprintUnsupported(path),
            "RetiredContentPreserved" => UpdateWording.RetiredPreserved(path),
            "RetirementIneligible" => UpdateWording.RetirementIneligible(path, TrimSentence(finding.Cause)),
            "ProjectionUnavailable" => CliFindingWording.ProjectionUnavailable(path, TrimSentence(finding.Cause)),
            "GeneratedRegionUnsafe" => CliFindingWording.GeneratedRegionUnsafe(path, TrimSentence(finding.Cause)),
            "PlanBlocked" => UpdateWording.PlanBlocked(path, TrimSentence(finding.Cause)),
            "RecoveryConflict" => CliFindingWording.RecoveryConflict(path),
            "RecoveryUnavailable" => CliFindingWording.RecoveryUnavailable(path),
            "RecoveryArtifactRetained" when data.RecoveryPath is { } recovery => CliFindingWording.RecoveryRetained(recovery),
            "RecoveryArtifactRetained" => CliFindingWording.CauseSentence(finding.Cause),
            "WriteFailed" => UpdateWording.WriteFailed(path, finding.Cause),
            "VerificationFailed" => UpdateWording.VerificationFailed(path, finding.Cause),
            "LifecyclePublicationFailed" => CliFindingWording.LifecyclePublicationFailed(),
            "RecoveryFailed" => CliFindingWording.RecoveryFailed(),
            "OperationFailed" => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Update.UpdateText.TitleUpdate(), TrimSentence(finding.Cause)),
            "Interrupted" => data.SuppressInterruptedFinding
                ? UpdateWording.Cancelled()
                : UpdateWording.CancelledAfter(data.CompletedChanges, data.TotalChanges),
            _ => CliFindingWording.CauseSentence(finding.Cause),
        };

    private static CliResolution? Resolution(UpdateFinding finding)
        => CodeName(finding) switch
        {
            "ConfirmationRequired" or "RetiredContentPreserved" => CliResolution.TargetedOperation,
            "PayloadUnavailable"
                or "LifecycleUnavailable"
                or "ProjectionUnavailable"
                or "RecoveryUnavailable" => CliResolution.BlockedRepair,
            _ => null,
        };

    private static CliNextAction? Action(UpdateResult result, UpdateFinding finding)
        => CodeName(finding) switch
        {
            "ConfirmationRequired" => new CliNextAction(
                UpdateWording.NextConfirmationCommand(result),
                UpdateWording.NextConfirmationReason()),
            "RetiredContentPreserved" => new CliNextAction(
                UpdateWording.NextRetiredCommand(),
                UpdateWording.NextRetiredReason()),
            "RecoveryArtifactRetained" => new CliNextAction(
                UpdateWording.NextRecoveryCommand(),
                UpdateWording.NextRecoveryReason()),
            "InvalidInput" => new CliNextAction(UpdateWording.HelpCommand, UpdateWording.NextHelpReason()),
            "Interrupted" => new CliNextAction(UpdateWording.NextRetryCommand(result), UpdateWording.NextRetryReason()),
            _ when result.Status is CliSemanticStatus.Incomplete or CliSemanticStatus.Failed or CliSemanticStatus.Blocked
                => new CliNextAction(UpdateWording.NextDoctorCommand(), UpdateWording.NextDoctorReason()),
            _ => null,
        };

    private static CliNextAction? Next(UpdateResult result)
    {
        if (result.Status == CliSemanticStatus.Failed
            || (result.Status == CliSemanticStatus.Interrupted
                && result.Recovery.State == UpdateRecoveryState.Retained))
        {
            return new CliNextAction(UpdateWording.NextDoctorCommand(), UpdateWording.NextDoctorReason());
        }

        if (result.Status == CliSemanticStatus.Complete
            && result.Findings.Any(finding => IsCode(finding, "OwnershipObservation")))
        {
            return new CliNextAction(UpdateWording.NextDoctorCommand(), UpdateWording.NextDoctorReason());
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Findings.Any(finding => IsCode(finding, "RecoveryArtifactRetained")))
        {
            return new CliNextAction(UpdateWording.NextRecoveryCommand(), UpdateWording.NextRecoveryReason());
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Findings.Any(finding => IsCode(finding, "RetiredContentPreserved")))
        {
            return new CliNextAction(UpdateWording.NextRetiredCommand(), UpdateWording.NextRetiredReason());
        }

        return result.Next;
    }

    private static CliEffect Effect(UpdateDataEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = effect switch
            {
                { IsDirectory: true } => CliEffectKind.Directory,
                { IsSection: true } => CliEffectKind.Section,
                _ => CliEffectKind.File,
            },
            Action = effect.IsRestore
                ? CliEffectAction.Restored
                : effect.ResultAction switch
                {
                    UpdatePhysicalEffectAction.Create => CliEffectAction.Created,
                    UpdatePhysicalEffectAction.Replace => CliEffectAction.Replaced,
                    UpdatePhysicalEffectAction.Delete => CliEffectAction.Deleted,
                    _ => throw new ArgumentOutOfRangeException(nameof(effect)),
                },
            Outcome = effect.ResultOutcome switch
            {
                UpdatePhysicalEffectOutcome.Planned => CliEffectOutcome.Planned,
                UpdatePhysicalEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                UpdatePhysicalEffectOutcome.Verified => CliEffectOutcome.Done,
                UpdatePhysicalEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                UpdatePhysicalEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(effect)),
            },
            Reason = effect.Reason,
            Before = effect.Before,
            After = effect.After,
        };

    private static CliWorkspaceEcho? Workspace(UpdateResult result)
    {
        if (result.WorkspacePath is not { } path)
        {
            return null;
        }

        return new CliWorkspaceEcho(
            path,
            result.WorkspaceExplicit);
    }

    private static CliRecovery Recovery(UpdateResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                UpdateRecoveryState.NotRequired or UpdateRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
                UpdateRecoveryState.Removed => CliRecoveryDisposition.Removed,
                UpdateRecoveryState.Retained => CliRecoveryDisposition.Retained,
                UpdateRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result)),
            });

    private static IReadOnlyList<string> Diagnostics(UpdateResult result, UpdateData data)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"force={(data.Force ? "true" : "false")}",
            $"prune={(data.Prune ? "true" : "false")}",
            $"automatic={(data.Automatic ? "true" : "false")}",
            $"effects={data.TotalChanges}",
            $"unchanged={data.UnchangedFiles}",
            $"sections={data.UpdatedSections}",
            $"recovery={CliReportVocabulary.Name(data.RecoveryState)}",
            $"verification={CliReportVocabulary.Name(data.VerificationState)}",
            $"findings={result.Findings.Count}",
            $"next={(Next(result) is null ? "none" : "present")}",
        ];

    private static string TrimSentence(string value)
        => value.Trim().TrimEnd('.');

    private static bool IsCode(UpdateFinding finding, string name)
        => string.Equals(CodeName(finding), name, StringComparison.Ordinal);

    private static string CodeName(UpdateFinding finding)
        => finding.Code.ToString();
}
