using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;

internal sealed class LibraryResidualWorkspace : IDisposable
{
    internal const string RecordText = """
        {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/review.md"]}]}
        """;
    private readonly List<string> _bundles = [];
    private readonly string? _lockPath;
    private RecoveryBundlePreparation? _preparation;
    private LibraryResidualEvidence? _evidence;
    private string? _targetPath;

    internal LibraryResidualWorkspace(bool dangling = false)
    {
        Files = new LibraryMutationWorkspace();
        Files.ConsumerRoute();
        LibraryMutationApplicationData.FrameworkFile(Files);
        if (!dangling)
        {
            Files.RoutedSource();
        }

        var store = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.None);
        _lockPath = store is null ? null : WorkspaceLockPathIdentity.LockPath(store, Files.Workspace);
        Assert.False(File.Exists(_lockPath));
    }

    internal LibraryMutationWorkspace Files { get; }
    internal RecoveryBundlePreparation Preparation => Assert.IsType<RecoveryBundlePreparation>(_preparation);
    internal LibraryResidualEvidence Evidence => Assert.IsType<LibraryResidualEvidence>(_evidence);
    internal string TargetPath => Assert.IsType<string>(_targetPath);
    internal string? PriorText { get; private set; }

    internal async Task PrepareAsync(string kind, string operation = "sync", bool includeUnselectedHost = false)
    {
        var token = TestContext.Current.CancellationToken;
        RecoveryBundleTarget target;
        var linked = kind is "link-create" or "link-delete";
        _targetPath = Files.Absolute(linked ? LibraryMutationWorkspace.Leaf : LibraryMutationWorkspace.RecordPath);
        if (linked)
        {
            Files.Record(LibraryMutationWorkspace.Leaf);
            if (kind == "link-delete")
            {
                Files.Link();
            }

            target = RecoveryBundleTarget.Create(LibraryMutationApplicationData.Link(delete: kind == "link-delete"),
                LibraryMutationApplicationData.Leaf(Files, linked: kind == "link-delete"));
        }
        else if (kind == "record-create")
        {
            var missing = FileStateSnapshot.Missing(TargetPath);
            target = RecoveryBundleTarget.CreateReversible(PlannedFileChange.Create(missing.Expectation, Encoding.UTF8.GetBytes(RecordText)), missing);
        }
        else
        {
            PriorText = RecordText;
            Files.Write(LibraryMutationWorkspace.RecordPath, PriorText);
            var prior = FileStateSnapshot.File(TargetPath, TargetPath, Encoding.UTF8.GetBytes(PriorText));
            var change = kind == "record-delete" ? PlannedFileChange.Delete(prior.Expectation)
                : PlannedFileChange.Replace(prior.Expectation, Encoding.UTF8.GetBytes(RecordText + "\n"));
            target = RecoveryBundleTarget.Create(change, prior);
        }

        var operationKind = operation switch
        {
            "attach" => RecoveryBundleOperation.Attach,
            "sync" => RecoveryBundleOperation.Sync,
            "detach" => RecoveryBundleOperation.Detach,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var targets = new List<RecoveryBundleTarget> { target };
        const string hostPath = ".agents/directives/_directives.md";
        string? hostIntended = null;
        if (includeUnselectedHost)
        {
            var host = Files.Absolute(hostPath);
            var priorHost = FileStateSnapshot.File(host, host, File.ReadAllBytes(host));
            hostIntended = File.ReadAllText(host) + "\n";
            targets.Add(RecoveryBundleTarget.Create(PlannedFileChange.Replace(priorHost.Expectation, Encoding.UTF8.GetBytes(hostIntended)), priorHost));
        }

        var input = RecoveryBundleInput.Create(Files.Workspace, $"library {operation}",
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, operationKind, Files.Workspace), Guid.NewGuid(), targets);
        var prepared = await RecoveryBundleStore.PrepareAsync(input, token);
        if (prepared.ResidualPath is { } residual)
        {
            _bundles.Add(residual);
        }

        Assert.True(prepared.State == RecoveryBundlePreparationState.Prepared && prepared.Preparation is not null,
            $"Library residual fixture preparation prerequisite: {prepared.State}; {prepared.Cause}");
        _preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        if (!_bundles.Contains(Preparation.BundlePath))
        {
            _bundles.Add(Preparation.BundlePath);
        }

        switch (kind)
        {
            case "link-create": Files.Link(); break;
            case "link-delete":
            case "record-delete": File.Delete(TargetPath); break;
            case "record-create": Files.Write(LibraryMutationWorkspace.RecordPath, RecordText); break;
            default: Files.Replace(LibraryMutationWorkspace.RecordPath, RecordText + "\n"); break;
        }

        if (hostIntended is not null)
        {
            Files.Replace(hostPath, hostIntended);
        }

        var read = await RecoveryBundleReader.ReadFinalAsync(Files.Workspace, Preparation.BundlePath, token);
        Assert.Equal(RecoveryBundleReadState.Valid, read.State);
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
        Assert.Equal(includeUnselectedHost ? 2 : 1, verified.Entries.Length);
        var entry = verified.Entries[0];
        var comparisons = verified.Entries.Select(CompareIntended).ToImmutableArray();
        var comparison = comparisons[0];
        var candidate = RecoveryBundleCandidateSnapshot.VerifiedFinal(verified);
        var record = await OpenForge.Cli.Core.Framework.Libraries.Shared.Observation.LibraryRegistrationReader.ReadAsync(
            new PhysicalPathResolver(), Files.Workspace, token);
        var priorRecord = kind == "record-delete"
            ? new LibraryRecoveryPriorRecord(LibraryMutationApplicationData.Record(registered: true), entry, Assert.IsType<RecoveryContentIdentity>(entry.Prior.OrdinaryFile))
            : null;
        _evidence = new LibraryResidualEvidence(LibraryId.Create("team-knowledge"), record, priorRecord,
            new RecoveryEntrySetObservation(Files.Workspace, candidate, comparisons), comparison);
    }

    private RecoveryEntryComparison CompareIntended(RecoveryEntry entry)
    {
        var context = new RecoveryEntryComparisonContext(Files.Workspace, entry);
        var path = context.LogicalPath;
        var leaf = entry.Intended.Kind switch
        {
            RecoveryEntryStateKind.Missing => NoFollowLeafObservation.Missing(path),
            RecoveryEntryStateKind.RelativeFileLink => NoFollowLeafObservation.CreateRelativeFileLink(path,
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, Assert.IsType<string>(new FileInfo(path).LinkTarget))),
            RecoveryEntryStateKind.OrdinaryFile => NoFollowLeafObservation.OrdinaryFile(path),
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.Intended.Kind, "The recovery state kind is not defined."),
        };
        var content = entry.Intended.Kind == RecoveryEntryStateKind.OrdinaryFile
            ? new RecoveryOrdinaryContentObservation(path, RecoveryContentIdentity.FromBytes(File.ReadAllBytes(path)), null)
            : null;
        return new RecoveryEntryComparison
        {
            Input = new RecoveryEntryComparisonInput(context, leaf, content),
            State = RecoveryBundleTargetComparisonState.Intended,
            Observed = entry.Intended,
            Cause = null,
        };
    }

    internal async Task<RecoveryBundlePreparation> PrepareOtherAsync(bool forward)
    {
        const string path = ".agents/independent-forward.md";
        Files.Write(path, "Independent prior bytes.\n");
        var absolute = Files.Absolute(path);
        var prior = FileStateSnapshot.File(absolute, absolute, File.ReadAllBytes(absolute));
        var input = RecoveryBundleInput.Create(Files.Workspace, forward ? "repair" : "library detach",
            RecoveryBundleAttribution.Create(forward ? RecoveryBundleProducer.Repair : RecoveryBundleProducer.Library,
                forward ? RecoveryBundleOperation.Repair : RecoveryBundleOperation.Detach, Files.Workspace),
            Guid.NewGuid(), [RecoveryBundleTarget.Create(PlannedFileChange.Delete(prior.Expectation), prior)]);
        var prepared = await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken);
        if (prepared.ResidualPath is { } pathToTrack)
        {
            _bundles.Add(pathToTrack);
        }

        Assert.True(prepared.State == RecoveryBundlePreparationState.Prepared && prepared.Preparation is not null,
            $"Independent preparation prerequisite: {prepared.State}; {prepared.Cause}");
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        _bundles.Add(preparation.BundlePath);
        return preparation;
    }

    internal FileStream HoldLock()
    {
        var path = _lockPath ?? throw new InvalidOperationException("The qualified Library fixture requires the current-user lock store.");
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException("The lock requires a parent directory."));
        return new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.ReadWrite,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous,
        });
    }

    internal RepairLibraryRecoveryEffect Effect()
        => new(new RepairSelectedLibraryRecovery(new RepairLibraryRecoveryProposal(Evidence), [RepairSelectionOrigin.Automatic]));

    internal IReadOnlyDictionary<string, string> Sources()
        => Files.Snapshot().Where(pair => pair.Key.StartsWith("shared/", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    public void Dispose()
    {
        foreach (var path in _bundles.Distinct(StringComparer.Ordinal))
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        if (_lockPath is not null && File.Exists(_lockPath))
        {
            File.Delete(_lockPath);
        }

        Files.Dispose();
    }
}
