using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Wording;

internal static class ExtensionInspectWording
{
    internal static CliHeadline Headline(ExtensionInspectResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Status switch
        {
            CliSemanticStatus.Complete => new(CompleteHeadline(result), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(AttentionHeadline(result), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(IncompleteHeadline(result), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(InvalidHeadline(result), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(BlockedHeadline(result), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(FailedHeadline(result), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageExtensionInspectWasCancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Extension Inspect status is not defined."),
        };
    }

    internal static string SourceKind(ExtensionInspectSourceKind kind)
        => kind switch
        {
            ExtensionInspectSourceKind.EmbeddedCatalogue => "embedded-catalogue",
            ExtensionInspectSourceKind.Package => "package",
            ExtensionInspectSourceKind.Catalogue => "catalogue",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension Inspect source kind is not defined."),
        };

    internal static string SourceText(ExtensionInspectResult result)
        => result.Source.Kind == ExtensionInspectSourceKind.EmbeddedCatalogue
            ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelBundledWithThisCli()
            : result.Source.Identity ?? result.Source.Supplied ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    internal static string Relation(ExtensionInspectPathRelation relation)
        => relation switch
        {
            ExtensionInspectPathRelation.Unchanged or ExtensionInspectPathRelation.Shared => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelUnchanged(),
            ExtensionInspectPathRelation.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChanged(),
            ExtensionInspectPathRelation.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
            ExtensionInspectPathRelation.New => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelNewInThePackage(),
            ExtensionInspectPathRelation.Retired => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelNoLongerPartOfThePackage(),
            ExtensionInspectPathRelation.NotApplicable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotApplicable(),
            ExtensionInspectPathRelation.Unknown => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelCouldNotBeCompared(),
            ExtensionInspectPathRelation.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeRead(),
            ExtensionInspectPathRelation.Invalid => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInvalid(),
            ExtensionInspectPathRelation.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
            ExtensionInspectPathRelation.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "The Extension Inspect path relation is not defined."),
        };

    internal static string FileMessage(
        ExtensionInspectResult result,
        ExtensionInspectPathComparison comparison)
        => comparison.Relation switch
        {
            ExtensionInspectPathRelation.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChangedSinceItWasInstalled(),
            ExtensionInspectPathRelation.Missing => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatMissingItWasInstalledBy(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{comparison.CurrentOwners.FirstOrDefault() ?? result.Subject.Id ?? result.Subject.Supplied ?? "the Extension"}")),
            ExtensionInspectPathRelation.New => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelNewInThePackageNotInstalledYet(),
            ExtensionInspectPathRelation.Retired => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelNoLongerPartOfThePackage(),
            _ => Relation(comparison.Relation),
        };

    internal static string DependencyState(ExtensionInspectDependencyPackageState state)
        => CliReportVocabulary.Name(state);

    internal static string Version(string? version) => version ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown();

    internal static string FileSummary(IReadOnlyList<ExtensionInspectDataFile> files)
    {
        var comparable = files
            .Where(file => file.Relation == "unchanged")
            .Select(file => file.Path)
            .ToArray();
        if (comparable.Length == 0)
        {
            return string.Empty;
        }

        var directory = CommonDirectory(comparable);
        return directory is null
            ? string.Create(CultureInfo.InvariantCulture, $"{comparable.Length} {CliText.Plural(comparable.Length, "file")}")
            : global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{comparable.Length}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(comparable.Length, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{directory}"));
    }

    internal static string FindingTitle(ExtensionInspectFindingCode code)
        => code switch
        {
            ExtensionInspectFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            ExtensionInspectFindingCode.InvalidStableId => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleInvalidExtensionId(),
            ExtensionInspectFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            ExtensionInspectFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            ExtensionInspectFindingCode.SourceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensionSourceIsUnavailable(),
            ExtensionInspectFindingCode.SourceInvalid => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionSourceIsInvalid(),
            ExtensionInspectFindingCode.SourceOverlap => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionSourceIsInsideTheWorkspace(),
            ExtensionInspectFindingCode.SourceAmbiguous => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionSourceIsAmbiguous(),
            ExtensionInspectFindingCode.IdentityAmbiguous => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionIdentityIsAmbiguous(),
            ExtensionInspectFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
            ExtensionInspectFindingCode.PackageUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionPackageIsUnavailable(),
            ExtensionInspectFindingCode.PackageInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionPackageIsInvalid(),
            ExtensionInspectFindingCode.DependencyIncomplete => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependencyCouldNotBeResolved(),
            ExtensionInspectFindingCode.DependencyCycle => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependencyCycle(),
            ExtensionInspectFindingCode.DependencyConflict => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependencyConflict(),
            ExtensionInspectFindingCode.PathUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileCouldNotBeRead(),
            ExtensionInspectFindingCode.PathInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFilePathIsInvalid(),
            ExtensionInspectFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflict(),
            ExtensionInspectFindingCode.FingerprintUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileCouldNotBeCompared(),
            ExtensionInspectFindingCode.FingerprintFallback => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileComparisonUsedBytes(),
            ExtensionInspectFindingCode.GeneratedBoundaryInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleGeneratedRegionIsInvalid(),
            ExtensionInspectFindingCode.DependencyChanged => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependenciesChanged(),
            ExtensionInspectFindingCode.PathChanged => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileChanged(),
            ExtensionInspectFindingCode.PathMissing => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileIsMissing(),
            ExtensionInspectFindingCode.PathNew => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileIsNew(),
            ExtensionInspectFindingCode.PathRetired => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFileIsRetired(),
            ExtensionInspectFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionInspectFailed(),
            ExtensionInspectFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtensionInspectWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Inspect finding code is not defined."),
        };

    internal static CliSeverity Severity(ExtensionInspectFindingCode code, CliSemanticStatus status)
        => code switch
        {
            ExtensionInspectFindingCode.InvalidInput
                or ExtensionInspectFindingCode.InvalidStableId
                or ExtensionInspectFindingCode.PackageUnavailable
                or ExtensionInspectFindingCode.PackageInvalid
                or ExtensionInspectFindingCode.SourceInvalid
                or ExtensionInspectFindingCode.SourceAmbiguous
                or ExtensionInspectFindingCode.SourceOverlap
                or ExtensionInspectFindingCode.WorkspaceUnavailable
                or ExtensionInspectFindingCode.WorkspaceUnsafe
                or ExtensionInspectFindingCode.IdentityAmbiguous
                or ExtensionInspectFindingCode.DependencyCycle
                or ExtensionInspectFindingCode.DependencyConflict
                or ExtensionInspectFindingCode.PathInvalid
                or ExtensionInspectFindingCode.OwnershipConflict
                or ExtensionInspectFindingCode.GeneratedBoundaryInvalid
                or ExtensionInspectFindingCode.OperationFailed
                or ExtensionInspectFindingCode.Interrupted => CliSeverity.Error,
            ExtensionInspectFindingCode.OwnershipObservation
                or ExtensionInspectFindingCode.FingerprintFallback => CliSeverity.Info,
            _ => CliReportVocabulary.Severity(status),
        };

    internal static string Message(ExtensionInspectResult result, ExtensionInspectFinding finding)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(finding);
        var id = finding.PackageId ?? finding.Subject ?? result.Subject.Id ?? result.Subject.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheExtension();
        var source = result.Source.Identity ?? result.Source.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource();
        var path = finding.Path ?? finding.Subject ?? source;
        return finding.Code switch
        {
            ExtensionInspectFindingCode.InvalidInput => TrimPeriod(finding.Cause),
            ExtensionInspectFindingCode.InvalidStableId => InvalidStableId(finding.Subject ?? result.Subject.Supplied),
            ExtensionInspectFindingCode.PackageUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatNoExtensionHasTheIdIn($"{id}", $"{source}"),
            ExtensionInspectFindingCode.PackageInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatThePackageAtIsInvalid($"{id}", $"{path}", $"{TrimPeriod(finding.Cause)}"),
            ExtensionInspectFindingCode.SourceInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsNotAnExtensionPackageOrPackageFolder($"{path}", $"{TrimPeriod(finding.Cause)}"),
            ExtensionInspectFindingCode.SourceUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatTheSourceCouldNotBeReadSoTheComparisonCouldNotFinish($"{path}"),
            ExtensionInspectFindingCode.SourceAmbiguous => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsFoundMoreThanOnceIn($"{id}", $"{source}"),
            ExtensionInspectFindingCode.SourceOverlap => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsInsideTheWorkspaceAndCannotBeUsedAsASource($"{path}"),
            ExtensionInspectFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(result.WorkspacePath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()),
            ExtensionInspectFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(
                result.WorkspacePath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace(),
                TrimPeriod(finding.Cause)),
            ExtensionInspectFindingCode.IdentityAmbiguous => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatTheOwnershipRecordNamesInAWayThatCannotBeMatchedToOnePackage($"{result.Subject.Id ?? finding.PackageId ?? finding.Subject ?? id}"),
            ExtensionInspectFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageNoOwnershipRecordExistsSoInstallationCannotBeChecked(),
            ExtensionInspectFindingCode.PathChanged => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatChangedSinceItWasInstalled($"{path}"),
            ExtensionInspectFindingCode.PathMissing => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatMissingItWasInstalledBy($"{path}", $"{id}"),
            ExtensionInspectFindingCode.PathNew => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatNewInThePackageNotInstalledYet($"{path}"),
            ExtensionInspectFindingCode.PathRetired => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatNoLongerPartOfThePackage($"{path}"),
            ExtensionInspectFindingCode.PathInvalid => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatInThePackageIsNotAValidWorkspacePath($"{path}"),
            ExtensionInspectFindingCode.PathUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatCouldNotBeRead($"{path}"),
            ExtensionInspectFindingCode.FingerprintUnavailable => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatCouldNotBeCompared($"{path}"),
            ExtensionInspectFindingCode.FingerprintFallback => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatWasComparedByteForByteBecauseItIsNotMarkdown($"{path}"),
            ExtensionInspectFindingCode.GeneratedBoundaryInvalid => TrimPeriod(finding.Cause),
            ExtensionInspectFindingCode.DependencyChanged => DependencyChanged(result, id),
            ExtensionInspectFindingCode.DependencyConflict => TrimPeriod(finding.Cause),
            ExtensionInspectFindingCode.DependencyCycle => DependencyCycle(result, result.Subject.Id ?? id, finding),
            ExtensionInspectFindingCode.DependencyIncomplete =>
                global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatTheDependencyOfCouldNotBeResolvedItWasNotCompared($"{finding.Dependency ?? "dependency"}", $"{id}", $"{TrimPeriod(finding.Cause)}"),
            ExtensionInspectFindingCode.OwnershipConflict => TrimPeriod(finding.Cause),
            ExtensionInspectFindingCode.OperationFailed => TrimPeriod(finding.Cause),
            ExtensionInspectFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageExtensionInspectWasCancelled(),
            _ => TrimPeriod(finding.Cause),
        };
    }

    internal static bool IsFileDifferenceCode(string code)
        => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectWording.IsFileDifferenceCode(code);

    internal static CliNextAction? Next(ExtensionInspectResult result)
    {
        if (result.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.InvalidStableId or ExtensionInspectFindingCode.PackageUnavailable))
        {
            return new CliNextAction("open-forge extension list", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageListAvailableExtensions());
        }

        if (result.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.IdentityAmbiguous
            or ExtensionInspectFindingCode.PathUnavailable
            or ExtensionInspectFindingCode.FingerprintUnavailable
            or ExtensionInspectFindingCode.DependencyIncomplete))
        {
            return new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageInspectTheRecordedWorkspaceFacts());
        }

        if (result.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.WorkspaceUnavailable
            or ExtensionInspectFindingCode.WorkspaceUnsafe
            or ExtensionInspectFindingCode.SourceAmbiguous
            or ExtensionInspectFindingCode.SourceOverlap
            or ExtensionInspectFindingCode.SourceInvalid
            or ExtensionInspectFindingCode.PackageInvalid
            or ExtensionInspectFindingCode.DependencyCycle
            or ExtensionInspectFindingCode.DependencyConflict
            or ExtensionInspectFindingCode.PathInvalid
            or ExtensionInspectFindingCode.OwnershipConflict
            or ExtensionInspectFindingCode.GeneratedBoundaryInvalid
            or ExtensionInspectFindingCode.OperationFailed
            or ExtensionInspectFindingCode.Interrupted))
        {
            return null;
        }

        var id = result.Subject.Id ?? result.Subject.Supplied;
        if (id is not null && result.Findings.Any(finding => finding.Code == ExtensionInspectFindingCode.PathRetired)
            && !result.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.PathChanged
                or ExtensionInspectFindingCode.PathMissing
                or ExtensionInspectFindingCode.PathNew
                or ExtensionInspectFindingCode.DependencyChanged))
        {
            return new CliNextAction($"open-forge extension update {id} --prune --dry-run", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageReviewTheRetiredFilesBeforePruningThem());
        }

        if (id is not null && result.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.PathChanged
            or ExtensionInspectFindingCode.PathMissing
            or ExtensionInspectFindingCode.PathNew
            or ExtensionInspectFindingCode.PathRetired
            or ExtensionInspectFindingCode.DependencyChanged))
        {
            return new CliNextAction($"open-forge extension update {id} --dry-run", global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.MessageReviewTheSelectedPackageChangesBeforeApplyingThem());
        }

        return null;
    }

