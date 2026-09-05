using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveRecoveryIntegrationTests
{
    [Fact(DisplayName = "Route Remove dry-run does not create or alter an external recovery bundle"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task DryRunLeavesRecoveryBoundaryUntouched()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-recovery-dry-run");
        var recoveryDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        var before = SnapshotRecovery(recoveryDirectory);
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(
            "not-created",
            document.RootElement.GetProperty("result")
                .GetProperty("recovery")
                .GetProperty("state")
                .GetString());
        Assert.Equal(before, SnapshotRecovery(recoveryDirectory));
    }

    [Fact(DisplayName = "Route Remove refuses a pre-existing recovery collision before any workspace effect"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task RecoveryCollisionBlocksBeforeApplication()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-recovery-collision");
        var recoveryDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        Directory.CreateDirectory(recoveryDirectory);
        var candidate = Path.Combine(
            recoveryDirectory,
            RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()));
        File.WriteAllBytes(candidate, "not a recovery archive"u8.ToArray());
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains(
            "route-remove.recovery-conflict",
            error.ToString(),
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.True(File.Exists(candidate));
    }

    [Fact(DisplayName = "Route Remove application reports an honest removed recovery state after verified effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ApplicationReportsRemovedRecoveryState()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-recovery-removed");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("verified", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("recovery").GetProperty("protectedPaths").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("recovery").GetProperty("residualPath").ValueKind);
    }

    private static IReadOnlyDictionary<string, string> SnapshotRecovery(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["directory"] = "absent",
            };
        }

        return Directory.EnumerateFileSystemEntries(directory)
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                path => Path.GetFileName(path),
                PathState,
                StringComparer.Ordinal);
    }

    private static string PathState(string path)
    {
        if (File.Exists(path))
        {
            return $"file:{Convert.ToBase64String(File.ReadAllBytes(path))}";
        }

        return Directory.Exists(path) ? "directory" : "absent";
    }
}
