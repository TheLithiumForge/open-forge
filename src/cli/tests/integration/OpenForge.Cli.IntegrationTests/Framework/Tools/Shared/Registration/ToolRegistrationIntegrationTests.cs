using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Integration")]
public sealed class ToolRegistrationIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Real installer-shaped files produce checked rewrites and passive receipts without planner writes")]
    [InlineData(false), InlineData(true)]
    public async Task PlansAndVerifiesFixtureFiles(bool decline)
    {
        using var fixture = new ToolRegistrationFixture();
        fixture.Seed();
        var request = fixture.Request with { DeclinedComponents = decline ? fixture.Definition.Components : [] };
        var before = fixture.Temporary.SnapshotHashes();
        var planner = new ToolEntryPlanner();
        var plan = await planner.Services.Plan(request, Token);
        Assert.Equal(4, plan.Edits.Length);
        Assert.All(plan.Checks, check => Assert.Equal(ToolEntryState.Original, check.State));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        await fixture.ApplyAsync(request, plan, Token);
        var verifier = new ToolEntryVerifier();
        var verification = await verifier.VerifyAsync(request, Token);
        Assert.Equal(decline ? 3 : 4, verification.Rewrites.Length);
        Assert.Equal(decline ? 1 : 0, verification.DeclinedComponents.Length);
        var written = fixture.Temporary.SnapshotHashes();
        var observed = await verifier.ObserveReceiptsAsync(fixture.Temporary.Path, verification.Rewrites, verification.DeclinedComponents, Token);
        Assert.All(observed, check => Assert.True(check.State is ToolEntryState.Matching or ToolEntryState.Declined));
        Assert.Equal(written, fixture.Temporary.SnapshotHashes());
        Assert.Empty((await planner.Services.Plan(request, Token)).Edits);
    }

    [Theory(DisplayName = "Whole-file rechecks reject concurrent edits to unrelated bytes before each rewrite")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public async Task RejectsStaleFile(int form)
    {
        using var fixture = new ToolRegistrationFixture();
        fixture.Seed();
        var rewrite = fixture.Definition.Rewrites[form];
        var request = fixture.Request with { Rewrites = [rewrite] };
        var edit = Assert.Single((await new ToolEntryPlanner().PlanAsync(request, Token)).Edits);
        var concurrent = edit.Before.Concat(Encoding.UTF8.GetBytes("\n ")).ToArray();
        fixture.Temporary.ReplaceBytes(edit.File, concurrent);
        var checks = await new ToolEntryVerifier().RecheckAsync(request, edit, Token);
        Assert.All(checks, check => Assert.Equal(ToolEntryState.Drifted, check.State));
        Assert.Equal(concurrent, await File.ReadAllBytesAsync(fixture.Temporary.Combine(edit.File), Token));
    }

    [Theory(DisplayName = "Receipt observations report each rewrite drift while tolerating unrelated authored edits")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public async Task ObservesSelectedPartsOnly(int form)
    {
        using var fixture = new ToolRegistrationFixture();
        fixture.Seed();
        var rewrite = fixture.Definition.Rewrites[form];
        var request = fixture.Request with { Rewrites = [rewrite] };
        var plan = await new ToolEntryPlanner().PlanAsync(request, Token);
        await fixture.ApplyAsync(request, plan, Token);
        var verifier = new ToolEntryVerifier();
        var verified = await verifier.VerifyAsync(request, Token);
        Assert.Single(verified.Rewrites);
        var edit = Assert.Single(plan.Edits);
        if (rewrite is ToolJsonCommandRewrite or ToolHookCommandRewrite)
        {
            var root = JsonNode.Parse(edit.After)?.AsObject() ?? throw new InvalidDataException();
            root["fixture-concurrent"] = true;
            fixture.Temporary.ReplaceText(edit.File, root.ToJsonString());
        }
        else fixture.Temporary.ReplaceBytes(edit.File, edit.After.Concat(Encoding.UTF8.GetBytes("\n# Authored addition\n")).ToArray());
        Assert.Equal(ToolEntryState.Matching, Assert.Single(await verifier.ObserveReceiptsAsync(fixture.Temporary.Path, verified.Rewrites, [], Token)).State);
        fixture.Temporary.ReplaceBytes(edit.File, edit.Before);
        var before = fixture.Temporary.SnapshotHashes();
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(await verifier.ObserveReceiptsAsync(fixture.Temporary.Path, verified.Rewrites, [], Token)).State);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        Assert.Empty((await verifier.VerifyAsync(request, Token)).Rewrites);
    }

    [Fact(DisplayName = "Restoring a declined hook is passive drift and never recreates or removes any hook")]
    public async Task ObservesDeclineDrift()
    {
        using var fixture = new ToolRegistrationFixture();
        fixture.Seed();
        var hook = fixture.Definition.Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var request = fixture.Request with { Rewrites = [hook], DeclinedComponents = fixture.Definition.Components };
        var plan = await new ToolEntryPlanner().PlanAsync(request, Token);
        await fixture.ApplyAsync(request, plan, Token);
        var verifier = new ToolEntryVerifier();
        var receipt = Assert.Single((await verifier.VerifyAsync(request, Token)).DeclinedComponents);
        fixture.Temporary.ReplaceBytes(hook.File, Assert.Single(plan.Edits).Before);
        var before = fixture.Temporary.SnapshotHashes();
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(await verifier.ObserveReceiptsAsync(fixture.Temporary.Path, [], [receipt], Token)).State);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Linked files and linked ancestors are unavailable without following their targets")]
    [InlineData(false), InlineData(true)]
    public async Task RefusesLinkedEntries(bool ancestor)
    {
        using var fixture = new ToolRegistrationFixture();
        using var outside = TemporaryWorkspace.Create("registration-link-target");
        var rewrite = fixture.Definition.Rewrites.OfType<ToolJsonCommandRewrite>().Single();
        outside.WriteBytes(rewrite.File, ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.Mcp));
        if (ancestor)
        {
            const string link = "fixture-link";
            Assert.True(fixture.Temporary.TryCreateDirectorySymbolicLink(link, outside.Path, out _));
            rewrite = rewrite with { File = link + "/" + rewrite.File };
        }
        else Assert.True(fixture.Temporary.TryCreateFileSymbolicLink(rewrite.File, outside.Combine(rewrite.File), out _));
        var request = fixture.Request with { Rewrites = [rewrite] };
        var before = outside.SnapshotHashes();
        var plan = await new ToolEntryPlanner().PlanAsync(request, Token);
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Unavailable, Assert.Single(plan.Checks).State);
        Assert.Equal(before, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Role guidance switches and clears both real agent files while keeping authored content and tool blocks")]
    public async Task KeepsBothAgentFiles()
    {
        using var fixture = new ToolRegistrationFixture();
        var block = $"# {MarkdownManagedHostReader.HeadingText}\n\nFixture managed body.\n\n**{MarkdownManagedHostReader.FooterText}**\n";
        var before = Encoding.UTF8.GetBytes(block).Concat(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.Agent)).ToArray();
        fixture.Temporary.WriteBytes(WorkspaceLayoutNames.AgentHostFileName, before);
        fixture.Temporary.WriteBytes(WorkspaceLayoutNames.ClaudeHostFileName, before);
        var request = fixture.RoleRequest();
        var planner = new ToolRoleGuidancePlanner();
        var original = fixture.Temporary.SnapshotHashes();
        var plan = await planner.Services.Plan(request, Token);
        Assert.Equal(original, fixture.Temporary.SnapshotHashes());
        Assert.Equal(2, plan.Edits.Length);
        foreach (var edit in plan.Edits)
        {
            Assert.True(await planner.RecheckAsync(request, edit, Token));
            fixture.Temporary.ReplaceBytes(edit.File, edit.After);
        }
        Assert.All(await planner.Services.Observe(request, Token), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        var second = request.Active[0] with { ToolId = ToolFixtureSource.Id + "-second", Guidance = request.Active[0].Guidance with { Line = "Use the second fixture provider." } };
        request = request with { Active = [second] };
        foreach (var edit in (await planner.PlanAsync(request, Token)).Edits) fixture.Temporary.ReplaceBytes(edit.File, edit.After);
        Assert.All(await planner.ObserveAsync(request, Token), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        request = request with { Active = [] };
        foreach (var edit in (await planner.PlanAsync(request, Token)).Edits) fixture.Temporary.ReplaceBytes(edit.File, edit.After);
        Assert.Equal(original, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Missing role files keep expected absence and reject a file created after planning")]
    public async Task ChecksMissingRoleFile()
    {
        using var fixture = new ToolRegistrationFixture();
        var request = fixture.RoleRequest("context/team");
        var planner = new ToolRoleGuidancePlanner();
        var before = fixture.Temporary.SnapshotHashes();
        var plan = await planner.PlanAsync(request, Token);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        Assert.Equal(2, plan.Edits.Length);
        foreach (var edit in plan.Edits)
        {
            Assert.Null(edit.Before);
            Assert.True(await planner.RecheckAsync(request, edit, Token));
            fixture.Temporary.WriteText(edit.File, "# Concurrent authored file\n");
            Assert.False(await planner.RecheckAsync(request, edit, Token));
            fixture.Temporary.ReplaceBytes(edit.File, edit.After);
            Assert.Equal(MarkdownManagedHostState.Present, new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(edit.After)).ManagedHost.State);
        }
        var written = fixture.Temporary.SnapshotHashes();
        Assert.All(await planner.ObserveAsync(request, Token), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        Assert.Equal(written, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Cancelled registration planning stops without writing files")]
    public async Task CancellationIsReadOnly()
    {
        using var fixture = new ToolRegistrationFixture();
        fixture.Seed();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        cancellation.Cancel();
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await new ToolEntryPlanner().PlanAsync(fixture.Request, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await new ToolRoleGuidancePlanner().ObserveAsync(fixture.RoleRequest(), cancellation.Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }
}
