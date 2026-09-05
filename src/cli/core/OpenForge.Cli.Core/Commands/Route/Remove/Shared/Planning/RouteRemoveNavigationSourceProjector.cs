using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveNavigationSourceProjector
{
    internal ImmutableArray<SourceLogicalSource> ProjectIntended(RouteRemoveResolvedSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return subject.Catalogue.Sources
            .Where(source => !IsRemoved(subject, source))
            .ToImmutableArray();
    }

    private static bool IsRemoved(
        RouteRemoveResolvedSubject subject,
        SourceLogicalSource source)
    {
        if (subject.Kind == Models.Result.RouteRemoveSubjectKind.Leaf)
        {
            return string.Equals(
                source.Identity.CanonicalBasePath,
                subject.SelectedSource.Identity.CanonicalBasePath,
                StringComparison.Ordinal);
        }

        var root = SourceLogicalPath.ReadParent(subject.SelectedSource.Identity.CanonicalBasePath);
        return source.Identity.CanonicalBasePath.StartsWith($"{root}/", StringComparison.Ordinal);
    }
}
