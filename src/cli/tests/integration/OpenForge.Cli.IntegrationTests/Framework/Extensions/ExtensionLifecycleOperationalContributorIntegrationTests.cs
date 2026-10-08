using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Settings;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;

using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

public sealed class ExtensionLifecycleOperationalContributorIntegrationTests
{
    private const string TargetPath = ".agents/toolkit.md";
    private const string ScopedContent = """
        ---
        open-forge:
          description: Toolkit guidance
          tags: [Guidance]
        ---

        # Toolkit
        """;
    private const string RootContent = """
        ---
        description: Toolkit guidance
        tags: [Guidance]
        ---

        # Toolkit
        """;

    [Fact(DisplayName = "Extension lifecycle Status and Doctor report matching root delivery as current")]
    [Trait("Boundary", "OS"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    public async Task MatchingRootDeliveryIsCurrent()
    {
        using var workspace = TemporaryWorkspace.Create("extension-root-target");
        using var source = TemporaryWorkspace.Create("extension-root-source");
        WriteFrontmatterScenario(workspace, source, RootContent);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"root"}""");
        var (status, doctor) = await ReadFrontmatterViewsAsync(workspace, source);
        Assert.Equal(OperationalTargetState.Current, Assert.Single(status.Targets).State);
        Assert.Equal(OperationalTargetState.Current, Assert.Single(doctor.Targets).Target.State);
        Assert.Equal(ExtensionManagedSetState.Current, doctor.ManagedSet);
        Assert.DoesNotContain(ExtensionLifecycleDoctorInspector.Inspect(doctor).Findings, finding =>
            finding.Kind == DoctorFindingKind.ExtensionManagedChanged);
    }

    [Theory(DisplayName = "Extension lifecycle reports opposite forms with the existing managed-change finding and Update advice")]
    [Trait("Boundary", "OS"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    [InlineData("root", false)]
    [InlineData("scoped", true)]
    public async Task OppositeFormReportsManagedChange(string setting, bool authoredRoot)
    {
        using var workspace = TemporaryWorkspace.Create("extension-opposite-target");
        using var source = TemporaryWorkspace.Create("extension-opposite-source");
        WriteFrontmatterScenario(workspace, source, authoredRoot ? RootContent : ScopedContent);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""{"schemaVersion":1,"frontmatter":"{{setting}}"}""");
        var (status, doctor) = await ReadFrontmatterViewsAsync(workspace, source);
        Assert.Equal(OperationalTargetState.Changed, Assert.Single(status.Targets).State);
        Assert.Equal(OperationalTargetState.Changed, Assert.Single(doctor.Targets).Target.State);
        var finding = Assert.Single(ExtensionLifecycleDoctorInspector.Inspect(doctor).Findings,
            finding => finding.Kind == DoctorFindingKind.ExtensionManagedChanged);
        Assert.Equal("extension.managed-changed", DoctorDefinitions.ReadFindingKind(finding.Kind));
        Assert.Equal(DoctorNextOperation.ExtensionUpdate, Assert.Single(finding.Actions).Operation);
    }

    [Theory(DisplayName = "Extension lifecycle leaves comparisons unavailable for invalid or unavailable settings")]
    [Trait("Boundary", "OS"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidSettingsMakesComparisonUnavailable(bool unavailable)
    {
        using var workspace = TemporaryWorkspace.Create("extension-invalid-target");
        using var source = TemporaryWorkspace.Create("extension-invalid-source");
        WriteFrontmatterScenario(workspace, source, ScopedContent);
        if (unavailable) workspace.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        else workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"unknown"}""");
        var (status, doctor) = await ReadFrontmatterViewsAsync(workspace, source);
        Assert.Equal(OperationalViewState.Incomplete, status.State);
        Assert.Equal(OperationalTargetState.Unavailable, Assert.Single(status.Targets).State);
        var target = Assert.Single(doctor.Targets).Target;
        Assert.Equal(OperationalTargetState.Unavailable, target.State);
        Assert.Null(target.IntendedFingerprint);
        Assert.Equal(ExtensionManagedSetState.Unavailable, doctor.ManagedSet);
    }

    [Fact(DisplayName = "Extension lifecycle leaves intended comparisons unavailable when root rendering collides with foreign metadata")]
    [Trait("Boundary", "OS"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    public async Task InvalidRenderingMakesComparisonUnavailable()
    {
        using var workspace = TemporaryWorkspace.Create("extension-collision-target");
        using var source = TemporaryWorkspace.Create("extension-collision-source");
        var collision = ScopedContent.Replace("open-forge:", "description: Foreign metadata\nopen-forge:", StringComparison.Ordinal);
        WriteFrontmatterScenario(workspace, source, collision);
        source.ReplaceText($"content/{TargetPath}", collision);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"root"}""");
        var (status, doctor) = await ReadFrontmatterViewsAsync(workspace, source);
        Assert.Equal(OperationalTargetState.Unavailable, Assert.Single(status.Targets).State);
        Assert.Null(Assert.Single(doctor.Targets).Target.IntendedFingerprint);
    }

    private static void WriteFrontmatterScenario(TemporaryWorkspace workspace, TemporaryWorkspace source, string currentContent)
    {
        source.WriteText("extension.json", """{"id":"toolkit","name":"Toolkit","description":"Test package","version":"1.0.0","dependencies":[]}""");
        source.WriteText($"content/{TargetPath}", ScopedContent);
        workspace.WriteText(TargetPath, currentContent);
        WriteLifecycle(workspace, [new ExtensionOwnership("toolkit", "1.0.0", source.Path, [], [TargetPath], [])]);
    }

