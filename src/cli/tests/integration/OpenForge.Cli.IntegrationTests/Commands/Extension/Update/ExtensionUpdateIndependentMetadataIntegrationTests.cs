using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateIndependentMetadataIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update completes a selected current preview beside unrelated malformed metadata"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task UnrelatedMalformedMetadataDoesNotBlockSelectedNoOpPreview()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-independent-metadata-no-op");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-independent-metadata-no-op-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        await InstallAsync(workspace, source, "toolkit");

        var parentPath = ".agents/patterns/_patterns.md";
        var parentBytes = File.ReadAllBytes(workspace.Combine(parentPath));
        const string notePath = ".agents/patterns/old-note.md";
        var noteBytes = System.Text.Encoding.UTF8.GetBytes(MalformedDocument("Old note"));
        workspace.CreateOccupant(notePath, MalformedDocument("Old note"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(parentBytes, File.ReadAllBytes(workspace.Combine(parentPath)));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(notePath)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update applies a selected package version beside unrelated malformed metadata"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task UnrelatedMalformedMetadataDoesNotBlockSelectedVersionUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-independent-metadata-version");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-independent-metadata-version-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit v1", "# Toolkit v1\n")));
        await InstallAsync(workspace, source, "toolkit");

        var parentPath = ".agents/patterns/_patterns.md";
        var parentBytes = File.ReadAllBytes(workspace.Combine(parentPath));
        const string notePath = ".agents/patterns/old-note.md";
        var noteBytes = System.Text.Encoding.UTF8.GetBytes(MalformedDocument("Old note"));
        workspace.CreateOccupant(notePath, MalformedDocument("Old note"));
        source.SetVersion("toolkit", "2.0.0");
        source.ReplacePayload("toolkit", ".agents/guidance/toolkit.md", Document("Toolkit v2", "# Toolkit v2\n"));
        var beforeDryRun = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var dryRun = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Equal(string.Empty, dryRun.StandardError);
        using (var dryRunDocument = JsonDocument.Parse(dryRun.StandardOutput))
        {
            Assert.NotEmpty(dryRunDocument.RootElement.GetProperty("effects").EnumerateArray());
            Assert.All(
                dryRunDocument.RootElement.GetProperty("effects").EnumerateArray(),
                effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));
        }

        Assert.Equal(beforeDryRun, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(parentBytes, File.ReadAllBytes(workspace.Combine(parentPath)));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(notePath)));

        var apply = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, apply.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, apply.Status);
        Assert.Equal(string.Empty, apply.StandardError);
        using (var applyDocument = JsonDocument.Parse(apply.StandardOutput))
        {
            Assert.NotEmpty(applyDocument.RootElement.GetProperty("effects").EnumerateArray());
        }

        Assert.Equal(Document("Toolkit v2", "# Toolkit v2\n"), workspace.ReadText(".agents/guidance/toolkit.md"));
        Assert.Equal(parentBytes, File.ReadAllBytes(workspace.Combine(parentPath)));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(notePath)));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update refuses malformed selected and direct dependency metadata without writes"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData("selected")]
    [InlineData("dependency")]
    public async Task MalformedAffectedMetadataIsWriteFree(string kind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-independent-metadata-{kind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-independent-metadata-{kind}-source");
        source.AddPackage(
            "base",
            [],
            (".agents/guidance/base.md", Document("Base v1", "# Base v1\n")));
        source.AddPackage(
            "toolkit",
            kind == "dependency" ? ["base"] : [],
            (".agents/guidance/toolkit.md", Document("Toolkit v1", "# Toolkit v1\n")));
        await InstallAllAsync(workspace, source);

        var malformedTarget = kind == "dependency"
            ? ".agents/guidance/base.md"
            : ".agents/guidance/toolkit.md";
        source.ReplacePayload(
            kind == "dependency" ? "base" : "toolkit",
            malformedTarget,
            MalformedDocument("Broken"));
        source.SetVersion(kind == "dependency" ? "base" : "toolkit", "2.0.0");
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-update.projection-unavailable");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update keeps malformed native Skill metadata strict"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task MalformedNativeSkillMetadataIsNotSkipped()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-independent-metadata-native");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-independent-metadata-native-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        await InstallAsync(workspace, source, "toolkit");
        Directory.CreateDirectory(workspace.Combine(".agents/patterns/native-bad"));
        workspace.CreateOccupant(
            ".agents/patterns/native-bad/SKILL.md",
            "---\nname: native-bad\ndescription: [\n---\n# Native skill\n");
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-update.projection-unavailable");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update keeps unreadable unrelated metadata strict"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task UnreadableUnrelatedMetadataIsWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-independent-metadata-invalid-utf8");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-independent-metadata-invalid-utf8-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        await InstallAsync(workspace, source, "toolkit");
        var invalidPath = workspace.Combine(".agents/patterns/invalid.md");
        workspace.CreateOccupant(".agents/patterns/invalid.md", Document("Invalid", "# Invalid\n"));
        File.WriteAllBytes(invalidPath, [0xC3, 0x28]);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-update.projection-unavailable");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source,
        string id)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", id,
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string MalformedDocument(string name)
        => $"---\nopen-forge:\n  description: [\n---\n# {name}\n";

    private static string Document(string description, string body)
        => OpenForgeDocumentSeed.Metadata(
            description,
            ["Extension"],
            body);
}
