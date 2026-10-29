using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Diagnostics;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "install-main-root"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallMainRootIntegrationTests
{
    private const string FixtureName = "install-main-root";
    private const string CustomRoot = "docs/forge";
    private const string OtherRoot = "context/forge";
    private const string OccupantName = "authored.txt";
    private const string Loader = CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string GuidanceDirectory = CustomRoot + "/" + WorkspaceCategoryNames.Guidance;
    private static readonly string Guidance = $"{GuidanceDirectory}/{SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceCategoryNames.Guidance)}";
    private static readonly byte[] AuthoredBytes = [0x41, 0x0D, 0x0A, 0x42, 0x0A];

    [Theory(DisplayName = "Internal Install maps a custom root and preserves fixed controls and native Skills")]
    [InlineData(true, false), InlineData(false, false), InlineData(false, true)]
    public async Task CustomInstall(bool rootForm, bool emptyDirectory)
    {
        var form = rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        if (emptyDirectory) workspace.CreateDirectory(CustomRoot);
        var request = await RequestAsync(workspace, form);
        var operation = InstallFrontmatterFixture.Operation(workspace);
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(result.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(result));
        var read = await ReadLayoutAsync(workspace);
        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(CustomRoot, read.Settings.Document.DeclaredMainRoot);
        Assert.Equal(form, read.Settings.Document.Frontmatter);
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, settings.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.True(File.Exists(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.True(File.Exists(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.True(File.Exists(workspace.Combine(Loader)));
        Assert.False(File.Exists(workspace.Combine(SourceLogicalPath.LoaderPath)));
        Assert.False(Directory.Exists(workspace.Combine(CustomRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName)));
        Assert.True(File.Exists(WorkspaceRuntimePaths.LockPath(workspace.Workspace)));
        Assert.False(Directory.Exists(workspace.Combine(CustomRoot + "/" + WorkspaceRuntimePaths.DirectoryName)));

        var framework = Assert.IsType<Core.Framework.Ownership.Models.Document.FrameworkOwnership>(read.Ownership.Document.Framework);
        Assert.Contains(Loader, framework.Paths);
        Assert.Contains(Guidance, framework.Paths);
        Assert.DoesNotContain(SourceLogicalPath.LoaderPath, framework.Paths);
        foreach (var host in new[] { WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName })
            Assert.Contains(framework.Regions, region => region.Path == host && region.Region == WorkspaceOwnershipDefinitions.ManagedBlockRegion);
        var agent = new MarkdownDocumentParser().Parse(File.ReadAllText(workspace.Combine(WorkspaceLayoutNames.AgentHostFileName)));
        Assert.Contains(agent.OpaqueSpans.Where(span => span.IsCode).Select(span => agent.Source[span.Span.Start..span.Span.End].Trim('`')), value => value == Loader);

        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var skill = Assert.Single(payload.Assets, asset => SourceFormClassifier.Matches(DefaultWorkspaceLayout, asset.Path, SourceDocumentForm.Skill));
        Assert.True(File.Exists(workspace.Combine(skill.Path)));
        Assert.Contains(skill.Path, framework.Paths);
        Assert.Contains(result.Facts.Effects, effect => effect.Path == Loader && effect.SourceAssetPath == FrameworkPayloadAsset.LoaderPath);
        AssertForm(workspace, form);

        var before = Bytes(workspace);
        var repeat = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.True(repeat.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(repeat));
        Assert.Empty(repeat.Facts.Effects);
        AssertBytes(before, workspace);
    }

    [Fact(DisplayName = "Custom Install refuses a file in place of its root before acquiring the workspace lock")]
    public async Task FileDestination()
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        workspace.WriteText(CustomRoot, string.Empty);
        File.WriteAllBytes(workspace.Combine(CustomRoot), AuthoredBytes);
        var before = Bytes(workspace);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            workspace.Request(force: true) with { RequestedMainRoot = CustomRoot }, TestContext.Current.CancellationToken);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.TargetUnsafe);
        Assert.Empty(result.Facts.Effects);
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
        AssertBytes(before, workspace);
    }

    [Fact(DisplayName = "Custom Install adopts an existing folder and keeps its other files")]
    public async Task OccupiedFolder()
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        var occupant = $"{CustomRoot}/{OccupantName}";
        workspace.WriteText(occupant, string.Empty);
        File.WriteAllBytes(workspace.Combine(occupant), AuthoredBytes);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            workspace.Request() with { RequestedMainRoot = CustomRoot }, TestContext.Current.CancellationToken);
        Assert.True(result.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(result));
        Assert.True(File.Exists(workspace.Combine(Loader)));
        Assert.Equal(AuthoredBytes, File.ReadAllBytes(workspace.Combine(occupant)));
    }

    [Theory(DisplayName = "A fresh Install keeps an existing file at the guide path, leaves the guide out and records the exclusion")]
    [InlineData(CustomRoot), InlineData(WorkspaceLayout.DefaultMainRoot)]
    public async Task KeptGuide(string root)
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, root);
        var guide = WorkspaceLayout.Create(workspace.Workspace, root).HumanGuidePath;
        workspace.WriteText(guide, string.Empty);
        File.WriteAllBytes(workspace.Combine(guide), AuthoredBytes);
        var request = workspace.Request() with { RequestedMainRoot = root == WorkspaceLayout.DefaultMainRoot ? null : root };
        // The kept guide is resolved with the setup choices, so this uses the composed operation.
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(result.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(result));
        Assert.Equal(guide, result.Input.KeptGuide);
        Assert.Equal(AuthoredBytes, File.ReadAllBytes(workspace.Combine(guide)));
        Assert.Contains(guide, (await ReadLayoutAsync(workspace)).Settings.Document.RemovedFiles);

        var repeat = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(repeat.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(repeat));
        Assert.Empty(repeat.Facts.Effects);
        Assert.Equal(AuthoredBytes, File.ReadAllBytes(workspace.Combine(guide)));
    }

    [Theory(DisplayName = "Install never selects the fixed workspace runtime directory as content")]
    [InlineData(WorkspaceRuntimePaths.DirectoryName), InlineData(WorkspaceRuntimePaths.DirectoryName + "/" + OccupantName)]
    public async Task RuntimeRootIsReserved(string root)
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            workspace.Request() with { RequestedMainRoot = root }, TestContext.Current.CancellationToken);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.InvalidInput);
        Assert.Empty(result.Facts.Effects);
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    [Theory(DisplayName = "Established Install refuses root changes before a held lock even during configuration")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task EstablishedRootCannotChange(bool configure, bool defaultRoot)
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        var operation = InstallFrontmatterFixture.Operation(workspace);
        var firstRequest = await RequestAsync(workspace, FrontmatterForm.Scoped);
        if (defaultRoot) firstRequest = firstRequest with { RequestedMainRoot = null };
        var first = await operation.ExecuteAsync(firstRequest, TestContext.Current.CancellationToken);
        Assert.True(first.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(first));
        var held = await new WorkspaceLockManager(workspace.LockStoreRoot).AcquireAsync(
            new(workspace.Workspace, InstallDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        using var lease = Assert.IsType<WorkspaceLockLease>(held.Lease);
        var before = Bytes(workspace);
        var result = await operation.ExecuteAsync(workspace.Request(force: true) with
        {
            RequestedMainRoot = OtherRoot,
            Configuration = configure ? new(true, InstallPreset.FullCore, InstallConfigurationChoices.Defaults(InstallPreset.FullCore)) : null,
        }, TestContext.Current.CancellationToken);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.InvalidInput);
        Assert.Empty(result.Facts.Effects);
        AssertBytes(before, workspace);
    }

    [Theory(DisplayName = "Install revalidates settings and the planned Framework paths in a custom root before effects")]
    [InlineData(false), InlineData(true)]
    public async Task ChangedPlanStops(bool settingsChanged)
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        workspace.CreateDirectory(CustomRoot);
        var paths = new PhysicalPathResolver();
        var build = await new InstallPlanBuilder(paths).BuildAsync(await RequestAsync(workspace, FrontmatterForm.Scoped), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<InstallPlan>(build.Plan);
        Assert.True(plan.IsComplete);
        var changed = settingsChanged ? WorkspaceSettingsDefinitions.RelativePath : Loader;
        workspace.WriteText(changed, string.Empty);
        File.WriteAllBytes(workspace.Combine(changed), AuthoredBytes);
        var outcome = await InstallApplicationOperationFactory.Create(paths, new FileExpectationValidator(paths), workspace.LockStoreRoot)
            .ExecuteAsync(plan, TestContext.Current.CancellationToken);
        Assert.Contains(outcome.Findings, finding => finding.Code == InstallFindingCode.TargetUnsafe);
        if (settingsChanged) Assert.False(File.Exists(workspace.Combine(Loader)));
        Assert.False(File.Exists(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.Equal(AuthoredBytes, File.ReadAllBytes(workspace.Combine(changed)));
    }

    [Fact(DisplayName = "Custom configuration uses mapped exclusions and sharing and can convert owned frontmatter")]
    public async Task ConfigureCustomRoot()
    {
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        var operation = InstallFrontmatterFixture.Operation(workspace);
        var request = await RequestAsync(workspace, FrontmatterForm.Scoped);
        var choices = InstallConfigurationChoices.Defaults(InstallPreset.Essentials);
        var installed = await operation.ExecuteAsync(request with
        {
            Configuration = new(false, InstallPreset.Essentials, choices),
        }, TestContext.Current.CancellationToken);
        Assert.True(installed.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(installed));
        var read = await ReadLayoutAsync(workspace);
        Assert.Contains(WorkspaceCategoryNames.Guidance, read.Settings.Document.RemovedCategories);
        Assert.False(File.Exists(workspace.Combine(Guidance)));
        var routes = Assert.IsType<Core.Framework.Ownership.Models.Document.FrameworkOwnership>(read.Ownership.Document.Framework).GitIgnoredRoutes;
        var sharedChoice = Assert.Single(choices, row => row.Action == InstallRouteAction.GitIgnore);
        Assert.Equal($"{CustomRoot}/{sharedChoice.Id}", Assert.Single(routes).Directory);
        Assert.NotEmpty(InstallIgnoreSection.Read(File.ReadAllBytes(workspace.Combine(InstallIgnoreSection.Path)), Assert.IsType<WorkspaceLayout>(read.Layout)));

        var configured = await operation.ExecuteAsync((await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true)) with
        {
            RequestedMainRoot = CustomRoot,
            Configuration = new(true, InstallPreset.FullCore, InstallConfigurationChoices.Defaults(InstallPreset.FullCore)),
        }, TestContext.Current.CancellationToken);
        Assert.True(configured.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(configured));
        AssertForm(workspace, FrontmatterForm.Root);
        Assert.Equal(WorkspaceLayoutReadState.Ready, (await ReadLayoutAsync(workspace)).State);
    }

    [Theory(DisplayName = "Internal custom Install feeds Context and Index through the observed layout")]
    [InlineData(true), InlineData(false)]
    public async Task InstallThenContextThenIndex(bool rootForm)
    {
        var form = rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        using var workspace = InstallOperationWorkspace.Create(FixtureName, CustomRoot);
        var installed = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(await RequestAsync(workspace, form), TestContext.Current.CancellationToken);
        Assert.True(installed.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(installed));
        var context = await ContextOperationFactory.Create().ExecuteAsync(new ContextRequest(workspace.Workspace, [], false,
            new ContextContentSelection([], []), ContextLinkExpansion.None, null, CliDetail.Minimal), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, context.Status);
        Assert.Contains(context.Sources, source => source.Path == Loader);
        var before = Bytes(workspace);
        var index = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new(workspace.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, index.Status);
        AssertBytes(before, workspace);
    }

    private static async Task<Core.Commands.Install.Models.Request.InstallRequest> RequestAsync(InstallOperationWorkspace workspace, FrontmatterForm form)
        => (await InstallFrontmatterFixture.RequestAsync(workspace, form)) with { RequestedMainRoot = CustomRoot };

    private static ValueTask<WorkspaceLayoutRead> ReadLayoutAsync(InstallOperationWorkspace workspace)
        => new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);

    private static void AssertForm(InstallOperationWorkspace workspace, FrontmatterForm expected)
    {
        var document = new MarkdownDocumentParser().Parse(File.ReadAllText(workspace.Combine(Guidance)));
        var metadata = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(FrameworkDocumentMetadataState.Complete, metadata.State);
        Assert.Equal(expected, metadata.Syntax.AuthoredForm);
    }

    private static Dictionary<string, byte[]> Bytes(InstallOperationWorkspace workspace)
        => Directory.EnumerateFiles(workspace.PhysicalPath, "*", SearchOption.AllDirectories)
            .Where(path => !WorkspaceRuntimePaths.Contains(workspace.Workspace, path))
            .ToDictionary(path => Path.GetRelativePath(workspace.PhysicalPath, path), File.ReadAllBytes, StringComparer.Ordinal);

    private static void AssertBytes(Dictionary<string, byte[]> expected, InstallOperationWorkspace workspace)
    {
        var actual = Bytes(workspace);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach (var pair in expected) Assert.Equal(pair.Value, actual[pair.Key]);
    }

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(InstallMainRootIntegrationTests)),
                Path.GetFullPath(nameof(InstallMainRootIntegrationTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
