using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Selection;

internal static class RemoveSelectionResolver
{
    internal static RemoveSelection Resolve(string? rawTarget, string? rawKind)
    {
        var target = rawTarget ?? string.Empty;
        if (string.IsNullOrWhiteSpace(target))
        {
            return RemoveSelection.Invalid(
                "<target>",
                "Supply one target for the Remove command.");
        }

        if (!TryReadKind(rawKind, out var kind))
        {
            return RemoveSelection.Invalid(
                target,
                "Choose one Remove kind: path, route, extension, or library.");
        }

        switch (kind)
        {
            case RemoveKind.Path:
                var path = target.StartsWith("./", StringComparison.Ordinal)
                    ? target[2..]
                    : target;
                return PortableWorkspacePath.TryNormalize(path, out var normalized)
                    ? RemoveSelection.Selected(kind, normalized)
                    : RemoveSelection.Invalid(
                        target,
                        "Path targets must be exact portable workspace-relative paths without traversal or wildcard segments.");
            case RemoveKind.Route:
                return ContainsGlob(target)
                    ? RemoveSelection.Invalid(target, "Route targets cannot contain wildcard characters.")
                    : RemoveSelection.Selected(kind, target);
            case RemoveKind.Extension:
                return ExtensionIdentity.IsValidStableId(target)
                    ? RemoveSelection.Selected(kind, target)
                    : RemoveSelection.Invalid(target, "The extension target must be a valid lowercase Extension ID.");
            case RemoveKind.Library:
                return LibraryId.TryCreate(target) is not null
                    ? RemoveSelection.Selected(kind, target)
                    : RemoveSelection.Invalid(target, "The library target must be a valid lowercase Library ID.");
            default:
                throw new ArgumentOutOfRangeException(nameof(rawKind), rawKind, "The Remove kind is not defined.");
        }
    }

    private static bool TryReadKind(string? value, out RemoveKind kind)
    {
        switch (value ?? "path")
        {
            case "path": kind = RemoveKind.Path; return true;
            case "route": kind = RemoveKind.Route; return true;
            case "extension": kind = RemoveKind.Extension; return true;
            case "library": kind = RemoveKind.Library; return true;
            default: kind = default; return false;
        }
    }

    private static bool ContainsGlob(string target)
        => target.Contains('*', StringComparison.Ordinal)
            || target.Contains('?', StringComparison.Ordinal)
            || target.Contains('[', StringComparison.Ordinal)
            || target.Contains(']', StringComparison.Ordinal);
}
