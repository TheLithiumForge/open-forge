using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

namespace OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

internal sealed record LibraryStatusView
{
    public required OperationalViewState State { get; init; }
    public required LifecycleOwnershipReadResult? Ownership { get; init; }
    public required LibraryLinkCapabilityFact? LinkCapability { get; init; }
    public required LibrariesRecordRead Record { get; init; }
    public required ImmutableArray<LibrarySourceRootObservation> Sources { get; init; }
    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }
}

internal sealed record LibraryDoctorView
{
    public required OperationalViewState State { get; init; }
    public required LifecycleOwnershipReadResult? Ownership { get; init; }
    public required LibraryLinkCapabilityFact? LinkCapability { get; init; }
    public required LibrariesRecordRead Record { get; init; }
    public required ImmutableArray<LibraryInventoryRead> Inventories { get; init; }
    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }
}

internal enum LibraryLinkCapabilityState
{
    Supported,
    Unsupported,
}

internal sealed record LibraryLinkCapabilityFact
{
    internal LibraryLinkCapabilityFact(LibraryLinkCapabilityState state, string evidence)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The proven Library link capability state is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(evidence);
        State = state;
        Evidence = evidence;
    }

    internal LibraryLinkCapabilityState State { get; }
    internal string Evidence { get; }
}
