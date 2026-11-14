using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryRegistryOutputs
{
    internal static void Add(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        string[] clients =
        [
            CodebaseMemoryDefinitions.Qoder, CodebaseMemoryDefinitions.Kimi, CodebaseMemoryDefinitions.GitLab, CodebaseMemoryDefinitions.Rovo,
            CodebaseMemoryDefinitions.Amp, CodebaseMemoryDefinitions.Devin, CodebaseMemoryDefinitions.Tabnine, CodebaseMemoryDefinitions.Continue,
            CodebaseMemoryDefinitions.VisualStudio, CodebaseMemoryDefinitions.Trae, CodebaseMemoryDefinitions.Roo, CodebaseMemoryDefinitions.AmazonQ,
            CodebaseMemoryDefinitions.CodeBuddy, CodebaseMemoryDefinitions.BobIde, CodebaseMemoryDefinitions.BobShell, CodebaseMemoryDefinitions.Pochi,
            CodebaseMemoryDefinitions.Cody, CodebaseMemoryDefinitions.Omp,
        ];
        foreach (var id in clients)
            outputs.Add(CodebaseMemoryNativeOutputs.Output(CodebaseMemoryNativeOutputs.Configuration(id), id));

        CodebaseMemoryNativeOutputs.AddClient(outputs, CodebaseMemoryDefinitions.Qoder, CodebaseMemoryNativeOutputs.Home(".qoder"), ".md");
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Kimi, CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Kimi), "AGENTS.md", "config.toml");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.Kimi, $"{CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Kimi)}/skills");
        outputs.Add(CodebaseMemoryNativeOutputs.Output(CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.GitLabHooksRoot), CodebaseMemoryDefinitions.GitLab));
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Rovo, CodebaseMemoryNativeOutputs.Home(".rovodev"), "AGENTS.md");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.Rovo, CodebaseMemoryNativeOutputs.Home(".rovodev/skills"));
        CodebaseMemoryNativeOutputs.AddProfiles(outputs, CodebaseMemoryDefinitions.Rovo, CodebaseMemoryNativeOutputs.Home(".rovodev/subagents"), ".md");
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Amp, CodebaseMemoryNativeOutputs.Home(".config/amp"), "AGENTS.md");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.Amp, CodebaseMemoryNativeOutputs.Home(".config/agents/skills"));
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Devin, CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Devin), "AGENTS.md");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.Devin, $"{CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Devin)}/skills");
        AddCodeBuddy(outputs);
        foreach (var id in new[] { CodebaseMemoryDefinitions.BobIde, CodebaseMemoryDefinitions.BobShell })
            CodebaseMemoryNativeOutputs.AddFiles(outputs, id, CodebaseMemoryNativeOutputs.Home(".bob"), "rules/codebase-memory.md");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.BobIde, CodebaseMemoryNativeOutputs.Home(".bob/skills"));
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Pochi, CodebaseMemoryNativeOutputs.Home(".pochi"), "README.pochi.md");
        CodebaseMemoryNativeOutputs.AddClient(outputs, CodebaseMemoryDefinitions.Pochi, CodebaseMemoryNativeOutputs.Home(".pochi"), ".md");
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.Pi, CodebaseMemoryNativeOutputs.Home(".pi/agent"), "AGENTS.md", "extensions/cbmem.ts");
        CodebaseMemoryNativeOutputs.AddSkills(outputs, CodebaseMemoryDefinitions.Pi, CodebaseMemoryNativeOutputs.Home(".pi/agent/skills"));
        CodebaseMemoryNativeOutputs.AddClient(outputs, CodebaseMemoryDefinitions.Omp, CodebaseMemoryNativeOutputs.Root(CodebaseMemoryDefinitions.Omp), ".md");
    }

    private static void AddCodeBuddy(ImmutableArray<ToolNativeOutputSpec>.Builder outputs)
    {
        CodebaseMemoryNativeOutputs.AddFiles(outputs, CodebaseMemoryDefinitions.CodeBuddy, CodebaseMemoryNativeOutputs.Home(".codebuddy"), "CODEBUDDY.md");
        CodebaseMemoryNativeOutputs.AddClient(outputs, CodebaseMemoryDefinitions.CodeBuddy, CodebaseMemoryNativeOutputs.Home(".codebuddy"), ".md");
    }
}
