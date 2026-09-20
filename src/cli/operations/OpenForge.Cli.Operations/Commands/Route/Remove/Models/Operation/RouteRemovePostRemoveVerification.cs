namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal sealed record RouteRemovePostRemoveVerification(
    RouteRemovePostRemoveVerificationState State,
    string? Cause);
