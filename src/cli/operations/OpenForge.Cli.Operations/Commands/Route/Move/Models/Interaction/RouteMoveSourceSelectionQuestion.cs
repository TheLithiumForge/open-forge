namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Interaction;

internal sealed record RouteMoveSourceSelectionQuestion(
    string RequestedId,
    IReadOnlyList<string> CandidatePaths);
