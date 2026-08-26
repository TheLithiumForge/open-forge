using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesDiagnosticsAndHelpRedTests
{
    [Theory(DisplayName = "References diagnostics preserve primary status and next action while staying bounded, escaped, and one line"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Attention)]
    [InlineData((int)CliSemanticStatus.Incomplete)]
    [InlineData((int)CliSemanticStatus.Invalid)]
    [InlineData((int)CliSemanticStatus.Blocked)]
    [InlineData((int)CliSemanticStatus.Failed)]
    [InlineData((int)CliSemanticStatus.Interrupted)]
    public void DiagnosticsAreBoundedEscapedAndStatusPreserving(int statusValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var result = ReferencesPresentationTestData.ForStatus(status);
        var diagnostics = ReferencesDiagnosticRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                result,
                CliOutputFormat.Human,
                CliView.Expanded,
                CliVerbosity.Verbose));

        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics!.Length, 1, 4096);
        Assert.DoesNotContain('\n', diagnostics);
        Assert.DoesNotContain('\r', diagnostics);
        Assert.DoesNotContain('\t', diagnostics);
        Assert.Contains(result.Status switch
        {
            CliSemanticStatus.Attention => "attention",
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Invalid => "invalid",
            CliSemanticStatus.Blocked => "blocked",
            CliSemanticStatus.Failed => "failed",
            CliSemanticStatus.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The diagnostic status case is not defined."),
        }, diagnostics, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "References diagnostics escape control-bearing findings without changing the primary typed result"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void DiagnosticsEscapeControlBearingFindings()
    {
        var result = ReferencesPresentationTestData.ControlCharacterResult();
        var diagnostics = ReferencesDiagnosticRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                result,
                CliOutputFormat.Json,
                CliView.Compact,
                CliVerbosity.Verbose));

        Assert.NotNull(diagnostics);
        Assert.Contains("line\\nbreak", diagnostics!, StringComparison.Ordinal);
        Assert.Contains("control\\tcause", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("line\nbreak", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("control\tcause", diagnostics, StringComparison.Ordinal);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal("open-forge doctor", result.Next!.Command);
    }

    [Fact(DisplayName = "References binding-owned help exposes syntax defaults forms examples related commands and result stream policy"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void HelpSectionsExposeCompletePublicContract()
    {
        var help = ReferencesHelpSections.Create();
        var text = string.Join(
            Environment.NewLine,
            help.Sections.Select(section => $"{section.Heading}:\n{section.Body}"));

        Assert.Contains("references <source-reference>", text, StringComparison.Ordinal);
        Assert.Contains("--direction", text, StringComparison.Ordinal);
        Assert.Contains("in|out|both", text, StringComparison.Ordinal);
        Assert.Contains("default: both", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--include", text, StringComparison.Ordinal);
        Assert.Contains("--exclude", text, StringComparison.Ordinal);
        Assert.Contains("--json", text, StringComparison.Ordinal);
        Assert.Contains("--view", text, StringComparison.Ordinal);
        Assert.Contains("--verbose", text, StringComparison.Ordinal);
        Assert.Contains("Examples", text, StringComparison.Ordinal);
        Assert.Contains("Related commands", text, StringComparison.Ordinal);
        Assert.Contains("Results and streams", text, StringComparison.Ordinal);
    }
}
