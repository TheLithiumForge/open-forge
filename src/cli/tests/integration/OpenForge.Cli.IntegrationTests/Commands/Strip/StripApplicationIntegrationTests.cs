using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Application;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Strip;

// The strip Behavior contract requires a changed file under the lease and an interruption to stop before any write.
public sealed class StripApplicationIntegrationTests
{
    private const string Note = WorkspaceLayout.DefaultMainRoot + "/notes/note.md";
    private const string Original = "---\nopen-forge:\n  description: Note\n  tags: [Docs]\n---\n# Note\n";

    [Fact(DisplayName = "Strip stops before any write when a planned file changes after the preview"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task ChangedFileUnderTheLease()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-target-changed");
        workspace.Write(Note, Original);
        var request = workspace.Request(StripCommandKind.Strip, [StripPart.Metadata]);
        var resolver = new PhysicalPathResolver();
        var builder = new StripPlanBuilder(resolver);
        var plan = Assert.IsType<StripPlan>(
            (await builder.BuildAsync(request, TestContext.Current.CancellationToken)).Plan);
        var changed = Original + "\nA later edit.\n";
        workspace.Write(Note, changed);

        var result = await new StripApplication(resolver, workspace.LockStoreRoot)
            .ApplyAsync(plan, token => builder.BuildAsync(request, token), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(StripFindingCode.TargetChanged, result.Findings[0].Code);
        Assert.Equal(changed, workspace.Read(Note));
    }

    [Fact(DisplayName = "Strip reports an interruption before its first effect and writes nothing"), Trait("Feature", "strip"), Trait("Evidence", "Integration")]
    public async Task InterruptedBeforeEffects()
    {
        using var workspace = StripIntegrationWorkspace.Create("strip-interrupted");
        workspace.Write(Note, Original);
        var request = workspace.Request(StripCommandKind.Strip, [StripPart.Metadata]);
        var resolver = new PhysicalPathResolver();
        var builder = new StripPlanBuilder(resolver);
        var plan = Assert.IsType<StripPlan>(
            (await builder.BuildAsync(request, TestContext.Current.CancellationToken)).Plan);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await new StripApplication(resolver, workspace.LockStoreRoot)
            .ApplyAsync(plan, token => builder.BuildAsync(request, token), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(StripFindingCode.Interrupted, result.Findings[0].Code);
        Assert.Equal(Original, workspace.Read(Note));
    }
}
