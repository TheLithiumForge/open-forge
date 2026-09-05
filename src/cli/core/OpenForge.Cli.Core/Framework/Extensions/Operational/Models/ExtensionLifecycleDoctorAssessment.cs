using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal enum ExtensionLifecycleSectionState
{
    Present,
    DocumentMissing,
    SectionMissing,
    Invalid,
    Unavailable,
    Blocked,
    Interrupted,
}

internal enum ExtensionManagedSetState
{
    Empty,
    Current,
    NonCurrent,
    Mixed,
    Unavailable,
}

internal sealed class ExtensionLifecycleDoctorAssessment
{
    private ExtensionLifecycleDoctorAssessment(
        OperationalViewState state,
        ExtensionLifecycleSectionState section,
        ExtensionManagedSetState managedSet,
        OperationalLifecycleState lifecycle,
        OperationalSourceAvailability sourceAvailability)
    {
        ValidateEnum(state, nameof(state));
        ValidateEnum(section, nameof(section));
        ValidateEnum(managedSet, nameof(managedSet));
        ValidateEnum(lifecycle, nameof(lifecycle));
        ValidateEnum(sourceAvailability, nameof(sourceAvailability));

        var compatible = section switch
        {
            ExtensionLifecycleSectionState.Present => true,
            ExtensionLifecycleSectionState.Blocked => state == OperationalViewState.Blocked,
            ExtensionLifecycleSectionState.Interrupted => state == OperationalViewState.Interrupted,
            ExtensionLifecycleSectionState.DocumentMissing
                or ExtensionLifecycleSectionState.SectionMissing
                or ExtensionLifecycleSectionState.Invalid
                or ExtensionLifecycleSectionState.Unavailable =>
                state == OperationalViewState.Incomplete,
            _ => false,
        };
        if (!compatible)
        {
            throw new ArgumentException(
                "The Extension lifecycle section and view states are incompatible.",
                nameof(section));
        }

        State = state;
        Section = section;
        ManagedSet = managedSet;
        Lifecycle = lifecycle;
        SourceAvailability = sourceAvailability;
    }

    internal OperationalViewState State { get; }

    internal ExtensionLifecycleSectionState Section { get; }

    internal ExtensionManagedSetState ManagedSet { get; }

    internal OperationalLifecycleState Lifecycle { get; }

    internal OperationalSourceAvailability SourceAvailability { get; }

    internal static ExtensionLifecycleDoctorAssessment Create(
        OperationalViewState state,
        ExtensionLifecycleSectionState section,
        ExtensionManagedSetState managedSet,
        OperationalLifecycleState lifecycle,
        OperationalSourceAvailability sourceAvailability)
        => new(state, section, managedSet, lifecycle, sourceAvailability);

    private static void ValidateEnum<T>(T value, string name)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                $"The {typeof(T).Name} value is not defined.");
        }
    }
}
