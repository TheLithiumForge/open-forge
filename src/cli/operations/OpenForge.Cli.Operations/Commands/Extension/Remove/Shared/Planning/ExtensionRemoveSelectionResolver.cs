using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemoveSelectionResolver
{
    private readonly CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> _selectionPrompt;
    private readonly string _selectionQuestion;

    internal ExtensionRemoveSelectionResolver(
        CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> selectionPrompt,
        string selectionQuestion)
    {
        ArgumentNullException.ThrowIfNull(selectionPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionQuestion);
        _selectionPrompt = selectionPrompt;
        _selectionQuestion = selectionQuestion;
    }

    internal async ValueTask<ExtensionRemoveSelectionRead> SelectAsync(
        ExtensionRemoveRequest request,
        IReadOnlyList<ExtensionOwnership> extensions,
        CancellationToken cancellationToken)
    {
        if (request.RequestedIds.Count > 0)
        {
            return new ExtensionRemoveSelectionRead(
                new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, request.RequestedIds),
                Boundary: null);
        }

        if (!request.AllowInteraction || request.Automatic)
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.SelectionRequired,
                "Extension Remove requires explicit stable IDs outside a prompt-capable human request.");
        }

        var candidates = extensions.Select(package => package.Id).Order(StringComparer.Ordinal).ToArray();
        if (candidates.Length == 0)
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.SelectionRequired,
                "No managed Extension packages are available for selection.");
        }

        var displayedIds = candidates.ToHashSet(StringComparer.Ordinal);
        var question = new CliMultiSelectQuestion<string>(
            _selectionQuestion,
            [.. extensions.OrderBy(package => package.Id, StringComparer.Ordinal)
                .Select(package => new CliChoice<string>(package.Id, package.Id))],
            [.. extensions.SelectMany(package => package.Dependencies
                .Where(displayedIds.Contains)
                .Select(dependency => new CliDependency<string>(package.Id, dependency)))],
            new HashSet<string>(StringComparer.Ordinal),
            CliDependencyDirection.Dependents);
        var reply = await _selectionPrompt(
            question,
            new CliPromptPolicy(Allowed: true),
            cancellationToken).ConfigureAwait(false);
        if (reply.State == CliPromptState.Unavailable)
        {
            return Boundary(request, ExtensionRemoveFindingCode.SelectionRequired,
                "Extension Remove requires explicit stable IDs outside a prompt-capable human request.");
        }
        if (reply.State == CliPromptState.Cancelled)
        {
            return Boundary(request, ExtensionRemoveFindingCode.Interrupted,
                "Extension remove was cancelled. Nothing was changed.");
        }
        if (reply.State != CliPromptState.Answered || reply.Value.Chosen.Count == 0)
        {
            return Boundary(request, ExtensionRemoveFindingCode.SelectionRequired,
                "Extension Remove requires at least one selected package.");
        }
        var selected = reply.Value.Chosen
            .Concat(reply.Value.Required)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return new ExtensionRemoveSelectionRead(
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.InteractiveIds, selected),
            Boundary: null);
    }

    private static ExtensionRemoveSelectionRead Boundary(
        ExtensionRemoveRequest request,
        ExtensionRemoveFindingCode code,
        string cause)
        => new(Selection: null, ExtensionRemovePlanner.Stop(request, code, cause));
}
