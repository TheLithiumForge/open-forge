using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Observation;

internal enum WorkspaceLayoutReadState
{
    Ready,
    Uninstalled,
    Mismatch,
    Invalid,
    Unavailable,
}

internal sealed record WorkspaceLayoutRead
{
    internal WorkspaceLayoutRead(
        WorkspaceLayoutReadState state,
        WorkspaceSettingsRead settings,
        WorkspaceOwnershipRead ownership,
        WorkspaceLayout? layout,
        ImmutableArray<WorkspaceLayoutIssue> issues,
        ImmutableArray<FileExpectation> expectations)
        : this(state, settings, ownership, layout, issues, expectations, [])
    {
    }

    internal WorkspaceLayoutRead(
        WorkspaceLayoutReadState state,
        WorkspaceSettingsRead settings,
        WorkspaceOwnershipRead ownership,
        WorkspaceLayout? layout,
        ImmutableArray<WorkspaceLayoutIssue> issues,
        ImmutableArray<FileExpectation> expectations,
        ImmutableArray<WorkspaceOutsideRouteRead> outsideRoutes)
    {
        if (!Enum.IsDefined(state)) throw new ArgumentOutOfRangeException(nameof(state));
        if (settings.State is WorkspaceSettingsReadState.Invalid or WorkspaceSettingsReadState.Unavailable && layout is not null)
            throw new ArgumentException("Invalid or unavailable settings cannot supply a layout.", nameof(layout));
        if (state is WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled or WorkspaceLayoutReadState.Mismatch && layout is null)
            throw new ArgumentException("This observation state requires a selected layout.", nameof(layout));
        State = state;
        Settings = settings;
        Ownership = ownership;
        Layout = layout;
        Issues = issues;
        Expectations = expectations;
        OutsideRoutes = outsideRoutes;
    }

    internal WorkspaceLayoutReadState State { get; }
    internal WorkspaceSettingsRead Settings { get; }
    internal WorkspaceOwnershipRead Ownership { get; }
    internal WorkspaceLayout? Layout { get; }
    internal ImmutableArray<WorkspaceLayoutIssue> Issues { get; }
    internal ImmutableArray<FileExpectation> Expectations { get; }
    internal ImmutableArray<WorkspaceOutsideRouteRead> OutsideRoutes { get; }
}
