using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Supply;

internal sealed record ToolArchiveMember(string Name, string Path, long Bytes, bool Directory, UnixFileMode Mode);
internal sealed record ToolArchivePlan(ToolArchiveMember[] Members, long ExpandedBytes);
internal sealed record ToolArtifactExtraction(ToolArtifact Artifact, string ArchivePath, string DestinationRoot);
internal sealed record ToolExtractedArtifact(ToolFileReceipt[] Files, string[] Directories);
