using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

// These names belong to the reviewed CBM v0.12.1 path resolvers, not client discovery.
internal sealed class ToolHomeEnvironment
{
    internal const string Home = "HOME";
    internal const string UserProfile = "USERPROFILE";
    internal const string AppData = "APPDATA";
    internal const string XdgConfigHome = "XDG_CONFIG_HOME";
    internal const string ClaudeConfigDirectory = "CLAUDE_CONFIG_DIR";
    internal const string CodexHome = "CODEX_HOME";
    internal const string KiroHome = "KIRO_HOME";
    internal const string HermesHome = "HERMES_HOME";
    internal const string QwenHome = "QWEN_HOME";
    internal const string ClineDataDirectory = "CLINE_DATA_DIR";
    internal const string CopilotHome = "COPILOT_HOME";
    internal const string VibeHome = "VIBE_HOME";
    internal const string GrokHome = "GROK_HOME";
    internal const string OpenCodeConfig = "OPENCODE_CONFIG";
    internal const string CrushConfig = "CRUSH_GLOBAL_CONFIG";
    internal const string GlabConfigDirectory = "GLAB_CONFIG_DIR";
    internal const string KimiHome = "KIMI_CODE_HOME";
    internal const string ContinueConfig = "CBM_CONTINUE_CONFIG_PATH";
    internal const string TraeConfig = "CBM_TRAE_CONFIG_PATH";
    internal const string RooConfig = "CBM_ROO_CONFIG_PATH";
    internal const string CodyConfig = "CBM_CODY_CONFIG_PATH";
    internal const string PiAgentDirectory = "PI_CODING_AGENT_DIR";
    internal const string OmpProfile = "OMP_PROFILE";
    internal const string OpenClawHome = "OPENCLAW_HOME";
    internal const string OpenClawState = "OPENCLAW_STATE_DIR";
    internal const string OpenClawConfig = "OPENCLAW_CONFIG_PATH";
    internal const string OpenClawWorkspace = "OPENCLAW_WORKSPACE_DIR";
    internal const string OpenClawProfile = "OPENCLAW_PROFILE";
    internal const string CacheDirectory = "CBM_CACHE_DIR";
    internal const string RuntimeDirectory = "CBM_RUNTIME_DIR";

    internal ToolHomeEnvironment(IReadOnlyDictionary<string, string> values, ToolOperatingSystem operatingSystem)
    {
        var comparer = operatingSystem == ToolOperatingSystem.Windows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        Values = values.ToImmutableDictionary(comparer);
    }

    internal ImmutableDictionary<string, string> Values { get; }
    internal string? Nonempty(string name) => Values.GetValueOrDefault(name) is { Length: > 0 } value ? value : null;

    internal string? Expanded(string name, string home)
        => Nonempty(name) is { } value ? Expand(value, home) : null;

    // CBM expands only ~, ~/... and ~\..., and otherwise requires an absolute path.
    internal static string? Expand(string value, string home)
    {
        if (value == "~") return home;
        if (value.StartsWith("~/", StringComparison.Ordinal) || value.StartsWith("~\\", StringComparison.Ordinal))
            return ToolHomePaths.Join(home, value[2..]);
        return Path.IsPathFullyQualified(value) ? value : null;
    }

    internal static bool IsProfile(string value) => value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');

    internal static string Profile(string value)
    {
        if (!IsProfile(value))
            throw new IOException("The native profile requires letters, digits, '-' or '_'.");
        return value;
    }
}
