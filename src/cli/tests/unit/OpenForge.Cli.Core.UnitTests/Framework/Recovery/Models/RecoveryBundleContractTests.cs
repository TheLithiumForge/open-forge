using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models;

public sealed class RecoveryBundleContractTests
{
    public enum ManifestMutation
    {
        DuplicateProperty,
        UnknownProperty,
        UnsupportedSchema,
    }

    public enum ExpectedManifestState
    {
        Malformed,
        Unsupported,
    }

    [Fact(DisplayName = "Recovery content identity requires cohesive lowercase SHA-256 facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ContentIdentityEnforcesLengthAndHash()
    {
        var identity = RecoveryContentIdentity.FromBytes([0, 1, 2, 255]);

        Assert.Equal(4, identity.Length);
        Assert.True(identity.Matches([0, 1, 2, 255]));
        Assert.False(identity.Matches([0, 1, 2]));
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryContentIdentity.Create(-1, identity.Sha256));
        Assert.Throws<ArgumentException>(() => RecoveryContentIdentity.Create(4, identity.Sha256.ToUpperInvariant()));
    }

    [Fact(DisplayName = "Recovery input includes existing effects and excludes creates"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void InputFormsExactExistingTargetCoverage()
    {
        var workspace = Workspace();
        var replacePath = Path.Combine(workspace.LexicalRoot, "replace.bin");
        var deletePath = Path.Combine(workspace.LexicalRoot, "delete.bin");
        var generatedPath = Path.Combine(workspace.LexicalRoot, "generated.bin");
        var createPath = Path.Combine(workspace.LexicalRoot, "created.bin");
        var replaceBefore = FileStateSnapshot.File(replacePath, replacePath, [0, 1, 2, 255]);
        var deleteBefore = FileStateSnapshot.File(deletePath, deletePath, [6, 7]);
        var generatedBefore = FileStateSnapshot.File(generatedPath, generatedPath, [8, 9]);
        var replace = PlannedFileChange.Replace(replaceBefore.Expectation, [3, 4, 5]);
        var delete = PlannedFileChange.Delete(deleteBefore.Expectation);
        var generated = PlannedFileChange.ReplaceGeneratedRegion(
            generatedBefore.Expectation,
            [10, 11]);
        var create = PlannedFileChange.Create(FileExpectation.Missing(createPath), [6]);
        var input = RecoveryBundleInput.Create(
            workspace,
            command: "index",
            attribution: RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId: Guid.NewGuid(),
            targets:
            [
                RecoveryBundleTarget.Create(replace, replaceBefore),
                RecoveryBundleTarget.Create(delete, deleteBefore),
                RecoveryBundleTarget.Create(generated, generatedBefore),
                RecoveryBundleTarget.Create(create, FileStateSnapshot.Missing(createPath)),
            ]);

        var entries = input.RecoveryTargets
            .Select((target, ordinal) => RecoveryBundleEntry.FromTarget(input, target, ordinal))
            .ToArray();

        Assert.Equal(4, input.Targets.Length);
        Assert.Equal(3, input.RecoveryTargets.Length);
        Assert.Equal(
            [
                PlannedFileChangeKind.Replace,
                PlannedFileChangeKind.Delete,
                PlannedFileChangeKind.ReplaceGeneratedRegion,
            ],
            entries.Select(entry => entry.ChangeKind));
        Assert.Equal(["replace.bin", "delete.bin", "generated.bin"], entries.Select(entry => entry.TargetPath));
        Assert.True(entries[0].Prior.Matches([0, 1, 2, 255]));
        Assert.True(entries[0].Intended?.Matches([3, 4, 5]));
        Assert.Null(entries[1].Intended);
        Assert.True(entries[2].Intended?.Matches([10, 11]));
    }

    [Fact(DisplayName = "Recovery targets reject mismatched prior snapshots"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void TargetRejectsMismatchedPriorState()
    {
        var workspace = Workspace();
        var path = Path.Combine(workspace.LexicalRoot, "existing.bin");
        var other = Path.Combine(workspace.LexicalRoot, "other.bin");
        var before = FileStateSnapshot.File(path, path, "before"u8);
        var change = PlannedFileChange.Delete(before.Expectation);

        Assert.Throws<ArgumentException>(() => RecoveryBundleTarget.Create(
            change,
            FileStateSnapshot.File(other, other, "before"u8)));
        Assert.Throws<ArgumentException>(() => RecoveryBundleTarget.Create(
            change,
            FileStateSnapshot.Missing(path)));
        Assert.Throws<ArgumentException>(() => RecoveryBundleEntry.Create(
            ordinal: 0,
            targetPath: "existing.bin",
            changeKind: PlannedFileChangeKind.Delete,
            prior: RecoveryContentIdentity.FromBytes("before"u8),
            intended: RecoveryContentIdentity.FromBytes("after"u8)));
        Assert.Throws<ArgumentException>(() => RecoveryBundleEntry.Create(
            ordinal: 0,
            targetPath: "existing.bin",
            changeKind: PlannedFileChangeKind.ReplaceGeneratedRegion,
            prior: RecoveryContentIdentity.FromBytes("before"u8),
            intended: null));
    }

    [Fact(DisplayName = "Recovery identity uses exact deterministic final and draft names"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PathIdentityIsExternalNormalizedAndDeterministic()
    {
        var workspace = Workspace();
        var operationId = Guid.NewGuid();
        var root = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("LocalApplicationData must be observable for path evidence.");
        var key = WorkspaceIdentity.Key(workspace.PhysicalRoot);

        Assert.Equal(RecoveryBundleFormatV1.Sha256HexLength, key.Length);
        Assert.Equal(key, WorkspaceIdentity.Key(
            Path.Combine(workspace.PhysicalRoot, ".")));
        Assert.Equal(
            Path.Combine(root, key, $"operation-{operationId:N}.zip"),
            RecoveryBundlePathIdentity.FinalPath(root, workspace.PhysicalRoot, operationId));
        Assert.Equal(
            Path.Combine(root, key, $"operation-{operationId:N}.draft"),
            RecoveryBundlePathIdentity.DraftPath(root, workspace.PhysicalRoot, operationId));
        Assert.True(RecoveryBundleFormatV1.TryParseCandidateFileName(
            $"operation-{operationId:N}.zip",
            out var parsed,
            out var kind));
        Assert.Equal(operationId, parsed);
        Assert.Equal(RecoveryBundleCandidateKind.Final, kind);
        Assert.False(RecoveryBundleFormatV1.TryParseCandidateFileName(
            $"operation-{operationId:D}.zip",
            out _,
            out _));
    }

    [Fact(DisplayName = "Recovery manifest accepts harmless JSON formatting and property order"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ManifestAcceptsSemanticJsonFormatting()
    {
        var (input, entry) = ManifestInput();
        var canonical = Encoding.UTF8.GetString(
            RecoveryBundleManifestCodec.Serialize(input, [entry]));
        var semantic = canonical
            .Replace(
                "{\"schemaVersion\":1,\"command\":\"index\"",
                "{\n  \"command\": \"index\",\n  \"schemaVersion\": 1",
                StringComparison.Ordinal)
            .Replace("\":", "\": ", StringComparison.Ordinal);

        var decoded = RecoveryBundleManifestCodec.Decode(Encoding.UTF8.GetBytes(semantic));

        Assert.Equal(RecoveryBundleManifestState.Valid, decoded.State);
        Assert.Null(decoded.Cause);
        Assert.Single(decoded.Entries);
    }

    [Theory(DisplayName = "Recovery manifest rejects duplicate unmapped and unsupported facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    [InlineData(ManifestMutation.DuplicateProperty, ExpectedManifestState.Malformed)]
    [InlineData(ManifestMutation.UnknownProperty, ExpectedManifestState.Malformed)]
    [InlineData(ManifestMutation.UnsupportedSchema, ExpectedManifestState.Unsupported)]
    public void ManifestRejectsContradictoryOrEvolvingJson(
        ManifestMutation mutation,
        ExpectedManifestState expectedState)
    {
        var (input, entry) = ManifestInput();
        var canonical = Encoding.UTF8.GetString(
            RecoveryBundleManifestCodec.Serialize(input, [entry]));
        var changed = mutation switch
        {
            ManifestMutation.DuplicateProperty => canonical.Replace(
                "{\"schemaVersion\":1,",
                "{\"schemaVersion\":1,\"schemaVersion\":1,",
                StringComparison.Ordinal),
            ManifestMutation.UnknownProperty => canonical.Replace(
                "{\"schemaVersion\":1,",
                "{\"schemaVersion\":1,\"future\":true,",
                StringComparison.Ordinal),
            ManifestMutation.UnsupportedSchema => canonical.Replace(
                "{\"schemaVersion\":1,",
                "{\"schemaVersion\":2,",
                StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        var decoded = RecoveryBundleManifestCodec.Decode(Encoding.UTF8.GetBytes(changed));

        Assert.Equal(
            expectedState == ExpectedManifestState.Unsupported
                ? RecoveryBundleManifestState.Unsupported
                : RecoveryBundleManifestState.Malformed,
            decoded.State);
        Assert.NotNull(decoded.Cause);
    }

    [Fact(DisplayName = "Recovery preparation cannot be synthesized from scalar read facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PreparationRejectsUnauthorizedReadbackToken()
    {
        var verified = new RecoveryBundleVerifiedRead
        {
            BundlePath = Path.Combine(Path.GetTempPath(), "operation.zip"),
            WorkspacePhysicalPath = Path.GetTempPath(),
            WorkspaceKey = new string('0', 64),
            Command = "index",
            Attribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                Workspace()),
            OperationId = Guid.NewGuid(),
            Entries = [],
        };

        Assert.Throws<ArgumentException>(() => new RecoveryBundleReader.VerifiedFinalToken(
            verified,
            new object()));
    }

    [Fact(DisplayName = "Recovery preparation failures carry only honest residual paths"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PreparationResultCarriesResidualPath()
    {
        var path = Path.Combine(Path.GetTempPath(), $"operation-{Guid.NewGuid():N}.draft");

        Assert.Equal(path, RecoveryBundlePreparationResult.Cancelled(path).ResidualPath);
        Assert.Null(RecoveryBundlePreparationResult.Cancelled().ResidualPath);
        Assert.Equal(
            path,
            RecoveryBundlePreparationResult.Incomplete("storage failed", path).ResidualPath);
    }

    [Fact(DisplayName = "Recovery deletion results retain every valid state and disposition shape"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void DeletionResultsPreserveValidStateAndDispositionShapes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"operation-{Guid.NewGuid():N}.zip");
        var failure = new FilesystemFailure(
            FilesystemFailureKind.InputOutput,
            "The deletion boundary failed.");
        var results = new[]
        {
            RecoveryBundleDeletionResult.Deleted(),
            RecoveryBundleDeletionResult.FailedRetained(path, "Deletion failed.", failure),
            RecoveryBundleDeletionResult.FailedUnknown(path, "Deletion outcome is unknown.", failure),
            RecoveryBundleDeletionResult.BlockedRetained(path, "Deletion was blocked."),
            RecoveryBundleDeletionResult.BlockedUnknown("Deletion was blocked."),
            RecoveryBundleDeletionResult.CancelledRetained(path),
            RecoveryBundleDeletionResult.CancelledUnknown(),
        };

        Assert.Equal(
            [
                (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed),
                (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained),
                (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Unknown),
                (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Retained),
                (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Unknown),
                (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained),
                (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Unknown),
            ],
            results.Select(result => (result.State, result.Disposition)));
        Assert.Equal(path, results[1].ResidualPath);
        Assert.Equal(path, results[2].ResidualPath);
        Assert.Null(results[0].ResidualPath);
        Assert.Null(results[4].ResidualPath);
        Assert.Null(results[6].ResidualPath);
    }

    [Fact(DisplayName = "Recovery deletion results reject impossible state and disposition shapes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void DeletionResultsRejectImpossibleStateAndDispositionShapes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"operation-{Guid.NewGuid():N}.zip");
        (RecoveryBundleDeletionState State, RecoveryBundleDisposition Disposition)[] impossible =
        [
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Retained),
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Unknown),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Removed),
            (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Removed),
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Removed),
        ];

        foreach (var value in impossible)
        {
            var cause = value.State is RecoveryBundleDeletionState.Failed
                or RecoveryBundleDeletionState.Blocked
                    ? "Deletion did not complete."
                    : null;
            var residualPath = value.Disposition == RecoveryBundleDisposition.Retained
                ? path
                : null;
            Assert.Throws<ArgumentException>(() => new RecoveryBundleDeletionResult(
                state: value.State,
                disposition: value.Disposition,
                residualPath: residualPath,
                failure: null,
                cause: cause));
        }

        Assert.Throws<ArgumentException>(() => new RecoveryBundleDeletionResult(
            state: RecoveryBundleDeletionState.Failed,
            disposition: RecoveryBundleDisposition.Retained,
            residualPath: null,
            failure: null,
            cause: "Deletion failed."));
        Assert.Throws<ArgumentException>(() => new RecoveryBundleDeletionResult(
            state: RecoveryBundleDeletionState.Deleted,
            disposition: RecoveryBundleDisposition.Removed,
            residualPath: path,
            failure: null,
            cause: null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecoveryBundleDeletionResult(
            state: (RecoveryBundleDeletionState)int.MaxValue,
            disposition: RecoveryBundleDisposition.Unknown,
            residualPath: null,
            failure: null,
            cause: null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecoveryBundleDeletionResult(
            state: RecoveryBundleDeletionState.Blocked,
            disposition: (RecoveryBundleDisposition)int.MaxValue,
            residualPath: null,
            failure: null,
            cause: "Deletion was blocked."));
    }

    private static (RecoveryBundleInput Input, RecoveryBundleEntry Entry) ManifestInput()
    {
        var workspace = Workspace();
        var path = Path.Combine(workspace.LexicalRoot, "existing.bin");
        var before = FileStateSnapshot.File(path, path, [0, 1, 2]);
        var change = PlannedFileChange.Delete(before.Expectation);
        var input = RecoveryBundleInput.Create(
            workspace,
            command: "index",
            attribution: RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId: Guid.NewGuid(),
            targets: [RecoveryBundleTarget.Create(change, before)]);
        return (
            input,
            RecoveryBundleEntry.FromTarget(input, input.RecoveryTargets[0], ordinal: 0));
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"open-forge-recovery-contract-{Guid.NewGuid():N}"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
