using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;

internal static class CodeGraphNativeSetup
{
    internal static ToolSetupSpec Create(bool uninstall)
    {
        ImmutableArray<string> arguments =
        [
            uninstall ? ToolCatalogueDefinitions.UninstallCommand : ToolCatalogueDefinitions.InstallCommand,
            ToolCatalogueDefinitions.YesArgument,
            CodeGraphDefinitions.LocalArgument, CodeGraphDefinitions.LocalLocation,
            CodeGraphDefinitions.TargetArgument, CodeGraphDefinitions.Targets,
        ];
        if (uninstall)
            arguments = arguments.Add(CodeGraphDefinitions.KeepCliArgument);

        return new()
        {
            Arguments = arguments,
            Environment = ImmutableDictionary<string, string?>.Empty,
            Timeout = ToolCatalogueDefinitions.SetupTimeout,
            MaximumOutputBytes = ToolCatalogueDefinitions.SetupMaximumBytes,
            Effects = ImmutableArray.Create(
                new ToolDeclaredEffect
                {
                    Id = "native-project-setup",
                    Scope = ToolEffectScope.Workspace,
                    Kind = ToolDeclaredEffectKind.Configuration,
                    Description = "Local Claude and Codex MCP entries, instruction blocks, Claude permissions and prompt hook. Keeps the CLI and index on removal.",
                },
                new ToolDeclaredEffect
                {
                    Id = "native-usage-telemetry",
                    Scope = ToolEffectScope.Account,
                    Kind = ToolDeclaredEffectKind.Telemetry,
                    Description = "Native setup and uninstall can record or flush account telemetry before Open Forge applies its declared stored-setting change.",
                }),
            PossibleOutputs = Outputs(),
        };
    }

    private static ImmutableArray<ToolNativeOutputSpec> Outputs()
    {
        var outputs = ImmutableArray.CreateBuilder<ToolNativeOutputSpec>();
        string[] files =
        [
            CodeGraphDefinitions.ClaudeMcpPath, CodeGraphDefinitions.ClaudeSettingsPath, CodeGraphDefinitions.ClaudeInstructionsPath,
            CodeGraphDefinitions.CodexConfigurationPath, CodeGraphDefinitions.CodexInstructionsPath, CodeGraphDefinitions.LegacyClaudePath,
        ];
        foreach (var path in files)
            outputs.Add(Output(path));
        foreach (var path in new[] { CodeGraphDefinitions.ClaudeMcpPath, CodeGraphDefinitions.ClaudeSettingsPath, CodeGraphDefinitions.LegacyClaudePath })
            outputs.Add(Output(path + CodeGraphDefinitions.NativeBackupSuffix));
        outputs.Add(new()
        {
            PathTemplate = CodeGraphDefinitions.TelemetryPath,
            Scope = ToolEffectScope.Account,
            Match = ToolNativeOutputMatch.Exact,
            Purpose = ToolNativeOutputPurpose.ReportOnly,
        });
        outputs.Add(new()
        {
            PathTemplate = CodeGraphDefinitions.TelemetryQueuePattern,
            Scope = ToolEffectScope.Account,
            Match = ToolNativeOutputMatch.ExistingPattern,
            Purpose = ToolNativeOutputPurpose.ReportOnly,
        });
        return outputs.ToImmutable();
    }

    private static ToolNativeOutputSpec Output(string path) => new()
    {
        PathTemplate = path,
        Scope = ToolEffectScope.Workspace,
        Match = ToolNativeOutputMatch.Exact,
        Purpose = ToolNativeOutputPurpose.Backup,
    };
}
