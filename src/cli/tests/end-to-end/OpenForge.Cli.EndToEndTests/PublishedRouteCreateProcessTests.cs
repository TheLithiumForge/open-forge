using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteCreateProcessTests
{
    [Fact(DisplayName = "Published Route Create JSON dry-run previews the two affected paths without writes"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        string[] arguments =
        [
            "route", "create", PublishedRouteCreateWorkspace.TargetId,
            "--description", PublishedRouteCreateWorkspace.Description,
            "--tag=Docs",
            "--tag=Overview",
            "--responsibility", PublishedRouteCreateWorkspace.Responsibility,
            "--dry-run",
            "--json",
        ];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteCreateWorkspace.TargetPath, result.GetProperty("target").GetProperty("path").GetString());
        Assert.Equal([PublishedRouteCreateWorkspace.TargetPath, PublishedRouteCreateWorkspace.ParentPath],
            result.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("path").GetString()));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Create applies exact bytes then repeats as a verified no-op"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        workspace.OwnApplicationCreatedTarget();
        var before = workspace.SnapshotState();
        string[] arguments =
        [
            "route", "create", PublishedRouteCreateWorkspace.TargetId,
            "--description", PublishedRouteCreateWorkspace.Description,
            "--tag=Docs",
            "--tag=Overview",
            "--responsibility", PublishedRouteCreateWorkspace.Responsibility,
        ];

        var application = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, application.ExitCode);
        Assert.Equal(string.Empty, application.StandardError);
        Assert.Contains("The routed file was created.", application.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", application.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            $"{PublishedRouteCreateWorkspace.TargetPath}: create routed-file / verified",
            application.StandardOutput,
            StringComparison.Ordinal);
        workspace.AssertOrdinaryTargetFile();
        Assert.Equal(
            Encoding.UTF8.GetBytes(PublishedRouteCreateWorkspace.ExpectedTargetDocument),
            await workspace.ReadTargetBytesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            Encoding.UTF8.GetBytes(workspace.ExpectedParentDocument),
            await workspace.ReadParentBytesAsync(TestContext.Current.CancellationToken));
        var after = workspace.SnapshotState();
        Assert.Equal(before.Count + 1, after.Count);
        Assert.Equal(before[".agents/loader.md"], after[".agents/loader.md"]);
        workspace.AssertPersistentExternalLock();

        var noOp = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains(
            "The routed file already matches the requested content.",
            noOp.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Status: complete", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Create invalid input uses invalid stderr exit without writes"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidMetadataUsesSharedStatusStreamAndExitWithoutWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        var invalid = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [
                "route", "create", PublishedRouteCreateWorkspace.TargetId,
                "--tag=Docs",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("Route Create could not start because the input is invalid.", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "requires one nonblank --description value",
            invalid.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route create --help", invalid.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

}
