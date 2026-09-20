using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal sealed record RouteRemoveFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteRemoveFinding(
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause,
        SourceLocation? location = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Remove finding code is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        if (status == CliSemanticStatus.Complete && code != RouteRemoveFindingCode.OwnershipUnavailable)
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
        Location = location;
    }

    internal RouteRemoveFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }

    internal SourceLocation? Location { get; }

    internal CommandSourceLocation? LocationView => Location is { } location
        ? CommandSourceLocation.From(location)
        : null;
}
