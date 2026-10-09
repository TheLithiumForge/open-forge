using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

internal static class InstallReportSelector
{
    private const string AgentsRootPath = ".agents";

    internal static CliReport<InstallData> Select(InstallResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var facts = result.Facts;
        var preview = result.Mode == InstallMode.DryRun;
        var includeSource = selection.Detail >= CliDetail.Full;
        var effects = facts.Effects.Select(effect => Project(effect, includeSource, result.Frontmatter?.Form)).ToArray();
        var directoryCount = DirectoryCount(facts);
        var replacementCount = ReplacementCount(effects, preview);
        var updatedFiles = UpdatedFileCount(effects, preview);
        var createdFiles = CountCreatedFiles(effects, preview);
        var createdPhysicalFiles = CountCreatedPhysicalFiles(effects, preview);
        var createdDirectories = CountCreatedDirectories(effects, preview);
        var addedSections = CountSections(effects, preview);
        var completedChanges = effects.Count(effect => effect.ResultOutcome == InstallEffectOutcome.Verified);
        var lockVerified = effects.Any(IsVerifiedOwnershipRecord);
        var blockedFindingIdentities = result.Findings
            .Where(finding => finding.Code is InstallFindingCode.TargetOccupied or InstallFindingCode.ManagedDivergence)
            .Select(finding => finding.Subject is { } subject
                ? new InstallDataTextFindingIdentity(finding.Code, subject)
                : null)
            .OfType<InstallDataTextFindingIdentity>()
            .Distinct()
            .ToArray();
        var blockedPaths = blockedFindingIdentities
            .Select(identity => identity.Path)
            .ToArray();
        var suppressInterruptedFinding = result.Status == CliSemanticStatus.Interrupted
            && !HasCancellationProgress(effects);
        var textRows = SelectTextRows(
            effects,
            selection.Detail,
            result.Status,
            preview,
            suppressInterruptedFinding,
            blockedPaths,
            facts.Migrations,
            facts.Recovery.State)
            .Concat(facts.Migrations.Select(MigrationTextRow))
            .ToArray();
        var migrations = facts.Migrations.Count > 0
            ? facts.Migrations.Select(ProjectMigration).ToArray()
            : null;
        var kept = result.Frontmatter?.Kept.Select(file => new InstallDataFrontmatterKeptFile(file.Path, KeptReason(file.Reason))).ToArray() ?? [];
        var data = new InstallData
        {
            Frontmatter = result.Frontmatter is { } form ? new InstallDataFrontmatter
            {
                Form = form.Form,
                PreviousForm = form.Changed ? form.PreviousForm : null,
                KeptCount = form.Kept.Length,
                Kept = kept.Length > 0 ? kept : null,
                TextKept = selection.Detail >= CliDetail.Standard ? kept : [],
            } : null,
            Configuration = result.Input.Configuration is { } configuration
                ? new InstallDataConfiguration(configuration.Configure, PresetName(configuration.Preset),
                    configuration.Routes.Select(row => new InstallDataRoute(row.Id, ActionName(row.Action))).ToArray())
                : null,
            Mode = InstallWireVocabulary.Name(result.Mode),
            Force = result.Force,
            Automatic = result.Automatic,
            Classification = facts.Classification is { } classification
                ? InstallWireVocabulary.Name(classification)
                : null,
            Footprint = facts.Footprint is { } footprint
                ? new InstallDataFootprint
                {
                    Files = footprint.PayloadFiles,
                    Directories = directoryCount,
                    Sections = footprint.ManagedRegions,
                }
                : null,
            LockPath = InstallWireVocabulary.OwnershipRecordPath,
            Effects = selection.Detail >= CliDetail.Full ? effects : null,
            TextEffects = effects,
            Source = selection.Detail >= CliDetail.Full && facts.Source is { } source
                ? new InstallDataSource
                {
                    InventoryFingerprint = source.InventoryFingerprint,
                    AssetCount = source.AssetCount,
                }
                : null,
            Lifecycle = selection.Detail >= CliDetail.Full
                ? new InstallDataLifecycle
                {
                    Action = InstallWireVocabulary.Name(facts.Lifecycle.Action),
                    Outcome = InstallWireVocabulary.Name(facts.Lifecycle.Outcome),
                }
                : null,
            Verification = selection.Detail >= CliDetail.Full
                ? InstallWireVocabulary.Name(facts.Verification.State)
                : null,
            Migrations = migrations,
            TextRows = textRows,
            TextSummaryLines = SelectTextSummaryLines(
                effects,
                facts.Migrations,
                selection.Detail,
                result.Status,
                preview,
                createdFiles,
                createdPhysicalFiles,
                createdDirectories,
                addedSections,
                updatedFiles,
                replacementCount,
                facts.Recovery.State,
                facts.Recovery.ResidualPath),
            TextDetailLines = SelectTextDetailLines(facts, selection.Detail),
            ShowNoChanges = preview && effects.Length > 0,
            SuppressInterruptedFinding = suppressInterruptedFinding,
            CreatedFiles = createdFiles,
            CreatedDirectories = createdDirectories,
            AddedSections = addedSections,
            ReplacedFiles = replacementCount,
            CompletedChanges = completedChanges,
            TotalChanges = effects.Length,
            Status = result.Status,
            ResultMode = result.Mode,
            IsPreview = preview,
            RecoveryState = facts.Recovery.State,
            RecoveryPath = facts.Recovery.ResidualPath,
            VerificationState = facts.Verification.State,
            IsNoOp = facts.Lifecycle.Outcome == InstallLifecycleOutcome.AlreadyCurrent,
            BlockedPaths = blockedPaths,
            BlockedFindingIdentities = blockedFindingIdentities,
        };

        return new CliReport<InstallData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, data),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(result, finding, data)).ToArray(),
            Effects = effects.Select(Effect).ToArray(),
            Counts =
            [
                new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), createdFiles),
                new CliCount("directoriesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDirectoriesCreated(), createdDirectories),
                new CliCount("sectionsAdded", global::OpenForge.Cli.OutputText.Install.InstallText.LabelSectionsAdded(), addedSections),
                new CliCount("filesReplaced", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesReplaced(), replacementCount),
            ],
            Data = data,
            Recovery = new CliRecovery(
                facts.Recovery.ResidualPath,
                facts.Recovery.State switch
                {
                    InstallResultRecoveryState.NotRequired or InstallResultRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
                    InstallResultRecoveryState.Removed => CliRecoveryDisposition.Removed,
                    InstallResultRecoveryState.Retained => CliRecoveryDisposition.Retained,
                    InstallResultRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                    _ => throw new ArgumentOutOfRangeException(nameof(result)),
                }),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data)
                : [],
        };
    }

    private static string PresetName(InstallPreset preset) => preset switch
    {
        InstallPreset.Essentials => "essentials",
        InstallPreset.FullCore => "full-core",
        InstallPreset.Custom => "custom",
        _ => throw new ArgumentOutOfRangeException(nameof(preset)),
    };

    private static string KeptReason(InstallFrontmatterKeptReason reason) => reason switch
    {
        InstallFrontmatterKeptReason.Edited => "edited",
        InstallFrontmatterKeptReason.SourceUnavailable => "source-unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(reason)),
    };

    private static string ActionName(InstallRouteAction action) => action switch
    {
        InstallRouteAction.Add => "add",
        InstallRouteAction.Remove => "remove",
        InstallRouteAction.GitIgnore => "git-ignore",
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };

    private static CliHeadline Headline(InstallResult result, InstallData data)
    {
        var workspace = result.WorkspacePath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace();
        var configure = (result.Input.Setup?.Configure == true || result.Input.Configuration?.Configure == true)
            && result.Frontmatter?.WasInstalled == true;
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status switch
        {
            CliSemanticStatus.Complete when data.IsNoOp =>
                new(InstallWording.AlreadyCurrent(), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when configure =>
                new(InstallWording.Configured(workspace, data.IsPreview), data.IsPreview ? CliHeadlineKind.Preview : CliHeadlineKind.Done),
            CliSemanticStatus.Complete when result.Mode == InstallMode.DryRun =>
                new(InstallWording.Preview(workspace, data.ReplacedFiles), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete =>
                new(InstallWording.Installed(workspace, data.ReplacedFiles), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when data.IsNoOp =>
                new(InstallWording.AlreadyCurrent(), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when configure =>
                new(InstallWording.Configured(workspace, data.IsPreview), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention =>
                new(InstallWording.Installed(workspace, data.ReplacedFiles), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete =>
                new(InstallWording.Incomplete(first?.Cause ?? InstallWording.DefaultIncompleteReason()), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid when first?.Code == InstallFindingCode.ConfirmationRequired =>
                new(CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstall()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Invalid =>
                new(InstallWording.Blocked(first?.Cause ?? InstallWording.DefaultInvalidReason()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked when result.Findings.Any(finding => finding.Code == InstallFindingCode.TargetOccupied) =>
                new(InstallWording.BlockedOccupied(CountFindingSubjects(result, InstallFindingCode.TargetOccupied)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Blocked when result.Findings.Any(finding => finding.Code == InstallFindingCode.ManagedDivergence) =>
                new(InstallWording.BlockedDivergence(CountFindingSubjects(result, InstallFindingCode.ManagedDivergence)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Blocked =>
                new(InstallWording.Blocked(first?.Cause ?? InstallWording.DefaultBlockedReason()), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed =>
                new(InstallWording.Failed(data.CompletedChanges, data.TotalChanges), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted when data.SuppressInterruptedFinding =>
                new(InstallWording.Cancelled(), CliHeadlineKind.Cancelled),
            CliSemanticStatus.Interrupted =>
                new(InstallWording.CancelledAfter(data.CompletedChanges, data.TotalChanges), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }

    private static string? HeadlineFindingCode(InstallResult result)
    {
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status == CliSemanticStatus.Invalid
            && first?.Code == InstallFindingCode.ConfirmationRequired
                ? InstallWireVocabulary.Name(first.Code)
                : null;
    }

    private static CliFinding Finding(InstallResult result, InstallFinding finding, InstallData data)
    {
        var path = finding.Subject ?? result.WorkspacePath ?? result.Command;
        var describesWorkspace = finding.Code
            is InstallFindingCode.WorkspaceUnavailable
            or InstallFindingCode.WorkspaceUnsafe;
        var subject = describesWorkspace || finding.Subject is null
            ? new CliSubject(CliSubjectKind.Workspace, null, path)
            : new CliSubject(CliSubjectKind.File, finding.Subject);
        var action = Action(finding.Code, result);
        return new CliFinding
        {
            Severity = finding.Status is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete
                ? CliSeverity.Warning
                : CliSeverity.Error,
            Code = InstallWireVocabulary.Name(finding.Code),
            Title = InstallWording.FindingTitle(finding.Code),
            Message = Message(result, finding, path, data),
            Subject = subject,
            Resolution = Resolution(finding.Code),
            Actions = action is null ? [] : [action],
            Evidence = finding.Code == InstallFindingCode.WriteFailed
                ? [new CliEvidence("cause", finding.Cause)]
                : [],
        };
    }

    private static string Message(InstallResult result, InstallFinding finding, string path, InstallData data)
        => finding.Code switch
        {
            InstallFindingCode.InvalidInput => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Install.InstallText.LabelInstall(), TrimSentence(finding.Cause)),
            InstallFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstall()),
            InstallFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(path),
            InstallFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(path, TrimSentence(finding.Cause)),
            InstallFindingCode.ManagedDivergence => InstallWording.ManagedDivergence(path),
            InstallFindingCode.TargetOccupied => CliFindingWording.TargetOccupied(path),
            InstallFindingCode.OwnershipConflict => CliFindingWording.CauseSentence(finding.Cause),
            InstallFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(path, TrimSentence(finding.Cause)),
            InstallFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(path, TrimSentence(finding.Cause)),
            InstallFindingCode.LifecycleBlocked => CliFindingWording.LifecycleBlocked(TrimSentence(finding.Cause)),
            InstallFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(path),
            InstallFindingCode.PayloadUnavailable => CliFindingWording.PayloadUnavailable(),
            InstallFindingCode.PayloadInvalid => CliFindingWording.PayloadInvalid(),
            InstallFindingCode.LifecycleUnavailable => CliFindingWording.LifecycleUnavailable(),
            InstallFindingCode.ProjectionUnavailable => CliFindingWording.ProjectionUnavailable(path, TrimSentence(finding.Cause)),
            InstallFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(path),
            InstallFindingCode.RecoveryArtifactRetained => data.RecoveryPath is { } recovery
                ? CliFindingWording.RecoveryRetained(recovery)
                : CliFindingWording.CauseSentence(finding.Cause),
            InstallFindingCode.WriteFailed => InstallWording.WriteFailed(path, finding.Cause),
            InstallFindingCode.VerificationFailed when data.RecoveryState == InstallResultRecoveryState.Retained
                && data.RecoveryPath is { } recovery => CliFindingWording.VerificationFailed(path, recovery),
            InstallFindingCode.VerificationFailed => InstallWording.VerificationFailed(path, finding.Cause),
            InstallFindingCode.LifecyclePublicationFailed => CliFindingWording.LifecyclePublicationFailed(),
            InstallFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            InstallFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstall(), TrimSentence(finding.Cause)),
            InstallFindingCode.Interrupted => data.SuppressInterruptedFinding
                ? InstallWording.Cancelled()
                : InstallWording.CancelledAfter(data.CompletedChanges, data.TotalChanges),
            _ => CliFindingWording.CauseSentence(finding.Cause),
        };

    private static CliResolution? Resolution(InstallFindingCode code)
        => code switch
        {
            InstallFindingCode.TargetOccupied
                or InstallFindingCode.ManagedDivergence
                or InstallFindingCode.ConfirmationRequired => CliResolution.TargetedOperation,
            InstallFindingCode.PayloadUnavailable
                or InstallFindingCode.LifecycleUnavailable
                or InstallFindingCode.ProjectionUnavailable
                or InstallFindingCode.RecoveryUnavailable => CliResolution.BlockedRepair,
            _ => null,
        };

    private static CliNextAction? Action(InstallFindingCode code, InstallResult result)
        => code switch
        {
            InstallFindingCode.TargetOccupied => new CliNextAction(
                InstallWording.NextOccupiedCommand(),
                InstallWording.NextOccupiedReason()),
            InstallFindingCode.ManagedDivergence => new CliNextAction(
                InstallWording.NextDivergenceCommand(),
                InstallWording.NextDivergenceReason()),
            InstallFindingCode.ConfirmationRequired => new CliNextAction(
                InstallWording.NextConfirmationCommand(result.Force),
                InstallWording.NextConfirmationReason()),
            InstallFindingCode.RecoveryArtifactRetained => new CliNextAction(
                InstallWording.NextRecoveryCommand(),
                InstallWording.NextRecoveryReason()),
            InstallFindingCode.Interrupted => new CliNextAction(
                InstallWording.NextInterruptedCommand(),
                InstallWording.NextInterruptedReason()),
            InstallFindingCode.InvalidInput => new CliNextAction(
                InstallWording.NextHelpCommand(),
                InstallWording.NextHelpReason()),
            _ when result.Status is CliSemanticStatus.Incomplete or CliSemanticStatus.Failed or CliSemanticStatus.Blocked =>
                new CliNextAction(InstallWording.NextDoctorCommand(), InstallWording.NextDoctorReason()),
            _ => null,
        };

    private static CliNextAction? Next(InstallResult result)
        => result.Status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => new CliNextAction(
                InstallWording.NextRecoveryCommand(),
                InstallWording.NextRecoveryReason()),
            CliSemanticStatus.Invalid when result.Findings.Any(finding => finding.Code == InstallFindingCode.ConfirmationRequired) => new CliNextAction(
                InstallWording.NextConfirmationCommand(result.Force),
                InstallWording.NextConfirmationReason()),
            CliSemanticStatus.Invalid => new CliNextAction(
                InstallWording.NextHelpCommand(),
                InstallWording.NextHelpReason()),
            CliSemanticStatus.Blocked when result.Findings.Any(finding => finding.Code == InstallFindingCode.TargetOccupied) => new CliNextAction(
                InstallWording.NextOccupiedCommand(),
                InstallWording.NextOccupiedReason()),
            CliSemanticStatus.Blocked when result.Findings.Any(finding => finding.Code == InstallFindingCode.ManagedDivergence) => new CliNextAction(
                InstallWording.NextDivergenceCommand(),
                InstallWording.NextDivergenceReason()),
            CliSemanticStatus.Interrupted => new CliNextAction(
                InstallWording.NextInterruptedCommand(),
                InstallWording.NextInterruptedReason()),
            _ => new CliNextAction(InstallWording.NextDoctorCommand(), InstallWording.NextDoctorReason()),
        };

    private static IReadOnlyList<string> Diagnostics(InstallResult result, InstallData data)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"force={(data.Force ? "true" : "false")}",
            $"automatic={(data.Automatic ? "true" : "false")}",
            $"classification={data.Classification ?? "none"}",
            $"effects={data.TextEffects.Count}",
            $"lifecycle={(data.Lifecycle is null ? "unavailable" : $"{data.Lifecycle.Action}/{data.Lifecycle.Outcome}")}",
            $"recovery={InstallWireVocabulary.Name(data.RecoveryState)}",
            $"verification={InstallWireVocabulary.Name(data.VerificationState)}",
            $"findings={result.Findings.Count}",
            $"next={(Next(result) is null ? "none" : "present")}",
        ];

    private static InstallDataEffect Project(InstallEffect effect, bool includeSource, string? frontmatterForm)
        => new()
        {
            Path = effect.Path,
            Kind = InstallWireVocabulary.Name(effect.Kind),
            Action = InstallWireVocabulary.Name(effect.Action),
            SourceAssetPath = includeSource ? effect.SourceAssetPath : null,
            Outcome = InstallWireVocabulary.Name(effect.Outcome),
            Residual = InstallWireVocabulary.Name(effect.Residual),
            ResultKind = effect.Kind,
            ResultAction = effect.Action,
            ResultOutcome = effect.Outcome,
            ContentChange = effect.ContentChange,
            FrontmatterForm = frontmatterForm,
        };

    private static CliEffect Effect(InstallDataEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = EffectKind(effect),
            Action = effect.ResultAction switch
            {
                InstallEffectAction.Create => CliEffectAction.Created,
                InstallEffectAction.Append => CliEffectAction.Rewritten,
                InstallEffectAction.Replace => CliEffectAction.Replaced,
                _ => throw new ArgumentOutOfRangeException(nameof(effect)),
            },
            Outcome = effect.ResultOutcome switch
            {
                InstallEffectOutcome.Planned => CliEffectOutcome.Planned,
                InstallEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                InstallEffectOutcome.Verified => CliEffectOutcome.Done,
                InstallEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                InstallEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(effect)),
            },
        };

    private static CliEffectKind EffectKind(InstallDataEffect effect)
    {
        if (effect.Path == InstallWireVocabulary.OwnershipRecordPath) return CliEffectKind.Record;
        if (effect.Path == ".agents/open-forge.json") return CliEffectKind.Setting;
        if (effect.Path == ".gitignore") return CliEffectKind.Section;
        if (effect.ResultKind == InstallEffectKind.Directory) return CliEffectKind.Directory;
        if (effect.ResultKind is InstallEffectKind.ManagedRegion or InstallEffectKind.GeneratedRegion) return CliEffectKind.Section;
        return CliEffectKind.File;
    }

    private static IReadOnlyList<InstallDataTextRow> SelectTextRows(
        IReadOnlyList<InstallDataEffect> effects,
        CliDetail detail,
        CliSemanticStatus status,
        bool preview,
        bool suppressInterruptedRows,
        IReadOnlyList<string> blockedPaths,
        IReadOnlyList<InstallMigration> migrations,
        InstallResultRecoveryState recoveryState)
    {
        if (suppressInterruptedRows)
            return [];

        if (status == CliSemanticStatus.Blocked && detail == CliDetail.Minimal)
            return blockedPaths.Select(path => new InstallDataTextRow(path, string.Empty)).ToArray();

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete)
            return [];

        var full = detail >= CliDetail.Full;
        var minimal = detail == CliDetail.Minimal;
        var hostCreateCount = HostCreateCount(effects, preview);
        var rows = new List<InstallDataTextRow>();
        foreach (var effect in effects)
        {
            var isLock = IsOwnershipRecord(effect);
            if (effect.ResultKind == InstallEffectKind.Directory
                && !full
                && IsProgressed(effect, preview))
                continue;

            if (minimal)
            {
                var progressed = IsProgressed(effect, preview);
                if (isLock && effect.ResultAction == InstallEffectAction.Create && progressed && !preview)
                    continue;
                if (effect.ResultAction == InstallEffectAction.Create && progressed && !isLock)
                {
                    if (!IsHost(effect) || hostCreateCount == 2)
                        continue;
                }
            }

            var wording = RowWording(effect, preview, isLock, migrations, recoveryState);
            if (full && effect.SourceAssetPath is { } source)
                wording += $"; {InstallWording.SourceAsset(source)}";
            rows.Add(new InstallDataTextRow(effect.Path, wording));
        }

        return rows;
    }

    private static IReadOnlyList<string> SelectTextSummaryLines(
        IReadOnlyList<InstallDataEffect> effects,
        IReadOnlyList<InstallMigration> migrations,
        CliDetail detail,
        CliSemanticStatus status,
        bool preview,
        int createdFiles,
        int createdPhysicalFiles,
        int createdDirectories,
        int addedSections,
        int updatedFiles,
        int replacedFiles,
        InstallResultRecoveryState recoveryState,
        string? recoveryPath)
    {
        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete)
            return MigrationSummaryLines(migrations);

        var partial = status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        var hostCreateCount = HostCreateCount(effects, preview);
        var lines = new List<string>();
        if (partial)
        {
            if (createdPhysicalFiles > 0 || createdDirectories > 0)
                lines.Add(InstallWording.PartialSummary(createdPhysicalFiles, createdDirectories));
        }
        else if (updatedFiles > 0 || replacedFiles > 0)
        {
            string? creationSummary = null;
            if (createdFiles > 0 || createdDirectories > 0)
            {
                creationSummary = preview
                    ? InstallWording.WouldCreateSummary(files: createdFiles, directories: createdDirectories, bothHostFiles: false)
                    : InstallWording.CreatedSummary(files: createdFiles, directories: createdDirectories,
                        lockVerified: migrations.Count == 0 && effects.Any(effect => IsVerifiedOwnershipRecord(effect)
                            && effect.ResultAction == InstallEffectAction.Create));
            }
            var summary = InstallWording.ChangeSummary(creationSummary: creationSummary,
                updatedFiles: updatedFiles, replacedFiles: replacedFiles, preview: preview);
            if (preview && hostCreateCount == 2)
                summary = InstallWording.WithHostFiles(summary);
            lines.Add(summary);
        }
        else if (createdFiles > 0 || createdDirectories > 0 || hostCreateCount == 2)
        {
            lines.Add(preview
                ? InstallWording.WouldCreateSummary(createdFiles, createdDirectories, hostCreateCount == 2)
                : InstallWording.CreatedSummary(createdFiles, createdDirectories,
                    migrations.Count == 0 && effects.Any(IsVerifiedOwnershipRecord)));
        }

        if (!partial && hostCreateCount == 2 && !preview)
        {
            lines.Add(preview
                ? InstallWording.WouldCreateHostFiles()
                : InstallWording.CreatedHostFiles());
        }

        if (detail >= CliDetail.Standard && addedSections > 0)
            lines.Add(InstallWording.SectionsSummary(addedSections, preview));

        lines.AddRange(MigrationSummaryLines(migrations));

        if (preview && updatedFiles == 0 && replacedFiles == 0 && effects.Count > 0)
            lines.Add(InstallWording.NoExistingChanges());

        if (!preview
            && detail < CliDetail.Full
            && effects.Count > 0
            && status is CliSemanticStatus.Attention or CliSemanticStatus.Failed
            && recoveryState == InstallResultRecoveryState.Retained
            && recoveryPath is { } residualPath)
        {
            lines.Add(InstallWording.RecoveryData(residualPath));
        }

        return lines;
    }

    private static IReadOnlyList<string> SelectTextDetailLines(InstallResultFacts facts, CliDetail detail)
    {
        if (detail < CliDetail.Full)
            return [];

        var lines = new List<string>();
        if (facts.Source is { } source)
            lines.Add(InstallWording.Source(source.InventoryFingerprint, source.AssetCount));
        lines.Add(InstallWording.Lifecycle(facts.Lifecycle.Action, facts.Lifecycle.Outcome));
        lines.Add(InstallWording.RecoveryDetail(facts.Recovery.State, facts.Recovery.ResidualPath));
        lines.Add(InstallWording.Verification(facts.Verification.State));
        return lines;
    }

    private static string RowWording(
        InstallDataEffect effect,
        bool preview,
        bool isLock,
        IReadOnlyList<InstallMigration> migrations,
        InstallResultRecoveryState recoveryState)
    {
        if (effect.ResultOutcome is InstallEffectOutcome.NotStarted)
            return InstallWording.NotStartedFile();

        if (effect.ResultOutcome is InstallEffectOutcome.CompletionUnknown)
            return InstallWording.UnknownFile();

        if (effect.ResultOutcome is InstallEffectOutcome.VerificationFailed)
            return InstallWording.FailedFile();

        if (isLock
            && effect.ResultAction == InstallEffectAction.Create
            && migrations.Count > 0
            && effect.ResultOutcome == InstallEffectOutcome.Verified)
        {
            return global::OpenForge.Cli.OutputText.Install.InstallText.LabelOwnershipReceiptCreated();
        }

        if (isLock && effect.ResultAction == InstallEffectAction.Create)
            return InstallWording.LockCreated(preview);

        if (effect.ResultKind == InstallEffectKind.Directory)
            return InstallWording.CreatedDirectory(preview);

        if (IsHost(effect) && effect.ResultAction == InstallEffectAction.Append)
            return InstallWording.AppendedHostFile(preview);

        if (IsHost(effect) && effect.ResultAction == InstallEffectAction.Create)
            return preview ? InstallWording.WouldCreateHostFile() : InstallWording.CreatedHostFile();

        if (effect.Path == ".gitignore")
            return InstallWording.GitIgnoreRules(effect.ContentChange == InstallEffectContentChange.GitIgnoreRulesAdded
                || effect.ResultAction == InstallEffectAction.Create, preview);

        if (effect.ResultAction == InstallEffectAction.Replace)
        {
            if (isLock)
                return CliChangeWording.OwnershipRecord(preview);
            if (effect.Path == ".agents/open-forge.json")
                return CliChangeWording.Settings(preview);
            if (effect.ContentChange == InstallEffectContentChange.FrontmatterConversion)
                return InstallWording.FrontmatterConversion(effect.FrontmatterForm
                    ?? throw new InvalidOperationException("A frontmatter conversion requires its selected form."), preview);
            if (effect.ResultKind == InstallEffectKind.GeneratedRegion)
                return CliChangeWording.Entries(preview);
            if (effect.ResultKind == InstallEffectKind.ManagedRegion)
                return CliChangeWording.OpenForgeSection(preview);
            if (effect.ContentChange == InstallEffectContentChange.PreservedContent)
                return migrations.Any(migration => migration.Path == effect.Path
                    && migration.Actions.Contains(WorkspaceAdoptionAction.MetadataCompleted))
                    ? InstallWording.MetadataCompleted(preview)
                    : CliChangeWording.Updated(preview);
        }

        return effect.ResultAction switch
        {
            InstallEffectAction.Create => InstallWording.CreatedFile(preview),
            InstallEffectAction.Append => InstallWording.AppendedSection(preview),
            InstallEffectAction.Replace => InstallWording.ReplacedFile(preview,
                recoveryKept: !preview && recoveryState == InstallResultRecoveryState.Retained),
            _ => throw new ArgumentOutOfRangeException(nameof(effect)),
        };
    }

    private static bool IsHost(InstallDataEffect effect)
        => string.Equals(effect.Path, "AGENTS.md", StringComparison.Ordinal)
            || string.Equals(effect.Path, "CLAUDE.md", StringComparison.Ordinal);

    private static bool IsAgentsDescendant(string path)
        => path.StartsWith($"{AgentsRootPath}/", StringComparison.Ordinal);

    private static bool IsOwnershipRecord(InstallDataEffect effect)
        => string.Equals(effect.Path, InstallWireVocabulary.OwnershipRecordPath, StringComparison.Ordinal);

    private static bool IsVerifiedOwnershipRecord(InstallDataEffect effect)
        => IsOwnershipRecord(effect)
            && effect.ResultOutcome == InstallEffectOutcome.Verified;

    private static InstallDataMigration ProjectMigration(InstallMigration migration)
        => new()
        {
            Path = migration.Path,
            Actions = migration.Actions.Select(InstallWireVocabulary.Name).ToArray(),
            Fields = migration.Fields.ToArray(),
            Derivation = migration.Derivation.Select(value => new InstallDataMigrationDerivation
            {
                Field = value.Field,
                Source = InstallWireVocabulary.Name(value.Source),
            }).ToArray(),
            Outcome = InstallWireVocabulary.Name(migration.Outcome),
        };

    private static InstallDataTextRow MigrationTextRow(InstallMigration migration)
    {
        var actionLabels = migration.Actions
            .Select(InstallWireVocabulary.Name)
            .Select(global::OpenForge.Cli.OutputText.Install.InstallText.MigrationActionLabel);
        var actions = global::OpenForge.Cli.OutputText.Install.InstallText.FormatMigrationActions(actionLabels);
        var wording = global::OpenForge.Cli.OutputText.Install.InstallText.FormatMigrationRow(
            InstallWireVocabulary.Name(migration.Outcome),
            actions);
        return new InstallDataTextRow(migration.Path, wording);
    }

    private static IReadOnlyList<string> MigrationSummaryLines(IReadOnlyList<InstallMigration> migrations)
    {
        if (migrations.Count == 0)
        {
            return [];
        }

        var lines = new List<string>();
        foreach (var outcome in new[] { InstallMigrationOutcome.Applied, InstallMigrationOutcome.Planned })
        {
            var count = migrations.Count(migration => migration.Outcome == outcome);
            if (count > 0)
            {
                lines.Add(global::OpenForge.Cli.OutputText.Install.InstallText.FormatMigrationSummary(
                    InstallWireVocabulary.Name(outcome),
                    count));
            }
        }

        return lines;
    }

    private static bool HasCancellationProgress(IReadOnlyList<InstallDataEffect> effects)
        => effects.Any(effect => effect.ResultOutcome is
            InstallEffectOutcome.Verified
                or InstallEffectOutcome.VerificationFailed
                or InstallEffectOutcome.CompletionUnknown);

    private static bool IsProgressed(InstallDataEffect effect, bool preview)
        => preview
            ? effect.ResultOutcome == InstallEffectOutcome.Planned
            : effect.ResultOutcome == InstallEffectOutcome.Verified;

    private static int HostCreateCount(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Count(effect => IsHost(effect)
            && effect.ResultAction == InstallEffectAction.Create
            && IsProgressed(effect, preview));

    private static int ReplacementCount(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Where(effect => IsWholeFileReplacement(effect) && IsProgressed(effect, preview))
            .Select(effect => effect.Path)
            .Distinct(StringComparer.Ordinal)
            .Count();

    private static int UpdatedFileCount(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Where(effect => effect.ResultKind != InstallEffectKind.Directory
                && effect.ResultAction is InstallEffectAction.Replace or InstallEffectAction.Append
                && !IsWholeFileReplacement(effect)
                && IsProgressed(effect, preview))
            .Select(effect => effect.Path)
            .Distinct(StringComparer.Ordinal)
            .Count();

    private static bool IsWholeFileReplacement(InstallDataEffect effect)
        => effect.ResultKind == InstallEffectKind.File
            && effect.ResultAction == InstallEffectAction.Replace
            && effect.ContentChange == InstallEffectContentChange.WholeFile
            && !IsOwnershipRecord(effect)
            && effect.Path is not (".agents/open-forge.json" or ".gitignore");

    private static int CountCreatedFiles(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Count(effect => effect.ResultAction == InstallEffectAction.Create
            && effect.ResultKind != InstallEffectKind.Directory
            && !IsOwnershipRecord(effect)
            && IsAgentsDescendant(effect.Path)
            && effect.Path != ".agents/open-forge.json"
            && IsProgressed(effect, preview));

    private static int CountCreatedPhysicalFiles(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Count(effect => effect.ResultAction == InstallEffectAction.Create
            && effect.ResultKind == InstallEffectKind.File
            && !IsOwnershipRecord(effect)
            && IsProgressed(effect, preview));

    private static int CountCreatedDirectories(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Count(effect => effect.ResultKind == InstallEffectKind.Directory
            && IsAgentsDescendant(effect.Path)
            && effect.Path != ".agents/open-forge.json"
            && IsProgressed(effect, preview));

    private static int CountSections(IReadOnlyList<InstallDataEffect> effects, bool preview)
        => effects.Count(effect => effect.ResultKind == InstallEffectKind.ManagedRegion
            && effect.ResultAction == InstallEffectAction.Append
            && IsProgressed(effect, preview));

    private static int? DirectoryCount(InstallResultFacts facts)
    {
        if (facts.Effects.Count > 0)
        {
            return facts.Effects.Count(effect => effect.Kind == InstallEffectKind.Directory
                && IsAgentsDescendant(effect.Path));
        }

        return facts.Lifecycle.Outcome == InstallLifecycleOutcome.AlreadyCurrent
            ? 0
            : null;
    }

    private static int CountFindingSubjects(InstallResult result, InstallFindingCode code)
    {
        var findings = result.Findings.Where(finding => finding.Code == code).ToArray();
        var subjects = findings
            .Select(finding => finding.Subject)
            .Where(subject => subject is not null)
            .Distinct(StringComparer.Ordinal)
            .Count();
        return subjects > 0 ? subjects : findings.Length;
    }

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
