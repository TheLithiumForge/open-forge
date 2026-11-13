using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolDownloadRequest(ToolReleaseAsset Asset, string DestinationPath);
internal sealed record ToolDownloadResult(string? VerifiedPath, ToolIntegrityEvidence? Integrity, string? Failure);
internal sealed record ToolExtractionRequest(ToolReleaseAsset Asset, string ArchivePath, string DestinationPath);
internal sealed record ToolExtractionResult(IReadOnlyList<string> ExtractedPaths, string? Failure);
internal sealed record ToolPrivateNodeRequest(ToolPrivateNodeSpec Runtime, string HomePath);
internal sealed record ToolPrivateNodeResult(string? NodePath, string? NpmCliPath, ToolNodeRuntimeRecord? Record, string? Failure);

internal sealed record ToolTransferProgress(long ReceivedBytes, long? TotalBytes);
