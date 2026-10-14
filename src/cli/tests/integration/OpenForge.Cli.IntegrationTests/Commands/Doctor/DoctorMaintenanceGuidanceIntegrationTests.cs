using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class DoctorMaintenanceGuidanceIntegrationTests
{
    [Theory(DisplayName = "Doctor respects actual native managed and intentionally excluded metadata boundaries")]
    [InlineData("native", "manual-decision")]
    [InlineData("managed", "targeted-operation")]
    [InlineData("excluded", "manual-decision")]
    public async Task CompletionBoundaries(string scenario, string lane)
    {
        using var workspace = await Seed();
        var path = ".agents/guidance/note.md";
        if (scenario == "native")
        {
            path = ".agents/guidance/package/resources/note.md";
            workspace.CreateFile(".agents/guidance/package/SKILL.md", OpenForgeDocumentSeed.Skill("package", "Native package"));
        }
        if (scenario == "managed") workspace.CreateFile(".agents/open-forge.lock.json",
            "{\"schemaVersion\":1,\"framework\":{\"source\":{\"id\":\"builtin\",\"version\":null},\"paths\":[\".agents/guidance/note.md\"],\"regions\":[]},\"extensions\":[],\"libraries\":[]}");
        if (scenario == "excluded") workspace.CreateFile(".agents/open-forge.json", "{\"schemaVersion\":1,\"removedFiles\":[\".agents/guidance/note.md\"]}");
        workspace.CreateFile(path, "# Note\nKeep my content.\n");
        var before = workspace.SnapshotHashes();
        using var doctor = await Diagnose(workspace);
        var finding = Finding(doctor, "route.metadata-required-missing", path);
        Assert.Equal(lane, finding.GetProperty("resolution").GetString());
        Assert.DoesNotContain(finding.GetProperty("actions").EnumerateArray(), action => action.GetProperty("command").GetString()?.StartsWith("open-forge index", StringComparison.Ordinal) == true);
        if (scenario == "managed") Assert.Contains("open-forge update", finding.GetProperty("actions")[0].GetProperty("command").GetString(), StringComparison.Ordinal);
        if (scenario == "excluded") Assert.DoesNotContain("open-forge index", Next(doctor), StringComparison.Ordinal);
        if (scenario == "native")
        {
            Assert.Equal($"{path} needs a description and tags. Index does not edit files inside a Skill package, so add them by hand.", finding.GetProperty("message").GetString());
            Assert.Equal($"Add the missing description and tags to {path} by hand.", finding.GetProperty("actions")[0].GetProperty("command").GetString());
        }
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Doctor distinguishes unique and duplicate committed-content successors before and after Index")]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GitSuccessors(int count)
    {
        using var git = new RenameEvidenceGitWorkspace();
        await git.InitializeAsync();
        var bytes = await File.ReadAllBytesAsync(git.PathFor(".agents/guidance/cli-design.md"), TestContext.Current.CancellationToken);
        var agents = Path.GetFullPath(git.PathFor(".agents"));
        Assert.StartsWith(git.RepositoryRoot + Path.DirectorySeparatorChar, agents, StringComparison.Ordinal);
        Directory.Delete(agents, recursive: true);
        git.Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        git.Write(".agents/loader.md", Loader());
        git.Write(".agents/guidance/_guidance.md", Root("- none - No entries - #Empty"));
        git.Write(".agents/guidance/source.md", Note("Incoming reference", "# Source\n\n[Rules](cli-design.md)\n"));
        for (var index = 0; index < count; index++)
            await File.WriteAllBytesAsync(git.PathFor($".agents/guidance/successor-{index}.md"), bytes, TestContext.Current.CancellationToken);
        var before = git.Snapshot();
        var first = await CliHostCapture.RunAsync(["doctor", "--workspace", git.Root, "--format", "json", "--detail", "full"], git.Root);
        using var firstReport = JsonDocument.Parse(first.Output);
        var firstFinding = Finding(firstReport, count == 1 ? "reference.target-renamed" : "reference.target-missing", ".agents/guidance/source.md");
        Assert.Equal(count == 1 ? "safe-exact" : "guided-choice", firstFinding.GetProperty("resolution").GetString());
        if (count == 2) Assert.Equal(".agents/guidance/cli-design.md is missing. More than one file looks like its new location.", firstFinding.GetProperty("message").GetString());
        Assert.Equal(before, git.Snapshot());
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index", "--workspace", git.Root], git.Root)).ExitCode);
        var after = await CliHostCapture.RunAsync(["doctor", "--workspace", git.Root, "--format", "json", "--detail", "full"], git.Root);
        using var afterReport = JsonDocument.Parse(after.Output);
        Assert.Equal($"open-forge repair{(count == 1 ? " --automatic" : string.Empty)} --workspace {CliCommandText.Quote(git.Root)}", Next(afterReport));
        Assert.DoesNotContain("--dry-run", Next(afterReport), StringComparison.Ordinal);
        if (count == 2) Assert.Equal(2, Finding(afterReport, "reference.target-missing", ".agents/guidance/source.md").GetProperty("candidates").GetArrayLength());
    }

    [Theory(DisplayName = "Doctor gives eligible plain and deep notes a working Index continuation without writes")]
    [InlineData(".agents/guidance/note.md", "# Authored note\n\nKeep these exact words.\n")]
    [InlineData(".agents/guidance/team/deep/note.md", "# Authored note\n\nKeep these exact words.\n")]
    [InlineData(".agents/guidance/no-heading.md", "Keep these exact words.\n")]
    [InlineData(".agents/guidance/level-two.md", "## Authored note\n\nKeep these exact words.\n")]
    public async Task CompletionConverges(string path, string body)
    {
        using var workspace = await Seed();
        using var created = new CreatedFiles(workspace, ".agents/guidance/team/_team.md", ".agents/guidance/team/deep/_deep.md");
        workspace.CreateFile(path, body);
        var before = workspace.SnapshotHashes();
        using var doctor = await Diagnose(workspace);
        var metadata = Finding(doctor, "route.metadata-required-missing", path);
        Assert.Equal("targeted-operation", metadata.GetProperty("resolution").GetString());
        Assert.Single(metadata.GetProperty("actions").EnumerateArray());
        Assert.Equal($"open-forge index --workspace {CliCommandText.Quote(workspace.Path)}", Next(doctor));
        Assert.Equal(before, workspace.SnapshotHashes());
        var text = await CliHostCapture.RunAsync(["doctor", "--workspace", workspace.Path], workspace.Path);
        Assert.Single(text.Output.Split('\n'), line => line.StartsWith("Next:", StringComparison.Ordinal));
        var indexed = await CliHostCapture.RunAsync(["index", "--workspace", workspace.Path], workspace.Path);
        Assert.Equal(0, indexed.ExitCode);
        using var clean = await Diagnose(workspace);
        Assert.Equal("completed", clean.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(clean.RootElement.GetProperty("findings").EnumerateArray(), IsHealthFinding);
        Assert.Equal(JsonValueKind.Null, clean.RootElement.GetProperty("next").ValueKind);
        Assert.EndsWith(body, File.ReadAllText(workspace.Combine(path)), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Doctor uses stale metadata proof for a rename and gives automatic Repair without changing files")]
    public async Task RecordedRename()
    {
        using var workspace = await Seed();
        workspace.ReplaceText(".agents/guidance/_guidance.md", Root("- [Specific authored rounding rules](old.md) - #Guidance"));
        workspace.CreateFile(".agents/guidance/new.md", Note("Specific authored rounding rules", "# Rounding\n\nKeep the rules.\n"));
        workspace.CreateFile(".agents/guidance/source.md", Note("Incoming reference", "# Source\n\n[Rules](old.md)\n"));
        var before = workspace.SnapshotHashes();
        using var doctor = await Diagnose(workspace);
        var renamed = Finding(doctor, "reference.target-renamed", ".agents/guidance/source.md");
        Assert.Equal("safe-exact", renamed.GetProperty("resolution").GetString());
        Assert.Contains(renamed.GetProperty("evidence").EnumerateArray(), evidence => evidence.GetProperty("value").GetString() == "new.md");
        Assert.Equal($"open-forge repair --automatic --workspace {CliCommandText.Quote(workspace.Path)}", Next(doctor));
        Assert.DoesNotContain(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-missing");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Deleted links have no self or neighborhood candidate and require the exact authored edit")]
    public async Task DeletedLink()
    {
        using var workspace = await Seed();
        const string source = ".agents/guidance/source.md";
        const string deleted = ".agents/guidance/deleted.md";
        workspace.CreateFile(deleted, Note("Deleted note", "# Deleted\n"));
        workspace.CreateFile(source, Note("Incoming reference", "# Source\n\n[Deleted](deleted.md)\n"));
        await CliHostCapture.RunAsync(["index"], workspace.Path);
        File.Delete(workspace.Combine(deleted));
        using var stale = await Diagnose(workspace);
        Assert.Single(stale.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-missing");
        Finding(stale, "route.generated-entry-extra", ".agents/guidance/_guidance.md");
        Assert.StartsWith("open-forge index", Next(stale), StringComparison.Ordinal);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index", ".agents/guidance/_guidance.md"], workspace.Path)).ExitCode);
        using var doctor = await Diagnose(workspace);
        var missing = Finding(doctor, "reference.target-missing", source);
        Assert.Equal("manual-decision", missing.GetProperty("resolution").GetString());
        Assert.Empty(missing.GetProperty("candidates").EnumerateArray());
        Assert.Equal(".agents/guidance/deleted.md is missing and no replacement was found.", missing.GetProperty("message").GetString());
        Assert.Contains(source, Next(doctor), StringComparison.Ordinal);
        Assert.Contains("line", Next(doctor), StringComparison.Ordinal);
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var text = await CliHostCapture.RunAsync(["doctor", "--detail", detail, "--workspace", workspace.Path], workspace.Path);
            Assert.Equal(1, text.Output.Split(Next(doctor), StringSplitOptions.None).Length - 1);
        }
        workspace.ReplaceText(source, Note("Incoming reference", "# Source\n\nThe obsolete reference was removed.\n"));
        using var clean = await Diagnose(workspace);
        Assert.DoesNotContain(clean.RootElement.GetProperty("findings").EnumerateArray(), IsHealthFinding);
    }

    [Theory(DisplayName = "Malformed frontmatter reports the validator cause and converges after the named edit and Index")]
    [InlineData("---\ndescription: [\n---\n# Bad\n", "The frontmatter contains invalid YAML.")]
    [InlineData("---\ndescription: Bad\n# Bad\n", "The frontmatter block is not closed.")]
    public async Task MalformedFrontmatter(string body, string cause)
    {
        using var workspace = await Seed();
        const string path = ".agents/guidance/bad.md";
        workspace.CreateFile(path, body);
        using var doctor = await Diagnose(workspace);
        var malformed = Finding(doctor, "workspace.frontmatter-malformed", path);
        Assert.Equal("manual-decision", malformed.GetProperty("resolution").GetString());
        Assert.Equal($"{path} has invalid frontmatter. {cause}", malformed.GetProperty("message").GetString());
        Assert.Contains(path, Next(doctor), StringComparison.Ordinal);
        Assert.Contains($"open-forge index --workspace {CliCommandText.Quote(workspace.Path)}", Next(doctor), StringComparison.Ordinal);
        workspace.ReplaceText(path, "# Bad\n");
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        using var clean = await Diagnose(workspace);
        Assert.DoesNotContain(clean.RootElement.GetProperty("findings").EnumerateArray(), IsHealthFinding);
    }

    [Theory(DisplayName = "Doctor treats supplied duplicate null and empty fields as malformed rather than completable")]
    [InlineData("description: One\ndescription: Two\ntags: [Guidance]\n")]
    [InlineData("description: null\ntags: [Guidance]\n")]
    [InlineData("description: Supplied\ntags: []\n")]
    public async Task SuppliedMalformedFields(string fields)
    {
        using var workspace = await Seed();
        const string path = ".agents/guidance/bad.md";
        workspace.CreateFile(path, $"---\n{fields}---\n\n# Bad\n");
        using var doctor = await Diagnose(workspace);
        var malformed = Finding(doctor, "workspace.frontmatter-malformed", path);
        Assert.Equal("error", malformed.GetProperty("severity").GetString());
        Assert.DoesNotContain(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("subject").GetProperty("path").GetString() == path
            && finding.GetProperty("code").GetString() is "workspace.frontmatter-duplicate" or "route.metadata-required-missing");
        Assert.Contains($"Fix the frontmatter in {path}", Next(doctor), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Unknown folders receive a move or Loader edit and never an automatic Index action")]
    public async Task OutsideFolders()
    {
        using var workspace = await Seed();
        const string path = ".agents/unknown/note.md";
        using var created = new CreatedFiles(workspace, ".agents/guidance/note.md");
        workspace.CreateFile(path, "# Note\n");
        using var doctor = await Diagnose(workspace);
        var unreachable = Finding(doctor, "route.unreachable", path);
        Assert.Equal("manual-decision", unreachable.GetProperty("resolution").GetString());
        Assert.Equal($"{path} is outside the folders Open Forge routes.", unreachable.GetProperty("message").GetString());
        Assert.Equal($"Move {path} into a category such as .agents/guidance/, or add its folder to .agents/loader.md.", Next(doctor));
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var text = await CliHostCapture.RunAsync(["doctor", "--detail", detail, "--workspace", workspace.Path], workspace.Path);
            Assert.Equal(1, text.Output.Split(Next(doctor), StringSplitOptions.None).Length - 1);
        }
        Assert.DoesNotContain(unreachable.GetProperty("actions").EnumerateArray(), action => action.GetProperty("command").GetString()?.StartsWith("open-forge index", StringComparison.Ordinal) == true);
        File.Move(workspace.Combine(path), workspace.Combine(".agents/guidance/note.md"));
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        using var clean = await Diagnose(workspace);
        Assert.DoesNotContain(clean.RootElement.GetProperty("findings").EnumerateArray(), IsHealthFinding);
    }

    [Theory(DisplayName = "Old navigation advises rename while canonical navigation is absent and copy-rules when present")]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task OldNavigationFile(bool hasEntries, bool canonicalExists)
    {
        using var workspace = await Seed();
        const string old = ".agents/guidance/invoices/_billing.md";
        const string canonical = ".agents/guidance/invoices/_invoices.md";
        using var created = new CreatedFiles(workspace, canonical);
        var body = "# Billing\n\n## Axioms\n\n- Keep these rules.\n";
        if (hasEntries) body += "\n## Entries\n\n- none - No entries - #Empty\n";
        workspace.CreateFile(old, Note("Billing rules", body));
        if (canonicalExists)
            workspace.CreateFile(canonical, Note("Invoices", "# Invoices\n\n## Axioms\n\n- Keep the current rules.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        workspace.CreateFile(".agents/guidance/invoices/note.md", "# Note\n");
        var indexCommand = $"open-forge index --workspace {CliCommandText.Quote(workspace.Path)}";
        var copyAction = $"Copy any rules you want to keep from {old} into {canonical}, then delete {old} and run {indexCommand}.";
        var renameAction = $"Rename {old} to _invoices.md to keep its rules, or delete it, then run {indexCommand}.";
        var before = workspace.SnapshotHashes();
        using (var first = await Diagnose(workspace))
        {
            var action = Finding(first, "route.unreachable", old).GetProperty("actions")[0].GetProperty("command").GetString();
            Assert.Equal(canonicalExists ? copyAction : renameAction, action);
            Assert.Equal(indexCommand, Next(first));
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index", "--workspace", workspace.Path], workspace.Path)).ExitCode);
        using var doctor = await Diagnose(workspace);
        var oldFinding = Finding(doctor, "route.unreachable", old);
        Assert.Equal($"{old} looks like this folder's old navigation file.", oldFinding.GetProperty("message").GetString());
        Assert.Equal("manual-decision", oldFinding.GetProperty("resolution").GetString());
        Assert.Equal(copyAction, Next(doctor));
        Assert.True(File.Exists(workspace.Combine(canonical)));
        var currentRule = canonicalExists ? "- Keep the current rules.\n" : string.Empty;
        Assert.Equal((FileAttributes)0, File.GetAttributes(workspace.Combine(canonical)) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
        await File.WriteAllTextAsync(workspace.Combine(canonical),
            Note("Invoices", $"# Invoices\n\n## Axioms\n\n{currentRule}- Keep these rules.\n\n## Entries\n\n- none - No entries - #Empty\n"), TestContext.Current.CancellationToken);
        File.Delete(workspace.Combine(old));
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index", "--workspace", workspace.Path], workspace.Path)).ExitCode);
        using var clean = await Diagnose(workspace);
        Assert.DoesNotContain(clean.RootElement.GetProperty("findings").EnumerateArray(), IsHealthFinding);
        Assert.Equal(JsonValueKind.Null, clean.RootElement.GetProperty("next").ValueKind);
        Assert.Contains("- Keep these rules.", File.ReadAllText(workspace.Combine(canonical)), StringComparison.Ordinal);
        if (canonicalExists) Assert.Contains("- Keep the current rules.", File.ReadAllText(workspace.Combine(canonical)), StringComparison.Ordinal);
    }

    private static async Task<TemporaryWorkspace> Seed()
    {
        var workspace = TemporaryWorkspace.Create("doctor maintenance's workspace");
        workspace.CreateFile("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.CreateFile(".agents/loader.md", Loader());
        workspace.CreateFile(".agents/guidance/_guidance.md", Root("- none - No entries - #Empty"));
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        return workspace;
    }

    private static string Loader() => GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
        .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.", StringComparison.Ordinal) + "\n";
    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], "\n" + body);
    private static string Root(string entries) => Note("Guidance", $"# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n{entries}\n");
    private static async Task<JsonDocument> Diagnose(TemporaryWorkspace workspace)
    {
        var run = await CliHostCapture.RunAsync(["doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(string.Empty, run.Error);
        return JsonDocument.Parse(run.Output);
    }
    private static JsonElement Finding(JsonDocument document, string code, string path)
        => Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == code && finding.GetProperty("subject").GetProperty("path").GetString() == path);
    private static string Next(JsonDocument document) => document.RootElement.GetProperty("next").GetProperty("command").GetString() ?? throw new InvalidOperationException();
    private static bool IsHealthFinding(JsonElement finding) => finding.GetProperty("severity").GetString() is "error" or "warning";

    private sealed class CreatedFiles(TemporaryWorkspace workspace, params string[] paths) : IDisposable
    {
        public void Dispose()
        {
            foreach (var path in paths.Select(path => workspace.Combine(path)))
            {
                if (!File.Exists(path)) continue;
                Assert.Equal((FileAttributes)0, File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
                File.Delete(path);
            }
        }
    }
}
