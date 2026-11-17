using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Application;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

// The strip Behavior contract requires a changed file under the lease and an interruption to stop, say how many
// changes it made and keep its recovery data.
public sealed class StripApplicationIntegrationTests
{
    private const string Notes = WorkspaceLayout.DefaultMainRoot + "/notes";
    private const string Note = Notes + "/note.md";
    private const string Other = Notes + "/other.md";
    private const string Original = "---\nopen-forge:\n  description: Note\n  tags: [Docs]\n---\n# Note\n";

    [Fact(DisplayName = "Strip stops before any write when a planned file changes after the preview"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task ChangedFileUnderTheLease()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-target-changed");
        workspace.Write(Note, Original);
        var (request, builder, plan) = await PlanAsync(workspace, null);
        var changed = Original + "\nA later edit.\n";
        workspace.Write(Note, changed);

        var result = await Application(workspace)
            .ApplyAsync(plan, token => builder.BuildAsync(request, token), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(StripFindingCode.TargetChanged, result.Findings[0].Code);
        Assert.Equal(changed, workspace.Read(Note));
    }

    // The edit lands after the rebuild under the lease has matched the preview, so only the per-file check sees it.
    [Fact(DisplayName = "Strip stops part way with target-changed-during-apply and keeps its recovery data when a later file changes"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task ChangedFileDuringApply()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-target-changed-during-apply");
        workspace.Write(Note, Original);
        workspace.Write(Other, Original);
        var (request, builder, plan) = await PlanAsync(workspace, null);
        var changed = Original + "\nA later edit.\n";

        var result = await Application(workspace).ApplyAsync(plan, async token =>
            {
                var rebuilt = await builder.BuildAsync(request, token);
                workspace.Write(Other, changed);
                return rebuilt;
            }, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(StripFindingCode.TargetChangedDuringApply, result.Findings[0].Code);
        Assert.Equal(Other, result.Findings[0].Path);
        Assert.Equal(CommandLines.Doctor, result.Next?.Command);
        Assert.Equal(StripRunOutcome.Unknown, result.Outcome);
        Assert.Equal(StripRecoveryState.Retained, result.Recovery);
        Assert.True(Path.Exists(result.RecoveryPath));
        Assert.Equal(plan.Entries.Single(entry => entry.Path == Note).After, Hash(workspace, Note));
        Assert.Equal(changed, workspace.Read(Other));
    }

    [Fact(DisplayName = "Strip reports an interruption before its first effect and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task InterruptedBeforeEffects()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-interrupted");
        workspace.Write(Note, Original);
        var (request, builder, plan) = await PlanAsync(workspace, null);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await Application(workspace)
            .ApplyAsync(plan, token => builder.BuildAsync(request, token), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(StripFindingCode.Interrupted, result.Findings[0].Code);
        Assert.Equal(StripRunOutcome.NotStarted, result.Outcome);
        Assert.Equal(Original, workspace.Read(Note));
    }

    private static StripApplication Application(StripIntegrationWorkspace workspace)
        => new(new PhysicalPathResolver(), workspace.LockStoreRoot);

    private static async Task<(StripRequest Request, StripPlanBuilder Builder, StripPlan Plan)> PlanAsync(StripIntegrationWorkspace workspace, string? flatten)
    {
        var request = workspace.Request(StripCommandKind.Strip, [StripPart.Metadata], flatten);
        var builder = new StripPlanBuilder(new PhysicalPathResolver());
        var plan = Assert.IsType<StripPlan>((await builder.BuildAsync(request, TestContext.Current.CancellationToken)).Plan);
        return (request, builder, plan);
    }

    private static string Hash(StripIntegrationWorkspace workspace, string path) => FileExpectation.Hash(workspace.ReadBytes(path));
}
