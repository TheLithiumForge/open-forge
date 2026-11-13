using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Integration")]
public sealed class ToolRunResolutionTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Run selects explicit workspace before host project directory and upward discovery while keeping child CWD")]
    public async Task SelectsWorkspaceInOrder()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        foreach (var request in new[]
        {
            fixture.Request with { HostProjectDirectory = fixture.Temporary.Combine("absent") },
            fixture.Request with { WorkspacePath = null, HostProjectDirectory = fixture.Workspace.LexicalRoot },
            fixture.Request with { WorkspacePath = null },
        })
        {
            var before = fixture.Temporary.SnapshotHashes();
            var resolution = await fixture.Operation.ResolveAsync(request, Token);
            Assert.Equal(fixture.Cwd, Assert.IsType<ToolRunReady>(resolution).Dispatch.WorkingDirectory);
            Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        }
    }

    [Theory(DisplayName = "Run never falls back from an invalid supplied workspace or host project directory")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RefusesBadSelection(bool explicitWorkspace)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var absent = fixture.Temporary.Combine("absent");
        var request = explicitWorkspace ? fixture.Request with { WorkspacePath = absent } : fixture.Request with { WorkspacePath = null, HostProjectDirectory = absent };
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(request, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.WorkspaceUnavailable, failure.Code);
        Assert.Equal(3, failure.ExitCode);
        Assert.Null(failure.Workspace);
    }

    [Theory(DisplayName = "Run refuses paths, shell strings, reserved IDs and unknown tool names")]
    [InlineData("./fixture-tool", (int)ToolRunFailureCode.InvalidInput)]
    [InlineData("fixture-tool; echo unsafe", (int)ToolRunFailureCode.InvalidInput)]
    [InlineData("none", (int)ToolRunFailureCode.InvalidInput)]
    [InlineData("unknown-tool", (int)ToolRunFailureCode.ToolUndeclared)]
    public async Task RefusesUndeclaredInput(string id, int expected)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var before = fixture.Temporary.SnapshotHashes();
        var refusal = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request with { Id = id }, Token));
        Assert.Equal((ToolRunFailureCode)expected, refusal.Failure.Code);
        Assert.Equal(4, refusal.Failure.ExitCode);
        Assert.Equal(fixture.Workspace.PhysicalRoot, refusal.Failure.Workspace?.PhysicalRoot);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Run refuses even an absolute executable path inside its verified store")]
    public async Task RefusesAbsoluteExecutable()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request with { Id = fixture.EntryPath }, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.InvalidInput, failure.Code);
        Assert.Equal(4, failure.ExitCode);
    }

    [Theory(DisplayName = "Run blocks every missing or changed immutable member with exit five")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task RefusesChangedMember(int scenario)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        var member = fixture.Store.Installs.At(fixture.UnitPath + "/" + ToolRunFixture.DataMember);
        if (scenario == 0) File.Delete(member);
        else if (scenario == 1) await File.AppendAllTextAsync(member, "changed", Token);
        else File.SetLastWriteTimeUtc(member, File.GetLastWriteTimeUtc(member).AddMinutes(-1));
        var before = fixture.Temporary.SnapshotHashes();
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.LaunchIntegrityMismatch, failure.Code);
        Assert.Equal(5, failure.ExitCode);
        Assert.False(failure.OfferInstall);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Run blocks copied declarations without local installation and keeps the resolved workspace for consent")]
    public async Task RefusesMissingLocalInstall()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await fixture.RewriteRegistryAsync([], Token);
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.DeclaredToolMissing, failure.Code);
        Assert.True(failure.OfferInstall);
        Assert.Equal(5, failure.ExitCode);
        Assert.Equal(WorkspaceIdentity.Key(fixture.Workspace.PhysicalRoot), WorkspaceIdentity.Key(Assert.IsType<OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace>(failure.Workspace).PhysicalRoot));
    }

    [Theory(DisplayName = "Run requires matching settings, readable receipts and complete account verification")]
    [InlineData(0, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(1, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(2, (int)ToolRunFailureCode.VerificationUnavailable)]
    public async Task RefusesUnavailableEvidence(int scenario, int expected)
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        if (scenario == 0) fixture.WriteSettings(new() { Version = "2.0.0", Roles = [] });
        else if (scenario == 1) File.Delete(Path.Combine(fixture.Workspace.LexicalRoot, WorkspaceOwnershipDefinitions.RelativePath));
        else await File.WriteAllTextAsync(Path.Combine(fixture.Workspace.LexicalRoot, WorkspaceSettingsDefinitions.RelativePath), "{", Token);
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal((ToolRunFailureCode)expected, failure.Code);
        Assert.Equal(fixture.Workspace.PhysicalRoot, failure.Workspace?.PhysicalRoot);
    }

    [Fact(DisplayName = "Run returns exit 130 when cancelled before launch and creates no state")]
    public async Task CancelsBeforeLaunch()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var before = fixture.Temporary.SnapshotHashes();
        var failure = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, cancellation.Token)).Failure;
        Assert.Equal(ToolRunFailureCode.Cancelled, failure.Code);
        Assert.Equal(130, failure.ExitCode);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }
}
