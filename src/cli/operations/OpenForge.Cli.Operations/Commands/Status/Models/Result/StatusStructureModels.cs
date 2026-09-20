namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusInstallation(
    StatusInstallationState State,
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
    StatusGeneratedNavigationState State);

internal sealed record StatusStructure
{
    public required StatusRootCategories RootCategories { get; init; }

    public required IReadOnlyList<StatusGeneratedNavigation> GeneratedNavigation { get; init; }
}
