using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Interaction;

internal sealed class RepairWizard(CliInteractiveSession session)
{
    private readonly CliInteractiveSession _session = session;

    internal async ValueTask<RepairWizardSelection> SelectAsync(
        IReadOnlyList<RepairProposal> proposals,
        CancellationToken cancellationToken)
    {
        var choices = new RepairRelinkRequest?[proposals.Count];
        var index = 0;
        while (index < proposals.Count)
        {
            var proposal = proposals[index];
            var response = await _session.AskAsync(ProposalPrompt(proposal), cancellationToken).ConfigureAwait(false);
            var answer = response.Answer?.Trim();
            if (answer is null || answer.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                return new RepairWizardSelection(Cancelled: true, Relinks: []);
            }

            if (answer.Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                index = Math.Max(0, index - 1);
                continue;
            }

            if (answer.Equals("skip", StringComparison.OrdinalIgnoreCase) || answer.Length == 0 && proposal.IsGuided)
            {
                choices[index++] = null;
                continue;
            }

            RepairTargetSelection? target = null;
            if (proposal.Resolution is { } exact && (answer.Length == 0 || answer.Equals("select", StringComparison.OrdinalIgnoreCase)))
            {
                target = exact.Target;
            }
            else if (proposal.Candidates is { } candidates
                && int.TryParse(answer, NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal)
                && ordinal > 0 && ordinal <= candidates.Items.Count)
            {
                target = candidates.Items[ordinal - 1].Target;
            }

            if (target is null)
            {
                continue;
            }

            choices[index++] = new RepairRelinkRequest(
                new RepairSourceLocation(proposal.SourceCanonicalPath, proposal.Occurrence.Line, proposal.Occurrence.Column),
                proposal.ExpectedDestination,
                target);
        }

        return new RepairWizardSelection(Cancelled: false, Relinks: [.. choices.OfType<RepairRelinkRequest>()]);
    }

    internal async ValueTask<bool> ConfirmAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        if (plan.Request.Mode == RepairMode.DryRun)
        {
            return false;
        }

        var prompt = new StringBuilder("Review the complete Repair plan.\n");
        foreach (var effect in plan.Effects)
        {
            prompt.AppendLine($"""
                Path: {effect.SourceCanonicalPath}
                Expected SHA-256: {effect.ExpectedState.Expectation.ContentHash}
                Intended SHA-256: {effect.IntendedState.Expectation.ContentHash}
                Verification: exact resulting bytes, addressed destinations, contained targets and required fragments.
                Recovery: one verified external bundle before replacement.
                """);
            foreach (var change in effect.Changes)
            {
                prompt.AppendLine(CultureInfo.InvariantCulture,
                    $"  {change.Occurrence.Line}:{change.Occurrence.Column} {Quote(change.ExpectedDestination)} -> {Quote(change.IntendedDestination)}");
            }
        }

        prompt.Append("Apply this complete plan? Yes / No [default: No], back, cancel: ");
        var response = await _session.AskAsync(prompt.ToString(), cancellationToken).ConfigureAwait(false);
        return string.Equals(response.Answer?.Trim(), "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static string ProposalPrompt(RepairProposal proposal)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine(CultureInfo.InvariantCulture,
            $"{proposal.SourceCanonicalPath}@{proposal.Occurrence.Line}:{proposal.Occurrence.Column}: {Quote(proposal.ExpectedDestination)}");
        if (proposal.Resolution is { } exact)
        {
            prompt.AppendLine($"Safe exact correction: {Quote(exact.IntendedDestination)}");
            prompt.Append("select [default], skip, back, cancel: ");
        }
        else if (proposal.Candidates is { } candidates)
        {
            prompt.AppendLine("No candidate selected by default. Review the bounded evidence:");
            for (var index = 0; index < candidates.Items.Count; index++)
            {
                var candidate = candidates.Items[index];
                var recommendation = candidate.RecommendedForReview ? " (recommended for review)" : string.Empty;
                prompt.AppendLine(CultureInfo.InvariantCulture,
                    $"  {index + 1}: {candidate.Target.CanonicalTargetPath}{recommendation}");
                foreach (var evidence in candidate.Evidence)
                {
                    prompt.AppendLine($"    {RepairDefinitions.ReadMachineName(evidence.Kind)}: {Quote(evidence.Value)}");
                }
            }

            prompt.Append("Choose a candidate number, skip [default], back, cancel: ");
        }

        return prompt.ToString();
    }

    private static string Quote(string value)
    {
        var escaped = value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}
