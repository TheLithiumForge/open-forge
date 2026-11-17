using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryLegacyOutputs
{
    internal static void Add(ImmutableArray<ToolNativeOutputSpec>.Builder outputs, bool uninstall)
    {
        outputs.Add(CodebaseMemoryNativeOutputs.Output(
            $"{CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Claude)}/.mcp.json",
            CodebaseMemoryDefinitions.Claude, match: ToolNativeOutputMatch.ExistingPattern));
        outputs.Add(CodebaseMemoryNativeOutputs.Output(
            $"{CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Claude)}/skills/codebase-memory-mcp",
            CodebaseMemoryDefinitions.Claude, match: ToolNativeOutputMatch.ExistingPattern, purpose: ToolNativeOutputPurpose.ReportOnly));
        outputs.Add(CodebaseMemoryNativeOutputs.Output(
            CodebaseMemoryNativeOutputs.Home(".gemini/antigravity-cli/settings.json"), CodebaseMemoryDefinitions.Antigravity,
            match: ToolNativeOutputMatch.ExistingPattern));
        outputs.Add(CodebaseMemoryNativeOutputs.Output(
            CodebaseMemoryNativeOutputs.Home(".kilocode/rules/codebase-memory-mcp.md"), CodebaseMemoryDefinitions.Kilo,
            match: ToolNativeOutputMatch.ExistingPattern));
        AddKiloLegacy(outputs);
        if (uninstall)
            outputs.Add(CodebaseMemoryNativeOutputs.Output(
                CodebaseMemoryNativeOutputs.Home(".config/crush/CRUSH.md"), CodebaseMemoryDefinitions.Crush, match: ToolNativeOutputMatch.ExistingPattern));
    }

    private static void AddKiloLegacy(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        const string settings = "Code/User/globalStorage/kilocode.kilo-code/settings/mcp_settings.json";
        outputs.Add(CodebaseMemoryNativeOutputs.Output(CodebaseMemoryNativeOutputs.Home($"AppData/Roaming/{settings}"),
            CodebaseMemoryDefinitions.Kilo, ToolOperatingSystem.Windows, ToolNativeOutputMatch.ExistingPattern));
        outputs.Add(CodebaseMemoryNativeOutputs.Output(CodebaseMemoryNativeOutputs.Home($"Library/Application Support/{settings}"),
            CodebaseMemoryDefinitions.Kilo, ToolOperatingSystem.MacOS, ToolNativeOutputMatch.ExistingPattern));
        outputs.Add(CodebaseMemoryNativeOutputs.Output(CodebaseMemoryNativeOutputs.Home($".config/{settings}"),
            CodebaseMemoryDefinitions.Kilo, ToolOperatingSystem.Linux, ToolNativeOutputMatch.ExistingPattern));
    }
}
