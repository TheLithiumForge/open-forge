using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

internal sealed record RouteMoveFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteMoveFinding(
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Move finding code is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        if (status == CliSemanticStatus.Complete && code != RouteMoveFindingCode.OwnershipUnavailable)
        {
            throw new ArgumentException(
                "A Route Move finding cannot carry complete status.",
                nameof(status));
        }

        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException(
                "A Route Move finding target cannot be empty.",
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

    internal RouteMoveFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}
