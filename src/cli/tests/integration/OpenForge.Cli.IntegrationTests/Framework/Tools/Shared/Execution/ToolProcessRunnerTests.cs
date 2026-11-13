using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Execution;

[Trait("Feature", "tool-process"), Trait("Evidence", "Integration")]
public sealed class ToolProcessRunnerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Declared lifecycle processes keep argument positions, physical CWD and child-local environment")]
    [InlineData(ToolVocabulary.InstallAction)]
    [InlineData(ToolVocabulary.UninstallAction)]
    [InlineData(ToolVocabulary.IndexCleanupAction)]
    public async Task RunsDeclaredArguments(string action)
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-args");
        var inherited = Environment.GetEnvironmentVariable(ToolCommandProbe.EnvironmentName);
        string[] payload = ["space value", "", "quote'\"value", "line\nvalue"];
        var request = await RequestAsync(temporary, ToolCommandProbe.Success, payload);
        request = request with { Action = action, Environment = new() { [ToolCommandProbe.EnvironmentName] = "" } };
        try
        {
            var outcome = await ToolProcessRunner.RunAsync(request, Token);
            Assert.True(outcome.Succeeded, outcome.Cause);
            Assert.Equal(0, outcome.ExitCode);
            using var witness = JsonDocument.Parse(await File.ReadAllBytesAsync(temporary.Combine(ToolCommandProbe.Witness), Token));
            Assert.Equal(request.WorkingDirectory, witness.RootElement.GetProperty(ToolCommandProbe.CwdKey).GetString());
            Assert.Equal(payload, witness.RootElement.GetProperty(ToolCommandProbe.ArgsKey).EnumerateArray().Select(item => item.GetString()).ToArray());
            Assert.Equal("", witness.RootElement.GetProperty(ToolCommandProbe.EnvironmentKey).GetString());
            Assert.Equal(inherited, Environment.GetEnvironmentVariable(ToolCommandProbe.EnvironmentName));
        }
        finally { Cleanup(temporary); }
    }

    [Fact(DisplayName = "A failed tool-owned command keeps its exact nonzero exit result")]
    public async Task KeepsFailureExit()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-failure");
        try
        {
            var outcome = await ToolProcessRunner.RunAsync(await RequestAsync(temporary, ToolCommandProbe.Failure, []), Token);
            Assert.Equal(ToolCommandState.Exited, outcome.State);
            Assert.Equal(ToolCommandProbe.FailureExit, outcome.ExitCode);
            Assert.False(outcome.Succeeded);
        }
        finally { Cleanup(temporary); }
    }

    [Fact(DisplayName = "Cancelling a started Tool command reaps the owned child before returning")]
    public async Task CancelsOwnedChild()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-cancel");
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var request = await RequestAsync(temporary, ToolCommandProbe.Wait, []);
        var running = ToolProcessRunner.RunAsync(request, cancellation.Token).AsTask();
        try
        {
            using var ready = CancellationTokenSource.CreateLinkedTokenSource(Token);
            ready.CancelAfter(TimeSpan.FromSeconds(15));
            while (!File.Exists(temporary.Combine(ToolCommandProbe.Witness)))
            {
                if (running.IsCompleted) Assert.Fail((await running).Cause);
                await Task.Delay(10, ready.Token);
            }
            using var witness = JsonDocument.Parse(await File.ReadAllBytesAsync(temporary.Combine(ToolCommandProbe.Witness), Token));
            using var child = Process.GetProcessById(witness.RootElement.GetProperty(ToolCommandProbe.PidKey).GetInt32());
            Assert.False(child.HasExited);
            cancellation.Cancel();
            var outcome = await running;
            Assert.Equal(ToolCommandState.Interrupted, outcome.State);
            Assert.True(child.HasExited);
        }
        finally
        {
            cancellation.Cancel();
            await running;
            Cleanup(temporary);
        }
    }

    [Fact(DisplayName = "A timed-out declared command reports failure and stops its owned child")]
    public async Task EnforcesTimeout()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-timeout");
        try
        {
            var request = await RequestAsync(temporary, ToolCommandProbe.Wait, []);
            var outcome = await ToolProcessRunner.RunAsync(request with { TimeoutSeconds = 1 }, Token);
            Assert.Equal(ToolCommandState.Failed, outcome.State);
            Assert.False(outcome.Succeeded);
            Assert.Null(outcome.ExitCode);
        }
        finally { Cleanup(temporary); }
    }

    [Fact(DisplayName = "A changed executable hash blocks launch before the fixture can write")]
    public async Task RefusesChangedExecutable()
    {
        using var temporary = TemporaryWorkspace.Create("tool-process-hash");
        var request = await RequestAsync(temporary, ToolCommandProbe.Success, []);
        var outcome = await ToolProcessRunner.RunAsync(request with { EntrySha256 = new string('0', 64) }, Token);
        Assert.Equal(ToolCommandState.Failed, outcome.State);
        Assert.Empty(temporary.SnapshotHashes());
    }

    private static async Task<ToolProcessRequest> RequestAsync(TemporaryWorkspace temporary, string mode, string[] payload)
    {
        var process = Environment.ProcessPath ?? throw new InvalidOperationException("The test executable is unavailable.");
        var paths = new PhysicalPathResolver();
        var root = paths.ResolveRoot(Path.GetDirectoryName(process) ?? throw new InvalidOperationException("The executable has no parent.")).GetContainedPhysicalPath();
        process = Path.Combine(root, Path.GetFileName(process));
        await using var binary = new FileStream(process, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(binary, Token));
        var prefix = string.Equals(Path.GetFileNameWithoutExtension(process), "dotnet", StringComparison.OrdinalIgnoreCase)
            ? new[] { Path.Combine(AppContext.BaseDirectory, "OpenForge.Cli.IntegrationTests.dll") } : [];
        return new()
        {
            Action = ToolVocabulary.InstallAction,
            Host = null,
            ExecutableRoot = root,
            Executable = process,
            EntrySha256 = hash,
            WorkingDirectory = paths.ResolveRoot(temporary.Path).GetContainedPhysicalPath(),
            Arguments = [.. prefix, ToolCommandProbe.ChildArgument, mode, temporary.Combine(ToolCommandProbe.Witness), .. payload],
            Environment = [],
            TimeoutSeconds = 30,
        };
    }

    private static void Cleanup(TemporaryWorkspace temporary)
    {
        File.Delete(temporary.Combine(ToolCommandProbe.Witness));
        File.Delete(temporary.Combine(ToolCommandProbe.Witness) + ToolCommandProbe.StageSuffix);
    }
}
