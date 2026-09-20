using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Selection;

internal static class ExtensionListReportSelector
{
    internal static CliReport<ExtensionListData> Select(
        ExtensionListResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var installed = result.Installed
            .Select(row => ProjectInstalled(row, selection.Detail))
            .ToArray();
        var available = result.Available
            .Select(row => ProjectAvailable(row, selection.Detail))
            .ToArray();
        var installedEmptyLine = installed.Length == 0
            ? result.LifecycleTrust == ExtensionListOwnershipTrust.Absent
                ? ExtensionListWording.InstalledNoOwnership()
                : result.InstalledCoverage == ExtensionListCoverage.Complete
                    ? ExtensionListWording.InstalledEmpty()
                    : ExtensionListWording.InstalledUnavailable()
            : null;
        var availableEmptyLine = available.Length == 0
            ? result.AvailableCoverage == ExtensionListCoverage.Complete
                ? ExtensionListWording.AvailableEmpty()
                : ExtensionListWording.AvailableUnavailable()
            : null;
        return new CliReport<ExtensionListData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new(Headline(result), HeadlineKind(result.Status)),
            Workspace = result.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => ProjectFinding(result, finding, selection.Detail)).ToArray(),
            Counts = ReadCounts(result),
            Data = new ExtensionListData
            {
                Source = result.Source is { } source
                    ? new ExtensionListDataSource(
                        source.Kind is { } kind ? ExtensionListWording.SourceKind(kind) : null,
                        source.Identity)
                    : null,
                Installed = installed,
                Available = available,
                TextSourceLine = selection.Detail >= CliDetail.Standard && result.Selection.Available
                    ? result.Source is { } sourceForText
                        ? ExtensionListWording.Source(sourceForText.Identity)
                        : ExtensionListWording.SourceUnavailableLine()
                    : null,
                InstalledHeading = ExtensionListWording.Installed(),
                InstalledEmptyLine = installedEmptyLine,
                AvailableHeading = ExtensionListWording.AvailableHeading(result.Source),
                AvailableEmptyLine = availableEmptyLine,
                ShowInstalled = result.Selection.Installed,
                ShowAvailable = result.Selection.Available,
                ShowSource = selection.Detail >= CliDetail.Standard && result.Selection.Available,
                ShowInstalledDetails = selection.Detail >= CliDetail.Full,
                ShowAvailableDependencies = selection.Detail >= CliDetail.Standard,
            },
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"installed={result.Installed.Count}",
                    $"available={result.Available.Count}",
                }
                .Concat(result.Findings.Select(finding => finding.Cause))
                .ToArray()
                : [],
        };
    }

    private static ExtensionListDataInstalled ProjectInstalled(
        ExtensionListInstalledRow row,
        CliDetail detail)
    {
        var note = ExtensionListWording.InstalledRowNote(row);
        var cells = new List<string>
        {
            row.Id,
            ExtensionListWording.Version(row.Version),
        };
        if (!string.IsNullOrEmpty(note))
        {
            cells.Add(note);
        }

        var details = detail >= CliDetail.Full
            ? new[]
            {
                ExtensionListWording.FileCount(row.ManagedPathCount),
                row.RecordedSource is { } recordedSource
                    ? ExtensionListWording.RecordedSource(recordedSource)
                    : null,
                ExtensionListWording.LockCoverage(row.Coverage),
            }
            .Where(line => line is not null)
            .OfType<string>()
            .ToArray()
            : [];
        return new ExtensionListDataInstalled
        {
            Id = row.Id,
            Version = row.Version,
            Note = string.IsNullOrEmpty(note) ? null : note,
            Full = detail >= CliDetail.Full
                ? new ExtensionListDataInstalledFull
                {
                    Files = row.ManagedPathCount,
                    RecordedSource = row.RecordedSource,
                    Coverage = ExtensionListWording.Coverage(row.Coverage),
                }
                : null,
            TextCells = cells,
            TextDetails = details,
        };
    }

    private static ExtensionListDataAvailable ProjectAvailable(
        ExtensionListAvailableRow row,
        CliDetail detail)
    {
        var installed = row.InstalledVersion is not { } installedVersion
            ? null
            : string.Equals(installedVersion, row.Version, StringComparison.Ordinal)
                ? ExtensionListWording.InstalledMarker()
                : ExtensionListWording.InstalledVersion(installedVersion);
        var annotations = new[]
        {
            ExtensionListWording.AvailablePackagesSummary(row.PackageCount),
            installed,
        }
        .Where(value => !string.IsNullOrEmpty(value))
        .OfType<string>()
        .ToArray();

        var dependencies = detail >= CliDetail.Standard ? row.Dependencies : null;
        var details = dependencies is { Count: > 0 }
            ? new[] { ExtensionListWording.Dependencies(dependencies) }
            : [];
        return new()
        {
            Id = row.Id,
            Version = row.Version,
            Name = row.Name,
            Description = row.Description,
            Packages = row.PackageCount,
            Dependencies = dependencies,
            Installed = installed,
            TextCells =
            [
                row.Id,
                ExtensionListWording.Version(row.Version),
                row.Description,
                .. annotations,
            ],
            TextDetails = details,
        };
    }

    private static CliFinding ProjectFinding(
        ExtensionListResult result,
        ExtensionListFinding finding,
        CliDetail detail)
    {
        var code = finding.MachineCode;
        var isInstalledFinding = finding.Code is ExtensionListFindingCode.InstalledSourceMissing
            or ExtensionListFindingCode.InstalledSourceUnavailable
            or ExtensionListFindingCode.InstalledSourceInvalid
            or ExtensionListFindingCode.InstalledSourceBlocked
            or ExtensionListFindingCode.InstalledFilesChanged
            or ExtensionListFindingCode.InstalledFilesMissing
            or ExtensionListFindingCode.InstalledTargetUnavailable
            or ExtensionListFindingCode.InstalledTargetBlocked
            or ExtensionListFindingCode.InstalledFilesUnavailable;
        var aggregatePackageFinding = finding.Code is ExtensionListFindingCode.InstalledFilesChanged
            or ExtensionListFindingCode.InstalledFilesMissing;
        var subjectPath = aggregatePackageFinding
            ? null
            : finding.Path
            ?? (isInstalledFinding && finding.Code is (ExtensionListFindingCode.InstalledSourceMissing
                or ExtensionListFindingCode.InstalledSourceUnavailable
                or ExtensionListFindingCode.InstalledSourceInvalid
                or ExtensionListFindingCode.InstalledSourceBlocked)
                    ? finding.Subject
                    : null);
        var subjectKind = finding.Code switch
        {
            ExtensionListFindingCode.WorkspaceUnavailable => CliSubjectKind.Workspace,
            ExtensionListFindingCode.SourceUnavailable
                or ExtensionListFindingCode.SourceInvalid
                or ExtensionListFindingCode.SourceBlocked
                or ExtensionListFindingCode.InstalledSourceMissing
                or ExtensionListFindingCode.InstalledSourceUnavailable
                or ExtensionListFindingCode.InstalledSourceInvalid
                or ExtensionListFindingCode.InstalledSourceBlocked => CliSubjectKind.Source,
            ExtensionListFindingCode.InstalledTargetUnavailable
                or ExtensionListFindingCode.InstalledTargetBlocked
                or ExtensionListFindingCode.InstalledFilesUnavailable => CliSubjectKind.File,
            _ => CliSubjectKind.Identifier,
        };
        var subject = new CliSubject(
            subjectKind,
            subjectPath,
            subjectPath is null ? finding.Subject ?? result.WorkspacePath ?? "extension-list" : finding.Subject);
        var actions = isInstalledFinding && finding.Owner is { } owner
            ? new[] { ExtensionListWording.Inspect(owner) }
            : Array.Empty<CliNextAction>();
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = code,
            Title = ExtensionListWording.FindingTitle(finding.Code),
            Message = Message(result, finding, subjectPath),
            Subject = subject,
            Resolution = finding.Code == ExtensionListFindingCode.OwnershipObservation
                ? CliResolution.Informational
                : null,
            Actions = actions,
            Evidence = detail >= CliDetail.Full
                ? Evidence(finding)
                : [],
            Provenance = detail >= CliDetail.Full && finding.Path is { } path
                ? new CliProvenance(result.Command, path)
                : null,
        };
    }

    private static string Message(
        ExtensionListResult result,
        ExtensionListFinding finding,
        string? subjectPath)
        => finding.Code switch
        {
            ExtensionListFindingCode.InvalidInput => ExtensionListWording.InvalidInput(finding.Cause),
            ExtensionListFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(subjectPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()),
            ExtensionListFindingCode.SourceUnavailable => ExtensionListWording.SourceUnavailable(subjectPath ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource()),
            ExtensionListFindingCode.SourceInvalid => ExtensionListWording.SourceInvalid(subjectPath ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource(), finding.Cause),
            ExtensionListFindingCode.SourceBlocked => ExtensionListWording.SourceBlocked(subjectPath ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource(), finding.Cause),
            ExtensionListFindingCode.InstalledSourceMissing => ExtensionListWording.InstalledSourceMissing(
                finding.Owner ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalled(), subjectPath ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheRecordedSource()),
            ExtensionListFindingCode.InstalledSourceUnavailable => ExtensionListWording.InstalledSourceUnavailable(
                finding.Owner ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalled(), subjectPath ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheRecordedSource()),
            ExtensionListFindingCode.InstalledSourceInvalid => ExtensionListWording.SourceInvalid(
                subjectPath ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheRecordedSource(), finding.Cause),
            ExtensionListFindingCode.InstalledSourceBlocked => ExtensionListWording.CannotList(finding.Cause),
            ExtensionListFindingCode.InstalledFilesChanged => ExtensionListWording.InstalledFilesChanged(
                finding.Owner ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalledPackage(), finding.Count ?? 0),
            ExtensionListFindingCode.InstalledFilesMissing => ExtensionListWording.InstalledFilesMissing(
                finding.Owner ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalledPackage(), finding.Count ?? 0),
            ExtensionListFindingCode.InstalledTargetUnavailable => CliFindingWording.InspectionIncomplete(
                subjectPath ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalledFile()),
            ExtensionListFindingCode.InstalledTargetBlocked => ExtensionListWording.InstalledTargetBlocked(
                subjectPath ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalledFile()),
            ExtensionListFindingCode.InstalledFilesUnavailable => ExtensionListWording.InstalledFilesUnavailable(
                finding.Owner ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheInstalledPackage()),
            ExtensionListFindingCode.OwnershipObservation => ExtensionListWording.OwnershipMissing(),
            ExtensionListFindingCode.OperationFailed => CliFindingWording.OperationFailed(
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.TitleExtensionList(), TrimPeriod(finding.Cause)),
            ExtensionListFindingCode.Interrupted => ExtensionListWording.Interrupted(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Extension List finding code is not defined."),
        };

    private static IReadOnlyList<CliEvidence> Evidence(ExtensionListFinding finding)
    {
        var evidence = new List<CliEvidence>
        {
            new("Cause", finding.Cause),
        };
        if (finding.Count is { } count)
        {
            evidence.Add(new CliEvidence("Count", count.ToString(CultureInfo.InvariantCulture)));
        }

        if (finding.Path is { } path)
        {
            evidence.Add(new CliEvidence("Path", path));
        }

        return evidence;
    }

    private static CliNextAction? Next(ExtensionListResult result)
    {
        var installedOwner = result.Findings
            .Where(finding => finding.Owner is not null && IsInstalledFinding(finding.Code))
            .Select(finding => finding.Owner)
            .OfType<string>()
            .FirstOrDefault();
        if (installedOwner is not null)
        {
            return ExtensionListWording.Inspect(installedOwner);
        }

        if (result.Selection.Available
            && result.Findings.Any(finding => finding.Code is ExtensionListFindingCode.SourceUnavailable
                or ExtensionListFindingCode.SourceInvalid
                or ExtensionListFindingCode.SourceBlocked))
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && result.Available.Count > 0
            ? new CliNextAction("open-forge extension install <id>", ExtensionListWording.NextInstall())
            : result.Next;
    }

    private static IReadOnlyList<CliCount> ReadCounts(ExtensionListResult result)
    {
        var counts = new List<CliCount>(capacity: 2);
        if (result.Selection.Installed)
        {
            counts.Add(ReadCount(
                "installed",
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelInstalledPackages(),
                result.Installed.Count,
                result.InstalledCoverage,
                result.Findings,
                IsInstalledCoverageFinding,
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageInstalledPackageFactsAreUnavailable()));
        }

        if (result.Selection.Available)
        {
            counts.Add(ReadCount(
                "available",
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelAvailablePackages(),
                result.Available.Count,
                result.AvailableCoverage,
                result.Findings,
                IsAvailableCoverageFinding,
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.MessageAvailablePackageFactsAreUnavailable()));
        }

        return counts;
    }

    private static CliCount ReadCount(
        string name,
        string label,
        int knownCount,
        ExtensionListCoverage coverage,
        IReadOnlyList<ExtensionListFinding> findings,
        Func<ExtensionListFinding, bool> findingPredicate,
        string fallbackReason)
        => coverage == ExtensionListCoverage.Complete
            ? new CliCount(name, label, knownCount)
            : new CliCount(
                name,
                label,
                Value: null,
                UnavailableReason: findings.FirstOrDefault(findingPredicate) is { } finding
                    ? CliFindingWording.CauseSentence(finding.Cause)
                    : fallbackReason);

    private static bool IsInstalledCoverageFinding(ExtensionListFinding finding)
        => finding.Code == ExtensionListFindingCode.OwnershipObservation
            || IsInstalledFinding(finding.Code)
            || finding.Code is ExtensionListFindingCode.WorkspaceUnavailable
                or ExtensionListFindingCode.OperationFailed
                or ExtensionListFindingCode.Interrupted;

    private static bool IsAvailableCoverageFinding(ExtensionListFinding finding)
        => finding.Code is ExtensionListFindingCode.SourceUnavailable
            or ExtensionListFindingCode.SourceInvalid
            or ExtensionListFindingCode.SourceBlocked
            or ExtensionListFindingCode.WorkspaceUnavailable
            or ExtensionListFindingCode.OperationFailed
            or ExtensionListFindingCode.Interrupted;

    private static string Headline(ExtensionListResult result)
    {
        var finding = result.Findings.FirstOrDefault(value => value.Status == result.Status);
        return result.Status switch
        {
            CliSemanticStatus.Complete => ExtensionListWording.Summary(result.Installed.Count, result.Available.Count),
            CliSemanticStatus.Attention => ExtensionListWording.WarningHeadline(),
            CliSemanticStatus.Incomplete => ExtensionListWording.IncompleteHeadline(),
            CliSemanticStatus.Invalid => ExtensionListWording.InvalidHeadline(finding?.Cause ?? global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.LabelTheSelectedInput()),
            CliSemanticStatus.Blocked => ExtensionListWording.BlockedHeadline(finding?.Cause ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource()),
            CliSemanticStatus.Failed => ExtensionListWording.FailedHeadline(finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheOperationFailed()),
            CliSemanticStatus.Interrupted => ExtensionListWording.CancelledHeadline(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Extension List status is not defined."),
        };
    }

    private static bool IsInstalledFinding(ExtensionListFindingCode code)
        => code is ExtensionListFindingCode.InstalledSourceMissing
            or ExtensionListFindingCode.InstalledSourceUnavailable
            or ExtensionListFindingCode.InstalledSourceInvalid
            or ExtensionListFindingCode.InstalledSourceBlocked
            or ExtensionListFindingCode.InstalledFilesChanged
            or ExtensionListFindingCode.InstalledFilesMissing
            or ExtensionListFindingCode.InstalledTargetUnavailable
            or ExtensionListFindingCode.InstalledTargetBlocked
            or ExtensionListFindingCode.InstalledFilesUnavailable;

    private static CliHeadlineKind HeadlineKind(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => CliHeadlineKind.Done,
            CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            CliSemanticStatus.Failed => CliHeadlineKind.Failed,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Extension List status is not defined."),
        };

    private static string TrimPeriod(string value) => CliFindingWording.PlainCause(value);
}
