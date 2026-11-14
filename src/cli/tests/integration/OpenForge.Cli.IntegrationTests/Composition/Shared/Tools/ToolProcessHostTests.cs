using System.Text;
using System.Diagnostics;
using System.Globalization;
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
        var working = temporary.CreateDirectory("working");
        var script = OperatingSystem.IsWindows() ? "while ($true) { [Console]::Write('0123456789'); [Console]::Error.Write('0123456789') }"
            : "while :; do printf '0123456789'; printf '0123456789' >&2; done";
        var result = await new ToolProcessHost().RunAsync(Request(temporary, Probe(temporary, "root", script))
            with
        { MaximumOutputBytes = 1000, WorkingDirectory = working }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.InRange(Encoding.UTF8.GetByteCount(result.StandardOutput) + Encoding.UTF8.GetByteCount(result.StandardError), 0, 1000);
        AssertStopped(temporary, "root");
        Directory.Move(working, temporary.Combine("released"));
        Directory.Move(temporary.Combine("released"), working);
    }

    [Theory(DisplayName = "Tool termination releases root and child files and the working directory before returning")]
    [InlineData("output"), InlineData("timeout"), InlineData("cancellation")]
    public async Task ReapsTree(string reason)
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-tree");
        var working = temporary.CreateDirectory("working");
        var body = reason == "output" ? "while ($true) { [Console]::Write('0123456789') }" : Sleep();
        if (!OperatingSystem.IsWindows() && reason == "output") body = "while :; do printf '0123456789'; done";
        var child = Probe(temporary, "child", body);
        var executable = Request(temporary, "").ExecutablePath;
        string script;
        if (OperatingSystem.IsWindows())
        {
            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(child));
            script = $"""
                $start=[Diagnostics.ProcessStartInfo]::new({Quote(executable)})
                $start.UseShellExecute=$false
                $start.CreateNoWindow=$true
                $start.RedirectStandardOutput=$true
                $start.RedirectStandardError=$true
                $start.WorkingDirectory={Quote(working)}
                $start.Arguments='-NoLogo -NoProfile -NonInteractive -EncodedCommand {encoded}'
                $child=[Diagnostics.Process]::Start($start)
                $copyOutput=$child.StandardOutput.BaseStream.CopyToAsync([Console]::OpenStandardOutput())
                $copyError=$child.StandardError.BaseStream.CopyToAsync([Console]::OpenStandardError())
                $child.WaitForExit()
                $copyOutput.GetAwaiter().GetResult()
                $copyError.GetAwaiter().GetResult()
                """;
        }
        else script = $"{Quote(executable)} -c {Quote(child)} & wait $!";
        var request = Request(temporary, Probe(temporary, "root", script)) with { WorkingDirectory = working, MaximumOutputBytes = 1000 };
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var watcher = new FileSystemWatcher(temporary.Path, "child.pid");
        watcher.Created += (_, _) => ready.TrySetResult();
        watcher.Renamed += (_, _) => ready.TrySetResult();
        watcher.EnableRaisingEvents = true;
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var running = new ToolProcessHost().RunAsync(request, cancelled.Token);
        ToolProcessResult result;
        try
        {
            await ready.Task.WaitAsync(request.Timeout, TestContext.Current.CancellationToken);
            if (reason == "cancellation") await cancelled.CancelAsync();
            result = await running;
        }
        finally
        {
            await cancelled.CancelAsync();
            await ((Task)running).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
        }
        Assert.NotNull(result.ExitCode);
        Assert.Equal(reason == "timeout", result.TimedOut);
        Assert.Equal(reason == "cancellation", result.Cancelled);
        if (reason == "output") Assert.NotNull(result.Failure);
        else Assert.Null(result.Failure);
        Assert.InRange(Encoding.UTF8.GetByteCount(result.StandardOutput) + Encoding.UTF8.GetByteCount(result.StandardError), 0, request.MaximumOutputBytes);
        AssertStopped(temporary, "root");
        AssertStopped(temporary, "child");
        Directory.Move(working, temporary.Combine("released"));
        Directory.Move(temporary.Combine("released"), working);
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

    private static string Probe(TemporaryWorkspace temporary, string name, string body)
    {
        temporary.WriteText($"{name}.lease", "");
        temporary.WriteText($"{name}.pid.pending", "");
        temporary.WriteText($"{name}.pid", "");
        File.Delete(temporary.Combine($"{name}.pid"));
        var lease = Quote(temporary.Combine($"{name}.lease"));
        var pending = Quote(temporary.Combine($"{name}.pid.pending"));
        var ready = Quote(temporary.Combine($"{name}.pid"));
        if (OperatingSystem.IsWindows())
            return $"""
                $lease=[IO.File]::Open({lease},[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
                [IO.File]::WriteAllText({pending},[string]$PID)
                [IO.File]::Move({pending},{ready})
                {body}
                """;
        return $"exec 9>{lease}; printf '%s' \"$$\" >{pending}; mv {pending} {ready}; {body}";
    }

    private static string Quote(string value) => OperatingSystem.IsWindows() ? $"'{value.Replace("'", "''", StringComparison.Ordinal)}'"
        : $"'{value.Replace("'", "'\\''", StringComparison.Ordinal)}'";

    private static void AssertStopped(TemporaryWorkspace temporary, string name)
    {
        var pid = int.Parse(File.ReadAllText(temporary.Combine($"{name}.pid")), CultureInfo.InvariantCulture);
        Process? process;
        try { process = Process.GetProcessById(pid); }
        catch (ArgumentException) { process = null; }
        using (process) if (process is not null) Assert.True(process.WaitForExit(TimeSpan.Zero));
        using var lease = File.Open(temporary.Combine($"{name}.lease"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

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
