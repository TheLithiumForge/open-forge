using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect;

internal static class RouteInspectOperationFactory
{
    internal static RouteInspectOperation Create()
    {
        return CreateCore(interactiveSession: null);
    }

    internal static RouteInspectOperation Create(CliInteractiveSession interactiveSession)
    {
        return CreateCore(interactiveSession);
    }

    private static RouteInspectOperation CreateCore(CliInteractiveSession? interactiveSession)
    {
        var resolver = new RouteInspectResolver(
            new RouteInspectInteractiveSourceSelector(interactiveSession));
        var profileBuilder = new RouteInspectProfileBuilder();
        var resultBuilder = new RouteInspectResultBuilder();
        return new RouteInspectOperationCoordinator(resolver, profileBuilder, resultBuilder).ExecuteAsync;
    }
}

internal sealed class RouteInspectOperationCoordinator
{
    private readonly RouteInspectResolver _resolver;
    private readonly RouteInspectProfileBuilder _profileBuilder;
    private readonly RouteInspectResultBuilder _resultBuilder;

    internal RouteInspectOperationCoordinator(
        RouteInspectResolver resolver,
        RouteInspectProfileBuilder profileBuilder,
        RouteInspectResultBuilder resultBuilder)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(profileBuilder);
        ArgumentNullException.ThrowIfNull(resultBuilder);
        _resolver = resolver;
        _profileBuilder = profileBuilder;
        _resultBuilder = resultBuilder;
    }

    internal async ValueTask<RouteInspectResult> ExecuteAsync(
        RouteInspectRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        RouteInspectResolution resolution;
        try
        {
            resolution = await _resolver
                .ResolveAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            resolution = CreateInterrupted(request, null);
        }
        catch (Exception)
        {
            resolution = CreateFailed(request, null);
        }

        RouteInspectProfile? profile = null;
        if (resolution.State is RouteInspectResolutionState.Resolved
            or RouteInspectResolutionState.Incomplete)
        {
            try
            {
                profile = _profileBuilder.Build(resolution, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                resolution = CreateInterrupted(request, resolution);
            }
            catch (Exception)
            {
                resolution = CreateFailed(request, resolution);
            }
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _resultBuilder.Build(request, resolution, profile);
        }
        catch (OperationCanceledException)
        {
            return CreateFallbackResult(request, resolution, interrupted: true);
        }
        catch (Exception)
        {
            return CreateFallbackResult(request, resolution, interrupted: false);
        }
    }

    private static RouteInspectResolution CreateInterrupted(
        RouteInspectRequest request,
        RouteInspectResolution? prior)
    {
        return CreateEventResolution(
            request,
            prior,
            RouteInspectResolutionState.Interrupted);
    }

    private static RouteInspectResolution CreateFailed(
        RouteInspectRequest request,
        RouteInspectResolution? prior)
    {
        return CreateEventResolution(
            request,
            prior,
            RouteInspectResolutionState.Failed);
    }

    private static RouteInspectResolution CreateEventResolution(
        RouteInspectRequest request,
        RouteInspectResolution? prior,
        RouteInspectResolutionState state)
    {
        var (code, message) = state switch
        {
            RouteInspectResolutionState.Interrupted => (
                RouteInspectResolutionIssueCode.Interrupted,
                "Source resolution or profile formation was interrupted."),
            RouteInspectResolutionState.Failed => (
                RouteInspectResolutionIssueCode.OperationFailure,
                "Route inspection failed while forming its result."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The operation event resolution state is not defined."),
        };
        var selection = prior?.Selection ?? ReadUnresolvedSelection(request.SourceReference);
        var subject = prior?.Identity?.CanonicalWorkspaceRelativePath
            ?? selection.RequestedReference
            ?? request.SourceReference;
        return RouteInspectResolution.Create(
            state,
            selection,
            null,
            null,
            [new RouteInspectResolutionIssue(code, subject, message)]);
    }

    private static RouteInspectSelection ReadUnresolvedSelection(string sourceReference)
    {
        return RouteInspectResolutionSupport.UnresolvedSelection(
            SourceReferenceParser.Parse(sourceReference));
    }

    private static RouteInspectResult CreateFallbackResult(
        RouteInspectRequest request,
        RouteInspectResolution resolution,
        bool interrupted)
    {
        var status = interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed;
        var code = interrupted
            ? RouteInspectConditionCode.Interrupted
            : RouteInspectConditionCode.OperationFailed;
        var message = interrupted
            ? "Route inspection was interrupted while forming its result."
            : "Route inspection failed while forming its result.";
        var next = interrupted
            ? new CliNextAction("open-forge route inspect", "Rerun the same route-inspect request.")
            : new CliNextAction("open-forge route inspect", "Address the reported failure, then retry route inspect.");
        return RouteInspectResult.Create(
            status,
            request.Workspace,
            resolution.Selection,
            null,
            null,
            [],
            [new RouteInspectCondition(code, status, request.SourceReference, message)],
            next);
    }
}
