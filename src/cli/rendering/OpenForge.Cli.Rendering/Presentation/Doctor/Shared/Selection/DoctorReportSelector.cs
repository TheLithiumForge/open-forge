using System.Globalization;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor.Models;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;

internal static class DoctorReportSelector
{
    private static readonly DoctorDomainKind[] DomainOrder = Enum.GetValues<DoctorDomainKind>();

    internal static CliReport<DoctorData> Select(DoctorResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var findings = result.Diagnosis.Domains
            .SelectMany(domain => domain.Findings)
            .OrderBy(finding => finding.Kind)
            .ThenBy(finding => finding.Subject.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject.Identifier, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject.LocationView?.Line)
            .ThenBy(finding => finding.Subject.LocationView?.Column)
            .GroupBy(Key)
            .Select(group => Merge(group))
            .ToArray();
        var domains = DomainOrder
            .Select(domain => result.Diagnosis.Domains.FirstOrDefault(value => value.Domain == domain))
            .ToArray();
        var coverageCounts = result.Diagnosis.CoverageCounts ?? UnavailableCoverageCounts();
        var errors = findings.LongCount(finding => finding.Severity == DoctorFindingSeverity.Error);
        var warnings = findings.LongCount(finding => finding.Severity == DoctorFindingSeverity.Warning);
        var infos = findings.LongCount(finding => finding.Severity == DoctorFindingSeverity.Information);
        var data = new DoctorData
        {
            Categories = selection.Detail >= CliDetail.Standard
                ? domains.Select((domain, index) => Category(
                    domain,
                    DomainOrder[index],
                    findings,
                    selection.Detail >= CliDetail.Full)).ToArray()
                : null,
            ChecksLine = selection.Detail == CliDetail.Minimal && result.Status == CliSemanticStatus.Complete
                ? JoinFacts(DoctorWording.ChecksSummary(coverageCounts), DoctorWording.CoverageSummary(coverageCounts))
                : null,
            CoverageLine = selection.Detail >= CliDetail.Standard
                ? DoctorWording.CoverageSummary(coverageCounts)
                : null,
            IncompleteLines = selection.Detail == CliDetail.Minimal || selection.Detail >= CliDetail.Standard
                ? domains
                    .Where(domain => domain is not null && domain.Coverage != DoctorCoverageState.Complete)
                    .Select(domain => DoctorWording.IncompleteLine(domain!.Domain, domain))
                    .ToArray()
                : [],
            LaneLine = selection.Detail >= CliDetail.Full
                ? DoctorWording.LaneSummary(findings)
                : null,
            HintLine = Hint(infos, selection),
        };
        return new CliReport<DoctorData>
        {
            Command = result.Command,
            ShowMinimalTextWarnings = true,
            Status = result.Status,
            Headline = Headline(result.Status, errors, warnings, infos, result.Diagnosis, findings),
            HeadlineFindingCode = HeadlineFindingCode(result.Status, findings),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = findings.Select(Finding).ToArray(),
            Counts = Counts(coverageCounts, errors, warnings, infos, result.Diagnosis),
            Limitations = Limitations(domains),
            Data = data,
            Next = Next(result.Status, findings),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(domains)
                : [],
        };
    }

    private static DoctorFinding Merge(IGrouping<DoctorFindingKey, DoctorFinding> group)
    {
        var first = group.First();
        var candidates = group.SelectMany(finding => finding.Candidates?.Items ?? [])
            .GroupBy(candidate => (candidate.Subject.Path, candidate.Subject.Identifier), StringTupleComparer.Instance)
            .Select(candidatesGroup => candidatesGroup.First())
            .ToArray();
        var evidence = group.SelectMany(finding => finding.Evidence).Distinct().ToArray();
        var actions = group.SelectMany(finding => finding.Actions).Distinct().ToArray();
        return first with
        {
            Candidates = candidates.Length == 0
                ? null
                : new DoctorCandidateSet
                {
                    Cardinality = candidates.Length switch
                    {
                        1 => DoctorCandidateCardinality.One,
                        _ => DoctorCandidateCardinality.Several,
                    },
                    Items = candidates,
                },
            Evidence = evidence,
            Actions = actions,
        };
    }

    private static DoctorFindingKey Key(DoctorFinding finding)
        => new(
            finding.Kind,
            finding.Subject.Path,
            finding.Subject.Identifier,
            finding.Subject.LocationView?.Line,
            finding.Subject.LocationView?.Column);

