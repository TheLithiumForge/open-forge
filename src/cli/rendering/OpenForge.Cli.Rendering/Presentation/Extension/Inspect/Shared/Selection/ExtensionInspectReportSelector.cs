using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Selection;

internal static class ExtensionInspectReportSelector
{
    internal static CliReport<ExtensionInspectData> Select(
        ExtensionInspectResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var includeStandard = selection.Detail >= CliDetail.Standard;
        var includeFull = selection.Detail >= CliDetail.Full;
        var files = result.Comparison.Paths
            .Select(path => ProjectFile(result, path, includeFull))
            .ToArray();
        var dependencies = result.Dependencies.Resolved
            .Select(dependency => new ExtensionInspectDataDependency
            {
                Id = dependency.Id,
                Version = dependency.Version,
                State = ExtensionInspectWording.DependencyState(dependency.State),
            })
            .ToArray();
        var sourcePath = result.Source.Identity ?? result.Source.Supplied;
        var registeredIn = result.Generated.Regions
            .Where(region => region.State == ExtensionInspectGeneratedRegionState.Valid)
            .Select(region => region.Path)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var data = new ExtensionInspectData
        {
            Id = result.Subject.Id ?? result.Subject.Supplied ?? "unknown",
            Installed = result.Installed.Package is { } installed
                ? new ExtensionInspectDataVersion { Version = installed.Version }
                : null,
            Available = result.Available.Package is { } available
                ? new ExtensionInspectDataVersion { Version = available.Version }
                : null,
            Source = new ExtensionInspectDataSource
            {
                Kind = result.Source.Kind is { } kind ? ExtensionInspectWording.SourceKind(kind) : null,
                Path = sourcePath,
                TextPath = ExtensionInspectWording.SourceText(result),
            },
            Matches = Matches(result),
            Files = files,
            Dependencies = includeStandard ? dependencies : [],
            Name = includeStandard ? result.Available.Package?.Name : null,
            Description = includeStandard ? result.Available.Package?.Description : null,
            ManifestPath = includeFull ? result.Available.Package?.ManifestPath : null,
            ResolutionOrder = includeFull ? result.Dependencies.Order : [],
            RegisteredIn = includeFull ? registeredIn : [],
            RecordCoverage = includeFull ? CliReportVocabulary.Name(result.Lifecycle.Coverage) : null,
            IncludeStandard = includeStandard,
            IncludeFull = includeFull,
            TextSourceLine = includeStandard ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatSource($"{ExtensionInspectWording.SourceText(result)}") : null,
            TextSummaryLine = selection.Detail == CliDetail.Minimal && Matches(result)
                ? ExtensionInspectWording.FileSummary(files)
                : null,
            TextFiles = files,
            TextDependencies = includeStandard
                ? dependencies.Select(dependency => (IReadOnlyList<string>)[
                    dependency.Id,
                    ExtensionInspectWording.Version(dependency.Version),
                    dependency.State,
                ]).ToArray()
                : [],
            TextManifestLine = includeFull ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatManifest($"{result.Available.Package?.ManifestPath ?? "unavailable"}") : null,
            TextResolutionLine = includeFull
                ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatResolutionOrder($"{(result.Dependencies.Order.Count == 0 ? "none" : string.Join(", ", result.Dependencies.Order))}")
                : null,
            TextRegisteredIn = includeFull ? registeredIn : [],
            TextCoverageLine = includeFull
                ? global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatRecordCoverage($"{CliReportVocabulary.Name(result.Lifecycle.Coverage)}")
                : null,
        };
        return new CliReport<ExtensionInspectData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = ExtensionInspectWording.Headline(result),
            HeadlineFindingCode = result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked
                && result.Findings.Count == 1
                ? result.Findings[0].MachineCode
                : null,
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings
                .Select(finding => ProjectFinding(result, finding, selection.Detail))
                .ToArray(),
            Counts = ReadCounts(result),
            Data = data,
            Next = ExtensionInspectWording.Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"source={result.Source.State}",
                    $"lifecycle={result.Lifecycle.Coverage}",
                    $"comparison={result.Comparison.State}",
                }
                .Concat(result.Findings.Select(finding => finding.Cause))
                .ToArray()
                : [],
        };
    }

    private static ExtensionInspectDataFile ProjectFile(
        ExtensionInspectResult result,
        ExtensionInspectPathComparison comparison,
        bool includeFull)
        => new()
        {
            Path = comparison.Path,
            Relation = CliReportVocabulary.Name(comparison.Relation),
            InstalledSha256 = includeFull ? comparison.Current?.Sha256 : null,
            PackageSha256 = includeFull ? comparison.Intended?.Sha256 : null,
            Text = new ExtensionInspectDataFileText
            {
                Severity = comparison.Relation is ExtensionInspectPathRelation.Changed
                    or ExtensionInspectPathRelation.Missing
                    or ExtensionInspectPathRelation.New
                    or ExtensionInspectPathRelation.Retired
                    ? "Warning"
                    : comparison.Relation is ExtensionInspectPathRelation.Invalid or ExtensionInspectPathRelation.Blocked
                        ? "Error"
                        : "Info",
                Message = ExtensionInspectWording.FileMessage(result, comparison),
            },
        };

    private static CliFinding ProjectFinding(
        ExtensionInspectResult result,
        ExtensionInspectFinding finding,
        CliDetail detail)
    {
        var subject = Subject(result, finding);
        var evidence = detail >= CliDetail.Full
            ? Evidence(finding)
            : [];
        var provenance = detail >= CliDetail.Full && finding.Path is { } path
            ? new CliProvenance(result.Command, path)
            : null;
        return new CliFinding
        {
            Severity = ExtensionInspectWording.Severity(finding.Code, finding.Status),
            Code = finding.MachineCode,
            Title = ExtensionInspectWording.FindingTitle(finding.Code),
            Message = ExtensionInspectWording.Message(result, finding),
            Subject = subject,
            Resolution = finding.Code is ExtensionInspectFindingCode.PackageInvalid
                or ExtensionInspectFindingCode.SourceAmbiguous
                or ExtensionInspectFindingCode.IdentityAmbiguous
                or ExtensionInspectFindingCode.DependencyConflict
                or ExtensionInspectFindingCode.DependencyCycle
                or ExtensionInspectFindingCode.PathInvalid
                ? CliResolution.ManualDecision
                : finding.Code == ExtensionInspectFindingCode.OwnershipObservation
                    ? CliResolution.Informational
                    : null,
            Candidates = detail >= CliDetail.Full
                ? finding.Candidates.Select(candidate => new CliCandidate(
                    new CliSubject(CliSubjectKind.Source, Path: candidate.Path, Id: candidate.Id),
                    [global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelCandidatePackage()])).ToArray()
                : [],
            Evidence = evidence,
            Provenance = provenance,
        };
    }

    private static CliSubject Subject(
        ExtensionInspectResult result,
        ExtensionInspectFinding finding)
    {
        if (finding.Path is { } path)
        {
            return new CliSubject(CliSubjectKind.File, Path: path, Id: finding.PackageId ?? result.Subject.Id);
        }

        if (finding.Code is ExtensionInspectFindingCode.SourceUnavailable
            or ExtensionInspectFindingCode.SourceInvalid
            or ExtensionInspectFindingCode.SourceOverlap
            or ExtensionInspectFindingCode.SourceAmbiguous)
        {
            return new CliSubject(CliSubjectKind.Source, Path: finding.Subject, Id: finding.PackageId ?? result.Subject.Id);
        }

        if (finding.Code is ExtensionInspectFindingCode.WorkspaceUnavailable or ExtensionInspectFindingCode.WorkspaceUnsafe)
        {
            return new CliSubject(
                CliSubjectKind.Workspace,
                Path: finding.Subject ?? result.WorkspacePath,
                Id: result.Subject.Id ?? result.Subject.Supplied ?? "extension inspect");
        }

        var identifier = finding.Code is ExtensionInspectFindingCode.IdentityAmbiguous
            or ExtensionInspectFindingCode.DependencyCycle
            ? result.Subject.Id ?? finding.PackageId ?? finding.Subject
            : finding.Subject ?? finding.PackageId ?? result.Subject.Id;
        return new CliSubject(
            CliSubjectKind.Identifier,
            Id: identifier ?? result.Subject.Supplied ?? "extension inspect");
    }

    private static IReadOnlyList<CliEvidence> Evidence(ExtensionInspectFinding finding)
    {
        var evidence = new List<CliEvidence>
        {
            new(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleCause(), finding.Cause),
        };
        if (finding.PackageId is { } packageId)
        {
            evidence.Add(new CliEvidence(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitlePackage(), packageId));
        }

        if (finding.Dependency is { } dependency)
        {
            evidence.Add(new CliEvidence(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependency(), dependency));
        }

        if (finding.Path is { } path)
        {
            evidence.Add(new CliEvidence(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitlePath(), path));
        }

        return evidence;
    }

    private static IReadOnlyList<CliCount> ReadCounts(ExtensionInspectResult result)
        =>
        [
            Count("filesUnchanged", result.Counts.UnchangedPaths, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFileUnchanged(), global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesUnchanged()),
            Count("filesChanged", result.Counts.ChangedPaths, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFileChanged(), global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelFilesChanged()),
            Count("filesMissing", result.Counts.MissingPaths, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFileMissing(), global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelFilesMissing()),
            Count("filesNew", result.Counts.NewPaths, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFileNew(), global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFilesNew()),
            Count("filesRetired", result.Counts.RetiredPaths, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelFileRetired(), global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesRetired()),
            Count("dependencies", result.Counts.Dependencies, global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelDependency(), global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelDependencies()),
        ];

    private static CliCount Count(string name, int? value, string singular, string plural)
        => new(name, ExtensionInspectWording.CountLabel(value, singular, plural), value);

    private static bool Matches(ExtensionInspectResult result)
        => result.Installed.Package is not null
            && result.Available.Package is not null
            && result.Status == CliSemanticStatus.Complete
            && result.Comparison.State == ExtensionInspectComparisonState.Complete
            && result.Comparison.Dependencies.Relation == ExtensionInspectDependencyRelation.Equal
            && result.Comparison.Paths.All(path => path.Relation is ExtensionInspectPathRelation.Unchanged or ExtensionInspectPathRelation.Shared);

}
