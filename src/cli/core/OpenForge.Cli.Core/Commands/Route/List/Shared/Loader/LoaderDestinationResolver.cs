using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal sealed class LoaderDestinationResolver
{
    private const string LoaderPath = ".agents/loader.md";

    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly LoaderDestinationBatchResolver _destinationResolver;

    internal LoaderDestinationResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
        _destinationResolver = new LoaderDestinationBatchResolver(
            new LoaderDestinationEntryResolver(physicalPathResolver));
    }

    internal async ValueTask<LoaderDestinationResolution> ResolveAsync(
        CliWorkspace workspace,
        RouteSourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(LoaderPath);
        }

        var physical = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            RouteLogicalPath.ToLexicalPath(workspace.LexicalRoot, LoaderPath));
        switch (physical.State)
        {
            case PhysicalPathState.Missing:
                return Incomplete(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderUnavailable,
                    LoaderPath,
                    "The Loader file is missing."));
            case PhysicalPathState.Contained:
                break;
            default:
                return Blocked(new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    LoaderPath,
                    "The Loader physical boundary could not be proved."));
        }

        var read = await StrictUtf8FileReader.ReadAsync(
                physical.ResolvedPhysicalPath!,
                LoaderPath,
                cancellationToken)
            .ConfigureAwait(false);
        switch (read.State)
        {
            case FileReadState.Cancelled:
                return Interrupted(LoaderPath);
            case FileReadState.Missing:
                return Incomplete(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderUnavailable,
                    LoaderPath,
                    "The Loader file became unavailable before it could be read."));
            case FileReadState.InvalidEncoding:
                return Incomplete(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderMalformed,
                    LoaderPath,
                    "The Loader file is not valid strict UTF-8."));
            case FileReadState.AccessDenied:
            case FileReadState.InputOutputFailure:
                return Incomplete(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderUnavailable,
                    LoaderPath,
                    "The Loader file could not be read."));
            case FileReadState.Complete:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(read.State), read.State, "The Loader read state is not defined.");
        }

        var entries = RouteLoaderEntriesParser.Parse(read.Value!);
        if (entries.State == RouteLoaderEntriesParseState.Malformed)
        {
            var issue = new RouteListSelectionIssue(
                RouteListFindingCode.LoaderMalformed,
                entries.AttemptedDestination ?? LoaderPath,
                entries.Cause!);
            if (entries.Destinations.Count == 0)
            {
                return Incomplete(issue);
            }

            return AddMalformedIssue(
                _destinationResolver.Resolve(
                    workspace,
                    catalogue,
                    entries.Destinations,
                    cancellationToken),
                issue);
        }

        return _destinationResolver.Resolve(
            workspace,
            catalogue,
            entries.Destinations,
            cancellationToken);
    }

    private static LoaderDestinationResolution Incomplete(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Incomplete,
            [],
            [issue]);
    }

    private static LoaderDestinationResolution AddMalformedIssue(
        LoaderDestinationResolution resolution,
        RouteListSelectionIssue issue)
    {
        var state = resolution.State == LoaderDestinationResolutionState.Resolved
            ? LoaderDestinationResolutionState.Incomplete
            : resolution.State;
        return new LoaderDestinationResolution(
            state,
            resolution.SelectedSources,
            resolution.Issues.Append(issue));
    }

    private static LoaderDestinationResolution Blocked(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Blocked,
            [],
            [issue]);
    }

    private static LoaderDestinationResolution Interrupted(string subject)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Interrupted,
            [],
            [new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Loader root resolution was interrupted.")]);
    }
}
