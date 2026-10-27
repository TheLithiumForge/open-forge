using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Actions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

internal static class DoctorResultBuilder
{
    internal static DoctorResult Build(DoctorRequest request, DoctorObservation observation)
    {
        var routeInputs = RouteInputs(observation, out var retainedAuthoredMissingTargets);
        var lifecycleAbsence = OperationalLifecycleAbsenceProof.IsProven(
            OperationalLifecycleAbsenceEvidence.FromDoctor(
                observation.WorkspaceEntry,
                observation.Routes,
                observation.RecoveryResiduals,
                observation.FrameworkLifecycle,
                observation.ExtensionLifecycle));
        var domains = new DoctorDomainReport[]
        {
            LibraryDoctorInspector.Inspect(
                request.Workspace,
                WorkspaceEntryDoctorInspector.Inspect(request.Workspace, observation with { Routes = routeInputs }),
                observation.Libraries,
                observation.RecoveryResiduals,
                observation.LibraryResiduals),
            RecoveryResidualDoctorInspector.Inspect(observation.RecoveryResiduals),
            RouteDoctorInspector.Inspect(routeInputs),
            LocalReferenceDoctorInspector.Inspect(observation.LocalReferences, observation.Maintenance, retainedAuthoredMissingTargets),
            FrameworkLifecycleDoctorInspector.Inspect(
                observation.FrameworkLifecycle,
                observation.RecoveryResiduals,
                observation.ExtensionLifecycle.Ownership,
                lifecycleAbsence,
                observation.Routes.GeneratedNavigation
                    .Where(target => target.State == OperationalGeneratedNavigationState.Current)
                    .Select(target => target.Path)
                    .ToHashSet(StringComparer.Ordinal)),
            ExtensionLifecycleDoctorInspector.Inspect(observation.ExtensionLifecycle),
        };
        domains = DoctorMaintenanceGuidance.Apply(domains, observation);
        domains = ProjectOutsideIndexActions(domains, observation);
        var layout = observation.Routes.Catalogue?.Layout;
        if (layout is not null) domains = domains.Select(domain => domain with
        {
            Findings = domain.Findings.Select(finding => DoctorLayoutPresentation.Project(layout, finding)).ToArray(),
        }).ToArray();
        var coverage = domains.Max(domain => domain.Coverage);
        var status = DoctorResultPolicy.ReadStatus(coverage, domains);
        return new DoctorResult
        {
            Status = status,
            Workspace = request.Workspace,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = coverage,
                Counts = DoctorFindingAggregation.Count(domains.SelectMany(domain => domain.Findings)),
                CoverageCounts = CoverageCounts(observation, domains),
                Actions = DoctorFindingAggregation.Actions(domains.SelectMany(domain => domain.Findings)),
                Domains = domains,
            },
        };
    }

    private static RouteDoctorView RouteInputs(DoctorObservation observation,
        out IReadOnlySet<LocalReferenceObservation> retainedAuthoredMissingTargets)
    {
        var missing = new HashSet<LocalReferenceObservation>(ReferenceEqualityComparer.Instance);
        retainedAuthoredMissingTargets = missing;
        if (observation.OutsideRoutes is not { Inspection: { } inspection } outside) return observation.Routes;
        var rows = outside.Routes.Where(route => route.Directory.State == WorkspaceOutsideRouteReadState.Available)
            .SelectMany(route => route.Rows.Where(row => row.ParentPath.StartsWith(route.Directory.Descriptor.DirectoryPath + "/", StringComparison.Ordinal)))
            .ToLookup(row => row.ParentPath, StringComparer.Ordinal);
        var navigation = observation.Routes.GeneratedNavigation.Select(target =>
        {
            if (target.State is not (OperationalGeneratedNavigationState.Current or OperationalGeneratedNavigationState.Changed)
                || !rows.Contains(target.Path)) return target;
            var host = inspection.Sources.SingleOrDefault(source => source.Source.Base.CanonicalPath == target.Path);
            if (host?.Layers[0].Text is not { } text) return target;
            var locations = new Utf8SourceMap(text);
            var comparisons = target.Content.EntryComparisons.Where(comparison =>
            {
                if (comparison.Kind != RouteGeneratedEntryComparisonKind.Extra) return true;
                var row = rows[target.Path].FirstOrDefault(row => row.Entry.Destination == comparison.Actual
                    && target.Content.CurrentEntries.Entries.Any(entry => entry.Destination == row.Entry.Destination
                        && entry.Span.Start == row.Entry.Span.Start && entry.Span.Length == row.Entry.Span.Length)
                    && locations.Map(row.Entry.Span.Start, row.Entry.Span.Length) is { } location
                    && comparison.Location is { } observed && observed.Line == location.Line && observed.Column == location.Column);
                if (row is null) return true;
                if (row.PhysicalState is PhysicalPathState.Contained or PhysicalPathState.Missing && row.AliasOf is null
                    && locations.Map(row.Entry.Span.Start, row.Entry.Span.Length) is { } rowLocation)
                    foreach (var reference in observation.LocalReferences.References.Where(reference =>
                        reference.SourcePath == target.Path && reference.Destination == row.Entry.Destination
                        && reference.Facts.Target.Resolution == SourceLinkTargetResolution.Missing
                        && reference.Location is { } occurrence && occurrence.ByteOffset >= rowLocation.ByteOffset
                        && occurrence.ByteOffset + occurrence.ByteLength <= rowLocation.ByteOffset + rowLocation.ByteLength))
                        missing.Add(reference);
                return false;
            }).ToArray();
            if (comparisons.Length == target.Content.EntryComparisons.Count) return target;
            return DoctorGeneratedNavigationTargetObservation.Available(target.Path, target.State,
                target.Content with { EntryComparisons = comparisons });
        }).ToArray();
        var admitted = ReadOutsideStructure(observation, outside);
        return observation.Routes with
        {
            GeneratedNavigation = navigation,
            Shape = observation.Routes.Shape.Where(shape =>
                shape.Kind is not (RouteShapeObservationKind.Detached or RouteShapeObservationKind.Unreachable)
                || !admitted.Contains(shape.Path)).ToArray(),
        };
    }

    private static IReadOnlySet<string> ReadOutsideStructure(DoctorObservation observation, RouteOutsideRoutesObservation outside)
    {
        var admitted = new HashSet<string>(StringComparer.Ordinal);
        if (!ReferenceEquals(observation.LayoutRead, outside.LayoutRead)
            || outside.Inspection is not { Session: not null } inspection) return admitted;
        var sources = inspection.Sources.ToDictionary(source => source.Source.Base.CanonicalPath, StringComparer.Ordinal);
        var facts = inspection.Routes.RouteFacts.ToDictionary(fact => fact.Identity.CanonicalBasePath, StringComparer.Ordinal);
        foreach (var route in outside.Routes.Where(route => route.Directory.State == WorkspaceOutsideRouteReadState.Available
                     && route.EntrypointPaths.Count == 1))
        {
            var directory = route.Directory.Descriptor.DirectoryPath;
            var root = route.EntrypointPaths[0];
            if (SourceLogicalPath.ReadParent(root) != directory || !IsProven(root)
                || !SourceFormClassifier.IsEntrypoint(sources[root].Source.Base.Form)) continue;
            var rows = route.Rows.Where(row => row.PhysicalState == PhysicalPathState.Contained && row.AliasOf is null)
                .Select(row => (row.ParentPath, row.Path)).ToHashSet();
            var pending = new Queue<string>();
            if (admitted.Add(root)) pending.Enqueue(root);
            while (pending.TryDequeue(out var parent))
            {
                if (sources[parent].GeneratedEntries.State != SourceGeneratedEntriesState.Complete) continue;
                var node = inspection.Routes.Topology.FindByPath(parent)!;
                foreach (var child in node.ChildPaths)
                {
                    if (!IsProven(child) || !rows.Contains((parent, child))) continue;
                    if (admitted.Add(child)) pending.Enqueue(child);
                }
            }

            bool IsProven(string path)
            {
                if (!path.StartsWith(directory + "/", StringComparison.Ordinal)
                    || !sources.TryGetValue(path, out var source) || !source.IsReadable || source.Document is null
                    || inspection.Catalogue.Aliases.ContainsKey(path)
                    || !facts.TryGetValue(path, out var fact) || fact.State is SourceRouteState.Ambiguous or SourceRouteState.Unavailable
                    || inspection.Routes.Topology.FindByPath(path) is not { ParentState: not SourceRouteParentState.Ambiguous }) return false;
                var workspace = inspection.Catalogue.Workspace;
                return PhysicalIdentityTracker.PathComparer.Equals(source.Source.Base.PhysicalPath,
                        SourceLogicalPath.ToLexicalPath(workspace.PhysicalRoot, path))
                    && (source.Source.Overwrite is not { } overwrite || PhysicalIdentityTracker.PathComparer.Equals(overwrite.PhysicalPath,
                        SourceLogicalPath.ToLexicalPath(workspace.PhysicalRoot, overwrite.CanonicalPath)));
            }
        }
        return admitted;
    }

    private static DoctorDomainReport[] ProjectOutsideIndexActions(DoctorDomainReport[] domains, DoctorObservation observation)
    {
        if (observation.OutsideRoutes is not { } outside || observation.Maintenance is not { } maintenance) return domains;
        return domains.Select(domain =>
        {
            var findings = domain.Findings.Select(finding =>
            {
                if (finding.Subject.Path is not { } path || !maintenance.IndexablePaths.Contains(path)
                    || !outside.Routes.Any(route => route.Directory.State == WorkspaceOutsideRouteReadState.Available
                        && path.StartsWith(route.Directory.Descriptor.DirectoryPath + "/", StringComparison.Ordinal))) return finding;
                return finding with
                {
                    Actions = finding.Actions.Select(action => action.Operation == DoctorNextOperation.Index
                    ? DoctorIndexAction.ForPath(SourceWorkspaceRelativePath.LiteralReferencePrefix + path) : action).ToArray()
                };
            }).ToArray();
            return domain with { Findings = findings, Actions = DoctorFindingAggregation.Actions(findings) };
        }).ToArray();
    }

    internal static DoctorResult Event(
        CliWorkspace? workspace,
        CliSemanticStatus status,
        DoctorFindingKind? kind,
        string cause)
    {
        var coverage = status == CliSemanticStatus.Blocked
            ? DoctorCoverageState.Blocked
            : DoctorCoverageState.Incomplete;
        DoctorFinding[] firstFindings = [];
        if (kind is { } findingKind)
        {
            var observedState = status == CliSemanticStatus.Blocked
                ? DoctorObservedState.Blocked
                : DoctorObservedState.Incomplete;
            firstFindings =
            [
                DoctorDomainSupport.Create(
                    DoctorDomainSupport.Error(findingKind, cause),
                    DoctorDomainSupport.Subject(DoctorSubjectKind.Workspace, workspace?.LexicalRoot),
                    DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, workspace?.LexicalRoot),
                    [new DoctorStateEvidence(observedState)]),
            ];
        }
        var domains = Enum.GetValues<DoctorDomainKind>()
            .Select((domain, index) => EventDomain(domain, coverage, cause, index == 0 ? firstFindings : []))
            .ToArray();
        return new DoctorResult
        {
            Status = status,
            Workspace = workspace,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = coverage,
                Counts = UnavailableCounts(),
                CoverageCounts = UnavailableCoverageCounts(),
                Actions = DoctorFindingAggregation.Actions(firstFindings),
                Domains = domains,
            },
        };
    }

    private static DoctorDomainReport EventDomain(
        DoctorDomainKind domain,
        DoctorCoverageState coverage,
        string cause,
        IReadOnlyList<DoctorFinding> findings)
        => new()
        {
            Domain = domain,
            Boundary = new DoctorBoundary { Kind = ReadBoundary(domain), Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [DoctorDomainSupport.Limitation(coverage, cause)],
            Counts = UnavailableCounts(),
            Findings = findings,
            Actions = DoctorFindingAggregation.Actions(findings),
        };

    private static DoctorCount Unavailable()
        => new() { State = OperationalValueState.Unavailable, Value = null };

    private static DoctorFindingCounts UnavailableCounts()
    {
        var count = Unavailable();
        return new DoctorFindingCounts
        {
            Resolution = new DoctorResolutionCounts
            {
                SafeExact = count,
                GuidedChoice = count,
                TargetedOperation = count,
                ManualDecision = count,
                BlockedRepair = count,
                Informational = count,
            },
            Severity = new DoctorSeverityCounts
            {
                Information = count,
                Warning = count,
                Error = count,
            },
        };
    }

    private static DoctorCoverageCounts CoverageCounts(
        DoctorObservation observation,
        IReadOnlyList<DoctorDomainReport> domains)
    {
        var references = observation.LocalReferences.References;
        var lifecycle = observation.FrameworkLifecycle;
        var extensions = observation.ExtensionLifecycle;
        var libraries = observation.Libraries;
        return new DoctorCoverageCounts
        {
            Checks = Enum.GetValues<DoctorDomainKind>().LongLength,
            ChecksComplete = domains.LongCount(domain => domain.Coverage == DoctorCoverageState.Complete),
            LinksChecked = observation.LocalReferences.UnavailableCause is null ? references.Count : null,
            LinksIgnored = observation.LocalReferences.IgnoredLinkCount,
            LinksValid = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Facts.Target.Resolution
                == SourceLinkTargetResolution.Complete),
            ExternalLinksNotChecked = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Facts.Target.Resolution
                == SourceLinkTargetResolution.ExternalUnchecked),
            ImageLinks = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Kind == LocalReferenceKind.Image),
            RoutesChecked = observation.Routes.Catalogue is null ? null : observation.Routes.Routes.RouteFacts.Count,
            FrameworkFiles = lifecycle.State == OperationalViewState.Complete
                ? lifecycle.Targets.Count
                : null,
            ExtensionsInstalled = extensions.State == OperationalViewState.Complete
                ? extensions.Packages.Count
                : null,
            LibrariesRegistered = libraries.Record.Record?.Libraries.Length,
        };
    }

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

    private static DoctorBoundaryKind ReadBoundary(DoctorDomainKind domain)
        => domain switch
        {
            DoctorDomainKind.WorkspaceEntry => DoctorBoundaryKind.Workspace,
            DoctorDomainKind.RecoveryResiduals => DoctorBoundaryKind.RecoveryStore,
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => DoctorBoundaryKind.RouteUniverse,
            DoctorDomainKind.LocalReferences => DoctorBoundaryKind.LocalReferenceUniverse,
            DoctorDomainKind.FrameworkLifecycle => DoctorBoundaryKind.FrameworkLifecycle,
            DoctorDomainKind.ExtensionLifecycle => DoctorBoundaryKind.ExtensionLifecycle,
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, "The Doctor domain is not defined."),
        };

}
