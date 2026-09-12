using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class PipelineTests
{
    [Fact(DisplayName = "CLI operation validates cancellation before effects"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task OperationStageValidatesCancellationBeforeInvokingOperation()
    {
        var calls = 0;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CliOperationStage.InvokeAsync(
                new CliOperationRequest<string>("request"),
                (request, token) =>
                {
                    calls++;
                    return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
                },
                cancellation.Token));

        Assert.Equal(0, calls);
    }

    [Fact(DisplayName = "CLI pipeline presents an interrupted result after operation cancellation"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task PipelinePresentsInterruptedResultAfterOperationCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var pipeline = new CliCommandPipeline<string, TestResult>(
            (request, cancellationToken) =>
            {
                cancellation.Cancel();
                return ValueTask.FromResult(Result(CliSemanticStatus.Interrupted));
            },
            new CliRendererSet<TestResult>(presentation => "interrupted", presentation => "{}"));

        var completion = await pipeline.ExecuteAsync(
            "request",
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            new CliOutputWriters(standardOutput, standardError),
            cancellation.Token);

        Assert.Equal(130, completion.ExitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Equal("interrupted" + Environment.NewLine, standardError.ToString());
    }

    [Fact(DisplayName = "CLI pipeline retains a complete result formed before later cancellation"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task PipelineRetainsCompleteResultFormedBeforeLaterCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var standardOutput = new StringWriter();
        var result = Result(CliSemanticStatus.Complete);
        var pipeline = new CliCommandPipeline<string, TestResult>(
            (request, cancellationToken) =>
            {
                cancellation.Cancel();
                return ValueTask.FromResult(result);
            },
            new CliRendererSet<TestResult>(presentation => "complete", presentation => "{}"));

        var completion = await pipeline.ExecuteAsync(
            "request",
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            new CliOutputWriters(standardOutput, new StringWriter()),
            cancellation.Token);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal("complete" + Environment.NewLine, standardOutput.ToString());
    }

    [Fact(DisplayName = "CLI pipeline invokes one operation and selected renderer"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task PipelineInvokesOneOperationAndOneSelectedRenderer()
    {
        var operationCalls = 0;
        var humanCalls = 0;
        var jsonCalls = 0;
        var diagnostics = 0;
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var pipeline = new CliCommandPipeline<string, TestResult>(
            (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
            },
            new CliRendererSet<TestResult>(
                presentation =>
                {
                    humanCalls++;
                    return "human";
                },
                presentation =>
                {
                    jsonCalls++;
                    return "{\"status\":\"complete\"}";
                }),
            presentation =>
            {
                diagnostics++;
                return "diagnostic";
            });

        var completion = await pipeline.ExecuteAsync(
            "request",
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Verbose),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(0, humanCalls);
        Assert.Equal(1, jsonCalls);
        Assert.Equal(1, diagnostics);
        Assert.Equal("{\"status\":\"complete\"}" + Environment.NewLine, standardOutput.ToString());
        Assert.Equal("diagnostic" + Environment.NewLine, standardError.ToString());
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardOutput, completion.PrimaryOutputTarget);
    }

    [Theory(DisplayName = "CLI human output and completion follow status policy"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit"),
     InlineData((int)CliSemanticStatus.Complete, 0, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Incomplete, 3, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Blocked, 5, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Failed, 1, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Attention, 2, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Invalid, 4, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Interrupted, 130, (int)CliOutputTarget.StandardError)]
    public async Task HumanOutputAndCompletionFollowStatusPolicy(
        int statusValue,
        int exitCode,
        int targetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var target = (CliOutputTarget)targetValue;
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var pipeline = CreatePipeline(status);

        var completion = await pipeline.ExecuteAsync(
            "request",
            new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(exitCode, completion.ExitCode);
        Assert.Equal(target, completion.PrimaryOutputTarget);
        Assert.Equal(target == CliOutputTarget.StandardOutput ? "human" + Environment.NewLine : string.Empty, standardOutput.ToString());
        Assert.Equal(target == CliOutputTarget.StandardError ? "human" + Environment.NewLine : string.Empty, standardError.ToString());
    }

    [Fact(DisplayName = "CLI rendering rejects unknown presentation before effects"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void RenderingRejectsUnknownPresentationBeforeRendererEffects()
    {
        var calls = 0;
        var renderers = new CliRendererSet<TestResult>(
            presentation =>
            {
                calls++;
                return "human";
            },
            presentation =>
            {
                calls++;
                return "json";
            });
        var request = new CliPresentationRequest<TestResult>(
            Result(CliSemanticStatus.Complete),
            new CliPresentation((CliOutputFormat)int.MaxValue, CliView.Expanded, CliVerbosity.Normal));

        Assert.Throws<ArgumentOutOfRangeException>(() => CliRenderingStage.Render(request, renderers, null));
        Assert.Equal(0, calls);
    }

    [Fact(DisplayName = "CLI output validates messages before writer effects"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task OutputStageValidatesBeforeWriterEffects()
    {
        var standardOutput = new CountingWriter();
        var standardError = new CountingWriter();
        var malformed = new CliRenderedOutput(
            CliSemanticStatus.Complete,
            CliOutputFormat.Human,
            (CliOutputTarget)int.MaxValue,
            "content",
            null);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await CliOutputStage.WriteAsync(
                malformed,
                new CliOutputWriters(standardOutput, standardError),
                TestContext.Current.CancellationToken));
        Assert.Equal(0, standardOutput.Writes);
        Assert.Equal(0, standardError.Writes);
    }

    [Fact(DisplayName = "CLI output rejects a defined target that violates status policy"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task OutputStageRejectsDefinedButIncorrectTarget()
    {
        var standardOutput = new CountingWriter();
        var standardError = new CountingWriter();
        var malformed = new CliRenderedOutput(
            CliSemanticStatus.Blocked,
            CliOutputFormat.Human,
            CliOutputTarget.StandardOutput,
            "blocked",
            null);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CliOutputStage.WriteAsync(
                malformed,
                new CliOutputWriters(standardOutput, standardError),
                TestContext.Current.CancellationToken));
        Assert.Equal(0, standardOutput.Writes);
        Assert.Equal(0, standardError.Writes);
    }

    [Fact(DisplayName = "CLI renderer selection is cached and finite"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void RendererSelectionIsCachedAndFinite()
    {
        static string Human(CliPresentationRequest<TestResult> presentation) => "human";
        static string Json(CliPresentationRequest<TestResult> presentation) => "json";
        var renderers = new CliRendererSet<TestResult>(Human, Json);

        Assert.Same(renderers.Read(CliOutputFormat.Human), renderers.Read(CliOutputFormat.Human));
        Assert.Same(renderers.Read(CliOutputFormat.Json), renderers.Read(CliOutputFormat.Json));
        Assert.Throws<ArgumentOutOfRangeException>(() => renderers.Read((CliOutputFormat)int.MaxValue));
    }

    [Theory(DisplayName = "Missing compact renderer falls back within its format without rerunning the operation"),
     InlineData(false), InlineData(true), Trait("Feature", "cli-views"), Trait("Evidence", "Unit")]
    public async Task MissingCompactFallsBackWithinSelectedFormat(bool json)
    {
        var result = Result(CliSemanticStatus.Blocked);
        var operationCalls = 0;
        var rendererCalls = 0;
        var format = json ? CliOutputFormat.Json : CliOutputFormat.Human;
        var presentation = new CliPresentation(format, CliView.Compact, CliVerbosity.Normal);
        string Expanded(CliPresentationRequest<TestResult> request)
        {
            rendererCalls++;
            Assert.Same(result, request.Result);
            Assert.Equal(CliView.Expanded, request.Presentation.View);
            Assert.Equal(format, request.Presentation.Format);
            return json ? "{}" : "blocked";
        }

        static string Unexpected(CliPresentationRequest<TestResult> request) => throw new InvalidOperationException("Unselected format invoked.");
        var selected = new CliViewRenderers<TestResult>(Expanded);
        var unselected = new CliViewRenderers<TestResult>(Unexpected);
        var renderers = json
            ? new CliRendererSet<TestResult>(unselected, selected)
            : new CliRendererSet<TestResult>(selected, unselected);
        var pipeline = new CliCommandPipeline<string, TestResult>((request, token) =>
        {
            operationCalls++;
            return ValueTask.FromResult(result);
        }, renderers);
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await pipeline.ExecuteAsync("request", presentation, new CliOutputWriters(output, error), TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(1, rendererCalls);
        Assert.Equal(CliView.Compact, presentation.View);
        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(json ? "{}" + Environment.NewLine : string.Empty, output.ToString());
        Assert.Equal(json ? string.Empty : "blocked" + Environment.NewLine, error.ToString());
    }

    [Theory(DisplayName = "Available view invokes only the selected renderer with the original request"),
     InlineData(false), InlineData(true), Trait("Feature", "cli-views"), Trait("Evidence", "Unit")]
    public void AvailableViewUsesOriginalRequest(bool compact)
    {
        var view = compact ? CliView.Compact : CliView.Expanded;
        var request = new CliPresentationRequest<TestResult>(Result(CliSemanticStatus.Complete), new(CliOutputFormat.Human, view, CliVerbosity.Normal));
        var calls = new List<string>();
        string Expanded(CliPresentationRequest<TestResult> actual)
        {
            Assert.Same(request, actual);
            calls.Add("expanded");
            return "expanded";
        }

        string Compact(CliPresentationRequest<TestResult> actual)
        {
            Assert.Same(request, actual);
            calls.Add("compact");
            return "compact";
        }

        var actual = new CliViewRenderers<TestResult>(Expanded, Compact).Render(request);

        Assert.Equal(compact ? "compact" : "expanded", actual);
        Assert.Equal([actual], calls);
    }

    [Fact(DisplayName = "Renderer exceptions do not fall back or retry"), Trait("Feature", "cli-views"), Trait("Evidence", "Unit")]
    public void RendererFailureDoesNotFallBack()
    {
        var error = new InvalidOperationException("Rendering failed.");
        var expandedCalls = 0;
        var renderers = new CliViewRenderers<TestResult>(
            request => { expandedCalls++; return "expanded"; },
            request => throw error);
        var request = new CliPresentationRequest<TestResult>(Result(CliSemanticStatus.Complete), new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal));

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => renderers.Render(request)));
        Assert.Equal(0, expandedCalls);
    }

    [Fact(DisplayName = "Undefined view is rejected before invoking a renderer"), Trait("Feature", "cli-views"), Trait("Evidence", "Unit")]
    public void UndefinedViewDoesNotFallBack()
    {
        var renderers = new CliViewRenderers<TestResult>(request => throw new InvalidOperationException("Renderer invoked."));
        var request = new CliPresentationRequest<TestResult>(Result(CliSemanticStatus.Complete), new(CliOutputFormat.Human, (CliView)int.MaxValue, CliVerbosity.Normal));

        Assert.Throws<ArgumentOutOfRangeException>(() => renderers.Render(request));
    }

    private static CliCommandPipeline<string, TestResult> CreatePipeline(CliSemanticStatus status)
    {
        return new CliCommandPipeline<string, TestResult>(
            (request, cancellationToken) => ValueTask.FromResult(Result(status)),
            new CliRendererSet<TestResult>(
                presentation => "human",
                presentation => "{\"status\":\"value\"}"));
    }

    private static TestResult Result(CliSemanticStatus status)
    {
        return new TestResult("test", status, null, null);
    }

    private sealed record TestResult(
        string Command,
        CliSemanticStatus Status,
        CliWorkspace? Workspace,
        CliNextAction? Next) : ICliCommandResult;

    private sealed class CountingWriter : StringWriter
    {
        internal int Writes { get; private set; }

        public override Task WriteLineAsync(string? value)
        {
            Writes++;
            return base.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            Writes++;
            return base.WriteLineAsync(buffer, cancellationToken);
        }
    }
}
