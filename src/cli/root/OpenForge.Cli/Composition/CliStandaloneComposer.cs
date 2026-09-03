using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Composition;

internal static class CliStandaloneComposer
{
    internal static CliStandaloneComposition Compose(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var findSymbols = FindBinding.CreateSymbols();
        var indexSymbols = IndexBinding.CreateSymbols();
        var installSymbols = InstallBinding.CreateSymbols();
        var referencesSymbols = ReferencesBinding.CreateSymbols();
        var contextSymbols = ContextBinding.CreateSymbols();
        return new CliStandaloneComposition
        {
            FindBinding = BuildFind(findSymbols),
            IndexBinding = BuildIndex(indexSymbols, lockStoreRoot),
            InstallBinding = BuildInstall(installSymbols, interactiveSession, lockStoreRoot),
            ReferencesBinding = BuildReferences(referencesSymbols),
            ContextBinding = BuildContext(contextSymbols),
            RootLeaves =
            [
                new CliRootLeaf(findSymbols.FindCommand, []),
                new CliRootLeaf(indexSymbols.IndexCommand, []),
                new CliRootLeaf(installSymbols.InstallCommand, []),
                new CliRootLeaf(referencesSymbols.ReferencesCommand, []),
                new CliRootLeaf(contextSymbols.ContextCommand, []),
            ],
        };
    }

    private static ICliCommandBinding BuildFind(FindSymbols symbols)
        => FindBinding.Close(
            symbols,
            new FindBindingComponents
            {
                Help = FindHelpSections.Create(),
                Operation = FindOperationFactory.Create(),
                Renderers = new CliRendererSet<FindResult>(
                    FindHumanRenderer.Render,
                    FindJsonRenderer.Render),
                DiagnosticRenderer = FindDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildIndex(
        IndexSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => IndexBinding.Close(
            symbols: symbols,
            help: IndexHelpSections.Create(),
            operation: IndexOperationFactory.Create(lockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<IndexResult>(
                IndexHumanRenderer.Render,
                IndexJsonRenderer.Render),
            diagnosticRenderer: IndexDiagnosticRenderer.Render);

    private static ICliCommandBinding BuildInstall(
        InstallSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => InstallBinding.Close(
            symbols: symbols,
            help: InstallHelpSections.Create(),
            operation: InstallOperationFactory.Create(
                interactiveSession,
                lockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<InstallResult>(
                InstallHumanRenderer.Render,
                InstallJsonRenderer.Render),
            diagnosticRenderer: InstallDiagnosticRenderer.Render);

    private static ICliCommandBinding BuildReferences(ReferencesSymbols symbols)
        => ReferencesBinding.Close(
            symbols,
            new ReferencesBindingComponents
            {
                Help = ReferencesHelpSections.Create(),
                Operation = ReferencesOperationFactory.Create(),
                Renderers = new CliRendererSet<ReferencesResult>(
                    ReferencesHumanRenderer.Render,
                    ReferencesJsonRenderer.Render),
                DiagnosticRenderer = ReferencesDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildContext(ContextSymbols symbols)
        => ContextBinding.Close(
            symbols,
            new ContextBindingComponents
            {
                Help = ContextHelpSections.Create(),
                Operation = ContextOperationFactory.Create(),
                Renderers = new CliRendererSet<ContextResult>(
                    ContextHumanRenderer.Render,
                    ContextJsonRenderer.Render),
                DiagnosticRenderer = ContextDiagnosticRenderer.Render,
            });
}
