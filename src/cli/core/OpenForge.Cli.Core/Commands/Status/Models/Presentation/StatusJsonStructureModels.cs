namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusJsonStructure
{
    public required StatusJsonRootCategories RootCategories { get; init; }

    public required StatusJsonGeneratedNavigation[] GeneratedNavigation { get; init; }
}
internal sealed class StatusJsonRootCategories
{
    public required StatusJsonIntegerValue Count { get; init; }

    public required string[] Added { get; init; }

    public required string[] Removed { get; init; }
}

internal sealed class StatusJsonGeneratedNavigation
{
    public required string Path { get; init; }

    public required string State { get; init; }
}
