using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Sources.References;

internal static class SourceCommentSourceReader
{
    internal static async ValueTask<SourceCommentScan> ReadAsync(SourceReadSession session, CancellationToken token,
        IReadOnlyDictionary<string, string>? retainedText = null, SourceCommentFileListRead? listFiles = null)
    {
        var sources = new List<SourceLogicalSource>();
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        var textFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var root = session.Catalogue.Workspace.LexicalRoot;
        IReadOnlyList<string> paths = [];
        var complete = false;
        if (listFiles is null) (paths, complete) = Walk(root, token);
        else if (await listFiles(root, token).ConfigureAwait(false) is { } listed) (paths, complete) = (listed, true);
        token.ThrowIfCancellationRequested();
        foreach (var path in paths.Order(StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            if (session.Catalogue.FindByPath(path) is not null || !SourceWorkspaceRelativePath.IsCanonical(path)) continue;
            var layer = new SourceLayer(path, Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)),
                SourceDocumentForm.Markdown, SourceLayerKind.Base);
            var text = retainedText?.GetValueOrDefault(path);
            if (text is null)
            {
                var read = await session.DocumentReader.ReadAsync(layer, token).ConfigureAwait(false);
                if (read.Read?.State == FileReadState.Cancelled) token.ThrowIfCancellationRequested();
                if (read.Read is not { State: FileReadState.Complete, Value: { } currentText })
                {
                    // A listed file deleted from disk, an external link or the CLI's runtime folder holds no comment links.
                    if (read.Verification.State is not (SourceLayerVerificationState.Missing or SourceLayerVerificationState.Unsafe)
                        && read.Read?.State is not FileReadState.InvalidEncoding) complete = false;
                    continue;
                }
                text = currentText;
            }
            if (text.Contains('\0')) continue;
            if (!SourceCommentDiscoveryPolicy.Allows(text)) continue;
            textFiles.Add(path, text);
            if (!text.Contains(SourceCommentLinkReader.OpenForgeMarker, StringComparison.Ordinal)
                && !text.Contains(SourceCommentLinkReader.RuneMarker, StringComparison.Ordinal)) continue;
            var document = session.DocumentReader.ParseDocument(text);
            if (SourceAuthoredLinkReader.Read(path, document).All(link => !link.IsComment)) continue;
            sources.Add(new SourceLogicalSource(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path), layer));
            documents.Add(path, document);
        }
        return new SourceCommentScan(sources.OrderBy(source => source.Base.CanonicalPath, StringComparer.Ordinal).ToArray(), documents, complete)
        { TextFiles = textFiles };
    }

    // Without a workspace file list, every file outside .git is a candidate.
    private static (IReadOnlyList<string> Paths, bool Complete) Walk(string root, CancellationToken token)
    {
        var paths = new List<string>();
        var pending = new Stack<string>();
        pending.Push(root);
        var complete = true;
        while (pending.TryPop(out var directory))
        {
            token.ThrowIfCancellationRequested();
            FileSystemInfo[] entries;
            try { entries = new DirectoryInfo(directory).GetFileSystemInfos(); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { complete = false; continue; }
            foreach (var entry in entries)
            {
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                if (entry is DirectoryInfo)
                {
                    if (!entry.Name.Equals(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase)) pending.Push(entry.FullName);
                    continue;
                }
                paths.Add(Path.GetRelativePath(root, entry.FullName).Replace(Path.DirectorySeparatorChar, '/'));
            }
        }
        return (paths, complete);
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
        if (!SourceWorkspaceRelativePath.IsCanonical(path)) return null;
        var workspace = session.Catalogue.Workspace;
        var lexical = Path.Combine(workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar));
        var physical = session.DocumentReader.PhysicalPaths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexical);
        if (physical.State != PhysicalPathState.Contained || !File.Exists(physical.GetContainedPhysicalPath())) return null;
        return new SourceLogicalSource(new(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new(path, physical.GetContainedPhysicalPath(), SourceDocumentForm.Markdown, SourceLayerKind.Base));
    }
}
