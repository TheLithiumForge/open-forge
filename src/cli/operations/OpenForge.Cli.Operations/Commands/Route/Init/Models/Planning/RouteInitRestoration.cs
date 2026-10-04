using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal sealed record RouteInitRestorationFile(
    FrameworkPayloadAsset Asset,
    FileStateSnapshot Before,
    SourceLogicalSource? Source);

internal sealed record RouteInitRestoration
{
    internal required WorkspaceSettingsRead Settings { get; init; }

    internal PlannedFileChange? SettingsChange { get; init; }

    internal required ImmutableArray<RouteInitRestorationFile> Files { get; init; }

    internal ImmutableArray<FileStateSnapshot> CompanionSnapshots { get; init; } = [];
}
