using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Comparison.Models;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Comparison;

internal sealed class LibraryInspectComparisonReader(PhysicalPathResolver resolver, CliWorkspace workspace)
{
    private readonly PhysicalPathResolver _resolver = resolver;
    private readonly CliWorkspace _workspace = workspace;

    internal LibraryInspectComparisonRead Read(LibraryRegistration selected, LibraryInventory? inventory, CancellationToken cancellationToken)
    {
        var registered = RegisteredPaths(selected);
        var eligible = (inventory?.Entries ?? [])
            .Select(entry => Eligible(LibraryPathIdentity.Map(selected.SourceRoot, selected.DestinationRoot, entry.SourcePath)))
            .ToArray();
        var inventoryComplete = inventory?.State == LibraryInventoryState.Complete;
        var findings = new List<LibraryInspectFinding>();
        var registeredByPath = registered.ToDictionary(
            path => path.SourcePath,
            StringComparer.Ordinal);
        var eligibleByPath = eligible.ToDictionary(
            path => path.SourcePath,
            StringComparer.Ordinal);
        var paths = new SortedSet<string>(registeredByPath.Keys, StringComparer.Ordinal);
        paths.UnionWith(eligibleByPath.Keys);
        var comparisons = new List<LibraryPathComparison>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Framework.Libraries.Models.Identity.SourceRelativeEligiblePath.Create(path);
            var mapping = LibraryPathIdentity.Map(selected.SourceRoot, selected.DestinationRoot, sourcePath);
            var observation = LibraryMappingObserver.Observe(
                _resolver,
                new LibraryMappingObservationRequest
                {
                    Workspace = _workspace,
                    Mapping = mapping,
                },
                cancellationToken);
            registeredByPath.TryGetValue(path, out var registeredPath);
            var isEligible = eligibleByPath.ContainsKey(path);
            var relation = ReadRelation(
                observation.State,
                registeredPath is not null,
                isEligible,
                inventoryComplete);
            comparisons.Add(new LibraryPathComparison
            {
                SourcePath = path,
                DestinationPath = observation.Mapping.DestinationPath.Value,
                SourceId = SourceIdentity.DeriveId(observation.Mapping.DestinationPath.Value),
                Relation = relation,
                Registered = registeredPath,
                ObservedRelativeLink = observation.Leaf.RelativeFileLink?.RawRelativeTarget,
            });
            AddComparisonFinding(findings, selected, mapping.DestinationPath.Value, relation, observation.Cause);
        }

        return new LibraryInspectComparisonRead
        {
            RegisteredPaths = registered,
            EligiblePaths = eligible,
            Comparisons = [.. comparisons],
            Findings = [.. findings],
        };
    }

    internal static LibraryRegisteredPath[] RegisteredPaths(LibraryRegistration selected)
        => [.. selected.Paths.Select(sourcePath => Registered(selected, sourcePath))];

    private static LibraryComparisonRelation ReadRelation(
        LibraryMappingObservationState observation,
        bool registered,
        bool eligible,
        bool inventoryComplete)
    {
        if (observation == LibraryMappingObservationState.Blocked)
        {
            return LibraryComparisonRelation.Blocked;
        }

        if (observation == LibraryMappingObservationState.Unavailable)
        {
            return LibraryComparisonRelation.Unavailable;
        }

        if (registered && eligible)
        {
            return observation switch
            {
                LibraryMappingObservationState.Current => LibraryComparisonRelation.Current,
                LibraryMappingObservationState.Missing => LibraryComparisonRelation.Missing,
                LibraryMappingObservationState.Changed => LibraryComparisonRelation.Changed,
                _ => throw new ArgumentOutOfRangeException(nameof(observation), observation, "The mapping observation state is not defined."),
            };
        }

        if (eligible)
        {
            return LibraryComparisonRelation.Added;
        }

        return inventoryComplete
            ? LibraryComparisonRelation.Retired
            : LibraryComparisonRelation.Unavailable;
    }

    private static LibraryRegisteredPath Registered(
        LibraryRegistration selected,
        Framework.Libraries.Models.Identity.SourceRelativeEligiblePath sourcePath)
    {
        var mapping = LibraryPathIdentity.Map(selected.SourceRoot, selected.DestinationRoot, sourcePath);
        return new LibraryRegisteredPath
        {
            SourcePath = sourcePath.Value,
            DestinationPath = mapping.DestinationPath.Value,
            ExpectedRelativeLink = mapping.ExpectedRelativeLink.Value,
            SourceId = SourceIdentity.DeriveId(mapping.DestinationPath.Value),
        };
    }

    private static LibraryEligiblePath Eligible(
        LibraryMapping mapping)
        => new()
        {
            SourcePath = mapping.SourcePath.Value,
            DestinationPath = mapping.DestinationPath.Value,
            SourceId = SourceIdentity.DeriveId(mapping.DestinationPath.Value),
        };

    private static void AddComparisonFinding(
        List<LibraryInspectFinding> findings,
        LibraryRegistration selected,
        string path,
        LibraryComparisonRelation relation,
        string? cause)
    {
        (LibraryInspectFindingCode Code, CliSemanticStatus Status, string Cause)? mapped = relation switch
        {
            LibraryComparisonRelation.Current => null,
            LibraryComparisonRelation.Added => (LibraryInspectFindingCode.PathAdded, CliSemanticStatus.Attention, "The eligible source path is not registered."),
            LibraryComparisonRelation.Retired => (LibraryInspectFindingCode.PathRetired, CliSemanticStatus.Attention, "The registered path is absent from the complete source inventory."),
            LibraryComparisonRelation.Missing => (LibraryInspectFindingCode.LinkMissing, CliSemanticStatus.Attention, "The eligible registered destination is absent."),
            LibraryComparisonRelation.Changed => (LibraryInspectFindingCode.LinkChanged, CliSemanticStatus.Attention, "The eligible registered destination differs from its expected relative link."),
            LibraryComparisonRelation.Blocked => (LibraryInspectFindingCode.LinkBlocked, CliSemanticStatus.Blocked, cause ?? "The Library destination is unsafe."),
            LibraryComparisonRelation.Unavailable => (LibraryInspectFindingCode.InventoryIncomplete, CliSemanticStatus.Incomplete, cause ?? "The required Library path facts are incomplete."),
            LibraryComparisonRelation.NotStarted => null,
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "The Library comparison relation is not defined."),
        };
        if (mapped is not { } finding)
        {
            return;
        }

        findings.Add(new LibraryInspectFinding
        {
            Code = finding.Code,
            Status = finding.Status,
            LibraryId = selected.Id.Value,
            Path = path,
            Cause = finding.Cause,
        });
    }

}
