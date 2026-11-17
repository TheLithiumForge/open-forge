using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;

/// <summary>
/// Removes the whole frontmatter block, whoever wrote its keys. The strip Interface
/// contract's frontmatter part defines the boundary.
/// </summary>
internal static class StripFrontmatterEdits
{
    private const char ByteOrderMark = '﻿';

    internal static StripDocumentEdits Read(string path, MarkdownDocumentFacts facts)
    {
        if (SourceLogicalPath.ReadFileName(path) == SourceFormClassifier.NativeSkillFileName
            || facts.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return StripDocumentEdits.None;
        }

        if (facts.Frontmatter is not { State: MarkdownFrontmatterState.Complete, BlockSpan: { } block, BodyStart: { } bodyStart })
        {
            return StripDocumentEdits.Refuse("The frontmatter has no closing --- line.");
        }

        return new StripDocumentEdits([WholeBlock(facts.Source, block.Start, bodyStart, StripRewritePart.Frontmatter)]);
    }

    // The block, its delimiters and the line ending after the closing ---. A byte order mark stays at the start.
    internal static StripTextEdit WholeBlock(string source, int blockStart, int bodyStart, StripRewritePart part)
        => new(source.Length > 0 && source[0] == ByteOrderMark ? 1 : blockStart, bodyStart, string.Empty, part);
}
