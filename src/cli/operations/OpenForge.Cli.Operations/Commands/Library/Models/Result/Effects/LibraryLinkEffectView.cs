using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryLinkEffectView
{
    internal bool IsImplicitContentDestination { get; init; }
    public required string Path { get; init; }

    public required LibraryLinkEffectKind Kind { get; init; }

    public required string RelativeTarget { get; init; }
    public required LibraryExpectedState Expected { get; init; }

}
