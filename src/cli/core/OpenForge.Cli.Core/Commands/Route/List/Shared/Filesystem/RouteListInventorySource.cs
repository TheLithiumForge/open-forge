using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal enum RouteListSourceKind
{
    Loader,
    Entrypoint,
    RoutedLeaf,
    RoutedNative,
    Unrouted,
}

internal sealed class RouteListInventorySource
{
    internal RouteListInventorySource(RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Base.Form == RouteSourceForm.OverwriteCompanion)
        {
            throw new ArgumentException("An overwrite companion cannot be an inventory source base.", nameof(source));
        }

        Source = source;
        Kind = ReadListKind(source);
    }

    internal RouteSource Source { get; }

    internal RouteListSourceKind Kind { get; }

    private static RouteListSourceKind ReadListKind(RouteSource source)
    {
        return source.Kind switch
        {
            RouteSourceKind.Loader => RouteListSourceKind.Loader,
            RouteSourceKind.Entrypoint => RouteListSourceKind.Entrypoint,
            RouteSourceKind.Markdown => source.Metadata.State == RouteSourceMetadataState.Complete
                ? RouteListSourceKind.RoutedLeaf
                : RouteListSourceKind.Unrouted,
            RouteSourceKind.Native => source.Metadata.State == RouteSourceMetadataState.Complete
                ? RouteListSourceKind.RoutedNative
                : RouteListSourceKind.Unrouted,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "The source kind is not defined."),
        };
    }
}
