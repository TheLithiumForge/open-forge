using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface ILocalReferenceOperationalContributor
{
    ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class LocalReferenceOperationalContributor(
    RouteSourceInspector sourceInspector,
    SourceLinkDestinationResolver destinationResolver,
    LocalReferenceCandidateReader candidateReader) : ILocalReferenceOperationalContributor
{
    private readonly MarkdownDocumentParser _markdownParser = new();

    internal async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var inspection = await sourceInspector.ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var observations = new LocalReferenceReadAccumulator();
        foreach (var source in inspection.Sources)
        {
            await ReadSourceAsync(
                workspace,
                inspection,
                source,
                observations,
                cancellationToken).ConfigureAwait(false);
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
            LocalReferenceGraphReader.Read(observations.References));
    }

    ValueTask<LocalReferenceDoctorView> ILocalReferenceOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private async ValueTask ReadSourceAsync(
        CliWorkspace workspace,
        RouteSourceInspection inspection,
        RouteSourceObservation source,
        LocalReferenceReadAccumulator observations,
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
                    : _markdownParser.Parse(text);
            var locations = new Utf8SourceMap(text);
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
                var facts = await destinationResolver.ResolveAsync(
                        new SourceLinkDestinationInput
                        {
                            Workspace = workspace,
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
    }
}