    private static DoctorDataCategory Category(
        DoctorDomainReport? domain,
        DoctorDomainKind domainKind,
        IReadOnlyList<DoctorFinding> findings,
        bool includeLanes)
    {
        var values = domain is null
            ? findings.Where(finding => DomainFor(finding) == domainKind).ToArray()
            : domain.Findings;
        var coverage = domain?.Coverage ?? DoctorCoverageState.Complete;
        return new DoctorDataCategory
        {
            Name = DoctorWording.Category(domainKind),
            Coverage = DoctorWording.Coverage(coverage),
            Counts = new DoctorDataCounts
            {
                Errors = values.LongCount(finding => finding.Severity == DoctorFindingSeverity.Error),
                Warnings = values.LongCount(finding => finding.Severity == DoctorFindingSeverity.Warning),
                Infos = values.LongCount(finding => finding.Severity == DoctorFindingSeverity.Information),
            },
            Limitations = domain?.Limitations.Select(limitation => limitation.Message).ToArray() ?? [],
            Lanes = !includeLanes
                ? null
                : new DoctorDataLanes
                {
                    SafeExact = values.LongCount(finding => finding.Resolution == DoctorResolutionLane.SafeExact),
                    GuidedChoice = values.LongCount(finding => finding.Resolution == DoctorResolutionLane.GuidedChoice),
                    TargetedOperation = values.LongCount(finding => finding.Resolution == DoctorResolutionLane.TargetedOperation),
                    ManualDecision = values.LongCount(finding => finding.Resolution == DoctorResolutionLane.ManualDecision),
                    BlockedRepair = values.LongCount(finding => finding.Resolution == DoctorResolutionLane.BlockedRepair),
                },
        };
    }

    private static DoctorDomainKind DomainFor(DoctorFinding finding)
        => finding.Provenance.Domain;

    private static CliHeadline Headline(
        CliSemanticStatus status,
        long errors,
        long warnings,
        long infos,
        DoctorDiagnosis diagnosis,
        IReadOnlyList<DoctorFinding> findings)
    {
        var sentence = status switch
        {
            CliSemanticStatus.Invalid => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatCannotRunDoctor($"{EventReason(diagnosis, "the input is invalid")}"),
            CliSemanticStatus.Blocked => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatCannotCheckThisWorkspace($"{EventReason(diagnosis, "the workspace cannot be checked")}"),
            CliSemanticStatus.Failed => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatDoctorStoppedBecauseOfAnUnexpectedError($"{EventReason(diagnosis, "the operation failed")}"),
            CliSemanticStatus.Interrupted => global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageDoctorWasCancelled(),
            _ => DoctorWording.CountFindings(errors, warnings, infos)
                + (status == CliSemanticStatus.Incomplete
                    ? $" {DoctorWording.IncompleteCheckSummary(diagnosis.CoverageCounts ?? UnavailableCoverageCounts())}"
                    : string.Empty),
        };
        return new(sentence, status switch
        {
            CliSemanticStatus.Complete => CliHeadlineKind.Done,
            CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            CliSemanticStatus.Failed => CliHeadlineKind.Failed,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Doctor status is not defined."),
        });
    }

    private static string EventReason(DoctorDiagnosis diagnosis, string fallback)
    {
        var reason = diagnosis.Domains.SelectMany(domain => domain.Limitations)
            .Select(limitation => limitation.Message)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(reason))
        {
            return fallback + ".";
        }

