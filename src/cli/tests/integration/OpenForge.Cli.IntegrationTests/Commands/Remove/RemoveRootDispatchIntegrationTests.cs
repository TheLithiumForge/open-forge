using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveRootDispatchIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove is one direct root binding with the exact public syntax")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task RootBindingAndHelpAreDirect()
    {
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var remove = Assert.Single(tree.Root.Subcommands, command => command.Name == "remove");
        var selection = CliBindingSelector.Select(tree.Parse(["remove", "README.md", "--dry-run"]));

        Assert.Same(remove, selection.Command);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.NotNull(selection.Binding);
        Assert.Equal("remove", selection.Binding.Command.Name);

        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-help");
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(["remove", "--help"], output, error);
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Contains("open-forge remove <target>", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("--kind", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("--allow-path path", output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root path selection delegates routed Markdown to Route Remove and preserves root report identity")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task RoutedMarkdownDispatchesToRouteOperation()
    {
        using var workspace = SeedRoutedWorkspace("remove-root-route-dispatch");
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", "./.agents/guide/article.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.False(File.Exists(workspace.Combine(".agents/guide/article.md")), output.ToString());
        Assert.True(File.Exists(workspace.Combine(".agents/guide/unmanaged.md")));
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("file", document.RootElement.GetProperty("data").GetProperty("subject").GetString());
        Assert.Equal(".agents/guide/article.md", document.RootElement.GetProperty("data").GetProperty("source").GetProperty("path").GetString());

        var textOutput = new StringWriter();
        var textError = new StringWriter();
        var dryRun = await workspace.RunAsync(
            ["remove", ".agents/guide/unmanaged.md", "--automatic", "--dry-run"],
            textOutput,
            textError);
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Equal(string.Empty, textError.ToString());
        Assert.Contains("Would remove", textOutput.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("remove path", textOutput.ToString(), StringComparison.Ordinal);
        Assert.True(File.Exists(workspace.Combine(".agents/guide/unmanaged.md")));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove reconciles stale generated navigation for an already missing excluded route leaf")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingExcludedRouteLeafReconcilesNavigationAndThenNoOps()
    {
        using var workspace = SeedRoutedWorkspace("remove-root-missing-route-leaf");
        File.Delete(workspace.Combine(".agents/guide/article.md"));
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"schemaVersion\":1,\"removedFiles\":[\".agents/guide/article.md\"]}");
        var settingsBefore = File.ReadAllBytes(workspace.Combine(".agents/open-forge.json"));

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", ".agents/guide/article.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using (var report = JsonDocument.Parse(output.ToString()))
        {
            Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
            Assert.Equal("Reconciled removal of .agents/guide/article.md", report.RootElement.GetProperty("summary").GetProperty("headline").GetString());
            Assert.Empty(report.RootElement.GetProperty("findings").EnumerateArray());
            Assert.Empty(report.RootElement.GetProperty("data").GetProperty("removed").EnumerateArray());
            Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
                effect.GetProperty("path").GetString() == ".agents/guide/_guide.md"
                && effect.GetProperty("action").GetString() == "replaced"
                && effect.GetProperty("outcome").GetString() == "done");
        }
        Assert.False(File.Exists(workspace.Combine(".agents/guide/article.md")));
        var navigation = File.ReadAllText(workspace.Combine(".agents/guide/_guide.md"));
        Assert.DoesNotContain("article.md", navigation, StringComparison.Ordinal);
        Assert.Contains("unmanaged.md", navigation, StringComparison.Ordinal);
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Combine(".agents/open-forge.json")));

        var afterRepair = workspace.SnapshotHashes();
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        var repeated = await workspace.RunAsync(
            ["remove", ".agents/guide/article.md", "--automatic", "--format", "json"],
            output,
            error);
        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        using var repeatedReport = JsonDocument.Parse(output.ToString());
        Assert.Equal("No changes for .agents/guide/article.md", repeatedReport.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Empty(repeatedReport.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(afterRepair, workspace.SnapshotHashes());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root category directory maps to its entrypoint and an explicit entrypoint file never widens")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task CategoryAndEntrypointDispatchAreBounded()
    {
        using var workspace = SeedRoutedWorkspace("remove-root-entrypoint-guard");
        var output = new StringWriter();
        var error = new StringWriter();
        var blocked = await workspace.RunAsync(
            ["remove", ".agents/guide/_guide.md", "--kind", "path", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.True(File.Exists(workspace.Combine(".agents/guide/_guide.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guide/article.md")));
        using (var report = JsonDocument.Parse(output.ToString()))
        {
            Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
            Assert.Equal("remove.entry-point-requires-route", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
            Assert.Contains("\".agents/guide\" --kind route", report.RootElement.GetProperty("next").GetProperty("command").GetString(), StringComparison.Ordinal);
        }

        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        var category = await workspace.RunAsync(
            ["remove", ".agents/guide", "--automatic", "--dry-run", "--format", "json"],
            output,
            error);
        Assert.Equal(0, category.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, category.Status);
        using var categoryReport = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", categoryReport.RootElement.GetProperty("command").GetString());
        Assert.Equal("route", categoryReport.RootElement.GetProperty("data").GetProperty("subject").GetString());
        Assert.Equal(".agents/guide/_guide.md", categoryReport.RootElement.GetProperty("data").GetProperty("source").GetProperty("path").GetString());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Root typed Extension and Library dispatch preserve remove identity and family data")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    [InlineData("extension", "sample-package", "packages")]
    [InlineData("library", "sample-library", "id")]
    public async Task TypedManagerDispatchUsesRemoveReport(string kind, string target, string familyData)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create($"remove-root-{kind}-dispatch");
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", target, "--kind", kind, "--dry-run", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", document.RootElement.GetProperty("command").GetString());
        Assert.True(document.RootElement.GetProperty("data").TryGetProperty(familyData, out _));
        Assert.Equal("dry-run", document.RootElement.GetProperty("data").GetProperty("mode").GetString());
        Assert.True(completion.Status is CliSemanticStatus.Complete or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete);

        var textOutput = new StringWriter();
        var textError = new StringWriter();
        var textCompletion = await workspace.RunAsync(
            ["remove", target, "--kind", kind, "--dry-run", "--automatic"],
            textOutput,
            textError);

        Assert.Equal(completion.Status, textCompletion.Status);
        Assert.Equal(string.Empty, textError.ToString());
        Assert.Contains(target, textOutput.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root route report uses the remove command name in noninteractive JSON and preserves prompt policy")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task JsonRouteRequestDoesNotPromptAndKeepsRootIdentity()
    {
        using var workspace = SeedRoutedWorkspace("remove-root-json-confirmation");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", ".agents/guide/article.md", "--format", "json"],
            output,
            error);

        Assert.True(completion.ExitCode == 4, output.ToString());
        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
        Assert.Equal("route-remove.confirmation-required", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root path removal updates a remaining native Skill navigation region")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task NativeSkillPathProjectsGeneratedNavigation()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-skill-navigation");
        workspace.WriteText(".agents/skills/_skills.md", OpenForgeDocumentSeed.SkillEntrypoint(["native-skill"]));
        workspace.WriteText(".agents/skills/native-skill/SKILL.md", OpenForgeDocumentSeed.Skill("native-skill", "Native fixture skill."));
        var originalNavigation = File.ReadAllBytes(workspace.Combine(".agents/skills/_skills.md"));

        var output = new StringWriter();
        var error = new StringWriter();
        var preview = await workspace.RunAsync(
            ["remove", ".agents/skills/native-skill/SKILL.md", "--dry-run", "--automatic", "--format", "json"],
            output,
            error);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(originalNavigation, File.ReadAllBytes(workspace.Combine(".agents/skills/_skills.md")));
        using (var report = JsonDocument.Parse(output.ToString()))
        {
            Assert.Equal("path", report.RootElement.GetProperty("data").GetProperty("kind").GetString());
            Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
                effect.GetProperty("path").GetString() == ".agents/skills/_skills.md"
                && effect.GetProperty("action").GetString() == "replaced");
        }

        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        var applied = await workspace.RunAsync(
            ["remove", ".agents/skills/native-skill/SKILL.md", "--automatic", "--format", "json"],
            output,
            error);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.False(File.Exists(workspace.Combine(".agents/skills/native-skill/SKILL.md")));
        var afterNavigation = File.ReadAllText(workspace.Combine(".agents/skills/_skills.md"));
        Assert.DoesNotContain("native-skill/SKILL.md", afterNavigation, StringComparison.Ordinal);
        Assert.Contains("- none - No entries - #Empty", afterNavigation, StringComparison.Ordinal);
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root route removal releases a managed source and removes an unmanaged sibling independently")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ManagedAndUnmanagedRouteSelection()
    {
        using var workspace = SeedRoutedWorkspace("remove-root-managed-route", managedArticle: true);

        var output = new StringWriter();
        var error = new StringWriter();
        var managed = await workspace.RunAsync(
            ["remove", ".agents/guide/article.md", "--automatic", "--format", "json"],
            output,
            error);
        Assert.Equal(0, managed.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, managed.Status);
        Assert.False(File.Exists(workspace.Combine(".agents/guide/article.md")), output.ToString());
        Assert.True(File.Exists(workspace.Combine(".agents/guide/unmanaged.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guide/_guide.md")));
        using (var report = JsonDocument.Parse(output.ToString()))
        {
            Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
            Assert.Empty(report.RootElement.GetProperty("findings").EnumerateArray());
        }
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            Assert.Contains(settings.RootElement.GetProperty("removedFiles").EnumerateArray(), path =>
                path.GetString() == ".agents/guide/article.md");
        }
        var managedOwnership = WorkspaceOwnershipCodec.Read(
            File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json")));
        var managedOwnershipDocument = Assert.IsType<WorkspaceOwnershipDocument>(managedOwnership.Document);
        Assert.DoesNotContain(".agents/guide/article.md", managedOwnershipDocument.ManagedPaths().Select(claim => claim.Path));

        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        var unmanaged = await workspace.RunAsync(
            ["remove", ".agents/guide/unmanaged.md", "--automatic", "--format", "json"],
            output,
            error);
        Assert.True(unmanaged.ExitCode == 0, output.ToString());
        Assert.Equal(CliSemanticStatus.Complete, unmanaged.Status);
        Assert.False(File.Exists(workspace.Combine(".agents/guide/unmanaged.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/guide/article.md")));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            var removedFiles = settings.RootElement.GetProperty("removedFiles").EnumerateArray()
                .Select(path => path.GetString()).ToArray();
            Assert.Contains(".agents/guide/article.md", removedFiles);
            Assert.Contains(".agents/guide/unmanaged.md", removedFiles);
        }
        var finalOwnership = WorkspaceOwnershipCodec.Read(
            File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json")));
        var finalOwnershipDocument = Assert.IsType<WorkspaceOwnershipDocument>(finalOwnership.Document);
        Assert.DoesNotContain(".agents/guide/article.md", finalOwnershipDocument.ManagedPaths().Select(claim => claim.Path));
        Assert.DoesNotContain(".agents/guide/unmanaged.md", finalOwnershipDocument.ManagedPaths().Select(claim => claim.Path));
        Assert.Equal(string.Empty, error.ToString());
    }

    private static RemoveRootIntegrationWorkspace SeedRoutedWorkspace(string purpose, bool managedArticle = false)
    {
        var workspace = RemoveRootIntegrationWorkspace.Create(purpose);
        try
        {
            workspace.WriteText(".agents/loader.md", Entrypoint("Loader", "- [Guide](guide/_guide.md) - #Route"));
            workspace.WriteText(".agents/guide/_guide.md", Entrypoint("Guide", "- [Article](article.md) - #Route\n- [Unmanaged](unmanaged.md) - #Route"));
            workspace.WriteText(".agents/guide/article.md", OpenForgeDocumentSeed.Metadata("Article", ["Route"], "# Article\n\nBody.\n"));
            workspace.WriteText(".agents/guide/unmanaged.md", OpenForgeDocumentSeed.Metadata("Unmanaged", ["Route"], "# Unmanaged\n\nBody.\n"));
            workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), managedArticle ? [".agents/guide/article.md"] : [], []),
            }));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static string Entrypoint(string title, string entries)
        => OpenForgeDocumentSeed.Metadata(
            title,
            ["Route"],
            $"# {title}\n\n{OpenForgeDocumentSeed.GeneratedEntries(entries)}");
}
