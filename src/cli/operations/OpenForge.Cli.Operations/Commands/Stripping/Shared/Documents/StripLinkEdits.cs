using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;

/// <summary>The facts one document's link pass needs.</summary>
internal sealed record StripLinkContext
{
    public required string LexicalRoot { get; init; }

    public required string Path { get; init; }

    public required bool StripLinks { get; init; }

    // Maps a canonical path to where it lives after flatten. Unmoved paths map to themselves.
    public required Func<string, string> Moved { get; init; }
}

/// <summary>
/// Turns links to documents into their text and rebases kept links whose source or
/// target moved. The strip Interface contract's links part and flatten define both.
/// </summary>
internal static class StripLinkEdits
{
    private const string ParentPrefix = "../";
    private const string CurrentPrefix = "./";

    internal static StripDocumentEdits Read(StripLinkContext context, MarkdownDocumentFacts facts)
    {
        var edits = new List<StripTextEdit>();
        var navigation = facts.GeneratedRegion.EntriesBlock is { Exists: true } block ? block.Span : null;
        var definitionUses = new Dictionary<int, DefinitionUse>();
        var rebasedDefinitions = new HashSet<int>();
        foreach (var link in SourceAuthoredLinkReader.Read(context.Path, facts))
        {
            if (link.IsComment)
            {
                AddRebase(context, facts.Source, link.Link, isComment: true, edits, rebasedDefinitions);
                continue;
            }

            if (link.Link.Form == MarkdownLinkForm.Autolink)
            {
                continue;
            }

            var reference = link.Link.Form == MarkdownLinkForm.Reference && link.DestinationSpan is not null;
            DefinitionUse? use = null;
            if (reference)
            {
                var key = link.DestinationSpan!.Start;
                if (!definitionUses.TryGetValue(key, out use))
                {
                    use = new DefinitionUse(link.DestinationSpan);
                    definitionUses.Add(key, use);
                }

                use.Total++;
            }

            var generated = navigation is not null && link.Span.Start >= navigation.Start && link.Span.End <= navigation.End;
            if (context.StripLinks && !generated && IsDocumentLink(context, link.RawDestination) && link.Link.LabelSpan is { } label)
            {
                edits.Add(new StripTextEdit(link.Span.Start, link.Span.End, facts.Source[label.Start..label.End], StripRewritePart.Links));
                if (use is not null)
                {
                    use.Stripped++;
                }

                continue;
            }

            AddRebase(context, facts.Source, link.Link, isComment: false, edits, rebasedDefinitions);
        }

        foreach (var image in facts.Images)
        {
            AddRebase(context, facts.Source, image, isComment: false, edits, rebasedDefinitions);
        }

        foreach (var uses in definitionUses.Values.Where(use => use.Total == use.Stripped).OrderBy(use => use.Span.Start))
        {
            var start = StripSourceLines.LineStart(facts.Source, uses.Span.Start);
            var end = StripSourceLines.NextLineStart(facts.Source, uses.Span.End);
            edits.RemoveAll(edit => edit.Start >= start && edit.End <= end);
            edits.Add(new StripTextEdit(start, end, string.Empty, StripRewritePart.Links));
        }

        return new StripDocumentEdits(edits);
    }

    private static bool IsDocumentLink(StripLinkContext context, string raw)
    {
        var destination = SourceLinkDestinationResolver.ReadLocalDestination(context.LexicalRoot, context.Path, raw);
        if (destination is not { Kind: SourceLocalDestinationKind.Local, CanonicalPath: { } path })
        {
            return false;
        }

        return destination.IsFragmentOnly
            || path.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.Ordinal)
            || destination.LexicalPath is { } lexical && Directory.Exists(lexical);
    }

    private static void AddRebase(
        StripLinkContext context,
        string source,
        MarkdownLinkFact link,
        bool isComment,
        List<StripTextEdit> edits,
        HashSet<int> rebasedDefinitions)
    {
        if (link.DestinationSpan is not { } span || !rebasedDefinitions.Add(span.Start))
        {
            return;
        }

        var destination = SourceLinkDestinationResolver.ReadLocalDestination(context.LexicalRoot, context.Path, link.RawDestination, isComment);
        if (destination is not { Kind: SourceLocalDestinationKind.Local, CanonicalPath: { } target } || destination.IsFragmentOnly)
        {
            return;
        }

        var from = context.Moved(context.Path);
        var to = context.Moved(target);
        if (from == context.Path && to == target)
        {
            return;
        }

        var rootRelative = isComment
            && !link.RawDestination.StartsWith(CurrentPrefix, StringComparison.Ordinal)
            && !link.RawDestination.StartsWith(ParentPrefix, StringComparison.Ordinal);
        var path = rootRelative ? to : RelativePath(Parent(from), to);
        if (isComment && !rootRelative && !path.StartsWith(ParentPrefix, StringComparison.Ordinal))
        {
            path = CurrentPrefix + path;
        }

        var hash = link.RawDestination.IndexOf('#');
        var rawPath = hash >= 0 ? link.RawDestination[..hash] : link.RawDestination;
        if (rawPath.EndsWith('/') && !path.EndsWith('/'))
        {
            path += "/";
        }

        var original = source[span.Start..span.End];
        var replacement = Encode(path) + destination.Suffix;
        if (original.Length >= 2 && original[0] == '<' && original[^1] == '>')
        {
            replacement = $"<{replacement}>";
        }

        if (!string.Equals(original, replacement, StringComparison.Ordinal))
        {
            edits.Add(new StripTextEdit(span.Start, span.End, replacement, StripRewritePart.Relink));
        }
    }

    private static string Encode(string path)
        => path.Any(character => char.IsWhiteSpace(character) || character is '(' or ')' or '<' or '>' or '%' or '#' or '?' or '\\')
            ? SourceDestinationEncoder.EncodePath(path)
            : path;

    private static string Parent(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path[..slash];
    }

    private sealed class DefinitionUse(MarkdownTextSpan span)
    {
        internal MarkdownTextSpan Span { get; } = span;

        internal int Total { get; set; }

        internal int Stripped { get; set; }
    }

    // The plain relative path between two canonical workspace paths.
    internal static string RelativePath(string fromDirectory, string toPath)
    {
        var from = fromDirectory.Length == 0 ? [] : fromDirectory.Split('/');
        var to = toPath.Split('/');
        var common = 0;
        while (common < from.Length && common < to.Length - 1 && string.Equals(from[common], to[common], StringComparison.Ordinal))
        {
            common++;
        }

        return string.Concat(Enumerable.Repeat(ParentPrefix, from.Length - common)) + string.Join('/', to[common..]);
    }
}
