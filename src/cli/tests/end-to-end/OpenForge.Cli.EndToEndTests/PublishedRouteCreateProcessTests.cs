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
            "--format=json",
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
        var result = document.RootElement.GetProperty("data");
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteCreateWorkspace.TargetPath, result.GetProperty("target").GetProperty("path").GetString());

        // The command data names the created file and the entrypoint that lists it. The complete
        // effect receipts stay on the envelope, not inside the command data.
        Assert.Equal(PublishedRouteCreateWorkspace.ParentPath, result.GetProperty("listedIn").GetString());
        Assert.Equal([PublishedRouteCreateWorkspace.TargetPath, PublishedRouteCreateWorkspace.ParentPath],
            document.RootElement.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("path").GetString()));
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
        // The headline states what was created and there is no `Status:` line.
        Assert.StartsWith($"Created {PublishedRouteCreateWorkspace.TargetPath}", application.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", application.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            $"Listed in {PublishedRouteCreateWorkspace.ParentPath}",
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
        // A no-op says so in the headline. A read-only result never prints "No files changed."
        Assert.StartsWith(
            $"{PublishedRouteCreateWorkspace.TargetPath} already has the requested content. Nothing to do.",
            noOp.StandardOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
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
                "--description=",
                "--tag=Docs",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        // The invalid-input family names the exact problem, and `Next` is the corrected command
        // rather than `--help`.
        Assert.StartsWith("Cannot create the routed file: ", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("--description is missing.", invalid.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route create ", invalid.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

}
