using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal interface ILibraryMutationObservation
{
    LibraryRegistrationRead Record { get; }
    LibraryInventoryRead? Source { get; }
    ImmutableArray<LibraryMappingObservation> Mappings { get; }
    WorkspaceOwnershipRead Ownership { get; }
}
