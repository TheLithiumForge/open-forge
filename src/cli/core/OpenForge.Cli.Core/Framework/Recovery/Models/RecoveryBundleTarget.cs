using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

internal sealed record RecoveryBundleTarget
{
    private RecoveryBundleTarget(
        PlannedFileChange change,
        FileStateSnapshot before)
    {
        Change = change;
        Before = before;
    }

    internal PlannedFileChange Change { get; }

    internal FileStateSnapshot Before { get; }

    internal bool RequiresRecovery => Change.Kind != PlannedFileChangeKind.Create;

    internal static RecoveryBundleTarget Create(
        PlannedFileChange change,
        FileStateSnapshot before)
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
        }
        else if (before.Kind != FileExpectationKind.File || !before.HasBytes)
        {
            throw new ArgumentException(
                "An existing-target recovery entry requires exact prior file bytes.",
                nameof(before));
        }

        return new RecoveryBundleTarget(change, before);
    }
}
