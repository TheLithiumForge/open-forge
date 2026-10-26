using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameGitPathIntegrationTests
{
    private const string ChildMarker = "OPENFORGE_RENAME_EMPTY_PATH_TEST";

    [Fact(DisplayName = "Git missing from PATH is optional in an isolated real process")]
    public async Task GitAbsentFromPath()
    {
        if (Environment.GetEnvironmentVariable(ChildMarker) == "1")
        {
            using var repository = new RenameEvidenceGitWorkspace();
            Assert.Equal(string.Empty, Environment.GetEnvironmentVariable("PATH"));
            var result = await new GitRenameEvidenceReader(timeProvider: new RenameEvidenceClock()).ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot) }), TestContext.Current.CancellationToken);
            Assert.Equal(RenameGitAvailability.Unavailable, result.Availability);
            return;
        }

        var executable = Path.Combine(AppContext.BaseDirectory, "OpenForge.Cli.IntegrationTests" + (OperatingSystem.IsWindows() ? ".exe" : string.Empty));
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("--filter-class");
        start.ArgumentList.Add(typeof(RenameGitPathIntegrationTests).FullName ?? throw new InvalidOperationException());
        start.Environment["PATH"] = string.Empty;
        start.Environment[ChildMarker] = "1";
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The isolated test process did not start.");
        var output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var error = process.StandardError.ReadToEndAsync(CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            Assert.True(process.ExitCode == 0, await output + await error);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(output, error);
        }
    }
}
