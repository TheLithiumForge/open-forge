using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallPlanningIntegrationTests
{
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
            "--automatic", "--json",
        ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var effects = document.RootElement.GetProperty("result").GetProperty("effects")
            .EnumerateArray()
            .Where(effect => effect.GetProperty("kind").GetString() == "package-file")
            .ToArray();
        Assert.Equal(target, Assert.Single(effects).GetProperty("path").GetString());
        var lifecyclePath = Assert.Single(
            workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(target, lifecyclePath.GetProperty("path").GetString());
        Assert.Equal(
            ["alpha", "beta"],
            lifecyclePath.GetProperty("owners").EnumerateArray()
                .Select(owner => owner.GetString())
                .ToArray());
    }

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
            "--dry-run", "--json",
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-install.ownership-conflict"
                && finding.GetProperty("target").GetString() == ".agents/Shared.txt");
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }

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
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(expectedSelectedBy, result.GetProperty("selection").GetProperty("selectedBy").GetString());
        Assert.Equal(
            scenario == "explicit-all" ? ["base", "toolkit"] : ["toolkit"],
            Strings(result.GetProperty("selection").GetProperty("rootIds")));
        Assert.Equal(["base", "toolkit"], Strings(result.GetProperty("packages"), "id"));
        Assert.Equal(
            scenario == "explicit-all" ? [true, true] : [false, true],
            result.GetProperty("packages").EnumerateArray().Select(package => package.GetProperty("selectedRoot").GetBoolean()));
        Assert.Empty(result.GetProperty("packages")[0].GetProperty("dependencies").EnumerateArray());
        Assert.Equal(["base"], Strings(result.GetProperty("packages")[1].GetProperty("dependencies")));
        Assert.Equal("publish", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("planned", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("planned", result.GetProperty("verification").GetProperty("targets").GetString());
        Assert.Equal("planned", result.GetProperty("verification").GetProperty("topology").GetString());
        Assert.DoesNotContain(".agents", Strings(result.GetProperty("footprint").GetProperty("directories")));
        Assert.Contains(".agents/base", Strings(result.GetProperty("footprint").GetProperty("directories")));
        Assert.Contains(".agents/toolkit", Strings(result.GetProperty("footprint").GetProperty("directories")));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        var source = result.GetProperty("source");
        Assert.Equal("embedded", source.GetProperty("kind").GetString());
        Assert.Equal(JsonValueKind.Null, source.GetProperty("path").ValueKind);
        Assert.Equal(["development-toolkit"], Strings(result.GetProperty("packages"), "id"));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(lockInfrastructureBefore, workspace.LockInfrastructureExists);
    }

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
            "--json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.framework-unavailable");
        Assert.Equal("none", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(workspace.LockInfrastructureExists);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == expectedCode);
        if (expectedCode == "extension-install.permission-required")
        {
            Assert.Contains(result.GetProperty("permissions").GetProperty("missing").EnumerateArray(),
                requirement => requirement.GetProperty("path").GetString() == target);
        }
        Assert.DoesNotContain(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.workspace-lock-unavailable");
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal("not-started", effect.GetProperty("outcome").GetString()));
        Assert.Equal(expectedCode == "extension-install.permission-required" ? "not-created" : "not-required",
            result.GetProperty("recovery").GetProperty("state").GetString());
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
                "--dry-run", "--json",
            ],
            "explicit-all" =>
            [
                "extension", "install", "--all",
                "--source", source.Path,
                "--dry-run", "--json",
            ],
            "single-package" =>
            [
                "extension", "install",
                "--source", source.PackagePath("toolkit"),
                "--dry-run", "--json",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The selection scenario is not defined."),
        };

    private static IReadOnlyList<string?> Strings(JsonElement array)
        => [.. array.EnumerateArray().Select(value => value.GetString())];

    private static IReadOnlyList<string?> Strings(JsonElement array, string property)
        => [.. array.EnumerateArray().Select(value => value.GetProperty(property).GetString())];

    private static string Document(string description, string body)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            description,
            ["Extension"],
            body);
}
