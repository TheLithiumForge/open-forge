using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallWorkspaceAdoptionSourceInput
{
    public required CliWorkspace Workspace { get; init; }

    public required IReadOnlyList<FrameworkPayloadAsset> SelectedPayloadAssets { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required WorkspaceAdoptionTopologyPlan Topology { get; init; }

    public required bool IsFirstInstall { get; init; }
}

internal sealed record InstallWorkspaceAdoptionSourcePlan
{
    public IReadOnlySet<string> PreservedEntrypointPaths { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    public required IReadOnlyList<FrameworkPayloadAsset> PayloadAssets { get; init; }

    public required IReadOnlyList<SourceLogicalSource> PayloadSources { get; init; }

    public required IReadOnlyList<SourceLogicalSource> IntendedSources { get; init; }

    public required IReadOnlySet<string> PayloadSourcePaths { get; init; }

    public required IReadOnlySet<string> CreatedEntrypointPaths { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record InstallWorkspaceAdoptionDocumentInput
{
    public required CliWorkspace Workspace { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required WorkspaceAdoptionTopologyPlan Topology { get; init; }

    public required IReadOnlyList<SourceLogicalSource> IntendedSources { get; init; }

    public required IReadOnlySet<string> PayloadSourcePaths { get; init; }

    public required IReadOnlySet<string> CreatedEntrypointPaths { get; init; }

    public required IReadOnlyDictionary<string, string> ObservedDocuments { get; init; }

    public required IReadOnlyDictionary<string, byte[]> ObservedBytes { get; init; }

    public required IReadOnlyDictionary<string, byte[]> PreservedEntrypointBytes { get; init; }

    public required IReadOnlyDictionary<string, byte[]> ExistingOverwriteBytes { get; init; }
}

internal sealed record InstallWorkspaceAdoptionDocumentPlan
{
    public required IReadOnlyDictionary<string, string> Documents { get; init; }

    public required IReadOnlyDictionary<string, byte[]> UserTargetBytes { get; init; }

    public required IReadOnlySet<string> UserOwnedPaths { get; init; }

    public required IReadOnlyList<InstallMigrationPlan> Migrations { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record InstallWorkspaceAdoptionLocalSource
{
    public required SourceLogicalSource? Source { get; init; }

    public required string? Cause { get; init; }
}
