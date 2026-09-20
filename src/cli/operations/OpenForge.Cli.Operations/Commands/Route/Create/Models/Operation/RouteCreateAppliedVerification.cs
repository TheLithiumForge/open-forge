namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;

internal enum RouteCreateAppliedVerificationState
{
    Verified,
    Failed,
    Cancelled,
}

internal sealed record RouteCreateAppliedVerification
{
    public required RouteCreateAppliedVerificationState State { get; init; }

    public string? Cause { get; init; }
}
