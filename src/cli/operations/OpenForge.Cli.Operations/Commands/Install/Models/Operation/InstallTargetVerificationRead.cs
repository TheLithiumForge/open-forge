using OpenForge.Cli.Core.Commands.Install.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Install.Models.Operation;

internal sealed record InstallTargetVerificationRead(
    IReadOnlyDictionary<string, InstallTargetRead> Reads,
    InstallVerificationResult? Result);
