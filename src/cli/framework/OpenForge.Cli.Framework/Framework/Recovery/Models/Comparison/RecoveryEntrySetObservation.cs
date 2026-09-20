using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

internal sealed record RecoveryEntrySetObservation
{
    internal RecoveryEntrySetObservation(
        CliWorkspace workspace,
        RecoveryBundleCandidateSnapshot candidate,
        ImmutableArray<RecoveryEntryComparison> entries)
    {
        ValidateCandidate(workspace, candidate);
        var verified = candidate.Verified
            ?? throw new ArgumentException("Entry comparisons require a verified final.", nameof(candidate));
        if (entries.IsDefault || entries.Length != verified.Entries.Length)
        {
            throw new ArgumentException("Entry comparisons require the complete ordered verified entry set.", nameof(entries));
        }

        for (var index = 0; index < entries.Length; index++)
        {
            var comparison = entries[index];
            if (comparison is null || comparison.Input.Context.Entry != verified.Entries[index]
                || comparison.Input.Context.Workspace != workspace)
            {
                throw new ArgumentException("Each comparison must retain its exact ordered entry and workspace context.", nameof(entries));
            }
        }

        Workspace = workspace;
        Candidate = candidate;
        Entries = entries;
    }

    internal CliWorkspace Workspace { get; }
    internal RecoveryBundleCandidateSnapshot Candidate { get; }
    internal ImmutableArray<RecoveryEntryComparison> Entries { get; }

    internal static void ValidateCandidate(CliWorkspace workspace, RecoveryBundleCandidateSnapshot candidate)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate.Kind != RecoveryBundleCandidateKind.Final || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified
            || !string.Equals(candidate.Path, verified.BundlePath, StringComparison.Ordinal)
            || !string.Equals(WorkspaceIdentity.Key(workspace.PhysicalRoot), verified.WorkspaceKey, StringComparison.Ordinal)
            || !string.Equals(WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot), verified.WorkspacePhysicalPath,
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new ArgumentException("Entry comparisons require an exact verified final for the selected workspace.", nameof(candidate));
        }
    }
}
