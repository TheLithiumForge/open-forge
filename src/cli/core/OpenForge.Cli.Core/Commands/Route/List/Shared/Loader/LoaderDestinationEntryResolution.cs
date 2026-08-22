using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal enum LoaderDestinationEntryResolutionState
{
    Resolved,
    Incomplete,
    Blocked,
    Interrupted,
}

internal sealed class LoaderDestinationEntryResolution
{
    private LoaderDestinationEntryResolution(
        LoaderDestinationEntryResolutionState state,
        RouteSource? source,
        RouteListSelectionIssue? issue)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Loader destination entry state is not defined.");
        }

        if (state == LoaderDestinationEntryResolutionState.Resolved)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (issue is not null)
            {
                throw new ArgumentException("A resolved Loader destination entry cannot contain an issue.", nameof(issue));
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(issue);
            if (source is not null)
            {
                throw new ArgumentException("An unresolved Loader destination entry cannot retain a source.", nameof(source));
            }
        }

        State = state;
        Source = source;
        Issue = issue;
    }

    internal LoaderDestinationEntryResolutionState State { get; }

    internal RouteSource? Source { get; }

    internal RouteListSelectionIssue? Issue { get; }

    internal static LoaderDestinationEntryResolution Resolved(RouteSource source)
    {
        return new LoaderDestinationEntryResolution(
            LoaderDestinationEntryResolutionState.Resolved,
            source,
            null);
    }

    internal static LoaderDestinationEntryResolution Incomplete(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationEntryResolution(
            LoaderDestinationEntryResolutionState.Incomplete,
            null,
            issue);
    }

    internal static LoaderDestinationEntryResolution Blocked(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationEntryResolution(
            LoaderDestinationEntryResolutionState.Blocked,
            null,
            issue);
    }

    internal static LoaderDestinationEntryResolution Interrupted(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationEntryResolution(
            LoaderDestinationEntryResolutionState.Interrupted,
            null,
            issue);
    }
}
