using OpenForge.Cli.Core.Commands.Tool.Models.Interaction;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal static class ToolWriterComponents
{
    internal static async ValueTask<ToolWriterComponentResolution> ResolveAsync(
        ToolWriterComponentRequest request, ToolWriterInteraction interaction, CancellationToken token)
    {
        var overrides = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var supplied in request.Supplied)
        {
            var candidates = request.Declarations.Where(item => item.Id == supplied.Id || item.Flag == supplied.Id).ToArray();
            if (candidates.Length == 0 && supplied.Id == ToolComponentFlags.PromptHook) continue;
            if (candidates.Length != 1 || !overrides.TryAdd(candidates[0].Id, supplied.Enabled))
                throw ToolWriterReader.Failure(ToolWriteIssueCode.InvalidInput, CliSemanticStatus.Invalid, supplied.Id, "A component choice must identify one declared component exactly once.");
        }
        var choices = new Dictionary<string, bool>(StringComparer.Ordinal);
        var issues = new List<ToolWriteIssue>();
        foreach (var component in request.Declarations.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            if (overrides.TryGetValue(component.Id, out var supplied)) { choices.Add(component.Id, supplied); continue; }
            if (request.Saved is not null && request.Saved.TryGetValue(component.Id, out var saved)) { choices.Add(component.Id, saved); continue; }
            if (request.Options.AllowInteraction && !request.Options.Automatic && !request.Options.DryRun)
            {
                var answer = await interaction.Component(new(request.Disclosure, component.Id, component.Description, component.DefaultEnabled),
                    new CliPromptPolicy(true), token).ConfigureAwait(false);
                if (answer.State == CliPromptState.Cancelled)
                {
                    issues.Add(new(ToolWriteIssueCode.Interrupted, CliSemanticStatus.Interrupted, component.Id, "The component choice was cancelled."));
                    break;
                }
                if (answer.State == CliPromptState.Answered) { choices.Add(component.Id, answer.Value); continue; }
            }
            issues.Add(new(ToolWriteIssueCode.ComponentChoiceMissing, CliSemanticStatus.Blocked, component.Id,
                $"Choose the optional component with --{component.Flag} yes or --{component.Flag} no."));
        }
        return new(choices, [.. issues]);
    }
}
