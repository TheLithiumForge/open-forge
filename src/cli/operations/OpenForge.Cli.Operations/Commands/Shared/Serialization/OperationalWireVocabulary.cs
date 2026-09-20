using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Serialization;

/// <summary>
/// Owns the wire names for the operational states shared by every command that
/// reports them, so each command answers the question the same way.
/// </summary>
internal static class OperationalWireVocabulary
{
    internal static string ValueState(OperationalValueState value)
        => value switch
        {
            OperationalValueState.Available => "available",
            OperationalValueState.Unavailable => "unavailable",
            OperationalValueState.NotApplicable => "not-applicable",
            _ => Undefined(nameof(value), value),
        };

    internal static string LifecycleState(OperationalLifecycleState value)
        => value switch
        {
            OperationalLifecycleState.Absent => "absent",
            OperationalLifecycleState.Trusted => "trusted",
            OperationalLifecycleState.Untrusted => "untrusted",
            OperationalLifecycleState.Incomplete => "incomplete",
            OperationalLifecycleState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string SourceAvailability(OperationalSourceAvailability value)
        => value switch
        {
            OperationalSourceAvailability.Available => "available",
            OperationalSourceAvailability.Unavailable => "unavailable",
            OperationalSourceAvailability.NotApplicable => "not-applicable",
            _ => Undefined(nameof(value), value),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            "The operational wire value is not defined.");
}
