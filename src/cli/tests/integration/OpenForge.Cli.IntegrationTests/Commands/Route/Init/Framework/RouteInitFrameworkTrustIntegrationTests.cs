using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

[Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
public sealed class RouteInitFrameworkTrustIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework Route Init requires an actual installed root route")]
    public async Task MissingInstalledRootIsRefused()
    {
        using var workspace = RouteInitFrameworkIntegrationWorkspace.CreateEmpty("route-init-framework-no-install");
        var before = workspace.SnapshotHashes();
        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.FrameworkInstallRequired);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Framework Route Init uses real route boundaries and ignores missing stale or unreadable ownership metadata")]
    [InlineData("missing"), InlineData("malformed"), InlineData("directory")]
    [InlineData("framework-null"), InlineData("extensions-null"), InlineData("empty")]
    [InlineData("older-source"), InlineData("edited-loader")]
    public async Task LockMetadataDoesNotGateSafeScopedCreation(string condition)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-lock-boundary", TestContext.Current.CancellationToken);
        const string leftover = "{ obsolete unrelated bytes }";
        workspace.WriteText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath, leftover);
        workspace.WriteText(".agents/open-forge.libraries.json", leftover);
        var lockPath = RouteInitFrameworkIntegrationWorkspace.OwnershipPath;
        if (condition is "missing" or "directory")
        {
            workspace.Delete(lockPath);
            if (condition == "directory")
            {
                Directory.CreateDirectory(workspace.Combine(lockPath));
            }
        }
        else if (condition is "malformed" or "empty")
        {
            workspace.WriteText(lockPath, condition == "malformed" ? "{" : "{}");
        }
        else if (condition == "edited-loader")
        {
            workspace.WriteText(RouteInitFrameworkIntegrationWorkspace.LoaderPath,
                workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LoaderPath).Replace("## Entries", "Authored workspace note.\n\n## Entries", StringComparison.Ordinal));
        }
        else
        {
            var document = JsonNode.Parse(workspace.ReadText(lockPath))!.AsObject();
            if (condition == "older-source")
            {
                document["framework"]!["source"]!["id"] = "older-framework";
                document["framework"]!["source"]!["version"] = "0.0.1";
            }
            else
            {
                document[condition == "framework-null" ? "framework" : "extensions"] = null;
            }
            workspace.WriteText(lockPath, document.ToJsonString());
        }
        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request("memory/release-notes/working"), TestContext.Current.CancellationToken);
        Assert.True(result.Status == CliSemanticStatus.Complete,
            string.Join("; ", result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(condition == "directory" ? RouteInitLifecycleOutcome.NotRequested : RouteInitLifecycleOutcome.Verified,
            result.Lifecycle.Outcome);
        Assert.True(workspace.Exists(".agents/memory/release-notes/working/_working.md"));
        Assert.Equal(leftover, workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath));
        Assert.Equal(leftover, workspace.ReadText(".agents/open-forge.libraries.json"));
        if (condition == "directory")
        {
            Assert.True(Directory.Exists(workspace.Combine(lockPath)));
        }
        else
        {
            var document = JsonNode.Parse(workspace.ReadText(lockPath))!.AsObject();
            var paths = document["framework"]!["paths"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
            Assert.Contains(".agents/memory/release-notes/working/_working.md", paths);
            Assert.DoesNotContain(".agents/memory/release-notes/_release-notes.md", paths);
        }
    }
}
