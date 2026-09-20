using System.Text.Json;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class PipelineTests
{
    [Theory(DisplayName = "CLI pipeline preserves a result formed before later cancellation"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData((int)CliSemanticStatus.Interrupted, 130, false)]
    [InlineData((int)CliSemanticStatus.Complete, 0, true)]
    public async Task ResultSurvivesLaterCancellation(int statusValue, int exitCode, bool stdout)
    {
        using var cancellation = new CancellationTokenSource();
        var output = new StringWriter();
        var error = new StringWriter();
        var calls = 0;
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>((request, token) =>
        {
            calls++;
            cancellation.Cancel();
            return ValueTask.FromResult(Result((CliSemanticStatus)statusValue));
        }, Rendering());
        var completion = await pipeline.ExecuteAsync("request", new(CliFormat.Text, CliDetail.Minimal, null), new(output, error), cancellation.Token);
        Assert.Equal(1, calls);
        Assert.Equal(exitCode, completion.ExitCode);
        Assert.Equal(stdout ? "Result preserved." + Environment.NewLine : string.Empty, output.ToString());
        Assert.Equal(stdout ? string.Empty : "Result preserved." + Environment.NewLine, error.ToString());
    }

    [Theory(DisplayName = "CLI pipeline invokes one operation and selection for either format at every detail"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData(false, (int)CliDetail.Minimal)]
    [InlineData(false, (int)CliDetail.Standard)]
    [InlineData(false, (int)CliDetail.Full)]
    [InlineData(false, (int)CliDetail.Debug)]
    [InlineData(true, (int)CliDetail.Minimal)]
    [InlineData(true, (int)CliDetail.Standard)]
    [InlineData(true, (int)CliDetail.Full)]
    [InlineData(true, (int)CliDetail.Debug)]
    public async Task PipelineInvokesOnlySelectedFormat(bool json, int detailValue)
    {
        var detail = (CliDetail)detailValue;
        var operationCalls = 0;
        var selectionCalls = 0;
        var textCalls = 0;
        var output = new StringWriter();
        var error = new StringWriter();
        var result = Result(CliSemanticStatus.Complete);
        var rendering = Rendering() with
        {
            Selector = (actual, selection) =>
            {
                selectionCalls++;
                Assert.Same(result, actual);
                Assert.Equal(detail, selection.Detail);
                return Report(actual) with { Diagnostics = ["diagnostic\nvalue"] };
            },
            DataTextRenderer = (data, selection, style) =>
            {
                textCalls++;
                Assert.Equal("typed data", data.Value);
                Assert.Equal(detail, selection.Detail);
                return new([]);
            },
        };
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>((request, token) =>
        {
            operationCalls++;
            return ValueTask.FromResult(result);
        }, rendering);
        var completion = await pipeline.ExecuteAsync("request", new(json ? CliFormat.Json : CliFormat.Text, detail, null),
            new(output, error), TestContext.Current.CancellationToken);
        Assert.Equal(1, operationCalls);
        Assert.Equal(1, selectionCalls);
        Assert.Equal(json ? 0 : 1, textCalls);
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardOutput, completion.PrimaryOutputTarget);
        Assert.Equal(detail == CliDetail.Debug ? "diagnostic\\nvalue" + Environment.NewLine : string.Empty, error.ToString());
        if (json)
        {
            using var document = JsonDocument.Parse(output.ToString());
            Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
            Assert.Equal("typed data", document.RootElement.GetProperty("data").GetProperty("value").GetString());
            Assert.DoesNotContain("diagnostic", output.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain("\n", output.ToString().TrimEnd('\r', '\n'), StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal("Result preserved." + Environment.NewLine, output.ToString());
        }
    }

    [Theory(DisplayName = "CLI text completion and stream follow every semantic status"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData((int)CliSemanticStatus.Complete, 0, true)]
    [InlineData((int)CliSemanticStatus.Incomplete, 3, true)]
    [InlineData((int)CliSemanticStatus.Blocked, 5, false)]
    [InlineData((int)CliSemanticStatus.Failed, 1, false)]
    [InlineData((int)CliSemanticStatus.Attention, 2, true)]
    [InlineData((int)CliSemanticStatus.Invalid, 4, false)]
    [InlineData((int)CliSemanticStatus.Interrupted, 130, false)]
    public async Task StatusPreservesStreamsInBothFormats(int statusValue, int exitCode, bool textStdout)
    {
        foreach (var format in new[] { CliFormat.Text, CliFormat.Json })
        {
            var output = new StringWriter();
            var error = new StringWriter();
            var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>(
                (request, token) => ValueTask.FromResult(Result((CliSemanticStatus)statusValue)), Rendering());
            var completion = await pipeline.ExecuteAsync("request", new(format, CliDetail.Minimal, null), new(output, error), TestContext.Current.CancellationToken);
            var stdout = format == CliFormat.Json || textStdout;
            Assert.Equal(exitCode, completion.ExitCode);
            Assert.Equal(stdout ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
            Assert.Equal(stdout, output.ToString().Length > 0);
            Assert.Equal(!stdout, error.ToString().Length > 0);
            if (format == CliFormat.Json)
            {
                using var document = JsonDocument.Parse(output.ToString());
                Assert.Equal("test", document.RootElement.GetProperty("command").GetString());
            }
        }
    }

    [Theory(DisplayName = "CLI presentation rejects undefined format or detail before selector and writer effects"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UndefinedPresentationFailsBeforeSelection(bool format)
    {
        var calls = 0;
        var output = new StringWriter();
        var error = new StringWriter();
        var rendering = Rendering() with { Selector = (result, selection) => { calls++; return Report(result); } };
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>((request, token) => ValueTask.FromResult(Result(CliSemanticStatus.Complete)), rendering);
        var presentation = new CliPresentation(format ? (CliFormat)int.MaxValue : CliFormat.Text, format ? CliDetail.Minimal : (CliDetail)int.MaxValue, null);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await pipeline.PresentAsync(Result(CliSemanticStatus.Complete), presentation, new(output, error)));
        Assert.Equal(0, calls);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Theory(DisplayName = "CLI selector cannot change operation command or status"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectorCannotChangeOperationIdentity(bool command)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var rendering = Rendering() with
        {
            Selector = (result, selection) => command ? Report(result) with { Command = "other" } : Report(result) with { Status = CliSemanticStatus.Failed },
        };
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>((request, token) => ValueTask.FromResult(Result(CliSemanticStatus.Complete)), rendering);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await pipeline.PresentAsync(Result(CliSemanticStatus.Complete), new(CliFormat.Text, CliDetail.Minimal, null), new(output, error)));
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact(DisplayName = "CLI data renderer failure is neither retried nor written"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task RendererFailureDoesNotRetry()
    {
        var calls = 0;
        var failure = new InvalidOperationException("Rendering failed.");
        var output = new StringWriter();
        var rendering = Rendering() with { DataTextRenderer = (data, selection, style) => { calls++; throw failure; } };
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>((request, token) => ValueTask.FromResult(Result(CliSemanticStatus.Complete)), rendering);
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pipeline.PresentAsync(Result(CliSemanticStatus.Complete), new(CliFormat.Text, CliDetail.Minimal, null), new(output, TextWriter.Null)));
        Assert.Same(failure, actual);
        Assert.Equal(1, calls);
        Assert.Equal(string.Empty, output.ToString());
    }

    [Theory(DisplayName = "CLI output rejects an undefined target or target inconsistent with status before writing"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OutputValidatesBeforeWrites(bool undefined)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var malformed = new CliRenderedOutput(CliSemanticStatus.Blocked, CliFormat.Text,
            undefined ? (CliOutputTarget)int.MaxValue : CliOutputTarget.StandardOutput, "blocked", null);
        if (undefined)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await CliOutputStage.WriteAsync(malformed, new(output, error), TestContext.Current.CancellationToken));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await CliOutputStage.WriteAsync(malformed, new(output, error), TestContext.Current.CancellationToken));
        }
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Theory(DisplayName = "CLI report preserves explicit workspace and one next action at every detail"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    [InlineData((int)CliDetail.Full)]
    [InlineData((int)CliDetail.Debug)]
    public async Task WorkspaceAndNextSurviveSelection(int detailValue)
    {
        var workspace = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-pipeline-workspace"));
        var rendering = Rendering() with
        {
            Selector = (result, selection) => Report(result) with
            {
                Workspace = new(workspace, Explicit: true),
                Next = new("open-forge doctor", "Review the findings."),
            },
        };
        var pipeline = new CliReportPipeline<string, TestResult, CliPipelineTestData>(
            (request, token) => ValueTask.FromResult(Result(CliSemanticStatus.Attention)), rendering);
        var output = new StringWriter();
        await pipeline.PresentAsync(Result(CliSemanticStatus.Attention), new(CliFormat.Text, (CliDetail)detailValue, null), new(output, TextWriter.Null));
        var text = output.ToString();
        Assert.StartsWith($"Result preserved.{Environment.NewLine}Workspace: {workspace}{Environment.NewLine}", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split("Next: open-forge doctor", StringSplitOptions.None).Length - 1);
        Assert.Equal(detailValue >= (int)CliDetail.Standard, text.Contains("Review the findings.", StringComparison.Ordinal));
    }

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
        Headline = new("Result preserved.", CliHeadlineKind.Done),
        Data = new("typed data"),
    };

    private static TestResult Result(CliSemanticStatus status) => new("test", status, null, null);
    private sealed record TestResult(string Command, CliSemanticStatus Status, CliWorkspace? Workspace, CliNextAction? Next) : ICliCommandResult;
}
