using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryRecoveryView
{

    public required LibraryRecoveryState State { get; init; }

    public required string? Path { get; init; }
}
