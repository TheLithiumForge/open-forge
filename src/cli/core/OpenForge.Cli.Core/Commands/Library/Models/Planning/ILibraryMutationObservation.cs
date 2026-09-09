using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;

namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal interface ILibraryMutationObservation
{
    LibrariesRecordRead Record { get; }
    LibraryInventoryRead? Source { get; }
    ImmutableArray<LibraryMappingObservation> Mappings { get; }
    LifecycleOwnershipReadResult Ownership { get; }
}
