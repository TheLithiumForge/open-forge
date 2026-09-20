using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Interaction;

internal sealed record RepairInteraction(
    CliPrompt<RepairReferencePromptQuestion, RepairReferencePromptAnswer> SelectReference,
    CliPrompt<RepairLibraryPromptQuestion, RepairLibraryPromptAnswer> SelectLibrary,
    CliPlanConfirmation<RepairResult, RepairConfirmationQuestion> Confirm);

internal sealed record RepairReferencePromptQuestion
{
    internal RepairReferencePromptQuestion(RepairProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        if (!proposal.IsGuided)
        {
            throw new ArgumentException("A Repair reference prompt requires a guided proposal.", nameof(proposal));
        }

        Proposal = proposal;
    }

    internal RepairProposal Proposal { get; }
}

internal sealed record RepairReferencePromptAnswer
{
    private RepairReferencePromptAnswer(RepairRelinkRequest? relink)
    {
        Relink = relink;
    }

    internal RepairRelinkRequest? Relink { get; }

    internal bool Skipped => Relink is null;

    internal static RepairReferencePromptAnswer Selected(RepairRelinkRequest relink)
    {
        ArgumentNullException.ThrowIfNull(relink);
        return new(relink);
    }

    internal static RepairReferencePromptAnswer Skip()
        => new RepairReferencePromptAnswer(relink: null);
}

internal sealed record RepairLibraryPromptQuestion
{
    internal RepairLibraryPromptQuestion(RepairLibraryRecoveryProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        Proposal = proposal;
        EntryKind = ReadEntryKind(proposal.Evidence.Entry.Input.Context.Entry.Kind);
    }

    internal RepairLibraryRecoveryProposal Proposal { get; }

    internal RepairLibraryPromptEntryKind EntryKind { get; }

    private static RepairLibraryPromptEntryKind ReadEntryKind(RecoveryEntryKind kind)
        => kind switch
        {
            RecoveryEntryKind.OrdinaryCreate => RepairLibraryPromptEntryKind.OrdinaryCreate,
            RecoveryEntryKind.OrdinaryReplace => RepairLibraryPromptEntryKind.OrdinaryReplace,
            RecoveryEntryKind.OrdinaryReplaceGeneratedRegion => RepairLibraryPromptEntryKind.OrdinaryReplaceGeneratedRegion,
            RecoveryEntryKind.OrdinaryDelete => RepairLibraryPromptEntryKind.OrdinaryDelete,
            RecoveryEntryKind.RelativeFileLinkCreate => RepairLibraryPromptEntryKind.RelativeFileLinkCreate,
            RecoveryEntryKind.RelativeFileLinkDelete => RepairLibraryPromptEntryKind.RelativeFileLinkDelete,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Recovery entry kind is not defined."),
        };
}

internal enum RepairLibraryPromptEntryKind
{
    OrdinaryCreate,
    OrdinaryReplace,
    OrdinaryReplaceGeneratedRegion,
    OrdinaryDelete,
    RelativeFileLinkCreate,
    RelativeFileLinkDelete,
}

internal sealed record RepairLibraryPromptAnswer(bool Selected);

internal enum RepairConfirmationKind
{
    Safe,
    Final,
}

internal sealed record RepairConfirmationQuestion
{
    internal RepairConfirmationQuestion(RepairConfirmationKind kind, int count)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Repair confirmation kind is not defined.");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The Repair confirmation count cannot be negative.");
        }

        Kind = kind;
        Count = count;
    }

    internal RepairConfirmationKind Kind { get; }

    internal int Count { get; }
}
