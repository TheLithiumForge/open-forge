using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallSelectionResolver(
    CliInteractiveSession interactiveSession,
    ExtensionInstallDependencyClosureResolver dependencyClosureResolver,
    ExtensionInstallPayloadNormalizer payloadNormalizer)
{
    private readonly CliInteractiveSession _interactiveSession = interactiveSession;
    private readonly ExtensionInstallDependencyClosureResolver _dependencyClosureResolver =
        dependencyClosureResolver;
    private readonly ExtensionInstallPayloadNormalizer _payloadNormalizer = payloadNormalizer;

    internal async ValueTask<ExtensionInstallSelectionResolution> ResolveAsync(
        ExtensionInstallRequest request,
        ExtensionInstallSourceResolution sourceResolution,
        CancellationToken cancellationToken)
    {
        var selection = await SelectAsync(request, sourceResolution, cancellationToken)
            .ConfigureAwait(false);
        if (selection.Finding is not null)
        {
            return new ExtensionInstallSelectionResolution(
                selection: null,
                packages: [],
                selection.Finding);
        }

        var selected = selection.Selection
            ?? throw new InvalidOperationException(
                "A successful Extension selection requires its fact.");
        var closure = _dependencyClosureResolver.Resolve(
            sourceResolution.Source.Packages,
            selected.RootIds);
        if (closure.Finding is not null)
        {
            return new ExtensionInstallSelectionResolution(
                selected,
                closure.Packages,
                closure.Finding);
        }

        var normalization = _payloadNormalizer.Normalize(closure.Packages);
        if (normalization.Finding is not null)
        {
            return new ExtensionInstallSelectionResolution(
                selected,
                closure.Packages,
                normalization.Finding);
        }

        return new ExtensionInstallSelectionResolution(
            selected,
            normalization.Packages,
            finding: null);
    }

    private async ValueTask<SelectionOutcome> SelectAsync(
        ExtensionInstallRequest request,
        ExtensionInstallSourceResolution sourceResolution,
        CancellationToken cancellationToken)
    {
        var source = sourceResolution.Source;
        if (request.RequestedIds.Count > 0 && !request.All)
        {
            return SelectionOutcome.Complete(new ExtensionInstallSelection(
                ExtensionInstallSelectionKind.ExplicitIds,
                request.RequestedIds.Order(StringComparer.Ordinal)));
        }

        if (request.All && request.RequestedIds.Count == 0)
        {
            return SelectionOutcome.Complete(new ExtensionInstallSelection(
                ExtensionInstallSelectionKind.ExplicitAll,
                source.Packages.Select(package => package.Id).Order(StringComparer.Ordinal)));
        }

        if (request.All)
        {
            return SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.InvalidInput,
                "Explicit Extension IDs and --all cannot be combined."));
        }

        if (sourceResolution.InferredRootId is { } inferredRoot)
        {
            return SelectionOutcome.Complete(new ExtensionInstallSelection(
                ExtensionInstallSelectionKind.SinglePackageInference,
                [inferredRoot]));
        }

        if (source.Packages.Count == 1)
        {
            return SelectionOutcome.Complete(new ExtensionInstallSelection(
                ExtensionInstallSelectionKind.SinglePackageInference,
                [source.Packages[0].Id]));
        }

        if (!request.AllowInteraction || !_interactiveSession.CanPrompt)
        {
            return SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SelectionRequired,
                "A multi-package source requires exact IDs or --all."));
        }

        var ids = source.Packages
            .Select(package => package.Id)
            .Order(StringComparer.Ordinal)
            .ToArray();
        while (true)
        {
            var prompt = $"Available Extension packages:{Environment.NewLine}{string.Join(Environment.NewLine, ids.Select(id => $"  {id}"))}{Environment.NewLine}Select one exact stable ID or all: ";
            var response = await _interactiveSession.AskAsync(prompt, cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput)
            {
                return SelectionOutcome.Stop(new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.InteractionEnded,
                    "Input ended before an exact Extension selection was supplied."));
            }

            if (string.Equals(response.Answer, "all", StringComparison.Ordinal))
            {
                return SelectionOutcome.Complete(new ExtensionInstallSelection(
                    ExtensionInstallSelectionKind.InteractiveAll,
                    ids));
            }

            if (response.Answer is not null
                && ids.Contains(response.Answer, StringComparer.Ordinal))
            {
                return SelectionOutcome.Complete(new ExtensionInstallSelection(
                    ExtensionInstallSelectionKind.InteractiveIds,
                    [response.Answer]));
            }
        }
    }

    private sealed record SelectionOutcome(
        ExtensionInstallSelection? Selection,
        ExtensionInstallFinding? Finding)
    {
        internal static SelectionOutcome Complete(ExtensionInstallSelection selection)
            => new(selection, null);

        internal static SelectionOutcome Stop(ExtensionInstallFinding finding)
            => new(null, finding);
    }
}
