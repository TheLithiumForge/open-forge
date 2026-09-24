using OpenForge.Cli.TestSupport.Isolation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Hosting;

public sealed class CliHostTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "CLI root and root help expose the accepted command surface")]
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
        Assert.Contains("Inspect and maintain an Open Forge workspace.", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("Getting started:", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("route list", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("open-forge route --help", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("remove <target>", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("--format", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("--detail", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("--detail-filter", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, standardError.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "CLI version bypasses workspace selection and uses generated version")]
    [Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    public async Task VersionBypassesWorkspaceSelectionAndUsesGeneratedVersion()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();
        var missingWorkspace = TestDataHome.AbsentPath("missing-workspace");

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

    [Trait("Boundary", "Host")]
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

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "CLI host rejects retired flags before command execution"), Trait("Feature", "cli-host"), Trait("Evidence", "Integration")]
    [InlineData("--json")]
    [InlineData("--verbose")]
    [InlineData("--view=compact")]
    public async Task RetiredFlagsProduceParserDiagnostics(string flag)
    {
        using var workspace = TemporaryWorkspace.Create("host-retired-flags");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await CliHost.RunAsync(["index", flag], workspace.Path, new CliOutputWriters(output, error), TestContext.Current.CancellationToken);
        Assert.Equal(4, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.NotEmpty(error.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
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
