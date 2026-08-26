using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed record GeneratedNavigationBoundedChangeInput
{
    public required SourceLocation ContentLocation { get; init; }

    public required string BeforeBody { get; init; }

    public required string ExpectedBody { get; init; }

    public required string Prefix { get; init; }

    public required string Suffix { get; init; }
}
