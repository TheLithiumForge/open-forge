using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Repair;

internal static class RepairPromptAdapters
{
    internal static RepairInteraction Create(
        CliPrompts prompts,
        CliReportRendering<RepairResult, RepairData> rendering)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        ArgumentNullException.ThrowIfNull(rendering);
        return new RepairInteraction(
            (question, policy, cancellationToken) => SelectReferenceAsync(
                prompts, question, policy, cancellationToken),
            (question, policy, cancellationToken) => SelectLibraryAsync(
                prompts, question, policy, cancellationToken),
            prompts.PlanConfirmation<RepairResult, RepairData, RepairConfirmationQuestion>(
                rendering,
                static question => new CliConfirmQuestion(RepairWording.Confirmation(question))));
    }

    private static async ValueTask<CliPromptReply<RepairReferencePromptAnswer>> SelectReferenceAsync(
        CliPrompts prompts,
        RepairReferencePromptQuestion question,
        CliPromptPolicy policy,
        CancellationToken cancellationToken)
    {
        var proposal = question.Proposal;
        var candidates = proposal.Candidates?.Items
            ?? throw new InvalidOperationException("A guided Repair prompt requires candidates.");
        var choices = candidates
            .Select(candidate => new CliChoice<RepairTargetChoice>(
                new RepairTargetChoice(candidate.Target),
                RepairWording.CandidateLabel(candidate),
                RepairWording.CandidateDescription(proposal, candidate)))
            .Append(new CliChoice<RepairTargetChoice>(
                RepairTargetChoice.Skip,
                RepairWording.LeaveLinkUnchanged()))
            .ToArray();
        var response = await prompts.SelectAsync(
            new CliSelectQuestion<RepairTargetChoice>(
                RepairWording.ReferenceQuestion(proposal),
                choices),
            policy,
            cancellationToken).ConfigureAwait(false);
        if (response.State != CliPromptState.Answered)
        {
            return response.State == CliPromptState.Unavailable
                ? CliPromptReply<RepairReferencePromptAnswer>.Unavailable()
                : CliPromptReply<RepairReferencePromptAnswer>.Cancelled();
        }

        if (response.Value.Target is not { } target)
        {
            return CliPromptReply<RepairReferencePromptAnswer>.Answered(
                RepairReferencePromptAnswer.Skip());
        }

        return CliPromptReply<RepairReferencePromptAnswer>.Answered(
            RepairReferencePromptAnswer.Selected(new RepairRelinkRequest(
                new RepairSourceLocation(
                    proposal.SourceCanonicalPath,
                    proposal.OccurrenceView.Line,
                    proposal.OccurrenceView.Column),
                proposal.ExpectedDestination,
                target)));
    }

    private static async ValueTask<CliPromptReply<RepairLibraryPromptAnswer>> SelectLibraryAsync(
        CliPrompts prompts,
        RepairLibrarySelectionQuestion question,
        CliPromptPolicy policy,
        CancellationToken cancellationToken)
    {
        var choices = question.Steps.Select((step, index) => new CliChoice<RepairLibraryPromptAnswer>(
                new(index),
                step.Proposal.TargetPath,
                RepairWording.LibraryRecoveryDescription(step)))
            .Append(new CliChoice<RepairLibraryPromptAnswer>(new(null), RepairWording.LeaveAllRecoveryOut(), RepairWording.LeaveRecoveryOutDescription()))
            .ToArray();
        var response = await prompts.SelectAsync(
            new CliSelectQuestion<RepairLibraryPromptAnswer>(
                RepairWording.LibrarySelectionQuestion(),
                choices),
            policy,
            cancellationToken).ConfigureAwait(false);
        if (response.State != CliPromptState.Answered)
        {
            return response.State == CliPromptState.Unavailable
                ? CliPromptReply<RepairLibraryPromptAnswer>.Unavailable()
                : CliPromptReply<RepairLibraryPromptAnswer>.Cancelled();
        }

        return CliPromptReply<RepairLibraryPromptAnswer>.Answered(
            response.Value);
    }

    private sealed record RepairTargetChoice(RepairTargetSelection? Target)
    {
        internal static RepairTargetChoice Skip { get; }
            = new RepairTargetChoice(Target: null);
    }

}
