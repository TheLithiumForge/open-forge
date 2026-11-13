using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Install.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.UnitTests.Commands.Tool.Run.Shared.Resolution;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class ToolRunSessionTests
{
    [Theory(DisplayName = "Interactive missing-tool consent uses the resolved workspace and rechecks its pin after team Install")]
    [InlineData((int)CliSemanticStatus.Complete)]
    [InlineData((int)CliSemanticStatus.Blocked)]
    public async Task OffersNormalInstall(int installStatus)
    {
        var seed = new ToolRunPinSeed();
        var calls = 0;
        ToolInstallRequest? offered = null;
        ToolRunRequest? retry = null;
        var session = new ToolRunSession((request, _) =>
        {
            calls++;
            if (calls == 1) return ValueTask.FromResult<ToolRunResolution>(Missing(seed));
            retry = request;
            return ValueTask.FromResult<ToolRunResolution>(new ToolRunReady(new(seed.Launch, request.CurrentDirectory, request.Arguments)) { Workspace = seed.Workspace });
        }, (request, _) =>
        {
            offered = request;
            return ValueTask.FromResult<ToolInstallResult?>(new()
            {
                Status = (CliSemanticStatus)installStatus,
                Workspace = request.Workspace,
                Data = new() { Mode = ToolWriteMode.Apply, Id = request.Id, Pin = null, Role = null },
            });
        });
        var result = await session.ResolveAsync(seed.Request with { Interactive = true }, TestContext.Current.CancellationToken);
        Assert.IsType<ToolRunReady>(result);
        Assert.NotNull(offered);
        Assert.Same(seed.Workspace, offered.Workspace);
        Assert.Null(offered.Id);
        Assert.True(offered.Options.AllowInteraction);
        Assert.False(offered.Options.Automatic);
        Assert.False(offered.AllowDownload);
        Assert.False(offered.Options.AllowSessionDaemon);
        Assert.Null(offered.Source);
        Assert.Null(offered.Version);
        Assert.Empty(offered.Components);
        Assert.Empty(offered.Options.AllowPaths);
        Assert.Equal(seed.Workspace.LexicalRoot, retry?.WorkspacePath);
        Assert.Equal(seed.Request.CurrentDirectory, retry?.CurrentDirectory);
        Assert.Equal(2, calls);
    }

    [Theory(DisplayName = "Non-interactive, declined and stubbed missing-tool requests never silently install")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task KeepsRefusal(bool interactive, bool stub)
    {
        var seed = new ToolRunPinSeed();
        var refusal = Missing(seed);
        var calls = 0;
        var session = new ToolRunSession((_, _) => ValueTask.FromResult<ToolRunResolution>(refusal), stub ? null : (_, _) =>
        {
            calls++;
            return ValueTask.FromResult<ToolInstallResult?>(null);
        });
        Assert.Same(refusal, await session.ResolveAsync(seed.Request with { Interactive = interactive }, TestContext.Current.CancellationToken));
        Assert.Equal(interactive && !stub ? 1 : 0, calls);
    }

    [Fact(DisplayName = "An interrupted installation offer returns pre-launch cancellation with the same workspace")]
    public async Task CancelsOffer()
    {
        var seed = new ToolRunPinSeed();
        var session = new ToolRunSession((_, _) => ValueTask.FromResult<ToolRunResolution>(Missing(seed)), (_, _) => throw new OperationCanceledException());
        var result = await session.ResolveAsync(seed.Request with { Interactive = true }, TestContext.Current.CancellationToken);
        var refused = Assert.IsType<ToolRunRefused>(result);
        Assert.Equal(ToolRunFailureCode.Cancelled, refused.Failure.Code);
        Assert.Same(seed.Workspace, refused.Failure.Workspace);
    }

    private static ToolRunRefused Missing(ToolRunPinSeed seed)
        => new(new(ToolRunFailureCode.DeclaredToolMissing, seed.Request.Id, "fixture", true) { Workspace = seed.Workspace });
}
