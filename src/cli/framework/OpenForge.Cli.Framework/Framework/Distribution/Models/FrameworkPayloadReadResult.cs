namespace OpenForge.Cli.Core.Framework.Distribution.Models;

internal enum FrameworkPayloadReadState
{
    Available,
    Unavailable,
    Invalid,
}

internal sealed class FrameworkPayloadReadResult
{
    private FrameworkPayloadReadResult(
        FrameworkPayloadReadState state,
        FrameworkPayload? payload,
        string? cause)
    {
        State = state;
        Payload = payload;
        Cause = cause;
    }

    internal FrameworkPayloadReadState State { get; }

    internal FrameworkPayload? Payload { get; }

    internal string? Cause { get; }

    internal static FrameworkPayloadReadResult Available(FrameworkPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return new FrameworkPayloadReadResult(
            FrameworkPayloadReadState.Available,
            payload,
            cause: null);
    }

    internal static FrameworkPayloadReadResult Unavailable(string cause)
        => new(
            FrameworkPayloadReadState.Unavailable,
            payload: null,
            ValidateCause(cause));

    internal static FrameworkPayloadReadResult Invalid(string cause)
        => new(
            FrameworkPayloadReadState.Invalid,
            payload: null,
            ValidateCause(cause));

    private static string ValidateCause(string cause)
    {
        if (string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException("A nonempty Framework payload read cause is required.", nameof(cause));
        }

        return cause;
    }
}
