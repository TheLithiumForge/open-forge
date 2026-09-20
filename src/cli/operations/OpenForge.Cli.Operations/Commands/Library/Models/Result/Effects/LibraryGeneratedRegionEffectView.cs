using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryGeneratedRegionEffectView
{
    public required string Path { get; init; }

    public required string ExpectedSha256 { get; init; }

    public required string IntendedSha256 { get; init; }
    public required LibraryExpectedState Expected { get; init; }

}
