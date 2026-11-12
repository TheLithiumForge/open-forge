using System.Runtime.InteropServices;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal static class ToolNativeTarget
{
    internal static string Current => (OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsMacOS() ? "osx" : OperatingSystem.IsLinux() ? "linux" : "unknown")
        + "-" + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
}
