using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Hosting.Shared.Presentation;

namespace OpenForge.Cli.IntegrationTests.Hosting.Shared.Presentation;

public sealed class CliHostColorPolicyTests
{
    [Theory(DisplayName = "Host color policy keeps redirected streams plain independently"), Trait("Feature", "cli-color"), Trait("Evidence", "Integration")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void IndependentStreams(bool outputRedirected, bool errorRedirected)
    {
        var colors = CliHostColorPolicy.Resolve(terminal: "xterm-256color", noColor: "", isWindows: false, standardOutputRedirected: outputRedirected, standardErrorRedirected: errorRedirected);
        Assert.Equal(!outputRedirected, colors.StandardOutput);
        Assert.Equal(!errorRedirected, colors.StandardError);
    }

    [Theory(DisplayName = "No-color preference and unavailable terminal capability select plain output"), Trait("Feature", "cli-color"), Trait("Evidence", "Integration")]
    [InlineData(null, null, false)]
    [InlineData("", null, false)]
    [InlineData("dumb", null, false)]
    [InlineData("xterm", "1", false)]
    [InlineData("xterm", "0", false)]
    [InlineData("xterm", null, true)]
    public void PlainFallback(string? terminal, string? noColor, bool isWindows)
    {
        Assert.Equal(CliOutputColors.Plain, CliHostColorPolicy.Resolve(terminal: terminal, noColor: noColor, isWindows: isWindows, standardOutputRedirected: false, standardErrorRedirected: false));
    }
}
