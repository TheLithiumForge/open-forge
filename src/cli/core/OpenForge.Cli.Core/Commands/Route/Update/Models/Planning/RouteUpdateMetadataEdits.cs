using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateMetadataEdit
{
    public required int YamlStart { get; init; }
    public required int YamlLength { get; init; }
    public required string Replacement { get; init; }
}

internal sealed record RouteUpdateMetadataEditPlan
{
    public required ImmutableArray<RouteUpdateMetadataEdit> Edits { get; init; }
    public required ImmutableArray<RouteUpdatePreviewHunk> Preview { get; init; }
}

internal sealed class RouteUpdateMetadataEditDraft(RouteUpdateMetadataLayout layout)
{
    internal RouteUpdateMetadataLayout Layout { get; } = layout;

    internal ImmutableArray<RouteUpdateMetadataEdit>.Builder Edits { get; } =
        ImmutableArray.CreateBuilder<RouteUpdateMetadataEdit>();

    internal ImmutableArray<RouteUpdatePreviewHunk>.Builder Preview { get; } =
        ImmutableArray.CreateBuilder<RouteUpdatePreviewHunk>();

    internal RouteUpdateMetadataEditPlan Build()
        => new()
        {
            Edits = Edits.ToImmutable(),
            Preview = Preview.ToImmutable(),
        };
}

internal sealed class RouteUpdateMetadataEditPlanBuild
{
    private RouteUpdateMetadataEditPlanBuild(
        RouteUpdateMetadataEditPlan? plan,
        string? cause)
    {
        if ((plan is null) == (cause is null))
        {
            throw new ArgumentException(
                "A metadata edit build requires either one plan or one unsafe cause.");
        }

        Plan = plan;
        Cause = cause;
    }

    public RouteUpdateMetadataEditPlan? Plan { get; }
    public string? Cause { get; }

    internal static RouteUpdateMetadataEditPlanBuild Complete(RouteUpdateMetadataEditPlan plan)
        => new(plan, cause: null);

    internal static RouteUpdateMetadataEditPlanBuild Unsafe(string cause)
        => new(plan: null, cause);
}

internal sealed record RouteUpdateMetadataByteEditInput
{
    public required RouteUpdateObservation Observation { get; init; }
    public required ImmutableArray<RouteUpdateMetadataEdit> Edits { get; init; }
}
