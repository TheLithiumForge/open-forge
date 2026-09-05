using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal sealed record RouteRemoveFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteRemoveFinding(
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Remove finding code is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        if (status == CliSemanticStatus.Complete)
        {
            throw new ArgumentException(
                "A Route Remove finding cannot carry complete status.",
                nameof(status));
        }

        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException(
                "A Route Remove finding target cannot be empty.",
                nameof(target));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        Status = status;
        Target = target;
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal RouteRemoveFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}
