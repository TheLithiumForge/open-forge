using OpenForge.Cli.Core.Presentation.Route.Include.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Include.Shared.Rendering;

internal static class RouteIncludeDataTextRenderer
{
    internal static CliTextDocument Render(RouteIncludeData data, CliSelection selection, CliTextStyle style)
    {
        var spans = new List<CliTextSpan>
        {
            new("  " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.Before(data.Before ?? global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.NoRow())) + "\n"),
            new("  " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.After(data.After ?? global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.NoRow())) + "\n"),
        };
        if (data.Catalogue is { } catalogue) spans.Insert(0, new CliTextSpan("  " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.Catalogue(catalogue)) + "\n"));
        if (data.Mode == "dry-run") spans.Add(new CliTextSpan(global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.DryRun() + "\n"));
        return new CliTextDocument(spans);
    }
}
