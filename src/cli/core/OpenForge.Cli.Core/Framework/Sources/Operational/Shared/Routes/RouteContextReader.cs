using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteContextReader
{
    internal RouteContextObservation Read(
        FrameworkPayloadReadResult payload,
        RouteSourceInspection inspection)
    {
        var initial = new RouteInitialContextReader().Read(
            inspection.Catalogue.Workspace,
            payload);
        var current = RouteCurrentContextReader.Read(inspection);
        var initialClosure = initial is null
            ? null
            : new RouteContextClosureResolver().Resolve(initial);
        var currentClosure = new RouteContextClosureResolver().Resolve(current);
        var safelyAbsentSourceInventory = RouteSourceInspectionPolicy.IsSafelyAbsentSourceInventory(
            inspection.Catalogue);
        var totalInventory = SelectTotalInventory(current, inspection);
        var initialMeasurement = initialClosure is null
            ? RouteContextMeasurementReader.Unavailable()
            : RouteContextMeasurementReader.Measure(
                initialClosure.Startup,
                initialClosure.IsComplete);
        var currentMeasurement = RouteContextMeasurementReader.Measure(
            currentClosure.Startup,
            currentClosure.IsComplete);
        var totalMeasurement = RouteContextMeasurementReader.Measure(
            totalInventory.Sources,
            totalInventory.IsComplete);
        var continuityMeasurement = RouteContextMeasurementReader.Measure(
            currentClosure.Continuity,
            currentClosure.IsComplete);
        return new RouteContextObservation
        {
            InitialStartup = initialMeasurement,
            CurrentStartup = currentMeasurement,
            TotalAvailable = totalMeasurement,
            Continuity = continuityMeasurement,
            ContinuitySources = RouteContextMeasurementReader.Contributions(currentClosure),
            InitialRootCategories = initial?.RootCategories ?? [],
            CurrentRootCategories = current.RootCategories,
            IsIncomplete = RouteContextMeasurementReader.IsUnavailable(initialMeasurement)
                || RouteContextMeasurementReader.IsUnavailable(totalMeasurement)
                || !safelyAbsentSourceInventory
                    && (RouteContextMeasurementReader.IsUnavailable(currentMeasurement)
                        || RouteContextMeasurementReader.IsUnavailable(continuityMeasurement)),
        };
    }

    private static (IReadOnlyList<RouteContextSource> Sources, bool IsComplete)
        SelectTotalInventory(
            RouteContextSet current,
            RouteSourceInspection inspection)
    {
        var sourcesComplete = RouteSourceInspectionPolicy.IsCatalogueComplete(
                inspection.Catalogue)
            && inspection.Sources.SelectMany(source => source.Layers)
                .All(layer => layer.Text is not null);
        switch (inspection.WorkspaceEntry.State)
        {
            case FileReadState.Missing:
                return (current.Sources, sourcesComplete);
            case FileReadState.Complete when inspection.WorkspaceEntry.Text is not null:
                return ([current.WorkspaceEntry, .. current.Sources], sourcesComplete);
            case FileReadState.Complete:
            case FileReadState.InvalidEncoding:
            case FileReadState.InvalidSyntax:
            case FileReadState.AccessDenied:
            case FileReadState.InputOutputFailure:
            case FileReadState.Cancelled:
                return (current.Sources, false);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(inspection),
                    inspection.WorkspaceEntry.State,
                    "The workspace-entry read state is not defined.");
        }
    }
}
