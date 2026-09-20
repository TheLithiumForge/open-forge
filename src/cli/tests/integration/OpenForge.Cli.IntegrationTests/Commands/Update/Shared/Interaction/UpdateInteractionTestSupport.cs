using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Update.Shared.Prompts;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;

internal static class UpdateInteractionTestSupport
{
    internal static CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> ScriptedConfirmation(
        string input,
        bool canPrompt)
    {
        ArgumentNullException.ThrowIfNull(input);
        var lines = new List<string?>();
        using var reader = new StringReader(input);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lines.Add(line);
        }

        if (lines.Count == 0)
        {
            lines.Add(null);
        }

        var scripted = ScriptedCliTerminal.Lines(lines, canPrompt);
        var prompts = new CliPrompts(scripted.Terminal);
        return prompts.PlanConfirmation<UpdateResult, UpdateData, UpdateConfirmationFacts>(
            UpdatePresentation.Rendering,
            static facts => UpdatePlanConfirmationQuestion.Create(facts));
    }

    internal static CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> Confirmation(
        bool accepted = true,
        Action<UpdateResult, UpdateConfirmationFacts>? observe = null)
        => (preview, facts, policy, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!policy.Allowed)
            {
                return ValueTask.FromResult(CliPromptReply<bool>.Unavailable());
            }

            observe?.Invoke(preview, facts);
            return ValueTask.FromResult(
                accepted
                    ? CliPromptReply<bool>.Answered(true)
                    : CliPromptReply<bool>.Cancelled());
        };

    internal static CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> Unavailable()
        => static (_, _, _, _) =>
            ValueTask.FromResult(CliPromptReply<bool>.Unavailable());
}
