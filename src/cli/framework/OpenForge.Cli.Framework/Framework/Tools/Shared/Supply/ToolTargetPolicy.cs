using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolTargetPolicy
{
    internal static bool Supports(ToolTarget required, ToolTarget actual)
        => required.OperatingSystem == actual.OperatingSystem && required.Cpu == actual.Cpu
            && (required.LinuxLibc is null or ToolLinuxLibc.Static || required.LinuxLibc == actual.LinuxLibc)
            && Meets(required.MinimumOperatingSystemVersion, actual.MinimumOperatingSystemVersion)
            && Meets(required.MinimumLibcVersion, actual.MinimumLibcVersion)
            && Meets(required.MinimumLinuxKernelVersion, actual.MinimumLinuxKernelVersion)
            && Meets(required.MinimumLibStdCppVersion, actual.MinimumLibStdCppVersion);

    internal static bool Supports(ToolChannelRequirements requirements, ToolTarget actual)
        => requirements.Targets.Count == 0 || requirements.Targets.Any(target => Supports(target, actual));

    private static bool Meets(string? floor, string? actual)
        => floor is null || floor == actual || Version.TryParse(floor, out var minimum) && Version.TryParse(actual, out var observed) && observed >= minimum;
}
