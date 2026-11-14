using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryNativeOutputs
{
    internal const string ConfigurationRootSuffix = "-config";

    // Directory keys name native profile roots. The -config keys name exact native-selected files.
    // Root supplies these bindings from the native overrides and discovery, never from Open Forge MCP edits.
    internal static string Root(string key) => $"{ToolRecipeDefinitions.ClientTokenPrefix}{key}}}";
    internal static string ConfigurationRootKey(string clientId) => $"{clientId}{ConfigurationRootSuffix}";
    internal static string Configuration(string clientId) => Root(ConfigurationRootKey(clientId));
    internal static string Home(string suffix) => $"{ToolRecipeDefinitions.NativeHomeToken}/{suffix}";

    internal static ImmutableArray<ToolNativeOutputSpec> Create(bool uninstall)
    {
        var outputs = ImmutableArray.CreateBuilder<ToolNativeOutputSpec>();
        AddBuiltInClients(outputs);
        CodebaseMemoryRegistryOutputs.Add(outputs);
        CodebaseMemoryLegacyOutputs.Add(outputs, uninstall);
        outputs.Add(Output(Root(CodebaseMemoryDefinitions.CacheRoot), purpose: ToolNativeOutputPurpose.ReportOnly));
        outputs.Add(Output(Root(CodebaseMemoryDefinitions.RuntimeRoot), purpose: ToolNativeOutputPurpose.ReportOnly));
        if (uninstall)
        {
            outputs.Add(Output($"{ToolRecipeDefinitions.NativeInstallDirectoryToken}/{CodebaseMemoryDefinitions.WindowsBinary}", operatingSystem: ToolOperatingSystem.Windows));
            outputs.Add(Output($"{ToolRecipeDefinitions.NativeInstallDirectoryToken}/{CodebaseMemoryDefinitions.Command}", operatingSystem: ToolOperatingSystem.MacOS));
            outputs.Add(Output($"{ToolRecipeDefinitions.NativeInstallDirectoryToken}/{CodebaseMemoryDefinitions.Command}", operatingSystem: ToolOperatingSystem.Linux));
        }
        else
        {
            outputs.Add(Output($"{ToolRecipeDefinitions.NativeInstallDirectoryToken}/{CodebaseMemoryDefinitions.Command}", operatingSystem: ToolOperatingSystem.MacOS));
        }
        return outputs.ToImmutable();
    }

    private static void AddBuiltInClients(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        string[] mcpClients =
        [
            CodebaseMemoryDefinitions.Claude, CodebaseMemoryDefinitions.Codex, CodebaseMemoryDefinitions.Gemini, CodebaseMemoryDefinitions.Zed,
            CodebaseMemoryDefinitions.OpenCode, CodebaseMemoryDefinitions.Antigravity, CodebaseMemoryDefinitions.Kilo, CodebaseMemoryDefinitions.VsCode,
            CodebaseMemoryDefinitions.Cursor, CodebaseMemoryDefinitions.Windsurf, CodebaseMemoryDefinitions.Augment, CodebaseMemoryDefinitions.OpenClaw,
            CodebaseMemoryDefinitions.Kiro, CodebaseMemoryDefinitions.Junie, CodebaseMemoryDefinitions.Hermes, CodebaseMemoryDefinitions.OpenHands,
            CodebaseMemoryDefinitions.Cline, CodebaseMemoryDefinitions.Qwen, CodebaseMemoryDefinitions.Copilot, CodebaseMemoryDefinitions.Factory,
            CodebaseMemoryDefinitions.Crush, CodebaseMemoryDefinitions.Goose, CodebaseMemoryDefinitions.Vibe, CodebaseMemoryDefinitions.Grok,
        ];
        foreach (var id in mcpClients)
            outputs.Add(Output(Configuration(id), id));

        AddFiles(outputs, CodebaseMemoryDefinitions.Claude, Root(CodebaseMemoryDefinitions.ClaudeUserRoot), ".claude.json");
        AddFiles(outputs, CodebaseMemoryDefinitions.Claude, Root(CodebaseMemoryDefinitions.Claude), "settings.json");
        AddClaudeHooks(outputs);
        AddFiles(outputs, CodebaseMemoryDefinitions.Codex, Root(CodebaseMemoryDefinitions.Codex), "AGENTS.md");
        outputs.Add(Output($"{Root(CodebaseMemoryDefinitions.Codex)}/hooks.json", CodebaseMemoryDefinitions.Codex, match: ToolNativeOutputMatch.ExistingPattern));
        AddFiles(outputs, CodebaseMemoryDefinitions.Gemini, Home(".gemini"), "GEMINI.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.OpenCode, Home(".config/opencode"), "AGENTS.md", "plugins/cbm-augment.ts");
        AddFiles(outputs, CodebaseMemoryDefinitions.Antigravity, Home(".gemini"), "GEMINI.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.Aider, ToolRecipeDefinitions.NativeHomeToken, ".aider.conf.yml", "CONVENTIONS.md");
        outputs.Add(Output(Home("AppData/Roaming/Zed/AGENTS.md"), CodebaseMemoryDefinitions.Zed, ToolOperatingSystem.Windows));
        outputs.Add(Output(Home(".config/zed/AGENTS.md"), CodebaseMemoryDefinitions.Zed, ToolOperatingSystem.MacOS));
        outputs.Add(Output(Home(".config/zed/AGENTS.md"), CodebaseMemoryDefinitions.Zed, ToolOperatingSystem.Linux));
        AddFiles(outputs, CodebaseMemoryDefinitions.Kilo, Home(".config/kilo"), "rules/codebase-memory-mcp.md");
        outputs.Add(Output($"{Root(CodebaseMemoryDefinitions.VsCode)}/profiles/*/mcp.json", CodebaseMemoryDefinitions.VsCode, match: ToolNativeOutputMatch.ExistingPattern));
        AddFiles(outputs, CodebaseMemoryDefinitions.Windsurf, Home(".codeium/windsurf"), "memories/global_rules.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.OpenClaw, Root(CodebaseMemoryDefinitions.OpenClawWorkspaceRoot), "AGENTS.md", "TOOLS.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.Kiro, Root(CodebaseMemoryDefinitions.Kiro), "steering/codebase-memory.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.OpenHands, Home(".openhands"), "settings.json");
        outputs.Add(Output(Home(".openhands/agent-profiles/*.json"), CodebaseMemoryDefinitions.OpenHands, match: ToolNativeOutputMatch.ExistingPattern));
        AddAugmentHooks(outputs);
        AddFiles(outputs, CodebaseMemoryDefinitions.Cline, Home(".cline"), "rules/codebase-memory-mcp.md");
        outputs.Add(Output($"{Root(CodebaseMemoryDefinitions.ClineDataRoot)}/settings/cline_mcp_settings.json", CodebaseMemoryDefinitions.Cline));
        AddClineHooks(outputs);
        AddFiles(outputs, CodebaseMemoryDefinitions.Qwen, Root(CodebaseMemoryDefinitions.Qwen), "QWEN.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.Copilot, Root(CodebaseMemoryDefinitions.Copilot), "copilot-instructions.md", "hooks/codebase-memory-mcp.json");
        // VS Code also installs the native shared Copilot context, even without Copilot CLI discovery.
        AddFiles(outputs, CodebaseMemoryDefinitions.VsCode, Root(CodebaseMemoryDefinitions.Copilot), "hooks/codebase-memory-mcp.json");
        AddFiles(outputs, CodebaseMemoryDefinitions.Factory, Home(".factory"), "AGENTS.md", "hooks.json");
        AddFiles(outputs, CodebaseMemoryDefinitions.Crush, Home(".config/crush"), "codebase-memory.md");
        AddFiles(outputs, CodebaseMemoryDefinitions.Goose, Home(".config/goose"), ".goosehints");
        AddFiles(outputs, CodebaseMemoryDefinitions.Vibe, Root(CodebaseMemoryDefinitions.Vibe), "AGENTS.md");
        AddProfiles(outputs, CodebaseMemoryDefinitions.Vibe, $"{Root(CodebaseMemoryDefinitions.Vibe)}/prompts", ".md");
        AddFiles(outputs, CodebaseMemoryDefinitions.Grok, Root(CodebaseMemoryDefinitions.Grok), "rules/codebase-memory.md");
        AddBuiltInSkillsAndProfiles(outputs);
    }

    private static void AddBuiltInSkillsAndProfiles(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        AddClient(outputs, CodebaseMemoryDefinitions.Claude, Root(CodebaseMemoryDefinitions.Claude), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.Codex, Root(CodebaseMemoryDefinitions.Codex), ".toml");
        AddProfiles(outputs, CodebaseMemoryDefinitions.Gemini, Home(".gemini/agents"), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.OpenCode, Home(".config/opencode"), ".md");
        AddProfiles(outputs, CodebaseMemoryDefinitions.Kilo, Home(".config/kilo/agents"), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.Cursor, Home(".cursor"), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.Kiro, Root(CodebaseMemoryDefinitions.Kiro), ".json");
        AddClient(outputs, CodebaseMemoryDefinitions.Junie, Home(".junie"), ".md");
        AddSkills(outputs, CodebaseMemoryDefinitions.Hermes, $"{Root(CodebaseMemoryDefinitions.Hermes)}/skills");
        AddProfiles(outputs, CodebaseMemoryDefinitions.Augment, Home(".augment/agents"), ".md");
        AddSkills(outputs, CodebaseMemoryDefinitions.Cline, Home(".cline/skills"));
        AddClient(outputs, CodebaseMemoryDefinitions.Qwen, Root(CodebaseMemoryDefinitions.Qwen), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.Copilot, Root(CodebaseMemoryDefinitions.Copilot), ".agent.md");
        AddClient(outputs, CodebaseMemoryDefinitions.VsCode, Root(CodebaseMemoryDefinitions.Copilot), ".agent.md");
        AddSkills(outputs, CodebaseMemoryDefinitions.Factory, Home(".factory/skills"));
        AddProfiles(outputs, CodebaseMemoryDefinitions.Factory, Home(".factory/droids"), ".md");
        AddClient(outputs, CodebaseMemoryDefinitions.Vibe, Root(CodebaseMemoryDefinitions.Vibe), ".toml");
        AddClient(outputs, CodebaseMemoryDefinitions.Grok, Root(CodebaseMemoryDefinitions.Grok), ".md");
        foreach (var id in new[] { CodebaseMemoryDefinitions.Zed, CodebaseMemoryDefinitions.OpenHands, CodebaseMemoryDefinitions.Warp })
            AddSkills(outputs, id, Root(CodebaseMemoryDefinitions.SharedSkillRoot));
    }

    private static void AddClaudeHooks(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        foreach (var script in new[] { "cbm-code-discovery-gate", "cbm-session-reminder", "cbm-subagent-reminder" })
        {
            var path = $"{Root(CodebaseMemoryDefinitions.Claude)}/hooks/{script}";
            outputs.Add(Output(path + ".cmd", CodebaseMemoryDefinitions.Claude, ToolOperatingSystem.Windows));
            outputs.Add(Output(path, CodebaseMemoryDefinitions.Claude, ToolOperatingSystem.MacOS));
            outputs.Add(Output(path, CodebaseMemoryDefinitions.Claude, ToolOperatingSystem.Linux));
            outputs.Add(Output(path, CodebaseMemoryDefinitions.Claude, ToolOperatingSystem.Windows, ToolNativeOutputMatch.ExistingPattern));
        }
    }

    private static void AddAugmentHooks(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        AddFiles(outputs, CodebaseMemoryDefinitions.Augment, Home(".augment"), "rules/codebase-memory.md");
        foreach (var name in new[] { "codebase-memory-session", "codebase-memory-coverage" })
        {
            var path = Home($".augment/hooks/{name}");
            outputs.Add(Output(path + ".ps1", CodebaseMemoryDefinitions.Augment, ToolOperatingSystem.Windows));
            outputs.Add(Output(path + ".sh", CodebaseMemoryDefinitions.Augment, ToolOperatingSystem.MacOS));
            outputs.Add(Output(path + ".sh", CodebaseMemoryDefinitions.Augment, ToolOperatingSystem.Linux));
        }
    }

    private static void AddClineHooks(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        // v0.12.1 withholds new hooks and removes only recognized existing adapters.
        foreach (var name in new[] { "TaskStart", "TaskResume", "UserPromptSubmit", "PreCompact" })
        {
            var path = Home($".cline/hooks/{name}");
            outputs.Add(Output(path + ".ps1", CodebaseMemoryDefinitions.Cline, ToolOperatingSystem.Windows, ToolNativeOutputMatch.ExistingPattern));
            outputs.Add(Output(path, CodebaseMemoryDefinitions.Cline, ToolOperatingSystem.MacOS, ToolNativeOutputMatch.ExistingPattern));
            outputs.Add(Output(path, CodebaseMemoryDefinitions.Cline, ToolOperatingSystem.Linux, ToolNativeOutputMatch.ExistingPattern));
        }
    }

    internal static void AddClient(ImmutableArray<ToolNativeOutputSpec>.Builder outputs, string id, string root, string profileSuffix)
    {
        AddSkills(outputs, id, $"{root}/skills");
        AddProfiles(outputs, id, $"{root}/agents", profileSuffix);
    }

    internal static void AddSkills(ImmutableArray<ToolNativeOutputSpec>.Builder outputs, string id, string root)
    {
        outputs.Add(Output($"{root}/{CodebaseMemoryDefinitions.SkillPath}", id));
        foreach (var directory in new[] { "codebase-memory-exploring", "codebase-memory-tracing", "codebase-memory-quality", "codebase-memory-reference" })
            outputs.Add(Output($"{root}/{directory}", id, match: ToolNativeOutputMatch.ExistingPattern, purpose: ToolNativeOutputPurpose.ReportOnly));
    }

    internal static void AddProfiles(ImmutableArray<ToolNativeOutputSpec>.Builder outputs, string id, string directory, string suffix)
    {
        foreach (var name in new[] { CodebaseMemoryDefinitions.ScoutProfile, CodebaseMemoryDefinitions.VerifyProfile, CodebaseMemoryDefinitions.AuditorProfile })
            outputs.Add(Output($"{directory}/{name}{suffix}", id));
    }

    internal static void AddFiles(ImmutableArray<ToolNativeOutputSpec>.Builder outputs, string id, string root, params string[] files)
    {
        foreach (var file in files)
            outputs.Add(Output($"{root}/{file}", id));
    }

    internal static ToolNativeOutputSpec Output(
        string path,
        string? clientId = null,
        ToolOperatingSystem? operatingSystem = null,
        ToolNativeOutputMatch match = ToolNativeOutputMatch.Exact,
        ToolNativeOutputPurpose purpose = ToolNativeOutputPurpose.Backup) => new()
        {
            PathTemplate = path,
            Scope = ToolEffectScope.Account,
            Match = match,
            Purpose = purpose,
            OperatingSystem = operatingSystem,
            ClientId = clientId,
        };
}
