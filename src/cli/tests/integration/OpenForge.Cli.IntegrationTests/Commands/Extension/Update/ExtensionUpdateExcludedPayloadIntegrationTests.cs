using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateExcludedPayloadIntegrationTests
{
    [Fact(DisplayName = "Toolkit on Essentials records fully excluded dependencies consistently for List Update and Doctor")]
    [Trait("Boundary", "Host"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task FullyExcludedDependencyRemainsInstalledAndReadable()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-essentials-toolkit");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--preset", "essentials", "--automatic"])).ExitCode);
        var installed = await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic", "--format", "json"]);
        Assert.True(installed.ExitCode == 0, installed.StandardOutput);
        var packages = workspace.ReadExtensionOwnership().EnumerateArray().ToArray();
        var scenarios = Assert.Single(packages, package => package.GetProperty("id").GetString() == "scenarios");
        Assert.Empty(scenarios.GetProperty("paths").EnumerateArray());
        Assert.Empty(scenarios.GetProperty("dependencies").EnumerateArray());
        Assert.False(Directory.Exists(workspace.Combine(".agents/templates")));
        var before = workspace.Snapshot();

        var listed = await workspace.RunAsync(["extension", "list", "--installed", "--format", "json"]);
        Assert.Equal(0, listed.ExitCode);
        using var listDocument = JsonDocument.Parse(listed.StandardOutput);
        Assert.Contains(listDocument.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray(),
            package => package.GetProperty("id").GetString() == "scenarios");
        var updated = await workspace.RunAsync(["extension", "update", "--all", "--dry-run", "--format", "json"]);
        Assert.True(updated.ExitCode == 0, updated.StandardOutput);
        using var updateDocument = JsonDocument.Parse(updated.StandardOutput);
        Assert.Empty(updateDocument.RootElement.GetProperty("effects").EnumerateArray());
        var doctor = await workspace.RunAsync(["doctor", "--format", "json"]);
        Assert.Equal(2, doctor.ExitCode);
        using var doctorDocument = JsonDocument.Parse(doctor.StandardOutput);
        Assert.All(doctorDocument.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            Assert.Equal("reference.target-missing", finding.GetProperty("code").GetString()));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Unreadable Extension dependency records name the affected recorded package without inferring effects")]
    [Trait("Boundary", "Host"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task BrokenDependencyNamesAffectedPackage()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-broken-toolkit-record");
        await workspace.SeedFrameworkAsync();
        Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic"])).ExitCode);
        var ownership = JsonNode.Parse(workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath))?.AsObject()
            ?? throw new InvalidOperationException("The fixture requires a readable lock.");
        var packages = ownership["extensions"]?.AsArray()
            ?? throw new InvalidOperationException("The fixture requires Extension records.");
        packages.Remove(Assert.Single(packages, package => package?["id"]?.GetValue<string>() == "scenarios"));
        workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
        var before = workspace.Snapshot();

        var updated = await workspace.RunAsync(["extension", "update", "--all", "--dry-run", "--format", "json"]);

        Assert.Equal(2, updated.ExitCode);
        using var document = JsonDocument.Parse(updated.StandardOutput);
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("development-toolkit", finding.GetProperty("subject").GetProperty("id").GetString());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        var text = await workspace.RunAsync(["extension", "update", "--all", "--dry-run"]);
        Assert.Contains("development-toolkit", text.StandardOutput);
        Assert.DoesNotContain("The the", text.StandardOutput);
        Assert.Contains("open-forge extension inspect development-toolkit", text.StandardOutput);
        Assert.Equal(before, workspace.Snapshot());
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "unreadable-toolkit-record",
            Arguments = ["extension", "update", "--all", "--dry-run"],
            ExitCode = 2,
        });
        Assert.Equal(before, workspace.Snapshot());
    }
}