    internal static string CountLabel(int? value, string singular, string plural)
        => value is 1 ? singular : plural;

    internal static string InvalidStableId(string? value)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatIsNotAValidExtensionIdUseLowercaseLettersDigitsAndHyphens($"{value ?? "The supplied value"}");

    private static string CompleteHeadline(ExtensionInspectResult result)
    {
        var id = result.Subject.Id ?? result.Subject.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtension();
        if (result.Installed.Package is null && result.Available.Package is { } available)
        {
            if (result.Findings.Any(finding => finding.Code == ExtensionInspectFindingCode.OwnershipObservation))
            {
                return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsAvailableNoOwnershipRecordExistsSoInstallationCannotBeChecked($"{id}", $"{Version(available.Version)}");
            }

            return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsAvailableAndNotInstalled($"{id}", $"{Version(available.Version)}");
        }

        if (result.Installed.Package is { } installed && result.Available.Package is { } package)
        {
            return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsInstalledAndMatchesThePackage($"{id}", $"{Version(installed.Version)}");
        }

        return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsAvailable($"{id}");
    }

    private static string AttentionHeadline(ExtensionInspectResult result)
    {
        var id = result.Subject.Id ?? result.Subject.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtension();
        var version = Version(result.Installed.Package?.Version);
        var count = result.Comparison.Paths.Count(path => path.Relation is ExtensionInspectPathRelation.Changed
            or ExtensionInspectPathRelation.Missing
            or ExtensionInspectPathRelation.New
            or ExtensionInspectPathRelation.Retired);
        var headline = global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsInstalledNeedAttention($"{id}", $"{version}", $"{count}", $"{CliText.Plural(count, "file")}");
        if (result.Available.Package is { } available
            && !string.Equals(result.Installed.Package?.Version, available.Version, StringComparison.Ordinal))
        {
            headline += global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatVersionIsAvailable($"{Version(available.Version)}");
        }

        return headline;
    }

