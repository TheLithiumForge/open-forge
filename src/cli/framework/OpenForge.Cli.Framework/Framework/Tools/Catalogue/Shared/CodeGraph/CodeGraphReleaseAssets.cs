using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;

internal static class CodeGraphReleaseAssets
{
    // Source: v1.6.2/SHA256SUMS, checked 2026-11-14. The attestation/tag mismatch doesn't block these pins.
    internal static ImmutableArray<ToolReleaseAsset> Create() =>
    [
        CreateAsset(Windows(ToolCpu.X64), "win32-x64", "7455e817ee8032666867a1badeb799b8a884c5f4fea9b444e088a02069b6deea", 54260850),
        CreateAsset(Windows(ToolCpu.Arm64), "win32-arm64", "308f7bf92e7886f4d5f767a4755d703e92d5cae78ac042b8907f1e99db0fc071", 50258514),
        CreateAsset(MacOS(ToolCpu.X64), "darwin-x64", "53d1a4d1a9af31d6cec11b346df2ae792087081e13f623f583e27783c1e7f9bc", 59393903),
        CreateAsset(MacOS(ToolCpu.Arm64), "darwin-arm64", "d74d1bfb4060db63ec3c2b72c4e17f76c31978af3bad6ace4370d1501ac0662e", 58280485),
        CreateAsset(Linux(ToolCpu.X64), "linux-x64", "ef0af416092128fb1ccc723786000b7edf4a6971fe604acd08bf966e4f37b828", 63433974),
        CreateAsset(Linux(ToolCpu.Arm64), "linux-arm64", "c8c6be292be21d00dea26bad8b28d434731cf612fb8cc475dc10f5b3038b5b64", 63017097),
    ];

    private static ToolReleaseAsset CreateAsset(ToolTarget target, string platform, string sha256, long maximumBytes)
    {
        var windows = target.OperatingSystem == ToolOperatingSystem.Windows;
        var extension = windows ? "zip" : "tar.gz";
        return new()
        {
            Version = CodeGraphDefinitions.Version,
            Target = target,
            Url = new($"https://github.com/{CodeGraphDefinitions.Repository}/releases/download/v{CodeGraphDefinitions.Version}/codegraph-{platform}.{extension}"),
            Sha256 = sha256,
            Archive = windows ? ToolArchiveKind.Zip : ToolArchiveKind.TarGzip,
            StripComponents = 1,
            EntryRelativePath = windows ? CodeGraphDefinitions.WindowsLauncher : CodeGraphDefinitions.UnixLauncher,
            BundledRuntime = new()
            {
                Runtime = ToolCatalogueDefinitions.NodeRuntime,
                Version = CodeGraphDefinitions.BundledNodeVersion,
                RuntimeRelativePath = windows ? CodeGraphDefinitions.WindowsRuntime : CodeGraphDefinitions.UnixRuntime,
                EntryRelativePath = CodeGraphDefinitions.ApplicationEntry,
                Arguments = ImmutableArray.Create(CodeGraphDefinitions.LiftoffArgument, CodeGraphDefinitions.WarningArgument),
            },
            MaximumBytes = maximumBytes,
        };
    }

    private static ToolTarget Windows(ToolCpu cpu) => new()
    {
        OperatingSystem = ToolOperatingSystem.Windows,
        Cpu = cpu,
        MinimumOperatingSystemVersion = "10.0",
    };

    private static ToolTarget MacOS(ToolCpu cpu) => new()
    {
        OperatingSystem = ToolOperatingSystem.MacOS,
        Cpu = cpu,
        MinimumOperatingSystemVersion = "13.5",
    };

    private static ToolTarget Linux(ToolCpu cpu) => new()
    {
        OperatingSystem = ToolOperatingSystem.Linux,
        Cpu = cpu,
        LinuxLibc = ToolLinuxLibc.Glibc,
        MinimumLibcVersion = "2.28",
        MinimumLinuxKernelVersion = "4.18",
        MinimumLibStdCppVersion = "6.0.25",
    };
}
