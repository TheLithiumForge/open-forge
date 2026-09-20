using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationProjection
{
    public required LibraryPlanState State { get; init; }

    public required LibraryMutationMapping[] Mappings { get; init; }

    public required LibraryCollision[] Collisions { get; init; }
    public required LibraryOwnershipObservation[] Ownership { get; init; }

}
