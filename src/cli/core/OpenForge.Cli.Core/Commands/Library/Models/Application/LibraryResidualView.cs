using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryResidualView
{
    public required string Path { get; init; }

    public required LibraryResidualKind Kind { get; init; }

    public required LibraryResidualState State { get; init; }
}
