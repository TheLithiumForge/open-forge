using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;

internal sealed record RouteCreateNavigationChange
{
    public required SourceLogicalSource Source { get; init; }

    public required FileStateSnapshot Before { get; init; }

    public required ImmutableArray<byte> IntendedBytes { get; init; }

    public GeneratedNavigationBoundedChange? RegionChange { get; init; }

    internal bool IsCreate => Before.Kind == FileExpectationKind.Missing;

    internal bool RequiresFileChange => IsCreate || RegionChange?.RequiresUpdate == true;
}
