using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryLinkEffectView
{
    public required string Path { get; init; }

    public required LibraryLinkEffectKind Kind { get; init; }

    public required string RawRelativeTarget { get; init; }
    public required LibraryExpectedState Expected { get; init; }

}
