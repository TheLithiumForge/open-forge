using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryPermissionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false, false, false), InlineData(true, false, false), InlineData(false, true, false), InlineData(true, true, false)]
    [InlineData(true, false, true), InlineData(true, true, true)]
    public static async Task RecoveryUsesCurrentMappedGrantWithoutSourceOrPermissionRestoration(bool granted, bool registered, bool newlyRegisteredSource)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-permission-recovery");
        Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        workspace.RecordAt("docs", registered ? ["gone.md"] : []);
        workspace.Link("docs/gone.md", "../shared/team-knowledge/gone.md");
        var absolute = workspace.Absolute("docs/gone.md");
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/team-knowledge/gone.md");
        var effect = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create("docs/gone.md"), link);
        var target = RecoveryBundleTarget.Create(effect, NoFollowLeafObservation.CreateRelativeFileLink(absolute, link));
        var input = RecoveryBundleInput.Create(workspace.Workspace, command: "library detach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, workspace.Workspace),
            operationId: Guid.NewGuid(), targets: [target]);
        var prepared = await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            File.Delete(absolute);
            var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, preparation.BundlePath, TestContext.Current.CancellationToken);
            var verified = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
            var entry = Assert.Single(verified.Entries);
            var comparison = new RecoveryEntryComparison
            {
                Input = new(new(workspace.Workspace, entry), NoFollowLeafObservation.Missing(absolute), ordinaryContent: null),
                State = RecoveryBundleTargetComparisonState.Intended,
                Observed = entry.Intended,
                Cause = null,
            };
            var record = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
            Assert.Equal(LibraryRegistrationReadState.Complete, record.State);
            var evidence = new LibraryResidualEvidence(LibraryId.Create("team-knowledge"), record, verifiedPriorRecord: null,
                residual: new(workspace.Workspace, RecoveryBundleCandidateSnapshot.VerifiedFinal(verified), [comparison]), entry: comparison);
            if (granted)
            {
                workspace.Write(".agents/open-forge.json", """
                    {"allowInstallPaths":["docs"]}
                    """);
            }
            if (newlyRegisteredSource)
            {
                var paths = registered ? "\"gone.md\"" : string.Empty;
                File.WriteAllText(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath), $$"""
                    {"schemaVersion":1,"libraries":[
                      {"id":"later","sourceRoot":"docs","destinationRoot":"elsewhere","paths":[]},
                      {"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":"docs","paths":[{{paths}}]}]}
                    """);
            }
            var before = workspace.Snapshot();

            var observed = await LibraryRecoveryPermissionReader.ObserveAsync(workspace.Workspace, evidence, TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
                new(workspace.Workspace, "repair", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
            var admitted = await LibraryRecoveryPermissionReader.ReadAsync(lease, evidence, TestContext.Current.CancellationToken);

            Assert.Equal(granted && !newlyRegisteredSource, observed.IsAdmitted);
            Assert.Equal(granted && !newlyRegisteredSource, admitted.IsAdmitted);
            Assert.Equal("docs/gone.md", Assert.Single(admitted.Evaluation.Required));
            Assert.Equal(before, workspace.Snapshot());
            Assert.Null(new FileInfo(absolute).LinkTarget);
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.False(Directory.Exists(workspace.Absolute(LibraryMutationWorkspace.SourceRoot)));
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
