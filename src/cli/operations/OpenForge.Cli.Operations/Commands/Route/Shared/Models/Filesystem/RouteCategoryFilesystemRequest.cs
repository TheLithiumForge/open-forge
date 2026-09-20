using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;

internal sealed record RouteCategoryFilesystemRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required string EntrypointLogicalPath { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public ImmutableArray<string> EntrypointPaths { get; init; } = [];

    public ImmutableArray<string> ExposedPaths { get; init; } = [];
}
