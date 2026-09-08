using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Operational;

internal interface IRecoveryResidualOperationalContributor
{
    ValueTask<RecoveryResidualStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);

    ValueTask<RecoveryResidualDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class RecoveryResidualOperationalContributor(
    RecoveryBundleTargetStateReader targetStateReader,
    PhysicalPathResolver physicalPathResolver) : IRecoveryResidualOperationalContributor
{
    internal static async ValueTask<RecoveryResidualStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var result = await RecoveryBundleCatalogue.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return new RecoveryResidualStatusView(ReadState(result.State), ReadCandidates(result));
    }

    internal async ValueTask<RecoveryResidualDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var result = await RecoveryBundleCatalogue.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (result.State != RecoveryBundleCatalogueState.Available)
        {
            return new RecoveryResidualDoctorView(
                ReadState(result.State),
                [],
                result.Cause);
        }

        var candidates = new List<RecoveryDoctorCandidateObservation>(result.Candidates.Length);
        try
        {
            foreach (var candidate in result.Candidates)
            {
                var verified = candidate.Verified;
                var comparison = verified?.Attribution.Producer == RecoveryBundleProducer.Framework
                    ? await targetStateReader
                        .ReadAsync(workspace, verified, cancellationToken)
                        .ConfigureAwait(false)
                    : null;
                var entryComparisons = verified?.Attribution.Producer == RecoveryBundleProducer.Library
                    ? await RecoveryEntrySetObserver.ReadAsync(physicalPathResolver, workspace, candidate, cancellationToken).ConfigureAwait(false)
                    : null;
                candidates.Add(RecoveryDoctorCandidateObservation.Create(
                    candidate,
                    comparison,
                    entryComparisons));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new RecoveryResidualDoctorView(
                OperationalViewState.Interrupted,
                [.. candidates],
                "Recovery target comparison was interrupted.");
        }

        return new RecoveryResidualDoctorView(
            OperationalViewState.Complete,
            [.. candidates],
            null);
    }

    ValueTask<RecoveryResidualStatusView> IRecoveryResidualOperationalContributor.ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, cancellationToken);

    ValueTask<RecoveryResidualDoctorView> IRecoveryResidualOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private static OperationalViewState ReadState(RecoveryBundleCatalogueState state)
        => state switch
        {
            RecoveryBundleCatalogueState.Available => OperationalViewState.Complete,
            RecoveryBundleCatalogueState.Unavailable => OperationalViewState.Incomplete,
            RecoveryBundleCatalogueState.Cancelled => OperationalViewState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The recovery-catalogue state is not defined."),
        };

    private static RecoveryCandidateObservation[] ReadCandidates(
        RecoveryBundleCatalogueResult result)
        => [.. result.Candidates.Select(candidate => new RecoveryCandidateObservation(
                candidate.Path,
                candidate.Kind,
                candidate.Integrity))];
}
