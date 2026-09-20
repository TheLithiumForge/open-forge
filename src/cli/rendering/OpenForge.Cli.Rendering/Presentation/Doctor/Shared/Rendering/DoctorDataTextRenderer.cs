using System.Text;
using OpenForge.Cli.Core.Presentation.Doctor.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;

internal static class DoctorDataTextRenderer
{
    internal static CliTextDocument Render(DoctorData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);
        var builder = new StringBuilder();
        Append(builder, data.ChecksLine);
        Append(builder, data.CoverageLine);
        foreach (var line in data.IncompleteLines)
        {
            Append(builder, line);
        }

        Append(builder, data.LaneLine);
        Append(builder, data.HintLine);
        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void Append(StringBuilder builder, string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        }
    }
}
