using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolNpmMetadataRequest(ToolNpmChannel Channel, string WorkspacePath);
internal sealed record ToolNpmVersionMetadata(string Version, string EntryRelativePath, string? NodeRange);
internal sealed record ToolNpmMetadataResult(IReadOnlyList<ToolNpmVersionMetadata> Versions, Uri? Registry, string? Failure);
