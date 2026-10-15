using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal sealed class LocalReferenceCandidateReader
{
    internal IReadOnlyList<LocalReferenceCandidateObservation> Read(
        IReadOnlyList<LocalReferenceObservation> references,
        IReadOnlyList<LocalReferenceSourceObservation> sources,
        SourceRouteTopology topology)
    {
        var missing = references.Where(reference => reference.Facts.Target.Resolution == SourceLinkTargetResolution.Missing).ToArray();
        if (missing.Length == 0) return [];
        var requestedLabels = missing.Select(reference => reference.Label.Text).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var byPath = new Dictionary<string, LocalReferenceSourceObservation>(StringComparer.Ordinal);
        var byFilename = new Dictionary<string, List<LocalReferenceSourceObservation>>(StringComparer.Ordinal);
        var byLabel = new Dictionary<string, List<(LocalReferenceSourceObservation Source, LocalReferenceCandidateBasis Basis)>>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            byPath.Add(source.Path, source);
            var filename = Path.GetFileName(source.Path);
            if (!byFilename.TryGetValue(filename, out var names)) byFilename.Add(filename, names = []);
            names.Add(source);
            foreach (var layer in source.Layers)
            {
                var title = layer.Document.Headings.FirstOrDefault(heading => heading.Level == 1);
                if (title is not null && requestedLabels.Contains(title.VisibleText))
                    AddLabel(source, title.VisibleText, LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.Title,
                        title.VisibleText, layer.Locations.Map(title.Span.Start, title.Span.Length)));
                foreach (var link in layer.Document.Links.Concat(layer.Document.Images))
                {
                    if (link.Label.Text is { } label && requestedLabels.Contains(label))
                        AddLabel(source, label, LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.LiteralContent,
                            label, layer.Locations.Map(link.Span.Start, link.Span.Length)));
                }
                foreach (var heading in layer.Document.Headings.Where(heading => requestedLabels.Contains(heading.VisibleText)))
                    AddLabel(source, heading.VisibleText, LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.LiteralContent,
                        heading.VisibleText, layer.Locations.Map(heading.Span.Start, heading.Span.Length)));
            }
        }
        var candidates = new List<LocalReferenceCandidateObservation>();
        foreach (var reference in missing)
        {
            var matches = new Dictionary<string, List<LocalReferenceCandidateBasis>>(StringComparer.Ordinal);
            if (ReadAuthoredLeaf(reference.Destination) is { } leaf && byFilename.TryGetValue(leaf, out var names))
                foreach (var source in names)
                    AddBasis(source.Path, LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.Filename, leaf, location: null));
            if (reference.Label.Text is { } label && byLabel.TryGetValue(label, out var labels))
                foreach (var entry in labels) AddBasis(entry.Source.Path, entry.Basis);
            foreach (var (path, relationship) in ReadRouteBases(reference.RoutePath, topology))
                if (byPath.ContainsKey(path))
                    AddBasis(path, LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.RouteNeighborhood, relationship, location: null));
            foreach (var (path, bases) in matches)
            {
                var source = byPath[path];
                candidates.Add(LocalReferenceCandidateObservation.Create(reference.SourcePath, reference.Destination,
                    new SourceLinkIdentity { Id = source.Id, Path = source.Path },
                    bases.Distinct().OrderBy(basis => basis.Kind).ThenBy(basis => basis.Value, StringComparer.Ordinal)
                        .ThenBy(basis => basis.Location?.ByteOffset).ToArray()));
            }

            void AddBasis(string path, LocalReferenceCandidateBasis basis)
            {
                if (!matches.TryGetValue(path, out var bases)) matches.Add(path, bases = []);
                bases.Add(basis);
            }
        }
        return candidates.OrderBy(candidate => candidate.SourcePath, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Destination, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Candidate.Path, StringComparer.Ordinal).ToArray();

        void AddLabel(LocalReferenceSourceObservation source, string label, LocalReferenceCandidateBasis basis)
        {
            if (!byLabel.TryGetValue(label, out var labels)) byLabel.Add(label, labels = []);
            labels.Add((source, basis));
        }
    }

    private static IEnumerable<(string Path, string Relationship)> ReadRouteBases(string routePath, SourceRouteTopology topology)
    {
        var source = topology.FindByPath(routePath);
        if (source is null) yield break;
        if (source.ParentState == SourceRouteParentState.Resolved)
        {
            var parentPath = source.ParentPaths[0];
            yield return (parentPath, "parent");
            var parent = topology.FindByPath(parentPath)
                ?? throw new InvalidOperationException("An established route parent must be present in the topology.");
            foreach (var child in parent.ChildPaths)
                if (!string.Equals(child, routePath, StringComparison.Ordinal)) yield return (child, "sibling");
        }
        foreach (var child in source.ChildPaths) yield return (child, "child");
    }

    private static string? ReadAuthoredLeaf(string destination)
    {
        var path = destination.Split('#', 2)[0];
        try { path = Uri.UnescapeDataString(path); }
        catch (UriFormatException) { return null; }
        var leaf = path.Replace('\\', '/').Split('/').LastOrDefault();
        return string.IsNullOrWhiteSpace(leaf) ? null : leaf;
    }
}
