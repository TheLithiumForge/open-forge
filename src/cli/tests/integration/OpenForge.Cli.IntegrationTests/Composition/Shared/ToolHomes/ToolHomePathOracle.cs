using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

// One independent oracle for the reviewed external storage layout. Other tests use this or production definitions.
internal static class ToolHomePathOracle
{
    internal static readonly IReadOnlyDictionary<string, string> FixedConfigurations = new Dictionary<string, string>
    {
        [Claude] = ".claude.json",
        [Codex] = ".codex/config.toml",
        [Gemini] = ".gemini/settings.json",
        [OpenCode] = ".config/opencode/opencode.json",
        [Antigravity] = ".gemini/config/mcp_config.json",
        [Kilo] = ".config/kilo/kilo.jsonc",
        [Cursor] = ".cursor/mcp.json",
        [Windsurf] = ".codeium/windsurf/mcp_config.json",
        [Augment] = ".augment/settings.json",
        [Kiro] = ".kiro/settings/mcp.json",
        [Junie] = ".junie/mcp/mcp.json",
        [OpenHands] = ".openhands/mcp.json",
        [Cline] = ".cline/mcp.json",
        [Qwen] = ".qwen/settings.json",
        [Copilot] = ".copilot/mcp-config.json",
        [Factory] = ".factory/mcp.json",
        [Crush] = ".config/crush/crush.json",
        [Vibe] = ".vibe/config.toml",
        [Grok] = ".grok/config.toml",
        [OpenClaw] = ".openclaw/openclaw.json",
        [Qoder] = ".qoder/settings.json",
        [Kimi] = ".kimi-code/mcp.json",
        [GitLab] = ".gitlab/duo/mcp.json",
        [Rovo] = ".rovodev/mcp.json",
        [Amp] = ".config/agents/skills/codebase-memory/mcp.json",
        [Devin] = ".config/devin/config.json",
        [Tabnine] = ".tabnine/mcp_servers.json",
        [AmazonQ] = ".aws/amazonq/default.json",
        [CodeBuddy] = ".codebuddy/.mcp.json",
        [BobIde] = ".bob/mcp.json",
        [BobShell] = ".bob/mcp_settings.json",
        [Pochi] = ".pochi/config.jsonc",
        [Omp] = ".omp/agent/mcp.json",
    };

    internal static readonly IReadOnlyDictionary<string, string[]> ExistingChoices = new Dictionary<string, string[]>
    {
        [OpenCode] = [".config/opencode/opencode.jsonc", ".config/opencode/opencode.json"],
        [Rovo] = [".rovodev/mcp.json", ".rovodev/mcp_config.json"],
        [AmazonQ] = [".aws/amazonq/default.json", ".aws/amazonq/agents/default.json", ".aws/amazonq/mcp.json"],
        [CodeBuddy] = [".codebuddy/.mcp.json", ".codebuddy/mcp.json", ".codebuddy.json"],
    };

    internal static IReadOnlyDictionary<string, string> PlatformConfigurations(ToolOperatingSystem os) => new Dictionary<string, string>
    {
        [Zed] = os switch
        {
            ToolOperatingSystem.Windows => "AppData/Roaming/Zed/settings.json",
            ToolOperatingSystem.MacOS => "Library/Application Support/Zed/settings.json",
            ToolOperatingSystem.Linux => ".config/zed/settings.json",
            _ => throw new ArgumentOutOfRangeException(nameof(os)),
        },
        [VsCode] = os switch
        {
            ToolOperatingSystem.Windows => "AppData/Roaming/Code/User/mcp.json",
            ToolOperatingSystem.MacOS => "Library/Application Support/Code/User/mcp.json",
            ToolOperatingSystem.Linux => ".config/Code/User/mcp.json",
            _ => throw new ArgumentOutOfRangeException(nameof(os)),
        },
        [Hermes] = os == ToolOperatingSystem.Windows ? "AppData/Local/hermes/config.yaml" : ".hermes/config.yaml",
        [Goose] = os == ToolOperatingSystem.Windows ? "AppData/Roaming/Block/goose/config/config.yaml" : ".config/goose/config.yaml",
    };

    internal static string[] RequiredKeys() => ToolHomeFixture.Catalogue.Entries.Values
        .SelectMany(entry => entry.Profiles).SelectMany(profile => new[] { profile.Setup, profile.Uninstall })
        .SelectMany(setup => setup.PossibleOutputs).Select(output => output.PathTemplate)
        .Where(path => path.StartsWith(ToolRecipeDefinitions.ClientTokenPrefix, StringComparison.Ordinal))
        .Select(path => path[ToolRecipeDefinitions.ClientTokenPrefix.Length..path.IndexOf('}')])
        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    internal static string ConfigurationKey(string id) => CodebaseMemoryNativeOutputs.ConfigurationRootKey(id);
}
