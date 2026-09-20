using System.Globalization;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Wording;

internal static class StatusWording
{
    internal static string InstalledCurrent() => global::OpenForge.Cli.OutputText.Status.StatusText.MessageOpenForgeIsInstalledAndCurrent();

    internal static string NotInstalled(string workspace)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.NotInstalled(workspace);

    internal static string Attention(long count, bool things)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatOpenForgeIsInstalledButAttention(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, things ? "thing" : "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? "needs" : "need")}"));

    internal static string Incomplete(long attentionCount, bool things)
        => attentionCount == 0
            ? global::OpenForge.Cli.OutputText.Status.StatusText.MessageOpenForgeIsInstalledButSomeChecksCouldNotFinish()
            : global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatOpenForgeIsInstalledButAttentionAndSomeChecksCouldNotFinish(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{attentionCount}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(attentionCount, things ? "thing" : "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(attentionCount == 1 ? "needs" : "need")}"));

    internal static string Invalid(string problem)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatCannotCheckStatus($"{problem.TrimEnd('.')}");

    internal static string Blocked(string reason)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatCannotCheckThisWorkspace($"{reason.TrimEnd('.')}");

    internal static string Failed(string reason)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatStatusStoppedBecauseOfAnUnexpectedError($"{reason.TrimEnd('.')}");

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Status.StatusText.MessageStatusWasCancelled();

    internal static string StartupContextHeading() => global::OpenForge.Cli.OutputText.Status.StatusText.TitleStartupContext();

    internal static string ShippedByCliLabel() => global::OpenForge.Cli.OutputText.Status.StatusText.HeadingShippedByThisCli();

    internal static string WorkspaceMeasurementLabel() => global::OpenForge.Cli.OutputText.Status.StatusText.HeadingThisWorkspace();

    internal static string MayLoadAgainLabel() => global::OpenForge.Cli.OutputText.Status.StatusText.HeadingMayLoadAgainLater();

    internal static string AllRoutedFilesLabel() => global::OpenForge.Cli.OutputText.Status.StatusText.TitleAllRoutedFiles();

    internal static string RoutesHeading() => global::OpenForge.Cli.OutputText.Status.StatusText.TitleRoutes();

    internal static string FrameworkFilesHeading() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles();

    internal static string EntriesSectionsHeading() => global::OpenForge.Cli.OutputText.Status.StatusText.TitleEntriesSections();

    internal static string LargestMayLoadAgainSourcesHeading() => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLargestMayLoadAgainSources();

    internal static string RecoveryDataHeading() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryData();

    internal static string ExtensionFilesHeading(string id)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.ExtensionFilesHeading(id);

    internal static string LibraryLinksHeading(string id)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.LibraryLinksHeading(id);

    internal static string StartupReads(long files, long routedFiles, long tokens)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatStartupReadsOfRoutedFiles(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{routedFiles}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Tokens(tokens)}"));

    internal static string ExtensionList(string names)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.ExtensionList(names);

    internal static string LibraryList(string names)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.LibraryList(names);

    internal static string MeasurementSummary(StatusDataMeasurement measurement)
        => $"{Count(measurement.Files, measurement.FilesState, "file", "files")}, {TokenCount(measurement.Tokens, measurement.TokensState)}";

    internal static string AllRouted(StatusDataMeasurement measurement, StatusDecimalValue startupShare)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatStartupIs($"{AllRoutedFilesLabel()}", $"{MeasurementSummary(measurement)}", $"{Percentage(startupShare)}");

    internal static string AddedSinceShipped(StatusDataMeasurement measurement)
    {
        var parts = new List<string>();
        if (measurement.FilesState == StatusValueState.Available
            && measurement.Files is { } files
            && files != 0)
        {
            parts.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"{files} {CliText.Plural(files, "file")}"));
        }

        if (measurement.TokensState == StatusValueState.Available
            && measurement.Tokens is { } tokens
            && tokens != 0)
        {
            parts.Add(CliText.Tokens(tokens));
        }

        return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatAddedSinceShipped($"{string.Join(", ", parts)}");
    }

    internal static string RoutesSummary(
        StatusIntegerValue roots,
        StatusIntegerValue currentEntries,
        IReadOnlyList<string> added,
        IReadOnlyList<string> removed)
    {
        var parts = new List<string>
        {
            Count(roots, global::OpenForge.Cli.OutputText.Status.StatusText.LabelRootCategory(), global::OpenForge.Cli.OutputText.Status.StatusText.LabelRootCategories()),
        };
        if (currentEntries.State != StatusValueState.NotApplicable)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Status.StatusPhrases.CountsCurrent($"{Count(currentEntries, "Entries section", "Entries sections")}"));
        }

        if (added.Count > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatAdded($"{string.Join(", ", added)}"));
        }

        if (removed.Count > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatRemoved($"{string.Join(", ", removed)}"));
        }

        return $"{RoutesHeading()}: {string.Join(", ", parts)}";
    }

    internal static string? FrameworkSummary(
        StatusIntegerValue current,
        StatusIntegerValue changed,
        StatusIntegerValue missing)
    {
        if (current.State == StatusValueState.NotApplicable)
        {
            return null;
        }

        if (current.State == StatusValueState.Unavailable)
        {
            return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FrameworkUnavailableSummary($"{FrameworkFilesHeading()}");
        }

        if (current.Value is 0
            && changed.Value is 0
            && missing.Value is 0)
        {
            return null;
        }

        return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FrameworkCurrentSummary($"{FrameworkFilesHeading()}", $"{current.Value!.Value.ToString(CultureInfo.InvariantCulture)}");
    }

    internal static string ExtensionSummary(StatusDataExtension extension)
    {
        var name = ExtensionName(extension);
        if (extension.FilesState == StatusValueState.Available
            && extension.TextCurrentFiles is { } current
            && current > 0)
        {
            return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatExtensionsCurrent($"{name}", $"{current}", $"{CliText.Plural(current, "file")}");
        }

        if (extension.FilesState == StatusValueState.Unavailable)
        {
            return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatExtensionsFilesUnavailable($"{name}");
        }

        return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatExtensions($"{name}");
    }

    internal static string LibrarySummary(StatusDataLibrary library)
    {
        var name = LibraryName(library, minimal: false);
        if (library.LinksState == StatusValueState.Available
            && library.TextCurrentLinks is { } current
            && current > 0)
        {
            return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatLibrariesCurrent($"{name}", $"{current}", $"{CliText.Plural(current, "link")}");
        }

        if (library.LinksState == StatusValueState.Unavailable)
        {
            return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatLibrariesLinksUnavailable($"{name}");
        }

        return global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatLibraries($"{name}");
    }

    internal static string ExtensionName(StatusDataExtension extension)
        => extension.Version is { } version
            ? $"{extension.Id} {version}"
            : extension.Id;

    internal static string LibraryName(StatusDataLibrary library, bool minimal)
    {
        var suffix = minimal
            && library.LinksState == StatusValueState.Available
            && library.TextCurrentLinks is { } count
            && count > 0
                ? string.Create(CultureInfo.InvariantCulture, $" ({count} {CliText.Plural(count, "link")})")
                : string.Empty;
        if (minimal)
        {
            return library.Id + suffix;
        }

        return $"{library.Id}, {library.SourceRoot} -> {library.DestinationRoot}";
    }

    internal static string ManagedTargetRow(string state)
        => state;

    internal static string LibraryLinkRow(
        string state,
        string expected,
        string? observed)
        => observed is { }
            ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatExpectedObserved($"{state}", $"{expected}", $"{observed}")
            : global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatExpected($"{state}", $"{expected}");

    internal static string RecoveryRow(StatusDataRecoveryCandidate candidate)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatRecoveryBundle($"{HumanState(candidate.CandidateKind)}", $"{HumanState(candidate.IntegrityState)}");

    internal static string NoRecovery() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone();

    internal static string StartupUnavailable(string? path)
        => path is { Length: > 0 }
            ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatStartupContextCouldNotBeMeasuredCompletelyCouldNotBeReadCompletely($"{path}")
            : global::OpenForge.Cli.OutputText.Status.StatusText.MessageStartupContextCouldNotBeMeasuredCompletely();

    internal static string CurrentStartupUnavailable()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheCurrentStartupContextCouldNotBeMeasured();

    internal static string ContinuityUnavailable()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheMayLoadAgainContextCouldNotBeMeasured();

    internal static string RootCategoriesUnavailable()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheRootCategoriesCouldNotBeReadFromAgentsLoaderMd();

    internal static string EntryUnavailable()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageAgentsMdCouldNotBeRead();

    internal static string EmbeddedFrameworkUnavailable()
        => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkBundledInThisCliCouldNotBeReadCompletely();

    internal static string RecoveryCatalogueUnavailable(string? path)
        => path is { Length: > 0 }
            ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheRecoveryStoreAtCouldNotBeRead($"{path}")
            : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheRecoveryStoreCouldNotBeRead();

    internal static string GeneratedNavigation(StatusFindingCode code, string? path)
        => code switch
        {
            StatusFindingCode.GeneratedNavigationChanged => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheEntriesSectionOfIsStale($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheEntriesSectionIsStale(),
            StatusFindingCode.GeneratedNavigationMissing => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatHasNoEntriesSection($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageThereIsNoEntriesSection(),
            StatusFindingCode.GeneratedNavigationUnavailable => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheEntriesSectionOfCouldNotBeRead($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheEntriesSectionCouldNotBeRead(),
            StatusFindingCode.GeneratedNavigationBlocked => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheEntriesSectionOfCouldNotBeReadSafely($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheEntriesSectionCouldNotBeReadSafely(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The generated navigation finding is not defined."),
        };

    internal static string GeneratedNavigationMetadataInvalid(string? path, string cause)
        => global::OpenForge.Cli.OutputText.Status.StatusWording.GeneratedNavigationMetadataInvalid(
            path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource(),
            TrimSentence(cause));

    internal static string FrameworkOwnership()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageNoOwnershipRecordExistsSoFrameworkFilesCannotBeCheckedAgainstIt();

    internal static string ExtensionOwnership()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageNoOwnershipRecordExistsSoInstalledExtensionsCannotBeListedFromIt();

    internal static string LibraryOwnership()
        => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoOwnershipRecordExistsSoLibrariesCannotBeListedFromIt();

    internal static string LifecycleUnavailable(string subject, string? cause)
        => string.IsNullOrWhiteSpace(cause)
            ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatAgentsOpenForgeLockJsonCouldNotBeReadCompletelyFor($"{subject}")
            : global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatAgentsOpenForgeLockJsonCannotBeUsedFor($"{subject}", $"{TrimSentence(cause)}");

    internal static string LifecycleBlocked(string subject, string? cause)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatAgentsOpenForgeLockJsonIsInvalidFor($"{subject}", $"{TrimSentence(cause ?? "the recorded data is unsafe")}");

    internal static string FrameworkTarget(StatusFindingCode code, string? cause)
        => code switch
        {
            StatusFindingCode.FrameworkTargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChangedSinceItWasInstalled(),
            StatusFindingCode.FrameworkTargetMissing => global::OpenForge.Cli.OutputText.Status.StatusText.LabelMissingItWasInstalledByTheFramework(),
            StatusFindingCode.FrameworkTargetUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeRead(),
            StatusFindingCode.FrameworkTargetBlocked => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatCouldNotBeCheckedSafely($"{TrimSentence(cause ?? "its location could not be verified")}"),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Framework target finding is not defined."),
        };

    internal static string ExtensionSourceUnavailable(string id, string? source)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheSourceOfTheExtensionCannotBeReadSoItsFilesWereNotCompared($"{id}", $"{source ?? "the recorded source"}");

    internal static string ExtensionTarget(StatusFindingCode code, string? owner, string? cause)
        => code switch
        {
            StatusFindingCode.ExtensionTargetChanged => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatChangedSinceItWasInstalledBy($"{owner ?? "an Extension"}"),
            StatusFindingCode.ExtensionTargetMissing => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatMissingItWasInstalledBy($"{owner ?? "an Extension"}"),
            StatusFindingCode.ExtensionTargetUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeRead(),
            StatusFindingCode.ExtensionTargetBlocked => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatCouldNotBeCheckedSafely($"{TrimSentence(cause ?? "its location could not be verified")}"),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension target finding is not defined."),
        };

    internal static string Recovery(StatusFindingCode code, string? path)
        => code switch
        {
            StatusFindingCode.RecoveryCandidateVerified => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatARecoveryBundleFromAnEarlierCommandIsKeptAt($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageARecoveryBundleFromAnEarlierCommandIsKept(),
            StatusFindingCode.RecoveryDraftIncomplete => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatAnUnfinishedRecoveryDraftIsAtACommandDidNotFinish($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageAnUnfinishedRecoveryDraftIsPresentACommandDidNotFinish(),
            StatusFindingCode.RecoveryFinalMalformed => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheRecoveryBundleAtIsDamagedAndCannotBeUsed($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheRecoveryBundleIsDamagedAndCannotBeUsed(),
            StatusFindingCode.RecoveryFinalUnsupported => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheRecoveryBundleAtWasWrittenByAnUnsupportedVersion($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheRecoveryBundleWasWrittenByAnUnsupportedVersion(),
            StatusFindingCode.RecoveryFinalUnavailable => path is { Length: > 0 }
                ? global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheRecoveryBundleAtCouldNotBeRead($"{path}")
                : global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheRecoveryBundleCouldNotBeRead(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The recovery finding is not defined."),
        };

    internal static string LibraryRecord(StatusFindingCode code, string? cause)
        => code switch
        {
            StatusFindingCode.LibraryRecordMalformed => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheLibrarySectionOfAgentsOpenForgeLockJsonIsInvalid($"{TrimSentence(cause ?? "the record is malformed")}"),
            StatusFindingCode.LibraryRecordUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.MessageTheLibrarySectionOfAgentsOpenForgeLockJsonCouldNotBeRead(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library record finding is not defined."),
        };

    internal static string LibrarySourceRoot(StatusFindingCode code, string id, string? path)
        => code switch
        {
            StatusFindingCode.LibrarySourceRootInvalid => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheSourceFolderOfTheLibraryIsNotAFolderInsideTheWorkspace($"{id}", $"{path ?? "the recorded source"}"),
            StatusFindingCode.LibrarySourceRootAliased => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheSourceFolderOfTheLibraryResolvesToAnAmbiguousLocation($"{id}", $"{path ?? "the recorded source"}"),
            StatusFindingCode.LibrarySourceRootUnavailable => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatTheSourceFolderOfTheLibraryCannotBeRead($"{id}", $"{path ?? "the recorded source"}"),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library source-root finding is not defined."),
        };

    internal static string LibraryProjection(StatusFindingCode code, string id, string? cause)
        => code switch
        {
            StatusFindingCode.LibraryProjectionMissing => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatMissingItIsALinkOfTheLibrary($"{id}"),
            StatusFindingCode.LibraryProjectionChanged => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatIsNoLongerTheLinkTheLibraryCreated($"{id}"),
            StatusFindingCode.LibraryProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeChecked(),
            StatusFindingCode.LibraryProjectionBlocked => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatCouldNotBeCheckedSafely($"{TrimSentence(cause ?? "its location could not be verified")}"),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library projection finding is not defined."),
        };

    internal static string WorkspaceUnavailable()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageDoesNotExistOrCannotBeRead();

    internal static string WorkspaceNotDirectory()
        => global::OpenForge.Cli.OutputText.Status.StatusText.MessageIsNotADirectory();

    internal static string WorkspaceUnsafe(string cause)
        => global::OpenForge.Cli.OutputText.Status.StatusPhrases.FormatItsLocationCouldNotBeVerified($"{TrimSentence(cause)}");

    internal static string FindingTitle(StatusFindingCode code) => code switch
    {
        StatusFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        StatusFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        StatusFindingCode.WorkspaceNotDirectory => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsNotADirectory(),
        StatusFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        StatusFindingCode.EntryUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleEntryIsUnavailable(),
        StatusFindingCode.EmbeddedFrameworkUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkIsUnavailable(),
        StatusFindingCode.ContextInventoryIncomplete => global::OpenForge.Cli.OutputText.Status.StatusText.TitleStartupContextIsIncomplete(),
        StatusFindingCode.StartupContextUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleStartupContextIsUnavailable(),
        StatusFindingCode.ContinuityContextUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleMayLoadAgainContextIsUnavailable(),
        StatusFindingCode.RootCategoriesUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleRootCategoriesAreUnavailable(),
        StatusFindingCode.GeneratedNavigationChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsStale(),
        StatusFindingCode.GeneratedNavigationMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsMissing(),
        StatusFindingCode.GeneratedNavigationUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        StatusFindingCode.GeneratedNavigationBlocked => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        StatusFindingCode.GeneratedNavigationMetadataInvalid => global::OpenForge.Cli.OutputText.Status.StatusWording.GeneratedNavigationMetadataInvalidTitle(),
        StatusFindingCode.FrameworkOwnershipObservation => global::OpenForge.Cli.OutputText.Status.StatusText.TitleFrameworkOwnershipIsUnavailable(),
        StatusFindingCode.ExtensionOwnershipObservation => global::OpenForge.Cli.OutputText.Status.StatusText.TitleExtensionOwnershipIsUnavailable(),
        StatusFindingCode.LibraryOwnershipObservation => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibraryOwnershipIsUnavailable(),
        StatusFindingCode.FrameworkLifecycleUntrusted => global::OpenForge.Cli.OutputText.Status.StatusText.TitleFrameworkFilesCannotBeChecked(),
        StatusFindingCode.FrameworkLifecycleIncomplete => global::OpenForge.Cli.OutputText.Status.StatusText.TitleFrameworkFilesCouldNotBeCheckedCompletely(),
        StatusFindingCode.FrameworkLifecycleBlocked => global::OpenForge.Cli.OutputText.Status.StatusText.TitleFrameworkFilesCannotBeCheckedSafely(),
        StatusFindingCode.FrameworkTargetChanged => global::OpenForge.Cli.OutputText.Status.StatusText.TitleChangedSinceItWasInstalled(),
        StatusFindingCode.FrameworkTargetMissing => global::OpenForge.Cli.OutputText.Status.StatusText.TitleMissingManagedFile(),
        StatusFindingCode.FrameworkTargetUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleManagedFileIsUnavailable(),
        StatusFindingCode.FrameworkTargetBlocked => global::OpenForge.Cli.OutputText.Status.StatusText.TitleManagedFileIsUnsafe(),
        StatusFindingCode.ExtensionLifecycleUntrusted => global::OpenForge.Cli.OutputText.Status.StatusText.TitleExtensionsCannotBeChecked(),
        StatusFindingCode.ExtensionLifecycleIncomplete => global::OpenForge.Cli.OutputText.Status.StatusText.TitleExtensionsCouldNotBeCheckedCompletely(),
        StatusFindingCode.ExtensionLifecycleBlocked => global::OpenForge.Cli.OutputText.Status.StatusText.TitleExtensionsCannotBeCheckedSafely(),
        StatusFindingCode.ExtensionSourceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensionSourceIsUnavailable(),
        StatusFindingCode.ExtensionTargetChanged => global::OpenForge.Cli.OutputText.Status.StatusText.TitleChangedSinceItWasInstalled(),
        StatusFindingCode.ExtensionTargetMissing => global::OpenForge.Cli.OutputText.Status.StatusText.TitleMissingManagedFile(),
        StatusFindingCode.ExtensionTargetUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleManagedFileIsUnavailable(),
        StatusFindingCode.ExtensionTargetBlocked => global::OpenForge.Cli.OutputText.Status.StatusText.TitleManagedFileIsUnsafe(),
        StatusFindingCode.RecoveryCandidateVerified => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsKept(),
        StatusFindingCode.RecoveryDraftIncomplete => global::OpenForge.Cli.OutputText.Status.StatusText.TitleRecoveryDraftIsIncomplete(),
        StatusFindingCode.RecoveryFinalMalformed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsDamaged(),
        StatusFindingCode.RecoveryFinalUnsupported => global::OpenForge.Cli.OutputText.Status.StatusText.TitleRecoveryBundleIsUnsupported(),
        StatusFindingCode.RecoveryFinalUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsUnavailable(),
        StatusFindingCode.RecoveryCatalogueUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        StatusFindingCode.LibraryRecordMalformed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsInvalid(),
        StatusFindingCode.LibraryRecordUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraryRecordIsUnavailable(),
        StatusFindingCode.LibrarySourceRootInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibrarySourceFolderIsInvalid(),
        StatusFindingCode.LibrarySourceRootAliased => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibrarySourceFolderIsAmbiguous(),
        StatusFindingCode.LibrarySourceRootUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibrarySourceFolderIsUnavailable(),
        StatusFindingCode.LibraryProjectionMissing => global::OpenForge.Cli.OutputText.Status.StatusText.TitleMissingLibraryLink(),
        StatusFindingCode.LibraryProjectionChanged => global::OpenForge.Cli.OutputText.Status.StatusText.TitleChangedLibraryLink(),
        StatusFindingCode.LibraryProjectionUnavailable => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibraryLinkIsUnavailable(),
        StatusFindingCode.LibraryProjectionBlocked => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibraryLinkIsUnsafe(),
        StatusFindingCode.LibraryExtensionCollision => global::OpenForge.Cli.OutputText.Status.StatusText.TitleLibraryAndExtensionOverlap(),
        StatusFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Status.StatusText.TitleStatusFailed(),
        StatusFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Status.StatusText.TitleStatusWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Status finding code is not defined."),
    };

    internal static string? ActionCommand(StatusFindingCode code, string? owner, string? subject)
        => code switch
        {
            StatusFindingCode.FrameworkTargetChanged or StatusFindingCode.FrameworkTargetMissing => "open-forge update",
            StatusFindingCode.ExtensionTargetChanged or StatusFindingCode.ExtensionTargetMissing
                => $"open-forge extension update {owner ?? "<id>"}",
            StatusFindingCode.ExtensionSourceUnavailable
                => $"open-forge extension inspect {owner ?? subject ?? "<id>"}",
            StatusFindingCode.LibraryProjectionMissing
                => $"open-forge library sync {owner ?? "<id>"}",
            StatusFindingCode.LibraryProjectionChanged or StatusFindingCode.LibrarySourceRootInvalid
                or StatusFindingCode.LibrarySourceRootAliased
                or StatusFindingCode.LibrarySourceRootUnavailable
                => $"open-forge library inspect {owner ?? "<id>"}",
            StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing => "open-forge index",
            StatusFindingCode.RecoveryCandidateVerified or StatusFindingCode.RecoveryDraftIncomplete
                or StatusFindingCode.RecoveryFinalMalformed or StatusFindingCode.RecoveryFinalUnsupported => "open-forge cleanup",
            StatusFindingCode.RecoveryFinalUnavailable or StatusFindingCode.RecoveryCatalogueUnavailable
                or StatusFindingCode.ContextInventoryIncomplete or StatusFindingCode.StartupContextUnavailable
                or StatusFindingCode.ContinuityContextUnavailable or StatusFindingCode.RootCategoriesUnavailable
                or StatusFindingCode.FrameworkLifecycleUntrusted or StatusFindingCode.FrameworkLifecycleIncomplete
                or StatusFindingCode.FrameworkLifecycleBlocked or StatusFindingCode.ExtensionLifecycleUntrusted
                or StatusFindingCode.ExtensionLifecycleIncomplete or StatusFindingCode.ExtensionLifecycleBlocked
                or StatusFindingCode.FrameworkTargetUnavailable or StatusFindingCode.FrameworkTargetBlocked
                or StatusFindingCode.ExtensionTargetUnavailable or StatusFindingCode.ExtensionTargetBlocked
                or StatusFindingCode.LibraryProjectionUnavailable or StatusFindingCode.LibraryProjectionBlocked
                or StatusFindingCode.LibraryRecordUnavailable or StatusFindingCode.LibraryRecordMalformed
                or StatusFindingCode.WorkspaceUnavailable or StatusFindingCode.WorkspaceNotDirectory
                or StatusFindingCode.WorkspaceUnsafe or StatusFindingCode.EntryUnavailable
                or StatusFindingCode.EmbeddedFrameworkUnavailable or StatusFindingCode.LibraryExtensionCollision
                => "open-forge doctor",
            StatusFindingCode.InvalidInput => "open-forge status --help",
            StatusFindingCode.OperationFailed => "open-forge status --detail debug",
            StatusFindingCode.Interrupted => "open-forge status",
            _ => null,
        };

    internal static string ActionReason(StatusFindingCode code)
        => code switch
        {
            StatusFindingCode.FrameworkTargetChanged or StatusFindingCode.FrameworkTargetMissing
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageUpdateTheFrameworkFilesThatNeedAttention(),
            StatusFindingCode.ExtensionTargetChanged or StatusFindingCode.ExtensionTargetMissing
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageUpdateTheExtensionFileThatNeedsAttention(),
            StatusFindingCode.ExtensionSourceUnavailable
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageInspectTheExtensionSourceBeforeComparingItsFiles(),
            StatusFindingCode.LibraryProjectionMissing => global::OpenForge.Cli.OutputText.Status.StatusText.MessageSynchronizeTheMissingLibraryLink(),
            StatusFindingCode.LibraryProjectionChanged or StatusFindingCode.LibrarySourceRootInvalid
                or StatusFindingCode.LibrarySourceRootAliased or StatusFindingCode.LibrarySourceRootUnavailable
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageInspectTheLibrarySourceAndLinkBoundary(),
            StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageRebuildTheEntriesSection(),
            StatusFindingCode.RecoveryCandidateVerified or StatusFindingCode.RecoveryDraftIncomplete
                or StatusFindingCode.RecoveryFinalMalformed or StatusFindingCode.RecoveryFinalUnsupported
                => global::OpenForge.Cli.OutputText.Status.StatusText.MessageReviewAndRemoveTheRecoveryData(),
            StatusFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Status.StatusText.MessageCorrectTheStatusInputThenRerunTheRequest(),
            StatusFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Status.StatusText.MessageRerunTheSameStatusRequest(),
            _ => global::OpenForge.Cli.OutputText.Status.StatusText.MessageInspectTheReportedOperationalFactsBeforeRerunningStatus(),
        };

    internal static string InstallPreviewCommand() => "open-forge install --dry-run";

    internal static string InstallPreviewReason() => global::OpenForge.Cli.OutputText.Status.StatusText.MessagePreviewTheInstallationForThisWorkspace();

    internal static string NextReason(CliSemanticStatus status, StatusFindingCode code)
        => status == CliSemanticStatus.Invalid
            ? global::OpenForge.Cli.OutputText.Status.StatusText.MessageCorrectTheStatusInputThenRerunTheRequest()
            : ActionReason(code);

    internal static string TrimSentence(string value)
        => value.Trim().TrimEnd('.');

    internal static string Value(StatusIntegerValue value)
        => value.State == StatusValueState.Available && value.Value is { } number
            ? number.ToString(CultureInfo.InvariantCulture)
            : value.State switch
            {
                StatusValueState.NotApplicable => global::OpenForge.Cli.OutputText.Status.StatusText.LabelNotAvailable(),
                StatusValueState.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
                _ => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
            };

    internal static string Percentage(StatusDecimalValue value)
        => value.State == StatusValueState.Available && value.Value is { } number
            ? string.Create(CultureInfo.InvariantCulture, $"{number:0}%")
            : value.State == StatusValueState.NotApplicable ? global::OpenForge.Cli.OutputText.Status.StatusText.LabelNotAvailable() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable();

    internal static string MachineCode(StatusFindingCode code)
        => $"status.{StatusFindingVocabulary.ReadMachineName(code)}";

    internal static string Plural(long? value, string singular)
        => value is 1 ? singular : singular + "s";

    internal static string State(StatusValueState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusInstallationState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusLifecycleState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusSourceAvailability value) => CliReportVocabulary.Name(value);
    internal static string State(StatusTargetState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusGeneratedNavigationState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusManagedTargetKind value) => CliReportVocabulary.Name(value);
    internal static string State(StatusLibraryRecordState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusLibrarySourceRootState value) => CliReportVocabulary.Name(value);
    internal static string State(StatusRecoveryCandidateKind value) => CliReportVocabulary.Name(value);
    internal static string State(StatusRecoveryIntegrity value) => CliReportVocabulary.Name(value);

    internal static string HumanState(StatusTargetState value)
        => value switch
        {
            StatusTargetState.Current => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCurrent(),
            StatusTargetState.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChanged(),
            StatusTargetState.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
            StatusTargetState.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
            StatusTargetState.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value,
                "The managed target state is not defined."),
        };

    internal static string HumanState(StatusGeneratedNavigationState value)
        => value switch
        {
            StatusGeneratedNavigationState.Current => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCurrent(),
            StatusGeneratedNavigationState.Changed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelChanged(),
            StatusGeneratedNavigationState.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
            StatusGeneratedNavigationState.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
            StatusGeneratedNavigationState.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelBlocked(),
            StatusGeneratedNavigationState.NotApplicable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotApplicable(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value,
                "The generated navigation state is not defined."),
        };

    internal static string HumanState(StatusRecoveryCandidateKind value)
        => value switch
        {
            StatusRecoveryCandidateKind.Final => global::OpenForge.Cli.OutputText.Status.StatusText.LabelFinal(),
            StatusRecoveryCandidateKind.Draft => global::OpenForge.Cli.OutputText.Status.StatusText.LabelDraft(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value,
                "The recovery candidate kind is not defined."),
        };

    internal static string HumanState(StatusRecoveryIntegrity value)
        => value switch
        {
            StatusRecoveryIntegrity.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerified(),
            StatusRecoveryIntegrity.Malformed => global::OpenForge.Cli.OutputText.Status.StatusText.LabelMalformed(),
            StatusRecoveryIntegrity.Unsupported => global::OpenForge.Cli.OutputText.Status.StatusText.LabelUnsupported(),
            StatusRecoveryIntegrity.Unavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnavailable(),
            StatusRecoveryIntegrity.Incomplete => global::OpenForge.Cli.OutputText.Status.StatusText.LabelIncomplete(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value,
                "The recovery integrity is not defined."),
        };

    private static string Count(
        long? value,
        StatusValueState state,
        string singular,
        string plural)
    {
        if (state == StatusValueState.Available && value is { } number)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{number} {(number == 1 ? singular : plural)}");
        }

        return global::OpenForge.Cli.OutputText.Status.StatusPhrases.CountsUnavailable($"{plural}");
    }

    private static string TokenCount(long? value, StatusValueState state)
    {
        if (state == StatusValueState.Available && value is { } number)
        {
            return CliText.Tokens(number);
        }

        return global::OpenForge.Cli.OutputText.Status.StatusText.LabelUnavailableTokens();
    }

    private static string Count(StatusIntegerValue value, string singular, string plural)
        => Count(value.Value, value.State, singular, plural);
}
