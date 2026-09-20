using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Wording;

internal static class ExtensionListWording
{
    private const string ExpectedManifest = "Expected extension.json with id, name, description, version, dependencies.";

    internal static string Summary(int installed, int available)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatTheResultContainsInstalledAndAvailable(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{installed}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(installed, "package")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{available}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(available, "package")}"));

    internal static string Installed() => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInstalled();

    internal static string InstalledMarker() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInstalled();

    internal static string AvailableEmbedded() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleAvailableBundledWithThisCli();

    internal static string AvailableFrom(string path) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.AvailableFrom(path);

    internal static string AvailableHeading(ExtensionListSource? source)
        => source switch
        {
            { Kind: ExtensionListSourceKind.EmbeddedCatalogue } => AvailableEmbedded(),
            { Identity: { } path } => AvailableFrom(path),
            _ => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleAvailable(),
        };

    internal static string InstalledNoOwnership() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.PlaceholderNoOwnershipRecordSoInstalledPackagesCannotBeListed();

    internal static string InstalledEmpty() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone();

    internal static string InstalledUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    internal static string AvailableEmpty() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone();

    internal static string AvailableUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    internal static string SourceUnavailableLine() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleSourceUnavailable();

    internal static string RecordedSource(string path) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.RecordedSource(path);

    internal static string Version(string? version) => version ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown();

    internal static string Source(string path) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.Source(path);

    internal static string SourceKind(ExtensionListSourceKind kind)
        => kind switch
        {
            ExtensionListSourceKind.EmbeddedCatalogue => "embedded-catalogue",
            ExtensionListSourceKind.Package => "package",
            ExtensionListSourceKind.Catalogue => "catalogue",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined."),
        };

    internal static string InstalledRowNote(ExtensionListInstalledRow row)
        => row.SourceState switch
        {
            ExtensionListSourceState.Missing => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceMissing(),
            ExtensionListSourceState.Invalid => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceInvalid(),
            ExtensionListSourceState.Blocked => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceBlocked(),
            ExtensionListSourceState.Unavailable or ExtensionListSourceState.Interrupted => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceUnavailable(),
            ExtensionListSourceState.Complete => CoverageNote(row),
            null when row.SourceAvailable is false => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceUnavailable(),
            null => CoverageNote(row),
            _ => throw new ArgumentOutOfRangeException(nameof(row), row.SourceState, "The Extension source state is not defined."),
        };

    private static string CoverageNote(ExtensionListInstalledRow row)
        => row.Coverage switch
        {
            ExtensionListInstalledCoverage.Changed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelFilesChanged(),
            ExtensionListInstalledCoverage.Missing => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelFilesMissing(),
            ExtensionListInstalledCoverage.Incomplete => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelFilesUnavailable(),
            ExtensionListInstalledCoverage.Blocked => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelFilesCouldNotBeChecked(),
            _ => row.PackageState switch
            {
                ExtensionListInstalledPackageState.Missing => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourcePackageMissing(),
                ExtensionListInstalledPackageState.VersionMismatch => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceVersionDiffers(),
                ExtensionListInstalledPackageState.DependencyMismatch => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceDependenciesDiffer(),
                ExtensionListInstalledPackageState.Ambiguous => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourcePackageIsAmbiguous(),
                ExtensionListInstalledPackageState.SourceUnavailable => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelSourceUnavailable(),
                _ => string.Empty,
            },
        };

    internal static string Coverage(ExtensionListInstalledCoverage coverage)
        => coverage switch
        {
            ExtensionListInstalledCoverage.Complete => "complete",
            ExtensionListInstalledCoverage.Changed => "files changed",
            ExtensionListInstalledCoverage.Missing => "files missing",
            ExtensionListInstalledCoverage.Incomplete => "incomplete",
            ExtensionListInstalledCoverage.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Extension installed coverage is not defined."),
        };

    internal static string LockCoverage(ExtensionListInstalledCoverage coverage)
        => coverage == ExtensionListInstalledCoverage.Complete
            ? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageTheInstalledFilesMatchTheirRecordedSources()
            : global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatInstalledFileCoverage($"{Coverage(coverage)}");

    internal static string FileCount(int count)
        => string.Create(CultureInfo.InvariantCulture, $"{count} {Plural(count, "file")}");

    internal static string Packages(int count)
        => string.Create(CultureInfo.InvariantCulture, $"{count} {Plural(count, "package")}");

    internal static string AvailablePackagesSummary(int count)
        => count > 1 ? $"({Packages(count)})" : string.Empty;

