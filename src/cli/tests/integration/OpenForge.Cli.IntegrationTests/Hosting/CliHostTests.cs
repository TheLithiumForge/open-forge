using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Hosting;

public sealed class CliHostTests
{
    [Theory(DisplayName = "CLI root and root help expose the accepted route command family")]
    [Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RootAndHelpExposeRouteCommandFamily(bool explicitHelp)
    {
        using var workspace = TemporaryWorkspace.Create("host-help");
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        string[] arguments = explicitHelp ? ["--help"] : [];

        var exitCode = await CliHost.RunAsync(
            arguments,
            workspace.Path,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("Open Forge CLI (`open-forge`)", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("Discovery:", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("route list", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("route inspect", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, standardError.ToString());
    }

    [Fact(DisplayName = "CLI version bypasses workspace selection and uses generated version")]
    [Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    public async Task VersionBypassesWorkspaceSelectionAndUsesGeneratedVersion()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var missingWorkspace = Path.Combine(Path.GetTempPath(), $"open-forge-missing-{Guid.NewGuid():N}");

        var exitCode = await CliHost.RunAsync(
            ["--workspace", missingWorkspace, "--version"],
            missingWorkspace,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Equal($"{CliBuildVersion.InformationalVersion}{Environment.NewLine}", standardOutput.ToString());
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "CLI unknown input writes parser diagnostics to stderr")]
    [Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    public async Task UnknownInputProducesParserDiagnosticsOnStandardError()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var exitCode = await CliHost.RunAsync(
            ["--unknown"],
            Path.GetTempPath(),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(4, exitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains("unknown", standardError.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "CLI pre-cancelled host call does not write")]
    [Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    public async Task PreCancelledHostCallDoesNotWrite()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CliHost.RunAsync(
                ["--version"],
                Path.GetTempPath(),
                new CliOutputWriters(standardOutput, standardError),
                cancellation.Token));
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Equal(string.Empty, standardError.ToString());
    }
}
