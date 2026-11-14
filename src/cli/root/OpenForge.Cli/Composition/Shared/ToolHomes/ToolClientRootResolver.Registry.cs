using System.Collections.Immutable;
using OpenForge.Cli.Composition.Shared.ToolHomes.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

internal sealed partial class ToolClientRootResolver
{
    private const string DefaultOmpDirectory = ".omp/agent";
    private static readonly ImmutableArray<ToolClientRootRecipe> RegistryRecipes =
    [
        new(Kimi, true, static roots => roots.ExpandedOrHome(ToolHomeEnvironment.KimiHome, ".kimi-code")),
        new(Devin, true, static roots => roots.DevinDirectory()),
        new(Omp, true, static roots => roots.OmpDirectory()),
        new(ConfigKey(Qoder), false, static roots => roots.Home(".qoder/settings.json")),
        new(ConfigKey(Kimi), false, static roots => roots.DirectoryFile(Kimi, "mcp.json")),
        new(ConfigKey(GitLab), false, static roots => ToolHomePaths.Join(roots.GitLabDirectory(), "mcp.json")),
        new(GitLabHooksRoot, false, static roots => ToolHomePaths.Join(roots.GitLabDirectory(), "hooks.json")),
        new(ConfigKey(Rovo), false, static roots => roots.RovoConfiguration()),
        new(ConfigKey(Amp), false, static roots => roots.Home(".config/agents/skills/codebase-memory/mcp.json")),
        new(ConfigKey(Devin), false, static roots => roots.DirectoryFile(Devin, "config.json")),
        new(ConfigKey(Tabnine), false, static roots => roots.Home(".tabnine/mcp_servers.json")),
        new(ConfigKey(Continue), false, static roots => roots.ExistingOverride(ToolHomeEnvironment.ContinueConfig, ".continue/config.yaml")),
        new(ConfigKey(VisualStudio), false, static roots => roots.VisualStudioConfiguration()),
        new(ConfigKey(Trae), false, static roots => roots.ExistingOverride(ToolHomeEnvironment.TraeConfig)),
        new(ConfigKey(Roo), false, static roots => roots.ExistingOverride(ToolHomeEnvironment.RooConfig)),
        new(ConfigKey(AmazonQ), false, static roots => roots.Choose(".aws/amazonq/default.json", ".aws/amazonq/agents/default.json", ".aws/amazonq/mcp.json")),
        new(ConfigKey(CodeBuddy), false, static roots => roots.Choose(".codebuddy/.mcp.json", ".codebuddy/mcp.json", ".codebuddy.json")),
        new(ConfigKey(BobIde), false, static roots => roots.Home(".bob/mcp.json")),
        new(ConfigKey(BobShell), false, static roots => roots.Home(".bob/mcp_settings.json")),
        new(ConfigKey(Pochi), false, static roots => roots.Home(".pochi/config.jsonc")),
        new(ConfigKey(Cody), false, static roots => roots.ExistingOverride(ToolHomeEnvironment.CodyConfig)),
        new(ConfigKey(Omp), false, static roots => roots.DirectoryFile(Omp, "mcp.json")),
    ];

    private string DevinDirectory() => operatingSystem == ToolOperatingSystem.Windows && environment.Expanded(ToolHomeEnvironment.AppData, home) is { } appData
        ? ToolHomePaths.Join(appData, Devin) : Home(".config/devin");
    private string? VisualStudioConfiguration() => operatingSystem == ToolOperatingSystem.Windows ? Home(".mcp.json") : null;
    private string RovoConfiguration() => ToolClientConfigurationReader.Rovo(home);
    private string Choose(params string[] suffixes) => ToolHomePaths.Choose(home, suffixes);

    private string GitLabDirectory()
    {
        if (environment.Expanded(ToolHomeEnvironment.GlabConfigDirectory, home) is { } custom) return ToolHomePaths.Join(custom, "duo");
        if (operatingSystem == ToolOperatingSystem.Windows && environment.Expanded(ToolHomeEnvironment.AppData, home) is { } appData)
            return ToolHomePaths.Join(appData, "GitLab/duo");
        if (environment.Expanded(ToolHomeEnvironment.XdgConfigHome, home) is { } xdg) return ToolHomePaths.Join(xdg, "gitlab/duo");
        return Home(".gitlab/duo");
    }

    private string? ExistingOverride(string variable, string? fallback = null)
    {
        var path = environment.Expanded(variable, home);
        if (path is null && fallback is not null) path = Home(fallback);
        return path is not null && ToolHomePaths.Exists(path) ? path : null;
    }

    private string OmpDirectory()
    {
        if (environment.Nonempty(ToolHomeEnvironment.OmpProfile) is { } profile)
        {
            if (ToolHomeEnvironment.IsProfile(profile)) return Home($".omp/profiles/{profile}/agent");
            return Home(DefaultOmpDirectory);
        }
        if (environment.Nonempty(ToolHomeEnvironment.PiAgentDirectory) is { } value)
            return ToolHomeEnvironment.Expand(value, home) ?? Home(DefaultOmpDirectory);
        return Home(DefaultOmpDirectory);
    }
}
