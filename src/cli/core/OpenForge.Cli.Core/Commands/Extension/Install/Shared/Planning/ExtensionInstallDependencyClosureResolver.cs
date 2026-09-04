using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallDependencyClosureResolver
{
    internal ExtensionInstallDependencyClosureResolution Resolve(
        IReadOnlyList<ExtensionPackageFact> universe,
        IReadOnlyList<string> roots)
    {
        var byId = new Dictionary<string, ExtensionPackageFact>(StringComparer.Ordinal);
        foreach (var package in universe)
        {
            if (!byId.TryAdd(package.Id, package))
            {
                return Stop(
                    $"The Extension source universe contains duplicate package ID '{package.Id}'.");
            }
        }

        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<ExtensionPackageFact>();
        string? cause = null;
        foreach (var root in roots.Order(StringComparer.Ordinal))
        {
            if (!Visit(root))
            {
                return Stop(cause ?? "The selected Extension dependency closure is invalid.");
            }
        }

        return new ExtensionInstallDependencyClosureResolution(ordered, finding: null);

        bool Visit(string id)
        {
            if (!byId.TryGetValue(id, out var package))
            {
                cause = $"The selected Extension ID '{id}' is absent from the source universe.";
                return false;
            }

            if (visited.Contains(id))
            {
                return true;
            }

            if (!visiting.Add(id))
            {
                cause = $"The Extension dependency closure contains a cycle at '{id}'.";
                return false;
            }

            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                if (!Visit(dependency))
                {
                    return false;
                }
            }

            _ = visiting.Remove(id);
            _ = visited.Add(id);
            ordered.Add(package);
            return true;
        }
    }

    private static ExtensionInstallDependencyClosureResolution Stop(string cause)
        => new(
            packages: [],
            new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SourceInvalid,
                cause));
}
