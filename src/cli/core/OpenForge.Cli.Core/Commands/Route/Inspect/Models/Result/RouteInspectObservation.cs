using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Inspect;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal enum RouteInspectObservationCode
{
    AutomaticIdNotUnique,
    CompatibilityEntrypoint,
    DetachedSource,
    NotRouted,
    ValidOverwrite,
}

internal sealed class RouteInspectObservation
{
    internal RouteInspectObservation(
        RouteInspectObservationCode code,
        string subject,
        string message,
        IEnumerable<string>? paths = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect observation code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Subject = subject;
        Message = message;
        Paths = MaterializePaths(paths);
    }

    internal RouteInspectObservationCode Code { get; }

    internal string MachineCode => RouteInspectDefinitions.ReadObservationCode(Code);

    internal string Subject { get; }

    internal string Message { get; }

    internal IReadOnlyList<string> Paths { get; }

    private static IReadOnlyList<string> MaterializePaths(IEnumerable<string>? paths)
    {
        var materialized = paths?.ToArray() ?? [];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in materialized)
        {
            if (path is null)
            {
                throw new ArgumentException("Observation paths cannot contain null.", nameof(paths));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            if (!seen.Add(path))
            {
                throw new ArgumentException("Observation paths must be unique.", nameof(paths));
            }
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}
