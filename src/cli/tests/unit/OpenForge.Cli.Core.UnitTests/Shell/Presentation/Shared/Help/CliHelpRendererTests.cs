using System.CommandLine;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Help;

public sealed class CliHelpRendererTests
{
    [Fact(DisplayName = "Help keeps inherited repeatable options without displaying runtime type names")]
    [Trait("Feature", "command-help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void RepeatableOptionsRetainTheirRows()
    {
        var root = new RootCommand();
        root.Add(new Option<string[]>("--allow-path")
        {
            HelpName = "path",
            Recursive = true,
            DefaultValueFactory = _ => [],
        });
        root.Add(new Command("read"));

        var text = CliHelpRenderer.Render(root.Parse(["read"]), CliHelpContent.Empty, 80);

        Assert.Single(text.Split(Environment.NewLine), line => line.Trim() == "--allow-path <path>");
        Assert.DoesNotContain("System.", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Help uses public Usage without changing parser arity or repeating options")]
    [Trait("Feature", "command-help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void PublicUsageAndGeneratedOptions()
    {
        var command = new Command("inspect", "Explain one source.");
        var argument = new Argument<string[]>("source-reference") { Arity = ArgumentArity.ZeroOrMore };
        command.Add(argument);
        command.Add(new Option<bool>("--matching-files") { Description = "List matching files.", Arity = ArgumentArity.Zero });
        var root = new RootCommand();
        root.Add(command);
        var content = CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = "open-forge route inspect <source-reference> [options]",
            Examples = "open-forge route inspect memory\nopen-forge route inspect directives --matching-files",
            Behavior = "Read one source without changing files.",
            Details = [new("Selection", "Use one exact source ID.")],
            RelatedCommands = [("route list", "Browse routed sources.")],
        });

        var text = CliHelpRenderer.Render(root.Parse(["inspect"]), content, 80);

        Assert.Equal(ArgumentArity.ZeroOrMore, argument.Arity);
        Assert.Equal(content.Usage, text.Split(Environment.NewLine).Single(line => line.StartsWith("  open-forge route inspect <", StringComparison.Ordinal)).Trim());
        Assert.Single(text.Split(Environment.NewLine), line => line.StartsWith("  --matching-files ", StringComparison.Ordinal));
        Assert.DoesNotContain("[<source-reference>...]", text, StringComparison.Ordinal);
        Assert.True(text.IndexOf("Examples:", StringComparison.Ordinal) < text.IndexOf("Arguments:", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Help aligns wrapped descriptions and gives long names a deeper fallback"), InlineData(40), InlineData(60), InlineData(80), InlineData(120)]
    [Trait("Feature", "command-help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void TwoColumnRowsAlignDescriptions(int width)
    {
        const string description = "Choose the source carefully and inspect its current content before changing any managed files in this workspace.";
        const string longName = "--an-option-name-that-exceeds-the-name-column";
        var content = new CliHelpContent(
        [
            new CliHelpSection("Related commands", [("read", description), (longName, "Inspect the selected source without changing files.")]),
        ]);

        var text = CliHelpRenderer.Render(new RootCommand().Parse([]), content, width);
        var lines = text[(text.IndexOf("Related commands:", StringComparison.Ordinal) + "Related commands:".Length)..]
            .Trim('\r', '\n').Split(Environment.NewLine);
        var descriptionColumn = lines[0].IndexOf("Choose", StringComparison.Ordinal);
        var longNameIndex = Array.FindIndex(lines, line => line.Trim() == longName);

        Assert.True(descriptionColumn > "  read".Length);
        Assert.All(lines.Skip(1).Take(longNameIndex - 1), line => Assert.Equal(descriptionColumn, line.Length - line.TrimStart().Length));
        Assert.StartsWith("      Inspect", lines[longNameIndex + 1], StringComparison.Ordinal);
        Assert.DoesNotContain(lines, line => line.StartsWith("  Inspect", StringComparison.Ordinal));
        Assert.All(lines.Where(line => !line.Contains(longName, StringComparison.Ordinal)), line => Assert.True(line.Length <= width));
    }

    [Fact(DisplayName = "Leaf help lists local options and summarizes registered global options once")]
    [Trait("Feature", "command-help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void LeafOptionsSummarizeGlobals()
    {
        var root = new RootCommand();
        root.Add(new Option<string>("--workspace") { Recursive = true, Description = "Select a workspace." });
        var leaf = new Command("read");
        leaf.Add(new Option<bool>("--local") { Arity = ArgumentArity.Zero, Description = "Read selected files." });
        root.Add(leaf);
        var content = new CliHelpContent([]) { SummarizeGlobalOptions = true };

        var text = CliHelpRenderer.Render(root.Parse(["read"]), content, 80);

        Assert.Contains("--local", text, StringComparison.Ordinal);
        Assert.Single(text.Split(Environment.NewLine), line => line.StartsWith("Global options:", StringComparison.Ordinal));
        Assert.Contains("See open-forge --help.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--workspace <", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Select a workspace.", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
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

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Help preserves separate examples and indivisible source references"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void ExamplesAndLongReferencesRemainIntact()
    {
        const string reference = ".agents/memory/project-alpha/crystallized/documents/overview.md";
        var command = new RootCommand("Review help.");
        var content = new CliHelpContent(
        [
            new CliHelpSection("Examples", $"open-forge context {reference}\nopen-forge doctor", preserveLines: true),
            new CliHelpSection("Notes", "Read only."),
        ]);

        var help = CliHelpRenderer.Render(command.Parse([]), content, 40);

        Assert.Contains($"  open-forge context {reference}{Environment.NewLine}  open-forge doctor", help, StringComparison.Ordinal);
        Assert.Contains($"open-forge doctor{Environment.NewLine}{Environment.NewLine}Notes:", help, StringComparison.Ordinal);
        Assert.DoesNotContain($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", help, StringComparison.Ordinal);
    }
}
