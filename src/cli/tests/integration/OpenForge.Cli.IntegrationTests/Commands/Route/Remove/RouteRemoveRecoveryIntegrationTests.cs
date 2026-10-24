using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveRecoveryIntegrationTests
{
    private static readonly byte[] DamagedBundleBytes = "not a recovery archive"u8.ToArray();

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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove warns about damaged history while applying the requested removal"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task DamagedHistoryDoesNotBlockApplication()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-recovery-collision");
        var recoveryDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        Directory.CreateDirectory(recoveryDirectory);
        var candidate = Path.Combine(
            recoveryDirectory,
            RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()));
        File.WriteAllBytes(candidate, DamagedBundleBytes);
        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var report = RouteRemoveReportSelector.Select(result, new CliSelection(CliDetail.Full, null));
        var finding = Assert.Single(report.Findings,
            finding => finding.Code == RouteRemoveDefinitions.RecoveryHistoryCode);
        Assert.Equal(candidate, finding.Subject.Path);
        CommandOutputRenderers<RouteRemoveResult>.From(RouteRemovePresentation.Rendering)
            .MatchDetails(result, "damaged-history", recoveryBundlePath: candidate);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.Equal(DamagedBundleBytes, File.ReadAllBytes(candidate));
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
