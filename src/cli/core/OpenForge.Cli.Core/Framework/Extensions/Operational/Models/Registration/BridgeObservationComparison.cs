namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

internal sealed class BridgeObservationComparison
{
    internal required string ParentPath { get; init; }

    internal required string ExpectedEntry { get; init; }

    internal string? ActualEntry { get; init; }

    internal required ExtensionBridgeRegistrationState State { get; init; }

    internal string? Cause { get; init; }
}
