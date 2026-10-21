using OpenForge.Cli.Core.Presentation.Help.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Help.Shared.Rendering;

internal static class HelpDataTextRenderer
{
    internal static CliTextDocument Render(HelpData data, CliSelection selection, CliTextStyle style)
    {
        if (!data.Complete) return new CliTextDocument([]);
        if (data.Markdown is { } markdown)
            return new CliTextDocument([new CliTextSpan(markdown, Authored: true)]);
        var width = data.Topics.Max(topic => topic.Length);
        var topics = data.Topics.Select(topic =>
        {
            var summary = global::OpenForge.Cli.OutputText.Help.HelpText.TopicSummary(topic);
            return $"  {topic.PadRight(width)}  {summary}".TrimEnd();
        });
        var text = global::OpenForge.Cli.OutputText.Help.HelpText.TopicsHeading() + "\n"
            + string.Join('\n', topics) + "\n\n" + global::OpenForge.Cli.OutputText.Help.HelpText.CommandOptions();
        return new CliTextDocument([new CliTextSpan(text)]);
    }
}
