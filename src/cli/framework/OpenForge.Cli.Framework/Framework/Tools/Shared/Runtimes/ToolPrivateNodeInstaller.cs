using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;

internal sealed class ToolPrivateNodeInstaller(IToolDownloadHost downloads, IToolProcessHost processes)
{
    internal const string WindowsNodeName = "node.exe";
    internal const string PosixNodePath = "bin/node";

    internal async Task<ToolPrivateNodeResult> InstallAsync(ToolPrivateNodeRequest request, CancellationToken token)
    {
        var runtime = request.Runtime;
        if (!IsReviewed(runtime))
            return new(null, null, null, "The private Node descriptor lacks reviewed official signed-checksum evidence.");
        var parent = System.IO.Path.Combine(request.HomePath, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName);
        var destination = System.IO.Path.Combine(parent, runtime.Version);
        try
        {
            var resolver = new PhysicalPathResolver();
            var home = resolver.ResolveRoot(request.HomePath).GetContainedPhysicalPath();
            if (resolver.ResolveCandidate(request.HomePath, home, destination).State != PhysicalPathState.Missing)
                return new(null, null, null, "The runtime destination already exists. Verify its machine receipt before reuse.");
            Directory.CreateDirectory(parent);
            var staging = Directory.CreateDirectory(System.IO.Path.Combine(parent, $"prepare-{Guid.NewGuid():N}")).FullName;
            var archive = System.IO.Path.Combine(staging, "download");
            var result = await downloads.DownloadAsync(new(runtime.Archive, archive), token).ConfigureAwait(false);
            if (result.VerifiedPath is null || result.Integrity is null) return new(null, null, null, result.Failure);
            var extracted = System.IO.Path.Combine(staging, "extracted");
            Directory.CreateDirectory(extracted);
            var prepared = await ToolNodeArchivePreparation.PrepareAsync(new(runtime.Archive, archive, extracted, ToolArchivePreparation.OmitLinks),
                System.IO.Path.Combine(staging, "prepared"), token).ConfigureAwait(false);
            await ToolArchiveExtractor.ExtractAsync(prepared, token).ConfigureAwait(false);
            var nodeRelative = runtime.Archive.Target.OperatingSystem == ToolOperatingSystem.Windows ? WindowsNodeName : PosixNodePath;
            var stagedNode = System.IO.Path.Combine(extracted, nodeRelative.Replace('/', System.IO.Path.DirectorySeparatorChar));
            var stagedNpm = System.IO.Path.Combine(extracted, runtime.NpmRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (!File.Exists(stagedNode) || !File.Exists(stagedNpm)) return new(null, null, null, "The portable Node archive lacks Node or its npm CLI script.");
            var probe = await processes.RunAsync(new()
            {
                ExecutablePath = stagedNode,
                Arguments = ["--version"],
                WorkingDirectory = extracted,
                Environment = new Dictionary<string, string?>(),
                Timeout = TimeSpan.FromSeconds(10),
                MaximumOutputBytes = 4096,
            }, token).ConfigureAwait(false);
            if (probe.ExitCode != 0 || probe.Cancelled || probe.TimedOut || probe.Failure is not null || ToolVersionService.ReadProbe(probe.StandardOutput) != runtime.Version)
                return new(null, null, null, "The extracted Node executable didn't verify its pinned version.");
            if (resolver.ResolveCandidate(request.HomePath, home, destination).State != PhysicalPathState.Missing)
                return new(null, null, null, "The runtime destination changed before publication.");
            Directory.Move(extracted, destination);
            return new(System.IO.Path.Combine(destination, nodeRelative.Replace('/', System.IO.Path.DirectorySeparatorChar)),
                System.IO.Path.Combine(destination, runtime.NpmRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar)), Record(runtime, request.HomePath), null);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or OperationCanceledException or InvalidOperationException)
        {
            return new(null, null, null, "Private Node preparation failed. Keep the preparation files for checking before retrying.");
        }
    }

    internal static bool IsReviewed(ToolPrivateNodeSpec runtime)
        => ToolArtifactSource.IsOfficial(runtime.Archive) && runtime.Archive.Url.Host == ToolArtifactSource.NodeHost
            && runtime.Archive.Version == runtime.Version && ToolVersionService.ReadStable(runtime.Version) is not null
            && ToolArtifactIntegrity.IsSha256(runtime.Archive.Sha256) && !string.IsNullOrWhiteSpace(runtime.VerifiedSigner)
            && ToolArtifactSource.IsHttps(runtime.SignedChecksums) && runtime.SignedChecksums.Host == ToolArtifactSource.NodeHost
            && runtime.SignedChecksums.AbsolutePath == $"/dist/v{runtime.Version}/SHASUMS256.txt.asc"
            && PortableWorkspacePath.TryNormalize(runtime.NpmRelativePath, out _);

    internal static ToolNodeRuntimeRecord Record(ToolPrivateNodeSpec runtime, string home) => new()
    {
        SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
        Version = runtime.Version,
        RuntimePath = ToolSupplyPaths.Runtime(home, runtime.Version),
        Integrity = new() { Kind = ToolVerificationKind.SignedNodeChecksums, Source = runtime.SignedChecksums.AbsoluteUri, Sha256 = runtime.Archive.Sha256, VerifiedIdentity = runtime.VerifiedSigner },
    };

    internal static bool Matches(ToolPrivateNodeSpec runtime, ToolNodeRuntimeRecord record, string home)
        => record.Version == runtime.Version && ToolSupplyPaths.Comparer.Equals(record.RuntimePath, ToolSupplyPaths.Runtime(home, runtime.Version))
            && record.Integrity.Kind == ToolVerificationKind.SignedNodeChecksums && record.Integrity.Sha256 == runtime.Archive.Sha256
            && record.Integrity.Source == runtime.SignedChecksums.AbsoluteUri && record.Integrity.VerifiedIdentity == runtime.VerifiedSigner;
}
