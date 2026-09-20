namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Interaction;

internal sealed record RouteRemoveSourceSelectionQuestion(
    string RequestedId,
    IReadOnlyList<string> CandidatePaths);
