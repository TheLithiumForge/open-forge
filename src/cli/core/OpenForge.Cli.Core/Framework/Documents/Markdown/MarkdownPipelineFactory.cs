using Markdig;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownPipelineFactory
{
    private static readonly Lazy<MarkdownPipeline> Pipeline = new(
        static () => new MarkdownPipelineBuilder()
            .UsePreciseSourceLocation()
            .Build());

    internal static MarkdownPipeline Get()
    {
        return Pipeline.Value;
    }
}
