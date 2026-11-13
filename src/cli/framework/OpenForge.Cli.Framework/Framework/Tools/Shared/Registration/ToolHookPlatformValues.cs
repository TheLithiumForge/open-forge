using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static class ToolHookPlatformValues
{
    internal static T Resolve<T>(T fallback, Dictionary<string, T>? overrides, IReadOnlyDictionary<string, string> values)
    {
        if (overrides is null) return fallback;
        if (overrides.Keys.Any(key => key is not (ToolHookPlatforms.Windows or ToolHookPlatforms.MacOS or ToolHookPlatforms.Linux)))
            throw new InvalidDataException("A hook platform override is unknown.");
        if (!values.TryGetValue(ToolVocabulary.TargetVariable, out var target) || !ToolVocabulary.Targets.Contains(target))
            throw new InvalidDataException("Hook platform overrides require the host's exact supported target.");
        var platform = Platform(target);
        if (!overrides.TryGetValue(platform, out var value)) return fallback;
        if (value is null) throw new InvalidDataException("A hook platform override cannot be null.");
        return value;
    }

    private static string Platform(string target)
    {
        if (target.StartsWith("win-", StringComparison.Ordinal)) return ToolHookPlatforms.Windows;
        if (target.StartsWith("osx-", StringComparison.Ordinal)) return ToolHookPlatforms.MacOS;
        if (target.StartsWith("linux-", StringComparison.Ordinal)) return ToolHookPlatforms.Linux;
        throw new InvalidDataException("The target has no supported hook platform.");
    }
}
