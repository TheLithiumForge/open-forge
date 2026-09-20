namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

internal sealed record BridgeCandidate(
    string TargetPath,
    IReadOnlyList<string> Owners,
    string PackageId,
    string SourceIdentity,
    string SourceAssetPath,
    byte[] Bytes);
