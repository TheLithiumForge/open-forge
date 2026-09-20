using OpenForge.Cli.TestSupport.Isolation;
using System.CommandLine;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class BindingTests
{
    [Fact(DisplayName = "CLI closed typed binding executes binder operation selector and text renderer once"), Trait("Feature", "cli-binding"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ClosedGenericBindingExecutesEachBoundaryOnce()
    {
        var command = new Command("leaf");
        var binderCalls = 0;
        var operationCalls = 0;
        var selectorCalls = 0;
        var rendererCalls = 0;
        var requestBinding = RequestBinding(command) with
        {
            Binder = (parse, invocation) =>
            {
                binderCalls++;
                return CliBindResult<TestRequest, TestResult>.Bound(new("value"));
            },
            Operation = (request, token) =>
            {
                operationCalls++;
                Assert.Equal("value", request.Value);
                return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
            },
        };
        var rendering = Rendering() with
        {
            Selector = (result, selection) => { selectorCalls++; return Report(result); },
            DataTextRenderer = (data, selection, style) => { rendererCalls++; return new([]); },
        };
        var binding = CliReportBinding.Close(requestBinding, rendering);
        var tree = CreateTree(command, binding);
        var parse = tree.Parse(["group", "leaf"]);
        var output = new StringWriter();
        var completion = await ((ICliCommandBinding)binding).InvokeAsync(new(parse.Result, parse.OriginalArguments), Invocation(),
            new(output, TextWriter.Null), TestContext.Current.CancellationToken);
        Assert.Equal(1, binderCalls);
        Assert.Equal(1, operationCalls);
        Assert.Equal(1, selectorCalls);
        Assert.Equal(1, rendererCalls);
        Assert.Equal("Completed." + Environment.NewLine, output.ToString());
        Assert.Equal(0, completion.ExitCode);
    }

    [Fact(DisplayName = "CLI absent workspace requirement preserves genuine absence"), Trait("Feature", "cli-invocation"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public void AbsentWorkspaceRequirementPerformsNoSelection()
    {
        var missing = TestDataHome.AbsentPath("unit-missing");
        var input = new CliGlobalInput(missing, 1, CliFormat.Text, 0, CliDetail.Minimal, 0, null, 0, false, 0, false, 0);
        var resolution = CliInvocationResolver.Resolve(input, new("open-forge", "test"), new(missing),
            CliWorkspaceRequirement.Absent, new CliWorkspaceSelector(new PhysicalPathResolver()));
        Assert.Null(resolution.InvalidInput);
        Assert.Null(resolution.Invocation?.Workspace);
        Assert.Equal(missing, resolution.Invocation?.WorkspaceRequest.ExplicitPath);
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "CLI binding constructors reject null callables and metadata"), Trait("Feature", "cli-binding"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public void BindingConstructorsRejectNullCallables()
    {
        var binding = RequestBinding(new Command("leaf"));
        var rendering = Rendering();
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding with { Binder = null! }, rendering));
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding with { InvalidResultFactory = null! }, rendering));
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding with { Operation = null! }, rendering));
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding, rendering with { Selector = null! }));
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding, rendering with { DataTextRenderer = null! }));
        Assert.Throws<ArgumentNullException>(() => CliReportBinding.Close(binding, rendering with { DataJsonTypeInfo = null! }));
        Assert.Throws<ArgumentNullException>(() => new CliOutputWriters(null!, TextWriter.Null));
    }

    [Fact(DisplayName = "CLI parser failures never enter a selected command binding"), Trait("Feature", "cli-binding"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ParserFailuresNeverEnterSelectedBinding()
    {
        var command = new Command("leaf");
        var invalidCalls = 0;
        var operationCalls = 0;
        var binding = CliReportBinding.Close(RequestBinding(command) with
        {
            InvalidResultFactory = input => { invalidCalls++; return Result(CliSemanticStatus.Invalid); },
            Operation = (request, token) => { operationCalls++; return ValueTask.FromResult(Result(CliSemanticStatus.Complete)); },
        }, Rendering());
        var application = new CliCoreApplication(new("open-forge", "test"), CreateTree(command, binding), new CliWorkspaceSelector(new PhysicalPathResolver()));
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await application.RunAsync(["group", "leaf", "--unknown"], new(Path.GetTempPath()), new(output, error), TestContext.Current.CancellationToken);
        Assert.Equal(0, invalidCalls);
        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("unknown", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "CLI invalid bound result bypasses operation and preserves JSON stdout"), Trait("Feature", "cli-binding"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task BoundInvalidResultBypassesOperation()
    {
        var command = new Command("leaf");
        var operationCalls = 0;
        var binding = CliReportBinding.Close(RequestBinding(command) with
        {
            Binder = (parse, invocation) => CliBindResult<TestRequest, TestResult>.Invalid(Result(CliSemanticStatus.Invalid)),
            Operation = (request, token) => { operationCalls++; return ValueTask.FromResult(Result(CliSemanticStatus.Complete)); },
        }, Rendering());
        var parse = CreateTree(command, binding).Parse(["group", "leaf"]);
        var output = new StringWriter();
        var error = new StringWriter();
        var invocation = Invocation() with { Presentation = new(CliFormat.Json, CliDetail.Minimal, null) };
        var completion = await binding.InvokeAsync(new(parse.Result, parse.OriginalArguments), invocation, new(output, error), TestContext.Current.CancellationToken);
        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
        Assert.Equal(string.Empty, error.ToString());
        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
    }

    private static CliRequestBinding<TestRequest, TestResult> RequestBinding(Command command) => new()
    {
        Command = command,
        Help = CliHelpContent.Empty,
        WorkspaceRequirement = CliWorkspaceRequirement.Absent,
        Binder = (parse, invocation) => CliBindResult<TestRequest, TestResult>.Bound(new("value")),
        InvalidResultFactory = input => Result(CliSemanticStatus.Invalid),
        Operation = (request, token) => ValueTask.FromResult(Result(CliSemanticStatus.Complete)),
    };

    private static CliReportRendering<TestResult, CliPipelineTestData> Rendering() => new()
    {
        Selector = (result, selection) => Report(result),
        DataTextRenderer = (data, selection, style) => new([]),
        DataJsonTypeInfo = CliPipelineTestJsonContext.Relaxed.CliPipelineTestData,
        Shape = CliCommandShape.Summary,
    };

    private static CliReport<CliPipelineTestData> Report(TestResult result) => new()
    {
        Command = result.Command,
        Status = result.Status,
        Headline = new("Completed.", CliHeadlineKind.Done),
        Data = new("value"),
    };

    private static CliCommandTree CreateTree(Command command, ICliCommandBinding binding)
    {
        var group = new Command("group");
        group.Add(command);
        return CliCommandTree.Create(CliHelpContent.Empty, [new CliRootBranch(group, CliHelpContent.Empty)], [binding]);
    }

    private static CliInvocation Invocation() => new(new("open-forge", "test"), new(CliFormat.Text, CliDetail.Minimal, null),
        CliTerminalMode.None, new(null, Path.GetTempPath()), null);
    private static TestResult Result(CliSemanticStatus status) => new("test", status, null, null);
    private sealed record TestRequest(string Value);
    private sealed record TestResult(string Command, CliSemanticStatus Status, CliWorkspace? Workspace, CliNextAction? Next) : ICliCommandResult;
}
