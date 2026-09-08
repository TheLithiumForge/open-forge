using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

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

    private static (RecoveryBundleInput Input, RecoveryEntry Entry) ManifestInput()
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
            command: "index",
            attribution: RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
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
