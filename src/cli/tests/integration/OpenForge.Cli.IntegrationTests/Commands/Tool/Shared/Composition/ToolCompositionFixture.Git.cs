using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

internal sealed partial class ToolCompositionFixture
{
    private IToolProcessHost? _git;
    private string? _gitExecutable;

    internal async Task EnableGitAsync()
    {
        var environment = new Dictionary<string, string?>
        {
            [ToolPathPolicy.PathVariable] = Environment.GetEnvironmentVariable(ToolPathPolicy.PathVariable),
            ["GIT_CONFIG_GLOBAL"] = _owned.CreateFile("git-config"),
            ["GIT_CONFIG_NOSYSTEM"] = "1",
            ["HOME"] = NativeHome,
            ["USERPROFILE"] = NativeHome,
        };
        _git = ToolHostFactory.CreateProcesses(environment);
        var command = await _git.ResolveAsync(ToolGitEnvironment.Executable, Workspace, TestContext.Current.CancellationToken);
        Assert.NotNull(command);
        _gitExecutable = command.Invocation.ExecutablePath;
        var initialized = await _git.RunAsync(new()
        {
            ExecutablePath = _gitExecutable,
            Arguments = ["init", "--quiet", Workspace],
            WorkingDirectory = Workspace,
            Environment = ToolGitEnvironment.Probe,
            Timeout = TimeSpan.FromSeconds(10),
            MaximumOutputBytes = 16384,
        }, TestContext.Current.CancellationToken);
        Assert.Equal(0, initialized.ExitCode);
    }
}
