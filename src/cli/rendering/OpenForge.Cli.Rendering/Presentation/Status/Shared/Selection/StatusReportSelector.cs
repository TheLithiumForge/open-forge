using System.Globalization;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Presentation.Status.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Selection;

internal static class StatusReportSelector
{
    internal static CliReport<StatusData> Select(StatusResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var attention = Attention(result.Findings);
        var data = Data(result, selection.Detail);
        var counts = Counts(result);
        return new CliReport<StatusData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, attention),
            HeadlineFindingCode = HeadlineFinding(result) is { } headline
                ? StatusWording.MachineCode(headline.Code)
                : null,
            Workspace = result.WorkspacePath is { } workspacePath
                ? new CliWorkspaceEcho(workspacePath, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(result, finding)).ToArray(),
            Counts = counts,
            Limitations = [],
            Data = data,
            Recovery = null,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static CliHeadline Headline(
        StatusResult result,
        AttentionSummary attention)
    {
        if (result.Status == CliSemanticStatus.Complete
            && result.Facts.Installation.State == StatusInstallationState.Uninstalled)
        {
            return new(
                StatusWording.NotInstalled(result.WorkspacePath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()),
                CliHeadlineKind.NothingToDo);
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => new(StatusWording.InstalledCurrent(), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(StatusWording.Attention(attention.Count, attention.Things), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(StatusWording.Incomplete(attention.Count, attention.Things), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(StatusWording.Invalid(Problem(result)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(StatusWording.Blocked(Problem(result)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(StatusWording.Failed(Problem(result)), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(StatusWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Status result is not defined."),
        };
    }

    private static StatusData Data(StatusResult result, CliDetail detail)
    {
        var facts = result.Facts;
        var structure = facts.Structure;
        var lifecycle = facts.Lifecycle;
        var recovery = facts.Recovery;
        var includeStandard = detail >= CliDetail.Standard;
        var includeFull = detail >= CliDetail.Full;
        var extensions = facts.Lifecycle.Extensions.Installed
            .OrderBy(extension => extension.Id, StringComparer.Ordinal)
            .Select(extension =>
            {
                var fileCounts = FileCounts(extension, facts.Lifecycle.Extensions.ManagedFiles);
                return new StatusDataExtension
                {
                    Id = extension.Id,
                    Version = extension.Version,
                    Files = includeStandard && !includeFull ? fileCounts : null,
                    FileRows = includeFull ? facts.Lifecycle.Extensions.ManagedFiles.Targets
                        .Where(target => target.Owners.Contains(extension.Id, StringComparer.Ordinal))
                        .OrderBy(target => target.Path, StringComparer.Ordinal)
                        .Select(target => new StatusDataManagedFile
                        {
                            Path = target.Path,
                            State = StatusWording.State(target.State),
                            TargetState = target.State,
                        }).ToArray() : null,
                    TextCurrentFiles = fileCounts.Current,
                    FilesState = facts.Lifecycle.Extensions.ManagedFiles.Counts.Current.State,
                };
            }).ToArray();
        var libraries = facts.Library.Records
            .OrderBy(record => record.IdValue, StringComparer.Ordinal)
            .Select(record => new StatusDataLibrary
            {
                Id = record.IdValue,
                SourceRoot = record.SourceRootValue,
                DestinationRoot = record.DestinationRootValue,
                Links = includeStandard && !includeFull ? LinkCounts(record.Counts) : null,
                LinkRows = includeFull ? record.Links
                    .OrderBy(link => link.DestinationPath, StringComparer.Ordinal)
                    .Select(link => new StatusDataLibraryLink
                    {
                        Path = link.DestinationPath,
                        State = StatusWording.State(link.State),
                        TargetState = link.State,
                        ExpectedTarget = link.ExpectedRelativeLink,
                        ObservedTarget = link.ObservedRelativeLink,
                    }).ToArray() : null,
                TextCurrentLinks = Available(record.Counts.Current),
                LinksState = record.Counts.Current.State,
            }).ToArray();

        return new StatusData
        {
            Installation = new StatusDataInstallation
            {
                State = StatusWording.State(facts.Installation.State),
                InstallationState = facts.Installation.State,
                EntryPath = facts.Installation.EntryPath,
                LoaderPath = facts.Installation.LoaderPath,
            },
            Context = new StatusDataContext
            {
                Startup = new StatusDataStartup
                {
                    Shipped = Measurement(facts.Context.Startup.Initial),
                    Current = Measurement(facts.Context.Startup.Current),
                    Difference = Measurement(facts.Context.Startup.Difference),
                    MayLoadAgain = Measurement(facts.Context.Continuity),
                },
                AllRouted = Measurement(facts.Context.TotalAvailable),
                StartupShare = Available(facts.Context.StartupPercentage),
                StartupShareState = facts.Context.StartupPercentage.State,
                MayLoadAgainSources = includeFull ? facts.Context.ContinuitySources
                    .OrderByDescending(source => source.Utf8Bytes)
                    .ThenBy(source => source.SourceId, StringComparer.Ordinal)
                    .Take(3)
                    .Select(source => new StatusDataContextSource
                    {
                        SourceId = source.SourceId,
                        Bytes = source.Utf8Bytes,
                        Layers = source.Layers.Select(layer => new StatusDataContextLayer
                        {
                            Path = layer.Path,
                            Bytes = layer.Utf8Bytes,
                        }).ToArray(),
                    }).ToArray() : null,
            },
            Extensions = extensions,
            Libraries = libraries,
            TextFacts = new StatusDataTextFacts
            {
                ShowOperationalSections = facts.Installation.State == StatusInstallationState.Installed,
                ShowStandard = includeStandard,
                ShowFull = includeFull,
                ShowDifference = HasNonZero(facts.Context.Startup.Difference),
                ShowMayLoadAgain = HasContent(facts.Context.Continuity),
                EntriesCurrent = AvailableEntries(structure),
                EntriesStale = AvailableEntries(structure, StatusGeneratedNavigationState.Changed),
                EntriesMissing = AvailableEntries(structure, StatusGeneratedNavigationState.Missing),
                FrameworkCurrent = AvailableFramework(lifecycle, StatusTargetState.Current),
                FrameworkChanged = AvailableFramework(lifecycle, StatusTargetState.Changed),
                FrameworkMissing = AvailableFramework(lifecycle, StatusTargetState.Missing),
            },
            Structure = includeStandard ? new StatusDataStructure
            {
                RootCategories = new StatusDataRootCategories
                {
                    Count = Available(facts.Structure.RootCategories.Count),
                    CountState = facts.Structure.RootCategories.Count.State,
                    Added = facts.Structure.RootCategories.Added.ToArray(),
                    Removed = facts.Structure.RootCategories.Removed.ToArray(),
                },
            } : null,
            FrameworkFiles = includeFull ? facts.Lifecycle.Framework.Targets
                .Where(target => target.Kind != StatusManagedTargetKind.GeneratedRegion)
                .OrderBy(target => target.Path, StringComparer.Ordinal)
                .Select(target => new StatusDataFrameworkFile
                {
                    Path = target.Path,
                    State = StatusWording.State(target.State),
                    TargetState = target.State,
                }).ToArray() : null,
            EntriesSections = includeFull ? facts.Structure.GeneratedNavigation
                .OrderBy(target => target.Path, StringComparer.Ordinal)
                .Select(target => new StatusDataEntrySection
                {
                    Path = target.Path,
                    State = StatusWording.State(target.State),
                    NavigationState = target.State,
                }).ToArray() : null,
            Recovery = includeFull ? new StatusDataRecovery
            {
                Candidates = facts.Recovery.Candidates.Select(candidate => new StatusDataRecoveryCandidate
                {
                    Path = candidate.Path,
                    Kind = StatusWording.State(candidate.Kind),
                    CandidateKind = candidate.Kind,
                    Integrity = StatusWording.State(candidate.Integrity),
                    IntegrityState = candidate.Integrity,
                }).ToArray(),
            } : null,
        };
    }

    private static StatusDataManagedFileCounts FileCounts(
        StatusInstalledExtension extension,
        StatusManagedExtensionFiles managedFiles)
    {
        var targets = managedFiles.Targets
            .Where(target => target.Owners.Contains(extension.Id, StringComparer.Ordinal))
            .ToArray();
        if (managedFiles.Counts.Current.State == StatusValueState.Unavailable)
        {
            return new StatusDataManagedFileCounts { Current = null, Changed = null, Missing = null };
        }

        if (extension.Paths.Count == 0 && targets.Length == 0)
        {
            return new StatusDataManagedFileCounts { Current = 0, Changed = 0, Missing = 0 };
        }

        return new StatusDataManagedFileCounts
        {
            Current = targets.LongCount(target => target.State == StatusTargetState.Current),
            Changed = targets.LongCount(target => target.State == StatusTargetState.Changed),
            Missing = targets.LongCount(target => target.State == StatusTargetState.Missing),
        };
    }

    private static StatusDataLinkCounts LinkCounts(StatusLibraryCounts counts)
        => new()
        {
            Current = Available(counts.Current),
            Missing = Available(counts.Missing),
            Changed = Available(counts.Changed),
        };

    private static StatusDataMeasurement Measurement(StatusMeasurement measurement)
        => new()
        {
            Files = Available(measurement.Files),
            FilesState = measurement.Files.State,
            Characters = Available(measurement.Characters),
            CharactersState = measurement.Characters.State,
            Bytes = Available(measurement.Utf8Bytes),
            BytesState = measurement.Utf8Bytes.State,
            Tokens = Available(measurement.EstimatedTokens),
            TokensState = measurement.EstimatedTokens.State,
        };

    private static long? Available(StatusIntegerValue value)
        => value.State == StatusValueState.Available ? value.Value : null;

    private static decimal? Available(StatusDecimalValue value)
        => value.State == StatusValueState.Available ? value.Value : null;

    private static StatusIntegerValue AvailableEntries(
        StatusStructure structure,
        StatusGeneratedNavigationState state = StatusGeneratedNavigationState.Current)
        => structure.RootCategories.Count.State switch
        {
            StatusValueState.Available => new StatusIntegerValue(
                StatusValueState.Available,
                structure.GeneratedNavigation.LongCount(target => target.State == state)),
            StatusValueState.NotApplicable => new StatusIntegerValue(StatusValueState.NotApplicable, null),
            StatusValueState.Unavailable => new StatusIntegerValue(StatusValueState.Unavailable, null),
            _ => throw new ArgumentOutOfRangeException(nameof(structure), structure.RootCategories.Count.State,
                "The root-category count state is not defined."),
        };

    private static StatusIntegerValue AvailableFramework(
        StatusLifecycle lifecycle,
        StatusTargetState state)
    {
        if (lifecycle.Framework.State == StatusLifecycleState.Absent)
        {
            return new StatusIntegerValue(StatusValueState.NotApplicable, null);
        }

        if (lifecycle.Framework.State != StatusLifecycleState.Trusted
            || lifecycle.Framework.SourceAvailability != StatusSourceAvailability.Available)
        {
            return new StatusIntegerValue(StatusValueState.Unavailable, null);
        }

        return new StatusIntegerValue(
            StatusValueState.Available,
            lifecycle.Framework.Targets.LongCount(target =>
                target.Kind != StatusManagedTargetKind.GeneratedRegion
                && target.State == state));
    }

    private static StatusIntegerValue InstalledCount(StatusExtensionLifecycle lifecycle)
    {
        if (lifecycle.State == StatusLifecycleState.Absent)
        {
            return new StatusIntegerValue(StatusValueState.NotApplicable, null);
        }

        if (lifecycle.State != StatusLifecycleState.Trusted)
        {
            return new StatusIntegerValue(StatusValueState.Unavailable, null);
        }

        return new StatusIntegerValue(StatusValueState.Available, lifecycle.Installed.Count);
    }

    private static bool HasContent(StatusMeasurement measurement)
        => measurement.Files.State == StatusValueState.Unavailable
            || measurement.Characters.State == StatusValueState.Unavailable
            || measurement.Utf8Bytes.State == StatusValueState.Unavailable
            || measurement.EstimatedTokens.State == StatusValueState.Unavailable
            || measurement.Files.Value is > 0
            || measurement.Characters.Value is > 0
            || measurement.Utf8Bytes.Value is > 0
            || measurement.EstimatedTokens.Value is > 0;

    private static bool HasNonZero(StatusMeasurement measurement)
        => HasNonZero(measurement.Files.Value)
            || HasNonZero(measurement.Characters.Value)
            || HasNonZero(measurement.Utf8Bytes.Value)
            || HasNonZero(measurement.EstimatedTokens.Value);

    private static bool HasNonZero(long? value)
        => value is { } number && number != 0;

    private static IReadOnlyList<CliCount> Counts(StatusResult result)
    {
        var facts = result.Facts;
        var context = facts.Context;
        var structure = facts.Structure;
        var lifecycle = facts.Lifecycle;
        var libraries = facts.Library;
        var recovery = facts.Recovery;
        return
        [
            Count("routedFiles", global::OpenForge.Cli.OutputText.Status.StatusText.LabelRoutedFiles(), context.TotalAvailable.Files,
                UnavailableReason(result, StatusFindingCode.ContextInventoryIncomplete)),
            Count("startupFiles", global::OpenForge.Cli.OutputText.Status.StatusText.LabelStartupFiles(), context.Startup.Current.Files,
                UnavailableReason(result, StatusFindingCode.StartupContextUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("startupTokens", global::OpenForge.Cli.OutputText.Status.StatusText.LabelStartupTokens(), context.Startup.Current.EstimatedTokens,
                UnavailableReason(result, StatusFindingCode.StartupContextUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("mayLoadAgainFiles", global::OpenForge.Cli.OutputText.Status.StatusText.LabelMayLoadAgainFiles(), context.Continuity.Files,
                UnavailableReason(result, StatusFindingCode.ContinuityContextUnavailable)),
            Count("mayLoadAgainTokens", global::OpenForge.Cli.OutputText.Status.StatusText.LabelMayLoadAgainTokens(), context.Continuity.EstimatedTokens,
                UnavailableReason(result, StatusFindingCode.ContinuityContextUnavailable)),
            Count("allTokens", global::OpenForge.Cli.OutputText.Status.StatusText.LabelAllRoutedTokens(), context.TotalAvailable.EstimatedTokens,
                UnavailableReason(result, StatusFindingCode.ContextInventoryIncomplete)),
            Count("startupShare", global::OpenForge.Cli.OutputText.Status.StatusText.LabelStartupShare(), context.StartupPercentage,
                UnavailableReason(result, StatusFindingCode.StartupContextUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("rootCategories", global::OpenForge.Cli.OutputText.Status.StatusText.LabelRootCategories(), structure.RootCategories.Count,
                UnavailableReason(result, StatusFindingCode.RootCategoriesUnavailable)),
            Count("entriesSectionsCurrent", global::OpenForge.Cli.OutputText.Status.StatusText.LabelCurrentEntriesSections(), AvailableEntries(structure),
                UnavailableReason(result, StatusFindingCode.RootCategoriesUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("entriesSectionsStale", global::OpenForge.Cli.OutputText.Status.StatusText.LabelStaleEntriesSections(), AvailableEntries(structure, StatusGeneratedNavigationState.Changed),
                UnavailableReason(result, StatusFindingCode.RootCategoriesUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("entriesSectionsMissing", global::OpenForge.Cli.OutputText.Status.StatusText.LabelMissingEntriesSections(), AvailableEntries(structure, StatusGeneratedNavigationState.Missing),
                UnavailableReason(result, StatusFindingCode.RootCategoriesUnavailable,
                    StatusFindingCode.ContextInventoryIncomplete)),
            Count("frameworkFilesCurrent", global::OpenForge.Cli.OutputText.Status.StatusText.LabelCurrentFrameworkFiles(), AvailableFramework(lifecycle, StatusTargetState.Current),
                UnavailableReason(result, StatusFindingCode.FrameworkLifecycleUntrusted,
                    StatusFindingCode.FrameworkLifecycleIncomplete,
                    StatusFindingCode.FrameworkLifecycleBlocked)),
            Count("frameworkFilesChanged", global::OpenForge.Cli.OutputText.Status.StatusText.LabelChangedFrameworkFiles(), AvailableFramework(lifecycle, StatusTargetState.Changed),
                UnavailableReason(result, StatusFindingCode.FrameworkLifecycleUntrusted,
                    StatusFindingCode.FrameworkLifecycleIncomplete,
                    StatusFindingCode.FrameworkLifecycleBlocked)),
            Count("frameworkFilesMissing", global::OpenForge.Cli.OutputText.Status.StatusText.LabelMissingFrameworkFiles(), AvailableFramework(lifecycle, StatusTargetState.Missing),
                UnavailableReason(result, StatusFindingCode.FrameworkLifecycleUntrusted,
                    StatusFindingCode.FrameworkLifecycleIncomplete,
                    StatusFindingCode.FrameworkLifecycleBlocked)),
            Count("extensionsInstalled", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInstalledExtensions(), InstalledCount(lifecycle.Extensions),
                UnavailableReason(result, StatusFindingCode.ExtensionLifecycleUntrusted,
                    StatusFindingCode.ExtensionLifecycleIncomplete,
                    StatusFindingCode.ExtensionLifecycleBlocked)),
            Count("librariesRegistered", global::OpenForge.Cli.OutputText.Status.StatusText.LabelRegisteredLibraries(), libraries.Counts.Registered,
                UnavailableReason(result, StatusFindingCode.LibraryRecordUnavailable,
                    StatusFindingCode.LibraryRecordMalformed)),
            Count("libraryLinksCurrent", global::OpenForge.Cli.OutputText.Status.StatusText.LabelCurrentLibraryLinks(), libraries.Counts.Current,
                UnavailableReason(result, StatusFindingCode.LibraryRecordUnavailable,
                    StatusFindingCode.LibraryRecordMalformed)),
            Count("libraryLinksMissing", global::OpenForge.Cli.OutputText.Status.StatusText.LabelMissingLibraryLinks(), libraries.Counts.Missing,
                UnavailableReason(result, StatusFindingCode.LibraryRecordUnavailable,
                    StatusFindingCode.LibraryRecordMalformed)),
            Count("libraryLinksChanged", global::OpenForge.Cli.OutputText.Status.StatusText.LabelChangedLibraryLinks(), libraries.Counts.Changed,
                UnavailableReason(result, StatusFindingCode.LibraryRecordUnavailable,
                    StatusFindingCode.LibraryRecordMalformed)),
            Count("recoveryBundles", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRecoveryBundles(), recovery.VerifiedFinals,
                UnavailableReason(result, StatusFindingCode.RecoveryCatalogueUnavailable)),
            Count("recoveryDrafts", global::OpenForge.Cli.OutputText.Status.StatusText.LabelRecoveryDrafts(), recovery.IncompleteDrafts,
                UnavailableReason(result, StatusFindingCode.RecoveryCatalogueUnavailable)),
        ];
    }

    private static CliCount Count(
        string name,
        string label,
        StatusIntegerValue value,
        string? unavailableReason)
        => new(name, label, Available(value), value.State == StatusValueState.Unavailable ? unavailableReason : null);

    private static CliCount Count(
        string name,
        string label,
        StatusDecimalValue value,
        string? unavailableReason)
        => new(name, label, Available(value), value.State == StatusValueState.Unavailable ? unavailableReason : null);

    private static CliCount Count(string name, string label, long value)
        => new(name, label, value);

    private static CliCount Count(
        string name,
        string label,
        long? value,
        string? unavailableReason)
        => new(name, label, value, value is null ? unavailableReason : null);

    private static string? UnavailableReason(
        StatusResult result,
        params StatusFindingCode[] codes)
        => result.Findings
            .Where(finding => codes.Contains(finding.Code) && finding.Cause.Length > 0)
            .Select(finding => finding.Cause)
            .FirstOrDefault();

    private static CliFinding Finding(StatusResult result, StatusFinding finding)
    {
        var subject = finding.Subject;
        var isLibrarySource = finding.Code is StatusFindingCode.LibrarySourceRootInvalid
            or StatusFindingCode.LibrarySourceRootAliased
            or StatusFindingCode.LibrarySourceRootUnavailable;
        var isOwnership = finding.Code is StatusFindingCode.FrameworkOwnershipObservation
            or StatusFindingCode.ExtensionOwnershipObservation
            or StatusFindingCode.LibraryOwnershipObservation;
        var isIdentifier = isLibrarySource || finding.Code is StatusFindingCode.ExtensionSourceUnavailable;
        var path = isOwnership
            ? subject ?? ".agents/open-forge.lock.json"
            : isIdentifier && !isLibrarySource ? null : subject;
        var id = isIdentifier
            ? isLibrarySource ? finding.Owner ?? finding.MachineCode : subject ?? finding.MachineCode
            : finding.Owner;
        if (path is null && id is null)
        {
            path = result.WorkspacePath;
            id = path is null ? finding.MachineCode : null;
        }

        var message = Message(finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = StatusWording.MachineCode(finding.Code),
            Title = StatusWording.FindingTitle(finding.Code),
            Message = message,
            Subject = new CliSubject(
                isIdentifier ? CliSubjectKind.Identifier : path is null ? CliSubjectKind.Identifier : CliSubjectKind.File,
                path,
                id),
            Actions = Action(finding) is { } action ? [action] : [],
            Resolution = Resolution(finding.Code),
            Evidence = Evidence(finding),
        };
    }

    private static string Message(StatusFinding finding)
    {
        var subject = finding.Subject ?? string.Empty;
        return finding.Code switch
        {
            StatusFindingCode.EntryUnavailable => StatusWording.EntryUnavailable(),
            StatusFindingCode.EmbeddedFrameworkUnavailable => StatusWording.EmbeddedFrameworkUnavailable(),
            StatusFindingCode.ContextInventoryIncomplete => StatusWording.StartupUnavailable(finding.Subject),
            StatusFindingCode.StartupContextUnavailable => StatusWording.CurrentStartupUnavailable(),
            StatusFindingCode.ContinuityContextUnavailable => StatusWording.ContinuityUnavailable(),
            StatusFindingCode.RootCategoriesUnavailable => StatusWording.RootCategoriesUnavailable(),
            StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing
                or StatusFindingCode.GeneratedNavigationUnavailable or StatusFindingCode.GeneratedNavigationBlocked
                => StatusWording.GeneratedNavigation(finding.Code, subject),
            StatusFindingCode.GeneratedNavigationMetadataInvalid
                => StatusWording.GeneratedNavigationMetadataInvalid(subject, finding.Cause),
            StatusFindingCode.FrameworkOwnershipObservation => StatusWording.FrameworkOwnership(),
            StatusFindingCode.ExtensionOwnershipObservation => StatusWording.ExtensionOwnership(),
            StatusFindingCode.LibraryOwnershipObservation => StatusWording.LibraryOwnership(),
            StatusFindingCode.FrameworkLifecycleUntrusted => StatusWording.LifecycleUnavailable(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(), finding.Cause),
            StatusFindingCode.FrameworkLifecycleIncomplete => StatusWording.LifecycleUnavailable(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(), finding.Cause),
            StatusFindingCode.FrameworkLifecycleBlocked => StatusWording.LifecycleBlocked(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(), finding.Cause),
            StatusFindingCode.FrameworkTargetChanged or StatusFindingCode.FrameworkTargetMissing
                 or StatusFindingCode.FrameworkTargetUnavailable or StatusFindingCode.FrameworkTargetBlocked
                => StatusWording.FrameworkTarget(finding.Code, finding.Cause),
            StatusFindingCode.ExtensionLifecycleUntrusted => StatusWording.LifecycleUnavailable(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensions(), finding.Cause),
            StatusFindingCode.ExtensionLifecycleIncomplete => StatusWording.LifecycleUnavailable(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensions(), finding.Cause),
            StatusFindingCode.ExtensionLifecycleBlocked => StatusWording.LifecycleBlocked(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensions(), finding.Cause),
            StatusFindingCode.ExtensionSourceUnavailable => StatusWording.ExtensionSourceUnavailable(subject, finding.Source),
            StatusFindingCode.ExtensionTargetChanged or StatusFindingCode.ExtensionTargetMissing
                 or StatusFindingCode.ExtensionTargetUnavailable or StatusFindingCode.ExtensionTargetBlocked
                => StatusWording.ExtensionTarget(finding.Code, finding.Owner, finding.Cause),
            StatusFindingCode.RecoveryCandidateVerified or StatusFindingCode.RecoveryDraftIncomplete
                 or StatusFindingCode.RecoveryFinalMalformed or StatusFindingCode.RecoveryFinalUnsupported
                 or StatusFindingCode.RecoveryFinalUnavailable
                => StatusWording.Recovery(finding.Code, subject),
            StatusFindingCode.RecoveryCatalogueUnavailable => StatusWording.RecoveryCatalogueUnavailable(subject),
            StatusFindingCode.LibraryRecordMalformed or StatusFindingCode.LibraryRecordUnavailable
                 => StatusWording.LibraryRecord(finding.Code, finding.Cause),
            StatusFindingCode.LibrarySourceRootInvalid or StatusFindingCode.LibrarySourceRootAliased
                 or StatusFindingCode.LibrarySourceRootUnavailable
                => StatusWording.LibrarySourceRoot(finding.Code, finding.Owner ?? global::OpenForge.Cli.OutputText.Status.StatusText.LabelThe(), subject),
            StatusFindingCode.LibraryProjectionMissing or StatusFindingCode.LibraryProjectionChanged
                 or StatusFindingCode.LibraryProjectionUnavailable or StatusFindingCode.LibraryProjectionBlocked
                => StatusWording.LibraryProjection(finding.Code, finding.Owner ?? global::OpenForge.Cli.OutputText.Status.StatusText.LabelThe(), finding.Cause),
            StatusFindingCode.LibraryExtensionCollision => finding.Cause,
            StatusFindingCode.InvalidInput => StatusWording.Invalid(StatusWording.TrimSentence(finding.Cause)),
            StatusFindingCode.WorkspaceUnavailable => StatusWording.WorkspaceUnavailable(),
            StatusFindingCode.WorkspaceNotDirectory => StatusWording.WorkspaceNotDirectory(),
            StatusFindingCode.WorkspaceUnsafe => StatusWording.WorkspaceUnsafe(finding.Cause),
            StatusFindingCode.OperationFailed => StatusWording.Failed(finding.Cause),
            StatusFindingCode.Interrupted => StatusWording.Cancelled(),
            _ => finding.Cause,
        };
    }

    private static CliNextAction? Action(StatusFinding finding)
    {
        var command = StatusWording.ActionCommand(finding.Code, finding.Owner, finding.Subject);
        if (command is null)
        {
            return null;
        }

        return new CliNextAction(command, StatusWording.ActionReason(finding.Code));
    }

    private static CliResolution? Resolution(StatusFindingCode code)
        => StatusWording.ActionCommand(code, owner: null, subject: null) is null
            ? null
            : code switch
            {
                StatusFindingCode.FrameworkTargetChanged or StatusFindingCode.FrameworkTargetMissing
                    or StatusFindingCode.ExtensionTargetChanged or StatusFindingCode.ExtensionTargetMissing
                    or StatusFindingCode.LibraryProjectionMissing or StatusFindingCode.LibraryProjectionChanged
                    or StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing
                    or StatusFindingCode.RecoveryCandidateVerified or StatusFindingCode.RecoveryDraftIncomplete
                    or StatusFindingCode.RecoveryFinalMalformed or StatusFindingCode.RecoveryFinalUnsupported
                    or StatusFindingCode.LibrarySourceRootInvalid or StatusFindingCode.LibrarySourceRootAliased
                    or StatusFindingCode.LibrarySourceRootUnavailable
                    => CliResolution.TargetedOperation,
                _ => CliResolution.Informational,
            };

    private static IReadOnlyList<CliEvidence> Evidence(StatusFinding finding)
    {
        var evidence = new List<CliEvidence> { new("cause", finding.Cause) };
        if (finding.Source is { } source)
        {
            evidence.Add(new CliEvidence("source", source));
        }

        if (finding.Owner is { } owner)
        {
            evidence.Add(new CliEvidence("owner", owner));
        }

        return evidence;
    }

    private static CliNextAction? Next(StatusResult result)
    {
        if (result.Status == CliSemanticStatus.Complete
            && result.Facts.Installation.State == StatusInstallationState.Uninstalled)
        {
            return new CliNextAction(
                StatusWording.InstallPreviewCommand(),
                StatusWording.InstallPreviewReason());
        }

        var finding = result.Findings
            .OrderBy(finding => NextRank(finding.Code))
            .ThenBy(finding => finding.Owner ?? finding.Subject, StringComparer.Ordinal)
            .FirstOrDefault(finding => finding.Status != CliSemanticStatus.Complete);
        if (finding is null)
        {
            return null;
        }

        var command = StatusWording.ActionCommand(finding.Code, finding.Owner, finding.Subject)
            ?? (result.Status == CliSemanticStatus.Invalid
                ? "open-forge status --help"
                : "open-forge doctor");
        return new CliNextAction(command, StatusWording.NextReason(result.Status, finding.Code));
    }

    private static int NextRank(StatusFindingCode code) => code switch
    {
        StatusFindingCode.FrameworkTargetChanged or StatusFindingCode.FrameworkTargetMissing
            or StatusFindingCode.FrameworkTargetUnavailable or StatusFindingCode.FrameworkTargetBlocked => 0,
        StatusFindingCode.ExtensionTargetChanged or StatusFindingCode.ExtensionTargetMissing
            or StatusFindingCode.ExtensionTargetUnavailable or StatusFindingCode.ExtensionTargetBlocked
            or StatusFindingCode.ExtensionSourceUnavailable => 1,
        StatusFindingCode.LibraryProjectionMissing or StatusFindingCode.LibraryProjectionChanged
            or StatusFindingCode.LibraryProjectionUnavailable or StatusFindingCode.LibraryProjectionBlocked
            or StatusFindingCode.LibrarySourceRootInvalid or StatusFindingCode.LibrarySourceRootAliased
            or StatusFindingCode.LibrarySourceRootUnavailable => 2,
        StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing
            or StatusFindingCode.GeneratedNavigationUnavailable or StatusFindingCode.GeneratedNavigationBlocked
            or StatusFindingCode.GeneratedNavigationMetadataInvalid => 3,
        StatusFindingCode.RecoveryCandidateVerified or StatusFindingCode.RecoveryDraftIncomplete
            or StatusFindingCode.RecoveryFinalMalformed or StatusFindingCode.RecoveryFinalUnsupported
            or StatusFindingCode.RecoveryFinalUnavailable or StatusFindingCode.RecoveryCatalogueUnavailable => 4,
        _ => 5,
    };

    private static StatusFinding? HeadlineFinding(StatusResult result)
        => result.Status == CliSemanticStatus.Interrupted
            ? result.Findings.FirstOrDefault(finding => finding.Code == StatusFindingCode.Interrupted)
            : result.Status is CliSemanticStatus.Invalid
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                ? result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
                : null;

    private static string Problem(StatusResult result)
        => StatusWording.TrimSentence(HeadlineFinding(result)?.Cause ?? "the workspace could not be checked");

    private static IReadOnlyList<string> Diagnostics(StatusResult result)
        => [$"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"findings={result.Findings.Count.ToString(CultureInfo.InvariantCulture)}",
            .. result.Findings.Select(finding => finding.Cause)];

    private static AttentionSummary Attention(IReadOnlyList<StatusFinding> findings)
    {
        var warningFindings = findings.Where(finding => finding.Status == CliSemanticStatus.Attention).ToArray();
        var filePaths = warningFindings
            .Where(IsFileFinding)
            .Select(finding => finding.Subject)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var things = warningFindings
            .Where(finding => !IsFileFinding(finding))
            .Select(finding => finding.Subject ?? finding.MachineCode)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new AttentionSummary(filePaths.Length + things.Length, things.Length > 0);
    }

    private static bool IsFileFinding(StatusFinding finding)
        => finding.Code is StatusFindingCode.GeneratedNavigationChanged
            or StatusFindingCode.GeneratedNavigationMissing
            or StatusFindingCode.GeneratedNavigationMetadataInvalid
            or StatusFindingCode.FrameworkTargetChanged
            or StatusFindingCode.FrameworkTargetMissing
            or StatusFindingCode.ExtensionTargetChanged
            or StatusFindingCode.ExtensionTargetMissing
            or StatusFindingCode.LibraryProjectionMissing
            or StatusFindingCode.LibraryProjectionChanged;

    private sealed record AttentionSummary(long Count, bool Things);
}
