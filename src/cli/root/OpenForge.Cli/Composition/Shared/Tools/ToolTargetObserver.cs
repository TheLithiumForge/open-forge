using System.Runtime.InteropServices;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal static class ToolTargetObserver
{
    internal const string LibStdCppName = "libstdc++.so.6";
    internal static async Task<ToolTarget> ObserveAsync(IToolProcessHost processes, string workspace, CancellationToken token)
    {
        var cpu = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => ToolCpu.X64,
            Architecture.Arm64 => ToolCpu.Arm64,
            _ => throw new PlatformNotSupportedException("This tool target needs x64 or ARM64."),
        };
        if (OperatingSystem.IsWindows()) return new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = cpu, MinimumOperatingSystemVersion = Environment.OSVersion.Version.ToString() };
        if (OperatingSystem.IsMacOS())
        {
            var version = await ReadAsync("sw_vers", ["-productVersion"]).ConfigureAwait(false);
            return new() { OperatingSystem = ToolOperatingSystem.MacOS, Cpu = cpu, MinimumOperatingSystemVersion = NumericVersion(version) };
        }
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("This tool operating system isn't supported.");
        var kernel = await ReadAsync("uname", ["-r"]).ConfigureAwait(false);
        var libc = await ReadAsync("getconf", ["GNU_LIBC_VERSION"]).ConfigureAwait(false);
        ToolLinuxLibc? family = null;
        string? libcVersion = null;
        if (libc?.StartsWith("glibc ", StringComparison.Ordinal) == true) { family = ToolLinuxLibc.Glibc; libcVersion = NumericVersion(libc[6..]); }
        if (family is null)
        {
            var ldd = await ReadAsync("ldd", ["--version"]).ConfigureAwait(false);
            if (ldd?.Contains("musl", StringComparison.OrdinalIgnoreCase) == true) family = ToolLinuxLibc.Musl;
        }
        return new()
        {
            OperatingSystem = ToolOperatingSystem.Linux,
            Cpu = cpu,
            LinuxLibc = family,
            MinimumOperatingSystemVersion = NumericVersion(kernel),
            MinimumLinuxKernelVersion = NumericVersion(kernel),
            MinimumLibcVersion = libcVersion,
            MinimumLibStdCppVersion = LibStdCpp(workspace, cpu)
        };

        async Task<string?> ReadAsync(string command, IReadOnlyList<string> arguments)
        {
            var resolved = await processes.ResolveAsync(command, workspace, token).ConfigureAwait(false);
            if (resolved is null) return null;
            var result = await processes.RunAsync(ToolPackageManagerDetector.Request(resolved.Invocation, arguments, workspace), token).ConfigureAwait(false);
            if (result.Cancelled || result.TimedOut || result.Failure is not null) return null;
            return $"{result.StandardOutput.Trim()}\n{result.StandardError.Trim()}".Trim();
        }
    }

    internal static string? NumericVersion(string? value)
    {
        if (value is null) return null;
        var prefix = new string(value.TakeWhile(character => char.IsAsciiDigit(character) || character == '.').ToArray()).TrimEnd('.');
        return Version.TryParse(prefix, out _) ? prefix : null;
    }

    private static string? LibStdCpp(string workspace, ToolCpu cpu)
    {
        var root = System.IO.Path.GetPathRoot(workspace) ?? throw new IOException("The host filesystem root is unavailable.");
        var triple = cpu == ToolCpu.Arm64 ? "aarch64-linux-gnu" : "x86_64-linux-gnu";
        var directories = new[] { System.IO.Path.Combine(root, "usr", "lib", triple), System.IO.Path.Combine(root, "usr", "lib64"), System.IO.Path.Combine(root, "usr", "lib"), System.IO.Path.Combine(root, "lib", triple) };
        return LibStdCpp(directories);
    }

    internal static string? LibStdCpp(IReadOnlyList<string> directories)
    {
        foreach (var directory in directories)
        {
            var file = System.IO.Path.Combine(directory, LibStdCppName);
            if (!File.Exists(file)) continue;
            try
            {
                var info = new FileInfo(file);
                var name = info.ResolveLinkTarget(returnFinalTarget: true)?.Name ?? info.Name;
                const string prefix = "libstdc++.so.";
                return name.StartsWith(prefix, StringComparison.Ordinal) && Version.TryParse(name[prefix.Length..], out var version) ? version.ToString() : null;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { return null; }
        }
        return null;
    }
}
