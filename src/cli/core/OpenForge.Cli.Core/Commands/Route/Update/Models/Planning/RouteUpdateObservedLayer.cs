using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateObservedLayer
{
    public required string Text { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed class RouteUpdateObservedLayerBuild
{
    private RouteUpdateObservedLayerBuild(
        RouteUpdateObservedLayer? layer,
        RouteUpdateFindingCode? failureCode,
        string? failureCause,
        bool isIncomplete)
    {
        var complete = layer is not null
            && failureCode is null
            && failureCause is null;
        var stopped = layer is null
            && failureCode is not null
            && failureCause is not null;
        if (!complete && !stopped)
        {
            throw new ArgumentException(
                "A layer observation requires either complete facts or one failure.");
        }

        Layer = layer;
        FailureCode = failureCode;
        FailureCause = failureCause;
        IsIncomplete = isIncomplete;
    }

    internal RouteUpdateObservedLayer? Layer { get; }

    internal RouteUpdateFindingCode? FailureCode { get; }

    internal string? FailureCause { get; }

    internal bool IsIncomplete { get; }

    internal static RouteUpdateObservedLayerBuild Complete(
        string text,
        FileStateSnapshot snapshot)
        => new(
            new RouteUpdateObservedLayer
            {
                Text = text,
                Snapshot = snapshot,
            },
            failureCode: null,
            failureCause: null,
            isIncomplete: false);

    internal static RouteUpdateObservedLayerBuild Stop(
        RouteUpdateFindingCode code,
        string cause,
        bool isIncomplete)
        => new(
            layer: null,
            code,
            cause,
            isIncomplete);
}
