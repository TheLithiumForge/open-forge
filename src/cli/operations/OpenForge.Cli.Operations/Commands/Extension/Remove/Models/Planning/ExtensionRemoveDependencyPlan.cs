using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemovePackageFact
{
    internal ExtensionRemovePackageFact(
        string id,
        bool selectedForRemoval,
        IEnumerable<string> dependencies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(dependencies);

        Id = id;
        SelectedForRemoval = selectedForRemoval;
        Dependencies = SnapshotIds(dependencies, nameof(dependencies));
    }

    internal string Id { get; }

    internal bool SelectedForRemoval { get; }

    internal IReadOnlyList<string> Dependencies { get; }

    private static IReadOnlyList<string> SnapshotIds(
        IEnumerable<string> values,
        string parameterName)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
            if (!seen.Add(value))
            {
                throw new ArgumentException(
                    "Extension Remove dependency IDs must be unique.",
                    parameterName);
            }

            result.Add(value);
        }

        return new ReadOnlyCollection<string>(result.ToArray());
    }
}

internal sealed record ExtensionRemoveRetainedDependentBlocker
{
    internal ExtensionRemoveRetainedDependentBlocker(
        string dependencyId,
        IEnumerable<string> retainedDependentIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dependencyId);
        ArgumentNullException.ThrowIfNull(retainedDependentIds);

        DependencyId = dependencyId;
        RetainedDependentIds = SnapshotIds(
            retainedDependentIds,
            nameof(retainedDependentIds));
        if (RetainedDependentIds.Count == 0)
        {
            throw new ArgumentException(
                "A retained-dependent blocker requires at least one dependent.",
                nameof(retainedDependentIds));
        }
    }

    internal string DependencyId { get; }

    internal IReadOnlyList<string> RetainedDependentIds { get; }

    private static IReadOnlyList<string> SnapshotIds(
        IEnumerable<string> values,
        string parameterName)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
            if (!seen.Add(value))
            {
                throw new ArgumentException(
                    "Extension Remove dependent IDs must be unique.",
                    parameterName);
            }

            result.Add(value);
        }

        result.Sort(StringComparer.Ordinal);
        return new ReadOnlyCollection<string>(result.ToArray());
    }
}

internal sealed record ExtensionRemoveDependencyPlan
{
    internal ExtensionRemoveDependencyPlan(
        IEnumerable<ExtensionRemovePackageFact> packages,
        IEnumerable<string> removalOrder,
        IEnumerable<ExtensionRemoveRetainedDependentBlocker> retainedDependentBlockers,
        IEnumerable<string> retainedOrphanDependencyIds)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(removalOrder);
        ArgumentNullException.ThrowIfNull(retainedDependentBlockers);
        ArgumentNullException.ThrowIfNull(retainedOrphanDependencyIds);

