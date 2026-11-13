using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal static class ToolProcessRunner
{
    internal static async ValueTask<ToolProcessOutcome> RunAsync(ToolProcessRequest request, CancellationToken token)
    {
        if (!ToolVocabulary.LifecycleActions.Contains(request.Action) || !Path.IsPathFullyQualified(request.Executable)
            || !Path.IsPathFullyQualified(request.ExecutableRoot) || !Path.IsPathFullyQualified(request.WorkingDirectory)
            || !ToolIdentity.IsSha256(request.EntrySha256) || request.TimeoutSeconds is <= 0 or > ToolVocabulary.MaximumCommandTimeoutSeconds)
            return new(ToolCommandState.Failed, null, "The declared process request is invalid.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds));
        var start = new ProcessStartInfo(request.Executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = request.WorkingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        foreach (var (key, value) in request.Environment) start.Environment[key] = value;
        using var process = new Process { StartInfo = start };
        var started = false;
        Task output = Task.CompletedTask;
        Task error = Task.CompletedTask;
        try
        {
            var workspace = new CliWorkspace(request.ExecutableRoot, request.ExecutableRoot, CliWorkspaceSelectionMethod.CurrentDirectory);
            var relative = Path.GetRelativePath(request.ExecutableRoot, request.Executable).Replace(Path.DirectorySeparatorChar, '/');
            if (!ToolIdentity.IsRelativePath(relative) || ToolFileReader.Observe(workspace, relative, timeout.Token) != NoFollowLeafState.OrdinaryFile)
                return new(ToolCommandState.Failed, null, "The executable is outside its verified install or isn't an ordinary file.");
            await using (var executable = new FileStream(request.Executable, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var digest = Convert.ToHexStringLower(await SHA256.HashDataAsync(executable, timeout.Token).ConfigureAwait(false));
                if (digest != request.EntrySha256) return new(ToolCommandState.Failed, null, "The executable hash changed before launch.");
            }
            timeout.Token.ThrowIfCancellationRequested();
            if (ToolFileReader.Observe(workspace, relative, timeout.Token) != NoFollowLeafState.OrdinaryFile)
                return new(ToolCommandState.Failed, null, "The executable path changed before launch.");
            started = process.Start();
            if (!started) return new(ToolCommandState.Failed, null, "The declared process couldn't start.");
            process.StandardInput.Close();
            // Reap the child before disposing pipes, including after cancellation on Windows.
            output = process.StandardOutput.BaseStream.CopyToAsync(Stream.Null, CancellationToken.None);
            error = process.StandardError.BaseStream.CopyToAsync(Stream.Null, CancellationToken.None);
            if (request.Started is { } notifyStarted) await notifyStarted(timeout.Token).ConfigureAwait(false);
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            await Task.WhenAll(output, error).WaitAsync(timeout.Token).ConfigureAwait(false);
            return new(ToolCommandState.Exited, process.ExitCode, null);
        }
        catch (OperationCanceledException)
        {
            return new(token.IsCancellationRequested ? ToolCommandState.Interrupted : ToolCommandState.Failed, null,
                token.IsCancellationRequested ? "The declared command was cancelled." : "The declared command exceeded its timeout.");
        }
        catch (Exception exception) when (exception is Win32Exception or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return new(ToolCommandState.Failed, null, exception.Message);
        }
        finally
        {
            if (started)
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch (InvalidOperationException) when (process.HasExited) { }
                }
                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
                timeout.Cancel();
                try { await Task.WhenAll(output, error).ConfigureAwait(false); }
                catch (Exception exception) when (exception is OperationCanceledException or IOException or ObjectDisposedException) { }
            }
        }
    }
}
