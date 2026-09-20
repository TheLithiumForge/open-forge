using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryResidualView
{
    public required string Path { get; init; }

    public required LibraryResidualKind Kind { get; init; }

    public required LibraryResidualState State { get; init; }
}
