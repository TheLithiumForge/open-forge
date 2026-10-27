using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceIncludeDoctorInspector
{
    internal static DoctorDomainReport InspectOutside(DoctorObservation observation, DoctorDomainReport domain)
    {
        if (observation.OutsideRoutes is not { } outside) return domain;
        var findings = domain.Findings.ToList();
        var coverage = domain.Coverage;
        if (!outside.RegistrationInventoryComplete)
        {
            findings.Add(CreateOutside(WorkspaceSettingsDefinitions.RelativePath,
                new(DoctorOutsideRouteState.Unavailable, outside.LayoutRead.Settings.Cause)));
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        }
        foreach (var route in outside.Routes)
        {
            var directory = route.Directory;
            var descriptor = directory.Descriptor;
            var candidates = outside.Inspection?.Catalogue.Candidates.Where(candidate => candidate.Form is { } form
                    && SourceFormClassifier.IsEntrypoint(form) && SourceLogicalPath.ReadParent(candidate.CanonicalPath) == descriptor.DirectoryPath)
                .Select(candidate => candidate.CanonicalPath).Concat(route.EntrypointPaths).Distinct(StringComparer.Ordinal).ToArray() ?? [];
            for (var index = findings.Count - 1; index >= 0; index--)
            {
                var finding = findings[index];
                if (finding.IsIncludedFile || finding.Subject.Path is not { } path
                    || path != descriptor.DirectoryPath && (!path.StartsWith(descriptor.DirectoryPath + "/", StringComparison.Ordinal)
                        || SourceLogicalPath.ReadParent(path) != descriptor.DirectoryPath)) continue;
                if (finding.Kind == DoctorFindingKind.WorkspaceDetached)
                {
                    findings.RemoveAt(index);
                    continue;
                }
                if (finding.Kind is DoctorFindingKind.WorkspaceEntryAmbiguous or DoctorFindingKind.WorkspaceEntryCompatibilityCollision)
                    findings[index] = finding with { OutsideRoute = new(DoctorOutsideRouteState.AmbiguousAnchor), Resolution = DoctorResolutionLane.ManualDecision, Actions = [] };
                else if (finding.Kind == DoctorFindingKind.WorkspacePhysicalAlias)
                    findings[index] = finding with { OutsideRoute = new(DoctorOutsideRouteState.PhysicalAlias), Actions = [] };
                else if (finding.Kind == DoctorFindingKind.WorkspacePathContainment)
                    findings[index] = finding with { OutsideRoute = new(DoctorOutsideRouteState.UnsafeBoundary), Actions = [] };
                else if (finding.Kind is DoctorFindingKind.WorkspaceRootMissing or DoctorFindingKind.WorkspaceRootUnreachable)
                    findings[index] = finding with { OutsideRoute = new(DoctorOutsideRouteState.StaleLoaderTarget), Resolution = DoctorResolutionLane.ManualDecision, Actions = [] };
            }
            var cause = directory.Failure?.DirectCause ?? directory.Issues.FirstOrDefault()?.Cause;
            switch (directory.State)
            {
                case WorkspaceOutsideRouteReadState.Missing:
                    findings.Add(CreateOutside(descriptor.DirectoryPath, new(DoctorOutsideRouteState.MissingDirectory, cause)));
                    break;
                case WorkspaceOutsideRouteReadState.Unsafe:
                    var state = directory.Issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.OutsideOverlap)
                        ? DoctorOutsideRouteState.PhysicalAlias : DoctorOutsideRouteState.UnsafeBoundary;
                    findings.Add(CreateOutside(descriptor.DirectoryPath, new(state, cause)));
                    coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Blocked);
                    break;
                case WorkspaceOutsideRouteReadState.Unavailable:
                    findings.Add(CreateOutside(descriptor.DirectoryPath, new(DoctorOutsideRouteState.Unavailable, cause)));
                    coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
                    break;
                case WorkspaceOutsideRouteReadState.Available:
                    if (candidates.Length > 1)
                    {
                        foreach (var candidate in candidates.Where(candidate => !findings.Any(finding => finding.Subject.Path == candidate
                            && finding.OutsideRoute?.State == DoctorOutsideRouteState.AmbiguousAnchor)))
                            findings.Add(CreateOutside(candidate, new(DoctorOutsideRouteState.AmbiguousAnchor)));
                    }
                    else if (candidates.Length == 0 && AnchorInventoryComplete(outside, descriptor.DirectoryPath))
                    {
                        var canInit = CanInitialize(outside, route);
                        findings.Add(CreateOutside(descriptor.CataloguePath,
                            new(DoctorOutsideRouteState.MissingAnchor, InitTarget: canInit ? descriptor.CataloguePath : null)));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(observation), directory.State, "The outside directory state is not defined.");
            }
            if (route.Listed is false)
                findings.Add(CreateOutside(descriptor.DirectoryPath, new(DoctorOutsideRouteState.Unlisted)));
        }
        return domain with
        {
            Coverage = coverage,
            Findings = DoctorFindingAggregation.Order(findings),
            Counts = DoctorFindingAggregation.Count(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static bool AnchorInventoryComplete(RouteOutsideRoutesObservation outside, string directory)
        => outside.Inspection is { Catalogue.IsCancelled: false } inspection
            && !inspection.Catalogue.Issues.Any(issue => issue.AttemptedCanonicalPath == directory
                || issue.AttemptedCanonicalPath.StartsWith(directory + "/", StringComparison.Ordinal));

    private static bool CanInitialize(RouteOutsideRoutesObservation outside, RouteOutsideRouteObservation route)
    {
        if (route.Directory.CatalogueSnapshot?.Kind != FileExpectationKind.Missing
            || route.Directory.PhysicalDirectory is null
            || !AnchorInventoryComplete(outside, route.Directory.Descriptor.DirectoryPath)) return false;
        var policy = ReferenceProtectionReader.Read(outside.LayoutRead).Policy;
        return policy is not null && new RoutedMetadataCompletionPolicy().Protection(route.Directory.Descriptor.CataloguePath, policy)
            == RoutedMetadataEligibility.Eligible;
    }

    private static DoctorFinding CreateOutside(string path, DoctorOutsideRouteCondition condition)
    {
        var descriptor = condition.State switch
        {
            DoctorOutsideRouteState.MissingDirectory => DoctorDomainSupport.Warning(DoctorFindingKind.WorkspaceRootMissing,
                "The registered outside directory is missing.", DoctorResolutionLane.ManualDecision),
            DoctorOutsideRouteState.MissingAnchor => DoctorDomainSupport.Warning(DoctorFindingKind.WorkspaceEntryMissing,
                "The registered outside directory has no recognized root entrypoint.",
                condition.InitTarget is null ? DoctorResolutionLane.ManualDecision : DoctorResolutionLane.TargetedOperation),
            DoctorOutsideRouteState.Unlisted => DoctorDomainSupport.Information(DoctorFindingKind.WorkspaceDetached,
                "The registered outside directory has no direct root Loader row."),
            DoctorOutsideRouteState.UnsafeBoundary => DoctorDomainSupport.Error(DoctorFindingKind.WorkspacePathContainment,
                "The registered outside boundary is unsafe."),
            DoctorOutsideRouteState.PhysicalAlias => DoctorDomainSupport.Error(DoctorFindingKind.WorkspacePhysicalAlias,
                "The registered outside boundary has an unsafe physical alias."),
            DoctorOutsideRouteState.Unavailable => DoctorDomainSupport.Error(DoctorFindingKind.WorkspaceParseIncomplete,
                "The registered outside observation is unavailable.", DoctorResolutionLane.ManualDecision),
            DoctorOutsideRouteState.AmbiguousAnchor => DoctorDomainSupport.Error(DoctorFindingKind.WorkspaceEntryAmbiguous,
                "The registered outside root has competing recognized entrypoints.", DoctorResolutionLane.ManualDecision),
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.State, "This outside condition annotates an existing finding."),
        };
        return DoctorDomainSupport.Create(descriptor, DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry,
                WorkspaceSettingsDefinitions.RelativePath),
            [new DoctorStateEvidence(condition.State switch
            {
                DoctorOutsideRouteState.MissingDirectory or DoctorOutsideRouteState.MissingAnchor => DoctorObservedState.Missing,
                DoctorOutsideRouteState.UnsafeBoundary or DoctorOutsideRouteState.PhysicalAlias or DoctorOutsideRouteState.AmbiguousAnchor => DoctorObservedState.Blocked,
                DoctorOutsideRouteState.Unavailable => DoctorObservedState.Unavailable,
                DoctorOutsideRouteState.Unlisted => DoctorObservedState.Present,
                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            }), new DoctorAuthoredValueEvidence(condition.Cause ?? path, null)]) with
        { OutsideRoute = condition };
    }

    internal static void Inspect(string loaderPath, RouteIncludedFilesObservation? includes, ICollection<DoctorFinding> findings)
    {
        foreach (var file in includes?.Files ?? [])
        {
            if (file.Missing)
                findings.Add(Create(loaderPath, file, DoctorDomainSupport.Error(DoctorFindingKind.WorkspaceRootMissing,
                    "The included file is missing. Restore the file or exclude its path.", DoctorResolutionLane.ManualDecision)));
            else if (file.Registered && file.Rejection is { } rejection)
                findings.Add(Create(loaderPath, file, DoctorDomainSupport.Warning(DoctorFindingKind.WorkspacePathInvalid,
                    $"{rejection} Update or remove its registration.", DoctorResolutionLane.ManualDecision)));

            if (file.Registered && !file.Listed)
                findings.Add(Create(loaderPath, file, DoctorDomainSupport.Information(DoctorFindingKind.WorkspaceDetached,
                    "The registered include has no loader row, so agents do not read it. Include it again to list it, or exclude it to remove the registration.")));
        }
    }

    private static DoctorFinding Create(string loaderPath, RouteIncludedFileObservation file, DoctorFindingDescriptor descriptor)
        => DoctorDomainSupport.Create(descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, file.Path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry,
                file.Registered ? WorkspaceSettingsDefinitions.RelativePath : loaderPath),
            [new DoctorAuthoredValueEvidence(file.Path, Location: null)]) with
        { IsIncludedFile = true };
}
