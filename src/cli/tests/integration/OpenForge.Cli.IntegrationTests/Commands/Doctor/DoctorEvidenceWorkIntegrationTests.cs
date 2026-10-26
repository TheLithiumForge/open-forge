using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Commands.Repair;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class DoctorEvidenceWorkIntegrationTests
{
    [Fact(DisplayName = "Doctor batches fresh eligibility checks with bounded workers and no surviving work")]
    public async Task EligibilityChecksAreFreshAndBounded()
    {
        using var temporary = TemporaryWorkspace.Create("doctor-eligibility-work");
        temporary.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        temporary.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Entries\n"));
        for (var index = 0; index < 32; index++)
            temporary.WriteText($".agents/guidance/note-{index}.md", Note("Note", "# Note\n"));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, token);
        var contributors = RepairOperationalContributorFactory.Create(new PhysicalPathResolver());
        var sharing = await NavigationSharingReader.ReadAsync(workspace, token);
        var routes = await contributors.Routes.ReadDoctorAsync(observation, sharing.Sharing, token);
        var references = await contributors.LocalReferences.ReadDoctorAsync(observation, token, routes);
        using var release = new ManualResetEventSlim();
        var reads = 0;
        var active = 0;
        var peak = 0;
        var paths = new PhysicalPathResolver(path =>
        {
            if (!PhysicalIdentityTracker.PathComparer.Equals(path, temporary.Combine(".agents/guidance")))
                return LinkTargetReader.Read(path);
            Interlocked.Increment(ref reads);
            var count = Interlocked.Increment(ref active);
            int previous;
            do { previous = Volatile.Read(ref peak); }
            while (count > previous && Interlocked.CompareExchange(ref peak, count, previous) != previous);
            if (count > 1) release.Set();
            try
            {
                Assert.True(release.Wait(TimeSpan.FromSeconds(10), token));
                return LinkTargetReader.Read(path);
            }
            finally { Interlocked.Decrement(ref active); }
        });
        var result = await new DoctorMaintenanceReader(new RenameEvidenceReader(new PhysicalPathResolver()), physicalPaths: paths)
            .ReadAsync(routes, references, sharing, token);
        Assert.Equal(34, reads);
        Assert.InRange(peak, 2, DoctorMaintenanceReader.MaximumConcurrentSources);
        Assert.Equal(0, active);
        Assert.Equal(routes.Sources.Count, result.Eligibility.Count);
    }

    [Fact(DisplayName = "Doctor computes route parent facts once per entrypoint rather than per source")]
    public async Task RouteParentWorkIsPerEntrypoint()
    {
        using var temporary = TemporaryWorkspace.Create("doctor-route-parent-work");
        temporary.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        temporary.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Entries\n"));
        for (var index = 0; index < 32; index++)
            temporary.WriteText($".agents/guidance/note-{index}.md", Note("Note", "# Note\n"));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, token);
        var contributors = RepairOperationalContributorFactory.Create(new PhysicalPathResolver());
        var sharing = await NavigationSharingReader.ReadAsync(workspace, token);
        var routes = await contributors.Routes.ReadDoctorAsync(observation, sharing.Sharing, token);
        var references = await contributors.LocalReferences.ReadDoctorAsync(observation, token, routes);
        var parents = 0;
        var reader = new DoctorMaintenanceReader(new RenameEvidenceReader(new PhysicalPathResolver()), path =>
        {
            parents++;
            return SourceLogicalPath.ReadParent(path);
        });
        var result = await reader.ReadAsync(routes, references, sharing, token);
        Assert.NotNull(routes.Catalogue);
        Assert.Equal(routes.Routes.Topology.LoaderRootPaths.Count
            + routes.Catalogue.Sources.Count(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)), parents);
        Assert.Equal(routes.Sources.Count, result.Eligibility.Count);
    }

    [Fact(DisplayName = "Doctor starts stale Entries evidence once while independent reference acquisition is pending")]
    public async Task StaleEntryEvidenceOverlapsReferenceAcquisition()
    {
        using var temporary = TemporaryWorkspace.Create("doctor-independent-evidence");
        temporary.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        temporary.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Entries\n\n- [Old note](old.md) - #Guidance\n"));
        temporary.WriteText(".agents/guidance/new.md", Note("New note", "# New note\n"));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var paths = new PhysicalPathResolver();
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var referenceReads = 0;
        var gitReads = 0;
        var gated = new OperationalContributorCatalogue(contributors.WorkspaceEntry, contributors.RecoveryResiduals, contributors.Routes,
            new GatedReferences(contributors.LocalReferences, release.Task, () => Interlocked.Increment(ref referenceReads)),
            contributors.FrameworkLifecycle, contributors.ExtensionLifecycle, contributors.Libraries);
        var renames = new RenameEvidenceReader(paths, (_, _) =>
        {
            Interlocked.Increment(ref gitReads);
            release.TrySetResult();
            return ValueTask.FromResult(RenameGitObservation.Unavailable(RenameGitAvailability.NoRepository));
        });
        using var deadlockGuard = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadlockGuard.CancelAfter(TimeSpan.FromSeconds(10));
        var cancelled = false;
        try
        {
            await new DoctorDiagnosisReader(gated, renames).ReadAsync(new DoctorRequest(workspace), deadlockGuard.Token);
        }
        catch (OperationCanceledException) when (deadlockGuard.IsCancellationRequested) { cancelled = true; }
        finally { release.TrySetResult(); }

        Assert.Equal(1, referenceReads);
        Assert.Equal(1, gitReads);
        Assert.False(cancelled);
    }

    [Theory(DisplayName = "Doctor requests Git evidence only when missing targets or stale Entries require it")]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task CleanNavigationSkipsGit(bool missing, int expectedReads)
    {
        using var temporary = TemporaryWorkspace.Create("doctor-evidence-demand");
        temporary.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        temporary.CreateFile(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Axioms\n\n- inherited\n\n## Entries\n\n- [Note](note.md) - #Guidance\n"));
        temporary.CreateFile(".agents/guidance/note.md", Note("Note", missing ? "# Note\n\n[Missing](missing.md)\n" : "# Note\n"));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var reads = 0;
        var paths = new PhysicalPathResolver();
        var renames = new RenameEvidenceReader(paths, (_, _) =>
        {
            reads++;
            return ValueTask.FromResult(RenameGitObservation.Unavailable(RenameGitAvailability.NoRepository));
        });
        var before = temporary.SnapshotHashes();

        await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), renames)
            .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);

        Assert.Equal(expectedReads, reads);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Repair reuses diagnosis rename evidence once while later diagnoses acquire fresh evidence")]
    public async Task RepairPlanningRetainsEvidenceWithinOneDiagnosis()
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string oldPath = ".agents/guidance/rounding.md";
        const string newPath = ".agents/guidance/money-rounding.md";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [oldPath] = Note("Rounding rules", "# Rounding\n\nAuthored rules.\n"),
            [".agents/guidance/source.md"] = Note("Incoming reference", "# Source\n\n[Rules](rounding.md)\n"),
            [".agents/guidance/_guidance.md"] = Note("Guidance", "# Guidance\n\n## Axioms\n\n- inherited\n\n## Entries\n\n- [Rounding rules](rounding.md) - #Guidance\n- [Incoming reference](source.md) - #Guidance\n"),
        });
        fixture.Move(oldPath, newPath);
        var reads = 0;
        var reader = new RenameEvidenceReader(new PhysicalPathResolver(), (_, _) =>
        {
            reads++;
            return ValueTask.FromResult(RenameGitObservation.Unavailable(RenameGitAvailability.NoRepository));
        });
        var adapter = new RoutedMaintenanceRenameEvidenceAdapter(reader);
        var components = RepairOperationFactory.CreateDefaultComponents(new(adapter.ReadEvidenceAsync, adapter)
        {
            ReadReferences = adapter.ReadEvidenceAsync,
            DiagnosisReader = reader,
        });
        var before = Snapshot(fixture);
        var result = await new RepairOperation(components).ExecuteAsync(
            new RepairRequest(fixture.Workspace, RepairMode.DryRun, automatic: true, relinks: [], allowInteraction: false),
            TestContext.Current.CancellationToken);

        Assert.Single(result.MaintenanceResults.Renames);
        Assert.Equal(1, reads);
        Assert.Equal(before, Snapshot(fixture));

        fixture.Write(newPath, Note("Changed rules", "# Changed\n"));
        var fresh = await components.DiagnosisReader.ReadAsync(new DoctorRequest(fixture.Workspace), TestContext.Current.CancellationToken);
        Assert.Equal(2, reads);
        Assert.Null(fresh.Observation.Maintenance?.Renames?.Find(oldPath)?.Proven);
    }

    private static string Note(string description, string body)
        => $"---\nopen-forge:\n  description: {description}\n  tags: [Guidance]\n---\n\n{body}";

    private static string[] Snapshot(RepairRenameGitWorkspace fixture)
        => Directory.EnumerateFiles(fixture.PathFor(".agents"), "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal).Select(path => $"{path}:{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)))}").ToArray();

    private sealed class GatedReferences(ILocalReferenceOperationalContributor inner, Task release, Action started) : ILocalReferenceOperationalContributor
    {
        public async ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(WorkspaceLayoutRead observation,
            CancellationToken token, RouteDoctorView? routes = null)
        {
            var workspace = observation.Layout?.Workspace ?? throw new InvalidOperationException();

            started();
            await release.WaitAsync(token);
            return await inner.ReadDoctorAsync(observation, token, routes);
        }
    }
}
