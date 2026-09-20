using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliPlanConfirmationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Plan confirmation renders the existing minimal report before one confirmation"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PlanConfirmationUsesConcretePreviewAndPreservesAuthoredBytes()
    {
        var scripted = ScriptedCliTerminal.Lines(["yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var selectorCalls = 0;
        var observedSelection = (CliSelection?)null;
        var rendering = new CliReportRendering<CliStyleTestResult, PlanData>
        {
            Selector = (result, selection) =>
            {
                selectorCalls++;
                observedSelection = selection;
                return new CliReport<PlanData>
                {
                    Command = result.Command,
                    Status = result.Status,
                    Headline = new CliHeadline("Would apply these changes.", CliHeadlineKind.Preview),
                    Data = new PlanData("preview"),
                };
            },
            DataTextRenderer = (_, _, _) => new CliTextDocument([
                CliTextSpan.FromAuthored(new CliAuthoredSpan("raw\r\n\u001b")),
            ]),
            DataJsonTypeInfo = PlanJsonContext.Relaxed.PlanData,
            Shape = CliCommandShape.ChangeReport,
        };
        var preview = new CliStyleTestResult(CliSemanticStatus.Complete);
        var confirm = prompts.PlanConfirmation(rendering, static (string _) => new CliConfirmQuestion("Apply? [y/N]"));

        var reply = await confirm(preview, "question-facts", new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.True(reply.Value);
        Assert.Equal(1, selectorCalls);
        var selection = Assert.IsType<CliSelection>(observedSelection);
        Assert.Equal(CliDetail.Minimal, selection.Detail);
        Assert.Null(selection.Filter);
        var output = scripted.Output.ToString();
        Assert.Contains("raw\r\n\u001b", output, StringComparison.Ordinal);
        Assert.Contains("Apply? [y/N]", output, StringComparison.Ordinal);
        var previewPosition = output.IndexOf("Would apply these changes.", StringComparison.Ordinal);
        var questionPosition = output.IndexOf("Apply? [y/N]", StringComparison.Ordinal);
        Assert.True(previewPosition >= 0 && previewPosition < questionPosition);
        Assert.Equal(1, output.Split("Apply? [y/N]", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, scripted.LineReadCalls);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Plan confirmation policy and capability guards perform no report or prompt IO"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PlanConfirmationChecksAvailabilityBeforeRendering()
    {
        var policyScript = ScriptedCliTerminal.Lines([]);
        var policySelections = 0;
        var policyPrompts = new CliPrompts(policyScript.Terminal);
        var policyConfirmation = policyPrompts.PlanConfirmation(
            CreateRendering(() => policySelections++),
            static (string _) => new CliConfirmQuestion("Apply? [y/N]"));

        var policyReply = await policyConfirmation(
            new CliStyleTestResult(CliSemanticStatus.Complete),
            "facts",
            new CliPromptPolicy(Allowed: false),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Unavailable, policyReply.State);
        Assert.Equal(0, policySelections);
        Assert.Equal(0, policyScript.WriteCalls);
        Assert.Equal(0, policyScript.LineReadCalls);

        var capabilityScript = ScriptedCliTerminal.Lines([], canPrompt: false);
        var capabilitySelections = 0;
        var capabilityPrompts = new CliPrompts(capabilityScript.Terminal);
        var capabilityConfirmation = capabilityPrompts.PlanConfirmation(
            CreateRendering(() => capabilitySelections++),
            static (string _) => new CliConfirmQuestion("Apply? [y/N]"));

        var capabilityReply = await capabilityConfirmation(
            new CliStyleTestResult(CliSemanticStatus.Complete),
            "facts",
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Unavailable, capabilityReply.State);
        Assert.Equal(0, capabilitySelections);
        Assert.Equal(0, capabilityScript.WriteCalls);
        Assert.Equal(0, capabilityScript.LineReadCalls);
    }

    private static CliReportRendering<CliStyleTestResult, PlanData> CreateRendering(Action selected)
        => new()
        {
            Selector = (result, _) =>
            {
                selected();
                return new CliReport<PlanData>
                {
                    Command = result.Command,
                    Status = result.Status,
                    Headline = new CliHeadline("Would apply these changes.", CliHeadlineKind.Preview),
                    Data = new PlanData("preview"),
                };
            },
            DataTextRenderer = (_, _, _) => new CliTextDocument([]),
            DataJsonTypeInfo = PlanJsonContext.Relaxed.PlanData,
            Shape = CliCommandShape.ChangeReport,
        };

}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PlanData))]
internal sealed partial class PlanJsonContext : JsonSerializerContext
{
    internal static PlanJsonContext Relaxed { get; } = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
}
