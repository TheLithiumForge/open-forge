using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusInstallation(
    OperationalInstallationState State,
    string? EntryPath,
    string? LoaderPath);

internal sealed record StatusRootCategories
{
    public required StatusIntegerValue Count { get; init; }

    public required IReadOnlyList<string> Added { get; init; }

    public required IReadOnlyList<string> Removed { get; init; }
}
internal sealed record StatusGeneratedNavigation(
    string Path,
    OperationalGeneratedNavigationState State);

internal sealed record StatusStructure
{
    public required StatusRootCategories RootCategories { get; init; }

    public required IReadOnlyList<StatusGeneratedNavigation> GeneratedNavigation { get; init; }
}
