using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Interaction;

internal static class RepairLibraryRecoveryWizard
{
    internal static async ValueTask<ImmutableArray<RepairLibraryRecoveryProposal>> SelectAsync(
        CliInteractiveSession session,
        ImmutableArray<RepairLibraryRecoveryProposal> proposals,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.CanPrompt || proposals.IsDefault)
        {
            throw new ArgumentException("Library guided selection requires the existing interactive session and current typed proposals.", nameof(proposals));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var selected = new bool[proposals.Length];
        var index = 0;
        while (index < proposals.Length)
        {
            var proposal = proposals[index];
            var evidence = proposal.Evidence;
            var entry = evidence.Entry.Input.Context.Entry;
            var response = await session.AskAsync(
                $"Library {evidence.LibraryId.Value}: recover {EntryKind(entry.Kind)} at {entry.TargetPath}? select, skip [default], back, cancel: ",
                cancellationToken).ConfigureAwait(false);
            var answer = response.Answer?.Trim();
            if (answer is null || answer.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new RepairLibrarySelectionInterruptedException();
            }

            if (answer.Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                index = Math.Max(0, index - 1);
                continue;
            }

            if (answer.Length == 0 || answer.Equals("skip", StringComparison.OrdinalIgnoreCase))
            {
                selected[index++] = false;
                continue;
            }

            if (answer.Equals("select", StringComparison.OrdinalIgnoreCase))
            {
                selected[index++] = true;
            }
        }

        return [.. proposals.Where((_, ordinal) => selected[ordinal])];
    }

    private static string EntryKind(RecoveryEntryKind kind)
        => kind switch
        {
            RecoveryEntryKind.OrdinaryCreate => "ordinary create",
            RecoveryEntryKind.OrdinaryReplace => "ordinary replace",
            RecoveryEntryKind.OrdinaryReplaceGeneratedRegion => "generated-region replace",
            RecoveryEntryKind.OrdinaryDelete => "ordinary delete",
            RecoveryEntryKind.RelativeFileLinkCreate => "relative-link create",
            RecoveryEntryKind.RelativeFileLinkDelete => "relative-link delete",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The recovery entry kind is not defined."),
        };
}

internal sealed class RepairLibrarySelectionInterruptedException : Exception
{
}
