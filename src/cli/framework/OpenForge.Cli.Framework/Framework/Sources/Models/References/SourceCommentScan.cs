using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal sealed record SourceCommentScan(
    IReadOnlyList<SourceLogicalSource> Sources,
    IReadOnlyDictionary<string, MarkdownDocumentFacts> Documents,
    bool Complete)
{
    internal IReadOnlyDictionary<string, string> TextFiles { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>Lists workspace-relative files for comment discovery, or returns null when the list is unavailable.</summary>
internal delegate ValueTask<IReadOnlyList<string>?> SourceCommentFileListRead(string workspaceRoot, CancellationToken cancellationToken);
