using System.Text.Json;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListApplicationIntegrationTests
{
    [Fact(DisplayName = "Route list invalid control-containing exact path emits safe structured source-path-invalid result"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task InvalidExactPathJsonIsSafe()
    {
        const string source = ".agents/\0invalid.md";
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var writers = new CliOutputWriters(standardOutput, standardError);

        var exitCode = await CliApplication.RunAsync(
            ["route", "list", source, "--json"],
            writers,
            TestContext.Current.CancellationToken);

        Assert.Equal(4, exitCode);
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.DoesNotContain("\0", standardOutput.ToString(), StringComparison.Ordinal);
        using var document = JsonDocument.Parse(standardOutput.ToString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal(RouteListFindingCodes.SourcePathInvalid, finding.GetProperty("code").GetString());
        Assert.Equal(source, finding.GetProperty("path").GetString());
    }
}
