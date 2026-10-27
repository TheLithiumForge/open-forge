using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;
using System.Text.Json;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Boundary", "OS"), Trait("Feature", "doctor-outside"), Trait("Evidence", "Integration")]
public sealed class DoctorOutsideIntegrationTests
{
    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData("knowledge/main", SourceFormClassifier.IndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName)]
    [InlineData("knowledge/main", SourceFormClassifier.UnderscoreIndexFileName)]
    public async Task MissingCompatibilityDestinationDoesNotHideTheUnlistedDiagnosis(string root, string name)
    {
        using var files = Create(root, "listed");
        var target = Directory + "/" + name;
        files.ReplaceText(root + "/loader.md", GeneratedLoaderDocumentBuilder.Build(
            $"- [Authored row]({Path.GetRelativePath(root, target).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        var before = files.SnapshotHashes();
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(new PhysicalPathResolver()))
            .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
        Assert.False(Assert.Single(read.Observation.OutsideRoutes!.Routes).Listed);
        var finding = Assert.Single(read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
            finding => finding.OutsideRoute?.State == DoctorOutsideRouteState.Unlisted);
        Assert.Equal(Directory, finding.Subject.Path);
        Assert.False(finding.IsIncludedFile);
        var run = await CliHostCapture.RunAsync([PublishedTokens.Doctor.Command, PublishedTokens.Syntax.FormatOption,
            PublishedTokens.Syntax.JsonFormat, PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail], files.Path);
        using var json = JsonDocument.Parse(run.Output);
        Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Findings).EnumerateArray(),
            item => item.GetProperty(PublishedTokens.Json.Subject).GetProperty(PublishedTokens.Json.Path).GetString() == Directory
                && item.GetProperty(PublishedTokens.Json.Code).GetString() == DoctorDefinitions.ReadFindingKind(finding.Kind));
        Assert.Equal(before, files.SnapshotHashes());
    }

    public static IEnumerable<object[]> Cases()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
            foreach (var situation in new[] { "listed", "unlisted", "missing-directory", "missing-anchor", "compatibility", "stale-compatibility", "ambiguous", "unknown-listing" })
                yield return [root, situation];
    }

    [Theory, MemberData(nameof(Cases))]
    public async Task RetainedFactsDistinguishRegistrationListingAndStructuralAnchors(string root, string situation)
    {
        using var files = Create(root, situation);
        var before = files.SnapshotHashes();
        var paths = new PhysicalPathResolver();
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layoutReads = 0;
        var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), readLayout: (_, token) =>
        {
            layoutReads++;
            return new WorkspaceLayoutReader(paths).ReadAsync(workspace, token);
        }).ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(1, layoutReads);
        Assert.Same(read.Observation.LayoutRead, read.Observation.OutsideRoutes!.LayoutRead);
        var directory = Assert.Single(read.Observation.OutsideRoutes.Routes);
        var findings = read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings).Where(finding => finding.OutsideRoute is not null).ToArray();
        var states = findings.Select(finding => finding.OutsideRoute!.State).ToArray();
        if (situation == "missing-directory")
        {
            var missing = Assert.Single(findings, finding => finding.OutsideRoute!.State == DoctorOutsideRouteState.MissingDirectory);
            Assert.Equal(DoctorFindingKind.WorkspaceRootMissing, missing.Kind);
            Assert.Equal(DoctorFindingSeverity.Warning, missing.Severity);
            Assert.Equal(DoctorResolutionLane.ManualDecision, missing.Resolution);
            Assert.All(DoctorWording.Actions(missing), action => Assert.Equal(CliNextActionKind.Sentence, action.Kind));
        }
        else if (situation == "missing-anchor")
        {
            var missing = Assert.Single(findings, finding => finding.OutsideRoute!.State == DoctorOutsideRouteState.MissingAnchor);
            Assert.Equal(Anchor, missing.Subject.Path);
            Assert.Equal(Anchor, missing.OutsideRoute!.InitTarget);
            var action = Assert.Single(DoctorWording.Actions(missing));
            Assert.Equal(CliNextActionKind.Command, action.Kind);
            var operand = Assert.Single(action.Command.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(3));
            var parsed = SourceReferenceParser.Parse(operand);
            Assert.Equal(SourceReferenceParseState.Valid, parsed.State);
            Assert.Equal(Anchor, parsed.AttemptedPath);
            var init = await CliHostCapture.RunAsync(["route", "init", operand, "--dry-run", "--format", "json"], files.Path);
            Assert.Equal(0, init.ExitCode);
        }
        else if (situation == "ambiguous")
        {
            Assert.Contains(DoctorOutsideRouteState.AmbiguousAnchor, states);
            Assert.DoesNotContain(DoctorOutsideRouteState.MissingAnchor, states);
            Assert.All(findings, finding => Assert.Null(finding.OutsideRoute!.InitTarget));
        }
        else if (situation is "compatibility" or "stale-compatibility")
        {
            Assert.DoesNotContain(DoctorOutsideRouteState.MissingAnchor, states);
            Assert.Contains(Directory + "/index.md", directory.EntrypointPaths);
            Assert.All(findings, finding => Assert.Null(finding.OutsideRoute!.InitTarget));
            Assert.True(directory.Listed);
        }
        else if (situation == "unknown-listing")
        {
            Assert.Null(directory.Listed);
            Assert.DoesNotContain(DoctorOutsideRouteState.Unlisted, states);
        }
        else Assert.Equal(situation == "unlisted", states.Contains(DoctorOutsideRouteState.Unlisted));
        Assert.All(findings, finding => Assert.False(finding.IsIncludedFile));
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData("knowledge/main", false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData("knowledge/main", true)]
    public async Task UnsafeAndUnavailableBoundariesRetainActualTypedCause(string root, bool unavailable)
    {
        using var files = Create(root, unavailable ? "listed" : "unsafe");
        PhysicalPathResolver paths;
        if (unavailable)
        {
            paths = new PhysicalPathResolver(path => PhysicalIdentityTracker.PathComparer.Equals(path, files.Combine(Directory))
                ? new PathComponent(PathComponentState.Inaccessible, path, null, null,
                    FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException("Controlled directory observation")))
                : LinkTargetReader.Read(path));
        }
        else
        {
            paths = new PhysicalPathResolver();
        }
        var before = files.SnapshotHashes();
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths),
                readLayout: (_, token) => new WorkspaceLayoutReader(paths).ReadAsync(workspace, token))
            .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
        var finding = Assert.Single(read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
            finding => finding.OutsideRoute?.State == (unavailable ? DoctorOutsideRouteState.Unavailable : DoctorOutsideRouteState.UnsafeBoundary));
        Assert.Equal(Directory, finding.Subject.Path);
        Assert.Null(finding.OutsideRoute!.InitTarget);
        Assert.NotNull(finding.OutsideRoute.Cause);
        Assert.All(DoctorWording.Actions(finding), action => Assert.Equal(CliNextActionKind.Sentence, action.Kind));
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Fact]
    public async Task RegisteredUnlistedSourcesRetainTheExistingC1Eligibility()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
            foreach (var compatibility in new[] { false, true })
            {
                using var files = Create(root, compatibility ? "unlisted-compatibility" : "unlisted");
                var anchor = compatibility ? Directory + "/index.md" : Anchor;
                var before = files.SnapshotHashes();
                var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
                var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(new PhysicalPathResolver()))
                    .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
                Assert.Equal(RoutedMetadataEligibility.Eligible, read.Observation.Maintenance!.Eligibility[anchor]);
                Assert.Equal(RoutedMetadataEligibility.Eligible, read.Observation.Maintenance.Eligibility[Directory + "/ordinary.md"]);
                Assert.DoesNotContain(read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
                    finding => finding.Subject.Path?.StartsWith(Directory + "/", StringComparison.Ordinal) == true
                        && finding.Maintenance?.Condition == DoctorMaintenanceCondition.OutsideFolders);
                foreach (var finding in read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings)
                    .Where(finding => finding.Subject.Path?.StartsWith(Directory + "/", StringComparison.Ordinal) == true))
                    foreach (var action in finding.Actions.Where(action => action.Operation == DoctorNextOperation.Index))
                    {
                        Assert.NotNull(action.Command);
                        var parsed = SourceReferenceParser.Parse(Assert.Single(action.Command.Split(' ').Skip(2)));
                        Assert.Equal(SourceReferenceParseState.Valid, parsed.State);
                        Assert.Equal(finding.Subject.Path, parsed.AttemptedPath);
                    }
                Assert.Equal(before, files.SnapshotHashes());
            }
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task AvailableC1BoundaryRetainsOriginalLayoutBesideAnUnavailableRegistration(string root)
    {
        using var files = Create(root, "unlisted");
        files.CreateDirectory("docs/unavailable");
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Directory}}","docs/unavailable"]}
            """);
        var paths = new PhysicalPathResolver(path => PhysicalIdentityTracker.PathComparer.Equals(path, files.Combine("docs/unavailable"))
            ? new PathComponent(PathComponentState.Inaccessible, path, null, null,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException("Controlled unrelated registration")))
            : LinkTargetReader.Read(path));
        var before = files.SnapshotHashes();
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths),
                readLayout: (_, token) => new WorkspaceLayoutReader(paths).ReadAsync(workspace, token))
            .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(RoutedMetadataEligibility.Eligible, read.Observation.Maintenance!.Eligibility[Anchor]);
        Assert.Same(read.Observation.LayoutRead, read.Observation.Maintenance.ReferenceProtectionPolicy!.LayoutRead);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Fact]
    public async Task CurrentOutsideRegionDoesNotCallItsRetainedAuthoredRowMissing()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
        {
            using var files = Create(root, "unlisted");
            var before = files.SnapshotHashes();
            var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
            var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(new PhysicalPathResolver()))
                .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
            Assert.True(File.Exists(files.Combine(Directory + "/ordinary.md")));
            Assert.Equal(OperationalGeneratedNavigationState.Current,
                Assert.Single(read.Observation.Routes.GeneratedNavigation, target => target.Path == Anchor).State);
            Assert.DoesNotContain(read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
                finding => finding.Kind == DoctorFindingKind.RouteGeneratedEntryExtra && finding.Subject.Path == Anchor);
            Assert.Equal(before, files.SnapshotHashes());
        }
    }

    [Fact]
    public async Task MissingAnchorAdviceRequiresCurrentExclusionAndOwnershipEvidence()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
            foreach (var unavailableOwnership in new[] { false, true })
            {
                using var files = Create(root, "missing-anchor");
                if (unavailableOwnership) files.WriteText(WorkspaceOwnershipDefinitions.RelativePath, "{invalid independent ownership");
                else files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, $$"""
                {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Directory}}"],"{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}":["{{Anchor}}"]}
                """);
                var before = files.SnapshotHashes();
                var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
                var paths = new PhysicalPathResolver();
                var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths))
                    .ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
                var finding = Assert.Single(read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
                    finding => finding.OutsideRoute?.State == DoctorOutsideRouteState.MissingAnchor);
                Assert.Null(finding.OutsideRoute!.InitTarget);
                Assert.Equal(DoctorResolutionLane.ManualDecision, finding.Resolution);
                Assert.All(DoctorWording.Actions(finding), action => Assert.Equal(CliNextActionKind.Sentence, action.Kind));
                Assert.Equal(before, files.SnapshotHashes());
            }
    }

    [Fact]
    public async Task OutsideDiagnosisViewsAreReviewed()
    {
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
        {
            using var files = Create(root, "missing-directory");
            var before = files.SnapshotHashes();
            var run = await CliHostCapture.RunAsync(["doctor", "--format", "json"], files.Path);
            await new ReadCommandOutputCapture(files.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = root == WorkspaceLayout.DefaultMainRoot ? "outside-default" : "outside-custom",
                Arguments = ["doctor"],
                ExitCode = run.ExitCode,
            }, snapshotCollector: captures);
            Assert.Equal(before, files.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    private const string Directory = "docs/team";
    private const string Anchor = Directory + "/_team.md";

    private static TemporaryWorkspace Create(string root, string situation)
    {
        var files = TemporaryWorkspace.Create("doctor-outside");
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Directory}}"]}
            """);
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Independent workspace\n");
        var target = situation is "compatibility" or "unlisted-compatibility" ? Directory + "/index.md" : Anchor;
        files.WriteText(root + "/loader.md", GeneratedLoaderDocumentBuilder.Build(situation is "unlisted" or "unlisted-compatibility" ? string.Empty
            : $"- [Independent team]({Path.GetRelativePath(root, target).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        if (situation != "missing-directory")
        {
            var contentDirectory = situation == "unsafe" ? "docs/real" : Directory;
            files.WriteText(contentDirectory + "/ordinary.md", OpenForgeDocumentSeed.Metadata("Independent leaf", ["Body"], "# Leaf\n\nUntouched bytes.\n"));
            if (situation != "missing-anchor") files.WriteText(situation is "compatibility" or "stale-compatibility" or "unlisted-compatibility" ? contentDirectory + "/index.md" : contentDirectory + "/_team.md",
                OpenForgeDocumentSeed.Metadata("Independent anchor", ["Body"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                { Prefix = "# Team", Entries = "- [Leaf](ordinary.md) - #Body\n", LineEnding = "\n" })));
            if (situation == "ambiguous") files.WriteText(Directory + "/index.md", "# Independent competing anchor\n");
            if (situation == "unsafe") files.CreateDirectorySymbolicLink(Directory, files.Combine(contentDirectory));
        }
        if (situation == "unknown-listing") files.ReplaceBytes(root + "/loader.md", [0xff, 0xfe, 0xff]);
        return files;
    }
}
