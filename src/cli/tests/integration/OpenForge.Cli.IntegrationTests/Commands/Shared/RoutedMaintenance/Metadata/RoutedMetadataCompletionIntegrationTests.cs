using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance.Metadata;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Integration")]
public sealed class RoutedMetadataCompletionIntegrationTests
{
    [Theory(DisplayName = "Filesystem route facts retain every category and scoped Memory state default"), Trait("Boundary", "OS")]
    [InlineData("directives", "", "LoadNow,Directive")]
    [InlineData("guidance", "", "Guidance")]
    [InlineData("maps", "", "Map")]
    [InlineData("patterns", "", "Pattern")]
    [InlineData("templates", "", "Template")]
    [InlineData("skills", "", "Skill")]
    [InlineData("extensions", "", "Extension")]
    [InlineData("memory", "", "Memory")]
    [InlineData("memory", "working", "Memory,Working,Contextual")]
    [InlineData("memory", "emerging", "Memory,Emerging,Contextual")]
    [InlineData("memory", "crystallized", "Memory,Crystallized")]
    [InlineData("memory", "archived", "Memory,Archived,Contextual,Historical")]
    [InlineData("custom", "guidance", "Workspace")]
    public async Task CompletesRecognizedRoutes(string root, string state, string expected)
    {
        using var temporary = TemporaryWorkspace.Create("routed-metadata-defaults");
        var rootPath = $".agents/{root}/_{root}.md";
        var directory = $".agents/{root}/team";
        var ancestors = new List<string> { rootPath };
        temporary.CreateFile(".agents/loader.md", $"# Loader\n\n## Entries\n\n- [Root]({root}/_{root}.md)\n");
        temporary.CreateFile(rootPath, "# Root\n");
        if (state.Length > 0)
        {
            var statePath = $"{directory}/{state}/_{state}.md";
            temporary.CreateFile(statePath, "# State\n");
            ancestors.Add(statePath);
            directory += $"/{state}/deep";
        }

        var canonicalPath = $"{directory}/moneyRounding.md";
        var path = temporary.CreateFile(canonicalPath, "Body without a heading\n");
        temporary.CreateFile(".agents/open-forge.json", "{\"schemaVersion\":1,\"frontmatter\":\"root\"}");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsReadState.Complete, settings.State);
        var context = new RoutedMetadataPolicyContext { Settings = settings.Document, Ownership = WorkspaceOwnershipDocument.Empty, NativeSkillPaths = [] };
        var input = new RoutedMetadataCompletionInput
        {
            File = new()
            {
                CanonicalPath = canonicalPath,
                LoaderRootEntrypointPath = rootPath,
                RouteEntrypointPaths = [.. ancestors],
                IsSelected = true,
                IsReadable = true,
                IsSafeOrdinaryLeaf = true,
                IsGeneratedFile = false,
                HasManagedAuthoredRegions = false,
            },
            Bytes = [.. await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)],
        };
        var before = temporary.SnapshotHashes();
        var result = new RoutedMetadataCompletionPolicy().Complete(input, context);
        Assert.Equal(RoutedMetadataEligibility.Eligible, result.Eligibility);
        Assert.Equal("Money rounding", result.Completion.Description);
        Assert.Equal(expected.Split(','), result.Completion.Tags?.ToArray());
        Assert.Equal(before, temporary.SnapshotHashes());
        var after = Encoding.UTF8.GetString(result.Completion.Bytes.AsSpan());
        Assert.StartsWith("---\ndescription: Money rounding\n", after, StringComparison.Ordinal);
        Assert.Null(result.FactCode);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp,
            new RoutedMetadataCompletionPolicy().Complete(input with { Bytes = result.Completion.Bytes }, context).Completion.State);
    }

    [Theory(DisplayName = "Real settings exclusions and sharing intent prevent ordinary completion without Git"), Trait("Boundary", "OS")]
    [InlineData("removedFiles", ".agents/guidance/note.md", false)]
    [InlineData("removedDirectories", ".agents/guidance", false)]
    [InlineData("removedCategories", "guidance", false)]
    [InlineData("removedFiles", ".agents/other.md", true)]
    public async Task RespectsPersistentPolicy(string key, string excluded, bool privateRoute)
    {
        using var temporary = TemporaryWorkspace.Create("routed-metadata-exclusions");
        var path = temporary.CreateFile(".agents/guidance/note.md", "# Note\nBody\n");
        temporary.CreateFile(".agents/open-forge.json", $"{{\"schemaVersion\":1,\"{key}\":[\"{excluded}\"]}}");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsReadState.Complete, settings.State);
        var ownership = WorkspaceOwnershipDocument.Empty;
        if (privateRoute)
            ownership = ownership with { Framework = new(new("framework", null), [], []) { GitIgnoredRoutes = [new(".agents/guidance", ".agents/guidance/_guidance.md")] } };
        var context = new RoutedMetadataPolicyContext { Settings = settings.Document, Ownership = ownership, NativeSkillPaths = [] };
        var input = new RoutedMetadataCompletionInput
        {
            File = new()
            {
                CanonicalPath = ".agents/guidance/note.md",
                LoaderRootEntrypointPath = ".agents/guidance/_guidance.md",
                RouteEntrypointPaths = [".agents/guidance/_guidance.md"],
                IsSelected = true,
                IsReadable = true,
                IsSafeOrdinaryLeaf = true,
                IsGeneratedFile = false,
                HasManagedAuthoredRegions = false,
            },
            Bytes = [.. await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)],
        };
        var before = temporary.SnapshotHashes();
        var result = new RoutedMetadataCompletionPolicy().Complete(input, context);
        Assert.Equal(privateRoute ? RoutedMetadataEligibility.PrivateContent : RoutedMetadataEligibility.SettingsExcluded, result.Eligibility);
        Assert.Equal(input.Bytes, result.Completion.Bytes);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(Directory.Exists(temporary.Combine(".git")));
    }
}
