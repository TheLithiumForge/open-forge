using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Application;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;

internal sealed record LibrarySyncPayload
{
    public required LibraryMutationIdentity Identity { get; init; }

    public required LibraryMutationRecord Record { get; init; }

    public required LibraryMutationSource Source { get; init; }

    public required LibraryMutationProjection Projection { get; init; }

    public required LibraryMutationPlanView Plan { get; init; }

    public required LibraryMutationApplication Application { get; init; }

    public required LibrarySyncFinding[] Findings { get; init; }
}
