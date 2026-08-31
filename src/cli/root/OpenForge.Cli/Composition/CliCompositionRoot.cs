using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route;
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
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Composition;

internal static class CliCompositionRoot
{
    internal static CliCoreApplication Create(CliProcessIdentity process)
    {
        return Create(
            process,
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
            });
    }

    internal static CliCoreApplication Create(
        CliProcessIdentity process,
        CliCompositionInputs inputs)
    {
        var interactiveSession = new CliInteractiveSession(
            standardInput: inputs.StandardInput,
            promptOutput: inputs.PromptOutput,
            canPrompt: !inputs.StandardInputRedirected && !inputs.PromptOutputRedirected);
        var rootHelp = new CliHelpContent(
        [
            new CliHelpSection(
                "Product",
                $"  Open Forge CLI (`{CliSyntaxDefinitions.ExecutableName}`)."),
            new CliHelpSection(
                "Discovery",
                """
                  route list        List routed sources and descendants at a structural depth.
                  route inspect     Explain one source's route behavior without returning authored content.
                  route init        Initialize every missing entrypoint in one exact route chain.
                  find              Find Markdown sources by authored tags and structural headings.
                  extension list    List installed and available Extension packages.
                  extension inspect Inspect one installed or available Extension package.
                  extension create  Create one local Extension package scaffold.
                """),
            new CliHelpSection(
                heading: "Lifecycle",
                body: "  Framework management is established or verified by install without reconciling managed divergence."),
        ]);
        var routeGroup = RouteBinding.CreateGroup();
        var listSymbols = RouteListBinding.CreateSymbols(routeGroup);
        var inspectSymbols = RouteInspectBinding.CreateSymbols(routeGroup);
        var initSymbols = RouteInitBinding.CreateSymbols(routeGroup);
        IReadOnlyList<CliDelimiterPolicy> routeDelimiterPolicies =
        [
            .. listSymbols.DelimiterPolicies,
            .. initSymbols.DelimiterPolicies,
        ];
        var listBinding = RouteListBinding.Close(
            listSymbols,
            new RouteListBindingComponents
            {
                Help = RouteListHelpSections.CreateList(),
                Operation = RouteListOperationFactory.Create(),
                Renderers = new CliRendererSet<RouteListResult>(
                    RouteListHumanRenderer.Render,
                    RouteListJsonRenderer.Render),
                DiagnosticRenderer = RouteListDiagnosticRenderer.Render,
            });
        var inspectBinding = RouteInspectBinding.Close(
            inspectSymbols,
            new RouteInspectBindingComponents
            {
                Help = RouteInspectHelpSections.CreateInspect(),
                Operation = RouteInspectOperationFactory.Create(interactiveSession),
                Renderers = new CliRendererSet<RouteInspectResult>(
                    RouteInspectHumanRenderer.Render,
                    RouteInspectJsonRenderer.Render),
                DiagnosticRenderer = RouteInspectDiagnosticRenderer.Render,
            });
        var initBinding = RouteInitBinding.Close(
            initSymbols,
            new RouteInitBindingComponents
            {
                Help = RouteInitHelpSections.Create(),
                Operation = RouteInitOperationFactory.Create(inputs.LockStoreRoot),
                Renderers = new CliRendererSet<RouteInitResult>(
                    RouteInitHumanRenderer.Render,
                    RouteInitJsonRenderer.Render),
                DiagnosticRenderer = RouteInitDiagnosticRenderer.Render,
            });
        var findSymbols = FindBinding.CreateSymbols();
        var findBinding = FindBinding.Close(
            findSymbols,
            new FindBindingComponents
            {
                Help = FindHelpSections.Create(),
                Operation = FindOperationFactory.Create(),
                Renderers = new CliRendererSet<FindResult>(
                    FindHumanRenderer.Render,
                    FindJsonRenderer.Render),
                DiagnosticRenderer = FindDiagnosticRenderer.Render,
            });
        var indexSymbols = IndexBinding.CreateSymbols();
        var indexBinding = IndexBinding.Close(
            symbols: indexSymbols,
            help: IndexHelpSections.Create(),
            operation: IndexOperationFactory.Create(inputs.LockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<IndexResult>(
                IndexHumanRenderer.Render,
                IndexJsonRenderer.Render),
            diagnosticRenderer: IndexDiagnosticRenderer.Render);
        var installSymbols = InstallBinding.CreateSymbols();
        var installBinding = InstallBinding.Close(
            symbols: installSymbols,
            help: InstallHelpSections.Create(),
            operation: InstallOperationFactory.Create(interactiveSession, inputs.LockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<InstallResult>(
                InstallHumanRenderer.Render,
                InstallJsonRenderer.Render),
            diagnosticRenderer: InstallDiagnosticRenderer.Render);
        var referencesSymbols = ReferencesBinding.CreateSymbols();
        var referencesBinding = ReferencesBinding.Close(
            referencesSymbols,
            new ReferencesBindingComponents
            {
                Help = ReferencesHelpSections.Create(),
                Operation = ReferencesOperationFactory.Create(),
                Renderers = new CliRendererSet<ReferencesResult>(
                    ReferencesHumanRenderer.Render,
                    ReferencesJsonRenderer.Render),
                DiagnosticRenderer = ReferencesDiagnosticRenderer.Render,
            });
        var contextSymbols = ContextBinding.CreateSymbols();
        var contextBinding = ContextBinding.Close(
            contextSymbols,
            new ContextBindingComponents
            {
                Help = ContextHelpSections.Create(),
                Operation = ContextOperationFactory.Create(),
                Renderers = new CliRendererSet<ContextResult>(
                    ContextHumanRenderer.Render,
                    ContextJsonRenderer.Render),
                DiagnosticRenderer = ContextDiagnosticRenderer.Render,
            });
        var extensionGroup = ExtensionBinding.CreateGroup();
        var extensionListSymbols = ExtensionListBinding.CreateSymbols(extensionGroup);
        var extensionListBinding = ExtensionListBinding.Close(
            extensionListSymbols,
            new ExtensionListBindingComponents
            {
                Help = ExtensionListHelpSections.Create(),
                Operation = ExtensionListOperationFactory.Create(),
                Renderers = new CliRendererSet<ExtensionListResult>(
                    ExtensionListHumanRenderer.Render,
                    ExtensionListJsonRenderer.Render),
                DiagnosticRenderer = ExtensionListDiagnosticRenderer.Render,
            });
        var extensionInspectSymbols = ExtensionInspectBinding.CreateSymbols(extensionGroup);
        var extensionInspectBinding = ExtensionInspectBinding.Close(
            extensionInspectSymbols,
            new ExtensionInspectBindingComponents
            {
                Help = ExtensionInspectHelpSections.Create(),
                Operation = ExtensionInspectOperationFactory.Create(),
                Renderers = new CliRendererSet<ExtensionInspectResult>(
                    ExtensionInspectHumanRenderer.Render,
                    ExtensionInspectJsonRenderer.Render),
                DiagnosticRenderer = ExtensionInspectDiagnosticRenderer.Render,
            });
        var extensionCreateSymbols = ExtensionCreateBinding.CreateSymbols(extensionGroup);
        var extensionCreateBinding = ExtensionCreateBinding.Close(
            extensionCreateSymbols,
            new ExtensionCreateBindingComponents
            {
                Help = ExtensionCreateHelpSections.Create(),
                Operation = ExtensionCreateOperationFactory.Create(interactiveSession),
                Renderers = new CliRendererSet<ExtensionCreateResult>(
                    ExtensionCreateHumanRenderer.Render,
                    ExtensionCreateJsonRenderer.Render),
                DiagnosticRenderer = ExtensionCreateDiagnosticRenderer.Render,
            });
        var tree = CliCommandTree.Create(
            rootHelp,
            [
                new CliRootBranch(
                    routeGroup,
                    RouteHelpSections.CreateGroup(),
                    routeDelimiterPolicies),
                new CliRootBranch(
                    extensionGroup,
                    ExtensionHelpSections.CreateGroup(),
                    []),
            ],
            [listBinding, inspectBinding, initBinding, findBinding, indexBinding, installBinding, referencesBinding, extensionListBinding, extensionInspectBinding, extensionCreateBinding, contextBinding],
            rootLeaves:
            [
                new CliRootLeaf(findSymbols.FindCommand, []),
                new CliRootLeaf(indexSymbols.IndexCommand, []),
                new CliRootLeaf(installSymbols.InstallCommand, []),
                new CliRootLeaf(referencesSymbols.ReferencesCommand, []),
                new CliRootLeaf(contextSymbols.ContextCommand, []),
            ]);
        var workspaceSelector = new CliWorkspaceSelector(new PhysicalPathResolver());
        return new CliCoreApplication(process, tree, workspaceSelector);
    }
}

internal sealed record CliCompositionInputs
{
    public required TextReader StandardInput { get; init; }

    public required TextWriter PromptOutput { get; init; }

    public required bool StandardInputRedirected { get; init; }

    public required bool PromptOutputRedirected { get; init; }

    public WorkspaceLockStoreRoot? LockStoreRoot { get; init; }
}
