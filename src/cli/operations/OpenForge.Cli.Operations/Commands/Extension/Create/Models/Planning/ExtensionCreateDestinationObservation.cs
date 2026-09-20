namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;

internal enum ExtensionCreateDestinationState
{
    Absent,
    Exact,
    Collision,
    Unsafe,
    Unavailable,
}

internal sealed record ExtensionCreateDestinationObservation
{
    internal ExtensionCreateDestinationObservation(
        ExtensionCreateDestinationState state,
        string? physicalIdentity,
        string? cause)
    {
        State = state;
        PhysicalIdentity = physicalIdentity;
        Cause = cause;
    }

    internal ExtensionCreateDestinationState State { get; }

    internal string? PhysicalIdentity { get; }

    internal string? Cause { get; }
}
