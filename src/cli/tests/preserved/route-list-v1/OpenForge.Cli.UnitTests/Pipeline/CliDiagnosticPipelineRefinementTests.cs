using System.Text.Json;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.UnitTests.Pipeline;

public sealed class CliDiagnosticPipelineRefinementTests
{
    [Fact(DisplayName = "Route-list pipeline normal verbosity never invokes diagnostics or changes stderr"), Trait("Feature", "cli-pipeline"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public async Task NormalVerbosityDoesNotRenderDiagnostics()
    {
        var execution = await RunAsync(CliOutputFormat.Human, CliVerbosity.Normal);

        Assert.Equal(1, execution.OperationCalls);
        Assert.Equal(1, execution.Primary.HumanCalls);
        Assert.Equal(0, execution.Primary.JsonCalls);
        Assert.Equal(0, execution.Diagnostics.Calls);
        Assert.Null(execution.Diagnostics.Message);
        Assert.Equal("human:result" + Environment.NewLine, execution.StandardOutput);
        Assert.Equal(string.Empty, execution.StandardError);
    }

    [Theory(DisplayName = "Route-list pipeline verbose human and JSON preserve primary output while rendering one stderr diagnostic"),
     InlineData((int)CliOutputFormat.Human),
     InlineData((int)CliOutputFormat.Json),
     Trait("Feature", "cli-pipeline"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public async Task VerboseRendersOneIsolatedDiagnostic(int formatValue)
    {
        var format = (CliOutputFormat)formatValue;
        var normal = await RunAsync(format, CliVerbosity.Normal);
        var verbose = await RunAsync(format, CliVerbosity.Verbose);

        Assert.Equal(1, verbose.OperationCalls);
        Assert.Equal(format == CliOutputFormat.Human ? 1 : 0, verbose.Primary.HumanCalls);
        Assert.Equal(format == CliOutputFormat.Json ? 1 : 0, verbose.Primary.JsonCalls);
        Assert.Equal(1, verbose.Diagnostics.Calls);
        var diagnostic = Assert.IsType<CliDiagnosticMessage<SampleResult>>(verbose.Diagnostics.Message);
        Assert.Equal(CliSemanticStatus.Complete, diagnostic.Status);
        Assert.Same(verbose.Result, diagnostic.Result);
        Assert.Same(verbose.Presentation, diagnostic.Presentation);

        Assert.Equal(normal.StandardOutput, verbose.StandardOutput);
        Assert.Equal(normal.Completion.Status, verbose.Completion.Status);
        Assert.Equal(normal.Completion.ExitCode, verbose.Completion.ExitCode);
        Assert.Equal(normal.Completion.PrimaryOutputTarget, verbose.Completion.PrimaryOutputTarget);
        Assert.Equal("diagnostic:complete" + Environment.NewLine, verbose.StandardError);

        if (format == CliOutputFormat.Json)
        {
            using var document = JsonDocument.Parse(verbose.StandardOutput);
            Assert.Equal("result", document.RootElement.GetProperty("value").GetString());
            Assert.DoesNotContain("diagnostic", verbose.StandardOutput, StringComparison.Ordinal);
        }
    }

    private static async Task<PipelineExecution> RunAsync(CliOutputFormat format, CliVerbosity verbosity)
    {
        var operationCalls = 0;
        var result = new SampleResult("result");
        var presentation = new CliPresentation(format, CliView.Expanded, verbosity);
        var primary = new PrimaryRendererProbe();
        var diagnostics = new DiagnosticRendererProbe();
        var pipeline = new CliCommandPipeline<SampleRequest, SampleResult>(
            (request, cancellationToken) =>
            {
                operationCalls++;
                Assert.Equal("request", request.Value);
                Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
                return Task.FromResult(new CliOperationResult<SampleResult>(CliSemanticStatus.Complete, result));
            },
            new CliRendererSet<SampleResult>(primary.RenderHuman, primary.RenderJson),
            diagnostics.Render);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();

        var completion = await pipeline.RunAsync(
            new CliCommandInvocation<SampleRequest>(new SampleRequest("request"), presentation),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        return new PipelineExecution(
            operationCalls,
            result,
            presentation,
            primary,
            diagnostics,
            completion,
            standardOutput.ToString(),
            standardError.ToString());
    }

    private sealed record SampleRequest(string Value);

    private sealed record SampleResult(string Value);

    private sealed record PipelineExecution(
        int OperationCalls,
        SampleResult Result,
        CliPresentation Presentation,
        PrimaryRendererProbe Primary,
        DiagnosticRendererProbe Diagnostics,
        CliCompletion Completion,
        string StandardOutput,
        string StandardError);

    private sealed class PrimaryRendererProbe
    {
        internal int HumanCalls { get; private set; }

        internal int JsonCalls { get; private set; }

        internal string RenderHuman(CliPresentationMessage<SampleResult> presentation)
        {
            HumanCalls++;
            return $"human:{presentation.Result.Value}";
        }

        internal string RenderJson(CliPresentationMessage<SampleResult> presentation)
        {
            JsonCalls++;
            return $"{{\"value\":\"{presentation.Result.Value}\"}}";
        }
    }

    private sealed class DiagnosticRendererProbe
    {
        internal int Calls { get; private set; }

        internal CliDiagnosticMessage<SampleResult>? Message { get; private set; }

        internal CliRenderedDiagnostic Render(CliDiagnosticMessage<SampleResult> diagnostic)
        {
            Calls++;
            Message = diagnostic;
            return new CliRenderedDiagnostic("diagnostic:complete");
        }
    }
}
