using System.Diagnostics;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Launchers;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Launchers;

[Trait("Feature", "tool-launchers"), Trait("Evidence", "Integration")]
public sealed class ToolLauncherWriterTests
{
    [Fact(DisplayName = "Direct launchers run without Open Forge and preserve argument boundaries")]
    public async Task RunsDirectLauncher()
    {
        using var temporary = TemporaryWorkspace.Create("tool-launcher");
        var windows = OperatingSystem.IsWindows();
        var folder = temporary.CreateDirectory("space and bang! percent% quote'");
        var script = System.IO.Path.Combine(folder, windows ? "entry.ps1" : "entry.sh");
        temporary.CreateFile(System.IO.Path.GetRelativePath(temporary.Path, script));
        var text = windows ? "[Console]::Write((ConvertTo-Json -Compress -InputObject @($args)))" : "printf '%s\\000' \"$@\"";
        await File.WriteAllTextAsync(script, text, TestContext.Current.CancellationToken);
        var executable = windows ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe")
            : System.IO.Path.Combine(System.IO.Path.GetPathRoot(AppContext.BaseDirectory) ?? throw new InvalidOperationException(), "bin", "sh");
        string[] fixedArguments = windows ? ["-NoLogo", "-NoProfile", "-NonInteractive", "-File", script, "fixed value"] : [script, "fixed value"];
        var launcher = new ToolLauncherSpec
        {
            ToolId = "sample",
            Kind = windows ? ToolLauncherKind.WindowsCmd : ToolLauncherKind.PosixSh,
            TargetKind = ToolLauncherTargetKind.ReleaseLauncher,
            EntryPath = executable,
            LauncherPath = System.IO.Path.Combine(folder, windows ? "sample.cmd" : "sample"),
            Arguments = fixedArguments,
        };
        temporary.CreateFile(System.IO.Path.GetRelativePath(temporary.Path, launcher.LauncherPath));
        File.Delete(launcher.LauncherPath);
        var pending = new List<ToolBackupCompletionRequest>();
        var result = await new ToolLauncherWriter(new Backups()).WriteAsync(new([launcher]), pending, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.True(Assert.Single(pending).Verified);
        var start = new ProcessStartInfo(windows ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe") : launcher.LauncherPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = temporary.Path,
        };
        string[] arguments = ["two words", "amp&value", "bang!value", "last\\"];
        if (windows)
        {
            var forwarded = string.Join(' ', arguments.Select(value => value.EndsWith('\\') ? value : $"\"{value}\""));
            start.Arguments = $"/d /v:off /s /c \"\"{launcher.LauncherPath}\" {forwarded}\"";
        }
        else foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var error = process.StandardError.ReadToEndAsync(timeout.Token);
        try { await process.WaitForExitAsync(timeout.Token); }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
        }
        Assert.True(process.ExitCode == 0, await error);
        var actual = await output;
        Assert.Empty(await error);
        if (windows)
        {
            using var json = JsonDocument.Parse(actual);
            Assert.Equal(new[] { "fixed value" }.Concat(arguments), json.RootElement.EnumerateArray().Select(value => value.GetString()));
        }
        else Assert.Equal(string.Join('\0', new[] { "fixed value" }.Concat(arguments)) + '\0', actual);
        if (!OperatingSystem.IsWindows()) Assert.True(File.GetUnixFileMode(launcher.LauncherPath).HasFlag(UnixFileMode.UserExecute));
    }

    [Fact(DisplayName = "Native binary publication replaces bytes and verifies each replacement")]
    public async Task RewritesNativeBinary()
    {
        using var temporary = TemporaryWorkspace.Create("tool-launcher");
        var source = temporary.CreateFile("native-source", new byte[] { 1, 2, 3 });
        var target = temporary.CreateFile(System.IO.Path.Combine("bin", "sample"));
        File.Delete(target);
        var launcher = new ToolLauncherSpec
        {
            ToolId = "sample",
            Kind = ToolLauncherKind.NativeExecutable,
            TargetKind = ToolLauncherTargetKind.NativeBinary,
            EntryPath = source,
            LauncherPath = target,
            Arguments = [],
        };
        var writer = new ToolLauncherWriter(new Backups());
        Assert.Null((await writer.WriteAsync(new([launcher]), [], TestContext.Current.CancellationToken)).Failure);
        byte[] changed = [4, 5, 6];
        await File.WriteAllBytesAsync(source, changed, TestContext.Current.CancellationToken);
        Assert.Null((await writer.WriteAsync(new([launcher]), [], TestContext.Current.CancellationToken)).Failure);
        Assert.Equal(changed, await File.ReadAllBytesAsync(target, TestContext.Current.CancellationToken));
    }

    private sealed class Backups : IToolBackupService
    {
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence });
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new ToolBackupCompletionResult(true, null, null));
    }
}
