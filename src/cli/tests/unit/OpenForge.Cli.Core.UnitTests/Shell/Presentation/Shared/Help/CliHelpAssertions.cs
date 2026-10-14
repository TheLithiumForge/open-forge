using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Help;

internal static class CliHelpAssertions
{
    internal static void PublicTemplate(CliHelpContent help, string usage)
    {
        Assert.Equal(usage, help.Usage);
        Assert.Equal(["Examples", "Behavior"], help.Introduction.Select(section => section.Heading));
        Assert.Equal(["Details", "Automation", "Related commands"], help.Sections.Select(section => section.Heading));
        var examples = help.Introduction[0];
        Assert.True(examples.PreserveLines);
        var lines = examples.Body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.InRange(lines.Length, 2, 3);
        Assert.All(lines, line => Assert.StartsWith("open-forge ", line.Trim(), StringComparison.Ordinal));
        Assert.All(help.Sections[0].Children, section => Assert.NotEmpty(section.Body));
        Assert.NotEmpty(help.Sections[^1].Rows);
        Assert.True(help.SummarizeGlobalOptions);
        Assert.False(help.Introduction[1].PreserveLines);
    }
}
