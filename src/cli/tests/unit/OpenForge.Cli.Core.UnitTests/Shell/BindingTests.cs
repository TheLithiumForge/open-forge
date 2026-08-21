using System.CommandLine;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class BindingTests
{
    [Fact(DisplayName = "CLI closed generic binding executes binder operation and renderer once")]
    [Trait("Feature", "cli-binding"), Trait("Evidence", "Unit")]
    public async Task ClosedGenericBindingExecutesEachBoundaryOnce()
    {
        var command = new Command("leaf");
        var binderCalls = 0;
        var operationCalls = 0;
        var rendererCalls = 0;
        var binding = new CliCommandBinding<TestRequest, TestResult>(
            command,
            CliHelpContent.Empty,
            CliWorkspaceRequirement.Absent,
            (parse, invocation) =>
            {
                binderCalls++;
                return CliBindResult<TestRequest, TestResult>.Bound(new TestRequest("value"));
            },
            (invalid, input, environment) => Result(CliSemanticStatus.Invalid),
            (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
            },
            new CliRendererSet<TestResult>(
                presentation =>
                {
                    rendererCalls++;
                    return "complete";
                },
                presentation => "{}"));
        var group = new Command("group");
        group.Add(command);
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(group, CliHelpContent.Empty, [])],
            [binding]);
        var parse = new CliParser(tree).Parse(["group", "leaf"]);
        var invocation = Invocation();
        var standardOutput = new StringWriter();

        var completion = await ((ICliCommandBinding)binding).InvokeAsync(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            invocation,
            new CliOutputWriters(standardOutput, new StringWriter()),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, binderCalls);
        Assert.Equal(1, operationCalls);
        Assert.Equal(1, rendererCalls);
        Assert.Equal("complete" + Environment.NewLine, standardOutput.ToString());
        Assert.Equal(0, completion.ExitCode);
    }

    [Fact(DisplayName = "CLI absent workspace requirement preserves genuine absence")]
    [Trait("Feature", "cli-invocation"), Trait("Evidence", "Unit")]
    public void AbsentWorkspaceRequirementPerformsNoSelection()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"open-forge-unit-missing-{Guid.NewGuid():N}");
        var input = new CliGlobalInput(
            missing,
            1,
            CliOutputFormat.Human,
            0,
            CliView.Expanded,
            0,
            CliVerbosity.Normal,
            0,
            false,
            0,
            false,
            0);

        var resolution = CliInvocationResolver.Resolve(
            input,
            new CliProcessIdentity("open-forge", "test"),
            new CliProcessEnvironment(missing),
            CliWorkspaceRequirement.Absent,
            new CliWorkspaceSelector(new PhysicalPathResolver()));

        Assert.Null(resolution.InvalidInput);
        Assert.Null(resolution.Invocation?.Workspace);
        Assert.Equal(missing, resolution.Invocation?.WorkspaceRequest.ExplicitPath);
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "CLI binding constructors reject null callables")]
    [Trait("Feature", "cli-binding"), Trait("Evidence", "Unit")]
    public void BindingConstructorsRejectNullCallables()
    {
        var command = new Command("leaf");
        CliRequestBinder<TestRequest, TestResult> binder =
            (parse, invocation) => CliBindResult<TestRequest, TestResult>.Bound(new TestRequest("value"));
        CliInvalidResultFactory<TestResult> invalid =
            (input, global, environment) => Result(CliSemanticStatus.Invalid);
        CliOperation<TestRequest, TestResult> operation =
            (request, cancellationToken) => ValueTask.FromResult(Result(CliSemanticStatus.Complete));
        var renderers = new CliRendererSet<TestResult>(presentation => "human", presentation => "{}");

        Assert.Throws<ArgumentNullException>(() => new CliCommandBinding<TestRequest, TestResult>(
            command,
            CliHelpContent.Empty,
            CliWorkspaceRequirement.Absent,
            null!,
            invalid,
            operation,
            renderers));
        Assert.Throws<ArgumentNullException>(() => new CliCommandBinding<TestRequest, TestResult>(
            command,
            CliHelpContent.Empty,
            CliWorkspaceRequirement.Absent,
            binder,
            invalid,
            null!,
            renderers));
        Assert.Throws<ArgumentNullException>(() => new CliOutputWriters(null!, TextWriter.Null));
    }

    [Fact(DisplayName = "CLI parser failures never enter a selected command binding")]
    [Trait("Feature", "cli-binding"), Trait("Evidence", "Unit")]
    public async Task ParserFailuresNeverEnterSelectedBinding()
    {
        var leaf = new Command("leaf");
        var group = new Command("group");
        group.Add(leaf);
        var invalidFactoryCalls = 0;
        var operationCalls = 0;
        var binding = new CliCommandBinding<TestRequest, TestResult>(
            leaf,
            CliHelpContent.Empty,
            CliWorkspaceRequirement.Absent,
            (parse, invocation) => CliBindResult<TestRequest, TestResult>.Bound(new TestRequest("value")),
            (invalid, input, environment) =>
            {
                invalidFactoryCalls++;
                return Result(CliSemanticStatus.Invalid);
            },
            (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
            },
            new CliRendererSet<TestResult>(presentation => "human", presentation => "{}"));
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(group, CliHelpContent.Empty, [])],
            [binding]);
        var application = new CliCoreApplication(
            new CliProcessIdentity("open-forge", "test"),
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var completion = await application.RunAsync(
            ["group", "leaf", "--unknown"],
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, invalidFactoryCalls);
        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains("unknown", standardError.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static CliInvocation Invocation()
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, Path.GetTempPath()),
            null);
    }

    private static TestResult Result(CliSemanticStatus status)
    {
        return new TestResult("test", status, null, null);
    }

    private sealed record TestRequest(string Value);

    private sealed record TestResult(
        string Command,
        CliSemanticStatus Status,
        CliWorkspace? Workspace,
        CliNextAction? Next) : ICliCommandResult;
}
