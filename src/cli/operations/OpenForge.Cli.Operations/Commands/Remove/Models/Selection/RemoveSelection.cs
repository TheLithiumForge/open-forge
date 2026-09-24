using OpenForge.Cli.Core.Commands.Remove.Models.Request;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Selection;

internal enum RemoveSelectionState
{
    Path,
    Route,
    Extension,
    Library,
    Invalid,
}

internal sealed record RemoveSelection
{
    private RemoveSelection(
        RemoveSelectionState state,
        string target,
        string? cause)
    {
        State = state;
        Target = target;
        Cause = cause;
    }

    internal RemoveSelectionState State { get; }

    internal string Target { get; }

    internal string? Cause { get; }

    internal static RemoveSelection Selected(RemoveKind kind, string target)
        => new(kind switch
        {
            RemoveKind.Path => RemoveSelectionState.Path,
            RemoveKind.Route => RemoveSelectionState.Route,
            RemoveKind.Extension => RemoveSelectionState.Extension,
            RemoveKind.Library => RemoveSelectionState.Library,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Remove kind is not defined."),
        }, target, cause: null);

    internal static RemoveSelection Invalid(string target, string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new(RemoveSelectionState.Invalid, target, cause);
    }
}
