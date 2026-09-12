namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallProjectionInputRead(
    InstallProjectionInputObservation? Observation,
    ReadOnlyMemory<byte> Bytes,
    InstallIntendedStateBuild? Build);
