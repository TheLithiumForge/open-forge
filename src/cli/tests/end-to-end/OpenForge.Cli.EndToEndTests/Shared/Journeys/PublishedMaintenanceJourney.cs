using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal sealed class PublishedMaintenanceJourney : IDisposable
{
    internal const string Guidance = ".agents/guidance/_guidance.md";
    internal const string Crystallized = ".agents/memory/crystallized/_crystallized.md";
    internal const string Inherited = "- inherited - No local axioms; loaded ancestor axioms remain active.";
    private readonly PublishedJourneyWorkspace _workspace;
    private readonly Dictionary<string, string> _environment;
    internal JourneyFrontmatterForm Form { get; }
    internal string Path => _workspace.Path;
    internal string WorkspaceFlag => $"--workspace {Quote(Path)}";

    private PublishedMaintenanceJourney(string purpose, JourneyFrontmatterForm form)
    {
        _workspace = PublishedJourneyWorkspace.Create(purpose);
        Form = form;
        _environment = new(_workspace.ProcessEnvironment)
        {
            ["GIT_CONFIG_GLOBAL"] = Combine("absent-global"),
            ["GIT_CONFIG_SYSTEM"] = Combine("absent-system"),
            ["GIT_CONFIG_NOSYSTEM"] = "1",
            ["GIT_CONFIG_COUNT"] = "2",
            ["GIT_CONFIG_KEY_0"] = "core.excludesFile",
            ["GIT_CONFIG_VALUE_0"] = Combine("absent-ignore"),
            ["GIT_CONFIG_KEY_1"] = "core.autocrlf",
            ["GIT_CONFIG_VALUE_1"] = "false",
        };
    }

    internal static async Task<PublishedMaintenanceJourney> CreateAsync(string purpose, JourneyFrontmatterForm form = JourneyFrontmatterForm.Scoped)
    {
        var journey = new PublishedMaintenanceJourney(purpose, form);
        try
        {
            await journey.GitAsync("init", "--quiet");
            journey.Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            journey.Write(".agents/open-forge.json", $"{{\"schemaVersion\":1,\"frontmatter\":\"{JourneyFrontmatter.Flag(form)}\"}}\n");
            journey.Write(".agents/loader.md", "# Open Forge Loader\n\n## Axioms\n\n- Preserve authored content.\n\n## Entries\n\n"
                + "- [Guidance](guidance/_guidance.md) - #Guidance\n- [Memory](memory/_memory.md) - #Memory\n");
            journey.Write(Guidance, journey.Document("Guidance", ["Guidance"], $"# Guidance\n\n## Axioms\n\n{Inherited}\n\n## Entries\n\n- none - No entries - #Empty\n"));
            journey.Write(".agents/memory/_memory.md", journey.Document("Memory", ["Memory"], $"# Memory\n\n## Axioms\n\n{Inherited}\n\n## Entries\n\n- [Crystallized](crystallized/_crystallized.md) - #Memory #Crystallized\n"));
            journey.Write(Crystallized, journey.Document("Crystallized", ["Memory", "Crystallized"], $"# Crystallized\n\n## Axioms\n\n{Inherited}\n\n## Entries\n\n- none - No entries - #Empty\n"));
            await journey.CleanDoctorAsync();
            return journey;
        }
        catch
        {
            journey.Dispose();
            throw;
        }
    }

    internal string Combine(string path) => _workspace.Combine(path);
    internal void Expect(params string[] paths) => _workspace.ExpectFiles(paths);
    internal void Write(string path, string text) => _workspace.WriteText(path, text);
    internal void WriteBytes(string path, byte[] bytes) => _workspace.WriteBytes(path, bytes);
    internal void AssertBytes(string path, string text) => Assert.Equal(Encoding.UTF8.GetBytes(text), File.ReadAllBytes(Combine(path)));

    internal string Document(string description, string[] tags, string body)
        => Header(Form, description, tags) + body;

    internal static string Header(JourneyFrontmatterForm form, string description, string[] tags, string newline = "\n")
    {
        var fields = $"description: {description}{newline}tags: [{string.Join(", ", tags)}]{newline}";
        if (form == JourneyFrontmatterForm.Scoped)
            fields = $"open-forge:{newline}  description: {description}{newline}  tags: [{string.Join(", ", tags)}]{newline}";
        return $"---{newline}{fields}---{newline}{newline}";
    }

    internal string Scaffold(string id, string title, string[] tags, string entries)
        => Document($"Notes in {id}", tags, $"# {title}\n\nNotes in {id}.\n\n## Axioms\n\n{Inherited}\n\n## Entries\n\n{entries}\n");

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _workspace.SnapshotState()) state[$"workspace/{pair.Key}"] = pair.Value;
        foreach (var pair in PublishedWorkspaceTreeSnapshot.Capture(_workspace.LockStore.LocalApplicationDataDirectory)) state[$"store/{pair.Key}"] = pair.Value;
        return state;
    }

    internal async Task<ProcessRunResult> TextAsync(int exit, bool readOnly, params string[] arguments)
    {
        string[] args = [.. arguments, "--workspace", Path];
        var result = readOnly
            ? await PublishedJourneyProcess.RunWithoutWritesAsync(_workspace.Target, Path, Snapshot, args, _environment)
            : await PublishedJourneyProcess.RunAsync(_workspace.Target, Path, args, _environment);
        Assert.True(result.ExitCode == exit, $"Expected exit {exit}, got {result.ExitCode}.\n{result.StandardOutput}\n{result.StandardError}");
        if (exit < 4) Assert.Equal(string.Empty, result.StandardError);
        else
        {
            Assert.Equal(string.Empty, result.StandardOutput);
            Assert.NotEmpty(result.StandardError);
        }
        return result;
    }

    internal async Task<JsonDocument> JsonAsync(int exit, bool readOnly, params string[] arguments)
    {
        var result = await TextAsync(exit, readOnly, [.. arguments, "--format", "json", "--detail", "full"]);
        var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(3, json.RootElement.GetProperty("schemaVersion").GetInt32());
        return json;
    }

    internal async Task CleanDoctorAsync()
    {
        var text = await TextAsync(0, true, "doctor");
        Assert.DoesNotContain("Next:", text.StandardOutput, StringComparison.Ordinal);
        using var json = await JsonAsync(0, true, "doctor");
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(json.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("severity").GetString() is "warning" or "error");
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("errors").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("warnings").GetInt32());
        Assert.Equal(6, json.RootElement.GetProperty("counts").GetProperty("checksComplete").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("limitations").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("next").ValueKind);
        Assert.All(json.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray(), category => Assert.Equal("complete", category.GetProperty("coverage").GetString()));
    }

    internal async Task NoOpAsync(string command)
    {
        var arguments = command == "repair" ? new[] { "repair", "--automatic" } : [command];
        var text = await TextAsync(0, true, arguments);
        Assert.Contains(command == "repair" ? "Nothing to repair." : "Nothing to do.", text.StandardOutput, StringComparison.Ordinal);
        using var json = await JsonAsync(0, true, arguments);
        Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
        _workspace.LockStore.AssertNoRecoveryArtifacts(Path);
    }

    internal async Task CommitAsync()
    {
        await GitAsync("add", "--", "AGENTS.md", ".agents");
        await GitAsync("-c", "user.name=Journey", "-c", "user.email=journey@example.invalid", "-c", "commit.gpgsign=false", "commit", "--quiet", "-m", "Journey baseline");
    }

    internal void DisableGit() => _environment["PATH"] = Combine("no-tools");

    internal async Task<string> GitAsync(params string[] arguments)
    {
        var start = new ProcessStartInfo("git") { WorkingDirectory = Path, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var item in _environment) start.Environment[item.Key] = item.Value;
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Git did not start.");
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        Assert.True(process.ExitCode == 0, await error);
        return (await output).Trim();
    }

    internal async Task<ProcessRunResult> CopyCommandAsync(string command, int exit)
    {
        Assert.StartsWith("open-forge ", command, StringComparison.Ordinal);
        Assert.Contains(WorkspaceFlag, command, StringComparison.Ordinal);
        Assert.DoesNotContain("--dry-run", command, StringComparison.Ordinal);
        var shell = OperatingSystem.IsWindows()
            ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe") : "/bin/sh";
        var start = new ProcessStartInfo(shell) { WorkingDirectory = Path, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var item in _environment) start.Environment[item.Key] = item.Value;
        start.Environment["OPENFORGE_TEST_EXECUTABLE"] = _workspace.Target.ExecutablePath;
        if (OperatingSystem.IsWindows())
        {
            start.ArgumentList.Add("-NoProfile");
            start.ArgumentList.Add("-NonInteractive");
            start.ArgumentList.Add("-Command");
            start.ArgumentList.Add("function open-forge { & $env:OPENFORGE_TEST_EXECUTABLE @args }\n" + command + "\nexit $LASTEXITCODE");
        }
        else
        {
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add("\"$OPENFORGE_TEST_EXECUTABLE\" " + command["open-forge ".Length..]);
        }
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The command shell did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        var result = new ProcessRunResult(process.ExitCode, await stdout, await stderr);
        TestContext.Current.TestOutputHelper?.WriteLine($"Copied command: {command}\nExit: {result.ExitCode}\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}");
        Assert.True(result.ExitCode == exit, $"{command}\n{result.StandardOutput}\n{result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
        return result;
    }

    internal static string Next(JsonDocument json) => json.RootElement.GetProperty("next").GetProperty("command").GetString() ?? throw new InvalidOperationException("No next command.");
    internal static string Quote(string value)
    {
        if (value.Length > 0 && value.All(character => char.IsLetterOrDigit(character) || "-_/.:=".Contains(character))) return value;
        return "'" + value.Replace("'", OperatingSystem.IsWindows() ? "''" : "'\\''", StringComparison.Ordinal) + "'";
    }
    internal static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    public void Dispose()
    {
        var git = Combine(".git");
        if (Directory.Exists(git))
        {
            Assert.StartsWith(Path + System.IO.Path.DirectorySeparatorChar, git, StringComparison.Ordinal);
            Assert.Null(new DirectoryInfo(git).LinkTarget);
            foreach (var entry in Directory.EnumerateFileSystemEntries(git, "*", SearchOption.AllDirectories))
            {
                Assert.Equal((FileAttributes)0, File.GetAttributes(entry) & FileAttributes.ReparsePoint);
                if (File.Exists(entry)) File.SetAttributes(entry, FileAttributes.Normal);
            }
            Directory.Delete(git, recursive: true);
        }
        _workspace.Dispose();
    }
}
