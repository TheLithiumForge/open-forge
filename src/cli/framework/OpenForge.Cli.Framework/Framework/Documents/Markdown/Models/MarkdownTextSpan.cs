namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal sealed record MarkdownTextSpan
{
    internal MarkdownTextSpan(int start, int length)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "A Markdown span start cannot be negative.");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "A Markdown span length cannot be negative.");
        }

        _ = checked(start + length);
        Start = start;
        Length = length;
    }

    internal int Start { get; }

    internal int Length { get; }

    internal int End => checked(Start + Length);
}
