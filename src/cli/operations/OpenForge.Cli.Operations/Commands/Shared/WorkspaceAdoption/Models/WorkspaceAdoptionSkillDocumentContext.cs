using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionSkillDocumentContext
{
    internal WorkspaceAdoptionSkillDocumentContext(
        string canonicalPath,
        string source,
        MarkdownDocumentFacts markdown,
        SourceAuthoredMetadataFacts authored,
        ReadOnlyMemory<byte> originalBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(authored);

        CanonicalPath = canonicalPath;
        Source = source;
        Markdown = markdown;
        Authored = authored;
        OriginalBytes = originalBytes;
    }

    internal string CanonicalPath { get; }

    internal string Source { get; }

    internal MarkdownDocumentFacts Markdown { get; }

    internal SourceAuthoredMetadataFacts Authored { get; }

    internal ReadOnlyMemory<byte> OriginalBytes { get; }
}
