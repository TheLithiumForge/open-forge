namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Interaction;

internal sealed record RouteUpdateSourceSelectionQuestion(
    string RequestedId,
    IReadOnlyList<string> CandidatePaths);
