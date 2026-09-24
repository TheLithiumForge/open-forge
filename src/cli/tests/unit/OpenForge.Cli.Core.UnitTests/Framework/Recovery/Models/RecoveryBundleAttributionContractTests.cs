using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models;

public sealed class RecoveryBundleAttributionContractTests
{
    public enum AttributionDefect
    {
        MissingProducer,
        UnknownProducer,
        MissingOperation,
        UnknownOperation,
        MissingSubject,
        UnknownSubject,
        InvalidTuple,
        SubjectWorkspaceMismatch,
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Recovery schema v1 rejects incomplete unknown and contradictory attribution")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    [InlineData(AttributionDefect.MissingProducer)]
    [InlineData(AttributionDefect.UnknownProducer)]
    [InlineData(AttributionDefect.MissingOperation)]
    [InlineData(AttributionDefect.UnknownOperation)]
    [InlineData(AttributionDefect.MissingSubject)]
    [InlineData(AttributionDefect.UnknownSubject)]
    [InlineData(AttributionDefect.InvalidTuple)]
    [InlineData(AttributionDefect.SubjectWorkspaceMismatch)]
    public void SchemaV1RejectsInvalidAttribution(AttributionDefect defect)
    {
        var (input, entry) = ManifestInput();
        var canonical = Encoding.UTF8.GetString(
            RecoveryBundleManifestCodec.Serialize(input, [entry]));
        var workspaceKey = WorkspaceIdentity.Key(input.Workspace.PhysicalRoot);
        var changed = defect switch
        {
            AttributionDefect.MissingProducer => canonical.Replace(
                "\"producer\":\"index\",",
                string.Empty,
                StringComparison.Ordinal),
            AttributionDefect.UnknownProducer => canonical.Replace(
                "\"producer\":\"index\"",
                "\"producer\":\"future\"",
                StringComparison.Ordinal),
            AttributionDefect.MissingOperation => canonical.Replace(
                "\"operation\":\"index\",",
                string.Empty,
                StringComparison.Ordinal),
            AttributionDefect.UnknownOperation => canonical.Replace(
                "\"operation\":\"index\"",
                "\"operation\":\"future\"",
                StringComparison.Ordinal),
            AttributionDefect.MissingSubject => canonical.Replace(
                $",\"subject\":{{\"kind\":\"workspace\",\"identity\":\"{workspaceKey}\"}}",
                string.Empty,
                StringComparison.Ordinal),
            AttributionDefect.UnknownSubject => canonical.Replace(
                "\"kind\":\"workspace\"",
                "\"kind\":\"future\"",
                StringComparison.Ordinal),
            AttributionDefect.InvalidTuple => canonical.Replace(
                "\"operation\":\"index\"",
                "\"operation\":\"install\"",
                StringComparison.Ordinal),
            AttributionDefect.SubjectWorkspaceMismatch => canonical.Replace(
                $"\"identity\":\"{workspaceKey}\"",
                $"\"identity\":\"{new string('f', RecoveryBundleFormatV1.Sha256HexLength)}\"",
                StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(defect), defect, "The attribution defect is not defined."),
        };

        Assert.NotEqual(canonical, changed);
        var decoded = RecoveryBundleManifestCodec.Decode(Encoding.UTF8.GetBytes(changed));

        Assert.Equal(RecoveryBundleManifestState.Malformed, decoded.State);
        Assert.Null(decoded.Attribution);
        Assert.Empty(decoded.Entries);
    }

    [Trait("Feature", "unified-remove"), Trait("Evidence", "Unit")]
    [Fact(DisplayName = "Recovery attribution admits Workspace Remove and rejects other Workspace operations")]
    public void WorkspaceProducerIsScopedToRemove()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"open-forge-workspace-remove-{Guid.NewGuid():N}"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var attribution = RecoveryBundleAttribution.Create(
            RecoveryBundleProducer.Workspace,
            RecoveryBundleOperation.Remove,
            workspace);

        Assert.Equal(RecoveryBundleProducer.Workspace, attribution.Producer);
        Assert.Equal(RecoveryBundleOperation.Remove, attribution.Operation);
        Assert.Throws<ArgumentException>(() => RecoveryBundleAttribution.Create(
            RecoveryBundleProducer.Workspace,
            RecoveryBundleOperation.Update,
            workspace));
    }

    [Trait("Feature", "unified-remove"), Trait("Evidence", "Unit")]
    [Fact(DisplayName = "Recovery schema v1 round-trips the Workspace Remove attribution")]
    public void WorkspaceRemoveAttributionUsesItsStableWireName()
    {
        var (input, entry) = ManifestInput(
            RecoveryBundleProducer.Workspace,
            RecoveryBundleOperation.Remove,
            "remove");

        var payload = RecoveryBundleManifestCodec.Serialize(input, [entry]);
        var serialized = Encoding.UTF8.GetString(payload);
        var decoded = RecoveryBundleManifestCodec.Decode(payload);

        Assert.Contains("\"producer\":\"workspace\"", serialized, StringComparison.Ordinal);
        Assert.Equal(RecoveryBundleManifestState.Valid, decoded.State);
        Assert.Equal(RecoveryBundleProducer.Workspace, decoded.Attribution?.Producer);
        Assert.Equal(RecoveryBundleOperation.Remove, decoded.Attribution?.Operation);
        Assert.Single(decoded.Entries);
    }

    private static (RecoveryBundleInput Input, RecoveryEntry Entry) ManifestInput(
        RecoveryBundleProducer producer = RecoveryBundleProducer.Index,
        RecoveryBundleOperation operation = RecoveryBundleOperation.Index,
        string command = "index")
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"open-forge-recovery-attribution-{Guid.NewGuid():N}"));
        var workspace = new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = Path.Combine(root, "existing.bin");
        var before = FileStateSnapshot.File(path, path, "before"u8);
        var input = RecoveryBundleInput.Create(
            workspace,
            command,
            attribution: RecoveryBundleAttribution.Create(
                producer,
                operation,
                workspace),
            operationId: Guid.NewGuid(),
            targets:
            [
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Delete(before.Expectation),
                    before),
            ]);
        return (
            input,
            RecoveryEntry.FromTarget(input, input.RecoveryTargets[0], ordinal: 0));
    }
}
