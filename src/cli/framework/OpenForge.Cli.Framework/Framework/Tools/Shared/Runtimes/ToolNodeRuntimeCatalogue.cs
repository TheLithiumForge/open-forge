using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;

internal static class ToolNodeRuntimeCatalogue
{
    // Review: lane-a.md records the verified signed SHASUMS and signer fingerprints.
    private const string CurrentSigner = "5BE8A3F6C8A5C01D106C0AD820B1A390B168D356";
    internal static IReadOnlyList<ToolPrivateNodeSpec> All { get; } =
    [
        Create("24.22.3", "win-x64.zip", ToolOperatingSystem.Windows, ToolCpu.X64, "e612af99f626ab415ea225af6bce5c971feb448cb5cae250bb1c3fa50c5f7d9f", CurrentSigner),
        Create("24.22.3", "win-arm64.zip", ToolOperatingSystem.Windows, ToolCpu.Arm64, "b3cc74a295b39d4f273271e96f58c06dd2b35757ca12abec64c7d0598b434f14", CurrentSigner),
        Create("24.22.3", "darwin-x64.tar.gz", ToolOperatingSystem.MacOS, ToolCpu.X64, "977614af0504c81045e7fd85596dbca0e5f951b96681516dadafedcca8c447f7", CurrentSigner),
        Create("24.22.3", "darwin-arm64.tar.gz", ToolOperatingSystem.MacOS, ToolCpu.Arm64, "ff6e17d4874b76d272212b908cc020d6e815fba5a6c7eaaa06add725840307fd", CurrentSigner),
        Create("24.22.3", "linux-x64.tar.gz", ToolOperatingSystem.Linux, ToolCpu.X64, "061ec3e3062115705b56b51fa9cc330ab50d34b6969387d4e4b1a50c6809acc2", CurrentSigner),
        Create("24.22.3", "linux-arm64.tar.gz", ToolOperatingSystem.Linux, ToolCpu.Arm64, "261af2af250dfd7e9f25cf151d1b2713a0adf8f10a9ad116c4947f68627affe8", CurrentSigner),
        Create("22.23.5", "win-x64.zip", ToolOperatingSystem.Windows, ToolCpu.X64, "4a191082e9706866830379d577d2cc8a50c776fcd9436a8d061b063ed0d9a0e3", CurrentSigner),
        Create("22.23.5", "win-arm64.zip", ToolOperatingSystem.Windows, ToolCpu.Arm64, "c1856817f611738c77534efbff8a5a17990593eab24046bc8d2c23a77a160d13", CurrentSigner),
        Create("22.23.5", "darwin-x64.tar.gz", ToolOperatingSystem.MacOS, ToolCpu.X64, "c53383238206b9ae4db23c5d8a63645e2e753bec8ab51a49cc81939aa64e9422", CurrentSigner),
        Create("22.23.5", "darwin-arm64.tar.gz", ToolOperatingSystem.MacOS, ToolCpu.Arm64, "e7131995759b61a0170b646010d2ad35a5d37b4cb0d471f3e8317eac6f742f3d", CurrentSigner),
        Create("22.23.5", "linux-x64.tar.gz", ToolOperatingSystem.Linux, ToolCpu.X64, "c54118104acc524633b5abe106bbf44a53126ec8b4e5fa105cb92b5d7de8a1a1", CurrentSigner),
        Create("22.23.5", "linux-arm64.tar.gz", ToolOperatingSystem.Linux, ToolCpu.Arm64, "8bd3dc69eee8f3f226315a80d7d1d4faccf217a4c8fae500168ac2e38f3bdba3", CurrentSigner),
    ];

    private static ToolPrivateNodeSpec Create(string version, string file, ToolOperatingSystem os, ToolCpu cpu, string sha256, string signer)
    {
        var windows = os == ToolOperatingSystem.Windows;
        string? osFloor = null;
        if (windows) osFloor = "10.0";
        if (os == ToolOperatingSystem.MacOS) osFloor = version.StartsWith("24.", StringComparison.Ordinal) ? "13.5" : "11.0";
        return new()
        {
            Version = version,
            SignedChecksums = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/SHASUMS256.txt.asc"),
            VerifiedSigner = signer,
            NpmRelativePath = windows ? "node_modules/npm/bin/npm-cli.js" : "lib/node_modules/npm/bin/npm-cli.js",
            Archive = new()
            {
                Version = version,
                Url = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/node-v{version}-{file}"),
                Sha256 = sha256,
                Archive = windows ? ToolArchiveKind.Zip : ToolArchiveKind.TarGzip,
                StripComponents = 1,
                EntryRelativePath = windows ? ToolPrivateNodeInstaller.WindowsNodeName : ToolPrivateNodeInstaller.PosixNodePath,
                MaximumBytes = 128L * 1024 * 1024,
                Target = new()
                {
                    OperatingSystem = os,
                    Cpu = cpu,
                    MinimumOperatingSystemVersion = osFloor,
                    LinuxLibc = os == ToolOperatingSystem.Linux ? ToolLinuxLibc.Glibc : null,
                    MinimumLibcVersion = os == ToolOperatingSystem.Linux ? "2.28" : null,
                    MinimumLinuxKernelVersion = os == ToolOperatingSystem.Linux ? "4.18" : null,
                    MinimumLibStdCppVersion = os == ToolOperatingSystem.Linux ? "6.0.25" : null,
                },
            },
        };
    }
}
