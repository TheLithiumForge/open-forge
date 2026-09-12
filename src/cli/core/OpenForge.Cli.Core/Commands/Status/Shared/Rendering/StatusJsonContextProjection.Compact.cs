using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static partial class StatusJsonContextProjection
{
    internal static StatusCompactJsonContext CreateCompact(StatusContext context)
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
        };
}
