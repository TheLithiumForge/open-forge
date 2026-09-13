using System.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Rendering;

public sealed class CliHumanStyleTests
{
    [Theory(DisplayName = "Status accents follow the actual primary stream and JSON always stays plain"), Trait("Feature", "cli-color"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "32", true)]
    [InlineData((int)CliSemanticStatus.Attention, "33", true)]
    [InlineData((int)CliSemanticStatus.Incomplete, "33", true)]
    [InlineData((int)CliSemanticStatus.Invalid, "31", false)]
    [InlineData((int)CliSemanticStatus.Blocked, "31", false)]
    [InlineData((int)CliSemanticStatus.Failed, "31", false)]
    [InlineData((int)CliSemanticStatus.Interrupted, "33", false)]
    public void StatusAndStream(int statusValue, string code, bool usesStandardOutput)
    {
        var status = (CliSemanticStatus)statusValue;
        var result = new CliStyleTestResult(status);
        var plain = new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal);
        var enabled = plain with { Colors = new CliOutputColors(StandardOutput: usesStandardOutput, StandardError: !usesStandardOutput) };
        var disabled = plain with { Colors = new CliOutputColors(StandardOutput: !usesStandardOutput, StandardError: usesStandardOutput) };
        Assert.Equal($"\u001b[{code}mlabel\u001b[39m", CliHumanStyle.For(new CliPresentationRequest<CliStyleTestResult>(result, enabled)).Status("label", status));
        Assert.Equal("label", CliHumanStyle.For(new CliPresentationRequest<CliStyleTestResult>(result, disabled)).Status("label", status));
        Assert.Equal("label", CliHumanStyle.For(new CliPresentationRequest<CliStyleTestResult>(result, plain)).Status("label", status));
        foreach (var view in new[] { CliView.Compact, CliView.Expanded })
        {
            var json = enabled with { Format = CliOutputFormat.Json, View = view, Colors = new CliOutputColors(StandardOutput: true, StandardError: true) };
            Assert.Equal("label", CliHumanStyle.For(new CliPresentationRequest<CliStyleTestResult>(result, json)).Status("label", status));
        }
    }

    [Fact(DisplayName = "Each label restores the default foreground before following text"), Trait("Feature", "cli-color"), Trait("Evidence", "Unit")]
    public void LabelsDoNotColorFollowingContent()
    {
        var style = CliHumanStyle.Color;
        Assert.Equal("\u001b[36mContext\u001b[39m body", style.Information("Context") + " body");
        Assert.Equal("\u001b[33mWARNING\u001b[39m path", style.Warning("WARNING") + " path");
        Assert.Equal("\u001b[31mERROR\u001b[39m cause", style.Error("ERROR") + " cause");
        Assert.Equal("Context", CliHumanStyle.Plain.Information("Context"));
        var presentation = new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)
        {
            Colors = new CliOutputColors(StandardOutput: true, StandardError: false),
        };
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, new CliPresentationRequest<CliStyleTestResult>(new CliStyleTestResult(CliSemanticStatus.Complete), presentation), "Direct links for", "docs/guide");
        Assert.StartsWith("\u001b[36mDirect links for\u001b[39m docs/guide", builder.ToString(), StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => style.Status("label", (CliSemanticStatus)999));
    }
}
