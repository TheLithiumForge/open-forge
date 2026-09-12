using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;

internal sealed record LibraryDetachPlanningInput : ILibraryMutationObservation
{
    public required LibraryDetachRequest Request { get; init; }

    public required LibraryConsumerBoundaryFacts ConsumerBoundary { get; init; }

    public required LibrariesRecordRead Record { get; init; }

    LibraryInventoryRead? ILibraryMutationObservation.Source => null;

    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public required ImmutableArray<PlannedFileChange> GeneratedRegionChanges { get; init; }

    public LibraryGeneratedNavigationIssue? GeneratedNavigationIssue { get; init; }
}
