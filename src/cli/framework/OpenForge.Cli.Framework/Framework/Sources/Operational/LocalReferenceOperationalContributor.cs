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
    LocalReferenceCandidateReader candidateReader) : ILocalReferenceOperationalContributor
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

        var candidates = candidateReader.Read(
            observations.References,
            observations.Sources,
            inspection.Routes.Topology);
        return new LocalReferenceDoctorView(
            settings.State is WorkspaceSettingsReadState.Invalid or WorkspaceSettingsReadState.Unavailable
                ? OperationalViewState.Incomplete : inspection.State,
            observations.References.ToArray(),
            ReadCandidateState(inspection.State),
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
        var settings = observations.Settings;
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
            var references = document.Links
                .Select(link => (Fact: link, Kind: LocalReferenceKind.Link))
                .Concat(document.Images.Select(image => (Fact: image, Kind: LocalReferenceKind.Image)));
            foreach (var reference in references)
            {
                if (WorkspaceLinkScope.IgnoresSource(settings, layer.Path))
                {
                    observations.IgnoredLinkCount++;
                    continue;
                }
                var link = reference.Fact;
                var facts = await resolver.ResolveAsync(
                        new SourceLinkDestinationInput
                        {
                            Workspace = inspection.Catalogue.Workspace,
                            Catalogue = inspection.Catalogue,
                            SourceCanonicalPath = layer.Path,
                            RawDestination = link.RawDestination,
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
                if (WorkspaceLinkScope.IgnoresTarget(settings, facts.Target.Path))
                {
                    observations.IgnoredLinkCount++;
                    continue;
                }
                var location = locations.Map(link.Span.Start, link.Span.Length);
                var destinationLocation = ReadEditableDestinationLocation(document, link, locations);
                observations.References.Add(new LocalReferenceObservation
                {
                    SourcePath = layer.Path,
                    RoutePath = source.Source.Identity.CanonicalBasePath,
                    Kind = reference.Kind,
                    Destination = link.RawDestination,
                    Label = link.Label,
                    Location = location,
                    DestinationLocation = destinationLocation,
                    Facts = facts,
                    Fragment = LocalReferenceFactReader.ReadFragment(facts),
                    Canonicalizations = reference.Kind == LocalReferenceKind.Link
                        ? LocalReferenceFactReader.ReadCanonicalizations(layer.Path, link.RawDestination, destinationLocation, facts)
                        : [],
                });
            }
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

    private static SourceLocation? ReadEditableDestinationLocation(MarkdownDocumentFacts document, MarkdownLinkFact link, Utf8SourceMap locations)
    {
        if (link.DestinationSpan is not { } span) return null;
        var start = span.Start;
        var length = span.Length;
        var authored = document.Source.AsSpan(start, length);
        // The parser includes angle delimiters in its span. Keep them outside the exact URL edit.
        if (authored.Length >= 2 && authored[0] == '<' && authored[^1] == '>')
        {
            start++;
            length -= 2;
            authored = authored[1..^1];
        }
        // Decoded entities or Markdown escapes do not prove the exact bytes required by Repair.
        return authored.SequenceEqual(link.RawDestination.AsSpan()) ? locations.Map(start, length) : null;
    }

    private static IReadOnlyList<(string Key, string Path)> ReadMissingEntries(
        WorkspaceSettingsDocument settings, SourceCatalogue catalogue, PhysicalPathResolver paths, CancellationToken token)
    {
        if (settings.IgnoreLinksFrom.IsEmpty && settings.IgnoreLinksTo.IsEmpty) return [];
        var workspace = catalogue.Workspace;
        var known = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in catalogue.Candidates)
        {
            var path = source.CanonicalPath;
            while (path.Length > 0 && known.Add(PortableWorkspacePath.CreatePortableKey(path)))
            {
                if (catalogue.Layout.SourceRoots.Contains(path, StringComparer.Ordinal)) break;
                path = SourceLogicalPath.ReadParent(path);
            }
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
