using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface ILocalReferenceOperationalContributor
{
    ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken,
        RouteDoctorView? routes = null);
}

internal sealed class LocalReferenceOperationalContributor(
    RouteSourceInspector sourceInspector,
    SourceLinkDestinationResolver destinationResolver,
    LocalReferenceCandidateReader candidateReader) : ILocalReferenceOperationalContributor
{
    internal const int MaximumConcurrentSources = 8;
    internal async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken,
        RouteDoctorView? routes = null)
    {
        var inspection = routes is null
            ? await sourceInspector.ReadAsync(workspace, cancellationToken).ConfigureAwait(false)
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
            var sourceObservations = new LocalReferenceReadAccumulator();
            await ReadSourceAsync(
                inspection,
                inspection.Sources[index],
                sourceObservations,
                resolver,
                cancellationToken).ConfigureAwait(false);
            acquired[index] = sourceObservations;
        }).ConfigureAwait(false);
        var observations = new LocalReferenceReadAccumulator();
        foreach (var source in acquired)
        {
            observations.References.AddRange(source.References);
            observations.Sources.AddRange(source.Sources);
            foreach (var document in source.Documents) observations.Documents[document.Key] = document.Value;
        }

        var candidates = candidateReader.Read(
            observations.References,
            observations.Sources,
            inspection.Routes.Topology);
        return new LocalReferenceDoctorView(
            inspection.State,
            observations.References.ToArray(),
            ReadCandidateState(inspection.State),
            candidates,
            LocalReferenceGraphReader.Read(observations.References))
        {
            Catalogue = inspection.Catalogue,
            Documents = observations.Documents,
        };
    }

    ValueTask<LocalReferenceDoctorView> ILocalReferenceOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken,
        RouteDoctorView? routes)
        => ReadDoctorAsync(workspace, cancellationToken, routes);

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
            var references = document.Links
                .Select(link => (Fact: link, Kind: LocalReferenceKind.Link))
                .Concat(document.Images.Select(image => (Fact: image, Kind: LocalReferenceKind.Image)));
            foreach (var reference in references)
            {
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
                var location = locations.Map(link.Span.Start, link.Span.Length);
                var destinationLocation = link.DestinationSpan is { } destinationSpan
                    ? locations.Map(destinationSpan.Start, destinationSpan.Length)
                    : null;
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
                    Canonicalizations = LocalReferenceFactReader.ReadCanonicalizations(
                        layer.Path,
                        link.RawDestination,
                        destinationLocation,
                        facts),
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

    private sealed class LocalReferenceReadAccumulator
    {
        internal List<LocalReferenceObservation> References { get; } = [];

        internal List<LocalReferenceSourceObservation> Sources { get; } = [];

        internal Dictionary<string, MarkdownDocumentFacts> Documents { get; } = new(StringComparer.Ordinal);
    }
}
