using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.References;

internal static class SourceAuthoredLinkReader
{
    internal static IReadOnlyList<SourceAuthoredLink> Read(string path, MarkdownDocumentFacts document)
    {
        var markdown = Path.GetExtension(path).Equals(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase);
        var links = markdown ? document.Links.Select(link => new SourceAuthoredLink(link)) : [];
        if (!SourceCommentDiscoveryPolicy.Allows(path, document.Source)) return links.ToArray();
        return links.Concat(SourceCommentLinkReader.Read(path, document.Source, markdown ? document : null))
            .OrderBy(link => link.Span.Start).ToArray();
    }
}
