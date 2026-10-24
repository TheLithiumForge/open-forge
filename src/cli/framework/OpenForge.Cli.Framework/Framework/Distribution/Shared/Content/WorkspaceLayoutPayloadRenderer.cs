using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

internal static class WorkspaceLayoutPayloadRenderer
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private const string ClaudeImportPrefix = "@";
    private const string WorkspaceRootPath = ".";

    internal static FrameworkFrontmatterTransformResult Render(
        WorkspaceLayout layout, string packagePath, ReadOnlyMemory<byte> sourceBytes)
    {
        if (layout.IsDefault || !packagePath.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.Ordinal))
            return FrameworkFrontmatterTransformResult.Unchanged(sourceBytes);

        string source;
        try
        {
            source = StrictUtf8.GetString(sourceBytes.Span);
        }
        catch (DecoderFallbackException)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The Markdown payload is not valid UTF-8.");
        }

        var document = new MarkdownDocumentParser().Parse(source);
        var replacements = new Dictionary<MarkdownTextSpan, string>();
        foreach (var link in document.Links.Concat(document.Images))
        {
            if (link.DestinationSpan is { } span
                && RebaseDestination(layout, packagePath, link.RawDestination) is { } rebased)
                replacements[span] = rebased;
        }

        AddHostReferences(layout, packagePath, document, replacements);
        WorkspaceLayoutPayloadGuidance.AddReplacements(layout, packagePath, sourceBytes, document, replacements);
        if (replacements.Count == 0)
            return FrameworkFrontmatterTransformResult.Unchanged(sourceBytes);

        var rendered = new StringBuilder(source);
        foreach (var (span, value) in replacements.OrderByDescending(pair => pair.Key.Start))
            rendered.Remove(span.Start, span.Length).Insert(span.Start, value);
        return FrameworkFrontmatterTransformResult.Changed(StrictUtf8.GetBytes(rendered.ToString()));
    }

    internal static string? RebaseDestination(WorkspaceLayout layout, string packagePath, string destination)
    {
        var hash = destination.IndexOf('#');
        var path = hash < 0 ? destination : destination[..hash];
        if (path.Length == 0 || path.Contains('?') || path.Contains(':') || path.Contains('\\') || path.StartsWith('/'))
            return null;

        // Parsed angle destinations can contain literal whitespace. Encode it for the shared strict decoder.
        var encodedWhitespace = string.Concat(path.Select(character => char.IsWhiteSpace(character)
            ? Uri.EscapeDataString(character.ToString()) : character.ToString()));
        if (!SourceDestinationDecoder.TryDecode(encodedWhitespace, out var decoded, out _)) return null;
        if (decoded.Contains('\\') || decoded.Contains(':') || decoded.StartsWith('/')) return null;

        var resolved = SourceLinkLexicalPathResolver.Resolve(layout.Workspace.LexicalRoot, packagePath, decoded);
        if (resolved.CanonicalPath is not { } target
            || target != WorkspaceRootPath && !PortableWorkspacePath.TryNormalize(target, out _))
            return null;

        var mappedSource = layout.MapPayloadPath(packagePath);
        var mappedTarget = target;
        if (target == WorkspaceLayout.DefaultMainRoot) mappedTarget = layout.MainRoot;
        else if (target != WorkspaceRootPath) mappedTarget = layout.MapPayloadPath(target);
        var oldRelative = RelativePath(layout, packagePath, target);
        var newRelative = RelativePath(layout, mappedSource, mappedTarget);
        if (oldRelative == newRelative) return null;

        var suffix = hash < 0 ? string.Empty : destination[hash..];
        var trailingSlash = path.EndsWith('/') ? "/" : string.Empty;
        return SourceDestinationEncoder.EncodePath(newRelative) + trailingSlash + suffix;
    }

    private static string RelativePath(WorkspaceLayout layout, string sourcePath, string targetPath)
    {
        var source = SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, sourcePath);
        var target = targetPath == WorkspaceRootPath ? layout.Workspace.LexicalRoot
            : SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, targetPath);
        var directory = Path.GetDirectoryName(source)
            ?? throw new InvalidOperationException("A payload source must have a workspace directory.");
        return Path.GetRelativePath(directory, target).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static void AddHostReferences(
        WorkspaceLayout layout, string packagePath, MarkdownDocumentFacts document,
        IDictionary<MarkdownTextSpan, string> replacements)
    {
        if (packagePath is not (FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath)
            || document.ManagedHost.Span is not { } managed)
            return;

        foreach (var code in document.OpaqueSpans.Where(code => code.IsCode))
        {
            var span = code.Span;
            if (span.Start >= managed.Start && span.End <= managed.End
                && document.Source[span.Start..span.End] == $"`{FrameworkPayloadAsset.LoaderPath}`")
                replacements[span] = $"`{layout.LoaderPath}`";
        }

        if (packagePath != FrameworkPayloadAsset.RootClaudePath) return;
        foreach (var visible in document.VisibleText)
        {
            var span = visible.Span;
            if (span.Start >= managed.Start && span.End <= managed.End
                && document.Source[span.Start..span.End] == ClaudeImportPrefix + FrameworkPayloadAsset.LoaderPath)
                replacements[span] = ClaudeImportPrefix + layout.LoaderPath;
        }
    }
}
