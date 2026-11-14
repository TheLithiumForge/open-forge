using System.Text;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools;

[Trait("Feature", "tool-process"), Trait("Evidence", "Integration")]
public sealed class ToolProcessHostTests
{
    private const string ProbeVariable = "OPENFORGE_PROCESS_TEST_VALUE";

    [Fact(DisplayName = "Tool processes keep argv separate and use the supplied working directory and environment")]
    public async Task PassesInvocation()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process");
        var payload = "spaces % & ! ' unicode-λ";
        var request = Request(temporary, OperatingSystem.IsWindows() ? $"[Console]::Write($env:{ProbeVariable})" : $"printf '%s' \"${ProbeVariable}\"")
            with
        { Environment = new Dictionary<string, string?> { [ProbeVariable] = payload } };
        var result = await new ToolProcessHost().RunAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(payload, result.StandardOutput);
        Assert.Null(result.Failure);
        Assert.False(result.Cancelled);
        Assert.False(result.TimedOut);
    }

    [Fact(DisplayName = "Tool process output is capped across both streams")]
    public async Task BoundsOutput()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process");
        var script = OperatingSystem.IsWindows() ? "while ($true) { [Console]::Write('0123456789'); [Console]::Error.Write('0123456789') }"
            : "while :; do printf '0123456789'; printf '0123456789' >&2; done";
        var result = await new ToolProcessHost().RunAsync(Request(temporary, script) with { MaximumOutputBytes = 1000 }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.InRange(Encoding.UTF8.GetByteCount(result.StandardOutput) + Encoding.UTF8.GetByteCount(result.StandardError), 0, 1000);
    }

    [Fact(DisplayName = "Tool process timeout reaps the child")]
    public async Task TimesOut()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process");
        var result = await new ToolProcessHost().RunAsync(Request(temporary, Sleep()) with { Timeout = TimeSpan.FromMilliseconds(100) }, TestContext.Current.CancellationToken);
        Assert.True(result.TimedOut);
        Assert.False(result.Cancelled);
        Assert.NotNull(result.ExitCode);
    }

    [Fact(DisplayName = "Tool process cancellation stays distinct from timeout")]
    public async Task Cancels()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process");
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancelled.CancelAfter(TimeSpan.FromMilliseconds(100));
        var result = await new ToolProcessHost().RunAsync(Request(temporary, Sleep()), cancelled.Token);
        Assert.True(result.Cancelled);
        Assert.False(result.TimedOut);
    }

    [Fact(DisplayName = "Missing tool executables return a bounded process failure")]
    public async Task ReportsMissingExecutable()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process");
        var result = await new ToolProcessHost().RunAsync(Request(temporary, "") with { ExecutablePath = temporary.Combine("missing") }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Null(result.ExitCode);
    }

    private static string Sleep() => OperatingSystem.IsWindows() ? "Start-Sleep -Seconds 30" : "sleep 30";

    private static ToolProcessRequest Request(TemporaryWorkspace temporary, string script)
    {
        var executable = OperatingSystem.IsWindows() ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe")
            : System.IO.Path.Combine(System.IO.Path.GetPathRoot(AppContext.BaseDirectory) ?? throw new InvalidOperationException(), "bin", "sh");
        IReadOnlyList<string> arguments = OperatingSystem.IsWindows()
            ? ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command", $"[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false); {script}"] : ["-c", script];
        return new()
        {
            ExecutablePath = executable,
            Arguments = arguments,
            WorkingDirectory = temporary.Path,
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(10),
            MaximumOutputBytes = 10000,
        };
    }
}
