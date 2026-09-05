using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal interface ILocalReferenceOperationalContributor
{
    ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class LocalReferenceOperationalContributor(
    SourceReadSessionReader sessionReader,
    SourceLinkDestinationResolver destinationResolver) : ILocalReferenceOperationalContributor
{
    private readonly MarkdownDocumentParser _markdownParser = new();

    internal async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var session = await sessionReader.ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var references = new List<LocalReferenceObservation>();
        var state = ReadCatalogueState(session.Catalogue);
        foreach (var source in session.Catalogue.Sources)
        {
            state = await ReadLayerAsync(
                    session,
                    source.Base,
                    references,
                    state,
                    cancellationToken)
                .ConfigureAwait(false);
            if (source.Overwrite is { } overwrite)
            {
                state = await ReadLayerAsync(
                        session,
                        overwrite,
                        references,
                        state,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new LocalReferenceDoctorView(state, references.ToArray());
    }

    ValueTask<LocalReferenceDoctorView> ILocalReferenceOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private async ValueTask<OperationalViewState> ReadLayerAsync(
        SourceReadSession session,
        SourceLayer layer,
        ICollection<LocalReferenceObservation> references,
        OperationalViewState currentState,
        CancellationToken cancellationToken)
    {
        var read = await session.DocumentReader.ReadAsync(layer, cancellationToken)
            .ConfigureAwait(false);
        if (read.Read is not { State: FileReadState.Complete, Value: { } text })
        {
            if (read.Verification.State == SourceLayerVerificationState.Cancelled
                || read.Read?.State == FileReadState.Cancelled)
            {
                return OperationalViewState.Interrupted;
            }

            if (currentState is OperationalViewState.Blocked or OperationalViewState.Interrupted)
            {
                return currentState;
            }

            return OperationalViewState.Incomplete;
        }

        var document = _markdownParser.Parse(text);
        var locations = new Utf8SourceMap(text);
        foreach (var link in document.Links)
        {
            var facts = await destinationResolver.ResolveAsync(
                    new SourceLinkDestinationInput
                    {
                        Workspace = session.Catalogue.Workspace,
                        Catalogue = session.Catalogue,
                        SourceCanonicalPath = layer.CanonicalPath,
                        RawDestination = link.RawDestination,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            references.Add(new LocalReferenceObservation
            {
                SourcePath = layer.CanonicalPath,
                Kind = LocalReferenceKind.Link,
                Destination = link.RawDestination,
                Location = locations.Map(link.Span.Start, link.Span.Length),
                Facts = facts,
            });
        }

        return currentState;
    }

    private static OperationalViewState ReadCatalogueState(SourceCatalogue catalogue)
    {
        if (catalogue.IsCancelled)
        {
            return OperationalViewState.Interrupted;
        }

        if (catalogue.Issues.Any(issue => issue.Code is SourceCatalogueIssueCode.RootUnsafe
            or SourceCatalogueIssueCode.CandidateUnsafe
            or SourceCatalogueIssueCode.IdentityCollision
            or SourceCatalogueIssueCode.PhysicalAlias))
        {
            return OperationalViewState.Blocked;
        }

        return catalogue.Issues.All(issue => issue.Code == SourceCatalogueIssueCode.RootMissing)
            ? OperationalViewState.Complete
            : OperationalViewState.Incomplete;
    }
}
