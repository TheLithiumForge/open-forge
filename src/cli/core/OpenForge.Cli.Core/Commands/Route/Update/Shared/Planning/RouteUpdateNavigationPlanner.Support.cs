using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdateNavigationPlanner
{
    private static async ValueTask<NavigationMetadataBuild> BuildMetadataAsync(
        RouteUpdateDestinationPlan destination,
        GeneratedNavigationFormation formation,
        IEnumerable<SourceLogicalSource> regionSources,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var observation = destination.Body.Metadata.Observation;
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in regionSources)
        {
            IReadOnlyList<string>? children = source == formation.Loader
                ? formation.Topology.LoaderRootPaths
                : formation.Topology.FindByPath(
                    source.Identity.CanonicalBasePath)?.ChildPaths;
            if (children is null)
            {
                return NavigationMetadataBuild.Stop(
                    new RouteUpdateFinding(
                        RouteUpdateFindingCode.ProjectionIncomplete,
                        "Generated navigation parent topology is unavailable.",
                        source.Identity.CanonicalBasePath));
            }

            paths.UnionWith(children);
        }

        var metadata = new List<GeneratedNavigationMetadata>();
        foreach (var path in paths.OrderBy(path => path, StringComparer.Ordinal))
        {
            var source = formation.FindSource(path)
                ?? throw new InvalidOperationException(
                    "A generated-navigation child must remain in formation.");
            if (ReferenceEquals(source, observation.TargetSource))
            {
                metadata.Add(new GeneratedNavigationMetadata(
                    source,
                    destination.IntendedMetadata));
                continue;
            }

            var read = await reader.ReadAsync(source.Base, cancellationToken)
                .ConfigureAwait(false);
            if (!TryReadText(read, out var text, out var finding))
            {
                return NavigationMetadataBuild.Stop(finding);
            }

            var facts = new SourceAuthoredMetadataParser().Parse(
                new MarkdownDocumentParser().Parse(text),
                source.Base.Form);
            if (facts.State != SourceAuthoredMetadataState.Complete)
            {
                return NavigationMetadataBuild.Stop(
                    new RouteUpdateFinding(
                        facts.State == SourceAuthoredMetadataState.Malformed
                            ? RouteUpdateFindingCode.GeneratedRegionUnsafe
                            : RouteUpdateFindingCode.ProjectionIncomplete,
                        "Every direct routed child requires complete authored metadata.",
                        source.Identity.CanonicalBasePath));
            }

            metadata.Add(new GeneratedNavigationMetadata(source, facts));
        }

        return NavigationMetadataBuild.Complete(metadata);
    }

    private static bool TryReadText(
        SourceDocumentReadResult read,
        out string text,
        out RouteUpdateFinding finding)
    {
        text = string.Empty;
        finding = new RouteUpdateFinding(
            RouteUpdateFindingCode.ProjectionIncomplete,
            read.Read?.Failure?.DirectCause
                ?? "A routed source required for navigation is unavailable.",
            read.Layer.CanonicalPath);
        if (read.Verification.State == SourceLayerVerificationState.Verified
            && read.Read?.State == FileReadState.Complete
            && read.Read.Value is { } value)
        {
            text = value;
            return true;
        }

        if (read.Read?.State == FileReadState.Cancelled)
        {
            finding = new RouteUpdateFinding(
                RouteUpdateFindingCode.Interrupted,
                "Route Update navigation reading was interrupted.",
                read.Layer.CanonicalPath);
            return false;
        }

        finding = read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => new RouteUpdateFinding(
                    RouteUpdateFindingCode.GeneratedRegionUnsafe,
                    "A routed navigation source physical identity is unsafe or changed.",
                    read.Layer.CanonicalPath),
            SourceLayerVerificationState.Cancelled => new RouteUpdateFinding(
                    RouteUpdateFindingCode.Interrupted,
                    "Route Update navigation reading was interrupted.",
                    read.Layer.CanonicalPath),
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

    private sealed class NavigationMetadataBuild
    {
        private NavigationMetadataBuild(
            IReadOnlyList<GeneratedNavigationMetadata> values,
            RouteUpdateFinding? finding)
        {
            Values = values;
            Finding = finding;
        }

        internal IReadOnlyList<GeneratedNavigationMetadata> Values { get; }

        internal RouteUpdateFinding? Finding { get; }

        internal static NavigationMetadataBuild Complete(
            IReadOnlyList<GeneratedNavigationMetadata> values)
            => new(values, finding: null);

        internal static NavigationMetadataBuild Stop(RouteUpdateFinding finding)
            => new([], finding);
    }
}
