using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemoveReferenceAbsenceResult
{
    public required RouteRemoveReferences References { get; init; }

    public RouteRemoveFinding? Finding { get; init; }
}
