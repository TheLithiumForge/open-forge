using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal enum ToolVerificationKind { PackageManager, PinnedSha256, Attestation, SignedNodeChecksums }

internal sealed record ToolIntegrityEvidence
{
    public required ToolVerificationKind Kind { get; init; }
    public required string Source { get; init; }
    public string? Sha256 { get; init; }
    public string? VerifiedIdentity { get; init; }
}

internal sealed record ToolMachineState
{
    public required int SchemaVersion { get; init; }
    public required string ToolId { get; init; }
    public required string Version { get; init; }
    public required string ChannelId { get; init; }
    public ToolPackageManagerFacts? PackageManager { get; init; }
    public required string CommandPath { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public string? ToolPath { get; init; }
    public string? RuntimePath { get; init; }
    public required bool PlacedByOpenForge { get; init; }
    public required IReadOnlyList<ToolLauncherSpec> Launchers { get; init; }
    public required IReadOnlyList<ToolIntegrityEvidence> Integrity { get; init; }
}

internal sealed record ToolNodeRuntimeRecord
{
    public required int SchemaVersion { get; init; }
    public required string Version { get; init; }
    public required string RuntimePath { get; init; }
    public required ToolIntegrityEvidence Integrity { get; init; }
}