        var packageValues = packages
            .Select(package => package ?? throw new ArgumentException(
                "Extension Remove package facts cannot contain null members.",
                nameof(packages)))
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .ToArray();
        var packageIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in packageValues)
        {
            if (!packageIds.Add(package.Id))
            {
                throw new ArgumentException(
                    "Extension Remove package IDs must be unique.",
                    nameof(packages));
            }
        }

        foreach (var package in packageValues)
        {
            foreach (var dependencyId in package.Dependencies)
            {
                if (!packageIds.Contains(dependencyId))
                {
                    throw new ArgumentException(
                        "Extension Remove dependency IDs must identify known packages.",
                        nameof(packages));
                }
            }
        }

        var orderValues = SnapshotIds(removalOrder, nameof(removalOrder));
        var selectedIds = packageValues
            .Where(package => package.SelectedForRemoval)
            .Select(package => package.Id)
            .ToArray();
        if (selectedIds.Length != orderValues.Count
            || !selectedIds.All(orderValues.Contains))
        {
            throw new ArgumentException(
                "Extension Remove removal order must contain every selected package exactly once.",
                nameof(removalOrder));
        }

        var removalPositions = orderValues
            .Select((id, position) => (id, position))
            .ToDictionary(value => value.id, value => value.position, StringComparer.Ordinal);
        var selectedPackageIds = new HashSet<string>(selectedIds, StringComparer.Ordinal);
        foreach (var package in packageValues.Where(package => package.SelectedForRemoval))
        {
            foreach (var dependencyId in package.Dependencies)
            {
                if (selectedPackageIds.Contains(dependencyId)
                    && removalPositions[package.Id] >= removalPositions[dependencyId])
                {
                    throw new ArgumentException(
                        "Extension Remove removal order must precede every selected dependency.",
                        nameof(removalOrder));
                }
            }
        }

        var blockerValues = retainedDependentBlockers
            .Select(blocker => blocker ?? throw new ArgumentException(
                "Extension Remove retained-dependent blockers cannot contain null members.",
                nameof(retainedDependentBlockers)))
            .OrderBy(blocker => blocker.DependencyId, StringComparer.Ordinal)
            .ToArray();
        var blockerIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var blocker in blockerValues)
        {
            if (!blockerIds.Add(blocker.DependencyId))
            {
                throw new ArgumentException(
                    "Extension Remove dependencies may have only one blocker fact.",
                    nameof(retainedDependentBlockers));
            }

            if (!packageValues.Any(package =>
                    package.Id == blocker.DependencyId && package.SelectedForRemoval))
            {
                throw new ArgumentException(
                    "Extension Remove blocker dependencies must be selected packages.",
                    nameof(retainedDependentBlockers));
            }

            foreach (var retainedDependentId in blocker.RetainedDependentIds)
            {
                var retainedDependent = packageValues.FirstOrDefault(
                    package => package.Id == retainedDependentId);
                if (retainedDependent is null
                    || retainedDependent.SelectedForRemoval
                    || !retainedDependent.Dependencies.Contains(
                        blocker.DependencyId,
                        StringComparer.Ordinal))
                {
                    throw new ArgumentException(
                        "Extension Remove retained dependents must be known unselected packages with the blocked edge.",
                        nameof(retainedDependentBlockers));
                }
            }
        }

        var orphanValues = SnapshotIds(
            retainedOrphanDependencyIds,
            nameof(retainedOrphanDependencyIds));
        foreach (var orphanId in orphanValues)
        {
            var orphan = packageValues.FirstOrDefault(package => package.Id == orphanId);
            if (orphan is null || orphan.SelectedForRemoval)
            {
                throw new ArgumentException(
                    "Extension Remove orphan dependency IDs must identify known unselected packages.",
                    nameof(retainedOrphanDependencyIds));
            }

            if (!packageValues
                .Where(package => package.SelectedForRemoval)
                .Any(package => package.Dependencies.Contains(
                    orphanId,
                    StringComparer.Ordinal)))
            {
                throw new ArgumentException(
                    "Extension Remove orphan dependency IDs must be dependencies of selected packages.",
                    nameof(retainedOrphanDependencyIds));
            }
        }

        Packages = new ReadOnlyCollection<ExtensionRemovePackageFact>(packageValues);
        RemovalOrder = orderValues;
        RetainedDependentBlockers = new ReadOnlyCollection<ExtensionRemoveRetainedDependentBlocker>(blockerValues);
        RetainedOrphanDependencyIds = orphanValues;
    }

    internal IReadOnlyList<ExtensionRemovePackageFact> Packages { get; }

    internal IReadOnlyList<string> RemovalOrder { get; }

    internal IReadOnlyList<ExtensionRemoveRetainedDependentBlocker> RetainedDependentBlockers { get; }

    internal IReadOnlyList<string> RetainedOrphanDependencyIds { get; }

    private static IReadOnlyList<string> SnapshotIds(
        IEnumerable<string> values,
        string parameterName)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
            if (!seen.Add(value))
            {
                throw new ArgumentException(
                    "Extension Remove IDs must be unique.",
                    parameterName);
            }

            result.Add(value);
        }

        return new ReadOnlyCollection<string>(result.ToArray());
    }
}
