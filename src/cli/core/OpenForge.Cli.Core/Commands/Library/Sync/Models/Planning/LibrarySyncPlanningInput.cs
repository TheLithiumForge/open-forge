using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;

internal sealed record LibrarySyncPlanningInput : ILibraryMutationObservation
{
    public required LibrarySyncRequest Request { get; init; }

    public required LibraryConsumerBoundaryFacts ConsumerBoundary { get; init; }

    public required LibrariesRecordRead Record { get; init; }

    public required LibraryInventoryRead Source { get; init; }

    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public required ImmutableArray<PlannedFileChange> GeneratedRegionChanges { get; init; }

    public LibraryGeneratedNavigationIssue? GeneratedNavigationIssue { get; init; }
}
