using System.Collections.Immutable;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Remove;
using OpenForge.Cli.Core.Presentation.Remove.Models;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

using OpenForge.Cli.Composition.Models.Remove;

namespace OpenForge.Cli.Composition;

internal sealed class CliRootRemoveCommandBinding : ICliCommandBinding
{
    private readonly Argument<string?> _target;
    private readonly Option<string?> _kind;
    private readonly Option<bool> _dryRun;
    private readonly Option<bool> _automatic;
    private readonly Option<string[]> _allowPath;
    private readonly CliReportPipeline<RemovePathRequest, RemoveResult, RemoveData> _pathPipeline;
    private readonly CliReportPipeline<RouteRemoveRequest, RootRouteRemoveResult, RouteRemoveData> _routePipeline;
    private readonly CliReportPipeline<ExtensionRemoveRequest, RootExtensionRemoveResult, ExtensionRemoveData> _extensionPipeline;
    private readonly CliReportPipeline<LibraryDetachRequest, RootLibraryRemoveResult, LibraryDetachData> _libraryPipeline;

    internal CliRootRemoveCommandBinding(
        RemovePathOperation pathOperation,
        RouteRemoveOperation routeOperation,
        ExtensionRemoveOperation extensionOperation,
        LibraryDetachOperation libraryOperation,
        CliHelpContent help)
    {
        ArgumentNullException.ThrowIfNull(pathOperation);
        ArgumentNullException.ThrowIfNull(routeOperation);
        ArgumentNullException.ThrowIfNull(extensionOperation);
        ArgumentNullException.ThrowIfNull(libraryOperation);
        ArgumentNullException.ThrowIfNull(help);

        Command = new Command("remove", "Remove one exact workspace path, route, Extension, or Library.");
        _target = new Argument<string?>("target")
        {
            Description = "One exact path, route reference, Extension ID, or Library ID.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        _kind = new Option<string?>("--kind")
        {
            Description = "Target kind: path, route, extension, or library. Defaults to path.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        _dryRun = new Option<bool>("--dry-run")
        {
            Description = "Preview the selected removal without writing workspace files.",
            Arity = ArgumentArity.Zero,
        };
        _automatic = new Option<bool>("--automatic")
        {
            Description = "Run without confirmation prompts.",
            Arity = ArgumentArity.Zero,
        };
        _allowPath = new Option<string[]>("--allow-path")
        {
            Description = "Authorize an exact additional path where the selected manager requires it.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
            DefaultValueFactory = _ => [],
        };
        Command.Arguments.Add(_target);
        Command.Options.Add(_kind);
        Command.Options.Add(_dryRun);
        Command.Options.Add(_automatic);
        Command.Options.Add(_allowPath);
        Help = help;

        _pathPipeline = new CliReportPipeline<RemovePathRequest, RemoveResult, RemoveData>(
            pathOperation.ExecuteAsync,
            RemovePresentation.Rendering);
        _routePipeline = new CliReportPipeline<RouteRemoveRequest, RootRouteRemoveResult, RouteRemoveData>(
            async (request, token) => new RootRouteRemoveResult(await routeOperation.ExecuteAsync(request, token).ConfigureAwait(false)),
            RemoveRouteRendering());
        _extensionPipeline = new CliReportPipeline<ExtensionRemoveRequest, RootExtensionRemoveResult, ExtensionRemoveData>(
            async (request, token) => new RootExtensionRemoveResult(await extensionOperation.ExecuteAsync(request, token).ConfigureAwait(false)),
            RemoveExtensionRendering());
        _libraryPipeline = new CliReportPipeline<LibraryDetachRequest, RootLibraryRemoveResult, LibraryDetachData>(
            async (request, token) => new RootLibraryRemoveResult(await libraryOperation.ExecuteAsync(request, token).ConfigureAwait(false)),
            RemoveLibraryRendering());
    }

    public Command Command { get; }

    public CliHelpContent Help { get; }

    public CliWorkspaceRequirement WorkspaceRequirement => CliWorkspaceRequirement.Required;

    public async ValueTask<CliProcessCompletion> InvokeAsync(
        CliBindingParse parse,
        CliInvocation invocation,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var target = parse.Result.GetValue(_target);
        var rawKind = parse.Result.GetValue(_kind);
        var dryRun = parse.Result.GetValue(_dryRun);
        var automatic = parse.Result.GetValue(_automatic);
        var allowPaths = parse.Result.GetValue(_allowPath).ToImmutableArray();
        var selection = RemoveSelectionResolver.Resolve(target, rawKind);
        if (selection.State == RemoveSelectionState.Invalid)
        {
            return await PresentResultAsync(
                RemoveResult.Refused(invocation.Workspace, selection.Target, rawKind ?? "path",
                    RemoveFindingCode.InvalidInput, CliSemanticStatus.Invalid,
                    selection.Cause ?? "The Remove input is invalid."),
                invocation.Presentation,
                writers).ConfigureAwait(false);
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The root Remove binding requires a selected workspace.");
        var dispatch = await RemoveTargetResolver.ResolveAsync(workspace, selection, cancellationToken).ConfigureAwait(false);
        if (dispatch.Cause is not null)
        {
            return await PresentResultAsync(RemoveResult.Refused(
                workspace,
                dispatch.Target,
                dispatch.Kind.ToString().ToLowerInvariant(),
                RemoveFindingCode.TargetUnsafe,
                CliSemanticStatus.Blocked,
                dispatch.Cause), invocation.Presentation, writers).ConfigureAwait(false);
        }
        if (dispatch.IsEntrypointFile)
        {
            var directory = Path.GetDirectoryName(dispatch.Target)?.Replace('\\', '/') ?? dispatch.Target;
            return await PresentResultAsync(RemoveResult.Refused(
                workspace,
                dispatch.Target,
                "path",
                RemoveFindingCode.EntryPointRequiresRoute,
                CliSemanticStatus.Blocked,
                "This file is a routed category entrypoint. Select its category directory or use --kind route.",
                $"open-forge remove \"{directory}\" --kind route",
                "Remove the category through its routed entrypoint so companions and navigation are handled together."), invocation.Presentation, writers).ConfigureAwait(false);
        }
        if (allowPaths.Length > 0 && dispatch.Kind is not (RemoveKind.Extension or RemoveKind.Library))
        {
            return await PresentResultAsync(RemoveResult.Refused(
                workspace,
                dispatch.Target,
                dispatch.Kind.ToString().ToLowerInvariant(),
                RemoveFindingCode.InvalidInput,
                CliSemanticStatus.Invalid,
                "--allow-path is available only when removing an Extension or Library."), invocation.Presentation, writers).ConfigureAwait(false);
        }

        var mode = dryRun ? RemoveMode.DryRun : RemoveMode.Apply;
        switch (dispatch.Kind)
        {
            case RemoveKind.Path:
                return await _pathPipeline.ExecuteAsync(
                    new RemovePathRequest(workspace, dispatch.Target, mode, automatic,
                        allowInteractiveConfirmation: invocation.Presentation.Format == CliFormat.Text && !automatic),
                    invocation.Presentation,
                    writers,
                    cancellationToken).ConfigureAwait(false);
            case RemoveKind.Route:
                return await _routePipeline.ExecuteAsync(
                    new RouteRemoveRequest(
                        workspace,
                        dispatch.Target,
                        dryRun ? RouteRemoveMode.DryRun : RouteRemoveMode.Apply,
                        automatic,
                        allowInteractiveSourceSelection: false,
                        allowInteractiveConfirmation: invocation.Presentation.Format == CliFormat.Text && !automatic),
                    invocation.Presentation,
                    writers,
                    cancellationToken).ConfigureAwait(false);
            case RemoveKind.Extension:
                return await _extensionPipeline.ExecuteAsync(
                    new ExtensionRemoveRequest(
                        workspace,
                        dryRun ? ExtensionRemoveMode.DryRun : ExtensionRemoveMode.Apply,
                        [dispatch.Target],
                        automatic,
                        allowInteraction: invocation.Presentation.Format == CliFormat.Text && !automatic,
                        allowPath: allowPaths),
                    invocation.Presentation,
                    writers,
                    cancellationToken).ConfigureAwait(false);
            case RemoveKind.Library:
                return await _libraryPipeline.ExecuteAsync(
                    new LibraryDetachRequest
                    {
                        Workspace = workspace,
                        LibraryId = LibraryId.Create(dispatch.Target),
                        AllowPrompt = invocation.Presentation.Format == CliFormat.Text && !automatic,
                        Automatic = automatic,
                        Mode = dryRun ? LibraryMode.DryRun : LibraryMode.Apply,
                        Allow = allowPaths,
                    },
                    invocation.Presentation,
                    writers,
                    cancellationToken).ConfigureAwait(false);
            default:
                throw new ArgumentOutOfRangeException(nameof(dispatch), dispatch.Kind, "The root Remove kind is not defined.");
        }
    }

    public ValueTask<CliProcessCompletion> PresentInvalidAsync(
        CliInvalidBindingInput input,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var reason = input.InvalidInput.Diagnostics.Count == 0
            ? "The Remove workspace could not be resolved."
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return PresentResultAsync(RemoveResult.Refused(
            workspace: null,
            target: "<target>",
            kind: "path",
            code: RemoveFindingCode.TargetUnavailable,
            status: CliSemanticStatus.Blocked,
            message: reason), input.GlobalInput.Presentation, writers);
    }

    private ValueTask<CliProcessCompletion> PresentResultAsync(
        RemoveResult result,
        CliPresentation presentation,
        CliOutputWriters writers)
        => _pathPipeline.PresentAsync(result, presentation, writers);

    private static CliReportRendering<RootRouteRemoveResult, RouteRemoveData> RemoveRouteRendering()
    {
        var source = RouteRemovePresentation.Rendering;
        return new CliReportRendering<RootRouteRemoveResult, RouteRemoveData>
        {
            Selector = (result, selection) => source.Selector(result.Operation, selection) with { Command = "remove" },
            DataTextRenderer = source.DataTextRenderer,
            DataJsonTypeInfo = source.DataJsonTypeInfo,
            Shape = source.Shape,
            SelectText = source.SelectText,
        };
    }

    private static CliReportRendering<RootExtensionRemoveResult, ExtensionRemoveData> RemoveExtensionRendering()
    {
        var source = ExtensionRemovePresentation.Rendering;
        return new CliReportRendering<RootExtensionRemoveResult, ExtensionRemoveData>
        {
            Selector = (result, selection) => source.Selector(result.Operation, selection) with { Command = "remove" },
            DataTextRenderer = source.DataTextRenderer,
            DataJsonTypeInfo = source.DataJsonTypeInfo,
            Shape = source.Shape,
            SelectText = source.SelectText,
        };
    }

    private static CliReportRendering<RootLibraryRemoveResult, LibraryDetachData> RemoveLibraryRendering()
    {
        var source = LibraryDetachPresentation.Rendering;
        return new CliReportRendering<RootLibraryRemoveResult, LibraryDetachData>
        {
            Selector = (result, selection) => source.Selector(result.Operation, selection) with { Command = "remove" },
            DataTextRenderer = source.DataTextRenderer,
            DataJsonTypeInfo = source.DataJsonTypeInfo,
            Shape = source.Shape,
            SelectText = source.SelectText,
        };
    }

}
