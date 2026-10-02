using System.Collections.Immutable;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateAdoptionEffectPlanningTests
{
    [Fact(DisplayName = "Update coalesces local adoption and generated Entries into one exact file effect"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void CoalescesLocalAdoptionAndGeneratedEntriesIntoOneExactEffect()
    {
        const string path = ".agents/memory/_memory.md";
        var original = new byte[] { 1, 2, 3 };
        var intended = new byte[] { 4, 5, 6 };
        var snapshot = FileSnapshot(path, original);
        var (plan, observations) = GeneratedReplacement(path, snapshot, original, intended);
        var target = new UpdateAdoptionTarget(
            path,
            snapshot,
            intended,
            OwnsGeneratedEntries: true);

        var effect = Assert.Single(UpdatePhysicalEffectPlanner.Plan(
            plan,
            observations,
            [target]));

        Assert.Equal(path, effect.ResultEffect.Path);
        Assert.Equal(UpdatePhysicalEffectAction.Replace, effect.ResultEffect.Action);
        Assert.Equal(2, effect.ResultEffect.Changes.Count);
        Assert.Contains(effect.ResultEffect.Changes, change =>
            change.Kind == UpdateComparisonTargetKind.GeneratedRegion
            && change.Region == "entries");
        var localChange = Assert.Single(effect.ResultEffect.Changes, change =>
            change.Kind == UpdateComparisonTargetKind.File);
        Assert.Equal(UpdateLogicalChangeAction.Replace, localChange.Action);
        Assert.Null(localChange.Region);
        Assert.Null(localChange.SourceAssetPath);
        Assert.Equal(PlannedFileChangeKind.Replace, effect.FileChange.Kind);
        Assert.Equal(intended, effect.FileChange.IntendedBytes.ToArray());
    }

    [Fact(DisplayName = "Update adoption replacements preserve the exact original file expectation"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void AdoptionReplacementKeepsExactOriginalExpectation()
    {
        const string path = ".agents/skills/task70/SKILL.md";
        var original = new byte[] { 8, 9 };
        var snapshot = FileSnapshot(path, original);
        var target = new UpdateAdoptionTarget(
            path,
            snapshot,
            [10, 11],
            OwnsGeneratedEntries: false);

        var effect = Assert.Single(UpdatePhysicalEffectPlanner.Plan(
            EmptyPlan(),
            [],
            [target]));

        Assert.Equal(snapshot.Expectation, effect.FileChange.Expectation);
        Assert.Equal(PlannedFileChangeKind.Replace, effect.FileChange.Kind);
    }

    [Fact(DisplayName = "Update adoption file changes keep null Framework provenance"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void LocalAdoptionLogicalChangeHasNoFrameworkProvenance()
    {
        const string path = ".agents/skills/task70/SKILL.md";
        var snapshot = FileSnapshot(path, [1]);
        var target = new UpdateAdoptionTarget(path, snapshot, [2], OwnsGeneratedEntries: false);

        var effect = Assert.Single(UpdatePhysicalEffectPlanner.Plan(
            EmptyPlan(),
            [],
            [target]));
        var change = Assert.Single(effect.ResultEffect.Changes);

        Assert.Equal(UpdatePhysicalEffectKind.File, effect.ResultEffect.Kind);
        Assert.Equal(UpdateComparisonTargetKind.File, change.Kind);
        Assert.Equal(UpdateLogicalChangeAction.Replace, change.Action);
        Assert.Null(change.Region);
        Assert.Null(change.SourceAssetPath);
    }

    [Fact(DisplayName = "Update omits an exact-byte local adoption no-op"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void ExactByteNoOpProducesNoPhysicalEffect()
    {
        const string path = ".agents/skills/task70/SKILL.md";
        var current = new byte[] { 7, 8, 9 };
        var snapshot = FileSnapshot(path, current);
        var target = new UpdateAdoptionTarget(path, snapshot, current.ToArray(), OwnsGeneratedEntries: false);

        var effects = UpdatePhysicalEffectPlanner.Plan(
            EmptyPlan(),
            [],
            [target]);

        Assert.Empty(effects);
    }

    [Fact(DisplayName = "Update adoption distinguishes create and replace from exact snapshots"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void AdoptionUsesCreateForMissingAndReplaceForExistingFiles()
    {
        const string createPath = ".agents/skills/new/SKILL.md";
        const string replacePath = ".agents/skills/existing/SKILL.md";
        var create = new UpdateAdoptionTarget(
            createPath,
            FileStateSnapshot.Missing(PhysicalPath(createPath)),
            [1, 2],
            OwnsGeneratedEntries: false);
        var replace = new UpdateAdoptionTarget(
            replacePath,
            FileSnapshot(replacePath, [3]),
            [4, 5],
            OwnsGeneratedEntries: false);

        var effects = UpdatePhysicalEffectPlanner.Plan(
            EmptyPlan(),
            [],
            [create, replace]);

        Assert.Equal(PlannedFileChangeKind.Create, effects.Single(effect =>
            effect.ResultEffect.Path == createPath).FileChange.Kind);
        Assert.Equal(PlannedFileChangeKind.Replace, effects.Single(effect =>
            effect.ResultEffect.Path == replacePath).FileChange.Kind);
        Assert.Equal(PhysicalPath(createPath), create.Snapshot.LogicalPath);
        Assert.Equal(PhysicalPath(replacePath), replace.Snapshot.LogicalPath);
    }

    [Fact(DisplayName = "Update rejects a shared adoption snapshot that differs from the Framework snapshot"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void ConflictingSharedSnapshotsFailClosed()
    {
        const string path = ".agents/memory/_memory.md";
        var original = new byte[] { 1, 2 };
        var intended = new byte[] { 3, 4 };
        var frameworkSnapshot = FileSnapshot(path, original);
        var (plan, observations) = GeneratedReplacement(path, frameworkSnapshot, original, intended);
        var adoptionSnapshot = FileSnapshot(path, [5, 6]);
        var target = new UpdateAdoptionTarget(
            path,
            adoptionSnapshot,
            intended,
            OwnsGeneratedEntries: true);

        Assert.Throws<InvalidOperationException>(() => UpdatePhysicalEffectPlanner.Plan(
            plan,
            observations,
            [target]));
    }

    [Fact(DisplayName = "Update rejects adoption bytes that would overwrite a planned generated Entries change"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void ConflictingSharedFinalBytesFailClosed()
    {
        const string path = ".agents/memory/_memory.md";
        var original = new byte[] { 1, 2 };
        var generatedFinal = new byte[] { 3, 4 };
        var conflictingFinal = new byte[] { 5, 6 };
        var snapshot = FileSnapshot(path, original);
        var (plan, observations) = GeneratedReplacement(path, snapshot, original, generatedFinal);
        var target = new UpdateAdoptionTarget(
            path,
            snapshot,
            conflictingFinal,
            OwnsGeneratedEntries: true);

        Assert.Throws<InvalidOperationException>(() => UpdatePhysicalEffectPlanner.Plan(
            plan,
            observations,
            [target]));
    }

    [Fact(DisplayName = "Update recovery uses the exact captured original for an adoption-only replacement"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public Task RecoveryTargetUsesExactAdoptedSkillBeforeState()
        => WithAdoptionExecutionAsync(
            "task70-update-recovery-replace",
            (execution, skillPath, _, originalSkillBytes) =>
            {
                var adoption = Assert.Single(execution.AdoptionTargets, target => target.Path == skillPath);
                var effect = Assert.Single(execution.Effects, value => value.ResultEffect.Path == skillPath);
                Assert.True(Path.IsPathFullyQualified(adoption.Snapshot.LogicalPath));
                Assert.False(Path.IsPathFullyQualified(effect.ResultEffect.Path));
                Assert.DoesNotContain(execution.Observations, value =>
                    value.Comparison.RelativePath == skillPath);

                var recoveryTarget = Assert.Single(
                    UpdateRecoveryOperation.ReadTargets(execution),
                    target => target.Change.LogicalPath == effect.FileChange.LogicalPath);

                Assert.Equal(PlannedFileChangeKind.Replace, recoveryTarget.Change.Kind);
                Assert.Equal(adoption.Snapshot.Expectation, recoveryTarget.Before.Expectation);
                Assert.Equal(originalSkillBytes, recoveryTarget.Before.Bytes.ToArray());
                Assert.True(recoveryTarget.RequiresRecovery);
                return Task.CompletedTask;
            });

    [Fact(DisplayName = "Update recovery preserves exact missing state for an adoption-only catalogue create"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public Task RecoveryTargetKeepsAdoptionOnlyCatalogueCreateNonReversible()
        => WithAdoptionExecutionAsync(
            "task70-update-recovery-catalogue-create",
            (execution, _, cataloguePath, _) =>
            {
                var adoption = Assert.Single(execution.AdoptionTargets, target => target.Path == cataloguePath);
                var effect = Assert.Single(execution.Effects, value => value.ResultEffect.Path == cataloguePath);
                Assert.True(Path.IsPathFullyQualified(adoption.Snapshot.LogicalPath));
                Assert.False(Path.IsPathFullyQualified(effect.ResultEffect.Path));
                Assert.DoesNotContain(execution.Observations, value =>
                    value.Comparison.RelativePath == cataloguePath);

                var recoveryTarget = Assert.Single(
                    UpdateRecoveryOperation.ReadTargets(execution),
                    target => target.Change.LogicalPath == effect.FileChange.LogicalPath);

                Assert.Equal(PlannedFileChangeKind.Create, recoveryTarget.Change.Kind);
                Assert.Equal(FileExpectationKind.Missing, adoption.Snapshot.Kind);
                Assert.False(adoption.Snapshot.HasBytes);
                Assert.Equal(adoption.Snapshot.Expectation, recoveryTarget.Before.Expectation);
                Assert.False(recoveryTarget.Before.HasBytes);
                Assert.False(recoveryTarget.RequiresRecovery);
                return Task.CompletedTask;
            });

    [Fact(DisplayName = "Update recovery fails closed when an effect has no captured source state"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public Task RecoveryTargetsFailClosedWithoutCapturedSourceState()
        => WithAdoptionExecutionAsync(
            "task70-update-recovery-missing-source",
            (execution, skillPath, _, _) =>
            {
                var effectPath = Assert.Single(
                    execution.Effects,
                    value => value.ResultEffect.Path == skillPath).ResultEffect.Path;
                var invalidExecution = execution with
                {
                    Observations = execution.Observations
                        .Where(value => value.Comparison.RelativePath != effectPath)
                        .ToArray(),
                    AdoptionTargets = execution.AdoptionTargets
                        .Where(value => value.Path != effectPath)
                        .ToArray(),
                };

                Assert.DoesNotContain(invalidExecution.Observations, value =>
                    value.Comparison.RelativePath == effectPath);
                Assert.DoesNotContain(invalidExecution.AdoptionTargets, value => value.Path == effectPath);
                Assert.Contains(invalidExecution.Effects, value => value.ResultEffect.Path == effectPath);
                Assert.Throws<InvalidOperationException>(() =>
                    UpdateRecoveryOperation.ReadTargets(invalidExecution));
                return Task.CompletedTask;
            });

    [Fact(DisplayName = "Update lease revalidation detects changed adoption targets and migration facts"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public async Task RevalidationDetectsChangedAdoptionAndMigrationFactsUnderLease()
    {
        const string skillPath = ".agents/skills/task70-lease-fixture/SKILL.md";
        using var workspaceScratch = TemporaryWorkspace.Create("task70-update-lease-revalidation");
        using var dataHome = TestDataHome.Create("task70-update-lease-revalidation");
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        Assert.Equal(FrameworkPayloadReadState.Available, payloadRead.State);
        var payload = payloadRead.Payload
            ?? throw new InvalidOperationException("The unit fixture requires the embedded Framework payload.");
        var workspace = new CliWorkspace(
            workspaceScratch.Path,
            workspaceScratch.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var lockStoreRoot = WorkspaceLockStoreRoot.FromAbsolutePath(dataHome.Path);
        PrepareInstallDirectories(workspaceScratch, payload);

        try
        {
            var install = InstallOperationFactory.Create(
                (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()),
                lockStoreRoot);
            var installed = await install.ExecuteAsync(
                new InstallRequest(
                    workspace,
                    InstallMode.Apply,
                    force: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, installed.Status);

            workspaceScratch.CreateDirectory(".agents/skills/task70-lease-fixture");
            workspaceScratch.CreateFile(
                skillPath,
                "---\nname: task70-lease-fixture\nlicense: Task70-lease-sentinel\n---\n\n# Task 70 lease fixture\n");
            var request = new UpdateRequest(
                workspace,
                UpdateMode.Apply,
                force: false,
                prune: false,
                automatic: true,
                allowsInteractiveConfirmation: false);
            var built = await UpdatePlanBuilder.Create().BuildExecutionAsync(
                request,
                TestContext.Current.CancellationToken);
            var execution = built.Execution
                ?? throw new InvalidOperationException("The adoption fixture must build one complete Update execution.");
            Assert.NotEmpty(execution.AdoptionTargets);
            Assert.NotEmpty(execution.Migrations);
            Assert.Contains(execution.AdoptionTargets, target => target.Path == skillPath);
            Assert.Contains(execution.Migrations, migration => migration.Path == skillPath);

            var lockResult = await new WorkspaceLockManager(lockStoreRoot).AcquireAsync(
                new WorkspaceLockRequest(workspace, "update", Guid.NewGuid()),
                TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockState.Acquired, lockResult.State);
            var lease = lockResult.Lease
                ?? throw new InvalidOperationException("A successful workspace lock result requires its lease.");
            await using (lease.ConfigureAwait(false))
            {
                var revalidator = new UpdatePlanRevalidator(
                    UpdatePlanBuilder.Create(),
                    new MutationRevalidator(new FileExpectationValidator(new PhysicalPathResolver())));
                var changedTargetExecution = execution with
                {
                    AdoptionTargets = [],
                };
                var changedTargets = await revalidator.RevalidateAsync(
                    changedTargetExecution,
                    lease,
                    TestContext.Current.CancellationToken);
                Assert.Equal(UpdatePlanRevalidationState.Changed, changedTargets.State);

                var changedMigrationExecution = execution with
                {
                    Migrations = [],
                };
                var changedMigrations = await revalidator.RevalidateAsync(
                    changedMigrationExecution,
                    lease,
                    TestContext.Current.CancellationToken);
                Assert.Equal(UpdatePlanRevalidationState.Changed, changedMigrations.State);
            }
        }
        finally
        {
            CleanupInstalledFiles(workspaceScratch, payload, skillPath);
        }
    }

    private static UpdatePlanningPlan EmptyPlan()
        => new(new UpdatePlanningAuthority(force: false, prune: false), []);

    private static async Task WithAdoptionExecutionAsync(
        string fixtureName,
        Func<UpdatePlanExecution, string, string, byte[], Task> verify)
    {
        const string skillPath = ".agents/skills/task70-lease-fixture/SKILL.md";
        const string guidePath = ".agents/skills/task70-lease-fixture/references/guide.md";
        const string cataloguePath = ".agents/skills/task70-lease-fixture/references/_references.md";
        using var workspaceScratch = TemporaryWorkspace.Create(fixtureName);
        using var dataHome = TestDataHome.Create(fixtureName);
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        Assert.Equal(FrameworkPayloadReadState.Available, payloadRead.State);
        var payload = payloadRead.Payload
            ?? throw new InvalidOperationException("The unit fixture requires the embedded Framework payload.");
        var workspace = new CliWorkspace(
            workspaceScratch.Path,
            workspaceScratch.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var lockStoreRoot = WorkspaceLockStoreRoot.FromAbsolutePath(dataHome.Path);
        PrepareInstallDirectories(workspaceScratch, payload);

        try
        {
            var install = InstallOperationFactory.Create(
                (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()),
                lockStoreRoot);
            var installed = await install.ExecuteAsync(
                new InstallRequest(
                    workspace,
                    InstallMode.Apply,
                    force: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, installed.Status);

            workspaceScratch.CreateDirectory(Path.GetDirectoryName(guidePath)!);
            workspaceScratch.CreateFile(
                skillPath,
                "---\nname: task70-lease-fixture\nlicense: Task70-lease-sentinel\n---\n\n# Task 70 lease fixture\n");
            workspaceScratch.CreateFile(guidePath, "# Task 70 reference guide\n");
            var originalSkillBytes = File.ReadAllBytes(workspaceScratch.Combine(skillPath));
            var built = await UpdatePlanBuilder.Create().BuildExecutionAsync(
                new UpdateRequest(
                    workspace,
                    UpdateMode.Apply,
                    force: false,
                    prune: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);
            var execution = built.Execution
                ?? throw new InvalidOperationException("The adoption fixture must build one complete Update execution.");
            Assert.Contains(execution.AdoptionTargets, target => target.Path == skillPath);
            Assert.Contains(execution.AdoptionTargets, target => target.Path == cataloguePath);

            await verify(execution, skillPath, cataloguePath, originalSkillBytes).ConfigureAwait(false);
        }
        finally
        {
            CleanupInstalledFiles(workspaceScratch, payload, skillPath, guidePath);
        }
    }

    private static (UpdatePlanningPlan Plan, IReadOnlyList<UpdateComparisonObservation> Observations)
        GeneratedReplacement(
            string path,
            FileStateSnapshot snapshot,
            byte[] original,
            byte[] intended)
    {
        var comparison = new UpdateComparison
        {
            RelativePath = path,
            Kind = UpdateComparisonTargetKind.GeneratedRegion,
            RegionIdentity = "entries",
            SourceAssetPath = null,
            SourceAssetPresentInCurrentInventory = null,
            FingerprintKind = UpdateComparisonFingerprintKind.ExactBytes,
            CurrentFingerprint = Hash(original),
            IntendedFingerprint = Hash(intended),
            CurrentState = UpdateComparisonCurrentState.Changed,
            IntendedState = UpdateComparisonIntendedState.Changed,
            RetirementEligibility = UpdateRetirementEligibility.NotApplicable,
            CurrentBytes = ByteFacts(original),
            IntendedBytes = ByteFacts(intended),
        };
        comparison.Validate();

        return (
            new UpdatePlanningPlan(
                new UpdatePlanningAuthority(force: false, prune: false),
                [new UpdatePlanningDecision(comparison, UpdatePlanningDisposition.Replace)]),
            [new UpdateComparisonObservation(comparison, snapshot, intended)]);
    }

    private static UpdateComparisonByteFacts ByteFacts(byte[] bytes)
    {
        var exactBytes = ImmutableArray.CreateRange(bytes);
        return new UpdateComparisonByteFacts
        {
            ExactBytes = exactBytes,
            Sha256 = Hash(bytes),
        };
    }

    private static string Hash(byte[] bytes)
        => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static string PhysicalPath(string relativePath)
        => Path.Combine(
            Path.GetTempPath(),
            "open-forge-task70-effect-planning",
            relativePath.Replace('/', '_'));

    private static FileStateSnapshot FileSnapshot(string relativePath, byte[] bytes)
    {
        var physicalPath = Path.GetFullPath(PhysicalPath(relativePath));
        return FileStateSnapshot.File(physicalPath, physicalPath, bytes);
    }

    private static void PrepareInstallDirectories(
        TemporaryWorkspace workspace,
        FrameworkPayload payload)
    {
        foreach (var directory in payload.Assets
                     .Select(asset => Path.GetDirectoryName(asset.Path))
                     .Where(directory => !string.IsNullOrWhiteSpace(directory))
                     .OfType<string>()
                     .Append(".agents")
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(path => path.Count(character => character is '/' or '\\'))
                     .ThenBy(path => path, StringComparer.Ordinal))
        {
            workspace.CreateDirectory(directory.Replace('\\', '/'));
        }
    }

    private static void CleanupInstalledFiles(
        TemporaryWorkspace workspace,
        FrameworkPayload payload,
        string skillPath,
        params string[] additionalPaths)
    {
        foreach (var relativePath in payload.Assets
                     .Select(asset => asset.Path)
                     .Append(WorkspaceOwnershipDefinitions.RelativePath)
                     .Append(".agents/open-forge.lifecycle.json")
                     .Append(skillPath)
                     .Concat(additionalPaths)
                     .Distinct(StringComparer.Ordinal))
        {
            var path = workspace.Combine(relativePath);
            if (!File.Exists(path))
            {
                continue;
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("The Update lease fixture cleanup target is not an ordinary file.");
            }

            File.Delete(path);
        }
    }
}
