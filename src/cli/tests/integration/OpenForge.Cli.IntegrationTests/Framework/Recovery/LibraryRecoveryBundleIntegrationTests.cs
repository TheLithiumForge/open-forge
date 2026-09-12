using System.IO.Compression;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryBundleIntegrationTests
{
    [Fact(DisplayName = "Library recovery prepares one verified external schema-v1 bundle with link and prior-missing record identities and no source payload")]
    public async Task PreparesTypedBundleWithoutSourceBytes()
    {
        using var temporary = TemporaryWorkspace.Create("library-recovery-bundle");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("shared/.agents/a.md", "source payload must never be stored");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
        var create = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/a.md"), link);
        var delete = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/retired.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/retired.md"));
        var recordPath = temporary.Combine(".agents/open-forge.libraries.json");
        var record = PlannedFileChange.Create(FileExpectation.Missing(recordPath), "{\"schemaVersion\":1,\"libraries\":[]}"u8);
        var input = RecoveryBundleInput.Create(workspace, command: "library sync",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Sync, workspace),
            operationId: Guid.NewGuid(), targets:
            [
                RecoveryBundleTarget.Create(create, NoFollowLeafObservation.Missing(temporary.Combine(".agents/a.md"))),
                RecoveryBundleTarget.Create(delete, NoFollowLeafObservation.CreateRelativeFileLink(temporary.Combine(".agents/retired.md"), delete.Link)),
                RecoveryBundleTarget.CreateReversible(record, FileStateSnapshot.Missing(recordPath)),
            ]);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            var result = await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundlePreparationState.Prepared, result.State);
            preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
            Assert.False(preparation.BundlePath.StartsWith(temporary.Path, StringComparison.Ordinal));
            Assert.Equal(3, preparation.Entries.Length);
            Assert.All(preparation.Entries, entry => Assert.Null(entry.PriorPayload));
            using var archive = ZipFile.OpenRead(preparation.BundlePath);
            Assert.Equal("manifest.json", Assert.Single(archive.Entries).FullName);
            using var manifestStream = archive.Entries[0].Open();
            using var manifest = await JsonDocument.ParseAsync(manifestStream, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(1, manifest.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("library", manifest.RootElement.GetProperty("attribution").GetProperty("producer").GetString());
            Assert.Equal(["relative-file-link-create", "relative-file-link-delete", "ordinary-create"],
                manifest.RootElement.GetProperty("entries").EnumerateArray().Select(entry => entry.GetProperty("kind").GetString()));
            Assert.DoesNotContain("source payload must never be stored", manifest.RootElement.GetRawText(), StringComparison.Ordinal);
            Assert.Equal("source payload must never be stored", File.ReadAllText(source));
            Assert.False(File.Exists(recordPath));
            Assert.Null(new FileInfo(temporary.Combine(".agents/a.md")).LinkTarget);
        }
        finally
        {
            if (preparation is not null)
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }
}
