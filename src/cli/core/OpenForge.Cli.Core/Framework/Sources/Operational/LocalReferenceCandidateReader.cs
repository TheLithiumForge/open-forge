using OpenForge.Cli.Core.Framework.Sources.Locations;
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
        var candidates = new List<LocalReferenceCandidateObservation>();
        foreach (var reference in references.Where(reference =>
                     reference.Facts.Target.Resolution == SourceLinkTargetResolution.Missing))
        {
            var authoredLeaf = ReadAuthoredLeaf(reference.Destination);
            foreach (var source in sources)
            {
                var bases = ReadBases(reference, source, authoredLeaf).ToList();
                bases.AddRange(ReadRouteBases(reference, source, topology));
                if (bases.Count == 0)
                {
                    continue;
                }

                candidates.Add(LocalReferenceCandidateObservation.Create(
                    reference.SourcePath,
                    reference.Destination,
                    new SourceLinkIdentity
                    {
                        Id = source.Id,
                        Path = source.Path,
                    },
                    bases
                        .Distinct()
                        .OrderBy(basis => basis.Kind)
                        .ThenBy(basis => basis.Value, StringComparer.Ordinal)
                        .ThenBy(basis => basis.Location?.ByteOffset)
                        .ToArray()));
            }
        }

        return candidates
            .OrderBy(candidate => candidate.SourcePath, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Destination, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Candidate.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<LocalReferenceCandidateBasis> ReadBases(
        LocalReferenceObservation reference,
        LocalReferenceSourceObservation source,
        string? authoredLeaf)
    {
        var bases = new List<LocalReferenceCandidateBasis>();
        var candidateLeaf = Path.GetFileName(source.Path);
        if (authoredLeaf is not null
            && string.Equals(authoredLeaf, candidateLeaf, StringComparison.Ordinal))
        {
            bases.Add(LocalReferenceCandidateBasis.Create(
                LocalReferenceCandidateBasisKind.Filename,
                candidateLeaf,
                location: null));
        }

        if (reference.Label.Text is not { } label)
        {
            return bases;
        }

        foreach (var layer in source.Layers)
        {
            var title = layer.Document.Headings.FirstOrDefault(heading => heading.Level == 1);
            if (title is not null
                && string.Equals(label, title.VisibleText, StringComparison.Ordinal))
            {
                bases.Add(LocalReferenceCandidateBasis.Create(
                    LocalReferenceCandidateBasisKind.Title,
                    title.VisibleText,
                    layer.Locations.Map(title.Span.Start, title.Span.Length)));
            }

            bases.AddRange(ReadLiteralOccurrences(layer, label));
        }

        return bases;
    }

    private static IReadOnlyList<LocalReferenceCandidateBasis> ReadLiteralOccurrences(
        LocalReferenceParsedLayer layer,
        string label)
        => layer.Document.Links
            .Concat(layer.Document.Images)
            .Where(link => string.Equals(link.Label.Text, label, StringComparison.Ordinal))
            .Select(link => LocalReferenceCandidateBasis.Create(
                LocalReferenceCandidateBasisKind.LiteralContent,
                label,
                layer.Locations.Map(link.Span.Start, link.Span.Length)))
            .Concat(layer.Document.Headings
                .Where(heading => string.Equals(
                    heading.VisibleText,
                    label,
                    StringComparison.Ordinal))
                .Select(heading => LocalReferenceCandidateBasis.Create(
                    LocalReferenceCandidateBasisKind.LiteralContent,
                    label,
                    layer.Locations.Map(heading.Span.Start, heading.Span.Length))))
            .ToArray();

    private static IReadOnlyList<LocalReferenceCandidateBasis> ReadRouteBases(
        LocalReferenceObservation reference,
        LocalReferenceSourceObservation candidate,
        SourceRouteTopology topology)
    {
        var source = topology.FindByPath(reference.RoutePath);
        if (source is null)
        {
            return [];
        }

        var relationships = new List<string>();
        if (source.ParentState == SourceRouteParentState.Resolved)
        {
            var parentPath = source.ParentPaths[0];
            if (string.Equals(parentPath, candidate.Path, StringComparison.Ordinal))
            {
                relationships.Add("parent");
            }

            var parent = topology.FindByPath(parentPath)
                ?? throw new InvalidOperationException(
                    "An established route parent must be present in the topology.");
            if (parent.ChildPaths.Contains(candidate.Path, StringComparer.Ordinal)
                && !string.Equals(candidate.Path, reference.RoutePath, StringComparison.Ordinal))
            {
                relationships.Add("sibling");
            }
        }

        if (source.ChildPaths.Contains(candidate.Path, StringComparer.Ordinal))
        {
            relationships.Add("child");
        }

        return relationships
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(relationship => LocalReferenceCandidateBasis.Create(
                LocalReferenceCandidateBasisKind.RouteNeighborhood,
                relationship,
                location: null))
            .ToArray();
    }

    private static string? ReadAuthoredLeaf(string destination)
    {
        var path = destination.Split('#', 2)[0];
        try
        {
            path = Uri.UnescapeDataString(path);
        }
        catch (UriFormatException)
        {
            return null;
        }

        var leaf = path.Replace('\\', '/').Split('/').LastOrDefault();
        return string.IsNullOrWhiteSpace(leaf) ? null : leaf;
    }
}
