using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliPresentation(CliFormat Format, CliDetail Detail, IReadOnlySet<CliSeverity>? Filter = null)
{
    internal const int DefaultWidth = 80;
    internal CliOutputColors Colors { get; init; } = CliOutputColors.Plain;
    internal int Width { get; init; } = DefaultWidth;
}
