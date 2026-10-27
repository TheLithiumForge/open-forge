using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;

internal sealed record RoutedMaintenanceRequest
{
    public required SourceCatalogue Catalogue { get; init; }
    public required ImmutableArray<string> RootPaths { get; init; }
    public required ImmutableArray<SourceLogicalSource> SelectedSources { get; init; }
    public ImmutableArray<string>? SelectedSourcePaths { get; init; }
    public required RoutedMetadataPolicyContext Policy { get; init; }
    public IReadOnlyDictionary<string, ImmutableArray<byte>> IntendedBytes { get; init; } = new Dictionary<string, ImmutableArray<byte>>();
    public IReadOnlyDictionary<string, MarkdownDocumentFacts> ObservedDocuments { get; init; } = new Dictionary<string, MarkdownDocumentFacts>();
}

internal sealed record RoutedMaintenanceMetadata(string Path, FrameworkMetadataCompletionResult Completion);
internal sealed record RoutedMaintenanceLimitation(string Path, string Cause, bool Malformed)
{
    internal bool Topology { get; init; }
}

internal sealed record RoutedMaintenanceIntent
{
    public required GeneratedNavigationFormation Formation { get; init; }
    public required IReadOnlyDictionary<string, MarkdownDocumentFacts> Documents { get; init; }
    public required IReadOnlyDictionary<string, FileStateSnapshot> Originals { get; init; }
    public IReadOnlyDictionary<string, ImmutableArray<byte>> Replacements { get; init; } = new Dictionary<string, ImmutableArray<byte>>();
    public required ImmutableArray<RoutedMaintenanceMetadata> Metadata { get; init; }
    public required ImmutableArray<RouteScaffoldEntrypointIntent> Entrypoints { get; init; }
    public required ImmutableArray<RoutedMaintenanceLimitation> Limitations { get; init; }
    public required ImmutableArray<RouteScaffoldOldNavigation> OldNavigation { get; init; }
    public required ImmutableArray<string> RemovedPaths { get; init; }
    public ImmutableArray<RouteScaffoldRenameProof> RenameProofs { get; init; } = [];
}
