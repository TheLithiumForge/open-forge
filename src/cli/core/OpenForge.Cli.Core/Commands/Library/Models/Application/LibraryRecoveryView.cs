using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryRecoveryView
{

    public required LibraryRecoveryState State { get; init; }

    public required string? Path { get; init; }
}
