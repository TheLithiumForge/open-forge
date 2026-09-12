using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusContextAggregator
{
    internal static StatusContext Build(
        RouteStatusView routes,
        OperationalInstallationState installation)
    {
        var initial = StatusMeasurementCalculator.Project(routes.InitialStartup);
        var total = StatusMeasurementCalculator.Project(routes.TotalAvailable);
        if (installation == OperationalInstallationState.Uninstalled)
        {
            return BuildNotApplicable(initial, total);
        }

        var current = StatusMeasurementCalculator.Project(routes.CurrentStartup);
        var continuity = StatusMeasurementCalculator.Project(routes.Continuity);
        return new StatusContext
        {
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(
                initial,
                current,
                StatusMeasurementCalculator.Difference(current, initial)),
            TotalAvailable = total,
            StartupPercentage = StatusMeasurementCalculator.StartupPercentage(current.Utf8Bytes, total.Utf8Bytes),
            Continuity = continuity,
            ContinuitySources = routes.ContinuitySources
                .OrderByDescending(source => source.Utf8Bytes)
                .ThenBy(source => source.SourceId, StringComparer.Ordinal)
                .Select(Project)
                .ToArray(),
        };
    }

    internal static StatusContext Unavailable()
    {
        var measurement = Measurement(OperationalValueState.Unavailable);
        return new StatusContext
        {
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(measurement, measurement, measurement),
            TotalAvailable = measurement,
            StartupPercentage = new StatusDecimalValue(OperationalValueState.Unavailable, null),
            Continuity = measurement,
            ContinuitySources = [],
        };
    }

    private static StatusContext BuildNotApplicable(StatusMeasurement initial, StatusMeasurement total)
    {
        var measurement = Measurement(OperationalValueState.NotApplicable);
        return new StatusContext
        {
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(initial, measurement, measurement),
            TotalAvailable = total,
            StartupPercentage = new StatusDecimalValue(OperationalValueState.NotApplicable, null),
            Continuity = measurement,
            ContinuitySources = [],
        };
    }

    private static StatusMeasurement Measurement(OperationalValueState state)
    {
        var value = new StatusIntegerValue(state, null);
        return new StatusMeasurement(value, value, value, value);
    }

    private static StatusContinuitySource Project(ContextSourceContributionObservation source)
    {
        return new StatusContinuitySource
        {
            SourceId = source.SourceId,
            Utf8Bytes = source.Utf8Bytes,
            Layers = source.Layers
                .Select(layer => new StatusContextLayer(layer.Path, layer.Utf8Bytes))
                .ToArray(),
        };
    }
}
