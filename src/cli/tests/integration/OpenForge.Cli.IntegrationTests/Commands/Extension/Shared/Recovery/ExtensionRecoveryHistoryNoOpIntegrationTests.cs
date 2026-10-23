using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

public sealed class ExtensionRecoveryHistoryNoOpIntegrationTests
{
    [Theory, InlineData("install"), InlineData("update"), InlineData("remove")]
    public async Task VerifiedNoOpCreatesNoRuntimeState(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-noop-runtime");
        using var source = ExtensionInstallCatalogue.Create("extension-noop-runtime-source");
        await SeedAsync(workspace, source, command);
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(Arguments(command, source.Path));

        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(Directory.Exists(workspace.Combine(".open-forge-state")));
    }

    [Theory, InlineData("install"), InlineData("update"), InlineData("remove")]
    public async Task VerifiedNoOpReportsEveryRetainedArtifactWithoutChangingIt(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-noop-history");
        using var source = ExtensionInstallCatalogue.Create("extension-noop-history-source");
        await SeedAsync(workspace, source, command);
        var paths = new[]
        {
            ExtensionRecoveryHistoryFixture.CandidatePath(workspace.Workspace,
                Guid.Parse("11111111-1111-1111-1111-111111111111"), RecoveryBundleCandidateKind.Draft),
            ExtensionRecoveryHistoryFixture.CandidatePath(workspace.Workspace,
                Guid.Parse("22222222-2222-2222-2222-222222222222"), RecoveryBundleCandidateKind.Final),
        };
        foreach (var path in paths) File.WriteAllText(path, "retained history");
        var before = workspace.Snapshot();
        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path, new ReadOutputScenario
        {
            Situation = command,
            Arguments = Arguments(command, source.Path),
            ExitCode = 2,
        }, testName: $"{nameof(VerifiedNoOpReportsEveryRetainedArtifactWithoutChangingIt)}.{command}");
        var run = await workspace.RunAsync([.. Arguments(command, source.Path), "--format", "json"]);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        Assert.Equal(paths.Order(StringComparer.Ordinal), findings.Select(finding =>
            finding.GetProperty("subject").GetProperty("path").GetString()).Order(StringComparer.Ordinal));
        Assert.All(findings, finding => Assert.Equal($"extension-{command}.recovery-history",
            finding.GetProperty("code").GetString()));
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(File.Exists(workspace.Combine(".open-forge-state/workspace.lock")));
        foreach (var path in paths) Assert.Equal("retained history", File.ReadAllText(path));
    }

    [Theory, InlineData("install"), InlineData("update"), InlineData("remove")]
    public async Task UnavailableHistoryDoesNotVetoVerifiedNoOp(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-noop-coverage");
        using var source = ExtensionInstallCatalogue.Create("extension-noop-coverage-source");
        await SeedAsync(workspace, source, command);
        var location = RecoveryStoreLocator.Local(workspace.Workspace);
        if (Directory.Exists(location.WorkspaceDirectory)) Directory.Delete(location.WorkspaceDirectory, recursive: true);
        Directory.CreateDirectory(Path.GetDirectoryName(location.WorkspaceDirectory)
            ?? throw new InvalidOperationException("The recovery directory requires a parent."));
        File.WriteAllText(location.WorkspaceDirectory, "unavailable catalogue");
        var before = workspace.Snapshot();

        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path, new ReadOutputScenario
        {
            Situation = command,
            Arguments = Arguments(command, source.Path),
            ExitCode = 2,
        }, testName: $"{nameof(UnavailableHistoryDoesNotVetoVerifiedNoOp)}.{command}");

        Assert.Equal(before, workspace.Snapshot());
        Assert.False(File.Exists(workspace.Combine(".open-forge-state/workspace.lock")));
    }

    private static string[] Arguments(string command, string source)
        => command == "remove"
            ? ["extension", command, "toolkit", "--automatic"]
            : ["extension", command, "toolkit", "--source", source, "--automatic"];

    [Theory, InlineData("install"), InlineData("update"), InlineData("remove")]
    public async Task UnavailableHistoryStillRequiresNewPreparationBeforeWriting(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-history-required-preparation");
        using var source = ExtensionInstallCatalogue.Create("extension-history-required-preparation-source");
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (".agents/toolkit.txt", "version one\n"));
        if (command != "install")
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments("install", source.Path))).ExitCode);
            if (command == "update") source.ReplacePayload("toolkit", ".agents/toolkit.txt", "version two\n");
        }
        var location = RecoveryStoreLocator.Local(workspace.Workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(location.WorkspaceDirectory)
            ?? throw new InvalidOperationException("The recovery directory requires a parent."));
        if (Directory.Exists(location.WorkspaceDirectory)) Directory.Delete(location.WorkspaceDirectory, recursive: true);
        File.WriteAllText(location.WorkspaceDirectory, "unavailable history");
        var before = workspace.Snapshot();
        var arguments = Arguments(command, source.Path);
        var preview = await workspace.RunAsync([.. arguments, "--dry-run", "--format", "json"]);
        Assert.Equal(CliSemanticStatus.Attention, preview.Status);
        Assert.Equal(before, workspace.Snapshot());

        var run = await workspace.RunAsync([.. arguments, "--format", "json"]);

        Assert.True(run.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete, run.StandardOutput);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("unavailable history", File.ReadAllText(location.WorkspaceDirectory));
    }

    private static async Task SeedAsync(
        ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source, string command)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (".agents/toolkit.txt", "toolkit\n"));
        Assert.Equal(0, (await workspace.RunAsync(Arguments("install", source.Path))).ExitCode);
        if (command == "remove")
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments("remove", source.Path))).ExitCode);
        }
        Directory.Delete(workspace.Combine(".open-forge-state"), recursive: true);
    }
}
