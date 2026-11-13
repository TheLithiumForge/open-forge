namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal enum ToolOperatingSystem { Windows, MacOS, Linux }
internal enum ToolCpu { X64, Arm64 }
internal enum ToolLinuxLibc { Glibc, Musl, Static }

internal sealed record ToolTarget
{
    public required ToolOperatingSystem OperatingSystem { get; init; }
    public required ToolCpu Cpu { get; init; }
    public ToolLinuxLibc? LinuxLibc { get; init; }
    public string? MinimumOperatingSystemVersion { get; init; }
    public string? MinimumLibcVersion { get; init; }
}

internal sealed record ToolRuntimeRequirement(string Runtime, string VersionRange);

internal sealed record ToolChannelRequirements
{
    public ToolRuntimeRequirement? Runtime { get; init; }
    public string? MinimumPackageManagerVersion { get; init; }
    public required IReadOnlyList<ToolTarget> Targets { get; init; }
}
