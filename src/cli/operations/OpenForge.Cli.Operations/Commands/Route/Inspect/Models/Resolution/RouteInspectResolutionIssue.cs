using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal enum RouteInspectResolutionIssueCode
{
    InvalidReference,
    LoaderSubject,
    UnknownSource,
    MissingSource,
    UnsupportedSource,
    AmbiguousSource,
    UnsafeSource,
    AmbiguousRoute,
    OrphanOverwrite,
    AmbiguousOverwrite,
    ReadUnavailable,
    IncompleteRoute,
    OperationFailure,
    Interrupted,
}

internal sealed class RouteInspectResolutionIssue
{
    internal RouteInspectResolutionIssue(
        RouteInspectResolutionIssueCode code,
        string subject,
        string message,
        IEnumerable<string>? paths = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect resolution issue code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Subject = subject;
        Message = message;
        Paths = MaterializePaths(paths);
    }

    internal RouteInspectResolutionIssueCode Code { get; }

    internal string Subject { get; }

    internal string Message { get; }

    internal IReadOnlyList<string> Paths { get; }

    private static IReadOnlyList<string> MaterializePaths(IEnumerable<string>? paths)
    {
        var materialized = paths?.ToArray() ?? [];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < materialized.Length; index++)
        {
            var path = materialized[index];
            if (path is null)
            {
                throw new ArgumentException("Resolution issue paths cannot contain null.", nameof(paths));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            if (!seen.Add(path)
                || index > 0 && string.CompareOrdinal(materialized[index - 1], path) >= 0)
            {
                throw new ArgumentException("Resolution issue paths must be unique strict ordinal order.", nameof(paths));
            }
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}
