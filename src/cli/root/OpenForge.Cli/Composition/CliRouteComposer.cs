using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliRouteComposer
{
    internal static CliRouteComposition Compose(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var group = RouteBinding.CreateGroup();
        var listSymbols = RouteListBinding.CreateSymbols(group);
        var inspectSymbols = RouteInspectBinding.CreateSymbols(group);
        var initSymbols = RouteInitBinding.CreateSymbols(group);
        var createSymbols = RouteCreateBinding.CreateSymbols(group);
        var updateSymbols = RouteUpdateBinding.CreateSymbols(group);
        var moveComposer = new RouteMoveBinding(
            new RouteMoveBindingValidator(),
            new RouteMoveInvalidResultFactory());
        var moveSymbols = moveComposer.CreateSymbols(group);
        var removeComposer = new RouteRemoveBinding(
            new RouteRemoveBindingValidator(),
            new RouteRemoveInvalidResultFactory());
        var removeSymbols = removeComposer.CreateSymbols(group);
        return new CliRouteComposition
        {
            Branch = new CliRootBranch(
                group,
                RouteHelpSections.CreateGroup(),
                ReadDelimiterPolicies(listSymbols, initSymbols, createSymbols, updateSymbols)),
            ListBinding = BuildList(listSymbols),
            InspectBinding = BuildInspect(inspectSymbols, interactiveSession),
            InitBinding = BuildInit(initSymbols, lockStoreRoot),
            CreateBinding = BuildCreate(createSymbols, lockStoreRoot),
            UpdateBinding = BuildUpdate(updateSymbols, lockStoreRoot),
            MoveBinding = BuildMove(moveComposer, moveSymbols, lockStoreRoot),
            RemoveBinding = BuildRemove(removeComposer, removeSymbols, lockStoreRoot),
        };
    }

    private static IReadOnlyList<CliDelimiterPolicy> ReadDelimiterPolicies(
        RouteListSymbols list,
        RouteInitSymbols init,
        RouteCreateSymbols create,
        RouteUpdateSymbols update)
        =>
        [
            .. list.DelimiterPolicies
                .Concat(init.DelimiterPolicies)
                .Concat(create.DelimiterPolicies)
                .Concat(update.DelimiterPolicies)
                .Distinct(),
        ];

    private static ICliCommandBinding BuildList(RouteListSymbols symbols)
        => RouteListBinding.Close(
            symbols,
            new RouteListBindingComponents
            {
                Help = RouteListHelpSections.CreateList(),
                Operation = RouteListOperationFactory.Create(),
                Renderers = new CliRendererSet<RouteListResult>(
                    RouteListHumanRenderer.Render,
                    RouteListJsonRenderer.Render),
                DiagnosticRenderer = RouteListDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildInspect(
        RouteInspectSymbols symbols,
        CliInteractiveSession interactiveSession)
        => RouteInspectBinding.Close(
            symbols,
            new RouteInspectBindingComponents
            {
                Help = RouteInspectHelpSections.CreateInspect(),
                Operation = RouteInspectOperationFactory.Create(interactiveSession),
                Renderers = new CliRendererSet<RouteInspectResult>(
                    RouteInspectHumanRenderer.Render,
                    RouteInspectJsonRenderer.Render),
                DiagnosticRenderer = RouteInspectDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildInit(
        RouteInitSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => RouteInitBinding.Close(
            symbols,
            new RouteInitBindingComponents
            {
                Help = RouteInitHelpSections.Create(),
                Operation = RouteInitOperationFactory.Create(lockStoreRoot),
                Renderers = new CliRendererSet<RouteInitResult>(
                    RouteInitHumanRenderer.Render,
                    RouteInitJsonRenderer.Render),
                DiagnosticRenderer = RouteInitDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildCreate(
        RouteCreateSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => RouteCreateBinding.Close(
            symbols,
            new RouteCreateBindingComponents
            {
                Help = RouteCreateHelpSections.Create(),
                Operation = RouteCreateOperationFactory.Create(lockStoreRoot),
                Renderers = new CliRendererSet<RouteCreateResult>(
                    RouteCreateHumanRenderer.Render,
                    RouteCreateJsonRenderer.Render),
                DiagnosticRenderer = RouteCreateDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildUpdate(
        RouteUpdateSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => new RouteUpdateBinding(symbols).Bind(
            new RouteUpdateBindingComponents
            {
                Help = RouteUpdateHelpSections.Create(),
                Operation = RouteUpdateOperationFactory.Create(lockStoreRoot),
                Renderers = new CliRendererSet<RouteUpdateResult>(
                    RouteUpdateHumanRenderer.Render,
                    RouteUpdateJsonRenderer.Render),
                DiagnosticRenderer = RouteUpdateDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildMove(
        RouteMoveBinding composer,
        RouteMoveSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => composer.Close(
            symbols,
            new RouteMoveBindingComponents
            {
                Help = RouteMoveHelpSections.Create(),
                Operation = RouteMoveOperationFactory.Create(lockStoreRoot),
                Renderers = new CliRendererSet<RouteMoveResult>(
                    RouteMoveHumanRenderer.Render,
                    RouteMoveJsonRenderer.Render),
                DiagnosticRenderer = RouteMoveDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildRemove(
        RouteRemoveBinding composer,
        RouteRemoveSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => composer.Close(
            symbols,
            new RouteRemoveBindingComponents
            {
                Help = RouteRemoveHelpSections.Create(),
                Operation = RouteRemoveOperationFactory.Create(lockStoreRoot),
                Renderers = new CliRendererSet<RouteRemoveResult>(
                    RouteRemoveHumanRenderer.Render,
                    RouteRemoveJsonRenderer.Render),
                DiagnosticRenderer = RouteRemoveDiagnosticRenderer.Render,
            });
}
