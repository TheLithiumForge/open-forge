using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceLayoutPermissionIntegrationTests
{
    private const string Parent = "docs";
    private const string CustomRoot = Parent + "/forge";
    private const string Category = WorkspaceCategoryNames.Directives;
    private const string Leaf = "review.md";
    private const string CustomPath = CustomRoot + "/" + Category + "/" + Leaf;
    private const string LegacyPath = WorkspaceLayout.DefaultMainRoot + "/" + Category + "/" + Leaf;
    private const string SkillPath = WorkspaceLayout.NativeSkillsPath + "/review/" + Leaf;
    private const string ExternalFolder = "external";
    private const string ExternalPath = ExternalFolder + "/" + Leaf;
    private const string SimilarPath = CustomRoot + "-other/" + Leaf;
    private const string CompetingSkillPath = CustomRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName + "/" + Leaf;
    private const string Owner = "permission-fixture";
    private const string LoaderContent = "# Loader\n";
    private const string ChangedContent = "changed after planning\n";
    private const string SourceRoot = "shared/team";
    private const string MixedRoot = "Docs/Forge";
    private const string Entrypoint = "_" + Category + ".md";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(CustomPath, false), InlineData(SkillPath, false)]
    [InlineData(LegacyPath, true), InlineData(ExternalPath, true)]
    [InlineData(SimilarPath, true), InlineData(CompetingSkillPath, true)]
    public async Task AdmissionFollowsContentLocations(string path, bool required)
    {
        using var temporary = Create();
        var read = await Read(temporary);
        var before = File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var extension = await new ExtensionPermissionOperation(NoPrompt).DetermineAsync(
            ExtensionRequest(read, path), Token);
        var library = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(LibraryRequest(read, path), Token);

        var decision = required ? WorkspacePermissionDecision.Required : WorkspacePermissionDecision.NotRequired;
        Assert.Equal(decision, extension.Result.Decision);
        Assert.Equal(decision, library.Result.Decision);
        Assert.Equal(required ? new[] { path } : [], extension.Result.Required);
        Assert.Equal(required ? new[] { path } : [], library.Result.Required);
        Assert.Null(extension.Change);
        Assert.Null(library.Change);
        Assert.Equal(before, File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.False(File.Exists(temporary.Combine(path)));
    }

    [Fact]
    public async Task ExplicitGrantsRemainLiteralAndQuestionsReceiveContextFolder()
    {
        using var temporary = Create();
        var read = await Read(temporary);
        var questions = new List<CliPermissionQuestion>();
        ValueTask<CliPromptReply<CliPermissionChoice>> Prompt(CliPermissionQuestion question, CliPromptPolicy policy, CancellationToken token)
        {
            questions.Add(question);
            return ValueTask.FromResult(CliPromptReply<CliPermissionChoice>.Answered(CliPermissionChoice.Once));
        }
        var extension = await new ExtensionPermissionOperation(Prompt).DetermineAsync(
            ExtensionRequest(read, ExternalPath) with { AllowPrompt = true, ExplicitGrantPaths = [LegacyPath] }, Token);
        var library = await new LibraryPermissionOperation(Prompt).DetermineAsync(
            LibraryRequest(read, ExternalPath) with { AllowPrompt = true, ExplicitGrantPaths = [LegacyPath] }, Token);

        Assert.Equal(2, questions.Count);
        Assert.All(questions, question => Assert.Equal(CustomRoot, question.ContextFolder));
        Assert.Equal(ExternalPath, Assert.Single(questions[0].Paths).Path);
        Assert.Equal(ExternalFolder, Assert.Single(questions[1].Paths).Path);
        Assert.Equal(WorkspacePermissionDecision.Approved, extension.Result.Decision);
        Assert.Equal(WorkspacePermissionDecision.Approved, library.Result.Decision);
        AssertGrant(Assert.IsType<PlannedFileChange>(extension.Change).IntendedBytes.ToArray(), LegacyPath);
        AssertGrant(Assert.IsType<PlannedFileChange>(library.Change).IntendedBytes.ToArray(), LegacyPath);
    }

    [Fact]
    public async Task IncludedOutsideRouteDoesNotBecomeImplicitPermission()
    {
        using var temporary = Create();
        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{CustomRoot}}","{{WorkspaceSettingsDefinitions.RootsProperty}}":["{{ExternalPath}}"]}""");
        temporary.CreateFile(ExternalPath, LoaderContent);
        var read = await Read(temporary);
        var before = File.ReadAllBytes(temporary.Combine(ExternalPath));
        var extension = await new ExtensionPermissionOperation(NoPrompt).DetermineAsync(ExtensionRequest(read, ExternalPath), Token);
        var library = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(LibraryRequest(read, ExternalPath), Token);
        Assert.Equal(WorkspacePermissionDecision.Required, extension.Result.Decision);
        Assert.Equal(WorkspacePermissionDecision.Required, library.Result.Decision);
        Assert.Equal(before, File.ReadAllBytes(temporary.Combine(ExternalPath)));
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task SavedGrantsUseExactPaths(bool library)
    {
        using var temporary = Create();
        using var locks = WorkspaceLockTestStore.Create(Owner);
        var read = await Read(temporary);
        var extensionOperation = new ExtensionPermissionOperation(NoPrompt);
        var extension = await extensionOperation.DetermineAsync(
            ExtensionRequest(read, LegacyPath) with { ExplicitGrantPaths = [LegacyPath] }, Token);
        var libraryStage = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(
            LibraryRequest(read, LegacyPath) with { ExplicitGrantPaths = [LegacyPath] }, Token);
        Assert.Equal(WorkspacePermissionDecision.Granted, extension.Result.Decision);
        Assert.Equal(WorkspacePermissionDecision.Granted, libraryStage.Result.Decision);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new(read.Layout!.Workspace, Owner, Guid.NewGuid()), Token)).Lease);
        var target = Assert.IsType<RecoveryBundleTarget>(library ? libraryStage.RecoveryTarget : extension.RecoveryTarget);
        var recovery = await Prepare(lease, target);

        var result = library
            ? (await LibraryPermissionOperation.ApplyAsync(lease, libraryStage, recovery, Token)).Result
            : (await extensionOperation.ApplyAsync(lease, extension, recovery, Token)).Result;

        Assert.Equal(WorkspacePermissionOutcome.Verified, result.Outcome);
        AssertGrant(File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath)), LegacyPath);
        Assert.False(File.Exists(temporary.Combine(LegacyPath)));
        Assert.False(File.Exists(temporary.Combine(CustomPath)));
    }

    [Theory]
    [InlineData(WorkspaceSettingsDefinitions.RelativePath)]
    [InlineData(CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName)]
    [InlineData(WorkspaceLayoutNames.AgentHostFileName)]
    public async Task ChangedLayoutRefusesBothGrantWrites(string changedPath)
    {
        using var temporary = Create();
        using var locks = WorkspaceLockTestStore.Create(Owner);
        var read = await Read(temporary);
        var extensionOperation = new ExtensionPermissionOperation(NoPrompt);
        var extension = await extensionOperation.DetermineAsync(
            ExtensionRequest(read, ExternalPath) with { ExplicitGrantPaths = [ExternalPath] }, Token);
        var library = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(
            LibraryRequest(read, ExternalPath) with { ExplicitGrantPaths = [ExternalPath] }, Token);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new(read.Layout!.Workspace, Owner, Guid.NewGuid()), Token)).Lease);
        var recovery = await Prepare(lease, Assert.IsType<RecoveryBundleTarget>(extension.RecoveryTarget));
        if (File.Exists(temporary.Combine(changedPath))) temporary.ReplaceText(changedPath, ChangedContent);
        else temporary.CreateFile(changedPath, ChangedContent);
        var before = File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath));

        Assert.False(await extensionOperation.RevalidateAsync(lease, extension, Token));
        Assert.False(await LibraryPermissionOperation.RevalidateAsync(lease, library, Token));
        Assert.Equal(ExtensionPermissionFailure.Changed, (await extensionOperation.ApplyAsync(lease, extension, recovery, Token)).Failure);
        Assert.Equal(LibraryPermissionFailure.Changed, (await LibraryPermissionOperation.ApplyAsync(lease, library, recovery, Token)).Failure);
        Assert.Equal(before, File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.False(File.Exists(temporary.Combine(ExternalPath)));
    }

    [Fact]
    public async Task ConflictingLayoutCannotPlanGrants()
    {
        using var temporary = Create();
        temporary.CreateFile(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, LoaderContent);
        var read = await Read(temporary, WorkspaceLayoutReadState.Mismatch);
        var extension = await new ExtensionPermissionOperation(NoPrompt).DetermineAsync(
            ExtensionRequest(read, CustomPath) with { ExplicitGrantPaths = [ExternalPath] }, Token);
        var library = await new LibraryPermissionOperation(NoPrompt).DetermineAsync(
            LibraryRequest(read, CustomPath) with { ExplicitGrantPaths = [ExternalPath] }, Token);
        Assert.Equal(ExtensionPermissionFailure.Invalid, extension.Failure);
        Assert.Equal(LibraryPermissionFailure.Invalid, library.Failure);
        Assert.Null(extension.Change);
        Assert.Null(library.Change);
    }

    [Fact]
    public async Task ConsumerBoundaryUsesNestedContextFolderAndNativeSkillAncestors()
    {
        using var temporary = Create();
        temporary.CreateDirectory(WorkspaceLayout.NativeSkillsPath);
        var read = await Read(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(read.Layout);
        var request = new LibraryConsumerBoundaryRequest
        {
            Workspace = layout.Workspace,
            Layout = layout,
            RequiredAncestorPaths = [CanonicalRelativePath.Create(Parent), CanonicalRelativePath.Create(WorkspaceLayout.DefaultMainRoot),
                CanonicalRelativePath.Create(WorkspaceLayout.NativeSkillsPath)],
        };
        var facts = await LibraryConsumerBoundaryReader.ReadAsync(new PhysicalPathResolver(), request, Token);
        var result = LibraryMutationPlanningPolicy.EvaluateConsumerBoundary(facts, allowMissingAncestors: false);
        Assert.Equal(LibraryPlanState.Complete, result.State);
        Assert.Equal(temporary.Combine(CustomRoot), facts.ConsumerRoot?.Leaf.LogicalPath);
        Assert.Empty(result.Directories);
    }

    [Theory]
    [InlineData(CustomRoot), InlineData(MixedRoot)]
    public async Task LibraryDestinationsKeepControlAndEntrypointRestrictions(string root)
    {
        using var temporary = Create();
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var layout = WorkspaceLayout.Create(workspace, root);
        var protectedPaths = new[]
        {
            root, root + "/" + WorkspaceLayoutNames.LoaderFileName,
            root + "/" + Category + "/" + Entrypoint,
            WorkspaceSettingsDefinitions.RelativePath,
            WorkspaceLayout.NativeSkillsPath,
        };
        foreach (var path in protectedPaths)
        {
            var registration = LibraryRegistration.Create(LibraryId.Create(Owner), WorkspaceRelativeDirectory.Create(SourceRoot),
                LibraryDestinationRoot.Create(LibraryDestinationRoot.WorkspaceRootValue), [SourceRelativeEligiblePath.Create(path)]);
            Assert.Equal(path, LibraryDestinationPolicy.FindConflict(layout, registration, registered: null, generatedTargets: []));
        }
        var ordinary = root + "/" + Category + "/" + Leaf;
        var selected = LibraryRegistration.Create(LibraryId.Create(Owner), WorkspaceRelativeDirectory.Create(SourceRoot),
            LibraryDestinationRoot.Create(LibraryDestinationRoot.WorkspaceRootValue), [SourceRelativeEligiblePath.Create(ordinary)]);
        Assert.Null(LibraryDestinationPolicy.FindConflict(layout, selected, registered: null, generatedTargets: []));
    }

    private static TemporaryWorkspace Create()
    {
        var temporary = TemporaryWorkspace.Create(Owner);
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{CustomRoot}}"}""");
        temporary.CreateFile(CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName, LoaderContent);
        return temporary;
    }

    private static async ValueTask<WorkspaceLayoutRead> Read(TemporaryWorkspace temporary, WorkspaceLayoutReadState state = WorkspaceLayoutReadState.Ready)
    {
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var read = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, Token);
        Assert.Equal(state, read.State);
        return read;
    }

    private static ExtensionPermissionRequest ExtensionRequest(WorkspaceLayoutRead read, string path)
        => new(read.Layout!.Workspace, [new(path, ExtensionPermissionEffect.Copy)], Owner, false) { LayoutRead = read };

    private static LibraryPermissionRequest LibraryRequest(WorkspaceLayoutRead read, string path) => new()
    {
        Workspace = read.Layout!.Workspace,
        LibraryId = LibraryId.Create(Owner),
        SettingsObservation = read.Settings,
        LayoutRead = read,
        Targets = [new(path, LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink }],
        AllowPrompt = false,
    };

    private static ValueTask<CliPromptReply<CliPermissionChoice>> NoPrompt(CliPermissionQuestion question, CliPromptPolicy policy, CancellationToken token)
        => throw new InvalidOperationException("This permission scenario must not prompt.");

    private static void AssertGrant(byte[] bytes, string expected)
    {
        using var settings = JsonDocument.Parse(bytes);
        Assert.Equal(CustomRoot, settings.RootElement.GetProperty(WorkspaceSettingsDefinitions.MainRootProperty).GetString());
        Assert.Equal(expected, Assert.Single(settings.RootElement.GetProperty(WorkspaceSettingsDefinitions.AllowInstallPathsProperty).EnumerateArray()).GetString());
    }

    private static async ValueTask<RecoveryBundlePreparation> Prepare(WorkspaceLockLease lease, RecoveryBundleTarget target)
    {
        var workspace = lease.Request.Workspace;
        var input = RecoveryBundleInput.Create(workspace, Owner,
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Extension, RecoveryBundleOperation.Install, workspace), lease.Request.OperationId, [target]);
        return Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, Token)).Preparation);
    }
}
