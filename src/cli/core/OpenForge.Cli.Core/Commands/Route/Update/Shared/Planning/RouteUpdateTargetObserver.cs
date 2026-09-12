using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateTargetObserver(
    RouteUpdateSourceSelector sourceSelector,
    RouteUpdateLayerObserver layerObserver,
    MarkdownDocumentParser markdownParser,
    YamlDocumentParser yamlParser)
{
    private readonly RouteUpdateSourceSelector _sourceSelector = sourceSelector;
    private readonly RouteUpdateLayerObserver _layerObserver = layerObserver;
    private readonly MarkdownDocumentParser _markdownParser = markdownParser;
    private readonly YamlDocumentParser _yamlParser = yamlParser;

    internal async ValueTask<RouteUpdateObservationBuild> ObserveAsync(
        RouteUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var selectionBuild = await _sourceSelector.SelectAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (selectionBuild.Selection is not { } selection)
        {
            return RouteUpdateObservationBuild.Stop(
                selectionBuild.Boundary
                    ?? throw new InvalidOperationException(
                        "An incomplete Route Update source selection requires one boundary."));
        }

        var operation = selection.Request;
        var target = selection.Target;
        var source = selection.Source;
        var targetLayerBuild = await _layerObserver.ObserveAsync(
                new RouteUpdateLayerObservationInput
                {
                    Workspace = operation.Workspace,
                    Layer = source.Base,
                },
                cancellationToken)
            .ConfigureAwait(false);
        if (targetLayerBuild.Layer is not { } targetLayer)
        {
            return FromLayerFailure(selection, targetLayerBuild);
        }

        var text = targetLayer.Text;
        var markdown = _markdownParser.Parse(text);
        if (markdown.Frontmatter.State != MarkdownFrontmatterState.Complete
            || markdown.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return Stop(selection, RouteUpdateFindingCode.FrontmatterUnsafe,
                "The selected source does not expose one complete leading frontmatter block.");
        }

        var yaml = _yamlParser.Parse(text[yamlSpan.Start..yamlSpan.End]);
        if (yaml.State != YamlDocumentState.Complete)
        {
            return Stop(selection, RouteUpdateFindingCode.FrontmatterUnsafe,
                "The selected source frontmatter is not complete parsed YAML.");
        }

        RouteUpdateObservedLayer? overwriteLayer = null;
        if (source.Overwrite is { } overwrite)
        {
            var overwriteBuild = await _layerObserver.ObserveAsync(
                    new RouteUpdateLayerObservationInput
                    {
                        Workspace = operation.Workspace,
                        Layer = overwrite,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            if (overwriteBuild.Layer is not { } observedOverwrite)
            {
                return FromLayerFailure(selection, overwriteBuild);
            }

            overwriteLayer = observedOverwrite;
        }

        return RouteUpdateObservationBuild.Complete(
            new RouteUpdateObservation
            {
                Request = operation,
                Target = target,
                Catalogue = selection.Catalogue,
                TargetSource = source,
                TargetSnapshot = targetLayer.Snapshot,
                OverwriteSnapshot = overwriteLayer?.Snapshot,
                TargetText = text,
                Markdown = markdown,
                Frontmatter = yaml,
            });
    }

    private static RouteUpdateObservationBuild FromLayerFailure(
        RouteUpdateSourceSelection selection,
        RouteUpdateObservedLayerBuild build)
        => RouteUpdateObservationBuild.Stop(
            RouteUpdateObservationBoundaryProjector.Stop(
                new RouteUpdateObservationBoundaryInput
                {
                    Request = selection.Request,
                    Target = selection.Target,
                    Code = build.FailureCode
                        ?? throw new InvalidOperationException(
                            "An incomplete layer observation requires one finding code."),
                    Cause = build.FailureCause
                        ?? throw new InvalidOperationException(
                            "An incomplete layer observation requires one cause."),
                    IsIncomplete = build.IsIncomplete,
                }));

    private static RouteUpdateObservationBuild Stop(
        RouteUpdateSourceSelection selection,
        RouteUpdateFindingCode code,
        string cause)
        => RouteUpdateObservationBuild.Stop(
            RouteUpdateObservationBoundaryProjector.Stop(
                new RouteUpdateObservationBoundaryInput
                {
                    Request = selection.Request,
                    Target = selection.Target,
                    Code = code,
                    Cause = cause,
                    IsIncomplete = false,
                }));
}
