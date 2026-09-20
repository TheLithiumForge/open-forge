using Markdig;
using Markdig.Extensions.AutoIdentifiers;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownPipelineFactory
{
    private static readonly Lazy<MarkdownPipeline> Pipeline = new(
        static () => new MarkdownPipelineBuilder()
            .UsePreciseSourceLocation()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .Build());

    internal static MarkdownPipeline Get()
    {
        return Pipeline.Value;
    }
}
