using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Observation;

internal sealed record WorkspaceOutsideRouteRead
{
    public required WorkspaceOutsideRoute Descriptor { get; init; }
    public required WorkspaceOutsideRouteReadState State { get; init; }
    public required string? PhysicalDirectory { get; init; }
    public required NoFollowLeafObservation DirectoryObservation { get; init; }
    public required PhysicalPathResolution DirectoryResolution { get; init; }
    public NoFollowLeafObservation? CatalogueObservation { get; init; }
    public PhysicalPathResolution? CatalogueResolution { get; init; }
    public FileStateSnapshot? CatalogueSnapshot { get; init; }
    public FilesystemFailure? Failure { get; init; }
    public ImmutableArray<WorkspaceLayoutIssue> Issues { get; init; } = [];
}
