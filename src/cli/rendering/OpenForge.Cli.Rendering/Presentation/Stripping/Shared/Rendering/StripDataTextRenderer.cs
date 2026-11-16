using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Strip;

namespace OpenForge.Cli.Core.Presentation.Stripping.Shared.Rendering;

internal static class StripDataTextRenderer
{
    private const string DryRunMode = "dry-run";

    internal static CliTextDocument Render(StripData data, CliSelection selection, CliTextStyle style)
    {
        var text = new StringBuilder();
        foreach (var entry in data.Plan)
        {
            var detail = entry.Parts.Count == 0 ? entry.Reason : string.Join(", ", entry.Parts);
            text.AppendLine(StripText.PlanLine(entry.Action, CliText.Escape(entry.Path), entry.To is null ? null : CliText.Escape(entry.To), detail));
            if (selection.Detail >= CliDetail.Full && (entry.Before is not null || entry.After is not null))
            {
                text.AppendLine(StripText.HashLine(entry.Before, entry.After));
            }
        }

        if (data.Kept.Count > 0)
        {
            text.AppendLine(StripText.KeptHeading());
            foreach (var kept in data.Kept)
            {
                text.AppendLine(StripText.KeptLine(CliText.Escape(kept.Path), kept.Reason));
            }
        }

        if (data.Collisions.Count > 0)
        {
            text.AppendLine(StripText.CollisionsHeading());
            foreach (var collision in data.Collisions)
            {
                text.AppendLine(StripText.CollisionLine(CliText.Escape(collision.Path), string.Join(", ", collision.Sources.Select(CliText.Escape))));
            }
        }

        if (data.Mode == DryRunMode && data.Plan.Count > 0)
        {
            text.AppendLine(StripText.NoFilesChanged());
        }

        return text.Length == 0 ? new CliTextDocument([]) : new CliTextDocument([new CliTextSpan(text.ToString().Replace("\r\n", "\n", StringComparison.Ordinal))]);
    }
}
