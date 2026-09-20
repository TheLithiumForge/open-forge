using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Attribution;

internal static class LibraryResidualArchive
{
    internal static async Task<RecoveryEntrySetObservation> CreateAsync(
        LibraryObservationWorkspace fixture,
        string? expectedPriorRecord = null,
        string? storedPayload = null,
        bool includePayload = true,
        string recordTarget = ".agents/open-forge.lock.json",
        string linkTarget = "../shared/team/.agents/a.md",
        bool retainEmptyLock = false)
    {
        var entries = ImmutableArray.CreateBuilder<RecoveryEntry>();
        if (expectedPriorRecord is not null)
        {
            var identity = RecoveryContentIdentity.FromBytes(Encoding.UTF8.GetBytes(expectedPriorRecord));
            const string emptyLock = "{\"schemaVersion\":1,\"framework\":null,\"extensions\":[],\"libraries\":[]}";
            if (retainEmptyLock)
            {
                fixture.Write(recordTarget, emptyLock);
            }
            entries.Add(RecoveryEntry.Create(0, CanonicalRelativePath.Create(recordTarget),
                retainEmptyLock ? RecoveryEntryKind.OrdinaryReplace : RecoveryEntryKind.OrdinaryDelete,
                RecoveryEntryState.Ordinary(identity),
                retainEmptyLock ? RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(Encoding.UTF8.GetBytes(emptyLock))) : RecoveryEntryState.Missing,
                "payloads/00000000.bin"));
        }
        entries.Add(RecoveryEntry.Create(entries.Count, CanonicalRelativePath.Create(".agents/a.md"), RecoveryEntryKind.RelativeFileLinkDelete,
            RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, linkTarget)), RecoveryEntryState.Missing));
        var operationId = Guid.NewGuid();
        var path = fixture.CreateRecoveryArchive(operationId);
        var manifestFacts = new RecoveryBundleVerifiedRead
        {
            BundlePath = path,
            WorkspacePhysicalPath = WorkspaceIdentity.NormalizePhysicalPath(fixture.Workspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(fixture.Workspace.PhysicalRoot),
            Command = "library detach",
            Attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, fixture.Workspace),
            OperationId = operationId,
            Entries = entries.ToImmutable(),
        };
        using (var file = File.Open(path, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
        {
            using (var stream = archive.CreateEntry("manifest.json").Open())
            {
                LibraryResidualManifest.Write(stream, manifestFacts);
            }
            if (expectedPriorRecord is not null)
            {
                using var stream = archive.CreateEntry("payloads/00000000.bin").Open();
                stream.Write(Encoding.UTF8.GetBytes(expectedPriorRecord));
            }
        }
        var read = await RecoveryBundleReader.ReadFinalAsync(fixture.Workspace, path, TestContext.Current.CancellationToken);
        Assert.True(read.State == RecoveryBundleReadState.Valid, $"Independent archive qualification failed: {read.State}: {read.Cause}");
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
        if (expectedPriorRecord is not null && (!includePayload || storedPayload is not null))
        {
            // Retain the real healthy readback, then change the archive to exercise stale-evidence refusal.
            using var archive = ZipFile.Open(path, ZipArchiveMode.Update);
            var payload = archive.GetEntry("payloads/00000000.bin")
                ?? throw new InvalidOperationException("The qualified archive must contain its prior payload.");
            payload.Delete();
            if (includePayload)
            {
                using var stream = archive.CreateEntry("payloads/00000000.bin").Open();
                stream.Write(Encoding.UTF8.GetBytes(storedPayload ?? expectedPriorRecord));
            }
        }
        var observations = verified.Entries.Select(entry =>
        {
            var context = new RecoveryEntryComparisonContext(fixture.Workspace, entry);
            var leaf = entry.Intended.OrdinaryFile is null
                ? NoFollowLeafObservation.Missing(context.LogicalPath)
                : NoFollowLeafObservation.OrdinaryFile(context.LogicalPath);
            var content = entry.Intended.OrdinaryFile is { } identity
                ? new RecoveryOrdinaryContentObservation(context.LogicalPath, identity, null)
                : null;
            return new RecoveryEntryComparison
            {
                Input = new RecoveryEntryComparisonInput(context, leaf, content),
                State = RecoveryBundleTargetComparisonState.Intended,
                Observed = entry.Intended,
                Cause = null,
            };
        }).ToImmutableArray();
        return new RecoveryEntrySetObservation(fixture.Workspace, RecoveryBundleCandidateSnapshot.VerifiedFinal(verified), observations);
    }

    internal static RecoveryResidualDoctorView View(RecoveryEntrySetObservation residual)
        => new(OperationalViewState.Complete, [RecoveryDoctorCandidateObservation.Create(residual.Candidate, null, residual)], null);
}
