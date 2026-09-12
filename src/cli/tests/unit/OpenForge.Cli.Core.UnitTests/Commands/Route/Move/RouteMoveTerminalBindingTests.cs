using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveTerminalBindingTests
{
    [Fact(DisplayName = "Route Move help with unknown named input stays shell-invalid and skips the command")]
    [Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public async Task HelpWithUnknownNamedInputStaysAtTheShellBoundary()
    {
        var routeGroup = RouteBinding.CreateGroup();
        var symbols = RouteMoveBindingTestData.CreateBinding().CreateSymbols(routeGroup);
        var binding = new CountingBinding(symbols.MoveCommand);
        var result = await RunApplicationAsync(
            routeGroup,
            binding,
            ["route", "move", "--unknown", "--help"]);

        Assert.Equal(4, result.Completion.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Equal(0, binding.InvocationCount);
        Assert.Equal(0, binding.InvalidPresentationCount);
    }

    [Theory(DisplayName = "Route Move ordinary terminal input succeeds without command execution")]
    [InlineData("--help")]
    [InlineData("--version")]
    [Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public async Task OrdinaryTerminalInputBypassesTheCommand(string terminalOption)
    {
        var routeGroup = RouteBinding.CreateGroup();
        var symbols = RouteMoveBindingTestData.CreateBinding().CreateSymbols(routeGroup);
        var binding = new CountingBinding(symbols.MoveCommand);
        var result = await RunApplicationAsync(
            routeGroup,
            binding,
            ["route", "move", terminalOption]);

        Assert.Equal(0, result.Completion.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal(0, binding.InvocationCount);
        Assert.Equal(0, binding.InvalidPresentationCount);
    }

    private static async Task<BoundaryResult> RunApplicationAsync(
        Command routeGroup,
        CountingBinding binding,
        string[] arguments)
    {
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(routeGroup, CliHelpContent.Empty)],
            [binding]);
        var application = new CliCoreApplication(
            new CliProcessIdentity("open-forge", "test-version"),
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new BoundaryResult(
            completion,
            standardOutput.ToString(),
            standardError.ToString());
    }

    private sealed record BoundaryResult(
        CliProcessCompletion Completion,
        string StandardOutput,
        string StandardError);

    private sealed class CountingBinding(Command command) : ICliCommandBinding
    {
        public Command Command { get; } = command;

        public CliHelpContent Help { get; } = CliHelpContent.Empty;

        public CliWorkspaceRequirement WorkspaceRequirement => CliWorkspaceRequirement.Required;

        internal int InvocationCount { get; private set; }

        internal int InvalidPresentationCount { get; private set; }

        public ValueTask<CliProcessCompletion> InvokeAsync(
            CliBindingParse parse,
            CliInvocation invocation,
            CliOutputWriters writers,
            CancellationToken cancellationToken)
        {
            InvocationCount++;
            throw new InvalidOperationException("Terminal evidence must not invoke Route Move.");
        }

        public ValueTask<CliProcessCompletion> PresentInvalidAsync(
            CliInvalidBindingInput input,
            CliOutputWriters writers,
            CancellationToken cancellationToken)
        {
            InvalidPresentationCount++;
            throw new InvalidOperationException("Terminal evidence must not present a Route Move result.");
        }
    }
}

internal static class RouteMoveBindingTestData
{
    internal static RouteMoveBinding CreateBinding()
        => new(
            new RouteMoveBindingValidator(),
            new RouteMoveInvalidResultFactory());
}
