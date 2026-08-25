using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal sealed class RouteSourceIdentityCollision
{
    internal RouteSourceIdentityCollision(string id, IEnumerable<string> paths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(paths);
        var materialized = paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (materialized.Length < 2
            || materialized.Any(path => !IsCanonicalPath(path))
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("An identity collision requires at least two unique canonical paths.", nameof(paths));
        }

        Id = id;
        Paths = new ReadOnlyCollection<string>(materialized);
    }

    internal string Id { get; }

    internal IReadOnlyList<string> Paths { get; }

    private static bool IsCanonicalPath(string? path)
    {
        return SourceLogicalPath.IsCanonical(path);
    }
}
