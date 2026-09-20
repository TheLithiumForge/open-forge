using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal sealed record RouteInitFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteInitFinding(
        RouteInitFindingCode code,
        string cause,
        string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException(
                "A Route Init finding target cannot be empty.",
                nameof(target));
        }

        Code = code;
        Status = RouteInitDefinitions.ReadStatus(code);
        Target = target;
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal RouteInitFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}
