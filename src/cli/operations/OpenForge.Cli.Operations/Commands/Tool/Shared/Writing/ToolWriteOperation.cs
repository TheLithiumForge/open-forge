using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal sealed class ToolWriteOperation(
    ToolWritePlanner planner, ToolWriteApplication application, ToolWriteRecordPublisher records,
    ToolWriteHostFacts facts, ToolWriteInteraction interaction)
{
    internal async ValueTask<ToolWriteResult> ExecuteAsync(ToolWriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ToolWritePlan? plan = null;
        ToolWriteExecution? state = null;
        try
        {
            var observed = await records.ReadAsync(new(request.Workspace.LexicalRoot, facts.HomePath), cancellationToken).ConfigureAwait(false);
            state = new(observed);
            var managers = new Dictionary<string, ToolNpmClient>(StringComparer.Ordinal);
            plan = new()
            {
                Input = new(request, observed, facts, managers),
                Tools = [],
                Findings = [],
                ManagerQuestions = [],
                UsedNetwork = false,
            };
            if (!request.AllowPaths.IsDefaultOrEmpty)
            {
                state.Grants = new(request.AllowPaths, ToolWriteStages.Planned, [], []);
                if (!request.Preview)
                {
                    if (!await records.AddGrantsAsync(state,
                        new(new(request.Workspace.LexicalRoot, facts.HomePath), observed, request.AllowPaths), cancellationToken).ConfigureAwait(false))
                        return ToolWriteProjection.Result(plan, state);
                    observed = state.Records;
                }
            }
            var usedNetwork = false;
            while (true)
            {
                plan = await planner.PlanAsync(new(request, observed, facts, managers), cancellationToken).ConfigureAwait(false);
                usedNetwork |= plan.UsedNetwork;
                plan = plan with { UsedNetwork = usedNetwork };
                if (plan.ManagerQuestions.Count == 0) break;
                if (plan.Findings.Count != 0) break;
                if (request.Yes || !request.CanPrompt || !interaction.CanPrompt || request.Format == CliFormat.Json || request.Preview)
                {
                    state.Findings.AddRange(plan.ManagerQuestions.Select(question =>
                        new ToolWriteFinding(ToolWriteFindingCode.ManagerRequired, CliSemanticStatus.Blocked, question.ToolId)));
                    break;
                }
                foreach (var question in plan.ManagerQuestions)
                {
                    var reply = await interaction.ChooseManager(question, new(true), cancellationToken).ConfigureAwait(false);
                    if (reply.State == CliPromptState.Cancelled) throw new OperationCanceledException(cancellationToken);
                    if (reply.State != CliPromptState.Answered || !question.Choices.Any(choice => choice.Value == reply.Value))
                    {
                        state.Findings.Add(new(ToolWriteFindingCode.ManagerRequired, CliSemanticStatus.Blocked, question.ToolId));
                        break;
                    }
                    managers[question.ToolId] = (ToolNpmClient)reply.Value;
                }
                if (state.Findings.Count != 0) break;
            }
            await application.DescribeAsync(plan, state, cancellationToken).ConfigureAwait(false);
            if (plan.Findings.Count != 0 || state.Findings.Count != 0 || request.Preview)
                return ToolWriteProjection.Result(plan, state);
            if (plan.Tools.All(tool => tool.NoOp))
            {
                await application.CompleteAsync(state, cancellationToken).ConfigureAwait(false);
                return ToolWriteProjection.Result(plan, state);
            }
            var preview = ToolWriteProjection.Result(plan, state);
            var planQuestion = new ToolWriteConsentQuestion
            {
                Kind = ToolWriteConsentKind.Plan,
                Effects = [],
                ToolId = request.Spec?.Id,
            };
            var consent = new ToolWriteConsentOperation((selected, policy, token) =>
                interaction.ConfirmPlan(preview with { Data = preview.Data with { Preview = true } }, selected, policy, token));
            var decision = await consent.AskAsync(new()
            {
                Consent = new(request.Yes, request.CanPrompt && interaction.CanPrompt),
                Format = request.Format,
                Preview = false,
                Question = planQuestion,
            }, cancellationToken).ConfigureAwait(false);
            if (decision is ToolWriteConsentDecision.Declined or ToolWriteConsentDecision.Cancelled)
                throw new OperationCanceledException(cancellationToken);
            if (decision != ToolWriteConsentDecision.Accepted)
            {
                state.Findings.Add(new(ToolWriteFindingCode.MissingConsent, CliSemanticStatus.Invalid, request.Spec?.Id));
                return ToolWriteProjection.Result(plan, state);
            }
            var fresh = await records.ReadAsync(new(request.Workspace.LexicalRoot, facts.HomePath), cancellationToken).ConfigureAwait(false);
            if (fresh.ExpectedFileSha256.Count != observed.ExpectedFileSha256.Count
                || observed.ExpectedFileSha256.Any(expected =>
                    !fresh.ExpectedFileSha256.TryGetValue(expected.Key, out var hash) || hash != expected.Value))
            {
                state.Findings.Add(new(ToolWriteFindingCode.ExpectedStateChanged, CliSemanticStatus.Blocked));
                return ToolWriteProjection.Result(plan, state);
            }
            state.Records = fresh;
            await application.ApplyAsync(plan, state, interaction, cancellationToken).ConfigureAwait(false);
            return ToolWriteProjection.Result(plan, state);
        }
        catch (OperationCanceledException)
        {
            return Failure(ToolWriteFindingCode.Interrupted, CliSemanticStatus.Interrupted, null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            return Failure(ToolWriteFindingCode.UnexpectedFailure, CliSemanticStatus.Failed, exception.Message);
        }

        ToolWriteResult Failure(ToolWriteFindingCode code, CliSemanticStatus status, string? cause)
        {
            var finding = new ToolWriteFinding(code, status, request.Spec?.Id, Cause: cause);
            if (plan is null || state is null) return ToolWriteResults.Invalid(request.Kind, finding, request.Workspace, request.Preview);
            state.Findings.Add(finding);
            state.Stopped = true;
            return ToolWriteProjection.Result(plan, state);
        }
    }
}
