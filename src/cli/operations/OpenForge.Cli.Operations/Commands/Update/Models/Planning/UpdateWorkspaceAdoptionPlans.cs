using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdateWorkspaceAdoptionSourcePlan(
    IReadOnlyList<UpdatePayloadTarget> PayloadAssets,
    IReadOnlyList<SourceLogicalSource> PayloadSources,
    IReadOnlyList<SourceLogicalSource> IntendedSources,
    IReadOnlySet<string> PayloadSourcePaths,
    IReadOnlySet<string> AdoptionSourcePaths,
    IReadOnlySet<string> CreatedEntrypointPaths,
    WorkspaceAdoptionTopologyPlan Topology,
    string? Cause);

internal sealed record UpdateWorkspaceAdoptionDocumentBuild(
    IReadOnlyDictionary<string, string> Documents,
    IReadOnlyDictionary<string, byte[]> TargetBytes,
    IReadOnlyDictionary<string, FileStateSnapshot> OriginalSnapshots,
    IReadOnlyList<UpdateMigrationPlan> Migrations,
    string? Cause);

internal sealed record UpdateWorkspaceAdoptionLocalSource(
    SourceLogicalSource? Source,
    string? Cause);

