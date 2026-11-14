using System.Collections.Immutable;
using OpenForge.Cli.Composition.Shared.ToolHomes.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

internal sealed partial class ToolClientRootResolver
{
    private static readonly ImmutableArray<ToolClientRootRecipe> BuiltInDirectoryRecipes =
    [
        new(Claude, true, static roots => roots.RawOrHome(ToolHomeEnvironment.ClaudeConfigDirectory, ".claude")),
        new(ClaudeUserRoot, true, static roots => roots.RawOrHome(ToolHomeEnvironment.ClaudeConfigDirectory, string.Empty)),
        new(Codex, true, static roots => roots.RawOrHome(ToolHomeEnvironment.CodexHome, ".codex")),
        new(Kiro, true, static roots => roots.ExpandedOrHome(ToolHomeEnvironment.KiroHome, ".kiro")),
        new(Hermes, true, static roots => roots.HermesDirectory()),
        new(Qwen, true, static roots => roots.ExpandedOrHome(ToolHomeEnvironment.QwenHome, ".qwen")),
        new(Copilot, true, static roots => roots.RawOrHome(ToolHomeEnvironment.CopilotHome, ".copilot")),
        new(Vibe, true, static roots => roots.RawOrHome(ToolHomeEnvironment.VibeHome, ".vibe")),
        new(Grok, true, static roots => roots.RawOrHome(ToolHomeEnvironment.GrokHome, ".grok")),
        new(ClineDataRoot, true, static roots => roots.ExpandedOrHome(ToolHomeEnvironment.ClineDataDirectory, ".cline/data")),
        new(SharedSkillRoot, true, static roots => roots.Home(".agents/skills")),
        new(VsCode, true, static roots => roots.CodeUserDirectory()),
    ];

    private static readonly ImmutableArray<ToolClientRootRecipe> BuiltInFileRecipes =
    [
        new(ConfigKey(Claude), false, static roots => roots.DirectoryFile(ClaudeUserRoot, ".claude.json")),
        new(ConfigKey(Codex), false, static roots => roots.DirectoryFile(Codex, "config.toml")),
        new(ConfigKey(Gemini), false, static roots => roots.Home(".gemini/settings.json")),
        new(ConfigKey(Zed), false, static roots => ToolHomePaths.Join(roots.ZedDirectory(), "settings.json")),
        new(ConfigKey(OpenCode), false, static roots => roots.OpenCodeConfiguration()),
        new(ConfigKey(Antigravity), false, static roots => roots.Home(".gemini/config/mcp_config.json")),
        new(ConfigKey(Kilo), false, static roots => roots.Home(".config/kilo/kilo.jsonc")),
        new(ConfigKey(VsCode), false, static roots => roots.DirectoryFile(VsCode, "mcp.json")),
        new(ConfigKey(Cursor), false, static roots => roots.Home(".cursor/mcp.json")),
        new(ConfigKey(Windsurf), false, static roots => roots.Home(".codeium/windsurf/mcp_config.json")),
        new(ConfigKey(Augment), false, static roots => roots.Home(".augment/settings.json")),
        new(ConfigKey(Kiro), false, static roots => roots.DirectoryFile(Kiro, "settings/mcp.json")),
        new(ConfigKey(Junie), false, static roots => roots.Home(".junie/mcp/mcp.json")),
        new(ConfigKey(Hermes), false, static roots => roots.DirectoryFile(Hermes, "config.yaml")),
        new(ConfigKey(OpenHands), false, static roots => roots.Home(".openhands/mcp.json")),
        new(ConfigKey(Cline), false, static roots => roots.Home(".cline/mcp.json")),
        new(ConfigKey(Qwen), false, static roots => roots.DirectoryFile(Qwen, "settings.json")),
        new(ConfigKey(Copilot), false, static roots => roots.DirectoryFile(Copilot, "mcp-config.json")),
        new(ConfigKey(Factory), false, static roots => roots.Home(".factory/mcp.json")),
        new(ConfigKey(Crush), false, static roots => roots.RawOrHome(ToolHomeEnvironment.CrushConfig, ".config/crush/crush.json")),
        new(ConfigKey(Goose), false, static roots => roots.GooseConfiguration()),
        new(ConfigKey(Vibe), false, static roots => roots.DirectoryFile(Vibe, "config.toml")),
        new(ConfigKey(Grok), false, static roots => roots.DirectoryFile(Grok, "config.toml")),
        new(ConfigKey(OpenClaw), false, static roots => roots.OpenClawConfiguration()),
        new(OpenClawWorkspaceRoot, true, static roots => roots.OpenClawWorkspace()),
    ];

    private string GooseConfiguration() => Home(operatingSystem == ToolOperatingSystem.Windows
        ? "AppData/Roaming/Block/goose/config/config.yaml" : ".config/goose/config.yaml");
    private string OpenClawConfiguration() => ToolOpenClawRoots.Configuration(environment, home);
    private string OpenClawWorkspace() => ToolOpenClawRoots.Workspace(environment, home, FilePath(ConfigKey(OpenClaw)));

    private string OpenCodeConfiguration()
    {
        if (environment.Nonempty(ToolHomeEnvironment.OpenCodeConfig) is { } custom) return custom;
        var jsonc = Home(".config/opencode/opencode.jsonc");
        return ToolHomePaths.Exists(jsonc) ? jsonc : Home(".config/opencode/opencode.json");
    }

    private string HermesDirectory()
    {
        if (environment.Expanded(ToolHomeEnvironment.HermesHome, home) is { } custom) return custom;
        var fallback = ".hermes";
        if (operatingSystem == ToolOperatingSystem.Windows)
        {
            var native = ToolHomePaths.Resolve(Home("AppData/Local/hermes"), directory: true);
            var legacy = ToolHomePaths.Resolve(Home(fallback), directory: true);
            if (System.IO.Directory.Exists(native) || !System.IO.Directory.Exists(legacy)) fallback = "AppData/Local/hermes";
        }
        return ExpandedOrHome(ToolHomeEnvironment.HermesHome, fallback);
    }

    private string CodeUserDirectory()
    {
        if (operatingSystem == ToolOperatingSystem.MacOS) return Home("Library/Application Support/Code/User");
        var root = operatingSystem == ToolOperatingSystem.Windows
            ? RawOrHome(ToolHomeEnvironment.AppData, "AppData/Roaming")
            : RawOrHome(ToolHomeEnvironment.XdgConfigHome, ".config");
        return ToolHomePaths.Join(root, "Code/User");
    }

    private string ZedDirectory() => operatingSystem switch
    {
        ToolOperatingSystem.Windows => Home("AppData/Roaming/Zed"),
        ToolOperatingSystem.MacOS => Home("Library/Application Support/Zed"),
        ToolOperatingSystem.Linux => ToolHomePaths.Join(RawOrHome(ToolHomeEnvironment.XdgConfigHome, ".config"), "zed"),
        _ => throw new ArgumentOutOfRangeException(nameof(operatingSystem)),
    };
}
