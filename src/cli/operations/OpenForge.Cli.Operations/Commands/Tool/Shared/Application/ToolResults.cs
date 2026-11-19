using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Application;

internal enum ToolConsent
{
    Accepted,
    Declined,
    Required,
}

internal static class ToolResults
{
    internal static ToolWriteResult Create(
        ToolWriteRequest request, CliSemanticStatus status, IReadOnlyList<ToolWriteTool> tools, IReadOnlyList<ToolFinding>? findings = null)
        => new()
        {
            Kind = request.Kind,
            Status = status,
            Workspace = request.Workspace,
            Data = new(request.Preview, tools),
            Findings = findings ?? [],
        };

    internal static ToolWriteResult Stop(ToolWriteRequest request, ToolFinding finding, IReadOnlyList<ToolWriteTool>? tools = null)
        => Create(request, finding.Status, tools ?? [], [finding]);

    internal static ToolWriteResult Invalid(ToolCommandKind kind, CliWorkspace? workspace, ToolFinding finding)
        => new()
        {
            Kind = kind,
            Status = finding.Status,
            Workspace = workspace,
            Data = new(false, []),
            Findings = [finding],
        };

    /// <summary>Asks for the shown plan, or reports why it can't be asked. --yes accepts it and JSON never prompts.</summary>
    internal static async ValueTask<ToolConsent> AskAsync(
        ToolWriteRequest request, ToolInteraction interaction, ToolWriteResult preview, CancellationToken cancellationToken)
    {
        if (request.Yes) return ToolConsent.Accepted;
        if (request.Format == CliFormat.Json || !interaction.CanPrompt) return ToolConsent.Required;
        var reply = await interaction.ConfirmPlan(preview, new(), new(true), cancellationToken).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered when reply.Value => ToolConsent.Accepted,
            CliPromptState.Unavailable => ToolConsent.Required,
            _ => ToolConsent.Declined,
        };
    }

    internal static ToolWriteResult Unconsented(ToolWriteRequest request, ToolConsent consent, IReadOnlyList<ToolWriteTool> tools)
        => consent == ToolConsent.Required
            ? Stop(request, new(ToolFindingCode.ConfirmationRequired, CliSemanticStatus.Invalid), tools)
            : Stop(request, new(ToolFindingCode.Interrupted, CliSemanticStatus.Interrupted), tools);

    internal static IReadOnlyList<ToolWriteChange> WithOutcomes(IReadOnlyList<ToolWriteChange> changes, int applied, bool failed)
        => [.. changes.Select((change, index) => change with
        {
            Outcome = index < applied ? ToolWire.Done : failed && index == applied ? ToolWire.Failed : ToolWire.NotStarted,
        })];
}
