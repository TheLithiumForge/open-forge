namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal enum RouteInitApplicationAttemptKind
{
    Directory,
    File,
}

internal sealed record RouteInitApplicationAttempt
{
    internal RouteInitApplicationAttempt(
        RouteInitApplicationAttemptKind kind,
        string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        if (!Path.IsPathFullyQualified(logicalPath))
        {
            throw new ArgumentException(
                "A Route Init attempted effect requires an absolute logical path.",
                nameof(logicalPath));
        }

        Kind = kind;
        LogicalPath = Path.GetFullPath(logicalPath);
    }

    internal RouteInitApplicationAttemptKind Kind { get; }

    internal string LogicalPath { get; }
}
