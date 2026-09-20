using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Rendering;

internal static class RouteRemoveDataTextRenderer
{
    internal static CliTextDocument Render(
        RouteRemoveData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        foreach (var row in data.TextRows)
        {
            builder.Append("  ")
                .Append(CliText.Escape(row))
                .Append('\n');
        }

        foreach (var line in data.TextDetailLines)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        if (data.ShowNoChanges)
        {
            builder.Append(CliText.Escape(global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged()))
                .Append('\n');
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
