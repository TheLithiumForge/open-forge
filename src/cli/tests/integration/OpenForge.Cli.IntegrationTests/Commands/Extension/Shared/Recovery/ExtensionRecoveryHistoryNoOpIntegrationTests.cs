using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

[Trait("Boundary", "OS"), Trait("Evidence", "Integration")]
public sealed class ExtensionRecoveryHistoryNoOpIntegrationTests
{
    private const string Target = SourceLogicalPath.AgentsRoot + "/toolkit.txt";
    private const string RetainedHistory = "retained history";
    private const string VersionOne = "version one\n";
    private const string VersionTwo = "version two\n";
    private const string UnavailableHistory = "unavailable history";
    private const string PackageId = "toolkit";
    [Theory(DisplayName = "Extension no-ops omit verified recovery history and keep exit zero")]
    [InlineData(RecoveryBundleOperation.Install), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    [Trait("Feature", "extension-recovery-history"), Trait("Evidence", "Integration")]
    public async Task VerifiedHistoryIsSilent(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-noop-verified-history");
        using var source = ExtensionInstallCatalogue.Create("extension-noop-verified-history-source");
        await SeedAsync(workspace, source, command);
        var prior = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
            workspace, ExtensionInstallDefinitions.CommandIdentity, RecoveryBundleOperation.Install, TestContext.Current.CancellationToken);
        var bytes = File.ReadAllBytes(prior.BundlePath);
        var before = workspace.Snapshot();

        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path, new ReadOutputScenario
        {
            Situation = RecoveryWireVocabulary.Operation(command),
            Arguments = Arguments(command, source.Path),
            ExitCode = 0,
        }, testName: $"{nameof(VerifiedHistoryIsSilent)}.{RecoveryWireVocabulary.Operation(command)}");
        var run = await workspace.RunAsync([.. Arguments(command, source.Path), CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var json = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(bytes, File.ReadAllBytes(prior.BundlePath));
        Assert.False(File.Exists(WorkspaceRuntimePaths.LockPath(workspace.Workspace)));
    }

    [Theory, InlineData(RecoveryBundleOperation.Install), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task VerifiedNoOpCreatesNoRuntimeState(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-noop-runtime");
        using var source = ExtensionInstallCatalogue.Create("extension-noop-runtime-source");
        await SeedAsync(workspace, source, command);
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(Arguments(command, source.Path));

        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    [Theory(DisplayName = "Extension no-ops warn only about draft and damaged history while preserving every bundle")]
    [InlineData(RecoveryBundleOperation.Install), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task VerifiedNoOpReportsEveryRetainedArtifactWithoutChangingIt(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
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
        foreach (var path in paths) File.WriteAllText(path, RetainedHistory);
        var verified = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
            workspace, ExtensionInstallDefinitions.CommandIdentity, RecoveryBundleOperation.Install, TestContext.Current.CancellationToken);
        var verifiedBytes = File.ReadAllBytes(verified.BundlePath);
        var before = workspace.Snapshot();
        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path, new ReadOutputScenario
        {
            Situation = RecoveryWireVocabulary.Operation(command),
            Arguments = Arguments(command, source.Path),
            ExitCode = 2,
        }, testName: $"{nameof(VerifiedNoOpReportsEveryRetainedArtifactWithoutChangingIt)}.{RecoveryWireVocabulary.Operation(command)}");
        var run = await workspace.RunAsync([.. Arguments(command, source.Path), CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var findings = document.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray().ToArray();
        Assert.Equal(paths.Order(StringComparer.Ordinal), findings.Select(finding =>
            finding.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString()).Order(StringComparer.Ordinal));
        Assert.All(findings, finding => Assert.Equal(ExtensionRecoveryHistoryFixture.HistoryCode(command),
            finding.GetProperty(CliJsonFields.Code).GetString()));
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(File.Exists(WorkspaceRuntimePaths.LockPath(workspace.Workspace)));
        foreach (var path in paths) Assert.Equal(RetainedHistory, File.ReadAllText(path));
        Assert.Equal(verifiedBytes, File.ReadAllBytes(verified.BundlePath));
    }

    [Theory, InlineData(RecoveryBundleOperation.Install), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task UnavailableHistoryDoesNotVetoVerifiedNoOp(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
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
            Situation = RecoveryWireVocabulary.Operation(command),
            Arguments = Arguments(command, source.Path),
            ExitCode = 2,
        }, testName: $"{nameof(UnavailableHistoryDoesNotVetoVerifiedNoOp)}.{RecoveryWireVocabulary.Operation(command)}");

        Assert.Equal(before, workspace.Snapshot());
        Assert.False(File.Exists(WorkspaceRuntimePaths.LockPath(workspace.Workspace)));
    }

    private static string[] Arguments(RecoveryBundleOperation command, string source)
        => ExtensionRecoveryHistoryFixture.Arguments(command, source, PackageId);

    [Theory, InlineData(RecoveryBundleOperation.Install), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task UnavailableHistoryStillRequiresNewPreparationBeforeWriting(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-history-required-preparation");
        using var source = ExtensionInstallCatalogue.Create("extension-history-required-preparation-source");
        await workspace.SeedFrameworkAsync();
        source.AddPackage(PackageId, [], (Target, VersionOne));
        if (command != RecoveryBundleOperation.Install)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments(RecoveryBundleOperation.Install, source.Path))).ExitCode);
            if (command == RecoveryBundleOperation.Update) source.ReplacePayload(PackageId, Target, VersionTwo);
        }
        var location = RecoveryStoreLocator.Local(workspace.Workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(location.WorkspaceDirectory)
            ?? throw new InvalidOperationException("The recovery directory requires a parent."));
        if (Directory.Exists(location.WorkspaceDirectory)) Directory.Delete(location.WorkspaceDirectory, recursive: true);
        File.WriteAllText(location.WorkspaceDirectory, UnavailableHistory);
        var before = workspace.Snapshot();
        var arguments = Arguments(command, source.Path);
        var preview = await workspace.RunAsync([.. arguments, ExtensionRecoveryHistoryFixture.DryRunOption(command), CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);
        Assert.Equal(CliSemanticStatus.Attention, preview.Status);
        Assert.Equal(before, workspace.Snapshot());

        var run = await workspace.RunAsync([.. arguments, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat]);

        Assert.True(run.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete, run.StandardOutput);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(UnavailableHistory, File.ReadAllText(location.WorkspaceDirectory));
    }

    private static async Task SeedAsync(
        ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source, RecoveryBundleOperation command)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage(PackageId, [], (Target, "toolkit\n"));
        Assert.Equal(0, (await workspace.RunAsync(Arguments(RecoveryBundleOperation.Install, source.Path))).ExitCode);
        if (command == RecoveryBundleOperation.Remove)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments(RecoveryBundleOperation.Remove, source.Path))).ExitCode);
        }
        Directory.Delete(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace), recursive: true);
    }
}
