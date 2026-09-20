using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Help;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Presentation.Route.List;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Legacy.Route.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Interaction;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliRouteComposer
{
    internal static CliRouteComposition Compose(
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(interaction);
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
                RouteHelpSections.CreateGroup()),
            ListBinding = BuildList(listSymbols),
            InspectBinding = BuildInspect(inspectSymbols, interaction.Prompts),
            InitBinding = BuildInit(initSymbols, lockStoreRoot),
            CreateBinding = BuildCreate(createSymbols, lockStoreRoot),
            UpdateBinding = BuildUpdate(updateSymbols, interaction.Prompts, lockStoreRoot),
            MoveBinding = BuildMove(moveComposer, moveSymbols, interaction.Prompts, lockStoreRoot),
            RemoveBinding = BuildRemove(removeComposer, removeSymbols, interaction.Prompts, lockStoreRoot),
        };
    }

    private static ICliCommandBinding BuildList(RouteListSymbols symbols)
        => CliReportBinding.Close(RouteListBinding.CreateRequestBinding(
            symbols,
            new RouteListBindingComponents
            {
                Help = RouteListHelpSections.CreateList(),
                Operation = RouteListOperationFactory.Create(),
            }), RouteListPresentation.Rendering);

    private static ICliCommandBinding BuildInspect(
        RouteInspectSymbols symbols,
        CliPrompts prompts)
        => CliReportBinding.Close(RouteInspectBinding.CreateRequestBinding(
            symbols,
            new RouteInspectBindingComponents
            {
                Help = RouteInspectHelpSections.CreateInspect(),
                Operation = RouteInspectOperationFactory.Create(
                    RouteInspectSourceSelectionPrompt.Create(prompts)),
            }), RouteInspectPresentation.Rendering);

    private static ICliCommandBinding BuildInit(
        RouteInitSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(RouteInitBinding.CreateRequestBinding(
            symbols,
            new RouteInitBindingComponents
            {
                Help = RouteInitHelpSections.Create(),
                Operation = RouteInitOperationFactory.Create(lockStoreRoot),
            }), RouteInitPresentation.Rendering);

    private static ICliCommandBinding BuildCreate(
        RouteCreateSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(RouteCreateBinding.CreateRequestBinding(
            symbols,
            new RouteCreateBindingComponents
            {
                Help = RouteCreateHelpSections.Create(),
                Operation = RouteCreateOperationFactory.Create(lockStoreRoot),
            }), RouteCreatePresentation.Rendering);

    private static ICliCommandBinding BuildUpdate(
        RouteUpdateSymbols symbols,
        CliPrompts prompts,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(new RouteUpdateBinding(symbols).Bind(
            new RouteUpdateBindingComponents
            {
                Help = RouteUpdateHelpSections.Create(),
                Operation = RouteUpdateOperationFactory.Create(
                    lockStoreRoot,
                    RouteUpdateSourceSelectionPrompt.Create(prompts)),
            }), RouteUpdatePresentation.Rendering);

    private static ICliCommandBinding BuildMove(
        RouteMoveBinding composer,
        RouteMoveSymbols symbols,
        CliPrompts prompts,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(composer.CreateRequestBinding(
            symbols,
            new RouteMoveBindingComponents
            {
                Help = RouteMoveHelpSections.Create(),
                Operation = RouteMoveOperationFactory.Create(
                    lockStoreRoot,
                    RouteMoveSourceSelectionPrompt.Create(prompts)),
            }), RouteMovePresentation.Rendering);

    private static ICliCommandBinding BuildRemove(
        RouteRemoveBinding composer,
        RouteRemoveSymbols symbols,
        CliPrompts prompts,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(composer.CreateRequestBinding(
            symbols,
            new RouteRemoveBindingComponents
            {
                Help = RouteRemoveHelpSections.Create(),
                Operation = RouteRemoveOperationFactory.Create(
                    lockStoreRoot,
                    RouteRemoveSourceSelectionPrompt.Create(prompts),
                    RouteRemoveConfirmationPrompt.Create(prompts)),
            }), RouteRemovePresentation.Rendering);
}
