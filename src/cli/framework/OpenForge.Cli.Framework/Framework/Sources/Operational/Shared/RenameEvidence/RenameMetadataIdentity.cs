using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameMetadataIdentity
{
    internal static RenameMetadataSignature? Read(MarkdownDocumentFacts document)
    {
        var facts = new SourceAuthoredMetadataParser().Parse(document, SourceDocumentForm.Markdown);
        if (facts.State != SourceAuthoredMetadataState.Complete || facts.Description is not { } description
            || facts.Tags.Count == 0 || facts.ApplyTo.State == ApplyToMetadataState.Invalid)
        {
            return null;
        }

        return Create(description, facts.Tags, facts.ApplyTo.Patterns.Select(pattern => pattern.Text));
    }

    internal static RenameMetadataSignature Create(string description, IEnumerable<string> tags, IEnumerable<string> applyTo)
        => new(Description: description, Tags: tags.ToImmutableArray(), ApplyTo: applyTo.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToImmutableArray());

    internal static bool Matches(RenameMetadataSignature? left, RenameMetadataSignature? right)
        => left is not null && right is not null && left.Description == right.Description
            && left.Tags.SequenceEqual(right.Tags, StringComparer.Ordinal)
            && left.ApplyTo.SequenceEqual(right.ApplyTo, StringComparer.Ordinal);

    internal static bool IsDistinctLabel(RenameRecordedIdentity old, RenameCurrentCandidate candidate)
    {
        if (old.Metadata is not { Tags.IsEmpty: false } metadata) return false;
        var description = metadata.Description;
        var stem = Path.GetFileNameWithoutExtension(old.Path);
        var title = old.Title ?? ReadTitle(candidate.Content.Document);
        return !EqualsLabel(description, stem)
            && !EqualsLabel(description, FrameworkMetadataDescriptionDeriver.SentenceCase(stem))
            && !EqualsLabel(description, old.Path[..^3])
            && !EqualsLabel(description, old.Path[(".agents/".Length)..^3])
            && (title is null || !EqualsLabel(description, title));
    }

    internal static string? ReadTitle(MarkdownDocumentFacts document)
        => document.Headings.FirstOrDefault(heading => heading.IsTopLevel && heading.IsVisibleTextSupported && !string.IsNullOrWhiteSpace(heading.VisibleText))?.VisibleText;

    private static bool EqualsLabel(string left, string right)
        => string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string text)
        => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
