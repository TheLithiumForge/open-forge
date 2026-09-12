using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational.Shared.Evaluation;

internal static class FrameworkLifecycleEvaluation
{
    internal static OperationalLifecyclePresenceState ReadPresence(
        LifecycleStoreReadState state)
        => state switch
        {
            LifecycleStoreReadState.Available
                or LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => OperationalLifecyclePresenceState.Present,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing => OperationalLifecyclePresenceState.Missing,
            LifecycleStoreReadState.Unavailable
                or LifecycleStoreReadState.Cancelled => OperationalLifecyclePresenceState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework lifecycle presence state is not defined."),
        };
}
