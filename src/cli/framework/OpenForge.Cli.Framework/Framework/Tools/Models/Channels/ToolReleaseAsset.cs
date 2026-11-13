namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal enum ToolArchiveKind { Zip, TarGzip, File }

internal sealed record ToolAttestation
{
    public required string Repository { get; init; }
    public required string Workflow { get; init; }
    public required string SourceCommit { get; init; }
}

internal sealed record ToolReleaseAsset
{
    public required string Version { get; init; }
    public required ToolTarget Target { get; init; }
    public required Uri Url { get; init; }
    public string? Sha256 { get; init; }
    public ToolAttestation? Attestation { get; init; }
    public required ToolArchiveKind Archive { get; init; }
    public required int StripComponents { get; init; }
    public required string EntryRelativePath { get; init; }
    public required long MaximumBytes { get; init; }
}
