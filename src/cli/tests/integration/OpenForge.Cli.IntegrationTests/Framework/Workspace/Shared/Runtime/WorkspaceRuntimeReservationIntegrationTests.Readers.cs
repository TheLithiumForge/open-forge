using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    [Theory(DisplayName = "Registered runtime files are rejected while other registrations and their children are read")]
    [InlineData(RuntimeFile), InlineData("runtime-alias/recovery/v1/history.md"), InlineData("runtime-alias.md")]
    [InlineData(".open-forge-state/linked-safe.md")]
    public async Task RegisteredRuntimeFilesAreRejected(string target)
    {
        using var files = TemporaryWorkspace.Create("runtime-registered");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        files.ReplaceText(".agents/open-forge.json", $$"""{"schemaVersion":2,"roots":["{{target}}","safe.md"]}""");
        files.ReplaceText(".agents/loader.md", $"# Loader\n\n## Entries\n\n- [Runtime](../{target}) - #LoadNow\n- [Safe](../safe.md) - #LoadNow\n");
        files.ReplaceText("safe.md", "# Safe\n\n## Entries\n\n- [Child](safe-child.md) - #LoadNow\n");
        files.CreateFile("safe-child.md", "# Safe child\n");
        var workspace = Workspace(files);
        var paths = new PhysicalPathResolver();
        var inspection = await new RouteSourceInspector(new SourceReadSessionReader(paths), paths).ReadAsync(workspace, Token);

        var result = await new RouteIncludedFileReader(workspace).ReadAsync(inspection, Token);

        Assert.NotNull(result);
        var rejected = Assert.Single(result.Files, file => file.Path == target);
        Assert.True(rejected.Registered);
        Assert.True(rejected.Listed);
        Assert.Equal("The included file is inside the reserved runtime directory.", rejected.Rejection);
        var safe = Assert.Single(result.Files, file => file.Path == "safe.md");
        Assert.True(safe.Registered);
        Assert.Null(safe.Rejection);
        var child = Assert.Single(result.Files, file => file.Path == "safe-child.md");
        Assert.True(child.FileExists);
        Assert.Null(child.Rejection);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
    }

    [Fact(DisplayName = "Included overwrite companions cannot read runtime bytes through a file link")]
    public async Task IncludedRuntimeCompanionIsOmitted()
    {
        using var files = TemporaryWorkspace.Create("runtime-companion");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.ReplaceText(".agents/open-forge.json", "{\"schemaVersion\":2,\"roots\":[\"safe.md\"]}");
        files.ReplaceText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Safe](../safe.md) - #LoadNow\n");
        const string content = "# Safe\n";
        files.CreateFile("safe.md", content);
        Assert.True(files.TryCreateFileSymbolicLink("safe.overwrite.md", files.Combine(RuntimeFile), out var cause), cause);
        var workspace = Workspace(files);
        var paths = new PhysicalPathResolver();
        var inspection = await new RouteSourceInspector(new SourceReadSessionReader(paths), paths).ReadAsync(workspace, Token);

        var result = await new RouteIncludedFileReader(workspace).ReadAsync(inspection, Token);

        Assert.NotNull(result);
        Assert.Equal(1L, result.Total.Files.Value);
        Assert.Equal(content.Length, result.Total.Utf8Bytes.Value);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
    }

    [Theory(DisplayName = "Reference fragments reject runtime targets before acquiring any target bytes")]
    [InlineData(RuntimeFile), InlineData("runtime-alias/recovery/v1/history.md"), InlineData("runtime-alias.md")]
    [InlineData(".open-forge-state/linked-safe.md")]
    public async Task ReferenceFragmentsNeverReadRuntime(string target)
    {
        using var files = TemporaryWorkspace.Create("runtime-fragment");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        var workspace = Workspace(files);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(workspace, Token);
        var reads = new List<string>();
        var resolver = new SourceLinkDestinationResolver(new PhysicalPathResolver(), (physical, logical, token) =>
        {
            reads.Add(logical);
            return StrictUtf8FileReader.ReadAsync(physical, logical, token);
        }, new MarkdownDocumentParser().Parse);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = ".agents/root/_root.md",
            RawDestination = $"../../{target}#history",
        };

        var result = await resolver.ResolveAsync(input, Token);

        Assert.Equal(SourceLinkTargetResolution.Unreadable, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, result.Finding?.Code);
        Assert.Equal("The local destination is inside the reserved runtime directory.", result.Finding?.Cause);
        Assert.Null(result.Target.PhysicalPath);
        Assert.Empty(reads);
        var safe = await resolver.ResolveAsync(input with { RawDestination = "../../safe.md#safe" }, Token);
        Assert.Equal(SourceLinkTargetResolution.Complete, safe.Target.Resolution);
        Assert.Equal(["safe.md"], reads);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
    }

    [Theory(DisplayName = "Context leaves runtime links unresolved and still expands ordinary links")]
    [InlineData(RuntimeFile, ""), InlineData(RuntimeFile, "#history")]
    [InlineData("runtime-alias/recovery/v1/history.md", ""), InlineData("runtime-alias/recovery/v1/history.md", "#history")]
    [InlineData("runtime-alias.md", ""), InlineData("runtime-alias.md", "#history")]
    public async Task ContextDoesNotExpandRuntimeLinks(string target, string fragment)
    {
        using var files = TemporaryWorkspace.Create("runtime-context");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        files.CreateFile("AGENTS.md", "Read `.agents/loader.md`.\n");
        files.ReplaceText(".agents/root/leaf.md", $"---\nopen-forge:\n  description: Leaf\n  tags: [Docs]\n---\n# Leaf\n\n[History](../../{target}{fragment})\n\n[Safe](../../safe.md)\n");

        var result = await CliHostCapture.RunAsync(["context", "root/leaf", "--follow-links", "all", "--format", "json"], files.Path);

        Assert.Equal(3, result.ExitCode);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "context.target-unreadable");
        Assert.DoesNotContain("RUNTIME-CONTENT-PROBE", result.Output, StringComparison.Ordinal);
        Assert.Contains("SAFE-CONTENT-PROBE", result.Output, StringComparison.Ordinal);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
    }

    private static void SeedRuntimeLinks(TemporaryWorkspace files)
    {
        files.CreateFile(RuntimeFile, "# History\n\nRUNTIME-CONTENT-PROBE\n");
        files.CreateFile("safe.md", "# Safe\n\nSAFE-CONTENT-PROBE\n");
        Assert.True(files.TryCreateDirectorySymbolicLink("runtime-alias", files.Combine(".open-forge-state"), out var directoryCause), directoryCause);
        Assert.True(files.TryCreateFileSymbolicLink("runtime-alias.md", files.Combine(RuntimeFile), out var fileCause), fileCause);
        Assert.True(files.TryCreateFileSymbolicLink(".open-forge-state/linked-safe.md", files.Combine("safe.md"), out var lexicalCause), lexicalCause);
    }
}
