using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectInstalledClosureReader
{
    internal static WorkspaceOwnershipRead ReadOwnership(WorkspaceOwnershipRead ownership)
    {
        if (!ownership.IsTrustworthy) return ownership;
        if (ownership.Document.Extensions.GroupBy(package => package.Id, StringComparer.Ordinal).Any(group => group.Count() != 1)
            || ownership.Document.Extensions.Any(package => !ExtensionIdentity.IsValidStableId(package.Id)
                || package.Paths.Any(path => !ExtensionDestinationPolicy.IsAllowed(path)))
            || !HasCompleteDependencies(ownership.Document.Extensions))
        {
            return ownership with
            {
                State = WorkspaceOwnershipReadState.Invalid,
                Document = WorkspaceOwnershipDocument.Empty,
                Cause = "Recorded Extension ownership cannot be interpreted safely; no installed ownership can be established.",
            };
        }
        return ownership;
    }

    private static bool HasCompleteDependencies(IReadOnlyList<ExtensionOwnership> packages)
    {
        // Duplicate IDs are rejected before this check.
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        var complete = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string id)
        {
            if (complete.Contains(id)) return true;
            if (!byId.TryGetValue(id, out var package) || !active.Add(id)) return false;
            if (!package.Dependencies.All(Visit)) return false;
            active.Remove(id);
            complete.Add(id);
            return true;
        }
        return packages.All(package => Visit(package.Id));
    }


    internal static IReadOnlyList<ExtensionOwnership> Read(
        IReadOnlyList<ExtensionOwnership> packages,
        ExtensionOwnership? selected)
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
