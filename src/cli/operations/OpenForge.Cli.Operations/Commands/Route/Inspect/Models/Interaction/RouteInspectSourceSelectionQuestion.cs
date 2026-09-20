namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Interaction;

internal sealed record RouteInspectSourceSelectionQuestion(
    string RequestedId,
    IReadOnlyList<string> CandidatePaths);
