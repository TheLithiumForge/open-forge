using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Application;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;

internal sealed record LibraryDetachPayload
{

    public required LibraryMutationIdentity Identity { get; init; }

    public required LibraryMutationRecord Record { get; init; }

    public required LibraryMutationProjection Projection { get; init; }

    public required LibraryMutationPlanView Plan { get; init; }

    public required LibraryPermissionView Permissions { get; init; }

    public required LibraryMutationApplication Application { get; init; }

    public required LibraryDetachFinding[] Findings { get; init; }
}
