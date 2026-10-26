using OpenForge.Cli.TestSupport.Contracts;
using System.Diagnostics;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRepairProcessTests
{
    [Fact(DisplayName = "Published Repair preserves a metadata-only choice until its last incoming link is relinked")]
    [Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedSequentialMetadataChoicesKeepEvidence()
    {
        using var fixture = new RenameWorkspace();
        await fixture.InitializeAsync();
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments)
            => await PublishedProcessTestSupport.RunAsync(target, fixture.Root, [.. arguments, "--workspace", fixture.Root], fixture.EnvironmentVariables);
        const string oldPath = ".agents/guidance/billing/rounding.md";
        const string newPath = ".agents/guidance/billing/new.md";
        const string navigationPath = ".agents/guidance/billing/_billing.md";
        const string firstSource = ".agents/guidance/billing/taxes.md";
        const string secondSource = ".agents/guidance/notes.md";
        fixture.Write(firstSource, RenameWorkspace.Document("Tax references", "# Taxes\n\nRead [ZZFirstLabelZZ](rounding.md#rounding).\n"));
        fixture.Write(secondSource, RenameWorkspace.Document("Maintainer references", "# Notes\n\nRead [ZZSecondLabelZZ](billing/rounding.md#rounding).\n"));
        Assert.Equal(0, (await Run("index")).ExitCode);
        await fixture.RemoveHeadAsync();
        var oldNavigation = File.ReadAllBytes(fixture.PathFor(navigationPath));
        var secondBytes = File.ReadAllBytes(fixture.PathFor(secondSource));
        File.Move(fixture.PathFor(oldPath), fixture.PathFor(newPath));
        fixture.Write(newPath, RenameWorkspace.Document("Money rounding policy", "# Changed heading\n\nCompletely different body.\n"));
        async Task<string[]> Choices()
        {
            var result = await Run("repair", "--automatic", "--dry-run", "--detail", "full");
            Assert.True(result.ExitCode == 2, result.StandardOutput + result.StandardError);
            var text = result.StandardOutput + result.StandardError;
            Assert.Contains("Next: Choose a target above, then run its command.", text);
            return text.Split('\n').Select(line => line.Trim()).Where(line => line.StartsWith("open-forge repair --relink", StringComparison.Ordinal)).ToArray();
        }
        var initial = await Choices();
        Assert.Equal(2, initial.Length);
        Assert.All(initial, command => Assert.DoesNotContain(newPath + "#", command));
        await CopyCommandAsync(target, fixture, initial[0]);
        Assert.Equal(oldNavigation, File.ReadAllBytes(fixture.PathFor(navigationPath)));
        Assert.Equal(secondBytes, File.ReadAllBytes(fixture.PathFor(secondSource)));
        Assert.Contains("[ZZFirstLabelZZ](new.md)", File.ReadAllText(fixture.PathFor(firstSource)));
        var continuation = Assert.Single(await Choices());
        Assert.Equal(initial[1], continuation);
        var doctorBefore = await Run("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(2, doctorBefore.ExitCode);
        using (var json = JsonDocument.Parse(doctorBefore.StandardOutput))
        {
            var missing = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-missing");
            Assert.Equal("guided-choice", missing.GetProperty("resolution").GetString());
        }
        await CopyCommandAsync(target, fixture, continuation);
        Assert.Contains("[ZZSecondLabelZZ](billing/new.md)", File.ReadAllText(fixture.PathFor(secondSource)));
        var navigation = File.ReadAllText(fixture.PathFor(navigationPath));
        Assert.DoesNotContain("](rounding.md)", navigation);
        Assert.Contains("](new.md)", navigation);
        Assert.Contains("- Keep exact monetary rules.", navigation);
        var doctor = await Run("doctor");
        Assert.True(doctor.ExitCode == 0, doctor.StandardOutput + doctor.StandardError);
        Assert.Contains("No problems found.", doctor.StandardOutput);
        Assert.Equal(0, (await Run("repair", "--automatic")).ExitCode);
    }

    [Theory, InlineData(false, 0), InlineData(false, 1), InlineData(true, 0), InlineData(true, 1)]
    [Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedGuidedCommandsCanBeCopiedForEveryCandidate(bool automatic, int candidateIndex)
    {
        using var fixture = new RenameWorkspace();
        await fixture.InitializeAsync();
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments)
            => await PublishedProcessTestSupport.RunAsync(target, fixture.Root, [.. arguments, "--workspace", fixture.Root], fixture.EnvironmentVariables);
        fixture.Write(".agents/guidance/choice notes.md", RenameWorkspace.Document("Another authored link", "# Choice\n\nRead [Rounding](billing/rounding.md#rounding).\n"));
        Assert.Equal(0, (await Run("index")).ExitCode);
        await fixture.RecordTreeAsync();
        File.Move(fixture.PathFor(".agents/guidance/billing/rounding.md"), fixture.PathFor(".agents/guidance/billing/money-rounding.md"));
        fixture.Write(".agents/guidance/billing/alternate-rounding.md", File.ReadAllText(fixture.PathFor(".agents/guidance/billing/money-rounding.md")));
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var output = await Run(automatic ? ["repair", "--automatic", "--detail", detail] : ["repair", "--detail", detail]);
            Assert.Equal(automatic ? 2 : 5, output.ExitCode);
            var text = output.StandardOutput + output.StandardError;
            Assert.Contains("Next: Choose a target above, then run its command.", text);
            var lines = text.Split('\n');
            Assert.Equal(4, lines.Count(line => line.TrimStart().StartsWith("To use billing/", StringComparison.Ordinal)));
            Assert.All(lines.Where(line => line.Contains("open-forge repair --relink", StringComparison.Ordinal)),
                line => Assert.StartsWith("           open-forge repair --relink", line));
            Assert.DoesNotContain("title match:", text);
            Assert.DoesNotContain("possible target .agents/", text);
            Assert.Equal(4, lines.Count(line => line.Contains("(same content):", StringComparison.Ordinal)));
            var commands = text.Split('\n').Select(line => line.Trim())
                .Where(line => line.StartsWith("open-forge repair --relink", StringComparison.Ordinal)).ToArray();
            Assert.True(commands.Length == 4, text);
            Assert.All(commands, command => Assert.Contains("--workspace", command));
            Assert.Contains("More than one file looks like its new location.", string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)));
            if (!automatic)
            {
                Assert.StartsWith("Repair cannot ask for a choice in this session. Nothing was changed.", text);
                Assert.DoesNotContain("  Error  ", text);
                Assert.Contains("Next: Choose a target above, then run its command.", text);
                var machine = await Run("repair", "--format", "json");
                Assert.Equal(5, machine.ExitCode);
                using var selectionJson = JsonDocument.Parse(machine.StandardOutput);
                Assert.Contains(selectionJson.RootElement.GetProperty("findings").EnumerateArray(),
                    finding => finding.GetProperty("code").GetString() == "repair.selection-required");
            }
            if (detail != "debug") continue;
            foreach (var command in commands.Where((_, index) => index % 2 == candidateIndex))
                await CopyCommandAsync(target, fixture, command);
        }
        var doctor = await Run("doctor", "--format", "json");
        Assert.True(doctor.ExitCode == 0, doctor.StandardOutput + doctor.StandardError);
        using var json = JsonDocument.Parse(doctor.StandardOutput);
        Assert.DoesNotContain(json.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("severity").GetString() is "warning" or "error");
    }

    private static async Task CopyCommandAsync(PublishedExecutableTarget target, RenameWorkspace fixture, string command)
    {
        var shell = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe") : "/bin/sh";
        var start = new ProcessStartInfo(shell)
        {
            WorkingDirectory = fixture.Root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var item in fixture.EnvironmentVariables) start.Environment[item.Key] = item.Value;
        start.Environment["OPENFORGE_TEST_EXECUTABLE"] = target.ExecutablePath;
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
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The printed command shell did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        Assert.True(process.ExitCode is 0 or 2, command + "\n" + await stdout + await stderr);
    }

    [Theory, InlineData("repair-first"), InlineData("index-first"), InlineData("move"), InlineData("folder"), InlineData("edited"), InlineData("two"), InlineData("deleted")]
    [Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRenameJourneyUsesGitProofAndConverges(string scenario)
    {
        using var fixture = new RenameWorkspace();
        await fixture.InitializeAsync();
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments)
            => await PublishedProcessTestSupport.RunAsync(target, fixture.Root, [.. arguments, "--workspace", fixture.Root], fixture.EnvironmentVariables);
        var indexed = await Run("index");
        Assert.True(indexed.ExitCode == 0, indexed.StandardOutput + indexed.StandardError);
        await fixture.RecordTreeAsync();
        const string oldPath = ".agents/guidance/billing/rounding.md";
        const string newPath = ".agents/guidance/billing/money-rounding.md";
        if (scenario == "folder") Directory.Move(fixture.PathFor(".agents/guidance/billing"), fixture.PathFor(".agents/guidance/invoices"));
        else if (scenario == "move")
        {
            Directory.CreateDirectory(fixture.PathFor(".agents/guidance/finance/deep"));
            File.Move(fixture.PathFor(oldPath), fixture.PathFor(".agents/guidance/finance/deep/money-rounding.md"));
        }
        else if (scenario == "deleted") File.Delete(fixture.PathFor(oldPath));
        else
        {
            File.Move(fixture.PathFor(oldPath), fixture.PathFor(newPath));
            if (scenario == "edited") File.AppendAllText(fixture.PathFor(newPath), "One edited line.\n");
            if (scenario == "two") fixture.Write(".agents/guidance/billing/alternate-rounding.md", File.ReadAllText(fixture.PathFor(newPath)));
        }
        if (scenario == "index-first") Assert.Equal(0, (await Run("index")).ExitCode);
        var before = fixture.Snapshot();
        var preview = await Run("repair", "--automatic", "--dry-run", "--format=json", "--detail=full");
        Assert.Equal(before, fixture.Snapshot());
        Assert.True(preview.ExitCode is 0 or 2, preview.StandardOutput + preview.StandardError);
        using var previewJson = JsonDocument.Parse(preview.StandardOutput);
        var data = previewJson.RootElement.GetProperty("data");
        if (scenario is "two" or "deleted")
        {
            Assert.Empty(data.GetProperty("renames").EnumerateArray());
            var remaining = Assert.Single(data.GetProperty("remaining").EnumerateArray());
            Assert.Equal(scenario == "two" ? "guided" : "manual", remaining.GetProperty("kind").GetString());
            var automatic = await Run("repair", "--automatic", "--format=json");
            Assert.Equal(2, automatic.ExitCode);
            using var automaticJson = JsonDocument.Parse(automatic.StandardOutput);
            var next = automaticJson.RootElement.GetProperty("next");
            if (scenario == "two")
            {
                // Listed choices precede Index, and repeating the request keeps the same choice action.
                Assert.Equal("sentence", next.GetProperty("kind").GetString());
                Assert.Equal("Choose a target above, then run its command.", next.GetProperty("command").GetString());
                var guided = await Run("repair", "--automatic", "--format=json");
                using var guidedJson = JsonDocument.Parse(guided.StandardOutput);
                Assert.Equal("Choose a target above, then run its command.", guidedJson.RootElement.GetProperty("next").GetProperty("command").GetString());
                Assert.DoesNotContain("--automatic", guidedJson.RootElement.GetProperty("next").GetProperty("command").GetString());
            }
            fixture.Write(".agents/guidance/notes.md", RenameWorkspace.Document("Maintainer rename journey", scenario == "deleted"
                ? "# Notes\n\nThe obsolete link was removed by its author.\n"
                : "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\nRead [Billing](billing/_billing.md).\n"));
            Assert.Equal(0, (await Run("index")).ExitCode);
        }
        else
        {
            Assert.NotEmpty(data.GetProperty("renames").EnumerateArray());
            Assert.All(data.GetProperty("renames").EnumerateArray(), rename => Assert.Equal(JsonValueKind.Object, rename.GetProperty("proof").ValueKind));
            var next = previewJson.RootElement.GetProperty("next").GetProperty("command").GetString();
            Assert.Contains("--automatic", next);
            Assert.Contains("--format json", next);
            Assert.Contains("--detail full", next);
            Assert.DoesNotContain("--dry-run", next);
            var applied = await Run("repair", "--automatic", "--format=json", "--detail=full");
            Assert.True(applied.ExitCode == 0, applied.StandardOutput + applied.StandardError);
            using var appliedJson = JsonDocument.Parse(applied.StandardOutput);
            var root = appliedJson.RootElement;
            Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "repair.rename-proven");
            Assert.Equal(scenario == "folder" ? 2 : 1, root.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            Assert.StartsWith("open-forge doctor", root.GetProperty("next").GetProperty("command").GetString());
            if (scenario == "folder")
            {
                Assert.False(File.Exists(fixture.PathFor(".agents/guidance/invoices/_billing.md")));
                Assert.Contains("- Keep exact monetary rules.", File.ReadAllText(fixture.PathFor(".agents/guidance/invoices/_invoices.md")));
            }
        }
        var doctor = await Run("doctor", "--format=json");
        Assert.True(doctor.ExitCode == 0, doctor.StandardOutput + doctor.StandardError);
        using var diagnosis = JsonDocument.Parse(doctor.StandardOutput);
        Assert.DoesNotContain(diagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("severity").GetString() is "warning" or "error");
    }

    internal sealed class RenameWorkspace : IDisposable
    {
        private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("published-repair-rename");
        private readonly PublishedWorkspaceLockStore _store = PublishedWorkspaceLockStore.Create("published-repair-rename-store");
        private string? _tree;
        private string? _customRoot;
        internal string Root { get; }
        internal IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>(_store.EnvironmentVariables)
        {
            ["GIT_CONFIG_GLOBAL"] = PathFor("absent-global"),
            ["GIT_CONFIG_SYSTEM"] = PathFor("absent-system"),
            ["GIT_CONFIG_NOSYSTEM"] = "1",
            ["GIT_CONFIG_COUNT"] = "1",
            ["GIT_CONFIG_KEY_0"] = "core.excludesFile",
            ["GIT_CONFIG_VALUE_0"] = PathFor("absent-ignore"),
        };
        internal RenameWorkspace()
        {
            Root = _temporary.CreateDirectory("repository");
            _store.Track(Root);
        }
        internal string PathFor(string path) => Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar));
        internal void Write(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathFor(path))!);
            File.WriteAllText(PathFor(path), text);
        }
        internal static string Document(string description, string body) => $"---\nopen-forge:\n  description: {description}\n  tags: [Guidance]\n---\n\n{body}";
        internal async Task InitializeAsync()
        {
            await InitializeGitAsync();
            Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            Write(".agents/loader.md", "# Open Forge Loader\n\n## Axioms\n\n- Preserve authored content.\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
            Write(".agents/guidance/_guidance.md", Document("Guidance", "# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n"));
            Write(".agents/guidance/billing/_billing.md", Document("Billing rules", "# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n"));
            Write(".agents/guidance/billing/rounding.md", Document("Money rounding policy", "# Rounding\n\n" + string.Concat(Enumerable.Repeat("Keep monetary amounts in minor units.\n", 30))));
            Write(".agents/guidance/notes.md", Document("Maintainer rename journey", "# Notes\n\nRead [Rounding](billing/rounding.md#rounding).\nRead [Billing](billing/_billing.md).\n"));
            await RecordTreeAsync();
        }
        private async Task InitializeGitAsync()
        {
            await GitAsync(Root, "init", "--quiet");
            var source = new DirectoryInfo(AppContext.BaseDirectory);
            while (source is not null && !File.Exists(Path.Combine(source.FullName, "OpenForge.Cli.slnx"))) source = source.Parent;
            var sourceRoot = source?.FullName ?? throw new InvalidOperationException();
            var common = (await GitAsync(sourceRoot, "-c", $"safe.directory={sourceRoot.Replace('\\', '/')}", "rev-parse", "--git-common-dir")).Trim();
            Write(".git/objects/info/alternates", Path.GetFullPath(Path.Combine(sourceRoot, common, "objects")).Replace('\\', '/') + "\n");
            var head = (await GitAsync(sourceRoot, "-c", $"safe.directory={sourceRoot.Replace('\\', '/')}", "rev-parse", "HEAD")).Trim();
            await GitAsync(Root, "update-ref", "HEAD", head);
            _tree = (await GitAsync(Root, "rev-parse", "HEAD^{tree}")).Trim();
        }

        internal async Task InitializeMainRootAsync(string mainRoot)
        {
            await InitializeGitAsync();
            _customRoot = mainRoot;
            Write(PublishedTokens.Settings.RelativePath, $$"""
                {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{mainRoot}}"}
                """);
            Write(PublishedTokens.Layout.AgentHost, $"# Workspace\n\nRead `{mainRoot}/{PublishedTokens.Layout.LoaderFileName}`.\n");
            Write($"{mainRoot}/{PublishedTokens.Layout.LoaderFileName}", OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Context\n\n## Axioms\n\n- Preserve authored rules.",
                Entries = $"- [Notes](notes/{PublishedTokens.Layout.IndexFile}) - #Docs",
            }));
            Write($"{mainRoot}/notes/{PublishedTokens.Layout.IndexFile}", OpenForgeDocumentSeed.Metadata("Notes", ["Docs"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Notes\n\n## Axioms\n\n- Preserve authored rules.",
                Entries = string.Empty,
            })));
            Write($"{mainRoot}/notes/rounding{PublishedTokens.Layout.MarkdownExtension}", OpenForgeDocumentSeed.Metadata("Rounding", ["Docs"],
                "# Rounding\n\n" + string.Concat(Enumerable.Repeat("Keep monetary amounts in minor units.\n", 30))));
            Write($"{mainRoot}/notes/checklist{PublishedTokens.Layout.MarkdownExtension}", OpenForgeDocumentSeed.Metadata("Checklist", ["Docs"],
                $"# Checklist\n\nRead [Rounding](rounding{PublishedTokens.Layout.MarkdownExtension}#rounding).\n"));
            await RecordTreeAsync();
        }

        internal async Task RecordTreeAsync()
        {
            await GitAsync(Root, "add", "--", PublishedTokens.Sources.AgentsRoot, PublishedTokens.Layout.AgentHost, _customRoot ?? PublishedTokens.Sources.AgentsRoot);
            var tree = (await GitAsync(Root, "write-tree")).Trim();
            await GitAsync(Root, "replace", "-f", _tree ?? throw new InvalidOperationException(), tree);
        }
        internal async Task RemoveHeadAsync() => await GitAsync(Root, "update-ref", "-d", "HEAD");
        internal IReadOnlyDictionary<string, string> Snapshot() => _temporary.SnapshotHashes();
        private async Task<string> GitAsync(string root, params string[] arguments)
        {
            var start = new ProcessStartInfo("git") { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var item in EnvironmentVariables) start.Environment[item.Key] = item.Value;
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add("safe.directory=*");
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Git did not start.");
            var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            Assert.True(process.ExitCode == 0, await error);
            return await output;
        }
        public void Dispose()
        {
            Assert.Null(new DirectoryInfo(Root).LinkTarget);
            Assert.StartsWith(_temporary.Path + Path.DirectorySeparatorChar, Root, StringComparison.Ordinal);
            foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(Root, recursive: true);
            _store.Dispose();
            _temporary.Dispose();
        }
    }
}
