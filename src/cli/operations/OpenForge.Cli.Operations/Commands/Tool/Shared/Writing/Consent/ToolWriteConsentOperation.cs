using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;

internal sealed class ToolWriteConsentOperation(CliPrompt<ToolWriteConsentQuestion, bool> confirm)
{
    internal async ValueTask<ToolWriteConsentDecision> AskAsync(
        ToolWriteConsentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var optional = request.Question.Kind switch
        {
            ToolWriteConsentKind.Plan => false,
            ToolWriteConsentKind.AccountUninstall or ToolWriteConsentKind.SharedFileDeletion or ToolWriteConsentKind.PathChange => true,
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Question.Kind, "The writer consent kind is not defined."),
        };
        if (request.Preview)
        {
            return ToolWriteConsentDecision.Preview;
        }
        if (request.Consent.Yes)
        {
            return optional ? ToolWriteConsentDecision.Skipped : ToolWriteConsentDecision.Accepted;
        }
        if (!request.Consent.CanPrompt || request.Format == CliFormat.Json)
        {
            return optional ? ToolWriteConsentDecision.Skipped : ToolWriteConsentDecision.MissingConsent;
        }

        var reply = await confirm(request.Question, new CliPromptPolicy(true), cancellationToken).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered when reply.Value => ToolWriteConsentDecision.Accepted,
            CliPromptState.Answered => optional ? ToolWriteConsentDecision.Skipped : ToolWriteConsentDecision.Declined,
            CliPromptState.Cancelled => ToolWriteConsentDecision.Cancelled,
            CliPromptState.Unavailable => optional ? ToolWriteConsentDecision.Skipped : ToolWriteConsentDecision.MissingConsent,
            _ => throw new ArgumentOutOfRangeException(nameof(reply), reply.State, "The prompt state is not defined."),
        };
    }
}
