using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction;

internal static class RouteInspectInteractionApplication
{
    internal static async Task<RouteInspectInteractionRun> RunAsync(
        string[] arguments,
        string workspacePath,
        string standardInput,
        bool canPrompt,
        CancellationToken cancellationToken)
    {
        using var input = new StringReader(standardInput);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: input,
            promptOutput: standardError,
            canPrompt: canPrompt);
        var application = Create(session);

        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(workspacePath),
            new CliOutputWriters(standardOutput, standardError),
            cancellationToken);
        var remainingInput = await input.ReadLineAsync(CancellationToken.None);
        return new RouteInspectInteractionRun
        {
            Completion = completion,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            RemainingInput = remainingInput,
        };
    }

    private static CliCoreApplication Create(CliInteractiveSession session)
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var binding = RouteInspectBinding.Close(
            symbols,
            new RouteInspectBindingComponents
            {
                Help = RouteInspectHelpSections.CreateInspect(),
                Operation = RouteInspectOperationFactory.Create(session),
                Renderers = new CliRendererSet<RouteInspectResult>(
                    RouteInspectHumanRenderer.Render,
                    RouteInspectJsonRenderer.Render),
                DiagnosticRenderer = RouteInspectDiagnosticRenderer.Render,
            });
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(route, RouteHelpSections.CreateGroup(), [])],
            [binding]);
        return new CliCoreApplication(
            new CliProcessIdentity("open-forge", "test"),
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
    }
}

internal sealed record RouteInspectInteractionRun
{
    public required CliProcessCompletion Completion { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string? RemainingInput { get; init; }
}
