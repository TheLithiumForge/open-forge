using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Selection;

internal static class ExtensionInstallReportSelector
{
    internal static CliReport<ExtensionInstallData> Select(
        ExtensionInstallResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var primaryId = PrimaryId(result);
        var preview = result.Mode == ExtensionInstallMode.DryRun;
        var changedSections = result.GeneratedNavigation?.Regions
            .Where(region => region.State == ExtensionInstallGeneratedRegionState.Changed)
            .Select(region => region.Path)
            .Order(StringComparer.Ordinal)
            .ToArray() ?? [];
        var unchangedSections = result.GeneratedNavigation?.Regions
            .Where(region => region.State == ExtensionInstallGeneratedRegionState.Unchanged)
            .Select(region => region.Path)
            .Order(StringComparer.Ordinal)
            .ToArray() ?? [];
        var projectedEffects = result.Effects.Select(ProjectEffect).ToArray();
        var counts = Counts(result, preview, projectedEffects);
        var noOp = result.Status == CliSemanticStatus.Complete
            && result.Lifecycle.Outcome == ExtensionInstallLifecycleOutcome.AlreadyCurrent;
        var textNextLines = TextNextLines(result, primaryId);
        var data = new ExtensionInstallData
        {
            Mode = EnumName(result.Mode),
            Force = result.Force,
            Automatic = result.Automatic,
            Source = result.Source is { } source
                ? new ExtensionInstallDataSource
                {
                    Kind = EnumName(source.Kind),
                    Path = source.Path,
                }
                : null,
            Packages = Packages(result.Packages),
            Permissions = Permissions(result),
            Selection = selection.Detail >= CliDetail.Standard && result.Selection is { } installSelection
                ? new ExtensionInstallDataSelection
                {
                    Method = EnumName(installSelection.SelectedBy),
                }
                : null,
            Sections = selection.Detail >= CliDetail.Standard ? changedSections : null,
            EntriesUnchanged = selection.Detail >= CliDetail.Full ? unchangedSections : null,
            FrameworkFingerprint = selection.Detail >= CliDetail.Full
                ? result.Framework?.InventoryFingerprint
                : null,
            Verification = selection.Detail >= CliDetail.Full
                ? Verification(result.Verification)
                : null,
            Recovery = selection.Detail >= CliDetail.Full
                ? Recovery(result.Recovery)
                : null,
            TextRows = TextRows(result, selection.Detail, preview),
            TextDetails = TextDetails(
                result,
                selection.Detail,
                preview,
                changedSections,
                unchangedSections),
            TextNextLines = textNextLines,
            IsNoOp = noOp,
        };

        return new CliReport<ExtensionInstallData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, primaryId, noOp),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings
                .Select(finding => Finding(result, finding, primaryId))
                .ToArray(),
            Effects = projectedEffects,
            Counts = counts,
            Data = data,
            Recovery = new CliRecovery(
                result.Recovery.ResidualPath,
                result.Recovery.State switch
                {
                    ExtensionInstallRecoveryState.NotRequired
                    or ExtensionInstallRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
                    ExtensionInstallRecoveryState.Removed => CliRecoveryDisposition.Removed,
                    ExtensionInstallRecoveryState.Retained => CliRecoveryDisposition.Retained,
                    ExtensionInstallRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                    _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State,
                        "The Extension Install recovery state is not defined."),
                }),
            Next = Next(result, primaryId),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"packages={result.Packages.Count.ToString(CultureInfo.InvariantCulture)}",
                    $"effects={result.Effects.Count.ToString(CultureInfo.InvariantCulture)}",
                    $"findings={result.Findings.Count.ToString(CultureInfo.InvariantCulture)}",
                }
                .Concat(result.Findings.Select(finding => finding.Cause))
                .ToArray()
                : [],
        };
    }

    private static CliHeadline Headline(
        ExtensionInstallResult result,
        string primaryId,
        bool noOp)
    {
        var finding = FirstForStatus(result);
        return result.Status switch
        {
            CliSemanticStatus.Complete when noOp => new(
                ExtensionInstallWording.AlreadyInstalled(primaryId),
                CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete => new(
                ExtensionInstallWording.Installed(
                    result.Selection?.RootIds ?? [primaryId],
                    result.Packages,
                    result.Mode == ExtensionInstallMode.DryRun),
                result.Mode == ExtensionInstallMode.DryRun
                    ? CliHeadlineKind.Preview
                    : CliHeadlineKind.Done),
            CliSemanticStatus.Attention when finding?.Code == ExtensionInstallFindingCode.PackageContentMissing
                => new(
                    ExtensionInstallWording.NothingInstalled(SourceName(result)),
                    CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention => new(
                ExtensionInstallWording.Installed(
                    result.Selection?.RootIds ?? [primaryId],
                    result.Packages,
                    result.Mode == ExtensionInstallMode.DryRun),
                CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(
                ExtensionInstallWording.Incomplete(
                    primaryId,
                    FindingMessage(result, finding, primaryId)),
                CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(
                ExtensionInstallWording.CannotInstall(InvalidProblem(result, finding)),
                CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked when finding?.Code == ExtensionInstallFindingCode.PermissionRequired
                => new(
                    ExtensionInstallWording.PermissionRequired(primaryId),
                    CliHeadlineKind.Blocked),
            CliSemanticStatus.Blocked when finding?.Code == ExtensionInstallFindingCode.InitialForceRequired
                => new(
                    ExtensionInstallWording.Blocked(
                        primaryId,
                        ExtensionInstallWording.InitialForceRequired(CountFindingTargets(result, finding.Code))),
                    CliHeadlineKind.Blocked),
            CliSemanticStatus.Blocked => new(
                ExtensionInstallWording.Blocked(
                    primaryId,
                    FindingMessage(result, finding, primaryId)),
                CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(
                ExtensionInstallWording.Failed(
                    CompletedChanges(result),
                    result.Effects.Count),
                CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(
                ExtensionInstallWording.Cancelled(),
                CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status,
                "The Extension Install status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(ExtensionInstallResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return null;
        }

        var matching = result.Findings
            .Where(finding => finding.Status == result.Status)
            .ToArray();
        return matching.Length == 0 || matching[0].Code == ExtensionInstallFindingCode.MetadataProjectionSkipped
            ? null
            : ExtensionInstallWording.FindingCode(matching[0].Code);
    }

    private static CliFinding Finding(
        ExtensionInstallResult result,
        ExtensionInstallFinding finding,
        string primaryId)
    {
        var subject = Subject(result, finding, primaryId);
        var action = Action(result, finding, primaryId);
        return new CliFinding
        {
            Severity = Severity(finding),
            Code = ExtensionInstallWording.FindingCode(finding.Code),
            Title = ExtensionInstallWording.FindingTitle(finding.Code),
            Message = FindingMessage(result, finding, primaryId),
            Subject = subject,
            Resolution = Resolution(finding.Code),
            Actions = action is null ? [] : [action],
        };
    }

    private static CliSubject Subject(
        ExtensionInstallResult result,
        ExtensionInstallFinding finding,
        string primaryId)
    {
        var target = finding.Target;
        var sourceTarget = finding.Code is ExtensionInstallFindingCode.SourceUnavailable
            or ExtensionInstallFindingCode.SourceInvalid
            ? SourceName(result)
            : null;
        var value = target ?? sourceTarget ?? primaryId;
        var kind = finding.Code switch
        {
            ExtensionInstallFindingCode.SourceUnavailable
                or ExtensionInstallFindingCode.SourceInvalid => CliSubjectKind.Source,
            ExtensionInstallFindingCode.FrameworkUnavailable
                or ExtensionInstallFindingCode.FrameworkUnsafe
                or ExtensionInstallFindingCode.LifecycleUnavailable
                or ExtensionInstallFindingCode.LifecycleBlocked
                or ExtensionInstallFindingCode.PermissionsInvalid
                or ExtensionInstallFindingCode.PermissionsUnavailable
                or ExtensionInstallFindingCode.PermissionWriteFailed => CliSubjectKind.Workspace,
            ExtensionInstallFindingCode.InitialForceRequired
                or ExtensionInstallFindingCode.PermissionRequired
                or ExtensionInstallFindingCode.PermissionDeclined
                or ExtensionInstallFindingCode.PermissionsChanged
                or ExtensionInstallFindingCode.TargetUnsafe
                or ExtensionInstallFindingCode.ProjectionUnavailable
                or ExtensionInstallFindingCode.MetadataProjectionSkipped
                or ExtensionInstallFindingCode.GeneratedRegionUnsafe
                or ExtensionInstallFindingCode.WorkspaceLockUnavailable
                or ExtensionInstallFindingCode.TargetChanged
                or ExtensionInstallFindingCode.OwnershipConflict
                or ExtensionInstallFindingCode.RecoveryConflict
                or ExtensionInstallFindingCode.RecoveryUnavailable
                or ExtensionInstallFindingCode.RecoveryArtifactRetained
                or ExtensionInstallFindingCode.WriteFailed
                or ExtensionInstallFindingCode.TopologyVerificationFailed
                or ExtensionInstallFindingCode.LifecyclePublicationFailed
                or ExtensionInstallFindingCode.VerificationFailed
                or ExtensionInstallFindingCode.RecoveryFailed => CliSubjectKind.File,
            _ => CliSubjectKind.Identifier,
        };
        return kind switch
        {
            CliSubjectKind.Source => new CliSubject(kind, value),
            CliSubjectKind.File => new CliSubject(kind, value),
            CliSubjectKind.Workspace => new CliSubject(kind, null, value),
            _ => new CliSubject(CliSubjectKind.Identifier, null, value),
        };
    }

    private static CliSeverity Severity(ExtensionInstallFinding finding)
        => finding.Code == ExtensionInstallFindingCode.LifecycleObservation
            ? CliSeverity.Info
            : CliReportVocabulary.Severity(finding.Status);

    private static CliResolution? Resolution(ExtensionInstallFindingCode code)
        => code switch
        {
            ExtensionInstallFindingCode.SelectionRequired
                or ExtensionInstallFindingCode.InitialForceRequired
                or ExtensionInstallFindingCode.PermissionRequired => CliResolution.GuidedChoice,
            ExtensionInstallFindingCode.ManagedDivergence
                or ExtensionInstallFindingCode.PackageContentsChanged => CliResolution.TargetedOperation,
            ExtensionInstallFindingCode.LifecycleObservation
                or ExtensionInstallFindingCode.RecoveryArtifactRetained => CliResolution.Informational,
            _ => null,
        };

    private static CliNextAction? Action(
        ExtensionInstallResult result,
        ExtensionInstallFinding finding,
        string primaryId)
        => finding.Code switch
        {
            ExtensionInstallFindingCode.SelectionRequired => new CliNextAction(
                "open-forge extension list",
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessageListTheAvailableExtensionsThenRerunTheRequestWithAnExplicitSelection()),
            ExtensionInstallFindingCode.ManagedDivergence when finding.Target is { } target => new CliNextAction(
                $"open-forge extension update {target}",
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessageUseExtensionUpdateToReconcileTheManagedPackage()),
            ExtensionInstallFindingCode.PackageContentsChanged when finding.Target is { } package => new CliNextAction(
                $"open-forge extension update {package}",
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessageUseExtensionUpdateToBringTheRecordedPackageBackInStepWithTheSource()),
            ExtensionInstallFindingCode.InitialForceRequired => new CliNextAction(
                ExtensionInstallWording.ForcePreviewNext(primaryId),
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessagePreviewReplacingTheExistingFileBeforeRerunningTheInstallation()),
            ExtensionInstallFindingCode.PermissionRequired when result.MissingPermissions.FirstOrDefault() is { } path
                => new CliNextAction(
                    ExtensionInstallWording.PermissionNext(primaryId, path),
                    ExtensionInstallWording.PermissionAlternative(path)),
            ExtensionInstallFindingCode.RecoveryArtifactRetained when result.Next is { } recovery => recovery,
            _ => null,
        };

    private static string FindingMessage(
        ExtensionInstallResult result,
        ExtensionInstallFinding? finding,
        string primaryId)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelTheRequiredExtensionInstallFactsAreUnavailable();
        }

        var target = finding.Target ?? primaryId;
        return finding.Code switch
        {
            ExtensionInstallFindingCode.InvalidInput => Sentence(finding.Cause),
            ExtensionInstallFindingCode.SelectionRequired => CliFindingWording.SelectionRequired(
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstall(),
                promptUnavailable: true),
            ExtensionInstallFindingCode.InteractionEnded => CliFindingWording.InteractionEnded(),
            ExtensionInstallFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstall()),
            ExtensionInstallFindingCode.SourceUnavailable => ExtensionInstallWording.SourceUnavailable(
                SourceName(result)),
            ExtensionInstallFindingCode.SourceInvalid => ExtensionInstallWording.SourceInvalid(
                SourceName(result),
                finding.Cause),
            ExtensionInstallFindingCode.PackageContentMissing =>
                (global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessageThePackageHasNoContentDirectory() + " ")
                + ExtensionInstallWording.NoContentRow(SourceName(result).TrimEnd('/', '\\')),
            ExtensionInstallFindingCode.FrameworkUnavailable => CliFindingWording.FrameworkUnavailable(),
            ExtensionInstallFindingCode.FrameworkUnsafe => CliFindingWording.FrameworkUnsafe(),
            ExtensionInstallFindingCode.LifecycleUnavailable => CliFindingWording.LifecycleUnavailable(),
            ExtensionInstallFindingCode.LifecycleBlocked => CliFindingWording.LifecycleBlocked(
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.LifecycleObservation
                or ExtensionInstallFindingCode.MetadataProjectionSkipped => finding.Cause,
            ExtensionInstallFindingCode.ManagedDivergence => CliFindingWording.ManagedDivergence(target),
            ExtensionInstallFindingCode.PackageContentsChanged => ExtensionInstallWording.PackageContentsChanged(target),
            ExtensionInstallFindingCode.InitialForceRequired => CliFindingWording.TargetOccupied(target),
            ExtensionInstallFindingCode.OwnershipConflict =>
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatOwnershipConflict(
                    finding.Cause,
                    target),
            ExtensionInstallFindingCode.PermissionRequired => ExtensionInstallWording.PermissionRequired(primaryId),
            ExtensionInstallFindingCode.PermissionDeclined => CliFindingWording.PermissionDeclined(),
            ExtensionInstallFindingCode.PermissionsInvalid => CliFindingWording.PermissionsInvalid(
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.PermissionsUnavailable => CliFindingWording.PermissionsUnavailable(),
            ExtensionInstallFindingCode.PermissionsChanged => CliFindingWording.PermissionsChanged(),
            ExtensionInstallFindingCode.PermissionWriteFailed => CliFindingWording.PermissionWriteFailed(),
            ExtensionInstallFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(
                target,
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.ProjectionUnavailable => CliFindingWording.ProjectionUnavailable(
                target,
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(
                target,
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            ExtensionInstallFindingCode.TargetChanged => CliFindingWording.TargetChanged(target),
            ExtensionInstallFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(target),
            ExtensionInstallFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(target),
            ExtensionInstallFindingCode.RecoveryArtifactRetained => finding.Cause,
            ExtensionInstallFindingCode.WriteFailed => CliFindingWording.CauseSentence(finding.Cause),
            ExtensionInstallFindingCode.TopologyVerificationFailed =>
                (global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HeadingTheEntriesSectionsDidNotMatchTheInstalledFilesAfterWritingRecoveryData() + " ")
                + (result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable()) + ".",
            ExtensionInstallFindingCode.LifecyclePublicationFailed => CliFindingWording.LifecyclePublicationFailed(),
            ExtensionInstallFindingCode.VerificationFailed => finding.Cause,
            ExtensionInstallFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            ExtensionInstallFindingCode.OperationFailed => CliFindingWording.OperationFailed(
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstall(),
                Sentence(finding.Cause)),
            ExtensionInstallFindingCode.Interrupted => ExtensionInstallWording.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code,
                "The Extension Install finding code is not defined."),
        };
    }

    private static string InvalidProblem(
        ExtensionInstallResult result,
        ExtensionInstallFinding? finding)
        => finding?.Code switch
        {
            ExtensionInstallFindingCode.SelectionRequired => CliFindingWording.SelectionRequired(
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstall(),
                promptUnavailable: true),
            ExtensionInstallFindingCode.InteractionEnded => CliFindingWording.InteractionEnded(),
            ExtensionInstallFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstall()),
            ExtensionInstallFindingCode.SourceInvalid when finding is not null => ExtensionInstallWording.SourceInvalid(
                SourceName(result),
                finding.Cause),
            _ when finding is not null => Sentence(finding.Cause),
            _ => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheInputIsInvalid(),
        };

    private static string SourceName(ExtensionInstallResult result)
        => result.Source?.Path
            ?? result.Source?.Identity
            ?? result.Findings.FirstOrDefault(finding => finding.Code is ExtensionInstallFindingCode.SourceUnavailable
                or ExtensionInstallFindingCode.SourceInvalid)?.Target
            ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource();

    private static string PrimaryId(ExtensionInstallResult result)
        => result.Selection?.RootIds.FirstOrDefault()
            ?? result.Packages.FirstOrDefault(package => package.SelectedRoot)?.Id
            ?? result.Packages.FirstOrDefault()?.Id
            ?? result.Findings.FirstOrDefault(finding =>
                finding.Code is ExtensionInstallFindingCode.ManagedDivergence or ExtensionInstallFindingCode.PackageContentsChanged)?.Target
            ?? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelSelected();

    private static ExtensionInstallFinding? FirstForStatus(ExtensionInstallResult result)
        => result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();

    private static int CountFindingTargets(
        ExtensionInstallResult result,
        ExtensionInstallFindingCode code)
        => Math.Max(1, result.Findings.Count(finding => finding.Code == code && finding.Target is not null));

    private static int CompletedChanges(ExtensionInstallResult result)
        => result.Effects.Count(effect => effect.Outcome == ExtensionInstallEffectOutcome.Verified);

    private static IReadOnlyList<ExtensionInstallDataPackage> Packages(
        IReadOnlyList<ExtensionInstallPackage> packages)
    {
        var requiredBy = packages.ToDictionary(
            package => package.Id,
            _ => new List<string>(),
            StringComparer.Ordinal);
        foreach (var package in packages)
        {
            foreach (var dependency in package.Dependencies)
            {
                if (requiredBy.TryGetValue(dependency, out var dependents))
                {
                    dependents.Add(package.Id);
                }
            }
        }

        return packages
            .Select(package => new ExtensionInstallDataPackage
            {
                Id = package.Id,
                Version = package.Version,
                Selected = package.SelectedRoot,
                RequiredBy = requiredBy[package.Id].Order(StringComparer.Ordinal).ToArray(),
            })
            .ToArray();
    }

    private static ExtensionInstallDataPermissions Permissions(ExtensionInstallResult result)
        => new()
        {
            Decision = EnumName(result.PermissionDecision),
            Required = [.. result.RequiredPermissions],
            Missing = [.. result.MissingPermissions],
            Saved = EnumName(result.PermissionOutcome) == "verified",
        };

    private static ExtensionInstallDataVerification Verification(
        ExtensionInstallVerification verification)
        => new()
        {
            Targets = EnumName(verification.Targets),
            Topology = EnumName(verification.Topology),
            ExtensionsLifecycle = EnumName(verification.ExtensionsLifecycle),
            FrameworkLifecycle = EnumName(verification.FrameworkLifecycle),
        };

    private static ExtensionInstallDataRecovery Recovery(ExtensionInstallRecovery recovery)
        => new()
        {
            State = EnumName(recovery.State),
            ProtectedPaths = [.. recovery.ProtectedPaths],
            ResidualPath = recovery.ResidualPath,
        };

    private static IReadOnlyList<CliCount> Counts(
        ExtensionInstallResult result,
        bool preview,
        IReadOnlyList<CliEffect> effects)
    {
        var contributes = effects
            .Where(effect => effect.Outcome is CliEffectOutcome.Planned or CliEffectOutcome.Done)
            .ToArray();
        var packageIds = contributes
            .Where(effect => effect.Owner is not null && effect.Kind == CliEffectKind.File)
            .Select(effect => effect.Owner!)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var created = contributes.Count(effect => effect.Kind == CliEffectKind.File
            && effect.Action == CliEffectAction.Created);
        var replaced = contributes.Count(effect => effect.Kind == CliEffectKind.File
            && effect.Action == CliEffectAction.Replaced);
        var sections = contributes.Count(effect => effect.Kind == CliEffectKind.Section);
        var saved = EnumName(result.PermissionOutcome) == "verified";
        if (preview)
        {
            saved = false;
        }

        return
        [
            new CliCount("packagesInstalled", global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelPackagesInstalled(), result.Status == CliSemanticStatus.Complete && !IsNoOp(result) ? packageIds : 0),
            new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), created),
            new CliCount("filesReplaced", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesReplaced(), replaced),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionsUpdated(), sections),
            new CliCount("grantsSaved", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelGrantsSaved(), saved ? 1 : 0),
        ];
    }

    private static bool IsNoOp(ExtensionInstallResult result)
        => result.Status == CliSemanticStatus.Complete
            && result.Lifecycle.Outcome == ExtensionInstallLifecycleOutcome.AlreadyCurrent;

    private static CliEffect ProjectEffect(ExtensionInstallEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                ExtensionInstallEffectKind.Directory => CliEffectKind.Directory,
                ExtensionInstallEffectKind.PackageFile => CliEffectKind.File,
                ExtensionInstallEffectKind.GeneratedRegion => CliEffectKind.Section,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind,
                    "The Extension Install effect kind is not defined."),
            },
            Action = effect.Kind switch
            {
                ExtensionInstallEffectKind.Directory when effect.Action == ExtensionInstallEffectAction.Create
                    => CliEffectAction.Created,
                ExtensionInstallEffectKind.PackageFile when effect.Action == ExtensionInstallEffectAction.Create
                    => CliEffectAction.Created,
                ExtensionInstallEffectKind.PackageFile when effect.Action == ExtensionInstallEffectAction.Replace
                    => CliEffectAction.Replaced,
                ExtensionInstallEffectKind.GeneratedRegion when effect.Action == ExtensionInstallEffectAction.Replace
                    => CliEffectAction.Rewritten,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Action,
                    "The Extension Install effect action is not defined."),
            },
            Outcome = effect.Outcome switch
            {
                ExtensionInstallEffectOutcome.Planned => CliEffectOutcome.Planned,
                ExtensionInstallEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                ExtensionInstallEffectOutcome.Verified => CliEffectOutcome.Done,
                ExtensionInstallEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                ExtensionInstallEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome,
                    "The Extension Install effect outcome is not defined."),
            },
            Reason = effect.Residual switch
            {
                ExtensionInstallEffectResidual.None => null,
                ExtensionInstallEffectResidual.Retained when effect.Kind == ExtensionInstallEffectKind.PackageFile
                    && effect.Action == ExtensionInstallEffectAction.Replace
                    => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelYourPreviousFileIsInTheRecoveryBundle(),
                ExtensionInstallEffectResidual.Retained => null,
                ExtensionInstallEffectResidual.Unknown => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelTheFinalStateIsUnknown(),
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Residual,
                    "The Extension Install effect residual is not defined."),
            },
            Owner = effect.PackageId,
        };

    private static IReadOnlyList<ExtensionInstallDataTextRow> TextRows(
        ExtensionInstallResult result,
        CliDetail detail,
        bool preview)
    {
        var rows = new List<ExtensionInstallDataTextRow>();
        var listAll = detail >= CliDetail.Standard
            || result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        var forceBoundary = result.Findings.Any(finding =>
            finding.Code == ExtensionInstallFindingCode.InitialForceRequired);
        foreach (var effect in result.Effects)
        {
            if (effect.Kind == ExtensionInstallEffectKind.Directory)
            {
                continue;
            }

            if (effect.Kind == ExtensionInstallEffectKind.GeneratedRegion)
            {
                if (detail >= CliDetail.Standard)
                {
                    rows.Add(new ExtensionInstallDataTextRow(
                        effect.Path,
                        EffectText(effect, preview)));
                }

                continue;
            }

            var include = listAll
                || effect.Action == ExtensionInstallEffectAction.Replace
                || forceBoundary;
            if (!include)
            {
                continue;
            }

            rows.Add(new ExtensionInstallDataTextRow(
                effect.Path,
                detail == CliDetail.Minimal && forceBoundary
                    ? string.Empty
                    : EffectText(effect, preview)));
        }

        if (result.Status == CliSemanticStatus.Blocked
            && result.Findings.Any(finding => finding.Code == ExtensionInstallFindingCode.PermissionRequired))
        {
            rows.Clear();
            rows.AddRange(result.MissingPermissions.Select(path => new ExtensionInstallDataTextRow(path, string.Empty)));
        }

        return rows;
    }

    private static string EffectText(ExtensionInstallEffect effect, bool preview)
    {
        return effect.Outcome switch
        {
            ExtensionInstallEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            ExtensionInstallEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            ExtensionInstallEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            ExtensionInstallEffectOutcome.Planned or ExtensionInstallEffectOutcome.Verified
                => effect.Kind switch
                {
                    ExtensionInstallEffectKind.PackageFile when effect.Action == ExtensionInstallEffectAction.Create
                        => ExtensionInstallWording.CreatedFile(effect.PackageId ?? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelPackage(), preview),
                    ExtensionInstallEffectKind.PackageFile when effect.Action == ExtensionInstallEffectAction.Replace
                        => ExtensionInstallWording.ReplacedFile(preview),
                    ExtensionInstallEffectKind.GeneratedRegion => ExtensionInstallWording.UpdatedSection(preview),
                    _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind,
                        "The Extension Install text effect kind is not defined."),
                },
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome,
                "The Extension Install text effect outcome is not defined."),
        };
    }

    private static IReadOnlyList<string> TextDetails(
        ExtensionInstallResult result,
        CliDetail detail,
        bool preview,
        IReadOnlyList<string> changedSections,
        IReadOnlyList<string> unchangedSections)
    {
        var details = new List<string>();
        var minimal = detail == CliDetail.Minimal;
        var standard = detail >= CliDetail.Standard;
        var full = detail >= CliDetail.Full;
        if (minimal)
        {
            var creates = result.Effects
                .Where(effect => effect.Kind == ExtensionInstallEffectKind.PackageFile
                    && effect.Action == ExtensionInstallEffectAction.Create
                    && effect.Outcome is ExtensionInstallEffectOutcome.Planned
                        or ExtensionInstallEffectOutcome.Verified)
                .GroupBy(effect => Directory(effect.Path), StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .ToArray();
            if (creates.Length > 0)
            {
                details.Add(ExtensionInstallWording.CreatedFiles(
                    creates.Sum(group => group.Count()),
                    string.Join(", ", creates.Select(group => group.Key)),
                    preview));
            }

            var updates = changedSections.Count;
            if (updates > 0)
            {
                details.Add(ExtensionInstallWording.UpdatedSections(updates, preview));
            }

            if (GrantPaths(result) is { Count: > 0 } grants
                && (EnumName(result.PermissionOutcome) == "verified"
                    || preview && EnumName(result.PermissionOutcome) == "planned"))
            {
                details.AddRange(grants.Select(path => ExtensionInstallWording.SavedGrant(path, preview)));
            }

            if (result.Findings.FirstOrDefault(finding => finding.Code == ExtensionInstallFindingCode.PackageContentMissing) is not null)
            {
                details.Add(ExtensionInstallWording.NoContentRow(SourceName(result).TrimEnd('/', '\\')));
            }
        }

        if (standard)
        {
            if (result.Source is { } source)
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatSource($"{source.Identity}", $"{EnumName(source.Kind)}"));
            }

            if (result.Packages.Count > 1)
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatDependencyOrder($"{string.Join(", ", result.Packages.Select(package => package.Id))}"));
            }

            if (GrantPaths(result) is { Count: > 0 } scopes)
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatPermissionScope($"{string.Join(", ", scopes)}"));
            }

            if (result.Lifecycle.Action != ExtensionInstallLifecycleAction.None)
            {
                details.Add($".agents/open-forge.lock.json  {ExtensionInstallWording.Lock(preview)}");
            }
        }

        if (full)
        {
            details.Add(ExtensionInstallWording.EntriesUnchanged(unchangedSections));
            if (EnumName(result.PermissionDecision) != "not-evaluated")
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatPermissionEvaluationDecisionRequiredMissingSaved(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{EnumName(result.PermissionDecision)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{result.RequiredPermissions.Length}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{result.MissingPermissions.Length}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(EnumName(result.PermissionOutcome) == "verified" ? "yes" : "no")}")));
            }

            if (result.Framework is { } framework)
            {
                details.Add(global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatFrameworkFingerprint($"{framework.InventoryFingerprint}"));
            }

            details.Add(ExtensionInstallWording.Verification(result.Verification));
            details.Add(ExtensionInstallWording.RecoveryFacts(result.Recovery));
        }

        return details;
    }

    private static IReadOnlyList<string>? GrantPaths(ExtensionInstallResult result)
    {
        var paths = result.RequiredPermissions.Length > 0
            ? result.RequiredPermissions
            : result.MissingPermissions;
        return paths.Length == 0 ? null : [.. paths.Order(StringComparer.Ordinal)];
    }

    private static IReadOnlyList<string> TextNextLines(
        ExtensionInstallResult result,
        string primaryId)
    {
        if (result.Findings.Any(finding => finding.Code == ExtensionInstallFindingCode.PermissionRequired)
            && result.MissingPermissions.FirstOrDefault() is { } path)
        {
            return
            [
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatNext($"{ExtensionInstallWording.PermissionNext(primaryId, path)}"),
                $"  {ExtensionInstallWording.PermissionAlternative(path)}",
            ];
        }

        if (result.Findings.Any(finding => finding.Code == ExtensionInstallFindingCode.InitialForceRequired))
        {
            return [global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatNextPreviewReplacingIt($"{ExtensionInstallWording.ForcePreviewNext(primaryId)}")];
        }

        return [];
    }

    private static CliNextAction? Next(ExtensionInstallResult result, string primaryId)
    {
        if (result.Findings.Any(finding => finding.Code == ExtensionInstallFindingCode.PermissionRequired)
            && result.MissingPermissions.FirstOrDefault() is { } path)
        {
            return new CliNextAction(
                ExtensionInstallWording.PermissionNext(primaryId, path),
                ExtensionInstallWording.PermissionAlternative(path));
        }

        if (result.Findings.Any(finding => finding.Code == ExtensionInstallFindingCode.InitialForceRequired))
        {
            return new CliNextAction(
                ExtensionInstallWording.ForcePreviewNext(primaryId),
                global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessagePreviewReplacingTheExistingFileBeforeRerunningTheInstallation());
        }

        return result.Next;
    }

    private static string Directory(string path)
    {
        var normalized = path.Replace('\\', '/');
        var separator = normalized.LastIndexOf('/');
        return separator > 0 ? normalized[..separator] : ".";
    }

    private static string Sentence(string value) => CliFindingWording.PlainCause(value);

    private static string EnumName(Enum value)
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
}
