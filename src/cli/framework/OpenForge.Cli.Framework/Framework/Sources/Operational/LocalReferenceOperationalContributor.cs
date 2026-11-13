using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface ILocalReferenceOperationalContributor
{
    ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        CancellationToken cancellationToken,
        RouteDoctorView? routes = null);
}

internal sealed class LocalReferenceOperationalContributor(
    RouteSourceInspector sourceInspector,
    SourceLinkDestinationResolver destinationResolver,
    LocalReferenceCandidateReader candidateReader,
    SourceLinkHistoryRead? historyRead = null) : ILocalReferenceOperationalContributor
{
    internal async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(CliWorkspace workspace, CancellationToken cancellationToken, RouteDoctorView? routes = null)
    {
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return await ReadDoctorAsync(observation, cancellationToken, routes).ConfigureAwait(false);
    }

    internal const int MaximumConcurrentSources = 8;
    internal async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        CancellationToken cancellationToken,
        RouteDoctorView? routes = null)
    {
        if (observation.Layout is not { } layout)
            return new(OperationalViewState.Incomplete, [], LocalReferenceCandidateScanState.Incomplete, [], [])
            { UnavailableCause = observation.Issues.FirstOrDefault()?.Cause };
        var paths = new PhysicalPathResolver().CreateReadSession();
        var settings = observation.Settings;
        var inspection = routes?.Catalogue is null || routes.WorkspaceEntry is null
            ? await sourceInspector.ReadAsync(layout, cancellationToken).ConfigureAwait(false)
            : new RouteSourceInspection
            {
                Session = routes.Session,
                Catalogue = routes.Catalogue,
                Routes = routes.Routes,
                WorkspaceEntry = routes.WorkspaceEntry,
                Sources = routes.Sources,
                State = routes.State,
            };
        var resolver = inspection.Session is { } session
            ? destinationResolver.CreateSession(session) : destinationResolver.CreateSession(inspection.Catalogue);
        foreach (var source in inspection.Sources)
        {
            if (source.Document is { } document
                && source.Layers.Any(layer => layer.Path == source.Source.Base.CanonicalPath && layer.Text == document.Source))
                resolver.RetainSource(source.Source.Base, document);
        }
        var acquired = new LocalReferenceReadAccumulator[inspection.Sources.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, acquired.Length), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentSources,
            CancellationToken = CancellationToken.None,
        }, async (index, token) =>
        {
            var sourceObservations = new LocalReferenceReadAccumulator(settings.Document);
            await ReadSourceAsync(
                inspection,
                inspection.Sources[index],
                sourceObservations,
                resolver,
                cancellationToken).ConfigureAwait(false);
            acquired[index] = sourceObservations;
        }).ConfigureAwait(false);
        var observations = new LocalReferenceReadAccumulator(settings.Document);
        foreach (var source in acquired)
        {
            observations.References.AddRange(source.References);
            observations.Sources.AddRange(source.Sources);
            observations.IgnoredLinkCount += source.IgnoredLinkCount;
            foreach (var document in source.Documents) observations.Documents[document.Key] = document.Value;
        }

        var readSession = inspection.Session ?? new SourceReadSession(inspection.Catalogue,
            new SourceDocumentReader(inspection.Catalogue.Workspace, paths), []);
        var retainedText = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var layer in inspection.Sources.SelectMany(source => source.Layers).Append(inspection.WorkspaceEntry))
        {
            if (layer.Text is { } text) retainedText.TryAdd(layer.Path, text);
        }
        var comments = await SourceCommentSourceReader.ReadAsync(readSession, cancellationToken, retainedText).ConfigureAwait(false);
        var commentSession = SourceCommentSourceReader.Extend(readSession, comments);
        foreach (var source in comments.Sources)
        {
            var path = source.Base.CanonicalPath;
            var document = comments.Documents[path];
            var locations = new Utf8SourceMap(document.Source);
            observations.Documents[path] = document;
            observations.Sources.Add(new LocalReferenceSourceObservation(path, source.Identity.AutomaticId,
                [new LocalReferenceParsedLayer(path, document, locations)]));
            await LocalReferenceOccurrenceReader.ReadAsync(new(commentSession.Catalogue, path, path, document, locations),
                observations, resolver, cancellationToken).ConfigureAwait(false);
        }

        var history = historyRead is null ? new SourceLinkHistory([])
            : await historyRead(new(inspection.Catalogue.Workspace, observations.References), cancellationToken).ConfigureAwait(false);
        var fragments = observations.References.GroupBy(reference => (reference.SourcePath, reference.Destination))
            .ToDictionary(group => group.Key, group => group.First().Facts.Fragment);
        var candidates = candidateReader.Read(
            observations.References,
            observations.Sources,
            inspection.Routes.Topology).Concat(SourceLinkSuggestionReader.Read(observations.References, observations.Sources, history, comments.TextFiles))
            .GroupBy(candidate => (candidate.SourcePath, candidate.Destination, candidate.Candidate.Path,
                Fragment: candidate.Fragment ?? fragments[(candidate.SourcePath, candidate.Destination)]))
            .Select(group => LocalReferenceCandidateObservation.Create(group.Key.SourcePath, group.Key.Destination,
                group.First().Candidate, group.SelectMany(candidate => candidate.Bases).Distinct().OrderBy(basis => basis.Priority).ToArray())
                with
            { Fragment = group.Key.Fragment })
            .OrderBy(candidate => candidate.Bases.Min(basis => basis.Priority)).ThenBy(candidate => candidate.Candidate.Path, StringComparer.Ordinal).ToArray();
        return new LocalReferenceDoctorView(
            !comments.Complete || settings.State is WorkspaceSettingsReadState.Invalid or WorkspaceSettingsReadState.Unavailable
                ? OperationalViewState.Incomplete : inspection.State,
            observations.References.ToArray(),
            !comments.Complete || history.TimedOut ? LocalReferenceCandidateScanState.Incomplete : ReadCandidateState(inspection.State),
            candidates,
            LocalReferenceGraphReader.Read(observations.References))
        {
            Catalogue = inspection.Catalogue,
            Documents = observations.Documents,
            IgnoredLinkCount = observations.IgnoredLinkCount,
            UnrecognizedSettingsSchemaVersion = settings.State == WorkspaceSettingsReadState.Complete && !settings.Document.IsKnownSchemaVersion
                ? settings.Document.SchemaVersion : null,
            MissingLinkScopeEntries = ReadMissingEntries(settings.Document, inspection.Catalogue, paths, cancellationToken),
        };
    }

    ValueTask<LocalReferenceDoctorView> ILocalReferenceOperationalContributor.ReadDoctorAsync(
        WorkspaceLayoutRead observation,
        CancellationToken cancellationToken,
        RouteDoctorView? routes)
        => ReadDoctorAsync(observation, cancellationToken, routes);

    private async ValueTask ReadSourceAsync(
        RouteSourceInspection inspection,
        RouteSourceObservation source,
        LocalReferenceReadAccumulator observations,
        SourceLinkDestinationResolver resolver,
        CancellationToken cancellationToken)
    {
        var parsedLayers = new List<LocalReferenceParsedLayer>();
        foreach (var layer in source.Layers)
        {
            if (layer.Text is not { } text)
            {
                continue;
            }

            var document = string.Equals(layer.Path, source.Source.Base.CanonicalPath, StringComparison.Ordinal)
                && source.Document is { } retainedDocument
                && string.Equals(retainedDocument.Source, text, StringComparison.Ordinal)
                    ? retainedDocument
                    : resolver.ParseDocument(text);
            var locations = new Utf8SourceMap(text);
            observations.Documents[layer.Path] = document;
            parsedLayers.Add(new LocalReferenceParsedLayer(
                layer.Path,
                document,
                locations));
            await LocalReferenceOccurrenceReader.ReadAsync(new(inspection.Catalogue, layer.Path, source.Source.Identity.CanonicalBasePath,
                document, locations), observations, resolver, cancellationToken).ConfigureAwait(false);
        }

        observations.Sources.Add(new LocalReferenceSourceObservation(
            source.Source.Identity.CanonicalBasePath,
            source.Source.Identity.AutomaticId,
            parsedLayers));
    }

    private static LocalReferenceCandidateScanState ReadCandidateState(
        OperationalViewState state)
        => state switch
        {
            OperationalViewState.Complete => LocalReferenceCandidateScanState.Complete,
            OperationalViewState.Incomplete => LocalReferenceCandidateScanState.Incomplete,
            OperationalViewState.Blocked => LocalReferenceCandidateScanState.Blocked,
            OperationalViewState.Interrupted => LocalReferenceCandidateScanState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The local-reference candidate scan state is not defined."),
        };

    private static IReadOnlyList<(string Key, string Path)> ReadMissingEntries(
        WorkspaceSettingsDocument settings, SourceCatalogue catalogue, PhysicalPathResolver paths, CancellationToken token)
    {
        if (settings.IgnoreLinksFrom.IsEmpty && settings.IgnoreLinksTo.IsEmpty) return [];
        var workspace = catalogue.Workspace;
        var known = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in catalogue.Candidates)
        {
            var path = source.CanonicalPath;
            // Sources in outside routes have no source root above them, so the walk also ends at a top-level folder.
            while (known.Add(PortableWorkspacePath.CreatePortableKey(path))
                && !catalogue.Layout.SourceRoots.Contains(path, StringComparer.Ordinal)
                && path.Contains('/'))
                path = SourceLogicalPath.ReadParent(path);
        }
        var entries = settings.IgnoreLinksFrom.Select(path => (Key: WorkspaceSettingsDefinitions.IgnoreLinksFromProperty, Path: path))
            .Concat(settings.IgnoreLinksTo.Select(path => (Key: WorkspaceSettingsDefinitions.IgnoreLinksToProperty, Path: path)));
        return entries.Distinct().Where(entry =>
        {
            if (known.Contains(PortableWorkspacePath.CreatePortableKey(entry.Path))) return false;
            if (!PortableWorkspacePath.TryNormalize(entry.Path, out _)) return false;
            var logical = Path.Combine(workspace.LexicalRoot, entry.Path.Replace('/', Path.DirectorySeparatorChar));
            token.ThrowIfCancellationRequested();
            var resolved = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logical);
            return resolved.State is PhysicalPathState.Missing or PhysicalPathState.Dangling;
        }).ToArray();
    }
}
