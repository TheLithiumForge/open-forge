using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryNativeSetup
{
    internal static ToolSetupSpec Create(bool uninstall)
    {
        ImmutableArray<string> arguments = uninstall
            ? [ToolCatalogueDefinitions.UninstallCommand, ToolCatalogueDefinitions.YesArgument, CodebaseMemoryDefinitions.DirectoryArgument, ToolRecipeDefinitions.NativeInstallDirectoryToken]
            : [ToolCatalogueDefinitions.InstallCommand, ToolCatalogueDefinitions.YesArgument, CodebaseMemoryDefinitions.SkipBinaryArgument,
                CodebaseMemoryDefinitions.DirectoryArgument, ToolRecipeDefinitions.NativeInstallDirectoryToken];
        return new()
        {
            Arguments = arguments,
            DryRunArguments = arguments.Add(ToolCatalogueDefinitions.DryRunArgument),
            Environment = ImmutableDictionary<string, string?>.Empty,
            Timeout = ToolCatalogueDefinitions.SetupTimeout,
            MaximumOutputBytes = ToolCatalogueDefinitions.SetupMaximumBytes,
            NativePlan = uninstall ? null : new()
            {
                Arguments = arguments.Add(CodebaseMemoryDefinitions.PlanArgument),
                Parser = ToolNativePlanParser.CbmInstallPlanV1,
                Timeout = ToolCatalogueDefinitions.ProbeTimeout,
                MaximumOutputBytes = ToolCatalogueDefinitions.SetupMaximumBytes,
            },
            PossibleOutputs = CodebaseMemoryNativeOutputs.Create(uninstall),
            Effects = Effects(uninstall),
        };
    }

    private static ImmutableArray<ToolDeclaredEffect> Effects(bool uninstall)
    {
        var effects = ImmutableArray.CreateBuilder<ToolDeclaredEffect>();
        effects.Add(new()
        {
            Id = "native-account-clients",
            Scope = ToolEffectScope.Account,
            Kind = ToolDeclaredEffectKind.Configuration,
            Description = uninstall
                ? "Native uninstall removes account MCP entries, Skills, tiered agents, instruction blocks and hooks from all discovered clients."
                : "Native default discovery configures account MCP entries, Skills, tiered agents, instruction blocks and hooks. Watcher defaults stay auto_index=false, auto_watch=true and watcher_enabled=true.",
        });
        effects.Add(new()
        {
            Id = "native-account-cache",
            Scope = ToolEffectScope.Account,
            Kind = ToolDeclaredEffectKind.Configuration,
            Description = "Native activation and daemon coordination use the account cache and runtime directory. Existing indexes are kept.",
        });
        if (uninstall)
        {
            effects.Add(new()
            {
                Id = "native-account-binary-removal",
                Scope = ToolEffectScope.Account,
                Kind = ToolDeclaredEffectKind.SharedFileDeletion,
                PathTemplate = ToolRecipeDefinitions.NativeInstallDirectoryToken,
                Description = "Native uninstall removes the account-wide binary and all agent entries. Requires live confirmation naming those effects.",
            });
            effects.Add(new()
            {
                Id = "native-windows-user-path",
                Scope = ToolEffectScope.Account,
                Kind = ToolDeclaredEffectKind.PathChange,
                Description = "On Windows, native uninstall removes its directory from persistent user PATH. --yes, no terminal or a declined account confirmation skips native uninstall.",
            });
        }
        else
        {
            effects.Add(new()
            {
                Id = "native-macos-signing",
                Scope = ToolEffectScope.Account,
                Kind = ToolDeclaredEffectKind.Configuration,
                PathTemplate = ToolRecipeDefinitions.NativeInstallDirectoryToken + "/" + CodebaseMemoryDefinitions.Command,
                Description = "On macOS, native setup can ad-hoc sign the existing binary. --skip-binary keeps placement and PATH unchanged on Windows.",
            });
        }
        return effects.ToImmutable();
    }
}
