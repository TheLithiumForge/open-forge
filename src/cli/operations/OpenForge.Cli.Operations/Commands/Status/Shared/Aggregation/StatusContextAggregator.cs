using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusContextAggregator
{
    internal static StatusContext Build(
        RouteStatusView routes,
        OperationalInstallationState installation,
        RouteOutsideRoutesObservation? outside = null)
    {
        if (outside is { RegistrationInventoryComplete: true, Routes.Count: 0 }) outside = null;
        var initial = StatusMeasurementCalculator.Project(routes.InitialStartup);
        var total = outside is null ? StatusMeasurementCalculator.Add(routes.TotalAvailable, routes.IncludedFiles?.Total)
            : StatusMeasurementCalculator.Project(outside.Total);
        var outsideRoutes = outside is { RegistrationInventoryComplete: true, Routes.Count: > 0 }
            ? outside.Routes.Select(route => new StatusOutsideRoute(route.Directory.Descriptor.DirectoryPath,
                route.Directory.State switch
                {
                    WorkspaceOutsideRouteReadState.Available => StatusOutsideRouteState.Available,
                    WorkspaceOutsideRouteReadState.Missing => StatusOutsideRouteState.Missing,
                    WorkspaceOutsideRouteReadState.Unsafe => StatusOutsideRouteState.Unsafe,
                    WorkspaceOutsideRouteReadState.Unavailable => StatusOutsideRouteState.Unavailable,
                    _ => throw new ArgumentOutOfRangeException(nameof(outside), route.Directory.State, "The outside directory state is not defined."),
                }, route.Listed)).ToArray() : null;
        var includedFiles = routes.IncludedFiles is { } included ? new StatusIncludedFiles(
            Listed: included.Files.Count(file => file.Registered && file.FileExists && file.Listed),
            Unlisted: included.Files.Count(file => file.Registered && file.FileExists && !file.Listed),
            Missing: included.Files.Count(file => file.Registered && !file.FileExists),
            HandWritten: included.HandWrittenRows,
            Startup: StatusMeasurementCalculator.Project(included.Startup),
            Pending: included.Pending) : null;
        if (installation == OperationalInstallationState.Uninstalled)
        {
            return BuildNotApplicable(initial, total) with { IncludedFiles = includedFiles, OutsideRoutes = outsideRoutes };
        }

        var current = outside is null ? StatusMeasurementCalculator.Add(routes.CurrentStartup, routes.IncludedFiles?.Startup)
            : StatusMeasurementCalculator.Project(outside.Startup);
        var continuity = outside is null ? StatusMeasurementCalculator.Add(routes.Continuity, routes.IncludedFiles?.Continuity)
            : StatusMeasurementCalculator.Project(outside.Continuity);
        return new StatusContext
        {
            IncludedFiles = includedFiles,
            OutsideRoutes = outsideRoutes,
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(
                initial,
                current,
                StatusMeasurementCalculator.Difference(current, initial)),
            TotalAvailable = total,
            StartupPercentage = StatusMeasurementCalculator.StartupPercentage(current.Utf8Bytes, total.Utf8Bytes),
            Continuity = continuity,
            ContinuitySources = outside is null ? routes.ContinuitySources
                .Concat(routes.IncludedFiles?.ContinuitySources ?? [])
                .OrderByDescending(source => source.Utf8Bytes)
                .ThenBy(source => source.SourceId, StringComparer.Ordinal)
                .Select(Project)
                .ToArray() : OutsideContributions(outside),
        };
    }

    private static StatusContinuitySource[] OutsideContributions(RouteOutsideRoutesObservation outside)
    {
        if (outside.Continuity.Files.State != OperationalValueState.Available || outside.IncludedCatalogue is not { } catalogue)
            return [];
        return (outside.Loading?.ContinuityPaths ?? []).Select(catalogue.FindByPath).OfType<SourceLogicalSource>()
            .DistinctBy(source => source.Identity.AutomaticId, StringComparer.Ordinal).Select(source =>
            {
                var layers = new[] { source.Base }.Concat(source.Overwrite is { } overwrite ? [overwrite] : [])
                    .Select(layer => new StatusContextLayer(layer.CanonicalPath,
                        outside.Layers.Single(item => PhysicalIdentityTracker.PathComparer.Equals(item.PhysicalPath, layer.PhysicalPath))
                            .Measurement.Utf8Bytes.Value ?? throw new InvalidOperationException("A complete continuity layer requires its measured bytes."))).ToArray();
                return new StatusContinuitySource { SourceId = source.Identity.AutomaticId, Layers = layers, Utf8Bytes = layers.Sum(layer => layer.Utf8Bytes) };
            }).OrderByDescending(source => source.Utf8Bytes).ThenBy(source => source.SourceId, StringComparer.Ordinal).ToArray();
    }

    internal static StatusContext Unavailable()
    {
        var measurement = Measurement(StatusValueState.Unavailable);
        return new StatusContext
        {
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(measurement, measurement, measurement),
            TotalAvailable = measurement,
            StartupPercentage = new StatusDecimalValue(StatusValueState.Unavailable, null),
            Continuity = measurement,
            ContinuitySources = [],
        };
    }

    private static StatusContext BuildNotApplicable(StatusMeasurement initial, StatusMeasurement total)
    {
        var measurement = Measurement(StatusValueState.NotApplicable);
        return new StatusContext
        {
            TokenEstimator = StatusDefinitions.TokenEstimator,
            Startup = new StatusStartupComparison(initial, measurement, measurement),
            TotalAvailable = total,
            StartupPercentage = new StatusDecimalValue(StatusValueState.NotApplicable, null),
            Continuity = measurement,
            ContinuitySources = [],
        };
    }

    private static StatusMeasurement Measurement(StatusValueState state)
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
