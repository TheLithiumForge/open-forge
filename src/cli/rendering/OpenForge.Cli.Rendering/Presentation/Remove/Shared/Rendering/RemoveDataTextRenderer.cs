using OpenForge.Cli.Core.Presentation.Remove.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.OutputText.Remove;

namespace OpenForge.Cli.Core.Presentation.Remove.Shared.Rendering;

internal static class RemoveDataTextRenderer
{
    internal static CliTextDocument Render(RemoveData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var lines = data.Effects
            .Where(effect => data.ShowEffects || effect.Outcome != "planned")
            .Select(effect => RemoveText.Effect(
                effect.Kind,
                effect.Action,
                effect.Outcome,
                CliText.Escape(effect.Path)))
            .ToArray();
        return lines.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(string.Join(Environment.NewLine, lines) + Environment.NewLine)]);
    }
}
