using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using StatusJsonContextModel = OpenForge.Cli.Core.Commands.Status.Models.Presentation.StatusJsonContext;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonContextProjection
{
    internal static StatusJsonContextModel Create(StatusContext context)
        => new()
        {
            TokenEstimator = context.TokenEstimator,
            Startup = new StatusJsonStartupComparison
            {
                Initial = Measurement(context.Startup.Initial),
                Current = Measurement(context.Startup.Current),
                Difference = Measurement(context.Startup.Difference),
            },
            TotalAvailable = Measurement(context.TotalAvailable),
            StartupPercentage = new StatusJsonDecimalValue
            {
                State = StatusWireVocabulary.ValueState(context.StartupPercentage.State),
                Value = context.StartupPercentage.Value,
            },
            Continuity = Measurement(context.Continuity),
            ContinuitySources = context.ContinuitySources.Select(source => new StatusJsonContinuitySource
            {
                SourceId = source.SourceId,
                Utf8Bytes = source.Utf8Bytes,
                Layers = source.Layers.Select(layer => new StatusJsonContextLayer
                {
                    Path = layer.Path,
                    Utf8Bytes = layer.Utf8Bytes,
                }).ToArray(),
            }).ToArray(),
        };

    internal static StatusJsonIntegerValue Value(StatusIntegerValue value)
        => new()
        {
            State = StatusWireVocabulary.ValueState(value.State),
            Value = value.Value,
        };

    private static StatusJsonMeasurement Measurement(StatusMeasurement measurement)
        => new()
        {
            Files = Value(measurement.Files),
            Characters = Value(measurement.Characters),
            Utf8Bytes = Value(measurement.Utf8Bytes),
            EstimatedTokens = Value(measurement.EstimatedTokens),
        };
}
