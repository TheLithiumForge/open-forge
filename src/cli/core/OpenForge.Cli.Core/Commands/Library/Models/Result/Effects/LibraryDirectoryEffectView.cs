using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryDirectoryEffectView
{
    public required string Path { get; init; }

    public required LibraryExpectedState Expected { get; init; }
}
