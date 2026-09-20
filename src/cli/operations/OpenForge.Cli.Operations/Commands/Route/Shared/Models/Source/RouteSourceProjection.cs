using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteSourceLayerProjection
{
    ExactLayers,
    BaseOnly,
}

internal sealed class RouteSourceProjection
{
    internal RouteSourceProjection(
        SourceLogicalSource logicalSource,
        RouteSource? source,
        SourceDocumentReadResult baseRead,
        SourceDocumentReadResult? overwriteRead)
    {
        ArgumentNullException.ThrowIfNull(logicalSource);
        ArgumentNullException.ThrowIfNull(baseRead);
        if (!ReferenceEquals(logicalSource.Base, baseRead.Layer))
        {
            throw new ArgumentException("The base read must reference the logical source base layer.", nameof(baseRead));
        }

        if (overwriteRead is not null
            && !ReferenceEquals(logicalSource.Overwrite, overwriteRead.Layer))
        {
            throw new ArgumentException("The overwrite read must reference the logical source overwrite layer.", nameof(overwriteRead));
        }

        if (logicalSource.Overwrite is null && overwriteRead is not null)
        {
            throw new ArgumentException("A source without an overwrite layer cannot carry an overwrite read.", nameof(overwriteRead));
        }

        if (source is not null
            && (!string.Equals(source.Id, logicalSource.Identity.AutomaticId, StringComparison.Ordinal)
                || !string.Equals(source.CanonicalPath, logicalSource.Identity.CanonicalBasePath, StringComparison.Ordinal)
                || (source.Overwrite is not null) != (overwriteRead is not null)))
        {
            throw new ArgumentException("The Route source must project the logical identity and selected layers.", nameof(source));
        }

        LogicalSource = logicalSource;
        Source = source;
        BaseRead = baseRead;
        OverwriteRead = overwriteRead;
    }

    internal SourceLogicalSource LogicalSource { get; }

    internal RouteSource? Source { get; }

    internal SourceDocumentReadResult BaseRead { get; }

    internal SourceDocumentReadResult? OverwriteRead { get; }
}