    private static string IncompleteHeadline(ExtensionInspectResult result)
    {
        var id = result.Subject.Id ?? result.Subject.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleExtension();
        var limitation = result.Findings
            .Select(finding => finding.Cause)
            .FirstOrDefault(cause => !string.IsNullOrWhiteSpace(cause)) ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelSomeFactsWereUnavailable();
        return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatIsInstalledButTheComparisonCouldNotFinish($"{id}", $"{TrimPeriod(limitation)}");
    }

    private static string InvalidHeadline(ExtensionInspectResult result)
    {
        var reference = result.Subject.Supplied ?? result.Subject.Id ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelTheRequest();
        var finding = result.Findings.FirstOrDefault();
        var problem = finding is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheInputIsInvalid() : Message(result, finding);
        return global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInspect($"{reference}", $"{TrimPeriod(problem)}");
    }

    private static string BlockedHeadline(ExtensionInspectResult result)
    {
        var reference = result.Subject.Id ?? result.Subject.Supplied ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelTheRequest();
        var finding = result.Findings.FirstOrDefault();
        var reason = finding is null ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelTheRequestIsBlocked() : Message(result, finding);
        return global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInspect($"{reference}", $"{TrimPeriod(reason)}");
    }

    private static string FailedHeadline(ExtensionInspectResult result)
    {
        var reason = result.Findings.Select(finding => finding.Cause).FirstOrDefault(cause => !string.IsNullOrWhiteSpace(cause))
            ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheOperationFailed();
        return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatExtensionInspectStoppedBecauseOfAnUnexpectedError($"{TrimPeriod(reason)}");
    }

