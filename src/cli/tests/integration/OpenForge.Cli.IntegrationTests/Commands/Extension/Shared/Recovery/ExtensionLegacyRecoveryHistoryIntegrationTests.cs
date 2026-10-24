using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

[Trait("Feature", "extension-recovery-history"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionLegacyRecoveryHistoryIntegrationTests
{
    private const string DraftHistory = "draft";
    private const string DamagedHistory = "damaged";
    private const string UnavailableHistory = "unavailable";
    private const string VerifiedHistory = "verified";
    private const string VersionOne = "version one\n";
    private const string VersionTwo = "version two\n";
    private const string PackageId = "toolkit";
    private const string Target = SourceLogicalPath.AgentsRoot + "/toolkit.txt";

    [Theory(DisplayName = "Extension writes and no-ops report legacy problems while verified finals stay silent")]
    [InlineData(RecoveryBundleOperation.Install, DraftHistory, false), InlineData(RecoveryBundleOperation.Install, DraftHistory, true)]
    [InlineData(RecoveryBundleOperation.Install, DamagedHistory, false), InlineData(RecoveryBundleOperation.Install, DamagedHistory, true)]
    [InlineData(RecoveryBundleOperation.Install, UnavailableHistory, false), InlineData(RecoveryBundleOperation.Install, UnavailableHistory, true)]
    [InlineData(RecoveryBundleOperation.Install, VerifiedHistory, false), InlineData(RecoveryBundleOperation.Install, VerifiedHistory, true)]
    [InlineData(RecoveryBundleOperation.Update, DraftHistory, false), InlineData(RecoveryBundleOperation.Update, DraftHistory, true)]
    [InlineData(RecoveryBundleOperation.Update, DamagedHistory, false), InlineData(RecoveryBundleOperation.Update, DamagedHistory, true)]
    [InlineData(RecoveryBundleOperation.Update, UnavailableHistory, false), InlineData(RecoveryBundleOperation.Update, UnavailableHistory, true)]
    [InlineData(RecoveryBundleOperation.Update, VerifiedHistory, false), InlineData(RecoveryBundleOperation.Update, VerifiedHistory, true)]
    [InlineData(RecoveryBundleOperation.Remove, DraftHistory, false), InlineData(RecoveryBundleOperation.Remove, DraftHistory, true)]
    [InlineData(RecoveryBundleOperation.Remove, DamagedHistory, false), InlineData(RecoveryBundleOperation.Remove, DamagedHistory, true)]
    [InlineData(RecoveryBundleOperation.Remove, UnavailableHistory, false), InlineData(RecoveryBundleOperation.Remove, UnavailableHistory, true)]
    [InlineData(RecoveryBundleOperation.Remove, VerifiedHistory, false), InlineData(RecoveryBundleOperation.Remove, VerifiedHistory, true)]
    public async Task LegacyHistoryDoesNotVetoCurrentState(object commandValue, string history, bool noOp)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-legacy-history");
        using var source = ExtensionInstallCatalogue.Create("extension-legacy-history-source");
        await workspace.SeedFrameworkAsync();
        source.AddPackage(PackageId, [], (Target, VersionOne));
        if (command != RecoveryBundleOperation.Install || noOp)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments(RecoveryBundleOperation.Install, source.Path))).ExitCode);
        }
        if (command == RecoveryBundleOperation.Remove && noOp)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments(RecoveryBundleOperation.Remove, source.Path))).ExitCode);
        }
        if (command == RecoveryBundleOperation.Update && !noOp) source.ReplacePayload(PackageId, Target, VersionTwo);

        var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace.Workspace));
        try
        {
            var path = await SeedHistoryAsync(workspace, legacy, history);
            var bytes = File.ReadAllBytes(path);
            Directory.Delete(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace), recursive: true);
            Assert.False(Directory.Exists(RecoveryStoreLocator.Local(workspace.Workspace).WorkspaceDirectory));
            var before = workspace.Snapshot();
            var sourceBefore = source.Snapshot();

            var run = await workspace.RunAsync([.. Arguments(command, source.Path), CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full]);

            var silent = history == VerifiedHistory;
            Assert.Equal(silent ? 0 : 2, run.ExitCode);
            Assert.Equal(silent ? CliSemanticStatus.Complete : CliSemanticStatus.Attention, run.Status);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = JsonDocument.Parse(run.StandardOutput);
            var findings = document.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray();
            if (silent)
            {
                Assert.Empty(findings);
            }
            else
            {
                var finding = Assert.Single(findings);
                Assert.Equal(ExtensionRecoveryHistoryFixture.HistoryCode(command, history == UnavailableHistory),
                    finding.GetProperty(CliJsonFields.Code).GetString());
                Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), finding.GetProperty(CliJsonFields.Severity).GetString());
                Assert.Equal(path, finding.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString());
                var recovery = document.RootElement.GetProperty(CliJsonFields.Recovery);
                var ownRecoveryPath = recovery.ValueKind == JsonValueKind.Object
                    ? recovery.GetProperty(CliJsonFields.Path).GetString() : null;
                var normalized = CommandOutputNormalization.Normalize(
                    CommandOutputNormalization.ReplaceDelimitedPath(run.StandardOutput, path, "<history-path>"),
                    workspace.Path, CliBuildVersion.InformationalVersion, ownRecoveryPath, source.Path);
                CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
                {
                    [$"{RecoveryWireVocabulary.Operation(command)}.{history}.{noOp}{CommandOutputSnapshot.JsonContentNameSegment}{CliPresentationDefinitions.Full}"] =
                        CommandOutputSnapshotFormatting.PrettyPrintJson(normalized),
                }, testName: $"{nameof(LegacyHistoryDoesNotVetoCurrentState)}.{RecoveryWireVocabulary.Operation(command)}.{history}.{noOp}");
            }
            Assert.Equal(bytes, File.ReadAllBytes(path));
            Assert.Equal(sourceBefore, source.Snapshot());
            if (noOp)
            {
                Assert.Equal(before, workspace.Snapshot());
                Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
            }
            else
            {
                if (command == RecoveryBundleOperation.Remove) Assert.False(File.Exists(workspace.Combine(Target)));
                else Assert.Equal(command == RecoveryBundleOperation.Update ? VersionTwo : VersionOne, workspace.ReadText(Target));
                if (command != RecoveryBundleOperation.Install)
                {
                    var ownPath = Assert.IsType<string>(document.RootElement.GetProperty(CliJsonFields.Recovery).GetProperty(CliJsonFields.Path).GetString());
                    var local = RecoveryStoreLocator.Local(workspace.Workspace);
                    Assert.Equal(local.WorkspaceDirectory, Path.GetDirectoryName(ownPath));
                    var own = await RecoveryBundleReader.ReadFinalAsync(local, ownPath, TestContext.Current.CancellationToken);
                    Assert.Equal(RecoveryBundleReadState.Valid, own.State);
                }
            }
        }
        finally
        {
            if (Directory.Exists(legacy.WorkspaceDirectory)) Directory.Delete(legacy.WorkspaceDirectory, recursive: true);
            else File.Delete(legacy.WorkspaceDirectory);
        }
    }

    private static async Task<string> SeedHistoryAsync(
        ExtensionInstallIntegrationWorkspace workspace, RecoveryStoreLocation legacy, string history)
    {
        if (history == UnavailableHistory)
        {
            Directory.CreateDirectory(legacy.RootPath);
            File.WriteAllText(legacy.WorkspaceDirectory, "unreadable legacy catalogue");
            return legacy.WorkspaceDirectory;
        }
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        if (history == VerifiedHistory)
        {
            var preparation = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
                workspace, ExtensionInstallDefinitions.CommandIdentity, RecoveryBundleOperation.Install, TestContext.Current.CancellationToken);
            return await ExtensionRecoveryHistoryFixture.CopyToLegacyAsync(preparation, TestContext.Current.CancellationToken);
        }
        var fileName = history switch
        {
            DraftHistory => RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()),
            DamagedHistory => RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()),
            _ => throw new ArgumentOutOfRangeException(nameof(history)),
        };
        var candidate = Path.Combine(legacy.WorkspaceDirectory, fileName);
        File.WriteAllText(candidate, "retained legacy recovery bytes");
        return candidate;
    }

    private static string[] Arguments(RecoveryBundleOperation command, string source)
        => ExtensionRecoveryHistoryFixture.Arguments(command, source, PackageId);
}
