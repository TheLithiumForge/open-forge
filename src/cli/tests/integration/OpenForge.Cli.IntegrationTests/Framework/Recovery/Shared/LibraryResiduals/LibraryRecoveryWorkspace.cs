using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

internal sealed class LibraryRecoveryWorkspace : IDisposable
{
    internal const string RelativeTarget = "../shared/.agents/source.md";
    internal const string TargetRelativePath = ".agents/target.md";
    private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("library-residual");
    private readonly WorkspaceLockTestStore _locks = WorkspaceLockTestStore.Create("library-residual-lock");
    private string? _bundlePath;
    private RecoveryBundlePreparation? _preparation;
    private RecoveryBundleCandidateSnapshot? _candidate;

    private LibraryRecoveryWorkspace()
    {
        Workspace = new CliWorkspace(_temporary.Path, _temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _temporary.CreateDirectory(".agents");
        _temporary.CreateFile("local.txt", "local untouched");
        _temporary.CreateFile("shared/.agents/source.md", "source untouched");
    }

    internal CliWorkspace Workspace { get; }
    internal string TargetPath => _temporary.Combine(TargetRelativePath);
    internal string SourcePath => _temporary.Combine("shared/.agents/source.md");
    internal RecoveryBundlePreparation Preparation => _preparation ?? throw new InvalidOperationException("The real recovery fixture is not prepared.");
    internal RecoveryBundleCandidateSnapshot Candidate => _candidate ?? throw new InvalidOperationException("The real recovery fixture is not verified.");

    internal static async Task<LibraryRecoveryWorkspace> CreateAsync(string kind, bool framework = false)
    {
        var fixture = new LibraryRecoveryWorkspace();
        try
        {
            var target = fixture.CreateTarget(kind);
            await fixture.PrepareAsync([target], framework);
            fixture.SeedIntended(kind);
            return fixture;
        }
        catch
        {
            fixture.Dispose();
            throw;
        }
    }

    internal static async Task<LibraryRecoveryWorkspace> CreateOrderedSetAsync()
    {
        var fixture = new LibraryRecoveryWorkspace();
        try
        {
            var ordinary = fixture.CreateTarget("create");
            var link = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/link.md"),
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RelativeTarget));
            var linkTarget = RecoveryBundleTarget.Create(link, NoFollowLeafObservation.Missing(fixture._temporary.Combine(".agents/link.md")));
            await fixture.PrepareAsync([ordinary, linkTarget], framework: false);
            fixture.SeedIntended("create");
            return fixture;
        }
        catch
        {
            fixture.Dispose();
            throw;
        }
    }

    internal async ValueTask<WorkspaceLockLease> AcquireLaterLeaseAsync()
    {
        var request = new WorkspaceLockRequest(Workspace, "explicit recovery", Guid.NewGuid());
        var result = await _locks.AcquireAsync(request, TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    internal void AssertUnrelatedPreserved()
    {
        Assert.Equal("local untouched", File.ReadAllText(_temporary.Combine("local.txt")));
        Assert.Equal("source untouched", File.ReadAllText(SourcePath));
    }

    internal string CreateOrdinaryFile(string path, byte[] bytes) => _temporary.CreateFile(path, bytes);

    public void Dispose()
    {
        if (_bundlePath is not null && (File.Exists(_bundlePath) || new FileInfo(_bundlePath).LinkTarget is not null))
        {
            File.Delete(_bundlePath);
        }
        _locks.Dispose();
        _temporary.Dispose();
    }

    private async Task PrepareAsync(IReadOnlyList<RecoveryBundleTarget> targets, bool framework)
    {
        var producer = framework ? RecoveryBundleProducer.Framework : RecoveryBundleProducer.Library;
        var operation = framework ? RecoveryBundleOperation.Update : RecoveryBundleOperation.Sync;
        var input = RecoveryBundleInput.Create(Workspace, command: "recovery fixture producer",
            attribution: RecoveryBundleAttribution.Create(producer, operation, Workspace), operationId: Guid.NewGuid(), targets: targets);
        var prepared = await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken);
        _bundlePath = prepared.Preparation?.BundlePath ?? prepared.ResidualPath;
        _preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var read = await RecoveryBundleReader.ReadFinalAsync(Workspace, Preparation.BundlePath, TestContext.Current.CancellationToken);
        _candidate = RecoveryBundleCandidateSnapshot.VerifiedFinal(Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified));
    }

    private RecoveryBundleTarget CreateTarget(string kind)
    {
        if (kind is "link-create" or "link-delete")
        {
            var identity = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RelativeTarget);
            if (kind == "link-delete")
            {
                _temporary.CreateFileSymbolicLink(TargetRelativePath, RelativeTarget);
                return RecoveryBundleTarget.Create(RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(TargetRelativePath), identity),
                    NoFollowLeafObservation.CreateRelativeFileLink(TargetPath, identity));
            }
            return RecoveryBundleTarget.Create(RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(TargetRelativePath), identity),
                NoFollowLeafObservation.Missing(TargetPath));
        }
        if (kind == "create")
        {
            return RecoveryBundleTarget.CreateReversible(PlannedFileChange.Create(FileExpectation.Missing(TargetPath), "intended"u8),
                FileStateSnapshot.Missing(TargetPath));
        }
        _temporary.CreateFile(TargetRelativePath, "prior");
        var before = FileStateSnapshot.File(TargetPath, TargetPath, "prior"u8);
        var change = kind switch
        {
            "replace" => PlannedFileChange.Replace(before.Expectation, "intended"u8),
            "generated" => PlannedFileChange.ReplaceGeneratedRegion(before.Expectation, "intended"u8),
            "delete" => PlannedFileChange.Delete(before.Expectation),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return RecoveryBundleTarget.Create(change, before);
    }

    private void SeedIntended(string kind)
    {
        switch (kind)
        {
            case "create": _temporary.CreateFile(TargetRelativePath, "intended"); break;
            case "replace":
            case "generated": _temporary.ReplaceText(TargetRelativePath, "intended"); break;
            case "delete":
            case "link-delete": File.Delete(TargetPath); break;
            case "link-create": _temporary.CreateFileSymbolicLink(TargetRelativePath, RelativeTarget); break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
