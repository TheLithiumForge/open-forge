using System.Text;
using OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

// Native path policy: CBM v0.12.1 src/cli/cli.c, cbm_openclaw_workspace_path.
// Copyright (c) 2025 DeusData. MIT licence, full Codebase Memory notice in root LICENSE.
internal static class ToolOpenClawRoots
{
    internal const string DefaultProfile = "default";
    internal const string ConfigurationFile = "openclaw.json";

    internal static string Configuration(ToolHomeEnvironment environment, string home)
    {
        var internalHome = InternalHome(environment, home);
        if (environment.Nonempty(ToolHomeEnvironment.OpenClawConfig) is { } custom) return Expand(custom, internalHome);
        var state = environment.Nonempty(ToolHomeEnvironment.OpenClawState) is { } stateValue
            ? Expand(stateValue, internalHome)
            : ToolHomePaths.Join(internalHome, $".openclaw{ProfileSuffix(environment)}");
        return ToolHomePaths.Join(state, ConfigurationFile);
    }

    internal static string Workspace(ToolHomeEnvironment environment, string home, string configuration)
    {
        var internalHome = InternalHome(environment, home);
        if (ToolClientConfigurationReader.Read(configuration) is { } text)
        {
            if (ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(text)) is { } value) return Expand(value, internalHome);
        }
        if (environment.Nonempty(ToolHomeEnvironment.OpenClawWorkspace) is { } custom) return Expand(custom, internalHome);
        return ToolHomePaths.Join(internalHome, $".openclaw/workspace{ProfileSuffix(environment)}");
    }

    private static string InternalHome(ToolHomeEnvironment environment, string home)
        => environment.Nonempty(ToolHomeEnvironment.OpenClawHome) is { } value ? Expand(value, home) : home;

    private static string Expand(string value, string home) => ToolHomeEnvironment.Expand(value, home)
        ?? throw new IOException("The native OpenClaw path must be absolute or home-relative.");

    private static string ProfileSuffix(ToolHomeEnvironment environment)
    {
        var profile = environment.Nonempty(ToolHomeEnvironment.OpenClawProfile);
        if (profile is null or DefaultProfile) return string.Empty;
        return $"-{ToolHomeEnvironment.Profile(profile)}";
    }

}
