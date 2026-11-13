using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;
using OpenForge.Cli.TestSupport.Tools;
using OpenForge.Cli.TestSupport.Tools.Run;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Integration")]
public sealed class ToolRunSignalTests
{
    public enum Stop { CliInput, McpInput, INT, QUIT, TERM, HUP }
    private const string OrdinaryArgument = "--ordinary-cli";
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(20);
    public static TheoryData<Stop> Cases
    {
        get
        {
            TheoryData<Stop> cases = [Stop.CliInput, Stop.McpInput];
            // Windows has no managed console-signal sender. Unix CI owns these four cases.
            if (!OperatingSystem.IsWindows()) { cases.Add(Stop.INT); cases.Add(Stop.QUIT); cases.Add(Stop.TERM); cases.Add(Stop.HUP); }
            return cases;
        }
    }

    [Theory(DisplayName = "Run keeps shared console interrupts, supervises parent-only stops and distinguishes MCP EOF from CLI EOF")]
    [MemberData(nameof(Cases))]
    public async Task SupervisesLifetime(Stop stop)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(Deadline);
        var token = cancellation.Token;
        using var fixture = await ToolRunFixture.CreateAsync(token, executable: true, mcp: true);
        string[] arguments = stop == Stop.CliInput ? [ToolFixtureSource.Id, ToolRunChild.Hold, OrdinaryArgument] : [ToolFixtureSource.Id, ToolRunChild.Hold];
        using var host = ToolRunProbe.Start(fixture, arguments);
        Process? child = null;
        Process? descendant = null;
        try
        {
            var ready = new byte[ToolRunChild.ReadyBytes.Length];
            await host.StandardOutput.BaseStream.ReadExactlyAsync(ready, token);
            Assert.Equal(ToolRunChild.ReadyBytes, ready);
            using var witness = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(fixture.Workspace.LexicalRoot, ToolRunChild.Witness), token));
            child = Process.GetProcessById(witness.RootElement.GetProperty(ToolRunChild.PidKey).GetInt32());
            descendant = Process.GetProcessById(witness.RootElement.GetProperty(ToolRunChild.DescendantKey).GetInt32());
            using var output = new MemoryStream();
            using var error = new MemoryStream();
            var readOutput = host.StandardOutput.BaseStream.CopyToAsync(output, token);
            var readError = host.StandardError.BaseStream.CopyToAsync(error, token);
            var interrupt = stop is Stop.INT or Stop.QUIT;
            var expected = stop switch
            {
                Stop.CliInput => ToolRunChild.ChildExit,
                Stop.INT or Stop.QUIT => ToolRunChild.InterruptExit,
                Stop.TERM => ToolDispatchSignals.TerminateExitCode,
                _ => ToolDispatchSignals.HangupExitCode,
            };
            if (stop is Stop.CliInput or Stop.McpInput)
            {
                host.StandardInput.Close();
                if (stop == Stop.CliInput)
                {
                    // Outwait the stop grace. Ordinary piped EOF must not start it, even for a recorded MCP-capable launch.
                    await Task.Delay(ToolDispatchSignals.StopGracePeriod + TimeSpan.FromMilliseconds(250), token);
                    Assert.False(host.HasExited);
                    fixture.Temporary.WriteBytes(ToolRunFixture.WorkspaceDirectory + "/" + ToolRunChild.Release, []);
                }
            }
            else
            {
                Assert.Equal(await GroupAsync(host.Id, token), await GroupAsync(child.Id, token));
                await SignalAsync(host.Id, stop, token);
                if (interrupt)
                {
                    // Positive PIDs keep the test runner safe while simulating the console's group delivery.
                    await Task.Delay(ToolDispatchSignals.StopGracePeriod, token);
                    Assert.False(host.HasExited);
                    Assert.False(child.HasExited);
                    await SignalAsync(child.Id, stop, token);
                }
            }
            await host.WaitForExitAsync(token);
            await Task.WhenAll(readOutput, readError);
            Assert.Equal(expected, host.ExitCode);
            Assert.Equal(interrupt ? ToolRunChild.InterruptBytes : [], output.ToArray());
            Assert.Empty(error.ToArray());
            await child.WaitForExitAsync(token);
            await descendant.WaitForExitAsync(token);
            Assert.True(child.HasExited);
            Assert.True(descendant.HasExited);
        }
        finally
        {
            foreach (var process in new[] { host, child, descendant })
                if (process is not null && !process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(CancellationToken.None); }
            child?.Dispose();
            descendant?.Dispose();
        }
    }

    private static async Task SignalAsync(int pid, Stop signal, CancellationToken token)
    {
        using var process = Process.Start(new ProcessStartInfo("kill")
        {
            UseShellExecute = false,
            ArgumentList = { "-s", signal.ToString(), pid.ToString(CultureInfo.InvariantCulture) },
        }) ?? throw new InvalidOperationException("The platform's signal sender didn't start.");
        await process.WaitForExitAsync(token);
        Assert.Equal(0, process.ExitCode);
    }

    private static async Task<int> GroupAsync(int pid, CancellationToken token)
    {
        using var process = Process.Start(new ProcessStartInfo("ps")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            ArgumentList = { "-o", "pgid=", "-p", pid.ToString(CultureInfo.InvariantCulture) },
        }) ?? throw new InvalidOperationException("The platform's process reader didn't start.");
        var group = await process.StandardOutput.ReadToEndAsync(token);
        await process.WaitForExitAsync(token);
        Assert.Equal(0, process.ExitCode);
        return int.Parse(group.Trim(), CultureInfo.InvariantCulture);
    }
}
