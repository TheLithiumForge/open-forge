using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Templates;

// The tool-manifest contract defines substitutions shared by tool commands and instructions.
internal static partial class ToolTemplate
{
    internal static void Validate(string value)
    {
        ToolManifestValidator.Require(value is not null && !value.Contains('\0'), "invalid template string");
        foreach (Match match in Variables().Matches(value))
            ToolManifestValidator.Require(ToolVocabulary.Variables.Contains(match.Groups[1].Value), "unknown template variable");
        ToolManifestValidator.Require(!Variables().Replace(value, "").Contains("${", StringComparison.Ordinal), "unfinished template variable");
    }

    internal static string Render(string value, IReadOnlyDictionary<string, string?> values)
    {
        Validate(value);
        return Variables().Replace(value, match =>
        {
            var name = match.Groups[1].Value;
            if (!values.TryGetValue(name, out var resolved) || resolved is null)
                throw new InvalidOperationException($"The tool template value {name} is unavailable.");
            if (resolved.Contains('\0')) throw new InvalidOperationException($"The tool template value {name} contains a null character.");
            return resolved;
        });
    }

    [GeneratedRegex(@"\$\{([^}]*)\}", RegexOptions.CultureInvariant)]
    private static partial Regex Variables();
}
