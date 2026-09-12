using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemoveSelectionResolver(CliInteractiveSession interactiveSession)
{
    private readonly CliInteractiveSession _interactiveSession = interactiveSession;

    internal async ValueTask<ExtensionRemoveSelectionRead> SelectAsync(
        ExtensionRemoveRequest request,
        ExtensionLifecycleState lifecycle,
        CancellationToken cancellationToken)
    {
        if (request.RequestedIds.Count > 0)
        {
            return new ExtensionRemoveSelectionRead(
                new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, request.RequestedIds),
                Boundary: null);
        }

        if (!request.AllowInteraction || !_interactiveSession.CanPrompt)
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.SelectionRequired,
                "Extension Remove requires explicit stable IDs outside a prompt-capable human request.");
        }

        var candidates = lifecycle.Packages.Select(package => package.Id).ToArray();
        if (candidates.Length == 0)
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.SelectionRequired,
                "No managed Extension packages are available for selection.");
        }

        var response = await _interactiveSession.AskAsync(
            $"Managed Extensions: {string.Join(", ", candidates)}{Environment.NewLine}Select stable IDs (comma or space separated): ",
            cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response.Answer))
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.InteractionEnded,
                "Extension Remove selection ended without a value.");
        }

        var ids = response.Answer.Split(
            [',', ' ', '\t'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (ids.Length == 0
            || ids.Distinct(StringComparer.Ordinal).Count() != ids.Length
            || ids.Any(id => !candidates.Contains(id, StringComparer.Ordinal)))
        {
            return Boundary(
                request,
                ExtensionRemoveFindingCode.InvalidInput,
                "The interactive Extension selection must contain unique displayed stable IDs.");
        }

        return new ExtensionRemoveSelectionRead(
            new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.InteractiveIds, ids),
            Boundary: null);
    }

    internal async ValueTask<ExtensionRemovePolicyRead> ResolvePolicyAsync(
        ExtensionRemoveRequest request,
        bool hasChangedFinalOwner,
        CancellationToken cancellationToken)
    {
        if (request.Prune)
        {
            return new ExtensionRemovePolicyRead(ExtensionRemoveChangedContentPolicy.Delete, Boundary: null);
        }

        if (!hasChangedFinalOwner || !request.AllowInteraction || !_interactiveSession.CanPrompt)
        {
            return new ExtensionRemovePolicyRead(ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged, Boundary: null);
        }

        var response = await _interactiveSession.AskAsync(
            "Changed final-owner content: [k]eep unmanaged or [d]elete? ",
            cancellationToken).ConfigureAwait(false);
        if (response.Answer is null)
        {
            return new ExtensionRemovePolicyRead(
                ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged,
                ExtensionRemovePlanner.Stop(
                    request,
                    ExtensionRemoveFindingCode.InteractionEnded,
                    "Extension Remove changed-content selection ended without a value."));
        }

        return response.Answer.Trim().ToLowerInvariant() switch
        {
            "" or "k" or "keep" => new ExtensionRemovePolicyRead(
                ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged,
                Boundary: null),
            "d" or "delete" => new ExtensionRemovePolicyRead(
                ExtensionRemoveChangedContentPolicy.Delete,
                Boundary: null),
            _ => new ExtensionRemovePolicyRead(
                ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged,
                ExtensionRemovePlanner.Stop(
                    request,
                    ExtensionRemoveFindingCode.InvalidInput,
                    "Extension Remove changed-content selection must be Keep or Delete.")),
        };
    }

    private static ExtensionRemoveSelectionRead Boundary(
        ExtensionRemoveRequest request,
        ExtensionRemoveFindingCode code,
        string cause)
        => new(Selection: null, ExtensionRemovePlanner.Stop(request, code, cause));
}
