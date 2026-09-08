using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;

internal static class WorkspacePermissionEvaluator
{
    internal static WorkspacePermissionEvaluation Evaluate(
        WorkspacePermissionDocument document,
        ImmutableArray<WorkspacePermissionRequirement> requirements)
    {
        ImmutableArray<WorkspacePermissionRequirement> required = [.. requirements.Distinct()
            .OrderBy(value => value.Subject.Id, StringComparer.Ordinal)
            .ThenBy(value => value.Path, StringComparer.Ordinal)];
        if (required.IsEmpty)
        {
            return new(required, [], WorkspacePermissionDecision.NotRequired);
        }
        ImmutableArray<WorkspacePermissionRequirement> missing = [.. required.Where(value => !HasGrant(document, value))];
        return new(required, missing, missing.IsEmpty ? WorkspacePermissionDecision.Granted : WorkspacePermissionDecision.Required);
    }

    internal static WorkspacePermissionDocument Grant(
        WorkspacePermissionDocument document,
        ImmutableArray<WorkspacePermissionRequirement> requirements)
    {
        var extensions = document.Extensions.ToDictionary(value => value.Id, StringComparer.Ordinal);
        var libraries = document.Libraries.ToDictionary(value => value.Id, StringComparer.Ordinal);
        foreach (var requirement in requirements)
        {
            switch (requirement.Subject)
            {
                case ExtensionPermissionSubject extension:
                    var paths = extensions.TryGetValue(extension.Id, out var existingExtension) ? existingExtension.Paths : [];
                    extensions[extension.Id] = new(extension.Id, AddPath(paths, requirement.Path));
                    break;
                case LibraryPermissionSubject library:
                    var libraryPaths = ImmutableArray<string>.Empty;
                    if (libraries.TryGetValue(library.Id, out var existingLibrary))
                    {
                        if (existingLibrary.SourceRoot != library.SourceRoot)
                        {
                            throw new InvalidOperationException("An existing Library permission cannot be silently rebound to another source root.");
                        }
                        libraryPaths = existingLibrary.Paths;
                    }
                    libraries[library.Id] = new(library.Id, library.SourceRoot, AddPath(libraryPaths, requirement.Path));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requirements), "The permission subject is not defined.");
            }
        }
        return new(
            [.. extensions.Values.OrderBy(value => value.Id, StringComparer.Ordinal)],
            [.. libraries.Values.OrderBy(value => value.Id, StringComparer.Ordinal)]);
    }

    private static bool HasGrant(WorkspacePermissionDocument document, WorkspacePermissionRequirement requirement)
        => requirement.Subject switch
        {
            ExtensionPermissionSubject extension => document.Extensions.Any(value => value.Id == extension.Id
                && HasPath(value.Paths, requirement.Path)),
            LibraryPermissionSubject library => document.Libraries.Any(value => value.Id == library.Id
                && value.SourceRoot == library.SourceRoot
                && HasPath(value.Paths, requirement.Path)),
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), "The permission subject is not defined."),
        };

    private static ImmutableArray<string> AddPath(ImmutableArray<string> paths, string path)
        => [.. paths.Append(path).DistinctBy(PortableWorkspacePath.CreatePortableKey, StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    private static bool HasPath(ImmutableArray<string> paths, string path)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        return paths.Any(value => PortableWorkspacePath.CreatePortableKey(value) == key);
    }
}
