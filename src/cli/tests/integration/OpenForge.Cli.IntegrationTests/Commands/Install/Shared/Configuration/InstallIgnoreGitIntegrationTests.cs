using System.Diagnostics;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;

[Trait("Feature", "workspace-route-sharing"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallIgnoreGitIntegrationTests
{
    [Theory(DisplayName = "Generated Gitignore keeps the route host and JSON trackable while ignoring private contents")]
    [InlineData("_working.md"), InlineData("index.md")]
    public async Task SharedEntrypointRemainsEligible(string entrypoint)
    {
        // Git creates its own inventory, so this test owns the complete temporary tree.
        var root = Directory.CreateTempSubdirectory("open-forge-sharing-git-").FullName;
        try
        {
            const string directory = ".agents/memory/working";
            var host = $"{directory}/{entrypoint}";
            var privatePaths = new[] { $"{directory}/note.md", $"{directory}/nested/_nested.md", $"{directory}/index.overwrite.md" };
            foreach (var path in privatePaths.Append(host).Append(".agents/open-forge.json").Append(".agents/open-forge.lock.json"))
            {
                var absolute = Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? root);
                await File.WriteAllTextAsync(absolute, "owned fixture", TestContext.Current.CancellationToken);
            }
            await File.WriteAllBytesAsync(Path.Combine(root, ".gitignore"), InstallIgnoreSection.Rewrite([], InstallConfigurationChoices.Defaults(InstallPreset.Essentials),
                [new(directory, host)]), TestContext.Current.CancellationToken);
            Assert.Equal(0, (await Git(root, ["init", "--quiet"])).ExitCode);
            var candidates = privatePaths.Append(host).Append(".agents/open-forge.json").Append(".agents/open-forge.lock.json").ToArray();
            var result = await Git(root, ["check-ignore", "--no-index", "-z", "--stdin"], string.Join('\0', candidates) + '\0');
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(privatePaths, result.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<(int ExitCode, string Output)> Git(string root, string[] arguments, string? input = null)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(root, "absent-global-config");
        start.Environment["GIT_CONFIG_SYSTEM"] = Path.Combine(root, "absent-system-config");
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        start.Environment["GIT_CONFIG_COUNT"] = "0";
        start.Environment["GIT_TERMINAL_PROMPT"] = "0";
        foreach (var name in new[] { "GIT_DIR", "GIT_WORK_TREE", "GIT_INDEX_FILE", "GIT_CONFIG_PARAMETERS" }) start.Environment.Remove(name);
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Git did not start.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (input is not null) await process.StandardInput.WriteAsync(input);
        process.StandardInput.Close();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(deadline.Token);
            Assert.True(process.ExitCode is 0 or 1, await error);
            return (process.ExitCode, await output);
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
