using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Framework.Extensions;

internal static class ExtensionDependencyValidator
{
    internal static void ValidateAcyclic(IReadOnlyList<ExtensionPackageFact> packages)
    {
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in packages)
        {
            Visit(package.Id, byId, visiting, visited);
        }
    }

    private static void Visit(
        string id,
        IReadOnlyDictionary<string, ExtensionPackageFact> packages,
        ISet<string> visiting,
        ISet<string> visited)
    {
        if (visited.Contains(id))
        {
            return;
        }

        if (!visiting.Add(id))
        {
            throw new InvalidDataException($"Extension dependency cycle detected at '{id}'.");
        }

        foreach (var dependency in packages[id].Dependencies)
        {
            Visit(dependency, packages, visiting, visited);
        }

        _ = visiting.Remove(id);
        _ = visited.Add(id);
    }
}
