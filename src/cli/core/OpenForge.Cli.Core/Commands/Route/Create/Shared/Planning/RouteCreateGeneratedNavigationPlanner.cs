using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateGeneratedNavigationPlanner
{
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly GeneratedNavigationProjector _projector = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal async ValueTask<RouteCreateNavigationPlanBuild> BuildAsync(
        RouteCreateDestinationPlan destination,
        CancellationToken cancellationToken)
    {
        var inspection = destination.Inspection;
        var targetPath = inspection.Target.Path
            ?? throw new InvalidOperationException(
                "A resolved Route Create target requires a canonical path.");
        var intendedSources = inspection.Catalogue.Sources
            .Where(source => !string.Equals(
                source.Identity.CanonicalBasePath,
                targetPath,
                StringComparison.Ordinal))
            .Append(destination.TargetSource)
            .ToArray();
        var formation = _formationBuilder.Build(inspection.Catalogue, intendedSources);
        if (HasTargetCollision(formation, targetPath))
        {
            return Stop(
                destination,
                RouteCreateFindingCode.IdentityCollision,
                "The intended Route Create target collides with another physical source.",
                isIncomplete: false);
        }

        var targetNode = formation.Topology.FindByPath(targetPath);
        if (targetNode is null || targetNode.ParentState == SourceRouteParentState.None)
        {
            return Stop(
                destination,
                RouteCreateFindingCode.ParentMissing,
                "The Route Create target requires one existing routable parent.",
                isIncomplete: false);
        }

        if (targetNode.ParentState == SourceRouteParentState.Ambiguous)
        {
            return Stop(
                destination,
                RouteCreateFindingCode.RouteAmbiguous,
                "The Route Create target has more than one routable parent.",
                isIncomplete: false);
        }

        var parentSource = formation.FindSource(targetNode.ParentPaths[0])
            ?? throw new InvalidOperationException(
                "A resolved Route Create parent must remain in the intended formation.");
        var reader = new SourceDocumentReader(inspection.Request.Workspace);
        var parentRead = await reader.ReadAsync(parentSource.Base, cancellationToken)
            .ConfigureAwait(false);
        if (!TryReadText(
                parentRead,
                parentSource.Identity.CanonicalBasePath,
                out var parentText,
                out var readFailure))
        {
            return Stop(destination, parentSource, readFailure);
        }

        var metadata = await BuildMetadataAsync(
                destination,
                formation,
                targetNode,
                reader,
                cancellationToken)
            .ConfigureAwait(false);
        if (metadata.Finding is { } metadataFinding)
        {
            return Stop(destination, parentSource, metadataFinding);
        }

        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            [new GeneratedNavigationRegionInput(
                parentSource,
                _markdownParser.Parse(parentText))],
            metadata.Values));
        var region = projection.Regions.Single();
        if (region.State != GeneratedNavigationRegionState.Available
            || region.Change is not { } navigationChange)
        {
            return Stop(destination, parentSource, ProjectionFinding(region));
        }

        var parentSnapshot = await new SourceDocumentSnapshotReader()
            .ReadAsync(inspection.Request.Workspace, parentRead, cancellationToken)
            .ConfigureAwait(false);
        return RouteCreateNavigationPlanBuild.Complete(
            new RouteCreateNavigationPlan
            {
                Formation = formation,
                ParentSource = parentSource,
                ParentSnapshot = parentSnapshot,
                Change = navigationChange,
            });
    }

    private async ValueTask<NavigationMetadataBuild> BuildMetadataAsync(
        RouteCreateDestinationPlan destination,
        GeneratedNavigationFormation formation,
        SourceRouteNode targetNode,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var targetSource = destination.TargetSource;
        var request = destination.Inspection.Request;
        var parent = formation.Topology.FindByPath(targetNode.ParentPaths[0])
            ?? throw new InvalidOperationException(
                "A resolved Route Create target requires its parent topology node.");
        var values = new List<GeneratedNavigationMetadata>
        {
            new(
                targetSource,
                SourceAuthoredMetadataFacts.Complete(
                    request.Metadata.Description,
                    request.Metadata.Tags)),
        };
        foreach (var childPath in parent.ChildPaths.Where(path => !string.Equals(
                     path,
                     targetSource.Identity.CanonicalBasePath,
                     StringComparison.Ordinal)))
        {
            var source = formation.FindSource(childPath)
                ?? throw new InvalidOperationException(
                    "A Route Create parent child must remain in the intended formation.");
            var read = await reader.ReadAsync(source.Base, cancellationToken)
                .ConfigureAwait(false);
            if (!TryReadText(read, childPath, out var text, out var readFailure))
            {
                return NavigationMetadataBuild.Stop(readFailure);
            }

            var facts = _metadataParser.Parse(_markdownParser.Parse(text), source.Base.Form);
            if (facts.State != SourceAuthoredMetadataState.Complete)
            {
                return NavigationMetadataBuild.Stop(new RouteCreateFinding(
                    facts.State == SourceAuthoredMetadataState.Malformed
                        ? RouteCreateFindingCode.MetadataUnsafe
                        : RouteCreateFindingCode.MetadataIncomplete,
                    "Every direct routed child requires complete authored metadata.",
                    childPath));
            }

            values.Add(new GeneratedNavigationMetadata(source, facts));
        }

        return NavigationMetadataBuild.Complete(values);
    }

    private static bool TryReadText(
        SourceDocumentReadResult read,
        string target,
        out string text,
        [NotNullWhen(false)] out RouteCreateFinding? finding)
    {
        text = string.Empty;
        finding = null;
        if (read.Verification.State == SourceLayerVerificationState.Verified
            && read.Read?.State == Framework.Filesystem.TypedReads.Models.FileReadState.Complete
            && read.Read.Value is { } value)
        {
            text = value;
            return true;
        }

        finding = new RouteCreateFinding(
            RouteCreateFindingCode.ProjectionIncomplete,
            read.Read?.Failure?.DirectCause ?? "The routed source content is unavailable.",
            target);

        finding = read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => new RouteCreateFinding(
                    RouteCreateFindingCode.GeneratedRegionUnsafe,
                    "The routed source physical identity is unsafe or changed.",
                    target),
            SourceLayerVerificationState.Cancelled => new RouteCreateFinding(
                RouteCreateFindingCode.Interrupted,
                "Route Create source reading was interrupted.",
                target),
            SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable => finding,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };
        return false;
    }

    private static bool HasTargetCollision(
        GeneratedNavigationFormation formation,
        string targetPath)
        => formation.IntendedTargetCollisions.Count != 0
            || formation.Ambiguities.Any(ambiguity =>
                ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
                && ambiguity.IntendedSources.Any(source => string.Equals(
                    source.Identity.CanonicalBasePath,
                    targetPath,
                    StringComparison.Ordinal)));

    private static RouteCreateFinding ProjectionFinding(
        GeneratedNavigationRegion region)
    {
        var code = region.UnavailableReason is GeneratedNavigationRegionUnavailableReason.MetadataUnavailable
            or GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable
            ? RouteCreateFindingCode.ProjectionIncomplete
            : RouteCreateFindingCode.GeneratedRegionUnsafe;
        return new RouteCreateFinding(
            code,
            region.Cause ?? "The parent generated navigation projection is unavailable.",
            region.Source.Identity.CanonicalBasePath);
    }

    private static RouteCreateNavigationPlanBuild Stop(
        RouteCreateDestinationPlan destination,
        RouteCreateFindingCode code,
        string cause,
        bool isIncomplete)
        => RouteCreateNavigationPlanBuild.Stop(Boundary(
            destination,
            parent: null,
            new RouteCreateFinding(
                code,
                cause,
                destination.Inspection.Target.Path
                    ?? destination.Inspection.Target.Requested),
            isIncomplete));

    private static RouteCreateNavigationPlanBuild Stop(
        RouteCreateDestinationPlan destination,
        SourceLogicalSource parent,
        RouteCreateFinding finding)
        => RouteCreateNavigationPlanBuild.Stop(Boundary(
            destination,
            RouteCreatePlanResultProjector.Parent(parent),
            finding,
            finding.Code is RouteCreateFindingCode.MetadataIncomplete
                or RouteCreateFindingCode.ProjectionIncomplete
                or RouteCreateFindingCode.Interrupted));

    private static RouteCreatePlanningBoundary Boundary(
        RouteCreateDestinationPlan destination,
        RouteCreateParent? parent,
        RouteCreateFinding finding,
        bool isIncomplete)
        => new()
        {
            Target = destination.Inspection.Target,
            Parent = parent,
            Template = destination.Template.Template,
            Finding = finding,
            IsIncomplete = isIncomplete,
        };

    private sealed class NavigationMetadataBuild
    {
        private NavigationMetadataBuild(
            IReadOnlyList<GeneratedNavigationMetadata> values,
            RouteCreateFinding? finding)
        {
            Values = values;
            Finding = finding;
        }

        internal IReadOnlyList<GeneratedNavigationMetadata> Values { get; }

        internal RouteCreateFinding? Finding { get; }

        internal static NavigationMetadataBuild Complete(
            IReadOnlyList<GeneratedNavigationMetadata> values)
            => new(values, finding: null);

        internal static NavigationMetadataBuild Stop(RouteCreateFinding finding)
            => new([], finding);
    }
}