    private static string DependencyChanged(ExtensionInspectResult result, string id)
    {
        var dependency = result.Comparison.Dependencies.Intended
            .Except(result.Comparison.Dependencies.Current, StringComparer.Ordinal)
            .FirstOrDefault() ?? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelADependency();
        return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatNowRequiresWhichTheInstalledVersionDidNot($"{id}", $"{dependency}");
    }

    private static string DependencyCycle(
        ExtensionInspectResult result,
        string id,
        ExtensionInspectFinding finding)
    {
        foreach (var edge in result.Dependencies.Declared)
        {
            if (result.Dependencies.Declared.Any(reverse => reverse.From == edge.To && reverse.To == edge.From))
            {
                return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatRequiresWhichRequires($"{edge.From}", $"{edge.To}", $"{edge.From}");
            }
        }

        const string marker = " at '";
        var start = finding.Cause.IndexOf(marker, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += marker.Length;
            var end = finding.Cause.IndexOf('\'', start);
            if (end > start)
            {
                return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatRequiresWhichRequires($"{id}", $"{finding.Cause[start..end]}", $"{id}");
            }
        }

        return global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatHasADependencyCycle($"{id}");
    }

    private static string? CommonDirectory(IReadOnlyList<string> paths)
    {
        var directories = paths
            .Select(path => path.LastIndexOf('/') is var index && index >= 0
                ? path[..index]
                : string.Empty)
            .ToArray();
        if (directories.Length == 0 || directories.Any(string.IsNullOrEmpty))
        {
            return null;
        }

        var parts = directories[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = parts.Length;
        foreach (var directory in directories.Skip(1))
        {
            var other = directory.Split('/', StringSplitOptions.RemoveEmptyEntries);
            count = Math.Min(count, other.Length);
            for (var index = 0; index < count; index++)
            {
                if (!string.Equals(parts[index], other[index], StringComparison.Ordinal))
                {
                    count = index;
                    break;
                }
            }
        }

        return count == 0 ? null : string.Join('/', parts.Take(count));
    }

    private static string TrimPeriod(string value) => CliFindingWording.PlainCause(value);
}
