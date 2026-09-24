using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;

internal sealed record LibraryAttachPlanningInput : ILibraryMutationObservation
{
    public required LibraryAttachRequest Request { get; init; }

    public required LibraryConsumerBoundaryFacts ConsumerBoundary { get; init; }

    public required LibraryRegistrationRead Record { get; init; }

    public required LibraryInventoryRead Source { get; init; }

    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }


    public required WorkspaceOwnershipRead Ownership { get; init; }

    public required WorkspaceSettingsRead Settings { get; init; }

    public required ImmutableArray<PlannedFileChange> GeneratedRegionChanges { get; init; }

    public LibraryGeneratedNavigationIssue? GeneratedNavigationIssue { get; init; }
}
