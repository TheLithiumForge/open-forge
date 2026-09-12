using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;

internal sealed record GeneratedNavigationTargetObservation(
    string Path,
    OperationalGeneratedNavigationState State);

internal sealed record DoctorGeneratedNavigationContent(
    SourceGeneratedEntriesFacts CurrentEntries,
    IReadOnlyList<GeneratedNavigationEntry> ExpectedEntries,
    IReadOnlyList<RouteGeneratedEntryComparison> EntryComparisons);

internal sealed record DoctorGeneratedNavigationUnavailability(
    GeneratedNavigationRegionUnavailableReason Reason,
    string? Cause);

internal sealed class DoctorGeneratedNavigationTargetObservation
{
    private DoctorGeneratedNavigationTargetObservation(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content,
        DoctorGeneratedNavigationUnavailability? unavailability)
    {
        var isAvailable = state is OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed;
        if (isAvailable != (unavailability is null))
        {
            throw new ArgumentException(
                "Generated-navigation availability must match its typed unavailable reason.",
                nameof(unavailability));
        }

        Path = path;
        State = state;
        Content = content;
        Unavailability = unavailability;
    }

    internal string Path { get; }

    internal OperationalGeneratedNavigationState State { get; }

    internal DoctorGeneratedNavigationContent Content { get; }

    internal DoctorGeneratedNavigationUnavailability? Unavailability { get; }

    internal SourceGeneratedEntriesFacts CurrentEntries => Content.CurrentEntries;

    internal IReadOnlyList<GeneratedNavigationEntry> ExpectedEntries => Content.ExpectedEntries;

    internal GeneratedNavigationRegionUnavailableReason? UnavailableReason =>
        Unavailability?.Reason;

    internal string? Cause => Unavailability?.Cause;

    internal static DoctorGeneratedNavigationTargetObservation Available(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content)
    {
        if (state is not (OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed))
        {
            throw new ArgumentException(
                "An available generated-navigation target must be current or changed.",
                nameof(state));
        }

        return new(path, state, content, unavailability: null);
    }

    internal static DoctorGeneratedNavigationTargetObservation Unavailable(
        string path,
        OperationalGeneratedNavigationState state,
        DoctorGeneratedNavigationContent content,
        DoctorGeneratedNavigationUnavailability unavailability)
    {
        if (state is OperationalGeneratedNavigationState.Current
            or OperationalGeneratedNavigationState.Changed)
        {
            throw new ArgumentException(
                "An unavailable generated-navigation target cannot be current or changed.",
                nameof(state));
        }

        return new(path, state, content, unavailability);
    }
}
