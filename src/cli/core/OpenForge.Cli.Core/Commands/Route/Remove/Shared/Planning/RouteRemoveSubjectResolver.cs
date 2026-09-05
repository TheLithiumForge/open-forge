using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveSubjectResolver
{
    private readonly RouteRemoveSubjectSelector _selector;
    private readonly FileExpectationValidator _expectationValidator;

    internal RouteRemoveSubjectResolver(
        SourceCatalogueReader catalogueReader,
        SourceReferenceResolver referenceResolver,
        SourceRouteFactsResolver routeFactsResolver,
        RouteRemoveNavigationExposureReader exposureReader,
        FileExpectationValidator expectationValidator)
    {
        _selector = new RouteRemoveSubjectSelector(
            catalogueReader,
            referenceResolver,
            routeFactsResolver,
            exposureReader);
        _expectationValidator = expectationValidator;
    }

    internal async ValueTask<RouteRemoveSubjectResolution> ResolveAsync(
        RouteRemoveSubjectResolutionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var selection = await _selector.SelectAsync(request.Request, cancellationToken)
            .ConfigureAwait(false);
        if (selection.Boundary is { } boundary)
        {
            return new RouteRemoveSubjectResolution(subject: null, boundary);
        }

        var subject = selection.Subject
            ?? throw new InvalidOperationException(
                "A successful Route Remove subject selection requires its subject.");
        var layers = await ReadLayersAsync(subject, cancellationToken).ConfigureAwait(false);
        if (layers is { } snapshots)
        {
            return new RouteRemoveSubjectResolution(
                subject with { Layers = snapshots },
                boundary: null);
        }

        return SnapshotBoundary(subject, cancellationToken.IsCancellationRequested);
    }

    private async ValueTask<ImmutableArray<RouteRemoveResolvedLayer>?> ReadLayersAsync(
        RouteRemoveResolvedSubject subject,
        CancellationToken cancellationToken)
    {
        var workspace = subject.Request.Workspace;
        var reader = new SourceDocumentReader(workspace);
        var snapshotReader = new SourceDocumentSnapshotReader();
        IReadOnlyList<SourceLayer> layers = subject.SelectedSource.Overwrite is { } overwrite
            ? [subject.SelectedSource.Base, overwrite]
            : [subject.SelectedSource.Base];
        var results = new List<RouteRemoveResolvedLayer>(layers.Count);
        foreach (var layer in layers)
        {
            var snapshot = await ReadLayerAsync(
                workspace,
                reader,
                snapshotReader,
                layer,
                cancellationToken).ConfigureAwait(false);
            if (snapshot is null)
            {
                return null;
            }

            results.Add(new RouteRemoveResolvedLayer { Layer = layer, Snapshot = snapshot });
        }

        return results.ToImmutableArray();
    }

    private async ValueTask<Framework.Mutation.Models.Filesystem.FileStateSnapshot?> ReadLayerAsync(
        CliWorkspace workspace,
        SourceDocumentReader reader,
        SourceDocumentSnapshotReader snapshotReader,
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var read = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Read?.State != Framework.Filesystem.TypedReads.FileReadState.Complete)
        {
            return null;
        }

        var snapshot = await snapshotReader.ReadAsync(
            workspace,
            read,
            cancellationToken).ConfigureAwait(false);
        var check = await _expectationValidator.ValidateAsync(
            workspace,
            snapshot.Expectation,
            cancellationToken).ConfigureAwait(false);
        return check.State == FileExpectationValidationState.Matched ? snapshot : null;
    }

    private static RouteRemoveSubjectResolution SnapshotBoundary(
        RouteRemoveResolvedSubject subject,
        bool interrupted)
    {
        var formation = RouteRemoveBoundary.Start(subject.Request) with { Source = subject.Source };
        return new RouteRemoveSubjectResolution(
            subject: null,
            RouteRemoveBoundary.Stop(
                formation,
                interrupted ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.SourceUnsafe,
                interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked,
                subject.SelectedSource.Identity.CanonicalBasePath,
                interrupted
                    ? "Route Remove source snapshotting was interrupted."
                    : "The selected source layers could not retain one exact safe snapshot."));
    }
}
