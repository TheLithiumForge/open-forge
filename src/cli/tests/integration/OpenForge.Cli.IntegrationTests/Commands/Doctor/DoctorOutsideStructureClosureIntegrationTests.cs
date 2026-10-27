using System.Text.Json;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Boundary", "OS"), Trait("Feature", "doctor-outside-structure"), Trait("Evidence", "Integration")]
public sealed class DoctorOutsideStructureClosureIntegrationTests
{
    private const string Team = "docs/team";
    private const string Anchor = Team + "/_team.md";
    private const string Deep = Team + "/deep/_deep.md";
    private const string Leaf = Team + "/deep/note.md";
    private const string Tag = "Review";

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false)]
    [InlineData("knowledge/main", false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false)]
    [InlineData("knowledge/main", true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true)]
    [InlineData("knowledge/main", false, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, true)]
    [InlineData("knowledge/main", true, true)]
    public async Task MissingAuthoredOutsideTargetsRetainTheExactManualOccurrence(string root, bool listed, bool catalogue)
    {
        using var files = Seed(root);
        File.Delete(files.Combine(Leaf));
        var destination = catalogue ? "absent/_absent.md" : "note.md";
        var target = Team + "/deep/" + destination;
        const string label = "Missing π policy";
        var link = $"[{label}]({destination})";
        var authored = Catalogue("Deep", $"- {link} - #Review #Policy - applies to `src/**`");
        files.ReplaceText(Deep, authored);
        if (listed) SetLoader(files, root, Anchor);
        files.CreateDirectory("independent-empty");
        var before = SnapshotAll(files);
        var read = await Diagnose(files);
        var occurrence = Assert.Single(read.Observation.LocalReferences.References,
            item => item.SourcePath == Deep && item.Destination == destination);
        Assert.Equal(Deep, occurrence.RoutePath);
        Assert.Equal(SourceLinkTargetResolution.Missing, occurrence.Facts.Target.Resolution);
        Assert.Equal(target, occurrence.Facts.Target.Path);
        var start = authored.IndexOf(link, StringComparison.Ordinal);
        var map = new Utf8SourceMap(authored);
        var location = map.Map(start, link.Length);
        var destinationLocation = map.Map(start + label.Length + 3, destination.Length);
        Assert.Equal(location, occurrence.Location);
        Assert.Equal(destinationLocation, occurrence.DestinationLocation);
        Assert.Equal(listed, Assert.Single(read.Observation.OutsideRoutes!.Routes).Listed);
        var finding = Assert.Single(Findings(read), item => item.Kind == DoctorFindingKind.ReferenceTargetMissing
            && item.Subject.Path == Deep && item.Subject.Identifier == destination);
        Assert.Equal(DoctorFindingSeverity.Warning, finding.Severity);
        Assert.Equal(DoctorResolutionLane.ManualDecision, finding.Resolution);
        Assert.Equal(location, finding.Subject.Location);
        Assert.Equal(DoctorDomainKind.LocalReferences, finding.Provenance.Domain);
        Assert.Equal(DoctorProvenanceSource.LocalReferences, finding.Provenance.Source);
        Assert.Equal(Deep, finding.Provenance.Path);
        Assert.Equal(location, finding.Provenance.Location);
        var value = Assert.Single(finding.Evidence.OfType<DoctorAuthoredValueEvidence>());
        Assert.Equal(destination, value.Value);
        Assert.Equal(destinationLocation, value.Location);
        Assert.Null(finding.Proposal);
        Assert.Null(finding.Candidates);
        Assert.DoesNotContain(finding.Actions, action => action.Operation is DoctorNextOperation.Index or DoctorNextOperation.Repair);
        Assert.DoesNotContain(read.Result.Diagnosis.Actions, action => action.Operation is DoctorNextOperation.Index or DoctorNextOperation.Repair);
        Assert.Equal(DoctorCoverageState.Complete, read.Result.Diagnosis.Coverage);
        var counts = Assert.IsType<DoctorCoverageCounts>(read.Result.Diagnosis.CoverageCounts);
        Assert.Equal(listed ? 3L : 2L, counts.LinksChecked);
        Assert.Equal(listed ? 2L : 1L, counts.LinksValid);
        var preview = await CliHostCapture.RunAsync([IndexDefinitions.IndexCommand.Name,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + Deep, IndexDefinitions.DryRun.Name,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], files.Path);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, preview.ExitCode);
        using var json = JsonDocument.Parse(preview.Output);
        Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.False(File.Exists(files.Combine(target)));
        Assert.Equal(before, SnapshotAll(files));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task PrimaryGeneratedMissingTargetRetainsItsNavigationDiagnosis(string root)
    {
        using var files = Seed(root);
        var primary = root + "/guide/_guide.md";
        files.WriteText(primary, Catalogue("Guide", "- [Missing primary](absent.md) - #Review"));
        SetLoader(files, root, primary);
        var before = SnapshotAll(files);
        var read = await Diagnose(files);
        Assert.Contains(Findings(read), finding => finding.Kind == DoctorFindingKind.RouteGeneratedEntryExtra && finding.Subject.Path == primary);
        Assert.DoesNotContain(Findings(read), finding => finding.Kind == DoctorFindingKind.ReferenceTargetMissing && finding.Subject.Path == primary);
        var preview = await CliHostCapture.RunAsync([IndexDefinitions.IndexCommand.Name,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + primary, IndexDefinitions.DryRun.Name,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], files.Path);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, preview.ExitCode);
        using var json = JsonDocument.Parse(preview.Output);
        Assert.NotEmpty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.Equal(before, SnapshotAll(files));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData("knowledge/main", false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", true)]
    public async Task ClosedUnlistedStructureIsInformationalWithoutInventingLoading(string root, bool compatibility)
    {
        using var files = Seed(root, compatibility: compatibility);
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        var outside = Assert.Single(read.Observation.OutsideRoutes!.Routes);
        Assert.False(outside.Listed);
        Assert.Equal(new[] { compatibility ? Team + "/index.md" : Anchor }, outside.EntrypointPaths);
        Assert.Equal(DoctorCoverageState.Complete, read.Result.Diagnosis.Coverage);
        Assert.All(Findings(read), finding => Assert.Equal(DoctorFindingSeverity.Information, finding.Severity));
        var unlisted = Assert.Single(Findings(read), finding => finding.OutsideRoute?.State == DoctorOutsideRouteState.Unlisted);
        Assert.Equal(DoctorFindingKind.WorkspaceDetached, unlisted.Kind);
        Assert.Equal(DoctorResolutionLane.Informational, unlisted.Resolution);
        Assert.Empty(unlisted.Actions);
        Assert.DoesNotContain(read.Result.Diagnosis.Actions, action => action.Operation == DoctorNextOperation.Index);
        Assert.All(read.Observation.Routes.Routes.RouteFacts.Where(fact => fact.Identity.CanonicalBasePath.StartsWith(Team + "/", StringComparison.Ordinal)),
            fact => Assert.Equal(SourceRouteState.Unrouted, fact.State));
        Assert.DoesNotContain(read.Observation.OutsideRoutes.Loading!.Startup, source => source.Path.StartsWith(Team + "/", StringComparison.Ordinal));
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Anchor)]
    [InlineData("knowledge/main", Anchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Deep)]
    [InlineData("knowledge/main", Deep)]
    public async Task AnchorAndExactFormerSuggestedIndexAreNoOpsForTheClosedTree(string root, string path)
    {
        using var files = Seed(root);
        var before = files.SnapshotHashes();
        var operand = SourceWorkspaceRelativePath.LiteralReferencePrefix + path;
        var parsed = SourceReferenceParser.Parse(operand);
        Assert.Equal(path, parsed.AttemptedPath);
        var preview = await CliHostCapture.RunAsync([IndexDefinitions.IndexCommand.Name, operand, IndexDefinitions.DryRun.Name,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], files.Path);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, preview.ExitCode);
        using var json = JsonDocument.Parse(preview.Output);
        Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Findings).EnumerateArray());
        var read = await Diagnose(files);
        Assert.DoesNotContain(Findings(read), finding => finding.Kind is DoctorFindingKind.RouteDetached or DoctorFindingKind.RouteUnreachable);
        Assert.False(Assert.Single(read.Observation.OutsideRoutes!.Routes).Listed);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData("knowledge/main", false)]
    public async Task MissingAuthoredParentRowsRetainUnreachableFindings(string root, bool rootRow)
    {
        using var files = Seed(root);
        files.ReplaceText(rootRow ? Anchor : Deep, Catalogue(rootRow ? "Team" : "Deep", PublishedTokens.Metadata.EmptyEntry));
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        var path = rootRow ? Deep : Leaf;
        Assert.Contains(Findings(read), finding => finding.Kind == DoctorFindingKind.RouteUnreachable && finding.Subject.Path == path);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData("knowledge/main", false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", true)]
    public async Task MissingAndCompetingAnchorsDoNotAdmitDescendants(string root, bool competing)
    {
        using var files = Seed(root);
        if (competing) files.WriteText(Team + "/index.md", Catalogue("Competing", PublishedTokens.Metadata.EmptyEntry));
        else File.Delete(files.Combine(Anchor));
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        Assert.Contains(Findings(read), finding => finding.OutsideRoute?.State == (competing
            ? DoctorOutsideRouteState.AmbiguousAnchor : DoctorOutsideRouteState.MissingAnchor));
        Assert.Contains(Findings(read), finding => finding.Kind == (competing
            ? DoctorFindingKind.RouteCompatibilityConflict : DoctorFindingKind.RouteUnreachable) && finding.Subject.Path == Leaf);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task UnreadableParentDoesNotProveItsChildRow(string root)
    {
        using var files = Seed(root);
        files.ReplaceBytes(Deep, [0xff, 0xfe, 0xff]);
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        Assert.Equal(DoctorCoverageState.Incomplete, read.Result.Diagnosis.Coverage);
        Assert.Contains(Findings(read), finding => finding.Kind == DoctorFindingKind.RouteUnreachable && finding.Subject.Path == Leaf);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData("knowledge/main", false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", true)]
    public async Task UnsafeOrUnavailableRegistrationRetainsItsTypedDefect(string root, bool unavailable)
    {
        using var files = Seed(root);
        PhysicalPathResolver paths;
        if (unavailable)
            paths = new PhysicalPathResolver(path => PhysicalIdentityTracker.PathComparer.Equals(path, files.Combine(Team))
                ? new PathComponent(PathComponentState.Inaccessible, path, null, null,
                    FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException("Controlled selected directory")))
                : LinkTargetReader.Read(path));
        else
        {
            files.CreateDirectory("docs/linked");
            files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(root, "docs/linked/team"));
            files.CreateDirectorySymbolicLink("docs/linked/team", files.Combine(Team));
            paths = new PhysicalPathResolver();
        }
        var before = files.SnapshotHashes();
        var read = await Diagnose(files, paths);
        Assert.Contains(Findings(read), finding => finding.OutsideRoute?.State == (unavailable
            ? DoctorOutsideRouteState.Unavailable : DoctorOutsideRouteState.UnsafeBoundary));
        if (unavailable) Assert.Equal(DoctorCoverageState.Incomplete, read.Result.Diagnosis.Coverage);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task PrimaryDetachedAndUnrootedNativeDiagnosisRemainIndependent(string root)
    {
        using var files = Seed(root);
        var primary = root + "/guidance/_guidance.md";
        files.WriteText(primary, Catalogue("Guidance", "- [Primary](primary.md) - #Review"));
        files.WriteText(root + "/guidance/primary.md", Note("Primary"));
        const string skill = WorkspaceLayout.NativeSkillsPath + "/independent/SKILL.md";
        files.WriteText(skill, OpenForgeDocumentSeed.Skill("independent", "Independent native skill"));
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        Assert.Contains(Findings(read), finding => finding.Kind == DoctorFindingKind.RouteDetached && finding.Subject.Path == primary);
        Assert.Contains(Findings(read), finding => finding.Subject.Path == skill && finding.Kind == DoctorFindingKind.RouteUnreachable);
        Assert.DoesNotContain(Findings(read), finding => finding.Subject.Path?.StartsWith(Team + "/", StringComparison.Ordinal) == true
            && finding.Kind is DoctorFindingKind.RouteDetached or DoctorFindingKind.RouteUnreachable);
        Assert.Equal(before, files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task IndependentFileInclusionDoesNotBecomeAnOutsideStructuralRoot(string root)
    {
        using var files = Seed(root);
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.RootsProperty}}":["{{Anchor}}"]}
            """);
        var before = files.SnapshotHashes();
        var read = await Diagnose(files);
        Assert.Null(read.Observation.OutsideRoutes);
        Assert.Contains(read.Observation.Routes.IncludedFiles!.Files, file => file.Path == Anchor && file.Registered);
        Assert.DoesNotContain(Findings(read), finding => finding.OutsideRoute is not null);
        Assert.Equal(before, files.SnapshotHashes());
    }

    private static IEnumerable<DoctorFinding> Findings(DoctorDiagnosisRead read)
        => read.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings);

    private static IReadOnlyDictionary<string, string> SnapshotAll(TemporaryWorkspace files)
        => System.IO.Directory.EnumerateFileSystemEntries(files.Path, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal).ToDictionary(path => Path.GetRelativePath(files.Path, path),
                path => System.IO.Directory.Exists(path) ? "directory" : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), StringComparer.Ordinal);

    private static void SetLoader(TemporaryWorkspace files, string root, string target)
        => files.ReplaceText(root + "/" + PublishedTokens.Layout.LoaderFileName, OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Independent Loader\n\n## Axioms\n\n- Keep workspace context precise.",
            Entries = $"- [Selected]({Path.GetRelativePath(root, target).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}",
        }));

    private static async Task<DoctorDiagnosisRead> Diagnose(TemporaryWorkspace files, PhysicalPathResolver? paths = null)
    {
        paths ??= new PhysicalPathResolver();
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var reads = 0;
        var read = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), readLayout: (_, token) =>
        {
            reads++;
            return new WorkspaceLayoutReader(paths).ReadAsync(workspace, token);
        }).ReadAsync(new DoctorRequest(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(1, reads);
        Assert.Same(read.Observation.LayoutRead, read.Observation.Maintenance!.ReferenceProtectionPolicy!.LayoutRead);
        if (read.Observation.OutsideRoutes is { } outside) Assert.Same(read.Observation.LayoutRead, outside.LayoutRead);
        Assert.False(System.IO.Directory.Exists(files.Combine(PublishedTokens.Runtime.DirectoryName)));
        return read;
    }

    private static TemporaryWorkspace Seed(string root, bool compatibility = false)
    {
        var files = TemporaryWorkspace.Create("doctor-outside-closure");
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(root, Team));
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Independent workspace\n");
        files.WriteText(root + "/" + PublishedTokens.Layout.LoaderFileName, OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        { Prefix = "# Independent Loader\n\n## Axioms\n\n- Keep workspace context precise.", Entries = PublishedTokens.Metadata.EmptyEntry }));
        files.WriteText(compatibility ? Team + "/index.md" : Anchor, Catalogue("Team", "- [Deep](deep/_deep.md) - #Review"));
        files.WriteText(Deep, Catalogue("Deep", "- [Note](note.md) - #Review"));
        files.WriteText(Leaf, Note("Note"));
        return files;
    }

    private static string Settings(string root, string registration) => $$"""
        {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{registration}}"]}
        """;

    private static string Catalogue(string description, string rows)
        => OpenForgeDocumentSeed.Metadata(description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        { Prefix = "# " + description, Entries = rows, LineEnding = "\n" }));

    private static string Note(string description) => OpenForgeDocumentSeed.Metadata(description, [Tag], "# " + description + "\n\nIndependent body bytes.\r\n");
}
