using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.UnitTests.Pipeline;

public sealed class CliPipelineTests
{
    [Fact(DisplayName = "CLI operation stage invokes one operation once and passes cancellation"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task OperationStageInvokesOnceWithCancellation()
    {
        var invocationCount = 0;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var request = new CliOperationRequest<SampleRequest>(new SampleRequest("request"));

        var result = await CliOperationStage.InvokeAsync<SampleRequest, SampleResult>(
            request,
            (operationRequest, cancellationToken) =>
            {
                invocationCount++;
                Assert.Equal(request.Request, operationRequest);
                Assert.Equal(cancellation.Token, cancellationToken);
                return Task.FromResult(new CliOperationResult<SampleResult>(
                    CliSemanticStatus.Interrupted,
                    new SampleResult("cancelled")));
            },
            cancellation.Token);

        Assert.Equal(1, invocationCount);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal("cancelled", result.Result.Value);
    }

    [Theory(DisplayName = "CLI command pipeline rejects malformed presentation before operation invocation"),
     InlineData(0),
     InlineData(1),
     InlineData(2),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task PipelineRejectsMalformedPresentationBeforeOperation(int presentationField)
    {
        var operationCalls = 0;
        var pipeline = new CliCommandPipeline<SampleRequest, SampleResult>(
            (request, cancellationToken) =>
            {
                operationCalls++;
                return Task.FromResult(CompleteResult());
            },
            new CliRendererSet<SampleResult>(
                static _ => "human",
                static _ => "json"));
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var invocation = new CliCommandInvocation<SampleRequest>(
            new SampleRequest("request"),
            MalformedPresentation(presentationField));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => pipeline.RunAsync(
            invocation,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken));

        Assert.Equal(0, operationCalls);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Equal(string.Empty, standardError.ToString());
    }

    [Fact(DisplayName = "CLI presentation stage accepts an already-formed invalid concrete result"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void PresentationStageAcceptsInvalidResult()
    {
        var result = new CliOperationResult<SampleResult>(
            CliSemanticStatus.Invalid,
            new SampleResult("invalid-input"));
        var selection = Presentation(CliOutputFormat.Json);

        var presentation = CliPresentationStage.Present(result, selection);

        Assert.Equal(CliSemanticStatus.Invalid, presentation.Status);
        Assert.Same(result.Result, presentation.Result);
        Assert.Same(selection, presentation.Presentation);
    }

    [Fact(DisplayName = "CLI presentation stage fails closed for an unknown output format"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void PresentationStageRejectsUnknownOutputFormat()
    {
        var result = CompleteResult();
        var presentation = new CliPresentation(
            (CliOutputFormat)int.MaxValue,
            CliView.Expanded,
            CliVerbosity.Normal);

        Assert.Throws<ArgumentOutOfRangeException>(() => CliPresentationStage.Present(result, presentation));
    }

    [Fact(DisplayName = "CLI presentation stage fails closed for an unknown view"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void PresentationStageRejectsUnknownView()
    {
        var result = CompleteResult();
        var presentation = new CliPresentation(
            CliOutputFormat.Human,
            (CliView)int.MaxValue,
            CliVerbosity.Normal);

        Assert.Throws<ArgumentOutOfRangeException>(() => CliPresentationStage.Present(result, presentation));
    }

    [Fact(DisplayName = "CLI presentation stage fails closed for unknown verbosity"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void PresentationStageRejectsUnknownVerbosity()
    {
        var result = CompleteResult();
        var presentation = new CliPresentation(
            CliOutputFormat.Human,
            CliView.Expanded,
            (CliVerbosity)int.MaxValue);

        Assert.Throws<ArgumentOutOfRangeException>(() => CliPresentationStage.Present(result, presentation));
    }

    [Theory(DisplayName = "CLI rendering stage invokes only the selected cached renderer"),
     InlineData((int)CliOutputFormat.Human, 1, 0, "human:value"),
     InlineData((int)CliOutputFormat.Json, 0, 1, "json:value"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void RenderingStageInvokesOneRenderer(
        int formatValue,
        int expectedHumanCalls,
        int expectedJsonCalls,
        string expectedContent)
    {
        var format = (CliOutputFormat)formatValue;
        var probe = new RendererProbe();
        var renderers = new CliRendererSet<SampleResult>(probe.RenderHuman, probe.RenderJson);
        var presentation = new CliPresentationMessage<SampleResult>(
            CliSemanticStatus.Complete,
            new SampleResult("value"),
            Presentation(format));

        var rendered = CliRenderingStage.Render(presentation, renderers);

        Assert.Equal(expectedContent, rendered.Content);
        Assert.Equal(expectedHumanCalls, probe.HumanCalls);
        Assert.Equal(expectedJsonCalls, probe.JsonCalls);
    }

    [Theory(DisplayName = "CLI rendering stage rejects malformed presentation messages before renderer invocation"),
     InlineData(0),
     InlineData(1),
     InlineData(2),
     InlineData(3),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void RenderingStageRejectsMalformedMessageBeforeRenderer(int messageField)
    {
        var probe = new RendererProbe();
        var renderers = new CliRendererSet<SampleResult>(probe.RenderHuman, probe.RenderJson);
        var presentation = MalformedPresentationMessage(messageField);

        Assert.Throws<ArgumentOutOfRangeException>(() => CliRenderingStage.Render(presentation, renderers));

        Assert.Equal(0, probe.HumanCalls);
        Assert.Equal(0, probe.JsonCalls);
    }

    [Fact(DisplayName = "CLI renderer selection fails closed for an unknown output format"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void RendererSelectionRejectsUnknownFormat()
    {
        var renderers = new CliRendererSet<SampleResult>(
            static _ => "human",
            static _ => "json");

        Assert.Throws<ArgumentOutOfRangeException>(() => renderers.Read((CliOutputFormat)42));
    }

    [Theory(DisplayName = "CLI output preparation sends human and JSON primary results to accepted targets"),
     InlineData((int)CliSemanticStatus.Complete, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Incomplete, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Failed, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Blocked, (int)CliOutputFormat.Json, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Interrupted, (int)CliOutputFormat.Json, (int)CliOutputTarget.StandardOutput),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void OutputPreparationSelectsTarget(
        int statusValue,
        int formatValue,
        int expectedTargetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var format = (CliOutputFormat)formatValue;
        var expectedTarget = (CliOutputTarget)expectedTargetValue;
        var rendered = new CliRenderedResult(status, format, "primary");

        var output = CliOutputStage.Prepare(rendered);

        Assert.Equal(expectedTarget, output.Target);
        Assert.Equal("primary", output.Content);
    }

    [Theory(DisplayName = "CLI output stage writes one primary result only to its explicit selected writer"),
     InlineData((int)CliSemanticStatus.Complete, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputFormat.Human, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputFormat.Json, (int)CliOutputTarget.StandardOutput),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void OutputStageWritesSelectedWriter(
        int statusValue,
        int formatValue,
        int targetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var format = (CliOutputFormat)formatValue;
        var target = (CliOutputTarget)targetValue;
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var output = CliOutputStage.Prepare(new CliRenderedResult(status, format, "primary"));

        var completion = CliOutputStage.Write(
            output,
            new CliOutputWriters(standardOutput, standardError));

        var expectedContent = $"primary{Environment.NewLine}";
        if (target == CliOutputTarget.StandardOutput)
        {
            Assert.Equal(expectedContent, standardOutput.ToString());
            Assert.Equal(string.Empty, standardError.ToString());
        }
        else
        {
            Assert.Equal(string.Empty, standardOutput.ToString());
            Assert.Equal(expectedContent, standardError.ToString());
        }

        Assert.Equal(status, completion.Status);
        Assert.Equal(output.Target, completion.PrimaryOutputTarget);
    }

    [Theory(DisplayName = "CLI output stage rejects malformed output messages without writing either stream"),
     InlineData(0),
     InlineData(1),
     InlineData(2),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void OutputStageRejectsMalformedMessageWithoutWriting(int messageField)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var output = MalformedOutputMessage(messageField);

        Assert.Throws<ArgumentOutOfRangeException>(() => CliOutputStage.Write(
            output,
            new CliOutputWriters(standardOutput, standardError)));

        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Equal(string.Empty, standardError.ToString());
    }

    [Fact(DisplayName = "CLI completion stage returns mapped process completion without terminating"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void CompletionStageReturnsMappedCompletion()
    {
        var output = CliOutputStage.Prepare(
            new CliRenderedResult(CliSemanticStatus.Interrupted, CliOutputFormat.Human, "stopped"));

        var completion = CliCompletionStage.Complete(output);

        Assert.Equal(CliSemanticStatus.Interrupted, completion.Status);
        Assert.Equal(130, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
    }

    [Fact(DisplayName = "CLI completion stage fails closed for an unknown output target"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void CompletionStageRejectsUnknownOutputTarget()
    {
        var output = new CliOutputMessage(
            CliSemanticStatus.Complete,
            CliOutputFormat.Human,
            (CliOutputTarget)int.MaxValue,
            "formed");

        Assert.Throws<ArgumentOutOfRangeException>(() => CliCompletionStage.Complete(output));
    }

    [Fact(DisplayName = "CLI completion stage fails closed for an unknown output format"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public void CompletionStageRejectsUnknownOutputFormat()
    {
        var output = new CliOutputMessage(
            CliSemanticStatus.Complete,
            (CliOutputFormat)int.MaxValue,
            CliOutputTarget.StandardOutput,
            "formed");

        Assert.Throws<ArgumentOutOfRangeException>(() => CliCompletionStage.Complete(output));
    }

    [Theory(DisplayName = "CLI command pipeline runs operation and selected renderer once through completion"),
     InlineData((int)CliOutputFormat.Human, "human:result"),
     InlineData((int)CliOutputFormat.Json, "json:result"),
     Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit")]
    public async Task PipelineRunsClosedStagesOnce(
        int formatValue,
        string expectedContent)
    {
        var format = (CliOutputFormat)formatValue;
        var operationCalls = 0;
        var probe = new RendererProbe();
        var pipeline = new CliCommandPipeline<SampleRequest, SampleResult>(
            (request, cancellationToken) =>
            {
                operationCalls++;
                Assert.Equal("request", request.Value);
                Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
                return Task.FromResult(new CliOperationResult<SampleResult>(
                    CliSemanticStatus.Complete,
                    new SampleResult("result")));
            },
            new CliRendererSet<SampleResult>(probe.RenderHuman, probe.RenderJson));
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var invocation = new CliCommandInvocation<SampleRequest>(
            new SampleRequest("request"),
            Presentation(format));

        var completion = await pipeline.RunAsync(
            invocation,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(1, probe.HumanCalls + probe.JsonCalls);
        Assert.Equal($"{expectedContent}{Environment.NewLine}", standardOutput.ToString());
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.Equal(0, completion.ExitCode);
    }

    private static CliOperationResult<SampleResult> CompleteResult()
    {
        return new CliOperationResult<SampleResult>(
            CliSemanticStatus.Complete,
            new SampleResult("complete"));
    }

    private static CliPresentation MalformedPresentation(int field)
    {
        return field switch
        {
            0 => new CliPresentation((CliOutputFormat)int.MaxValue, CliView.Expanded, CliVerbosity.Normal),
            1 => new CliPresentation(CliOutputFormat.Human, (CliView)int.MaxValue, CliVerbosity.Normal),
            2 => new CliPresentation(CliOutputFormat.Human, CliView.Expanded, (CliVerbosity)int.MaxValue),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "The presentation test field is not defined."),
        };
    }

    private static CliPresentationMessage<SampleResult> MalformedPresentationMessage(int field)
    {
        return field switch
        {
            0 => new CliPresentationMessage<SampleResult>(
                (CliSemanticStatus)int.MaxValue,
                new SampleResult("result"),
                Presentation(CliOutputFormat.Human)),
            1 => new CliPresentationMessage<SampleResult>(
                CliSemanticStatus.Complete,
                new SampleResult("result"),
                MalformedPresentation(0)),
            2 => new CliPresentationMessage<SampleResult>(
                CliSemanticStatus.Complete,
                new SampleResult("result"),
                MalformedPresentation(1)),
            3 => new CliPresentationMessage<SampleResult>(
                CliSemanticStatus.Complete,
                new SampleResult("result"),
                MalformedPresentation(2)),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "The presentation-message test field is not defined."),
        };
    }

    private static CliOutputMessage MalformedOutputMessage(int field)
    {
        return field switch
        {
            0 => new CliOutputMessage(
                (CliSemanticStatus)int.MaxValue,
                CliOutputFormat.Human,
                CliOutputTarget.StandardOutput,
                "formed"),
            1 => new CliOutputMessage(
                CliSemanticStatus.Complete,
                (CliOutputFormat)int.MaxValue,
                CliOutputTarget.StandardOutput,
                "formed"),
            2 => new CliOutputMessage(
                CliSemanticStatus.Complete,
                CliOutputFormat.Human,
                (CliOutputTarget)int.MaxValue,
                "formed"),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "The output-message test field is not defined."),
        };
    }

    private static CliPresentation Presentation(CliOutputFormat format)
    {
        return new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal);
    }

    private sealed record SampleRequest(string Value);

    private sealed record SampleResult(string Value);

    private sealed class RendererProbe
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
            return $"json:{presentation.Result.Value}";
        }
    }
}
