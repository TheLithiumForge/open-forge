namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record RouteMovePostMoveVerification(
    RouteMovePostMoveVerificationState State,
    string? Cause);
