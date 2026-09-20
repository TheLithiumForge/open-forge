using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
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
