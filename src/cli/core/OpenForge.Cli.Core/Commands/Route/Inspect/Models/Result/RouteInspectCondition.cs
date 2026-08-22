using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal enum RouteInspectConditionCode
{
    InvalidWorkspace,
    WorkspaceUnavailable,
    UnsafeWorkspace,
    MissingSource,
    MultipleSources,
    InvalidSourceReference,
    LoaderSubject,
    UnknownSource,
    MissingSourceFile,
    UnsupportedSource,
    AmbiguousSource,
    UnsafeSource,
    AmbiguousRoute,
    OrphanOverwrite,
    AmbiguousOverwrite,
    UnreadableSource,
    IncompleteRoute,
    UnavailableFact,
    OperationFailed,
    Interrupted,
}

internal sealed class RouteInspectCondition
{
    internal RouteInspectCondition(
        RouteInspectConditionCode code,
        CliSemanticStatus status,
        string subject,
        string message,
        IEnumerable<string>? paths = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect condition code is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        ValidateCodeStatus(code, status);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Status = status;
        Subject = subject;
        Message = message;
        Paths = MaterializePaths(paths);
    }

    internal RouteInspectConditionCode Code { get; }

    internal string MachineCode => RouteInspectDefinitions.ReadConditionCode(Code);

    internal CliSemanticStatus Status { get; }

    internal string Subject { get; }

    internal string Message { get; }

    internal IReadOnlyList<string> Paths { get; }

    private static void ValidateCodeStatus(RouteInspectConditionCode code, CliSemanticStatus status)
    {
        var expectedStatus = code switch
        {
            RouteInspectConditionCode.InvalidWorkspace
                or RouteInspectConditionCode.MissingSource
                or RouteInspectConditionCode.MultipleSources
                or RouteInspectConditionCode.InvalidSourceReference
                or RouteInspectConditionCode.LoaderSubject
                or RouteInspectConditionCode.UnknownSource
                or RouteInspectConditionCode.MissingSourceFile
                or RouteInspectConditionCode.UnsupportedSource => CliSemanticStatus.Invalid,
            RouteInspectConditionCode.AmbiguousSource
                or RouteInspectConditionCode.WorkspaceUnavailable
                or RouteInspectConditionCode.UnsafeWorkspace
                or RouteInspectConditionCode.UnsafeSource
                or RouteInspectConditionCode.AmbiguousRoute
                or RouteInspectConditionCode.OrphanOverwrite
                or RouteInspectConditionCode.AmbiguousOverwrite => CliSemanticStatus.Blocked,
            RouteInspectConditionCode.UnreadableSource
                or RouteInspectConditionCode.IncompleteRoute
                or RouteInspectConditionCode.UnavailableFact => CliSemanticStatus.Incomplete,
            RouteInspectConditionCode.OperationFailed => CliSemanticStatus.Failed,
            RouteInspectConditionCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The route-inspect condition code is not defined."),
        };
        if (status != expectedStatus)
        {
            throw new ArgumentException(
                "The route-inspect condition status does not match its code.",
                nameof(status));
        }
    }

    private static IReadOnlyList<string> MaterializePaths(IEnumerable<string>? paths)
    {
        var materialized = paths?.ToArray() ?? [];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in materialized)
        {
            if (path is null)
            {
                throw new ArgumentException("Condition paths cannot contain null.", nameof(paths));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            if (!seen.Add(path))
            {
                throw new ArgumentException("Condition paths must be unique.", nameof(paths));
            }
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}