        return reason.EndsWith(".", StringComparison.Ordinal) ? reason : reason + ".";
    }

    private static string? HeadlineFindingCode(CliSemanticStatus status, IReadOnlyList<DoctorFinding> findings)
    {
        if (status is not (CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted)
            || findings.Count != 1)
        {
            return null;
        }

        return DoctorWording.MachineCode(findings[0].Kind);
    }

    private static CliFinding Finding(DoctorFinding finding)
        => new()
        {
            Severity = finding.Severity switch
            {
                DoctorFindingSeverity.Error => CliSeverity.Error,
                DoctorFindingSeverity.Warning => CliSeverity.Warning,
                DoctorFindingSeverity.Information => CliSeverity.Info,
                _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Severity, "The Doctor finding severity is not defined."),
            },
            Code = DoctorWording.MachineCode(finding.Kind),
            Title = DoctorWording.Title(finding.Kind),
            Message = DoctorWording.Message(finding),
            Subject = Subject(finding),
            Category = DoctorWording.Category(finding.Kind),
            Resolution = finding.Resolution switch
            {
                DoctorResolutionLane.SafeExact => CliResolution.SafeExact,
                DoctorResolutionLane.GuidedChoice => CliResolution.GuidedChoice,
                DoctorResolutionLane.TargetedOperation => CliResolution.TargetedOperation,
                DoctorResolutionLane.ManualDecision => CliResolution.ManualDecision,
                DoctorResolutionLane.BlockedRepair => CliResolution.BlockedRepair,
                DoctorResolutionLane.Informational => CliResolution.Informational,
                _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Resolution, "The Doctor finding resolution is not defined."),
            },
            Actions = DoctorWording.Actions(finding),
            Candidates = finding.Candidates?.Items.Select(Candidate).ToArray() ?? [],
            Evidence = finding.Evidence.SelectMany(Evidence).ToArray(),
            Provenance = Provenance(finding.Provenance),
        };

    private static CliCandidate Candidate(DoctorCandidate candidate)
        => new(
            Subject(candidate.Subject),
            candidate.Evidence
                .Select(evidence => DoctorWording.CandidateReason(evidence.Kind))
                .Distinct(StringComparer.Ordinal)
                .ToArray());

    private static IReadOnlyList<CliEvidence> Evidence(DoctorEvidence evidence)
        => evidence switch
        {
            DoctorAvailabilityEvidence availability => [new("availability", CliReportVocabulary.Name(availability.StateValue))],
            DoctorStateEvidence state => [new("state", CliReportVocabulary.Name(state.State))],
            DoctorComparisonEvidence comparison =>
            [
                new("expected", comparison.Expected),
                new("actual", comparison.Actual),
            ],
            DoctorIntegrityEvidence integrity => [new("integrity", CliReportVocabulary.Name(integrity.State))],
            DoctorAuthoredValueEvidence authored => [new("authored value", authored.Value)],
            DoctorCandidateBasisEvidence basis => [new("candidate basis", DoctorWording.CandidateReason(basis.Basis))],
            _ => throw new ArgumentOutOfRangeException(nameof(evidence), evidence.Kind, "The Doctor evidence kind is not defined."),
        };

    private static CliProvenance Provenance(DoctorProvenance provenance)
        => new(
            DoctorWording.Provenance(provenance.Source),
            provenance.Path,
            provenance.LocationView is { } location ? new CliSourceLocation(location.Line, location.Column) : null);

    private static CliSubject Subject(DoctorSubject subject)
        => Subject(subject, subject.Path is null ? "candidate" : null);

    private static CliSubject Subject(DoctorFinding finding)
        => Subject(finding.Subject, finding.Subject.Path is null
            ? DoctorWording.SubjectFallback(finding.Kind)
            : null);

    private static CliSubject Subject(DoctorSubject subject, string? fallback)
    {
        var kind = Enum.TryParse<CliSubjectKind>(DoctorWording.SubjectKind(subject.Kind), ignoreCase: true, out var parsedKind)
            ? parsedKind
            : CliSubjectKind.File;
        var identifier = subject.Identifier ?? fallback;
        return new(
            kind,
            subject.Path,
            identifier,
            subject.LocationView is { } location ? new CliSourceLocation(location.Line, location.Column) : null);
    }

    private static IReadOnlyList<CliCount> Counts(
        DoctorCoverageCounts coverage,
        long errors,
        long warnings,
        long infos,
        DoctorDiagnosis diagnosis)
    {
        var reason = diagnosis.Domains.SelectMany(domain => domain.Limitations)
            .Select(limitation => limitation.Message)
            .FirstOrDefault();
        return
        [
            new CliCount("checks", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelChecks(), coverage.Checks, reason),
            new CliCount("checksComplete", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelChecksComplete(), coverage.ChecksComplete, reason),
            new CliCount("errors", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelErrors(), errors),
            new CliCount("warnings", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelWarnings(), warnings),
            new CliCount("infos", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInfos(), infos),
            new CliCount("linksChecked", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelLinksChecked(), coverage.LinksChecked, reason),
            new CliCount("linksValid", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelLinksValid(), coverage.LinksValid, reason),
            new CliCount("externalLinksNotChecked", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelExternalLinksNotChecked(), coverage.ExternalLinksNotChecked, reason),
            new CliCount("imageLinks", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelImageLinks(), coverage.ImageLinks, reason),
            new CliCount("routesChecked", global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelRoutesChecked(), coverage.RoutesChecked, reason),
            new CliCount("frameworkFiles", global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(), coverage.FrameworkFiles, reason),
            new CliCount("extensionsInstalled", global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionsInstalled(), coverage.ExtensionsInstalled, reason),
            new CliCount("librariesRegistered", global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibrariesRegistered(), coverage.LibrariesRegistered, reason),
        ];
    }

    private static IReadOnlyList<CliLimitation> Limitations(IReadOnlyList<DoctorDomainReport?> domains)
        => domains
            .Where(domain => domain is not null && domain.Coverage != DoctorCoverageState.Complete)
            .SelectMany(domain => domain!.Limitations.Count > 0
                ? domain.Limitations.Select(limitation => new CliLimitation(
                    DoctorWording.Category(domain.Domain),
                    limitation.Message,
                    domain.Boundary.Path is { } path ? new CliSubject(CliSubjectKind.Directory, path) : null))
                : [new CliLimitation(
                    DoctorWording.Category(domain.Domain),
                    DoctorWording.IncompleteLine(domain.Domain, domain),
                    null)])
            .Distinct()
            .ToArray();

    private static IReadOnlyList<string> Diagnostics(IReadOnlyList<DoctorDomainReport?> domains)
        => domains
            .Select((domain, index) => string.Create(
                CultureInfo.InvariantCulture,
                $"{DoctorWording.Category(DomainOrder[index])}: boundary={DoctorWording.Boundary(domain?.Boundary.Kind ?? Boundary(DomainOrder[index]))}, coverage={DoctorWording.Coverage(domain?.Coverage ?? DoctorCoverageState.Complete)}"))
            .ToArray();

    private static DoctorBoundaryKind Boundary(DoctorDomainKind domain)
        => domain switch
        {
            DoctorDomainKind.WorkspaceEntry => DoctorBoundaryKind.Workspace,
            DoctorDomainKind.RecoveryResiduals => DoctorBoundaryKind.RecoveryStore,
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => DoctorBoundaryKind.RouteUniverse,
            DoctorDomainKind.LocalReferences => DoctorBoundaryKind.LocalReferenceUniverse,
            DoctorDomainKind.FrameworkLifecycle => DoctorBoundaryKind.FrameworkLifecycle,
            DoctorDomainKind.ExtensionLifecycle => DoctorBoundaryKind.ExtensionLifecycle,
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, "The Doctor domain boundary is not defined."),
        };

    private static CliNextAction? Next(CliSemanticStatus status, IReadOnlyList<DoctorFinding> findings)
    {
        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted)
        {
            return null;
        }

        var safe = findings.Count(finding => finding.Resolution == DoctorResolutionLane.SafeExact);
        if (safe > 0)
        {
            return new CliNextAction(
                "open-forge repair --dry-run",
                global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatPreviewThatSafeToApply($"{safe}", $"{Plural(safe, "repair")}", $"{(safe == 1 ? "is" : "are")}"));
        }

        foreach (var domain in DomainOrder)
        {
            var finding = findings.FirstOrDefault(value => value.Provenance.Domain == domain
                && value.Resolution == DoctorResolutionLane.TargetedOperation);
            if (finding is null)
            {
                continue;
            }

            var action = DoctorWording.Actions(finding).FirstOrDefault(value => value.Kind == CliNextActionKind.Command);
            if (action is not null)
            {
                return action;
            }
        }

        if (findings.Any(finding => finding.Resolution == DoctorResolutionLane.GuidedChoice))
        {
            return new CliNextAction("open-forge repair", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReviewTheFindingsAndChooseARepair());
        }

        var manual = findings.FirstOrDefault(finding => finding.Severity == DoctorFindingSeverity.Error
            && finding.Resolution == DoctorResolutionLane.ManualDecision);
        return manual is null
            ? null
            : new CliNextAction(
                global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatEditByHand($"{manual.Subject.Path ?? manual.Subject.Identifier ?? "the reported file"}"),
                global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageEditTheReportedFileByHand())
            {
                Kind = CliNextActionKind.Sentence
            };
    }

    private static string? Hint(long infos, CliSelection selection)
        => selection.Filter is not null
            ? null
            : DoctorWording.Hint(0, infos, selection.Detail);

    private static string JoinFacts(string first, string second)
        => string.IsNullOrEmpty(first) ? second : string.IsNullOrEmpty(second) ? first : $"{first} {second}";

    private static string Plural(long count, string singular)
        => count == 1 ? singular : singular + "s";

    private static DoctorCoverageCounts UnavailableCoverageCounts()
        => new()
        {
            Checks = null,
            ChecksComplete = null,
            LinksChecked = null,
            LinksValid = null,
            ExternalLinksNotChecked = null,
            ImageLinks = null,
            RoutesChecked = null,
            FrameworkFiles = null,
            ExtensionsInstalled = null,
            LibrariesRegistered = null,
        };

    private readonly record struct DoctorFindingKey(
        DoctorFindingKind Kind,
        string? Path,
        string? Identifier,
        int? Line,
        int? Column);

    private sealed class StringTupleComparer : IEqualityComparer<(string? Path, string? Identifier)>
    {
        internal static StringTupleComparer Instance { get; } = new();

        public bool Equals((string? Path, string? Identifier) x, (string? Path, string? Identifier) y)
            => string.Equals(x.Path, y.Path, StringComparison.Ordinal)
                && string.Equals(x.Identifier, y.Identifier, StringComparison.Ordinal);

        public int GetHashCode((string? Path, string? Identifier) obj)
            => HashCode.Combine(
                StringComparer.Ordinal.GetHashCode(obj.Path ?? string.Empty),
                StringComparer.Ordinal.GetHashCode(obj.Identifier ?? string.Empty));
    }
}
