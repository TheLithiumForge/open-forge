using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Shared.Planning;

internal sealed record StripInventory(ImmutableArray<string> Documents, ImmutableArray<string> Linked);

/// <summary>
/// Lists the Markdown files below the layout's inventory roots in ordinal order,
/// without entering Git metadata, runtime storage or any symbolic link.
/// </summary>
internal static class StripDocumentInventory
{
    internal static StripInventory Read(WorkspaceLayout layout, CancellationToken token)
    {
        var documents = new SortedSet<string>(StringComparer.Ordinal);
        var linked = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var root in layout.InventoryRoots.Distinct(StringComparer.Ordinal))
        {
            var directory = new DirectoryInfo(SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, root));
            if (!directory.Exists)
            {
                continue;
            }

            if (directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                linked.Add(root);
                continue;
            }

            Walk(directory, root, documents, linked, token);
        }

        return new StripInventory([.. documents], [.. linked]);
    }

    internal static bool IsSkippedDirectory(string name)
        => string.Equals(name, WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, WorkspaceRuntimePaths.DirectoryName, StringComparison.OrdinalIgnoreCase);

    private static void Walk(DirectoryInfo directory, string path, SortedSet<string> documents, SortedSet<string> linked, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            var child = $"{path}/{entry.Name}";
            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                linked.Add(child);
            }
            else if (entry is DirectoryInfo nested)
            {
                if (!IsSkippedDirectory(entry.Name))
                {
                    Walk(nested, child, documents, linked, token);
                }
            }
            else if (entry.Name.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.Ordinal))
            {
                documents.Add(child);
            }
        }
    }
}
