using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Framework.Sources.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;

internal static class DoctorMaintenanceGuidance
{
    internal static DoctorDomainReport[] Apply(DoctorDomainReport[] domains, DoctorObservation observation)
    {
        if (observation.Routes.Catalogue is not { } catalogue) return domains.ToArray();
        if (observation.Maintenance is not { } maintenance) return domains;
        var malformed = domains.SelectMany(domain => domain.Findings)
            .Where(finding => finding.Kind is DoctorFindingKind.WorkspaceFrontmatterMalformed or DoctorFindingKind.WorkspaceFrontmatterDuplicate)
            .Select(finding => finding.Subject.Path).OfType<string>().Concat(maintenance.MalformedMetadata.Keys).ToHashSet(StringComparer.Ordinal);
        return domains.Select(domain =>
        {
            var findings = domain.Findings.Where(finding => !(finding.Kind == DoctorFindingKind.RouteMetadataRequiredMissing
                    && finding.Subject.Path is { } path && (malformed.Contains(path)
                        || maintenance.Eligibility.GetValueOrDefault(path) == RoutedMetadataEligibility.OutsideRoot)))
                .Select(finding => Project(finding, maintenance, catalogue.Layout)).ToList();
            var malformedPaths = new HashSet<string>(StringComparer.Ordinal);
            findings.RemoveAll(finding => finding.Kind == DoctorFindingKind.WorkspaceFrontmatterMalformed
                && finding.Subject.Path is { } malformedPath && !malformedPaths.Add(malformedPath));
            if (domain.Domain == DoctorDomainKind.WorkspaceEntry)
            {
                foreach (var (path, cause) in maintenance.MalformedMetadata)
                {
                    if (findings.Any(finding => finding.Kind == DoctorFindingKind.WorkspaceFrontmatterMalformed && finding.Subject.Path == path)) continue;
                    var source = observation.Routes.Sources.First(source => source.Source.Identity.CanonicalBasePath == path);
                    var location = source.Document?.Frontmatter.BlockSpan is { } span
                        ? new Utf8SourceMap(source.Document.Source).Map(span.Start, span.Length) : null;
                    var finding = DoctorDomainSupport.Create(DoctorDomainSupport.Error(DoctorFindingKind.WorkspaceFrontmatterMalformed, cause, DoctorResolutionLane.ManualDecision),
                        DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path) with { Location = location },
                        DoctorDomainSupport.Provenance(domain.Domain, DoctorProvenanceSource.RouteMetadata, path),
                        [new DoctorAuthoredValueEvidence(cause, location)]);
                    findings.Add(Project(finding, maintenance, observation.Routes.Catalogue.Layout));
                }
            }
            if (domain.Domain == DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation)
            {
                foreach (var (path, canonicalName) in maintenance.OldNavigationPaths.OrderBy(item => item.Key, StringComparer.Ordinal))
                {
                    var canonicalPath = SourceLogicalPath.Combine(SourceLogicalPath.ReadParent(path), canonicalName);
                    findings.RemoveAll(finding => finding.Kind is DoctorFindingKind.RouteUnreachable or DoctorFindingKind.RouteDetached && finding.Subject.Path == path);
                    var finding = DoctorDomainSupport.Create(DoctorDomainSupport.Warning(DoctorFindingKind.RouteUnreachable,
                            "An old navigation file remains in a renamed folder.", DoctorResolutionLane.ManualDecision),
                        DoctorDomainSupport.Subject(DoctorSubjectKind.Route, path),
                        DoctorDomainSupport.Provenance(domain.Domain, DoctorProvenanceSource.RouteInventory, path),
                        [new DoctorAuthoredValueEvidence(path, null)]);
                    findings.Add(Explain(finding, DoctorMaintenanceCondition.OldNavigation, finding.Message, DoctorResolutionLane.ManualDecision,
                        Manual(path)) with
                    {
                        Maintenance = new()
                        {
                            Condition = DoctorMaintenanceCondition.OldNavigation,
                            CanonicalName = canonicalName,
                            ExistingCanonicalPath = maintenance.ExistingCanonicalNavigationPaths.Contains(canonicalPath) ? canonicalPath : null,
                            IndexCommand = IndexCommand(catalogue.Workspace)
                        },
                    });
                }
            }
            return domain with
            {
                Findings = DoctorFindingAggregation.Order(findings),
                Counts = DoctorFindingAggregation.Count(findings),
                Actions = DoctorFindingAggregation.Actions(findings),
            };
        }).ToArray();
    }

    internal static DoctorFinding Project(DoctorFinding finding, DoctorMaintenanceObservation maintenance, WorkspaceLayout layout)
    {
        if (finding.Subject.Path is not { } path || !maintenance.Eligibility.TryGetValue(path, out var eligibility)) return finding;
        if (finding.Kind is DoctorFindingKind.WorkspaceFrontmatterMalformed or DoctorFindingKind.WorkspaceFrontmatterDuplicate
            && (eligibility is RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.OutsideRoot || maintenance.MalformedMetadata.ContainsKey(path)))
            return Explain(finding, DoctorMaintenanceCondition.MalformedFrontmatter, finding.Message, DoctorResolutionLane.ManualDecision,
                Manual(path)) with
            {
                Kind = DoctorFindingKind.WorkspaceFrontmatterMalformed,
                Severity = DoctorFindingSeverity.Error,
                Maintenance = new()
                {
                    Condition = DoctorMaintenanceCondition.MalformedFrontmatter,
                    IndexCommand = IndexCommand(layout.Workspace),
                    Cause = maintenance.MalformedMetadata.GetValueOrDefault(path),
                }
            };
        if (finding.Kind is DoctorFindingKind.RouteUnreachable or DoctorFindingKind.RouteDetached)
        {
            if (eligibility == RoutedMetadataEligibility.OutsideRoot)
                return Explain(finding, DoctorMaintenanceCondition.OutsideFolders, finding.Message, DoctorResolutionLane.ManualDecision, Manual(path));
            if (maintenance.IndexablePaths.Contains(path))
            {
                if (!maintenance.ChainGapPaths.Contains(path))
                    return finding with
                    {
                        Resolution = DoctorResolutionLane.TargetedOperation,
                        Actions = [Command(DoctorNextOperation.Index, "open-forge index", "Refresh the existing navigation.")],
                    };
                return Explain(finding, DoctorMaintenanceCondition.ChainGap, finding.Message, DoctorResolutionLane.TargetedOperation,
                    Command(DoctorNextOperation.Index, "open-forge index", "Complete the eligible route chain."));
            }
            if (eligibility is RoutedMetadataEligibility.SettingsExcluded or RoutedMetadataEligibility.PrivateContent)
                return Explain(finding, DoctorMaintenanceCondition.RestorationBoundary, finding.Message, DoctorResolutionLane.ManualDecision, Manual(path));
            return Explain(finding, DoctorMaintenanceCondition.UnsafeBoundary, finding.Message, DoctorResolutionLane.ManualDecision,
                Manual(path));
        }
        if (finding.Kind != DoctorFindingKind.RouteMetadataRequiredMissing) return finding;
        if (maintenance.IndexablePaths.Contains(path))
        {
            var condition = SourceFormClassifier.TryClassify(layout, path, out var form) && form == SourceDocumentForm.Skill
                ? DoctorMaintenanceCondition.MissingSkillFields : DoctorMaintenanceCondition.MissingFields;
            return Explain(finding, condition, finding.Message, DoctorResolutionLane.TargetedOperation,
                Command(DoctorNextOperation.Index, "open-forge index", "Complete the eligible missing metadata."));
        }
        if (eligibility == RoutedMetadataEligibility.ManagedContent)
        {
            return Explain(finding, DoctorMaintenanceCondition.ManagedMetadata, finding.Message, DoctorResolutionLane.TargetedOperation,
                maintenance.ManagingUpdates.GetValueOrDefault(path) ?? Command(DoctorNextOperation.Update, "open-forge update", "The managing operation owns this metadata."));
        }
        if (eligibility is RoutedMetadataEligibility.SettingsExcluded or RoutedMetadataEligibility.PrivateContent)
            return Explain(finding, DoctorMaintenanceCondition.RestorationBoundary, finding.Message, DoctorResolutionLane.ManualDecision, Manual(path));
        return Explain(finding, DoctorMaintenanceCondition.UnsafeBoundary, finding.Message, DoctorResolutionLane.ManualDecision,
            Manual(path));
    }

    internal static DoctorFinding Explain(DoctorFinding finding, DoctorMaintenanceCondition condition, string message, DoctorResolutionLane lane, DoctorNextAction action)
        => finding with { Maintenance = new() { Condition = condition }, Message = message, Resolution = lane, Actions = [action] };

    internal static DoctorNextAction Command(DoctorNextOperation operation, string command, string reason)
        => new() { Kind = DoctorNextActionKind.AcceptedOperation, Operation = operation, Command = command, Reason = reason };
    internal static DoctorNextAction Manual(string instruction)
        => new() { Kind = DoctorNextActionKind.ManualDecision, Operation = null, Command = null, Reason = instruction };
    private static string IndexCommand(CliWorkspace workspace)
        => workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace ? $"open-forge index --workspace {CliCommandText.Quote(workspace.LexicalRoot)}" : "open-forge index";
}
