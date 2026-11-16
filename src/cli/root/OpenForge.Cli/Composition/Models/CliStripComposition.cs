using OpenForge.Cli.Core.Shell.Composition;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliStripComposition
{
    public required ICliCommandBinding StripBinding { get; init; }

    public required ICliCommandBinding UninstallBinding { get; init; }
}
