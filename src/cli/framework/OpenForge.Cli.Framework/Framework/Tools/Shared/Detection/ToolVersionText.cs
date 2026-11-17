using System.Text.RegularExpressions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Detection;

/// <summary>Reads the first dotted version number a tool prints for --version, such as 1.6.2 from "codegraph v1.6.2".</summary>
internal static partial class ToolVersionText
{
    internal static string? Read(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var match = Version().Match(output);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\d+\.\d+(\.\d+)?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Version();
}
