using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Request;

internal enum RepairMode
{
    Apply,
    DryRun,
}

internal enum RepairSelectionMode
{
    InteractiveWizard,
    Automatic,
    ExplicitRelinks,
    AutomaticAndExplicit,
    NonInteractiveBlocked,
}

internal sealed record RepairRequest
{
    internal RepairRequest(
        CliWorkspace workspace,
        RepairMode mode,
        bool automatic,
        IEnumerable<RepairRelinkRequest> relinks,
        bool allowInteraction)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(relinks);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Repair mode is not defined.");
        }

        var values = relinks
            .Select(relink => relink ?? throw new ArgumentException(
                "Repair relink requests cannot contain null members.",
                nameof(relinks)))
            .ToArray();
        var distinct = RepairRelinkNormalizer.Distinct(values);
        if (RepairRelinkNormalizer.HasContradictions(distinct))
        {
            throw new ArgumentException(
                "Repair relink requests cannot contradict one source occurrence.",
                nameof(relinks));
        }

        Workspace = workspace;
        Mode = mode;
        Automatic = automatic;
        Relinks = new ReadOnlyCollection<RepairRelinkRequest>(distinct);
        AllowInteraction = allowInteraction;
        SelectionMode = ReadSelectionMode(automatic, Relinks.Count != 0, allowInteraction);
    }

    internal CliWorkspace Workspace { get; }

    internal RepairMode Mode { get; }

    internal bool Automatic { get; }

    internal IReadOnlyList<RepairRelinkRequest> Relinks { get; }

    internal bool AllowInteraction { get; }

    internal RepairSelectionMode SelectionMode { get; }

    internal bool HasSelectionAuthority
        => Automatic || Relinks.Count != 0 || SelectionMode == RepairSelectionMode.InteractiveWizard;

    private static RepairSelectionMode ReadSelectionMode(
        bool automatic,
        bool hasRelinks,
        bool allowInteraction)
        => (automatic, hasRelinks, allowInteraction) switch
        {
            (true, true, _) => RepairSelectionMode.AutomaticAndExplicit,
            (true, false, _) => RepairSelectionMode.Automatic,
            (false, true, _) => RepairSelectionMode.ExplicitRelinks,
            (false, false, true) => RepairSelectionMode.InteractiveWizard,
            _ => RepairSelectionMode.NonInteractiveBlocked,
        };
}

internal sealed record RepairRelinkRequest
{
    internal RepairRelinkRequest(
        RepairSourceLocation sourceLocation,
        string expectedDestination,
        RepairTargetSelection target)
    {
        ArgumentNullException.ThrowIfNull(sourceLocation);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedDestination);
        ArgumentNullException.ThrowIfNull(target);
        SourceLocation = sourceLocation;
        ExpectedDestination = expectedDestination;
        Target = target;
    }

    internal RepairRelinkRequest(
        string sourceCanonicalPath,
        int line,
        int column,
        string expectedDestination,
        string selectedTargetPath,
        string? selectedTargetFragment)
        : this(
            new RepairSourceLocation(sourceCanonicalPath, line, column),
            expectedDestination,
            new RepairTargetSelection(selectedTargetPath, selectedTargetFragment))
    {
    }

    internal RepairSourceLocation SourceLocation { get; }

    internal string SourceCanonicalPath => SourceLocation.SourceCanonicalPath;

    internal int Line => SourceLocation.Line;

    internal int Column => SourceLocation.Column;

    internal string ExpectedDestination { get; }

    internal RepairTargetSelection Target { get; }

    internal string SelectedTargetPath => Target.CanonicalTargetPath;

    internal string? SelectedTargetFragment => Target.TargetFragment;

    internal static RepairRelinkRequest Parse(
        string sourceLocation,
        string expectedDestination,
        string selectedTarget)
        => new(
            RepairSourceLocation.Parse(sourceLocation),
            expectedDestination,
            RepairTargetSelection.Parse(selectedTarget));
}
