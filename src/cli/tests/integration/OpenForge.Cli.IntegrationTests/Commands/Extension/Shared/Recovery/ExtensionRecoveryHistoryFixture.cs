using System.Collections.Frozen;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport.Isolation;
using System.Runtime.CompilerServices;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

internal static class ExtensionRecoveryHistoryFixture
{
    internal const string TargetPath = SourceLogicalPath.AgentsRoot + "/recovery-history.txt";


    private static readonly FrozenDictionary<RecoveryBundleOperation, (string Name, string? Source, string Automatic, string DryRun, string History, string Coverage)> Commands =
        new Dictionary<RecoveryBundleOperation, (string Name, string? Source, string Automatic, string DryRun, string History, string Coverage)>
        {
            [RecoveryBundleOperation.Install] = (ExtensionInstallDefinitions.InstallCommand.Name, ExtensionInstallDefinitions.Source.Name,
            ExtensionInstallDefinitions.Automatic.Name, ExtensionInstallDefinitions.DryRun.Name,
            ExtensionInstallDefinitions.ReadMachineName(ExtensionInstallFindingCode.RecoveryHistory),
            ExtensionInstallDefinitions.ReadMachineName(ExtensionInstallFindingCode.RecoveryCoverageUnavailable)),
            [RecoveryBundleOperation.Update] = (ExtensionUpdateDefinitions.UpdateCommand.Name, ExtensionUpdateDefinitions.Source.Name,
            ExtensionUpdateDefinitions.Automatic.Name, ExtensionUpdateDefinitions.DryRun.Name,
            ExtensionUpdateDefinitions.ReadMachineName(ExtensionUpdateFindingCode.RecoveryHistory),
            ExtensionUpdateDefinitions.ReadMachineName(ExtensionUpdateFindingCode.RecoveryCoverageUnavailable)),
            [RecoveryBundleOperation.Remove] = (ExtensionRemoveDefinitions.RemoveCommand.Name, null,
            ExtensionRemoveDefinitions.Automatic.Name, ExtensionRemoveDefinitions.DryRun.Name,
            ExtensionRemoveDefinitions.ReadMachineName(ExtensionRemoveFindingCode.RecoveryHistory),
            ExtensionRemoveDefinitions.ReadMachineName(ExtensionRemoveFindingCode.RecoveryCoverageUnavailable)),
        }.ToFrozenDictionary();

    internal static string[] Arguments(RecoveryBundleOperation command, string source, string package, bool dryRun = false)
    {
        var definition = Commands[command];
        List<string> arguments = [ExtensionDefinitions.ExtensionGroup.Name, definition.Name, package];
        if (definition.Source is { } option) arguments.AddRange([option, source]);
        arguments.Add(dryRun ? definition.DryRun : definition.Automatic);
        return arguments.ToArray();
    }

    internal static string HistoryCode(RecoveryBundleOperation command, bool unavailable = false)
        => unavailable ? Commands[command].Coverage : Commands[command].History;

    internal static string DryRunOption(RecoveryBundleOperation command) => Commands[command].DryRun;

    private static string NormalizedStoreDirectory(ExtensionInstallIntegrationWorkspace workspace)
    {
        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            WorkspaceRuntimePaths.RecoveryRoot(workspace.Workspace), workspace.Workspace.PhysicalRoot);
        var workspaceKey = Path.GetFileName(directory);
        return directory.Replace(workspace.Workspace.PhysicalRoot, "<workspace>", StringComparison.Ordinal)
            .Replace(workspaceKey, "<workspace-key>", StringComparison.Ordinal).Replace('\\', '/');
    }

    internal static async Task<string> CopyToLegacyAsync(RecoveryBundlePreparation preparation, CancellationToken cancellationToken)
    {
        var legacy = RecoveryStoreLocator.Legacy(preparation.Location.Workspace)
            ?? throw new InvalidOperationException("The recovery history fixture requires a legacy location.");
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var path = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
        File.Copy(preparation.BundlePath, path);
        var read = await RecoveryBundleReader.ReadFinalAsync(legacy, path, cancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, read.State);
        return path;
    }

    internal static async Task MatchHistoryAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string sourcePath,
        ReadOutputScenario scenario,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "")
    {
        var location = RecoveryStoreLocator.Local(workspace.Workspace);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        await new ReadCommandOutputCapture(workspace.Path, sourcePath, location.WorkspaceDirectory)
            .MatchDetailsAsync(scenario, snapshotCollector: captures);
        CommandOutputSnapshot.MatchDetailSnapshot(captures.ToDictionary(pair => pair.Key,
            pair => pair.Value.Replace("<recovery-bundle>",
                NormalizedStoreDirectory(workspace), StringComparison.Ordinal)),
            sourceFile, testName);
    }

    internal static async ValueTask<RecoveryBundlePreparation> CreateVerifiedFinalAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string command,
        RecoveryBundleOperation operation,
        CancellationToken cancellationToken)
    {
        const string priorContents = "prior retained history bytes\n";
        workspace.CreateOccupant(TargetPath, priorContents);
        var targetPath = workspace.Combine(TargetPath);
        var prior = FileStateSnapshot.File(targetPath, targetPath, Encoding.UTF8.GetBytes(priorContents));
        var change = PlannedFileChange.Replace(
            prior.Expectation,
            "proposed recovery bytes\n"u8);
        var input = RecoveryBundleInput.Create(
            workspace.Workspace,
            command,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Extension,
                operation,
                workspace.Workspace),
            Guid.NewGuid(),
            [RecoveryBundleTarget.Create(change, prior)]);
        var result = await RecoveryBundleStore.PrepareAsync(input, cancellationToken).ConfigureAwait(false);
        return result.Preparation
            ?? throw new InvalidOperationException(
                result.Cause ?? "The Extension recovery history fixture did not create a final bundle.");
    }

    internal static string CandidatePath(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace,
        Guid operationId,
        RecoveryBundleCandidateKind kind)
    {
        var storeRoot = TestDataHome.LocalRecoveryStoreRoot(workspace.PhysicalRoot);
        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, workspace.PhysicalRoot);
        Directory.CreateDirectory(directory);
        var fileName = kind switch
        {
            RecoveryBundleCandidateKind.Final => RecoveryBundleFormatV1.FinalFileName(operationId),
            RecoveryBundleCandidateKind.Draft => RecoveryBundleFormatV1.DraftFileName(operationId),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery candidate kind is not defined."),
        };
        return Path.Combine(directory, fileName);
    }
}