    internal static string Dependencies(IReadOnlyList<string> dependencies)
        => dependencies.Count == 0 ? string.Empty : global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatNeeds($"{string.Join(", ", dependencies)}");

    internal static string InstalledVersion(string version) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.InstalledVersion(version);

    internal static string FindingTitle(ExtensionListFindingCode code)
        => code switch
        {
            ExtensionListFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            ExtensionListFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            ExtensionListFindingCode.SourceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensionSourceIsUnavailable(),
            ExtensionListFindingCode.SourceInvalid => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionSourceIsInvalid(),
            ExtensionListFindingCode.SourceBlocked => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleExtensionSourceIsBlocked(),
            ExtensionListFindingCode.InstalledSourceMissing => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledExtensionSourceIsMissing(),
            ExtensionListFindingCode.InstalledSourceUnavailable => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledExtensionSourceIsUnavailable(),
            ExtensionListFindingCode.InstalledSourceInvalid => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledExtensionSourceIsInvalid(),
            ExtensionListFindingCode.InstalledSourceBlocked => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledExtensionSourceIsBlocked(),
            ExtensionListFindingCode.InstalledFilesChanged => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledFilesChanged(),
            ExtensionListFindingCode.InstalledFilesMissing => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledFilesAreMissing(),
            ExtensionListFindingCode.InstalledTargetUnavailable => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledFileIsUnavailable(),
            ExtensionListFindingCode.InstalledTargetBlocked => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledFileCouldNotBeChecked(),
            ExtensionListFindingCode.InstalledFilesUnavailable => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleInstalledFilesCouldNotBeCompared(),
            ExtensionListFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
            ExtensionListFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleExtensionListFailed(),
            ExtensionListFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleExtensionListWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension List finding code is not defined."),
        };

    internal static string InvalidInput(string reason) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatCannotListExtensions($"{TrimPeriod(reason)}");

    internal static string SourceUnavailable(string path) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.SourceUnavailable(path);

    internal static string SourceInvalid(string path, string reason)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatIsNotAnExtensionPackageOrPackageFolder($"{path}", $"{TrimPeriod(reason)}", $"{ExpectedManifest}");

    internal static string SourceBlocked(string path, string reason)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatCannotBeUsedAsASource($"{path}", $"{BlockedReason(reason)}");

    internal static string InstalledSourceMissing(string id, string path)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.InstalledSourceMissing(id, path);

    internal static string InstalledSourceUnavailable(string id, string path)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.InstalledSourceUnavailable(id, path);

    internal static string InstalledFilesChanged(string id, int count)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatInstalledByChangedSinceInstallation(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? "has" : "have")}"));

    internal static string InstalledFilesMissing(string id, int count)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatInstalledByMissing(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? "is" : "are")}"));

    internal static string InstalledFilesUnavailable(string id)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.InstalledFilesUnavailable(id);

    internal static string InstalledTargetBlocked(string path)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListWording.InstalledTargetBlocked(path);

    internal static string CannotList(string reason) => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatCannotListExtensions($"{TrimPeriod(reason)}");

    internal static string Interrupted() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageExtensionListWasCancelled();

    internal static CliNextAction Inspect(string id)
        => new($"open-forge extension inspect {id}", global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageInspectTheInstalledExtensionFacts());

    internal static string NextInstall() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageInstallOneOfTheAvailableExtensions();

    internal static string OwnershipMissing() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageNoOwnershipRecordExistsSoInstalledPackagesCannotBeListedFromIt();

    internal static string IncompleteHeadline() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageExtensionListCouldNotBeReadCompletely();

    internal static string BlockedHeadline(string reason) => CannotList(reason);

    internal static string FailedHeadline(string reason)
        => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListPhrases.FormatExtensionListStoppedBecauseOfAnUnexpectedError($"{TrimPeriod(reason)}");

    internal static string InvalidHeadline(string reason) => CannotList(reason);

    internal static string WarningHeadline() => global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageExtensionListCompletedWithWarnings();

    internal static string CancelledHeadline() => Interrupted();

    private static string Plural(int count, string singular) => CliText.Plural(count, singular);

    private static string TrimPeriod(string value) => CliFindingWording.PlainCause(value);

    private static string BlockedReason(string value)
    {
        var reason = TrimPeriod(value);
        return reason.Contains("overlaps", StringComparison.OrdinalIgnoreCase)
            ? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelItIsInsideTheWorkspace()
            : reason.Contains("aliases", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("unsafe", StringComparison.OrdinalIgnoreCase)
                ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelItResolvesToAnUnsafeLocation()
                : reason;
    }
}
