using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

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
        RouteListSource? source,
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

    internal RouteListSource? Source { get; }

    internal RouteListSelectionIssue? Issue { get; }

    internal static LoaderDestinationEntryResolution Resolved(RouteListSource source)
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

internal sealed class LoaderDestinationEntryResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal LoaderDestinationEntryResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal LoaderDestinationEntryResolution Resolve(
        CliWorkspace workspace,
        RouteListSourceCatalogue catalogue,
        LoaderDestinationParseResult destination,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.State != LoaderDestinationParseState.Valid)
        {
            throw new ArgumentException("A Loader destination entry resolver requires a valid parsed destination.", nameof(destination));
        }

        var physical = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            CombineWorkspacePath(workspace.LexicalRoot, destination.CanonicalPath!));
        switch (physical.State)
        {
            case PhysicalPathState.Missing:
                return Incomplete(
                    destination.AttemptedDestination,
                    RouteListFindingCode.LoaderUnavailable,
                    "The declared Loader destination is missing.");
            case PhysicalPathState.Contained:
                break;
            default:
                return Blocked(
                    destination.AttemptedDestination,
                    RouteListFindingCode.PhysicalBoundary,
                    "The declared Loader destination crosses an unproved physical boundary.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(destination.AttemptedDestination);
        }

        if (destination.CanonicalPath!.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration cannot target an overwrite companion.");
        }

        var source = catalogue.FindByPath(destination.CanonicalPath);
        if (source is null)
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderUnavailable,
                "The declared Loader destination is not a recognized source.");
        }

        if (string.Equals(destination.CanonicalPath, source.OverwritePath, StringComparison.Ordinal))
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration cannot target an overwrite companion.");
        }

        if (source.Kind != RouteListSourceKind.Entrypoint)
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration must identify an entrypoint.");
        }

        if (source.IsRouteAmbiguous)
        {
            return Blocked(
                destination.AttemptedDestination,
                RouteListFindingCode.RouteAmbiguous,
                "The declared Loader route has ambiguous entrypoint meaning.");
        }

        if (!PhysicalIdentityTracker.PathComparer.Equals(
                physical.ResolvedPhysicalPath!,
                source.PhysicalPath))
        {
            return Blocked(
                destination.AttemptedDestination,
                RouteListFindingCode.PhysicalBoundary,
                "The declared Loader destination does not match its catalogue physical identity.");
        }

        return LoaderDestinationEntryResolution.Resolved(source);
    }

    private static LoaderDestinationEntryResolution Incomplete(
        string subject,
        RouteListFindingCode code,
        string cause)
    {
        return LoaderDestinationEntryResolution.Incomplete(
            new RouteListSelectionIssue(code, subject, cause));
    }

    private static LoaderDestinationEntryResolution Blocked(
        string subject,
        RouteListFindingCode code,
        string cause)
    {
        return LoaderDestinationEntryResolution.Blocked(
            new RouteListSelectionIssue(code, subject, cause));
    }

    private static LoaderDestinationEntryResolution Interrupted(string subject)
    {
        return LoaderDestinationEntryResolution.Interrupted(
            new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Loader root resolution was interrupted."));
    }

    private static string CombineWorkspacePath(string workspaceRoot, string logicalPath)
    {
        var relative = logicalPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(workspaceRoot, relative);
    }
}
