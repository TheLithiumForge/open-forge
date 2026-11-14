using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-target"), Trait("Evidence", "Unit")]
public sealed class ToolTargetPolicyTests
{
    [Theory(DisplayName = "Runtime targets require known kernel libc and C++ library floors")]
    [InlineData("4.18", "2.28", "6.0.25", true)]
    [InlineData("6.1", "2.39", "6.0.33", true)]
    [InlineData("4.17", "2.28", "6.0.25", false)]
    [InlineData("4.18", "2.27", "6.0.25", false)]
    [InlineData("4.18", "2.28", "6.0.24", false)]
    [InlineData(null, "2.28", "6.0.25", false)]
    [InlineData("4.18", null, "6.0.25", false)]
    [InlineData("4.18", "2.28", null, false)]
    public void RequiresKnownFloors(string? kernel, string? libc, string? cpp, bool expected)
    {
        var required = new ToolTarget
        {
            OperatingSystem = ToolOperatingSystem.Linux,
            Cpu = ToolCpu.X64,
            LinuxLibc = ToolLinuxLibc.Glibc,
            MinimumLinuxKernelVersion = "4.18",
            MinimumLibcVersion = "2.28",
            MinimumLibStdCppVersion = "6.0.25"
        };
        var actual = required with { MinimumLinuxKernelVersion = kernel, MinimumLibcVersion = libc, MinimumLibStdCppVersion = cpp };
        Assert.Equal(expected, ToolTargetPolicy.Supports(required, actual));
        Assert.False(ToolTargetPolicy.Supports(required, actual with { LinuxLibc = ToolLinuxLibc.Musl }));
    }
}
