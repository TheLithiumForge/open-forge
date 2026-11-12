using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.References;

internal static class SourceLinkSuggestionReader
{
    internal static IReadOnlyList<LocalReferenceCandidateObservation> Read(
        IReadOnlyList<LocalReferenceObservation> references,
        IReadOnlyList<LocalReferenceSourceObservation> sources,
        SourceLinkHistory history,
        IReadOnlyDictionary<string, string>? textFiles = null)
    {
        var result = new List<LocalReferenceCandidateObservation>();
        var files = sources.SelectMany(source => source.Layers.Select(layer => KeyValuePair.Create(layer.Path, layer.Document.Source)))
            .Concat(textFiles ?? new Dictionary<string, string>()).DistinctBy(pair => pair.Key).ToArray();
        var parser = new MarkdownDocumentParser();
        var retained = sources.SelectMany(source => source.Layers.Select(layer => (Path: layer.Path, source.Id, Document: layer.Document))).ToArray();
        var retainedPaths = retained.Select(item => item.Path).ToHashSet(StringComparer.Ordinal);
        var sections = retained.Concat(files.Where(file => !retainedPaths.Contains(file.Key))
                .Where(file => Path.GetExtension(file.Key).Equals(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase))
                .Select(file => (Path: file.Key, Id: (string?)null, Document: parser.Parse(file.Value))))
            .Where(item => Path.GetExtension(item.Path).Equals(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => item.Document.Sections.Select(section => (item.Path, item.Id, item.Document, Section: section))).ToArray();
        foreach (var reference in references)
        {
            if (reference.Facts.Finding?.Code == SourceLinkDestinationFindingCode.LegacyAnchor)
            {
                foreach (var target in reference.Facts.Finding.Candidates)
                    Add(target, reference.Facts.Fragment, LocalReferenceCandidateBasisKind.LiteralContent, "legacy ID path");
                continue;
            }
            if (reference.Facts.Target.Resolution is not (SourceLinkTargetResolution.Missing or SourceLinkTargetResolution.FragmentMissing)) continue;
            var entry = history.Entries.FirstOrDefault(entry => entry.SourcePath == reference.SourcePath && entry.Destination == reference.Destination);
            if (entry?.MovedPath is { } moved)
                Add(new() { Id = null, Path = moved }, reference.Facts.Fragment, LocalReferenceCandidateBasisKind.GitRename, $"Git rename from {entry.Commit}");
            if (entry?.PreviousText is { } previousFile)
                foreach (var file in files.Where(file => file.Key != reference.Facts.Target.Path && file.Value == previousFile))
                    Add(new() { Id = null, Path = file.Key }, reference.Facts.Fragment,
                        LocalReferenceCandidateBasisKind.GitRename, $"Git committed content SHA256 {Hash(previousFile)} from {entry.Commit}");
            if (reference.Facts.Fragment is not { } fragment || reference.Facts.Target.Path is not { } targetPath
                || !Path.GetExtension(targetPath).Equals(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase)) continue;
            var decoded = Uri.UnescapeDataString(fragment);
            if (entry?.PreviousText is { } previous)
            {
                var old = parser.Parse(previous);
                var section = old.Sections.SingleOrDefault(section => section.Heading.FragmentIdentifier == decoded);
                if (section is not null && Body(old, section).Length > 0)
                {
                    var hash = Hash(Body(old, section));
                    foreach (var match in sections.Where(item => item.Section.Heading.FragmentIdentifier is not null
                        && Hash(Body(item.Document, item.Section)) == hash))
                    {
                        var sameTarget = match.Path == (entry.MovedPath ?? reference.Facts.Target.Path);
                        Add(new() { Id = match.Id, Path = match.Path }, match.Section.Heading.FragmentIdentifier,
                            sameTarget ? LocalReferenceCandidateBasisKind.GitHeadingChange : LocalReferenceCandidateBasisKind.SectionContent,
                            sameTarget ? $"Git section heading change to #{match.Section.Heading.FragmentIdentifier} from {entry.Commit}"
                                : $"section #{match.Section.Heading.FragmentIdentifier} content SHA256 {hash}");
                    }
                }
            }
            var sameSlug = sections.Where(item => item.Section.Heading.FragmentIdentifier == decoded
                && item.Path != reference.Facts.Target.Path).DistinctBy(item => item.Path).ToArray();
            if (sameSlug.Length == 1)
                Add(new() { Id = sameSlug[0].Id, Path = sameSlug[0].Path }, fragment,
                    LocalReferenceCandidateBasisKind.UniqueSlug, "unique heading slug (weak)");

            void Add(SourceLinkIdentity target, string? targetFragment, LocalReferenceCandidateBasisKind kind, string value)
                => result.Add(LocalReferenceCandidateObservation.Create(reference.SourcePath, reference.Destination, target,
                    [LocalReferenceCandidateBasis.Create(kind, value, null)]) with
                { Fragment = targetFragment });
        }
        return result.OrderBy(candidate => candidate.Bases.Min(basis => basis.Priority))
            .ThenBy(candidate => candidate.Candidate.Path, StringComparer.Ordinal).ThenBy(candidate => candidate.Fragment, StringComparer.Ordinal).ToArray();
    }

    private static string Body(MarkdownDocumentFacts document, MarkdownSectionFact section)
        => document.Source[section.Heading.Span.End..section.Span.End].Trim();

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
