using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallSelectionResolver
{
    private readonly CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> _selectionPrompt;
    private readonly string _selectionQuestion;
    private readonly ExtensionInstallDependencyClosureResolver _dependencyClosureResolver;

    internal ExtensionInstallSelectionResolver(
        CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> selectionPrompt,
        string selectionQuestion,
        ExtensionInstallDependencyClosureResolver dependencyClosureResolver)
    {
        ArgumentNullException.ThrowIfNull(selectionPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionQuestion);
        _selectionPrompt = selectionPrompt;
        _selectionQuestion = selectionQuestion;
        _dependencyClosureResolver = dependencyClosureResolver;
    }

    internal async ValueTask<ExtensionInstallSelectionResolution> ResolveAsync(
        ExtensionInstallRequest request,
        ExtensionInstallSourceResolution sourceResolution,
        CancellationToken cancellationToken)
    {
        var ownership = await ReadOwnershipAsync(request, cancellationToken).ConfigureAwait(false);
        var selection = await SelectAsync(request, sourceResolution, ownership, cancellationToken)
            .ConfigureAwait(false);
        if (selection.Finding is not null)
        {
            return new ExtensionInstallSelectionResolution(
                selection: null,
                packages: [],
                selection.Finding)
            {
                Ownership = ownership,
            };
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
                closure.Finding)
            {
                Ownership = ownership,
            };
        }

        var normalization = ExtensionInstallPayloadNormalizer.Normalize(closure.Packages);
        if (normalization.Finding is not null)
        {
            return new ExtensionInstallSelectionResolution(
                selected,
                closure.Packages,
                normalization.Finding)
            {
                Ownership = ownership,
            };
        }

        return new ExtensionInstallSelectionResolution(
            selected,
            normalization.Packages,
            finding: null)
        {
            Ownership = ownership,
        };
    }

    private async ValueTask<SelectionOutcome> SelectAsync(
        ExtensionInstallRequest request,
        ExtensionInstallSourceResolution sourceResolution,
        WorkspaceOwnershipRead ownership,
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

        if (!request.AllowInteraction || request.Automatic)
        {
            return SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SelectionRequired,
                "A multi-package source requires exact IDs or --all."));
        }

        var ids = source.Packages
            .Select(package => package.Id)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var displayedIds = ids.ToHashSet(StringComparer.Ordinal);
        // Only a complete ownership read can establish that a source package is
        // already installed. An invalid or unreadable lock deliberately leaves
        // the state unknown, so it must not be presented as an authoritative
        // empty inventory (or used to disable rows).
        var installed = ownership.IsTrustworthy
            ? ownership.Document.Extensions
                .Select(package => package.Id)
                .ToHashSet(StringComparer.Ordinal)
            : null;
        if (installed is not null && ids.All(installed.Contains))
        {
            return SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SelectionRequired,
                "A multi-package source has no selectable package; supply exact IDs or --all."));
        }

        var disabled = source.Packages
            .Select(package => package.Id)
            .Where(id => installed?.Contains(id) == true)
            .ToHashSet(StringComparer.Ordinal);
        var question = new CliMultiSelectQuestion<string>(
            _selectionQuestion,
            [.. source.Packages.OrderBy(package => package.Id, StringComparer.Ordinal).Select(package =>
                new CliChoice<string>(package.Id, package.Id, package.Description))],
            [.. source.Packages.SelectMany(package => package.Dependencies
                .Where(displayedIds.Contains)
                .Select(dependency => new CliDependency<string>(package.Id, dependency)))],
            disabled,
            CliDependencyDirection.Requires);
        var reply = await _selectionPrompt(
            question,
            new CliPromptPolicy(!request.Automatic && request.AllowInteraction),
            cancellationToken).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered => reply.Value.Chosen.Count == 0
                ? SelectionOutcome.Stop(new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.SelectionRequired,
                    "A multi-package source requires at least one selected package."))
                : SelectionOutcome.Complete(new ExtensionInstallSelection(
                    reply.Value.Chosen.Count == ids.Length
                        ? ExtensionInstallSelectionKind.InteractiveAll
                        : ExtensionInstallSelectionKind.InteractiveIds,
                    reply.Value.Chosen)),
            CliPromptState.Unavailable => SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.SelectionRequired,
                "A multi-package source requires exact IDs or --all.")),
            CliPromptState.Cancelled => SelectionOutcome.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension install was cancelled. Nothing was changed.")),
            _ => throw new ArgumentOutOfRangeException(nameof(reply), reply.State,
                "The Extension Install selection state is not defined."),
        };
    }

    private static async ValueTask<WorkspaceOwnershipRead> ReadOwnershipAsync(
        ExtensionInstallRequest request,
        CancellationToken cancellationToken)
        => await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(), request.Workspace, cancellationToken).ConfigureAwait(false);

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
