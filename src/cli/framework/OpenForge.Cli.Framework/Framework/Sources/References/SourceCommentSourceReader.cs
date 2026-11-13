using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;

namespace OpenForge.Cli.Core.Framework.Sources.References;

internal static class SourceCommentSourceReader
{
    internal static IReadOnlySet<string> ExcludedDirectories => SourceCommentDiscoveryPolicy.ExcludedDirectories;

    internal static async ValueTask<SourceCommentScan> ReadAsync(SourceReadSession session, CancellationToken token,
        IReadOnlyDictionary<string, string>? retainedText = null)
    {
        var sources = new List<SourceLogicalSource>();
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        var textFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(session.Catalogue.Workspace.LexicalRoot);
        var complete = true;
        while (pending.TryPop(out var directory))
        {
            token.ThrowIfCancellationRequested();
            FileSystemInfo[] entries;
            try { entries = new DirectoryInfo(directory).GetFileSystemInfos(); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { complete = false; continue; }
            foreach (var entry in entries.OrderBy(entry => entry.Name, StringComparer.Ordinal))
            {
                token.ThrowIfCancellationRequested();
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                if (entry is DirectoryInfo)
                {
                    if (!ExcludedDirectories.Contains(entry.Name)) pending.Push(entry.FullName);
                    continue;
                }
                if (entry.Name.Contains(".generated.", StringComparison.OrdinalIgnoreCase)
                    || entry.Name.EndsWith(SourceCommentDiscoveryPolicy.GeneratedSuffix, StringComparison.OrdinalIgnoreCase)
                    || entry.Name.Contains(".min.", StringComparison.OrdinalIgnoreCase)) continue;
                var path = Path.GetRelativePath(session.Catalogue.Workspace.LexicalRoot, entry.FullName).Replace(Path.DirectorySeparatorChar, '/');
                if (session.Catalogue.FindByPath(path) is not null || !SourceWorkspaceRelativePath.IsCanonical(path)) continue;
                var layer = new SourceLayer(path, entry.FullName, SourceDocumentForm.Markdown, SourceLayerKind.Base);
                var text = retainedText?.GetValueOrDefault(path);
                if (text is null)
                {
                    var read = await session.DocumentReader.ReadAsync(layer, token).ConfigureAwait(false);
                    if (read.Read?.State == FileReadState.Cancelled) token.ThrowIfCancellationRequested();
                    if (read.Read is not { State: FileReadState.Complete, Value: { } currentText })
                    {
                        if (read.Read?.State is not FileReadState.InvalidEncoding) complete = false;
                        continue;
                    }
                    text = currentText;
                }
                if (text.Contains('\0')) continue;
                if (!SourceCommentDiscoveryPolicy.Allows(path, text)) continue;
                textFiles.Add(path, text);
                if (!text.Contains(SourceCommentLinkReader.OpenForgeMarker, StringComparison.Ordinal)
                    && !text.Contains(SourceCommentLinkReader.RuneMarker, StringComparison.Ordinal)) continue;
                var document = session.DocumentReader.ParseDocument(text);
                if (SourceAuthoredLinkReader.Read(path, document).All(link => !link.IsComment)) continue;
                sources.Add(new SourceLogicalSource(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path), layer));
                documents.Add(path, document);
            }
        }
        return new SourceCommentScan(sources.OrderBy(source => source.Base.CanonicalPath, StringComparer.Ordinal).ToArray(), documents, complete)
        { TextFiles = textFiles };
    }

    internal static SourceReadSession Extend(SourceReadSession session, SourceCommentScan scan)
    {
        var catalogue = session.Catalogue;
        return new SourceReadSession(new SourceCatalogue(catalogue.Layout, catalogue.Candidates,
            catalogue.Sources.Concat(scan.Sources), catalogue.Issues, catalogue.IsCancelled, catalogue.Aliases, catalogue.IncludedRows),
            session.DocumentReader, session.DefaultSelectionScopes)
        { CommentScanComplete = scan.Complete };
    }

    internal static SourceLogicalSource? ReadExactSource(SourceReadSession session, string path)
    {
        if (!SourceWorkspaceRelativePath.IsCanonical(path)
            || path.Split('/').Any(ExcludedDirectories.Contains)) return null;
        var workspace = session.Catalogue.Workspace;
        var lexical = Path.Combine(workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar));
        var physical = session.DocumentReader.PhysicalPaths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexical);
        if (physical.State != PhysicalPathState.Contained || !File.Exists(physical.GetContainedPhysicalPath())) return null;
        return new SourceLogicalSource(new(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new(path, physical.GetContainedPhysicalPath(), SourceDocumentForm.Markdown, SourceLayerKind.Base));
    }
}
