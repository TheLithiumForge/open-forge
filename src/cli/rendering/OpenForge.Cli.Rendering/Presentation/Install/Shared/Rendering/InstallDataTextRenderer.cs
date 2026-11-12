using System.Text;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.OutputText.Install;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;

internal static class InstallDataTextRenderer
{
    private const string ProseContinuationIndent = "      ";

    internal static CliTextDocument Render(InstallData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        if (data.Layout is { } layout)
            spans.Add(new CliTextSpan(global::OpenForge.Cli.OutputText.Shared.WorkspaceLayoutText.Describe(CliText.Escape(layout.MainRoot), CliText.Escape(layout.LoaderPath)) + "\n"));
        if (data.TextReviewHeadline is { } headline)
        {
            spans.Add(new CliTextSpan($"{InstallSetupText.ReviewCaption()}\n"));
            foreach (var line in CliSelectionText.Wrap(headline, style.Width))
                spans.Add(new CliTextSpan($"{style.Status(line, data.Status)}\n"));
        }
        if (data.TextReviewWorkspace is { } workspace)
            spans.Add(new CliTextSpan($"Workspace: {CliText.Escape(workspace)}\n"));
        foreach (var text in data.TextSetupLines)
            foreach (var line in CliSelectionText.Wrap(text, style.Width)) spans.Add(new CliTextSpan($"{line}\n"));
        if (data.InvalidRoute is { } route)
        {
            foreach (var text in new[]
            {
                global::OpenForge.Cli.OutputText.Install.InstallInputText.RouteForms(),
                global::OpenForge.Cli.OutputText.Install.InstallInputText.RouteIds(route.BuiltInIds),
            })
                foreach (var line in CliSelectionText.Wrap(text, style.Width)) spans.Add(new CliTextSpan(line + "\n"));
        }
        if (data.Frontmatter is { } form && data.ShowFrontmatter)
        {
            var line = form.PreviousForm is { } previous
                ? InstallSetupText.FrontmatterChange(previous, form.Form)
                : InstallSetupText.Frontmatter(form.Form);
            spans.Add(new CliTextSpan($"{CliText.Escape(line)}\n"));
            if (form.KeptCount > 0)
                spans.Add(new CliTextSpan($"{InstallSetupText.FrontmatterKept(form.KeptCount)}\n"));
            if (form.TextKept.Count > 0)
                spans.Add(new CliTextSpan(CliTable.Render(style, form.TextKept.Select(file => (IReadOnlyList<string>)[file.Path, file.Reason]).ToArray(),
                    (column, cell) => column == 0 ? style.Subject(cell) : cell)));
        }
        var pathOnlyRows = data.TextRows.Where(row => row.Wording.Length == 0).ToArray();
        if (pathOnlyRows.Length > 0)
        {
            var paths = new StringBuilder();
            foreach (var row in pathOnlyRows)
                paths.Append("  ").Append(style.Subject(CliText.Escape(row.Path))).Append('\n');
            spans.Add(new CliTextSpan(paths.ToString()));
        }

        var rowsWithWording = data.TextRows.Where(row => row.Wording.Length > 0).ToArray();
        if (rowsWithWording.Length > 0)
        {
            var rows = rowsWithWording
                .Select(row => (IReadOnlyList<string>)[row.Path, row.Wording])
                .ToArray();
            spans.Add(new CliTextSpan(CliTable.Render(style, rows, (column, cell) =>
                column == 0 ? style.Subject(cell) : cell)));
        }

        var builder = new StringBuilder();
        foreach (var text in data.TextSummaryLines.Concat(data.TextDetailLines))
            foreach (var line in CliSelectionText.Wrap(text, style.Width - 2)) builder.AppendLine($"  {line}");
        foreach (var text in data.TextNoticeLines)
            foreach (var line in CliSelectionText.Wrap(text, style.Width)) builder.AppendLine(line);
        if (data.ShowNoChanges)
            builder.Append(OpenForge.Cli.Core.Presentation.Install.Shared.Wording.InstallWording.NoChanges()).Append('\n');
        if (data.ShowFirstTask)
            foreach (var text in InstallSetupText.FirstTask().Split('\n'))
            {
                // Only a Next line hangs its continuation under the command text. Plain sentences wrap flush.
                var indent = text.StartsWith("Next: ", StringComparison.Ordinal) ? ProseContinuationIndent : string.Empty;
                var lines = CliSelectionText.Wrap(text, style.Width - indent.Length);
                for (var index = 0; index < lines.Count; index++)
                    builder.AppendLine(index == 0 ? lines[index] : $"{indent}{lines[index]}");
            }
        if (builder.Length > 0)
            spans.Add(new CliTextSpan(builder.ToString()));

        return new CliTextDocument(spans);
    }
}
