using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Move.Models;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Rendering;

internal static class RouteMoveDataTextRenderer
{
    internal static CliTextDocument Render(
        RouteMoveData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        foreach (var row in data.TextRows)
        {
            builder.Append("  ").Append(CliText.Escape(row)).Append('\n');
        }

        if (data.Mode == "dry-run")
        {
            builder.Append(CliText.Escape(RouteMoveWording.DryRunComplete())).Append('\n');
        }

        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
