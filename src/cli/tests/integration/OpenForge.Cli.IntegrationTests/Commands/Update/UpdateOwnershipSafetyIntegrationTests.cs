using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update"), Trait("Evidence", "IntegrationSafety")]
public sealed class UpdateOwnershipSafetyIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact]
    public async Task UnwritableOwnershipDoesNotInvalidateVerifiedContentEffects()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-unwritable-ownership");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.RemoveManagedContent();
        workspace.RemoveFile(UpdateIntegrationWorkspace.OwnershipPath);
        workspace.CreateDirectory(UpdateIntegrationWorkspace.OwnershipPath);
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join("; ", result.Findings.Select(finding => $"{finding.Code}: {finding.Target}: {finding.Cause}")));
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.OwnershipObservation);
        Assert.True(File.Exists(Path.Combine(workspace.Workspace.LexicalRoot, UpdateIntegrationWorkspace.ManagedPath)));
        Assert.True(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, UpdateIntegrationWorkspace.OwnershipPath)));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task MissingRequiredParentStopsTheWholePlanBeforeWrites()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-missing-parents");
        workspace.CreateDirectory(".agents");
        Assert.False(workspace.Exists(".agents/open-forge.json"));
        Assert.False(workspace.Exists(UpdateIntegrationWorkspace.OwnershipPath));
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.TargetUnsafe);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("outside.txt"), InlineData(".agents/open-forge.json"), InlineData(".agents/open-forge.lock.json"), InlineData(".git/config"), InlineData(".agents/loader.overwrite.md")]
    public async Task InvalidStaleClaimDoesNotAuthorizeAnyDeletion(string target)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-stale-boundary");
        if (target == ".git/config") workspace.CreateDirectory(".git");
        var overwrite = target.EndsWith(".overwrite.md", StringComparison.Ordinal);
        if (target is not ".agents/open-forge.lock.json" && !overwrite)
        {
            workspace.WriteText(
                target,
                target == ".agents/open-forge.json"
                    ? "{\"note\":\"user content\"}\n"
                    : "user content\n");
        }
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        if (overwrite)
        {
            using var stream = new FileStream(Path.Combine(workspace.Workspace.LexicalRoot, target), FileMode.CreateNew);
            using var writer = new StreamWriter(stream);
            writer.Write("user content\n");
        }
        try
        {
            var document = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
            document["framework"]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create(target));
            workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, document.ToJsonString());
            var before = workspace.SnapshotHashes();
            var result = await workspace.ExecuteAsync(workspace.Request(prune: true));
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Empty(result.Effects);
            Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.OwnershipObservation);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        finally
        {
            if (overwrite) workspace.RemoveFile(target);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task SharedAllowListAdmitsRetirementAndMissingPathNeverGetsADeleteEffect(bool missing)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-shared-allow-list");
        const string target = "outside.txt";
        workspace.WriteText(".agents/open-forge.json", "{\"allowInstallPaths\":[\"outside.txt\"]}");
        if (!missing) workspace.WriteText(target, "uncommitted user content\n");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var document = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        document["framework"]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create(target));
        workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, document.ToJsonString());
        var result = await workspace.ExecuteAsync(workspace.Request(prune: true));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(!missing, result.Effects.Any(effect => effect.Path == target && effect.Action == UpdatePhysicalEffectAction.Delete));
        Assert.False(File.Exists(Path.Combine(workspace.Workspace.LexicalRoot, target)));
        var after = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!;
        Assert.DoesNotContain(after["framework"]!["paths"]!.AsArray(), node => node!.GetValue<string>() == target);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task RemovedCategoryStaysRemovedWhileAnotherOwnedFileUpdates(bool hasGit)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-removed-category");
        workspace.WriteText(".agents/open-forge.json", "{}");
        if (hasGit) workspace.CreateDirectory(".git");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        const string removed = ".agents/skills/_skills.md";
        workspace.RemoveFile(removed);
        const string settings = "{\"removedCategories\":[\"skills\"],\"foreign\":true}";
        workspace.ReplaceText(".agents/open-forge.json", settings);
        workspace.MutateManagedContent();
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Effects, effect => effect.Path.StartsWith(".agents/skills/", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(workspace.Workspace.LexicalRoot, removed)));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        Assert.NotNull(result.Next);
        Assert.Contains(result.Recovery.ResidualPath!, result.Next.Reason);
        Assert.Equal(hasGit ? "git diff" : "open-forge doctor", result.Next.Command);
    }
}
