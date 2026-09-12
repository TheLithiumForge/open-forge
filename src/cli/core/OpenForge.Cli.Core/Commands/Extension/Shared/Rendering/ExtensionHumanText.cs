using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;

internal static class ExtensionHumanText
{
    internal static string Value(string? value) => CliHumanText.Text(value ?? "unavailable");

    internal static string Count(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "unknown";

    internal static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values.Select(Value));

    internal static void AppendFinding(StringBuilder builder, CliSemanticStatus status, string code, string cause, string? target)
    {
        builder.AppendLine($"{CliHumanText.Status(status).ToUpperInvariant()}: {Value(cause)} [{code}]");
        if (target is not null)
        {
            builder.AppendLine($"  Target: {Value(target)}");
        }
    }
}
