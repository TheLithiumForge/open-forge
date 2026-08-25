namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownFrontmatterState
{
    Missing,
    Complete,
    Unavailable,
}

internal sealed record MarkdownFrontmatterBoundary
{
    internal MarkdownFrontmatterBoundary(
        MarkdownFrontmatterState state,
        MarkdownTextSpan? blockSpan,
        MarkdownTextSpan? yamlSpan,
        int? bodyStart)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown frontmatter state is not defined.");
        }

        if (state == MarkdownFrontmatterState.Missing
            && (blockSpan is not null || yamlSpan is not null || bodyStart != 0)
            || state == MarkdownFrontmatterState.Complete
                && (blockSpan is null || yamlSpan is null || bodyStart is null)
            || state == MarkdownFrontmatterState.Unavailable
                && (blockSpan is not null || yamlSpan is not null || bodyStart is not null))
        {
            throw new ArgumentException("The Markdown frontmatter boundary facts do not match their state.");
        }

        if (blockSpan is not null && yamlSpan is not null
            && (yamlSpan.Start < blockSpan.Start || yamlSpan.End > blockSpan.End))
        {
            throw new ArgumentException("The YAML span must be contained by the complete frontmatter block.", nameof(yamlSpan));
        }

        if (blockSpan is not null && bodyStart is { } completeBodyStart && completeBodyStart < blockSpan.End)
        {
            throw new ArgumentException("The body must begin at or after the frontmatter block.", nameof(bodyStart));
        }

        State = state;
        BlockSpan = blockSpan;
        YamlSpan = yamlSpan;
        BodyStart = bodyStart;
    }

    internal MarkdownFrontmatterState State { get; }

    internal MarkdownTextSpan? BlockSpan { get; }

    internal MarkdownTextSpan? YamlSpan { get; }

    internal int? BodyStart { get; }
}
