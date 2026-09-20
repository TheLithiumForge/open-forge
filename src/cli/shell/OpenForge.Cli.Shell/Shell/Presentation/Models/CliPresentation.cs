using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliPresentation(CliFormat Format, CliDetail Detail, IReadOnlySet<CliSeverity>? Filter = null)
{
    internal CliOutputColors Colors { get; init; } = CliOutputColors.Plain;
}
