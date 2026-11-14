using System.IO.Compression;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Runtimes;

[Trait("Feature", "tool-private-node"), Trait("Evidence", "Integration")]
public sealed class ToolPrivateNodeInstallerTests
{
    [Fact(DisplayName = "Private Node installation uses a versioned home and its real npm script path")]
    public async Task InstallsPinnedNode()
    {
        using var temporary = TemporaryWorkspace.Create("tool-private-node");
        var runtime = Runtime(temporary, out var archive);
        var processes = new NodeProcess(runtime.Version);
        var installer = new ToolPrivateNodeInstaller(new Download(archive), processes);
        var home = temporary.CreateDirectory("home");
        try
        {
            var result = await installer.InstallAsync(new(runtime, home), TestContext.Current.CancellationToken);
            Assert.Null(result.Failure);
            Assert.NotNull(result.NodePath);
            Assert.NotNull(result.NpmCliPath);
            Assert.NotNull(result.Record);
            Assert.Equal(ToolVerificationKind.SignedNodeChecksums, result.Record.Integrity.Kind);
            Assert.Equal(runtime.Archive.Sha256, result.Record.Integrity.Sha256);
            Assert.Equal(System.IO.Path.Combine(home, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName, runtime.Version), result.Record.RuntimePath);
            Assert.True(File.Exists(result.NodePath));
            Assert.True(File.Exists(result.NpmCliPath));
            Assert.Equal(1, processes.Calls);
            Assert.False(Directory.Exists(System.IO.Path.Combine(home, ToolRecordDefinitions.BinDirectoryName)));
        }
        finally { DeleteHome(temporary, home); }
    }

    [Theory(DisplayName = "Invalid private runtime evidence and wrong executable versions block publication")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BlocksInvalidRuntime(bool invalidEvidence)
    {
        using var temporary = TemporaryWorkspace.Create("tool-private-node");
        var runtime = Runtime(temporary, out var archive);
        if (invalidEvidence) runtime = runtime with { VerifiedSigner = "" };
        var home = temporary.CreateDirectory("home");
        try
        {
            var result = await new ToolPrivateNodeInstaller(new Download(archive), new NodeProcess("0.0.1"))
                .InstallAsync(new(runtime, home), TestContext.Current.CancellationToken);
            Assert.NotNull(result.Failure);
            Assert.Null(result.Record);
            Assert.False(Directory.Exists(System.IO.Path.Combine(home, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName, runtime.Version)));
        }
        finally { DeleteHome(temporary, home); }
    }

    private static ToolPrivateNodeSpec Runtime(TemporaryWorkspace temporary, out string archive)
    {
        const string version = "24.1.0";
        const string npmPath = "node_modules/npm/bin/npm-cli.js";
        archive = temporary.CreateFile("node.zip");
        using (var stream = File.Create(archive))
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            foreach (var name in new[] { ToolPrivateNodeInstaller.WindowsNodeName, npmPath })
            {
                using var entry = zip.CreateEntry($"wrapper/{name}").Open();
                entry.Write(new byte[] { 1, 2, 3 });
            }
        return new()
        {
            Version = version,
            NpmRelativePath = npmPath,
            VerifiedSigner = "synthetic-reviewed-signer",
            SignedChecksums = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/SHASUMS256.txt.asc"),
            Archive = new()
            {
                Version = version,
                Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
                Url = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/node.zip"),
                Archive = ToolArchiveKind.Zip,
                StripComponents = 1,
                EntryRelativePath = ToolPrivateNodeInstaller.WindowsNodeName,
                MaximumBytes = new FileInfo(archive).Length,
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(archive))),
            },
        };
    }

    private static void DeleteHome(TemporaryWorkspace temporary, string home)
    {
        var root = new PhysicalPathResolver().ResolveRoot(temporary.Path).GetContainedPhysicalPath();
        var resolved = new PhysicalPathResolver().ResolveCandidate(temporary.Path, root, home).GetContainedPhysicalPath();
        Assert.True(PhysicalContainment.Contains(root, resolved));
        Assert.NotEqual(root, resolved);
        Assert.Null(new DirectoryInfo(home).LinkTarget);
        Directory.Delete(home, recursive: true);
    }

    private sealed class Download(string source) : IToolDownloadHost
    {
        public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
        {
            File.Copy(source, request.DestinationPath);
            return Task.FromResult(new ToolDownloadResult(request.DestinationPath, new() { Kind = ToolVerificationKind.PinnedSha256, Source = request.Asset.Url.AbsoluteUri, Sha256 = request.Asset.Sha256 }, null));
        }
    }

    private sealed class NodeProcess(string version) : IToolProcessHost
    {
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken) => Task.FromResult<ToolResolvedCommand?>(null);
        internal int Calls { get; private set; }
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            Assert.True(File.Exists(request.ExecutablePath));
            Assert.Equal(ToolPrivateNodeInstaller.WindowsNodeName, System.IO.Path.GetFileName(request.ExecutablePath));
            return Task.FromResult(new ToolProcessResult { ExitCode = 0, Cancelled = false, TimedOut = false, StandardOutput = $"v{version}\n", StandardError = "" });
        }
    }
}
