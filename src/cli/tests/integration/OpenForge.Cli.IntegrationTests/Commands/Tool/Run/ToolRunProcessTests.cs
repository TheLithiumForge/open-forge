using System.Diagnostics;
using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.TestSupport.Tools;

using OpenForge.Cli.TestSupport.Tools.Run;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Integration")]
public sealed class ToolRunProcessTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private static readonly byte[] Input = [0, 255, 1, 128, 13, 10, 127];

    [Theory(DisplayName = "Run forwards raw stdin stdout stderr, argument boundaries, help, environment and exact child exit")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ForwardsChildProcess(bool zeroExit)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token, executable: true);
        string[] arguments = [CliSyntaxDefinitions.Help.Name, "space value", "", "quote\"value", "line\nvalue", "--", CliSyntaxDefinitions.Workspace.Name, "child-only"];
        if (zeroExit) arguments = [.. arguments, ToolRunChild.ExitZero];
        var result = await RunAsync(fixture, [ToolFixtureSource.Id, "--", .. arguments], Input);
        Assert.Equal(zeroExit ? 0 : ToolRunChild.ChildExit, result.ExitCode);
        Assert.Equal(Input, result.Output);
        Assert.Equal(ToolRunChild.ErrorBytes, result.Error);
        var witness = Path.Combine(fixture.Workspace.LexicalRoot, ToolRunChild.Witness);
        using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(witness, Token));
        Assert.Equal(arguments, document.RootElement.GetProperty(ToolRunChild.ArgsKey).EnumerateArray().Select(item => item.GetString()).ToArray());
        Assert.Equal(fixture.Cwd, document.RootElement.GetProperty(ToolRunChild.CwdKey).GetString());
        Assert.Equal(ToolRunChild.EnvironmentValue, document.RootElement.GetProperty(ToolRunChild.EnvironmentKey).GetString());
    }

    [Fact(DisplayName = "Non-interactive missing Tool Run writes only a raw stderr refusal and exact Install next step")]
    public async Task ReportsMissingWithoutInstall()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await fixture.RewriteRegistryAsync([], Token);
        var before = fixture.Temporary.SnapshotHashes();
        var result = await RunAsync(fixture, [ToolFixtureSource.Id], []);
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.Output);
        CommandOutputSnapshot.MatchSnapshot(System.Text.Encoding.UTF8.GetString(result.Error), "missing-tool.stderr");
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "A verified file that the OS cannot execute returns the child-start failure exit without stdout")]
    public async Task ReportsStartFailure()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var result = await RunAsync(fixture, [ToolFixtureSource.Id], []);
        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
    }

    [Theory(DisplayName = "Run pre-launch refusals keep stdout empty, return their exact exit and never create a child witness")]
    [InlineData(0, 4)]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 5)]
    [InlineData(4, 3)]
    public async Task RefusesWithoutChild(int scenario, int exit)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var id = ToolFixtureSource.Id;
        if (scenario == 0) id = "unknown-tool";
        else if (scenario == 1) await fixture.RewriteRegistryAsync([], Token);
        else if (scenario == 2) fixture.WriteSettings(new() { Version = "2.0.0", Roles = [] });
        else if (scenario == 3) File.Delete(fixture.Store.Installs.At(fixture.UnitPath + "/" + ToolRunFixture.DataMember));
        else File.Delete(Path.Combine(fixture.Workspace.LexicalRoot, WorkspaceOwnershipDefinitions.RelativePath));
        var before = fixture.Temporary.SnapshotHashes();
        var result = await RunAsync(fixture, [id], []);
        Assert.Equal(exit, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
        Assert.False(File.Exists(Path.Combine(fixture.Workspace.LexicalRoot, ToolRunChild.Witness)));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    private static async Task<(int ExitCode, byte[] Output, byte[] Error)> RunAsync(ToolRunFixture fixture, string[] arguments, byte[] input)
    {
        using var process = ToolRunProbe.Start(fixture, arguments);
        try
        {
            using var output = new MemoryStream();
            using var error = new MemoryStream();
            var readOutput = process.StandardOutput.BaseStream.CopyToAsync(output, Token);
            var readError = process.StandardError.BaseStream.CopyToAsync(error, Token);
            if (input.Length != 0) await process.StandardInput.BaseStream.WriteAsync(input, Token);
            process.StandardInput.Close();
            await process.WaitForExitAsync(Token);
            await Task.WhenAll(readOutput, readError);
            return (process.ExitCode, output.ToArray(), error.ToArray());
        }
        finally
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(CancellationToken.None); }
        }
    }
}
