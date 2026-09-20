using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallPlanningIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install coalesces one compatible shared target and publishes every owner"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task CompatibleSharedTargetHasOneEffectAndEveryOwner()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-compatible-shared-target");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-compatible-shared-target-source");
        const string target = ".agents/shared.txt";
        const string contents = "shared package bytes\n";
        source.AddPackage("alpha", [], (target, contents));
        source.AddPackage("beta", [], (target, contents));

        var result = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var effects = document.RootElement.GetProperty("effects")
            .EnumerateArray()
            .Where(effect => effect.GetProperty("kind").GetString() == "file")
            .ToArray();
        Assert.Equal(target, Assert.Single(effects).GetProperty("path").GetString());
        var receipts = workspace.ReadExtensionOwnership().EnumerateArray().ToArray();
        Assert.Equal(["alpha", "beta"], receipts.Select(receipt => receipt.GetProperty("id").GetString()));
        Assert.All(receipts, receipt => Assert.Equal(target,
            Assert.Single(receipt.GetProperty("paths").EnumerateArray()).GetString()));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install adds a compatible sequential shared owner without rewriting the target and removal retains it"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task SequentialCompatibleSharedTargetAddsOwnerWithoutRewrite()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-sequential-compatible-shared-target");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-sequential-compatible-shared-target-source");
        const string target = ".agents/shared.txt";
        const string contents = "shared package bytes\n";
        source.AddPackage("alpha", [], (target, contents));
        source.AddPackage("beta", [], (target, contents));

        var first = await workspace.RunAsync(
        [
            "extension", "install", "alpha",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        var bytesAfterFirst = File.ReadAllBytes(workspace.Combine(target));

        var second = await workspace.RunAsync(
        [
            "extension", "install", "beta",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, second.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        using var secondDocument = JsonDocument.Parse(second.StandardOutput);
        Assert.DoesNotContain(
            secondDocument.RootElement.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("kind").GetString() == "file"
                && effect.GetProperty("path").GetString() == target);
        Assert.Equal(bytesAfterFirst, File.ReadAllBytes(workspace.Combine(target)));
        var receipts = workspace.ReadExtensionOwnership().EnumerateArray().ToArray();
        Assert.Equal(["alpha", "beta"], receipts.Select(receipt => receipt.GetProperty("id").GetString()));
        Assert.All(receipts, receipt => Assert.Equal(target,
            Assert.Single(receipt.GetProperty("paths").EnumerateArray()).GetString()));

        var removal = await workspace.RunAsync(
        [
            "extension", "remove", "alpha",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, removal.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, removal.Status);
        Assert.True(File.Exists(workspace.Combine(target)));
        Assert.Equal(bytesAfterFirst, File.ReadAllBytes(workspace.Combine(target)));
        var remaining = Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal("beta", remaining.GetProperty("id").GetString());
        Assert.Equal(target, Assert.Single(remaining.GetProperty("paths").EnumerateArray()).GetString());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install rejects differing selected bytes for one portable target before planning"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task DifferingPortableTargetIsOwnershipConflict()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-shared-generated-conflict");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-shared-generated-conflict-source");
        source.AddPackage("alpha", [], (".agents/shared.txt", "alpha bytes\n"));
        source.AddPackage("beta", [], (".agents/Shared.txt", "beta bytes\n"));
        var before = workspace.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--dry-run", "--format", "json",
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-install.ownership-conflict"
                && finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/Shared.txt");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install blocks incompatible sequential shared content without writes"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task DifferingSequentialTargetIsOwnershipConflictAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-sequential-shared-conflict");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-sequential-shared-conflict-source");
        const string target = ".agents/shared.txt";
        source.AddPackage("alpha", [], (target, "alpha bytes\n"));
        source.AddPackage("beta", [], (target, "beta bytes\n"));

        var first = await workspace.RunAsync(
        [
            "extension", "install", "alpha",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        var before = workspace.Snapshot();
        var beforeBytes = File.ReadAllBytes(workspace.Combine(target));

        var second = await workspace.RunAsync(
        [
            "extension", "install", "beta",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, second.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, second.Status);
        using var document = JsonDocument.Parse(second.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-install.ownership-conflict"
                && finding.GetProperty("subject").GetProperty("path").GetString() == target);
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(beforeBytes, File.ReadAllBytes(workspace.Combine(target)));
        var owner = Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal("alpha", owner.GetProperty("id").GetString());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install resolves exact IDs all and single-package inference with mandatory dependency closure"),
     Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("explicit-ids", "explicit-ids")]
    [InlineData("explicit-all", "explicit-all")]
    [InlineData("single-package", "single-package-inference")]
    public static async Task SelectionModesResolveOneExactSourceUniverse(
        string scenario,
        string expectedSelectedBy)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create($"extension-install-selection-{scenario}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create($"extension-install-source-{scenario}");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base", "# Base\n")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit", "# Toolkit\n")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(SelectionArguments(scenario, source));

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension install", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(expectedSelectedBy, result.GetProperty("selection").GetProperty("method").GetString());
        Assert.Equal(
            scenario == "explicit-all" ? ["base", "toolkit"] : ["toolkit"],
            result.GetProperty("packages").EnumerateArray()
                .Where(package => package.GetProperty("selected").GetBoolean())
                .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(["base", "toolkit"], Strings(result.GetProperty("packages"), "id"));
        Assert.Equal(
            scenario == "explicit-all" ? [true, true] : [false, true],
            result.GetProperty("packages").EnumerateArray().Select(package => package.GetProperty("selected").GetBoolean()));
        Assert.Equal("1.0.0", result.GetProperty("packages")[0].GetProperty("version").GetString());
        Assert.Equal(["toolkit"], Strings(result.GetProperty("packages")[0].GetProperty("requiredBy")));
        Assert.Empty(result.GetProperty("packages")[1].GetProperty("requiredBy").EnumerateArray());
        Assert.All(root.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal("planned", effect.GetProperty("outcome").GetString()));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install uses the embedded catalogue when source is omitted"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task OmittedSourceIsEmbeddedAndPromptless()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-embedded-source");
        await workspace.SeedFrameworkAsync();
        var before = workspace.Snapshot();
        var lockInfrastructureBefore = workspace.LockInfrastructureExists;

        var run = await workspace.RunAsync(
        [
            "extension", "install", "development-toolkit",
            "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        var source = result.GetProperty("source");
        Assert.Equal("embedded", source.GetProperty("kind").GetString());
        Assert.Equal(JsonValueKind.Null, source.GetProperty("path").ValueKind);
        Assert.Equal(["workflows", "development", "planning", "project-documents", "development-toolkit"],
            Strings(result.GetProperty("packages"), "id"));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(lockInfrastructureBefore, workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install treats an absent agents container as an unavailable Framework anchor without effects"),
     Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task MissingFrameworkAnchorIsIncompleteAndNeverCreated()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-missing-anchor");
        using var source = ExtensionInstallCatalogue.Create("extension-install-missing-anchor-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit", "# Toolkit\n")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic",
            "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.framework-unavailable");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(workspace.LockInfrastructureExists);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install blocks unapproved external and protected destinations before lease acquisition"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("README.md", "extension-install.permission-required")]
    [InlineData(".agents", "extension-install.target-unsafe")]
    [InlineData(".apm/toolkit.md", "extension-install.permission-required")]
    public static async Task UnapprovedOrProtectedTargetIsBlockedBeforeLock(
        string target, string expectedCode)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-target-policy");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-target-policy-source");
        source.AddPackage("toolkit", [], (target, "protected package bytes\n"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        using var heldLease = workspace.HoldLock();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic",
            "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("data");
        Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == expectedCode);
        if (expectedCode == "extension-install.permission-required")
        {
            Assert.Contains(result.GetProperty("permissions").GetProperty("missing").EnumerateArray(),
                requirement => requirement.GetString() == target);
        }
        Assert.DoesNotContain(root.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.workspace-lock-unavailable");
        Assert.All(root.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal("not-started", effect.GetProperty("outcome").GetString()));
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install keeps unrelated malformed metadata as an exact warning across apply and repeat"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task UnrelatedMalformedMetadataIsWarningAndPreservesOwnership()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-unrelated-malformed-metadata");
        await workspace.SeedFrameworkAsync();
        const string notePath = ".agents/patterns/old-note.md";
        var noteBytes = System.Text.Encoding.UTF8.GetBytes(MalformedDocument("Old note"));
        workspace.CreateOccupant(notePath, MalformedDocument("Old note"));
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-unrelated-malformed-metadata-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        var frameworkBefore = workspace.ReadFrameworkOwnership().GetRawText();
        var arguments = new[]
        {
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json", "--detail", "full",
        };

        var first = await workspace.RunAsync(arguments);

        Assert.Equal(2, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, first.Status);
        using var firstDocument = JsonDocument.Parse(first.StandardOutput);
        Assert.Contains(firstDocument.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.metadata-projection-skipped"
            && finding.GetProperty("subject").GetProperty("path").GetString() == notePath
            && finding.GetProperty("message").GetString()
                == "Every direct routed child requires complete authored source metadata.");
        Assert.Equal(
            source.ReadPayloadBytes("toolkit", ".agents/guidance/toolkit.md"),
            File.ReadAllBytes(workspace.Combine(".agents/guidance/toolkit.md")));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(notePath)));
        Assert.Equal(frameworkBefore, workspace.ReadFrameworkOwnership().GetRawText());
        var extensionAfterFirst = workspace.ReadExtensionOwnership().GetRawText();
        var afterFirst = workspace.Snapshot();

        var repeat = await workspace.RunAsync(arguments);

        Assert.Equal(2, repeat.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, repeat.Status);
        using var repeatDocument = JsonDocument.Parse(repeat.StandardOutput);
        Assert.Contains(repeatDocument.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.metadata-projection-skipped"
            && finding.GetProperty("subject").GetProperty("path").GetString() == notePath
            && finding.GetProperty("message").GetString()
                == "Every direct routed child requires complete authored source metadata.");
        Assert.Equal(afterFirst, workspace.Snapshot());
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(notePath)));
        Assert.Equal(frameworkBefore, workspace.ReadFrameworkOwnership().GetRawText());
        Assert.Equal(extensionAfterFirst, workspace.ReadExtensionOwnership().GetRawText());
        using var ownership = JsonDocument.Parse(extensionAfterFirst);
        var extension = Assert.Single(ownership.RootElement.EnumerateArray());
        Assert.Equal("toolkit", extension.GetProperty("id").GetString());
        Assert.Equal(
            [".agents/guidance/toolkit.md"],
            Strings(extension.GetProperty("paths")));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install refuses malformed required metadata without writes"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData(".agents/guidance/toolkit.md", "child")]
    [InlineData(".agents/guidance/_guidance.md", "host")]
    public static async Task MalformedRequiredMetadataIsWriteFree(
        string target,
        string kind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-install-required-malformed-{kind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-install-required-malformed-{kind}-source");
        source.AddPackage("toolkit", [], (target, MalformedDocument("Broken")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.generated-region-unsafe");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install refuses denied and invalid UTF-8 authored metadata without writes"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task UnreadableMetadataIsWriteFree()
    {
        if (OperatingSystem.IsWindows())
        {
            using (var deniedWorkspace = ExtensionInstallIntegrationWorkspace.Create(
                       "extension-install-denied-metadata"))
            {
                await deniedWorkspace.SeedFrameworkAsync();
                using var source = ExtensionInstallCatalogue.Create("extension-install-denied-metadata-source");
                var deniedPath = deniedWorkspace.Combine(".agents/patterns/denied.md");
                deniedWorkspace.CreateOccupant(".agents/patterns/denied.md", Document("Denied", "# Denied\n"));
                source.AddPackage(
                    "toolkit",
                    [],
                    (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
                var beforeWorkspace = deniedWorkspace.Snapshot();
                var beforeSource = source.Snapshot();

                using (var denied = new FileStream(
                           deniedPath,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.None))
                {
                    var readDenied = false;
                    try
                    {
                        _ = File.ReadAllBytes(deniedPath);
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                    {
                        readDenied = true;
                    }

                    Assert.True(readDenied, "The locked authored metadata remained readable.");
                    var result = await deniedWorkspace.RunAsync(
                    [
                        "extension", "install", "toolkit",
                        "--source", source.Path,
                        "--automatic", "--format", "json",
                    ]);

                    Assert.Equal(5, result.ExitCode);
                    Assert.Equal(CliSemanticStatus.Blocked, result.Status);
                }

                Assert.Equal(beforeWorkspace, deniedWorkspace.Snapshot());
                Assert.Equal(beforeSource, source.Snapshot());
            }
        }

        using var invalidWorkspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-invalid-utf8-metadata");
        await invalidWorkspace.SeedFrameworkAsync();
        using var invalidSource = ExtensionInstallCatalogue.Create(
            "extension-install-invalid-utf8-metadata-source");
        var invalidPath = invalidWorkspace.Combine(".agents/patterns/invalid.md");
        invalidWorkspace.CreateOccupant(".agents/patterns/invalid.md", "owned invalid UTF-8 fixture");
        File.WriteAllBytes(invalidPath, [0xC3, 0x28]);
        invalidSource.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        var invalidBeforeWorkspace = invalidWorkspace.Snapshot();
        var invalidBeforeSource = invalidSource.Snapshot();

        var invalidResult = await invalidWorkspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", invalidSource.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, invalidResult.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, invalidResult.Status);
        Assert.Equal(invalidBeforeWorkspace, invalidWorkspace.Snapshot());
        Assert.Equal(invalidBeforeSource, invalidSource.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install does not treat malformed native Skill metadata as optional")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task MalformedNativeSkillMetadataIsNotSkipped()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-native-metadata");
        await workspace.SeedFrameworkAsync();
        Directory.CreateDirectory(workspace.Combine(".agents/patterns/native-bad"));
        workspace.CreateOccupant(
            ".agents/patterns/native-bad/SKILL.md",
            "---\nname: native-bad\ndescription: [\n---\n# Native skill\n");
        using var source = ExtensionInstallCatalogue.Create("extension-install-native-metadata-source");
        source.AddPackage("toolkit", [], (".agents/guidance/toolkit.md", Document("Toolkit", "# Toolkit\n")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "toolkit", "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, result.ExitCode);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.generated-region-unsafe");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static string[] SelectionArguments(
        string scenario,
        ExtensionInstallCatalogue source)
        => scenario switch
        {
            "explicit-ids" =>
            [
                "extension", "install", "toolkit",
                "--source", source.Path,
                "--dry-run", "--format", "json", "--detail", "standard",
            ],
            "explicit-all" =>
            [
                "extension", "install", "--all",
                "--source", source.Path,
                "--dry-run", "--format", "json", "--detail", "standard",
            ],
            "single-package" =>
            [
                "extension", "install",
                "--source", source.PackagePath("toolkit"),
                "--dry-run", "--format", "json", "--detail", "standard",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The selection scenario is not defined."),
        };

    private static IReadOnlyList<string?> Strings(JsonElement array)
        => [.. array.EnumerateArray().Select(value => value.GetString())];

    private static IReadOnlyList<string?> Strings(JsonElement array, string property)
        => [.. array.EnumerateArray().Select(value => value.GetProperty(property).GetString())];

    private static string MalformedDocument(string name)
        => $"---\nopen-forge:\n  description: [\n---\n# {name}\n";

    private static string Document(string description, string body)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            description,
            ["Extension"],
            body);
}
