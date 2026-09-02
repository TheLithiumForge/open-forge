using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateObservationRequest
{
    public required RouteUpdateRequest Request { get; init; }
}

internal sealed record RouteUpdateObservation
{
    public required RouteUpdateRequest Request { get; init; }

    public required RouteUpdateTarget Target { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceLogicalSource TargetSource { get; init; }

    public required FileStateSnapshot TargetSnapshot { get; init; }

    public required FileStateSnapshot? OverwriteSnapshot { get; init; }

    public required string TargetText { get; init; }

    public required MarkdownDocumentFacts Markdown { get; init; }

    public required YamlDocumentFacts Frontmatter { get; init; }
}

internal sealed record RouteUpdatePlanningBoundary
{
    public required RouteUpdateResultFormation Formation { get; init; }
}

internal sealed class RouteUpdateObservationBuild
{
    private RouteUpdateObservationBuild(
        RouteUpdateObservation? observation,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((observation is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Update observation must contain exactly one observation or boundary.");
        }

        Observation = observation;
        Boundary = boundary;
    }

    internal RouteUpdateObservation? Observation { get; }

    internal RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateObservationBuild Complete(
        RouteUpdateObservation observation)
        => new(observation: observation, boundary: null);

    internal static RouteUpdateObservationBuild Stop(
        RouteUpdatePlanningBoundary boundary)
        => new(observation: null, boundary: boundary);
}
