namespace OpenForge.Cli.Core.Presentation.Shared.Text.Models;

internal sealed record CliAuthoredSpan(string Content);

internal sealed record CliTextSpan(string Content, bool Authored = false)
{
    internal static CliTextSpan FromAuthored(CliAuthoredSpan span) => new(span.Content, true);
}

internal sealed record CliTextDocument(IReadOnlyList<CliTextSpan> Spans)
{
    internal string Content => string.Concat(Spans.Select(span => span.Content));
}
