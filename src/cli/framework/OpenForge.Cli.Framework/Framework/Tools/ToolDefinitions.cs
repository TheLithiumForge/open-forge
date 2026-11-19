using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Core.Framework.Tools;

internal enum ToolPlatform
{
    Windows,
    Unix,
}

/// <summary>A program that an install method needs on PATH.</summary>
internal enum ToolRequirement
{
    Npm,
    Curl,
    Bash,
    PowerShell,
}

internal static class ToolDefinitions
{
    internal const string CodeSearchCategory = "Code search";

    // Probes answer only whether a program runs. Installs, setups and uninstalls download and configure, so they get far longer.
    internal static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(15);
    internal static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(15);

    internal static ToolPlatform CurrentPlatform => OperatingSystem.IsWindows() ? ToolPlatform.Windows : ToolPlatform.Unix;

    internal static string MethodName(ToolMethod method) => method switch
    {
        ToolMethod.Npm => "npm",
        ToolMethod.Script => "script",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The install method is not defined."),
    };

    internal static string RequirementName(ToolRequirement requirement) => requirement switch
    {
        ToolRequirement.Npm => "npm",
        ToolRequirement.Curl => "curl",
        ToolRequirement.Bash => "bash",
        ToolRequirement.PowerShell => "powershell",
        _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, "The requirement is not defined."),
    };

    internal static ImmutableArray<ToolRequirement> Requirements(ToolMethod method, ToolPlatform platform) => (method, platform) switch
    {
        (ToolMethod.Npm, _) => [ToolRequirement.Npm],
        (ToolMethod.Script, ToolPlatform.Windows) => [ToolRequirement.PowerShell],
        (ToolMethod.Script, ToolPlatform.Unix) => [ToolRequirement.Curl, ToolRequirement.Bash],
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The install method is not defined."),
    };

    internal static ToolCommand Probe(ToolRequirement requirement) => requirement switch
    {
        ToolRequirement.PowerShell => new("powershell -NoProfile -NonInteractive -Command exit"),
        _ => new($"{RequirementName(requirement)} --version"),
    };
}
