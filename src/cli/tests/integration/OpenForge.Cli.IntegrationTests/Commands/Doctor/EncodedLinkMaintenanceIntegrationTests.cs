using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "reference-encoding"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class EncodedLinkMaintenanceIntegrationTests
{
    [Fact(DisplayName = "Index generated encoded links are canonical in managed and user-owned entrypoints")]
    public async Task IndexedLinksAreCanonical()
    {
        using var workspace = await SeedAsync(managedEntrypoint: true);
        Assert.Contains("(old%20guide.md)", File.ReadAllText(workspace.Combine(".agents", "guidance", "_guidance.md")), StringComparison.Ordinal);
        Assert.Contains("(spaced%20topic.md)", File.ReadAllText(workspace.Combine(".agents", "guidance", "topics", "_topics.md")), StringComparison.Ordinal);
        var before = workspace.SnapshotHashes();
        using var doctor = await DoctorAsync(workspace);
        Assert.Empty(References(doctor));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Repair applies every automatic path encoding and heading correction offered by Doctor")]
    public async Task AutomaticCorrectionsAgree()
    {
        using var workspace = await SeedAsync();
        var authored = Note("Map", "# Map\n\n[Encoding](%6Fld%20guide.md)\n[Heading](old%20guide.md#HEADING)\n[Path](./old%20guide.md)\n[Combined](./%6Fld%20guide.md#HEADING)\n[Angle](<old guide.md>)\n");
        workspace.ReplaceText(".agents/guidance/map.md", authored);
        using (var doctor = await DoctorAsync(workspace))
        {
            var offered = References(doctor);
            Assert.Equal(5, offered.Length);
            Assert.All(offered, finding => Assert.Equal("safe-exact", finding.GetProperty("resolution").GetString()));
            Assert.All(offered, finding => Assert.Contains(finding.GetProperty("actions").EnumerateArray(),
                action => action.GetProperty("command").GetString() == "open-forge repair --automatic"));
            Assert.Contains(offered, finding => finding.GetProperty("message").GetString() ==
                "%6Fld%20guide.md uses a different encoding than the canonical: old%20guide.md.");
        }
        var repaired = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(0, repaired.ExitCode);
        Assert.Equal(authored.Replace("%6F", "o", StringComparison.Ordinal).Replace("./", "", StringComparison.Ordinal)
            .Replace("#HEADING", "#heading", StringComparison.Ordinal),
            File.ReadAllText(workspace.Combine(".agents", "guidance", "map.md")));
        using var clean = await DoctorAsync(workspace);
        Assert.Empty(References(clean));
        var repeated = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(0, repeated.ExitCode);
        Assert.Contains("Nothing to repair.", repeated.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Protected canonicalization keeps its own finding identity and never promises Repair or Update")]
    public async Task ProtectedCorrectionKeepsIdentity()
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Guide](%6Fld%20guide.md)\n"));
        Protect(workspace, ".agents/guidance/map.md");
        var before = workspace.SnapshotHashes();
        using var doctor = await DoctorAsync(workspace);
        var finding = Assert.Single(References(doctor));
        Assert.Equal("reference.same-target-encoding", finding.GetProperty("code").GetString());
        Assert.Equal("Equivalent link encoding is available", finding.GetProperty("title").GetString());
        Assert.Equal("info", finding.GetProperty("severity").GetString());
        Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        Assert.Equal("%6Fld%20guide.md uses a different encoding than the canonical: old%20guide.md.", finding.GetProperty("message").GetString());
        Assert.Equal("Correct this link in .agents/guidance/map.md at line 8, column 1 through the owner of .agents/guidance/map.md.",
            Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "A missing heading prevents Doctor from offering a path fix that Repair cannot apply")]
    public async Task UnresolvedFragmentHasNoAutomaticOffer()
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Guide](%6Fld%20guide.md#missing)\n"));
        using var doctor = await DoctorAsync(workspace);
        Assert.DoesNotContain(References(doctor), finding => finding.GetProperty("resolution").GetString() == "safe-exact");
        Assert.Equal("reference.fragment-missing", Assert.Single(References(doctor)).GetProperty("code").GetString());
    }

    [Fact(DisplayName = "Doctor directs generated Entries spelling to Index and never offers it to Repair")]
    public async Task UserEntrypointCorrectionBelongsToIndex()
    {
        using var workspace = await SeedAsync();
        var path = workspace.Combine(".agents", "guidance", "topics", "_topics.md");
        var canonical = File.ReadAllText(path);
        workspace.ReplaceText(".agents/guidance/topics/_topics.md", canonical.Replace("spaced%20topic.md", "%73paced%20topic.md", StringComparison.Ordinal));
        using (var doctor = await DoctorAsync(workspace))
        {
            Assert.Empty(References(doctor));
            Assert.Contains(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("actions").EnumerateArray().Any(action => action.GetProperty("command").GetString() == "open-forge index .agents/guidance/topics/_topics.md"));
        }
        var index = await CliHostCapture.RunAsync(["index"], workspace.Path);
        Assert.Equal(0, index.ExitCode);
        Assert.Equal(canonical, File.ReadAllText(path));
        using var clean = await DoctorAsync(workspace);
        Assert.Empty(References(clean));
    }

    [Fact(DisplayName = "Image spelling observations never offer automatic repairs outside Repair's authored-link catalogue")]
    public async Task ImageSpellingHasNoAutomaticOffer()
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n![Guide](%6Fld%20guide.md)\n"));
        using var doctor = await DoctorAsync(workspace);
        Assert.Empty(References(doctor));
        Assert.Equal(1, doctor.RootElement.GetProperty("counts").GetProperty("imageLinks").GetInt32());
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(0, repair.ExitCode);
        Assert.Contains("Nothing to repair.", repair.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Readable Unicode parentheses and angle-enclosed spaces produce no encoding findings")]
    public async Task LiteralSpellingsStayUnchanged()
    {
        using var workspace = await SeedAsync();
        workspace.WriteText(".agents/guidance/café(one).md", Note("Cafe", "# Cafe\n"));
        workspace.WriteText(".agents/guidance/café#%.md", Note("Hash percent", "# Hash percent\n"));
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Spaces](<old guide.md>)\n[Literal](café(one).md)\n[Encoded](caf%C3%A9%28one%29.md)\n[Reserved](café%23%25.md)\n"));
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var before = workspace.SnapshotHashes();
        using var doctor = await DoctorAsync(workspace);
        Assert.Empty(References(doctor));
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(0, repair.ExitCode);
        Assert.Contains("Nothing to repair.", repair.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Index accepts hash filenames and Repair applies Doctor's exact encoding offer")]
    public async Task HashFilenameCanBeIndexedAndRepaired()
    {
        using var workspace = await SeedAsync();
        workspace.WriteText(".agents/guidance/hash#name.md", Note("Hash", "# Hash\n"));
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Guide](%68ash%23name.md)\n"));
        using (var doctor = await DoctorAsync(workspace))
        {
            var finding = Assert.Single(References(doctor));
            Assert.Equal("reference.same-target-encoding", finding.GetProperty("code").GetString());
            Assert.Equal("safe-exact", finding.GetProperty("resolution").GetString());
            Assert.Equal("%68ash%23name.md uses a different encoding than the canonical: hash%23name.md.", finding.GetProperty("message").GetString());
        }
        Assert.Equal(0, (await CliHostCapture.RunAsync(["repair", "--automatic", "--dry-run"], workspace.Path)).ExitCode);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path)).ExitCode);
        Assert.Contains("[Guide](hash%23name.md)", File.ReadAllText(workspace.Combine(".agents", "guidance", "map.md")), StringComparison.Ordinal);
        Assert.Contains("(hash%23name.md)", File.ReadAllText(workspace.Combine(".agents", "guidance", "_guidance.md")), StringComparison.Ordinal);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        using var clean = await DoctorAsync(workspace);
        Assert.Empty(References(clean));
    }

    [Theory(DisplayName = "Image heading corrections remain manual in ordinary and protected sources"), InlineData(false), InlineData(true)]
    public async Task ImageFragmentRemainsManual(bool managed)
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n![Guide](old%20guide.md#HEADING)\n"));
        if (managed) Protect(workspace, ".agents/guidance/map.md");
        var before = workspace.SnapshotHashes();
        using var doctor = await DoctorAsync(workspace);
        var finding = Assert.Single(References(doctor));
        Assert.Equal("reference.fragment-missing", finding.GetProperty("code").GetString());
        Assert.Equal("Linked heading was not found", finding.GetProperty("title").GetString());
        Assert.Equal("old%20guide.md has no heading #HEADING. No possible target was found.", finding.GetProperty("message").GetString());
        Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        Assert.DoesNotContain(finding.GetProperty("actions").EnumerateArray(), action => action.GetProperty("command").GetString() == "open-forge repair --automatic");
        Assert.Equal(managed
                ? "Correct this link in .agents/guidance/map.md at line 8, column 1 through the owner of .agents/guidance/map.md."
                : "Fix the link by hand.",
            Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(2, repair.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Managed source ownership alone never promises that Update fixes a broken local link"),
        InlineData("missing.md", "reference.target-missing", "Broken link"),
        InlineData("old%20guide.md#missing", "reference.fragment-missing", "Linked heading was not found")]
    public async Task ManagedSourceDoesNotInventUpdate(string destination, string code, string title)
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", $"# Map\n\n[Guide]({destination})\n"));
        Protect(workspace, ".agents/guidance/map.md");
        using var doctor = await DoctorAsync(workspace);
        var finding = Assert.Single(References(doctor));
        Assert.Equal(code, finding.GetProperty("code").GetString());
        Assert.Equal(title, finding.GetProperty("title").GetString());
        Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        Assert.Equal("Correct this link in .agents/guidance/map.md at line 8, column 1 through the owner of .agents/guidance/map.md.",
            Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
    }

    [Theory(DisplayName = "Decoded Markdown entity destinations keep heading problems manual without rewriting nonliteral bytes"),
        InlineData("", 0), InlineData("#HEADING", 1)]
    public async Task NonliteralDestinationHasNoAutomaticOffer(string fragment, int findings)
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", $"# Map\n\n[Guide](old&#32;guide.md{fragment})\n"));
        var before = workspace.SnapshotHashes();
        using var doctor = await DoctorAsync(workspace);
        Assert.Equal(findings, References(doctor).Length);
        if (findings != 0)
        {
            var finding = Assert.Single(References(doctor));
            Assert.Equal("reference.fragment-missing", finding.GetProperty("code").GetString());
            Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        }
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(findings == 0 ? 0 : 2, repair.ExitCode);
        if (findings == 0) Assert.Contains("Nothing to repair.", repair.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Accepted entity filename spellings resolve to one intended file when the decoded alias also exists"),
        InlineData("entity&amp;.md", "entity%26amp;.md", "entity&.md"),
        InlineData("entity&#65;.md", "entity&%2365;.md", "entityA.md"),
        InlineData("entity&#x41;.md", "entity&%23x41;.md", "entityA.md")]
    public async Task EntitySpellingsAgreeWithReferences(string filename, string literal, string alias)
    {
        using var workspace = await SeedAsync();
        workspace.WriteText($".agents/guidance/{filename}", Note("Entity", "# Entity\n"));
        workspace.WriteText($".agents/guidance/{alias}", Note("Alias", "# Alias\n"));
        var encoded = Uri.EscapeDataString(filename);
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", $"# Map\n\n[Index](<{encoded}>)\n[Literal](<{literal}>)\n"));
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var before = workspace.SnapshotHashes();
        var references = await CliHostCapture.RunAsync(["references", "guidance/map", "--direction", "out", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(0, references.ExitCode);
        using var parsed = JsonDocument.Parse(references.Output);
        var outgoing = parsed.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray().ToArray();
        Assert.Equal(2, outgoing.Length);
        Assert.All(outgoing, link => Assert.Equal($".agents/guidance/{filename}", link.GetProperty("resolvedPath").GetString()));
        Assert.All(outgoing, link => Assert.Equal("complete", link.GetProperty("state").GetString()));
        using var doctor = await DoctorAsync(workspace);
        Assert.Empty(References(doctor));
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], workspace.Path);
        Assert.Equal(0, repair.ExitCode);
        Assert.Contains("Nothing to repair.", repair.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<JsonDocument> DoctorAsync(TemporaryWorkspace workspace)
    {
        var run = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(string.Empty, run.Error);
        return JsonDocument.Parse(run.Output);
    }

    private static JsonElement[] References(JsonDocument document)
        => document.RootElement.GetProperty("findings").EnumerateArray()
            .Where(finding => finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true).ToArray();

    private static async Task<TemporaryWorkspace> SeedAsync(bool managedEntrypoint = false)
    {
        var workspace = TemporaryWorkspace.Create("encoded-link-maintenance");
        workspace.WriteText("AGENTS.md", "Read `.agents/loader.md`.\n");
        workspace.WriteText(".agents/loader.md", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        workspace.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n"));
        workspace.WriteText(".agents/guidance/old guide.md", Note("Old guide", "# Old guide\n\n## Heading\n"));
        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n"));
        workspace.WriteText(".agents/guidance/topics/_topics.md", Note("Topics", "# Topics\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n"));
        workspace.WriteText(".agents/guidance/topics/spaced topic.md", Note("Spaced topic", "# Spaced topic\n"));
        if (managedEntrypoint) Protect(workspace, ".agents/guidance/_guidance.md");
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        return workspace;
    }

    private static void Protect(TemporaryWorkspace workspace, string path)
        => workspace.WriteText(".agents/open-forge.lock.json", $"{{\"schemaVersion\":1,\"framework\":{{\"source\":{{\"id\":\"builtin\",\"version\":null}},\"paths\":[\"{path}\"],\"regions\":[]}},\"extensions\":[],\"libraries\":[]}}");

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], body);
}
