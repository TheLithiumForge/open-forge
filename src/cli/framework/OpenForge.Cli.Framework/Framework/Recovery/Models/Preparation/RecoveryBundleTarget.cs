using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

internal sealed record RecoveryBundleTarget
{
    private readonly PlannedFileChange? _change;
    private readonly FileStateSnapshot? _before;
    private readonly RelativeFileLinkEffect? _linkEffect;
    private readonly NoFollowLeafObservation? _linkBefore;
    private readonly bool _reversible;

    private RecoveryBundleTarget(
        PlannedFileChange change,
        FileStateSnapshot before,
        bool reversible)
    {
        _change = change;
        _before = before;
        _reversible = reversible;
    }

    private RecoveryBundleTarget(
        RelativeFileLinkEffect linkEffect,
        NoFollowLeafObservation linkBefore)
    {
        _linkEffect = linkEffect;
        _linkBefore = linkBefore;
    }

    // Existing ordinary planners consume these properties directly. A link
    // target carries the separate typed effect and deliberately has no
    // ordinary-file substitute.
    internal PlannedFileChange Change
        => _change ?? throw new InvalidOperationException(
            "A relative file-link recovery target has no ordinary file change.");

    internal FileStateSnapshot Before
        => _before ?? throw new InvalidOperationException(
            "A relative file-link recovery target has no ordinary file snapshot.");

    internal RelativeFileLinkEffect? LinkEffect => _linkEffect;

    internal NoFollowLeafObservation? LinkBefore => _linkBefore;

    internal bool RequiresRecovery
        => _linkEffect is not null
            || _reversible
            || (_change is not null && _change.Kind != PlannedFileChangeKind.Create);

    internal static RecoveryBundleTarget Create(
        PlannedFileChange change,
        FileStateSnapshot before)
        => CreateOrdinary(change, before, reversible: false);

    internal static RecoveryBundleTarget CreateReversible(
        PlannedFileChange change,
        FileStateSnapshot before)
    {
        ArgumentNullException.ThrowIfNull(change);
        if (change.Kind != PlannedFileChangeKind.Create)
        {
            throw new ArgumentException(
                "Only a prior-missing ordinary Create can use the reversible target form.",
                nameof(change));
        }

        return CreateOrdinary(change, before, reversible: true);
    }

    internal static RecoveryBundleTarget Create(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(before);

        var expectedStateMatches = effect.Kind switch
        {
            RelativeFileLinkEffectKind.Create
                => before.State == NoFollowLeafState.Missing,
            RelativeFileLinkEffectKind.Delete
                => before.State == NoFollowLeafState.RelativeFileLink
                    && before.RelativeFileLink == effect.Link,
            _ => throw new ArgumentOutOfRangeException(
                nameof(effect),
                effect.Kind,
                "The relative file-link effect kind is not defined."),
        };
        if (!expectedStateMatches)
        {
            throw new ArgumentException(
                "A relative file-link recovery target requires the exact expected no-follow leaf state.",
                nameof(before));
        }

        return new RecoveryBundleTarget(effect, before);
    }

    private static RecoveryBundleTarget CreateOrdinary(
        PlannedFileChange change,
        FileStateSnapshot before,
        bool reversible)
    {
        ArgumentNullException.ThrowIfNull(change);
        ArgumentNullException.ThrowIfNull(before);
        if (before.Expectation != change.Expectation)
        {
            throw new ArgumentException(
                "A recovery target requires the exact planned prior snapshot.",
                nameof(before));
        }

        if (change.Kind == PlannedFileChangeKind.Create)
        {
            if (before.Kind != FileExpectationKind.Missing || before.HasBytes)
            {
                throw new ArgumentException(
                    "A create recovery target requires an exact missing snapshot.",
                    nameof(before));
            }

            if (reversible is false)
            {
                return new RecoveryBundleTarget(change, before, reversible: false);
            }
        }
        else if (before.Kind != FileExpectationKind.File || !before.HasBytes)
        {
            throw new ArgumentException(
                "An existing-target recovery entry requires exact prior file bytes.",
                nameof(before));
        }

        return new RecoveryBundleTarget(change, before, reversible);
    }
}
