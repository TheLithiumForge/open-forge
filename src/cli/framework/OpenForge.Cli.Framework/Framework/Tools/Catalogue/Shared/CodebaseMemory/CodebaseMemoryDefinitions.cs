using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryDefinitions
{
    internal const string Id = "codebase-memory";
    internal const string Version = "0.12.1";
    internal const string ProfileId = "codebase-memory-0.12";
    internal const string Repository = "DeusData/codebase-memory-mcp";
    internal const string Command = "codebase-memory-mcp";
    internal const string WindowsBinary = Command + ".exe";
    internal const string SkipBinaryArgument = "--skip-binary";
    internal const string DirectoryArgument = "--dir";
    internal const string PlanArgument = "--plan";
    internal const string IgnorePath = ".cbmignore";
    internal const string IgnoreStartMarker = "# Open Forge codebase-memory archives start";
    internal const string IgnoreEndMarker = "# Open Forge codebase-memory archives end";
    internal const string ArchivePattern = "/" + ToolRecipeDefinitions.MainRootToken + "/memory/archived/";
    internal const string SkillPath = "codebase-memory/SKILL.md";
    internal const string ScoutProfile = "codebase-memory-scout";
    internal const string VerifyProfile = "codebase-memory";
    internal const string AuditorProfile = "codebase-memory-auditor";
    internal const string ClaudeUserRoot = "claude-user";
    internal const string SharedSkillRoot = "shared-skills";
    internal const string ClineDataRoot = "cline-data";
    internal const string OpenClawWorkspaceRoot = "openclaw-workspace";
    internal const string CacheRoot = "codebase-memory-cache";
    internal const string RuntimeRoot = "codebase-memory-runtime";
    internal const string GitLabHooksRoot = "gitlab-duo-hooks";

    // Native plan IDs, not display labels. Keep the complete upstream discovery set.
    internal const string Claude = "claude-code";
    internal const string Codex = "codex";
    internal const string Gemini = "gemini";
    internal const string Zed = "zed";
    internal const string OpenCode = "opencode";
    internal const string Antigravity = "antigravity";
    internal const string Aider = "aider";
    internal const string Kilo = "kilocode";
    internal const string VsCode = "vscode";
    internal const string Cursor = "cursor";
    internal const string Windsurf = "windsurf";
    internal const string Augment = "augment-auggie";
    internal const string OpenClaw = "openclaw";
    internal const string Kiro = "kiro";
    internal const string Junie = "junie";
    internal const string Hermes = "hermes";
    internal const string OpenHands = "openhands";
    internal const string Cline = "cline";
    internal const string Warp = "warp";
    internal const string Qwen = "qwen";
    internal const string Copilot = "copilot-cli";
    internal const string Factory = "factory-droid";
    internal const string Crush = "crush";
    internal const string Goose = "goose";
    internal const string Vibe = "mistral-vibe";
    internal const string Grok = "grok";
    internal const string Qoder = "qoder";
    internal const string Kimi = "kimi";
    internal const string GitLab = "gitlab-duo";
    internal const string Rovo = "rovo-dev";
    internal const string Amp = "amp";
    internal const string Devin = "devin";
    internal const string Tabnine = "tabnine";
    internal const string Continue = "continue";
    internal const string VisualStudio = "visual-studio";
    internal const string Trae = "trae";
    internal const string Roo = "roo-code";
    internal const string AmazonQ = "amazon-q";
    internal const string CodeBuddy = "codebuddy";
    internal const string BobIde = "ibm-bob-ide";
    internal const string BobShell = "ibm-bob-shell";
    internal const string Pochi = "pochi";
    internal const string Pi = "pi";
    internal const string Cody = "sourcegraph-cody";
    internal const string Omp = "omp";
}
