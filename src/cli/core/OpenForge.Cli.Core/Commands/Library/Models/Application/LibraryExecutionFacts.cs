namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal enum LibraryExecutionStage
{
    Preflight,
    RecoveryPreparation,
    Application,
    Verification,
    RecoveryCleanup,
    Completed,
}

internal enum LibraryRecordPublicationOrder
{
    NotObserved,
    Last,
    NotLast,
}

internal sealed record LibraryCancellationFact
{
    internal LibraryCancellationFact(LibraryExecutionStage stage)
    {
        if (!Enum.IsDefined(stage))
        {
            throw new ArgumentOutOfRangeException(nameof(stage), stage, "The Library execution stage is not defined.");
        }

        Stage = stage;
    }

    internal LibraryExecutionStage Stage { get; }
}

internal sealed record LibraryUnexpectedFailureFact
{
    internal LibraryUnexpectedFailureFact(LibraryExecutionStage stage, string cause)
    {
        if (!Enum.IsDefined(stage))
        {
            throw new ArgumentOutOfRangeException(nameof(stage), stage, "The Library execution stage is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Stage = stage;
        Cause = cause;
    }

    internal LibraryExecutionStage Stage { get; }
    internal string Cause { get; }
}
