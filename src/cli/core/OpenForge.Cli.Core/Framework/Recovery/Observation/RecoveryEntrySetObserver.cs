using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Observation;

internal static class RecoveryEntrySetObserver
{
    internal static async ValueTask<RecoveryEntrySetObservation> ReadAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        RecoveryBundleCandidateSnapshot candidate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        RecoveryEntrySetObservation.ValidateCandidate(workspace, candidate);
        var verified = candidate.Verified
            ?? throw new ArgumentException("Entry observation requires a verified final.", nameof(candidate));
        var entries = ImmutableArray.CreateBuilder<RecoveryEntryComparison>(verified.Entries.Length);
        foreach (var entry in verified.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var input = await RecoveryEntryObservationReader.ReadAsync(
                resolver, new RecoveryEntryComparisonContext(workspace, entry), cancellationToken).ConfigureAwait(false);
            entries.Add(RecoveryEntryComparer.Compare(input));
        }

        return new RecoveryEntrySetObservation(workspace, candidate, entries.ToImmutable());
    }
}
