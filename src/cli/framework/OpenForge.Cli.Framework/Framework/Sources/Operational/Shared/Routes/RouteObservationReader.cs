using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteObservationReader
{
    internal async ValueTask<RouteOutsideRoutesObservation> ReadOutsideAsync(
        WorkspaceLayoutRead observation, CancellationToken cancellationToken)
    {
        if (observation.Layout is null) return RouteOutsideRouteReader.Read(observation, null, null);
        var inspection = await _sourceInspector.ReadAsync(RouteOutsideRouteReader.InventoryLayout(observation), cancellationToken)
            .ConfigureAwait(false);
        var session = inspection.Session ?? throw new InvalidOperationException("Outside observations require the retained inspection session.");
        var rows = await new SourceIncludedEntryReader().ReadEntriesAsync(inspection.Catalogue, session.DocumentReader, cancellationToken)
            .ConfigureAwait(false);
        var catalogue = SourceIncludedEntryReader.Extend(inspection.Catalogue, rows.Sources, rows.Rows);
        var included = await new RouteIncludedFileReader(inspection.Catalogue.Layout)
            .ReadAsync(inspection, observation.Settings, cancellationToken, rows).ConfigureAwait(false);
        var loading = await RouteOutsideRouteReader.ReadLoadingAsync(inspection, catalogue, cancellationToken).ConfigureAwait(false);
        return RouteOutsideRouteReader.Read(observation, inspection, included, catalogue, loading);
    }

    private readonly RouteGeneratedNavigationReader _generatedNavigationReader;
    private readonly RouteDoctorGeneratedNavigationReader _doctorGeneratedNavigationReader;
    private readonly RouteSourceInspector _sourceInspector;

    internal async ValueTask<RouteDoctorView> ReadDoctorAsync(CliWorkspace workspace, SourceSharing? sharing, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadDoctorAsync(observation, sharing, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<RouteStatusView> ReadStatusAsync(CliWorkspace workspace, SourceSharing? sharing, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadStatusAsync(observation, sharing, cancellationToken).ConfigureAwait(false);
    }

    internal RouteObservationReader(
        RouteSourceInspector sourceInspector,
        RouteGeneratedNavigationReader generatedNavigationReader,
        RouteDoctorGeneratedNavigationReader doctorGeneratedNavigationReader)
    {
        _sourceInspector = sourceInspector;
        _generatedNavigationReader = generatedNavigationReader;
        _doctorGeneratedNavigationReader = doctorGeneratedNavigationReader;
    }

    internal async ValueTask<RouteStatusView> ReadStatusAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken,
        RouteOutsideRoutesObservation? retained = null)
    {
        if (retained is not null && !ReferenceEquals(retained.LayoutRead, observation))
            throw new ArgumentException("Operational consumers must retain the same layout observation.", nameof(retained));
        if (observation.Layout is null) return RouteUnavailableObservation.Status(observation);
        var payload = EmbeddedFrameworkPayloadReader.Read();
        var inspection = retained?.Inspection ?? await _sourceInspector.ReadAsync(observation.Layout, cancellationToken)
            .ConfigureAwait(false);
        var context = new RouteContextReader().Read(payload, inspection);
        var included = retained is null
            ? await new RouteIncludedFileReader(inspection.Catalogue.Layout).ReadAsync(inspection, observation.Settings, cancellationToken).ConfigureAwait(false)
            : retained.IncludedFiles;
        var generated = _generatedNavigationReader.Read(inspection.Catalogue.Workspace, payload, inspection, sharing, observation.Settings.Document);
        var state = ReadState(inspection.State, context.IsIncomplete);
        var contextIssue = ReadContextIssue(inspection);
        return new RouteStatusView
        {
            IncludedFiles = included,
            State = state,
            SourceInventory = ReadSourceInventory(inspection),
            InitialStartup = context.InitialStartup,
            CurrentStartup = context.CurrentStartup,
            TotalAvailable = context.TotalAvailable,
            Continuity = context.Continuity,
            ContinuitySources = context.ContinuitySources,
            InitialRootCategories = context.InitialRootCategories,
            CurrentRootCategories = context.CurrentRootCategories,
            GeneratedNavigation = generated,
            ContextUnavailablePath = contextIssue?.Path,
            ContextUnavailableCause = contextIssue?.Cause,
        };
    }

    internal async ValueTask<RouteDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        SourceSharing? sharing,
        CancellationToken cancellationToken,
        RouteOutsideRoutesObservation? retained = null)
    {
        if (retained is not null && !ReferenceEquals(retained.LayoutRead, observation))
            throw new ArgumentException("Operational consumers must retain the same layout observation.", nameof(retained));
        if (observation.Layout is null) return RouteUnavailableObservation.Doctor(observation);
        var inspection = retained?.Inspection ?? await _sourceInspector.ReadAsync(observation.Layout, cancellationToken)
            .ConfigureAwait(false);
        var generated = _doctorGeneratedNavigationReader.Read(inspection, sharing);
        var included = retained is null
            ? await new RouteIncludedFileReader(inspection.Catalogue.Layout).ReadAsync(inspection, observation.Settings, cancellationToken).ConfigureAwait(false)
            : retained.IncludedFiles;
        return new RouteDoctorView
        {
            IncludedFiles = included,
            Session = inspection.Session,
            State = inspection.State,
            SourceInventory = ReadSourceInventory(inspection),
            Catalogue = inspection.Catalogue,
            Routes = inspection.Routes,
            Metadata = RouteDoctorFactReader.ReadMetadata(inspection),
            WorkspaceEntry = inspection.WorkspaceEntry,
            Sources = inspection.Sources,
            GeneratedNavigation = generated,
            DeclaredRoots = RouteDoctorFactReader.ReadDeclaredRoots(inspection.Catalogue.Layout, inspection.Routes),
            Shape = RouteDoctorFactReader.ReadShape(inspection),
        };
    }

    private static OperationalViewState ReadState(
        OperationalViewState inspection,
        bool contextIncomplete)
    {
        if (!contextIncomplete
            || inspection is OperationalViewState.Blocked or OperationalViewState.Interrupted)
        {
            return inspection;
        }

        return OperationalViewState.Incomplete;
    }

    private static RouteSourceInventoryState ReadSourceInventory(
        RouteSourceInspection inspection)
    {
        if (inspection.State == OperationalViewState.Interrupted)
        {
            return RouteSourceInventoryState.Interrupted;
        }

        if (inspection.State == OperationalViewState.Blocked)
        {
            return RouteSourceInventoryState.Blocked;
        }

        if (RouteSourceInspectionPolicy.IsSafelyAbsentSourceInventory(
                inspection.Catalogue))
        {
            return RouteSourceInventoryState.SafelyAbsent;
        }

        return RouteSourceInspectionPolicy.IsCatalogueComplete(inspection.Catalogue)
            ? RouteSourceInventoryState.Present
            : RouteSourceInventoryState.Incomplete;
    }

    private static (string Path, string? Cause)? ReadContextIssue(
        RouteSourceInspection inspection)
    {
        var issue = new[] { inspection.WorkspaceEntry }
            .Concat(inspection.Sources.SelectMany(source => source.Layers))
            .Where(layer => layer.State != FileReadState.Complete)
            .OrderBy(layer => layer.Path, StringComparer.Ordinal)
            .FirstOrDefault();
        return issue is null ? null : (issue.Path, issue.Cause);
    }
}
