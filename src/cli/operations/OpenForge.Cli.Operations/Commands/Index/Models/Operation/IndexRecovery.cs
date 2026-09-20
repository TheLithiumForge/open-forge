namespace OpenForge.Cli.Core.Commands.Index.Models.Operation;

internal enum IndexRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal sealed record IndexRecovery
{
    internal IndexRecovery(IndexRecoveryState state, string? residualPath)
    {
        var coherent = state switch
        {
            IndexRecoveryState.NotRequired
                or IndexRecoveryState.NotCreated
                or IndexRecoveryState.Removed => residualPath is null,
            IndexRecoveryState.Retained => !string.IsNullOrWhiteSpace(residualPath),
            IndexRecoveryState.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Index recovery state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException("The Index recovery residual path does not match its state.", nameof(residualPath));
        }

        if (residualPath is not null
            && (!Path.IsPathFullyQualified(residualPath)
                || !string.Equals(Path.GetFullPath(residualPath), residualPath, StringComparison.Ordinal)))
        {
            throw new ArgumentException("An Index recovery residual path must be absolute and normalized.", nameof(residualPath));
        }

        State = state;
        ResidualPath = residualPath;
    }

    internal IndexRecoveryState State { get; }

    internal string? ResidualPath { get; }

    internal static IndexRecovery NotRequired { get; } = new(IndexRecoveryState.NotRequired, null);
}
