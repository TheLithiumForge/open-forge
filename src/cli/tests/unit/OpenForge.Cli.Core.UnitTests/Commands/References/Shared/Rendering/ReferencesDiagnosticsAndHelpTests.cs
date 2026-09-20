using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Presentation.References;
using OpenForge.Cli.Core.Presentation.References.Shared.Help;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesDiagnosticsAndHelpTests
{
    [Trait("Boundary", "Output")]
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
        var diagnostics = CliRenderingStage.Render(
            ReferencesPresentationTestData.Presentation(
                result,
                CliFormat.Text,
                CliDetail.Standard, CliDetail.Debug), ReferencesPresentation.Rendering).DiagnosticContent;

        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics!.Length, 1, 4096);
        Assert.All(diagnostics.Split('\n'), line => Assert.InRange(line.Length, 1, 240));
        Assert.DoesNotContain('\r', diagnostics);
        Assert.DoesNotContain('\t', diagnostics);
        Assert.Contains(result.Status switch
        {
            CliSemanticStatus.Attention => "completed-with-warnings",
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Invalid => "invalid-input",
            CliSemanticStatus.Blocked => "blocked",
            CliSemanticStatus.Failed => "failed",
            CliSemanticStatus.Interrupted => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The diagnostic status case is not defined."),
        }, diagnostics, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References diagnostics escape control-bearing findings without changing the primary typed result"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void DiagnosticsEscapeControlBearingFindings()
    {
        var result = ReferencesPresentationTestData.ControlCharacterResult();
        var rendered = CliRenderingStage.Render(
            ReferencesPresentationTestData.Presentation(
                result,
                CliFormat.Json,
                CliDetail.Minimal, CliDetail.Debug), ReferencesPresentation.Rendering);

        // Diagnostics carry each finding's cause, so the control character in the cause is the
        // one that must appear escaped there.
        var diagnostics = rendered.DiagnosticContent;
        Assert.NotNull(diagnostics);
        Assert.Contains("control\\tcause", diagnostics!, StringComparison.Ordinal);
        Assert.DoesNotContain("control\tcause", diagnostics, StringComparison.Ordinal);

        // The authored subject reaches the primary document, and it is escaped there.
        Assert.Contains("line\\nbreak", rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("line\nbreak", rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal("open-forge doctor", result.Next!.Command);
    }

    [Trait("Boundary", "Output")]
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
        Assert.Contains("--format", text, StringComparison.Ordinal);
        Assert.Contains("--detail", text, StringComparison.Ordinal);
        Assert.Contains("--detail-filter", text, StringComparison.Ordinal);
        Assert.Contains("Examples", text, StringComparison.Ordinal);
        Assert.Contains("Related commands", text, StringComparison.Ordinal);
        Assert.Contains("Results and streams", text, StringComparison.Ordinal);
    }
}
