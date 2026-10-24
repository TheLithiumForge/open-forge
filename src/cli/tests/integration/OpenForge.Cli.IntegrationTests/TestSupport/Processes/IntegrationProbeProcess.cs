using System.Diagnostics;

namespace OpenForge.Cli.IntegrationTests.TestSupport.Processes;

internal static class IntegrationProbeProcess
{
    internal static async Task<string[]> RunAsync(string[] arguments, bool disableImplicitLocking = false)
    {
        var executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("The test executable path is unavailable.");
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "OpenForge.Cli.IntegrationTests.dll"));
        }

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        if (disableImplicitLocking)
        {
            start.Environment["DOTNET_SYSTEM_IO_DISABLEFILELOCKING"] = "1";
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("The test probe did not start.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            Assert.True(process.ExitCode == 0, await error);
            return (await output).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}
