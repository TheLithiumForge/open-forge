using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;

internal sealed record RouteUpdateMetadataEdit
{
    public required int DocumentStart { get; init; }
    public required int DocumentLength { get; init; }
    public int InsertionOrder { get; init; }
    public required string Replacement { get; init; }
}

internal sealed record RouteUpdateMetadataEditPlan
{
    public required ImmutableArray<RouteUpdateMetadataEdit> Edits { get; init; }
    public required ImmutableArray<RouteUpdatePreviewHunk> Preview { get; init; }
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

internal sealed record RouteUpdateMetadataByteEdit(int Start, int Length, byte[] Replacement);
