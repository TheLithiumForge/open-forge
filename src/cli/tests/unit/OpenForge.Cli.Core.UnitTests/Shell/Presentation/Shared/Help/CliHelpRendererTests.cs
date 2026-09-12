using System.CommandLine;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Help;

public sealed class CliHelpRendererTests
{
    [Theory(DisplayName = "Help wraps product sections to the supplied width without losing text"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    [InlineData(40)]
    [InlineData(60)]
    [InlineData(80)]
    [InlineData(120)]
    public void ProductSectionsWrapAtTheSuppliedWidth(int width)
    {
        const string paragraph = "Preview the complete plan without writing files. Existing files remain protected and any blocked changes are reported.";
        var command = new RootCommand("Review help.");
        var content = new CliHelpContent([new CliHelpSection("Write policy", paragraph)]);

        var help = CliHelpRenderer.Render(command.Parse([]), content, width);
        var body = help[(help.IndexOf("Write policy:", StringComparison.Ordinal) + "Write policy:".Length)..].Trim('\r', '\n');
        var lines = body.Split(Environment.NewLine);

        Assert.All(lines, line =>
        {
            Assert.StartsWith("  ", line, StringComparison.Ordinal);
            Assert.True(line.Length <= width, $"Line exceeds {width} columns: {line}");
        });
        Assert.Equal(paragraph, string.Join(" ", lines.Select(line => line.Trim())));
    }

    [Fact(DisplayName = "Help preserves separate examples and indivisible source references"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void ExamplesAndLongReferencesRemainIntact()
    {
        const string reference = ".agents/memory/project-alpha/crystallized/documents/overview.md";
        var command = new RootCommand("Review help.");
        var content = new CliHelpContent(
        [
            new CliHelpSection("Examples", $"open-forge context {reference}\nopen-forge doctor"),
            new CliHelpSection("Notes", "Read only."),
        ]);

        var help = CliHelpRenderer.Render(command.Parse([]), content, 40);

        Assert.Contains($"  {reference}{Environment.NewLine}  open-forge doctor", help, StringComparison.Ordinal);
        Assert.Contains($"open-forge doctor{Environment.NewLine}{Environment.NewLine}Notes:", help, StringComparison.Ordinal);
        Assert.DoesNotContain($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", help, StringComparison.Ordinal);
    }
}
