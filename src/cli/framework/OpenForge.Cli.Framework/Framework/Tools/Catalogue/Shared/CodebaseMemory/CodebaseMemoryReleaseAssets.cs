using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;

internal static class CodebaseMemoryReleaseAssets
{
    // Source: v0.12.1/checksums.txt, checked 2026-11-14. Linux pins are the fully static portable archives.
    internal static ImmutableArray<ToolReleaseAsset> Create() =>
    [
        CreateAsset(Target(ToolOperatingSystem.Windows, ToolCpu.X64), "windows-amd64", "4e4e0f16b45233af5c757a1fcbe3c3b16e9056375b1fbe2ce020b6de75dedcc0", 48508123),
        CreateAsset(Target(ToolOperatingSystem.Windows, ToolCpu.Arm64), "windows-arm64", "934ab6bd67a76c6ebd9e110d87b7090ffd629919bedaeddabe5a19175038b063", 47934918),
        CreateAsset(Target(ToolOperatingSystem.MacOS, ToolCpu.X64), "darwin-amd64", "ca6e64397a8ae80cec898a8205a6aa16750ac798acc05a0c3ed983837944a581", 50555159),
        CreateAsset(Target(ToolOperatingSystem.MacOS, ToolCpu.Arm64), "darwin-arm64", "9bfcbe38414b47969a3e1d1c1999f5357409387c86fa3d24221b5458267660d2", 49875423),
        CreateAsset(Target(ToolOperatingSystem.Linux, ToolCpu.X64), "linux-amd64-portable", "8c3d0341064301e78ecc84cf7348d4e2c6562a0f57448243ea560aaa34de36c1", 48920994),
        CreateAsset(Target(ToolOperatingSystem.Linux, ToolCpu.Arm64), "linux-arm64-portable", "ed0f1f548cd79b121c401c2e9f7841c189a001af26d3768368088c24f6a53533", 48466763),
    ];

    private static ToolReleaseAsset CreateAsset(ToolTarget target, string platform, string sha256, long maximumBytes)
    {
        var windows = target.OperatingSystem == ToolOperatingSystem.Windows;
        var extension = windows ? "zip" : "tar.gz";
        return new()
        {
            Version = CodebaseMemoryDefinitions.Version,
            Target = target,
            Url = new($"https://github.com/{CodebaseMemoryDefinitions.Repository}/releases/download/v{CodebaseMemoryDefinitions.Version}/{CodebaseMemoryDefinitions.Command}-{platform}.{extension}"),
            Sha256 = sha256,
            Archive = windows ? ToolArchiveKind.Zip : ToolArchiveKind.TarGzip,
            StripComponents = 0,
            EntryRelativePath = windows ? CodebaseMemoryDefinitions.WindowsBinary : CodebaseMemoryDefinitions.Command,
            MaximumBytes = maximumBytes,
        };
    }

    private static ToolTarget Target(ToolOperatingSystem operatingSystem, ToolCpu cpu) => new()
    {
        OperatingSystem = operatingSystem,
        Cpu = cpu,
        LinuxLibc = operatingSystem == ToolOperatingSystem.Linux ? ToolLinuxLibc.Static : null,
    };
}
