using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectInstalledClosureReader
{
    internal static IReadOnlyList<LifecycleInstalledPackage> Read(
        IReadOnlyList<LifecycleInstalledPackage> packages,
        LifecycleInstalledPackage? selected)
    {
        if (selected is null)
        {
            return [];
        }

        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var closure = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(selected.Id);
        while (pending.TryPop(out var id))
        {
            if (!closure.Add(id) || !byId.TryGetValue(id, out var package))
            {
                continue;
            }

            foreach (var dependency in package.Dependencies)
            {
                pending.Push(dependency);
            }
        }

        return packages
            .Where(package => closure.Contains(package.Id))
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .ToArray();
    }
}
