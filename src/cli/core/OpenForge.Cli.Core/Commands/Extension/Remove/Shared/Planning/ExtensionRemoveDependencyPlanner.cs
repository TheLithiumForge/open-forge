using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal static class ExtensionRemoveDependencyPlanner
{
    internal static ExtensionRemoveDependencyPlan Build(
        ExtensionLifecycleState lifecycle,
        IReadOnlyList<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet(StringComparer.Ordinal);
        var packages = lifecycle.Packages
            .Select(package => new ExtensionRemovePackageFact(
                package.Id,
                selected.Contains(package.Id),
                package.Dependencies))
            .Concat(selectedIds
                .Where(id => lifecycle.Packages.All(package => package.Id != id))
                .Select(id => new ExtensionRemovePackageFact(id, selectedForRemoval: true, [])))
            .ToArray();
        var blockers = selectedIds
            .Select(id => new
            {
                Id = id,
                Dependents = lifecycle.Packages
                    .Where(package => !selected.Contains(package.Id)
                        && package.Dependencies.Contains(id, StringComparer.Ordinal))
                    .Select(package => package.Id)
                    .ToArray(),
            })
            .Where(value => value.Dependents.Length > 0)
            .Select(value => new ExtensionRemoveRetainedDependentBlocker(value.Id, value.Dependents))
            .ToArray();
        var retainedOrphans = lifecycle.Packages
            .Where(package => !selected.Contains(package.Id)
                && lifecycle.Packages
                    .Where(candidate => selected.Contains(candidate.Id))
                    .Any(candidate => candidate.Dependencies.Contains(package.Id, StringComparer.Ordinal))
                && lifecycle.Packages
                    .Where(candidate => !selected.Contains(candidate.Id))
                    .All(candidate => !candidate.Dependencies.Contains(package.Id, StringComparer.Ordinal)))
            .Select(package => package.Id)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return new ExtensionRemoveDependencyPlan(
            packages,
            ReadRemovalOrder(lifecycle.Packages, selectedIds),
            blockers,
            retainedOrphans);
    }

    private static IReadOnlyList<string> ReadRemovalOrder(
        IReadOnlyList<LifecycleExtensionPackageV1> packages,
        IReadOnlyList<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet(StringComparer.Ordinal);
        var dependencies = packages.ToDictionary(
            package => package.Id,
            package => package.Dependencies.Where(selected.Contains).ToArray(),
            StringComparer.Ordinal);
        foreach (var absent in selectedIds.Where(id => !dependencies.ContainsKey(id)))
        {
            dependencies.Add(absent, []);
        }

        var order = new List<string>();
        var remaining = selectedIds.ToHashSet(StringComparer.Ordinal);
        while (remaining.Count > 0)
        {
            var next = remaining
                .Where(candidate => remaining.All(dependent =>
                    !dependencies[dependent].Contains(candidate, StringComparer.Ordinal)))
                .Order(StringComparer.Ordinal)
                .First();
            order.Add(next);
            remaining.Remove(next);
        }

        return order;
    }
}
