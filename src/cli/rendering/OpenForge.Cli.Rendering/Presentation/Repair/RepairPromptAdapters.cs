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
                RepairWording.CandidateDescription(candidate)))
            .Append(new CliChoice<RepairTargetChoice>(
                RepairTargetChoice.Skip,
                RepairWording.Skip()))
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
        RepairLibraryPromptQuestion question,
        CliPromptPolicy policy,
        CancellationToken cancellationToken)
    {
        var choices = new[]
        {
            new CliChoice<RepairLibraryChoice>(RepairLibraryChoice.Select, RepairWording.Select()),
            new CliChoice<RepairLibraryChoice>(RepairLibraryChoice.Skip, RepairWording.Skip()),
        };
        var response = await prompts.SelectAsync(
            new CliSelectQuestion<RepairLibraryChoice>(
                RepairWording.LibraryQuestion(question),
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
            new RepairLibraryPromptAnswer(response.Value == RepairLibraryChoice.Select));
    }

    private sealed record RepairTargetChoice(RepairTargetSelection? Target)
    {
        internal static RepairTargetChoice Skip { get; }
            = new RepairTargetChoice(Target: null);
    }

    private enum RepairLibraryChoice
    {
        Select,
        Skip,
    }
}
