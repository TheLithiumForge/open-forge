using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryMutationApplication
{

    public required LibraryApplicationState State { get; init; }

    public required LibraryVerificationState Verification { get; init; }

    public required LibraryRecoveryView Recovery { get; init; }

    public required LibraryResidualView[] Residuals { get; init; }
    public required LibraryRecordPublication RecordPublication { get; init; }

}