    private static async Task<(ExtensionLifecycleStatusView Status, ExtensionLifecycleDoctorView Doctor)> ReadFrontmatterViewsAsync(
        TemporaryWorkspace workspace, TemporaryWorkspace source)
    {
        var before = workspace.SnapshotHashes();
        var sourceBefore = source.SnapshotHashes();
        var resolver = new PhysicalPathResolver();
        var contributor = new ExtensionLifecycleOperationalContributor(resolver, new ExtensionSourceReader(resolver), new ExtensionLifecycleTargetReader(resolver));
        var subject = Workspace(workspace);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(resolver, subject, TestContext.Current.CancellationToken);
        var status = await contributor.ReadStatusAsync(subject, ownership, TestContext.Current.CancellationToken);
        var doctor = await contributor.ReadDoctorAsync(subject, TestContext.Current.CancellationToken);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(sourceBefore, source.SnapshotHashes());
        return (status, doctor);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension lifecycle Doctor view treats a present empty section as complete"),
        Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    public async Task DoctorViewTreatsPresentEmptySectionAsComplete()
    {
        using var workspace = TemporaryWorkspace.Create("extension-doctor-empty");
        WriteLifecycle(workspace, []);
        var resolver = new PhysicalPathResolver();
        var contributor = new ExtensionLifecycleOperationalContributor(
            physicalPathResolver: resolver,
            sourceReader: new ExtensionSourceReader(resolver),
            targetReader: new ExtensionLifecycleTargetReader(resolver));

        var view = await contributor.ReadDoctorAsync(
            Workspace(workspace),
            TestContext.Current.CancellationToken);

        Assert.Equal(ExtensionLifecycleSectionState.Present, view.Section);
        Assert.Equal(OperationalViewState.Complete, view.State);
        Assert.Equal(OperationalLifecycleState.Trusted, view.LifecycleState);
        Assert.Equal(OperationalSourceAvailability.NotApplicable, view.SourceAvailability);
        Assert.Equal(ExtensionManagedSetState.Empty, view.ManagedSet);
        var source = Assert.Single(view.Sources);
        Assert.Null(source.RecordedSource);
        Assert.Equal(ExtensionSourceReadState.Complete, source.Read.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension lifecycle Doctor view preserves distinct sources and installed facts when one source is unavailable"),
        Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Integration")]
    public async Task DoctorViewPreservesDistinctSourcesAndInstalledFactsWhenOneSourceIsUnavailable()
    {
        using var workspace = TemporaryWorkspace.Create("extension-doctor-workspace");
        using var sources = TemporaryWorkspace.Create("extension-doctor-sources");
        var missingSource = sources.Combine("A-missing");
        var availableSource = sources.CreateDirectory("a-available");
        sources.WriteText(
            "a-available/extension.json",
            """
            {
              "id": "available-package",
              "name": "Available package",
              "description": "A readable recorded source.",
              "version": "1.0.0",
              "dependencies": []
            }
            """);
        var packages = new[]
        {
            Package("alpha-embedded", source: null),
            Package("available-first", availableSource),
            Package("available-second", availableSource),
            Package("unavailable", missingSource),
            Package("zeta-embedded", source: null),
        };
        WriteLifecycle(workspace, packages);
        var resolver = new PhysicalPathResolver();
        var contributor = new ExtensionLifecycleOperationalContributor(
            physicalPathResolver: resolver,
            sourceReader: new ExtensionSourceReader(resolver),
            targetReader: new ExtensionLifecycleTargetReader(resolver));

        var view = await contributor.ReadDoctorAsync(
            Workspace(workspace),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceOwnershipReadState.Complete, view.Ownership.State);
        Assert.Equal(OperationalLifecycleState.Trusted, view.LifecycleState);
        Assert.Equal(
            [
                ("alpha-embedded", (string?)null),
                ("available-first", availableSource),
                ("available-second", availableSource),
                ("unavailable", missingSource),
                ("zeta-embedded", (string?)null),
            ],
            view.Ownership.Document.Extensions
                .Select(package => (package.Id, package.Source))
                .OrderBy(package => package.Id, StringComparer.Ordinal));
        Assert.Collection(
            view.Sources,
            embedded => AssertSource(
                observation: embedded,
                recordedSource: null,
                readIdentity: "embedded catalogue",
                readState: ExtensionSourceReadState.Complete),
            missing => AssertSource(
                observation: missing,
                recordedSource: missingSource,
                readIdentity: missingSource,
                readState: ExtensionSourceReadState.Missing),
            available => AssertSource(
                observation: available,
                recordedSource: availableSource,
                readIdentity: availableSource,
                readState: ExtensionSourceReadState.Complete));
    }

    private static ExtensionOwnership Package(string id, string? source)
        => new(id, "1.0.0", source, [], [], []);

    private static void WriteLifecycle(TemporaryWorkspace workspace, ExtensionOwnership[] packages)
        => workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath,
            WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with { Extensions = [.. packages] }));

    private static CliWorkspace Workspace(TemporaryWorkspace workspace)
        => new(
            lexicalRoot: workspace.Path,
            physicalRoot: workspace.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static void AssertSource(
        ExtensionSourceObservation observation,
        string? recordedSource,
        string readIdentity,
        ExtensionSourceReadState readState)
    {
        Assert.Equal(recordedSource, observation.RecordedSource);
        Assert.Equal(readIdentity, observation.Read.Identity);
        Assert.Equal(readState, observation.Read.State);
    }
}
