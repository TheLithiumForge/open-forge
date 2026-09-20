using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering;

internal static class CliDiagnosticRenderer
{
    internal const int ValueLimit = 240;

    internal static string? Render(IReadOnlyList<string> diagnostics, CliDetail detail)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (detail != CliDetail.Debug || diagnostics.Count == 0)
        {
            return null;
        }

        var values = diagnostics.Select(value => CliText.Clamp(CliText.Escape(value), ValueLimit));
        var content = CliText.Clamp(string.Join("\n", values), CliPresentationDefinitions.MaximumDiagnosticLength - Environment.NewLine.Length);
        var framingExpansion = content.Count(character => character == '\n') * (Environment.NewLine.Length - 1);
        return CliText.Clamp(content, CliPresentationDefinitions.MaximumDiagnosticLength - Environment.NewLine.Length - framingExpansion);
    }
}
