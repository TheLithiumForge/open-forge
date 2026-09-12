using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal static class ReferencesBindingResultBuilder
{
    internal static ReferencesResult CreateInvalidResult(
        ReferencesBindingInput input,
        CliWorkspace? workspace,
        IReadOnlyList<ReferencesFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(findings);
        var direction = input.Direction;
        var status = CliSemanticStatus.Invalid;
        var selection = direction is ReferencesDirection.In or ReferencesDirection.Both
            ? CreateSelection(input)
            : null;
        var incoming = direction is ReferencesDirection.In or ReferencesDirection.Both
            ? new ReferencesSection(ReferencesCoverage.Blocked, status, [])
            : null;
        var outgoing = direction is ReferencesDirection.Out or ReferencesDirection.Both
            ? new ReferencesSection(ReferencesCoverage.Blocked, status, [])
            : null;
        return new ReferencesResult(
            workspace,
            null,
            direction,
            selection,
            incoming,
            outgoing,
            findings,
            status,
            ReferencesDefinitions.ReadNextAction(status, findings));
    }

    internal static ReferencesResult CreateBlockedResult(
        ReferencesBindingInput input,
        ReferencesFinding finding)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(finding);
        var direction = input.Direction ?? ReferencesDirection.Both;
        var selection = direction is ReferencesDirection.In or ReferencesDirection.Both
            ? CreateSelection(input)
            : null;
        var incoming = direction is ReferencesDirection.In or ReferencesDirection.Both
            ? new ReferencesSection(ReferencesCoverage.Blocked, CliSemanticStatus.Blocked, [])
            : null;
        var outgoing = direction is ReferencesDirection.Out or ReferencesDirection.Both
            ? new ReferencesSection(ReferencesCoverage.Blocked, CliSemanticStatus.Blocked, [])
            : null;
        return new ReferencesResult(
            null,
            null,
            input.Direction,
            selection,
            incoming,
            outgoing,
            [finding],
            CliSemanticStatus.Blocked,
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Blocked, [finding]));
    }

    private static ReferencesIncomingSelection CreateSelection(ReferencesBindingInput input)
    {
        var roleCounts = new Dictionary<SourceUniverseSelectorRole, int>();
        var supplied = input.SelectorOccurrences
            .Select(value => new ReferencesSelectorOccurrence(value.Role, value.Value))
            .ToArray();
        var resolved = input.SelectorOccurrences
            .Select(value =>
            {
                roleCounts.TryGetValue(value.Role, out var count);
                roleCounts[value.Role] = ++count;
                var parsed = SourceReferenceParser.Parse(value.Value);
                return new ReferencesSelectorResolution(
                    value.Role,
                    count,
                    value.Value,
                    parsed.Kind,
                    SourceReferenceResolutionState.Invalid,
                    null,
                    null,
                    []);
            })
            .ToArray();
        var mode = input.FilterWasSupplied
            ? ReferencesSelectionMode.Filtered
            : ReferencesSelectionMode.Default;
        return new ReferencesIncomingSelection(mode, supplied, resolved, [], []);
    }
}
