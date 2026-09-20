using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveRecoveryIntegrationTests
{
    [Trait("Boundary", "Host")]
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
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("recovery").ValueKind);
        Assert.Equal(before, SnapshotRecovery(recoveryDirectory));
    }

    [Trait("Boundary", "Host")]
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
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic", "--detail", "full"],
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

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove application reports an honest removed recovery state after verified effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ApplicationReportsRemovedRecoveryState()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-recovery-removed");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").ValueKind);
        Assert.Contains(
            root.GetProperty("data").GetProperty("removed").EnumerateArray(),
            path => path.GetString() == RouteRemoveIntegrationWorkspace.LeafPath);
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
