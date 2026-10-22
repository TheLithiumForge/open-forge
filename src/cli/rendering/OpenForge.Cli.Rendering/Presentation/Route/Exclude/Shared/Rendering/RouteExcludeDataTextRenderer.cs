using OpenForge.Cli.Core.Presentation.Route.Exclude.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Rendering;

internal static class RouteExcludeDataTextRenderer
{
    internal static CliTextDocument Render(RouteExcludeData data, CliSelection selection, CliTextStyle style)
    {
        var spans = new List<CliTextSpan>
        {
            new("  " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.Before(data.Before ?? global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.NoRow())) + "\n"),
            new("  " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.After(data.After ?? global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.NoRow())) + "\n"),
        };
        if (data.Mode == "dry-run") spans.Add(new CliTextSpan(global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.DryRun() + "\n"));
        return new CliTextDocument(spans);
    }
}
