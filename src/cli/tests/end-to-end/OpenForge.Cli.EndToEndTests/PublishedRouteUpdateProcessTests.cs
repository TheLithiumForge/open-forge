using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport.Contracts;
using System.Text;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteUpdateProcessTests
{
    [Theory(DisplayName = "Published outside Update previews patches exact bytes and converges without relisting")]
    [InlineData(".agents", false), InlineData("knowledge/main", true)]
    [Trait("Feature", "route-update-outside"), Trait("Evidence", "EndToEnd")]
    public async Task OutsideExactPathJourney(string root, bool compatibility)
    {
        var executable = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var path = workspace.SeedOutside(root, compatibility);
        var before = workspace.SnapshotState();
        var original = workspace.ReadBytes(path);
        var parent = "docs/team/" + (compatibility ? PublishedTokens.Layout.IndexFile : "_team" + PublishedTokens.Layout.MarkdownExtension);
        var parentBefore = workspace.ReadBytes(parent);
        string[] arguments = ["route", PublishedTokens.RouteUpdate.Command, PublishedTokens.Layout.LiteralReferencePrefix + path,
            PublishedTokens.RouteUpdate.Description, PublishedRouteUpdateWorkspace.ExpectedDescription,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
        var preview = await RunWithoutWritesAsync(executable, workspace, [.. arguments, PublishedTokens.RouteCreate.DryRun]);
        Assert.True(preview.ExitCode == 0, preview.StandardOutput + preview.StandardError);
        Assert.Equal(string.Empty, preview.StandardError);
        workspace.AssertNoLockInfrastructure();
        using (var document = JsonDocument.Parse(preview.StandardOutput))
        {
            Assert.NotEmpty(document.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
            Assert.All(document.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray(),
                effect => Assert.StartsWith("docs/team/", effect.GetProperty(PublishedTokens.Json.Path).GetString(), StringComparison.Ordinal));
        }
        var applied = await PublishedProcessTestSupport.RunAsync(executable, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using (var document = JsonDocument.Parse(applied.StandardOutput))
        {
            Assert.Equal(PublishedTokens.Layout.LiteralReferencePrefix + path,
                document.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty("target").GetProperty(PublishedTokens.Json.Id).GetString());
            Assert.Equal(path, Assert.Single(document.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray())
                .GetProperty(PublishedTokens.Json.Path).GetString());
        }
        Assert.Equal(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(original).Replace("description: Before overview", "description: After overview", StringComparison.Ordinal)), workspace.ReadBytes(path));
        Assert.Equal(parentBefore, workspace.ReadBytes(parent));
        var after = workspace.SnapshotState();
        foreach (var row in before.Where(row => !row.Key.StartsWith("docs/team/", StringComparison.Ordinal))) Assert.Equal(row.Value, after[row.Key]);
        Assert.All(after.Keys.Except(before.Keys, StringComparer.Ordinal), key => Assert.StartsWith("docs/team/", key, StringComparison.Ordinal));
        var repeated = await RunWithoutWritesAsync(executable, workspace, arguments);
        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var noOp = JsonDocument.Parse(repeated.StandardOutput);
        Assert.Empty(noOp.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Update JSON dry-run previews an update without writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
            "--dry-run",
            "--format=json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteUpdateWorkspace.TargetPath, result.GetProperty("target").GetProperty("path").GetString());
        Assert.NotEmpty(result.GetProperty("changes").EnumerateArray());
        Assert.NotEmpty(document.RootElement.GetProperty("effects").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Update applies exact base and navigation bytes then converges"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyThenNoOpIsOneExactJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var targetBefore = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentBefore = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
        ];

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains($"Updated {PublishedRouteUpdateWorkspace.TargetId}", applied.StandardOutput, StringComparison.Ordinal);
        var targetAfter = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentAfter = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        Assert.Equal(
            targetBefore.Replace("Before overview", "After overview", StringComparison.Ordinal)
                .Replace("[Before, Memory]", "[After, Memory]", StringComparison.Ordinal),
            targetAfter);
        Assert.Equal(
            parentBefore.Replace(
                "- [Before overview](overview.md) - #Before #Memory",
                "- [After overview](overview.md) - #After #Memory",
                StringComparison.Ordinal),
            parentAfter);
        workspace.AssertPersistentExternalLock();
        var after = workspace.SnapshotState();

        var noOp = await RunWithoutWritesAsync(target, workspace, arguments);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains($"{PublishedRouteUpdateWorkspace.TargetId} already has these values. Nothing to do.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(after, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published Route Update repeated apply-to options regenerate and then clear Entries")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "EndToEnd")]
    public async Task ApplyToOptionsRefreshAndClearGeneratedEntry()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        string[] setArguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--apply-to", "**/*.cs",
            "--apply-to", "docs/*.md",
            "--format=json",
        ];

        var set = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            setArguments,
            workspace.ProcessEnvironment);
        Assert.True(
            set.ExitCode == 0,
            $"Published applyTo update failed. stdout: {set.StandardOutput} stderr: {set.StandardError}");
        Assert.Equal(string.Empty, set.StandardError);
        using (var setDocument = JsonDocument.Parse(set.StandardOutput))
        {
            var result = setDocument.RootElement.GetProperty("data");
            var change = Assert.Single(result.GetProperty("changes").EnumerateArray());
            Assert.Equal("applyTo", change.GetProperty("field").GetString());
            Assert.Equal(
                new[] { "**/*.cs", "docs/*.md" },
                change.GetProperty("afterValues").EnumerateArray()
                    .Select(value => value.GetString()));
            Assert.Equal(
                1,
                setDocument.RootElement.GetProperty("counts")
                    .GetProperty("fieldsChanged").GetInt32());
        }
        Assert.Contains(
            "applyTo: [\"**/*.cs\", \"docs/*.md\"]",
            await workspace.ReadTargetAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
        Assert.Contains(
            "- [Before overview](overview.md) - #Before #Memory - applies to `**/*.cs`, `docs/*.md`",
            await workspace.ReadParentAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);

        var cleared = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--clear-apply-to", "--format=json"],
            workspace.ProcessEnvironment);
        Assert.Equal(0, cleared.ExitCode);
        Assert.Equal(string.Empty, cleared.StandardError);
        using (var clearDocument = JsonDocument.Parse(cleared.StandardOutput))
        {
            var change = Assert.Single(clearDocument.RootElement.GetProperty("data")
                .GetProperty("changes").EnumerateArray());
            Assert.Equal("applyTo", change.GetProperty("field").GetString());
            Assert.Equal(
                new[] { "**/*.cs", "docs/*.md" },
                change.GetProperty("beforeValues").EnumerateArray()
                    .Select(value => value.GetString()));
            Assert.False(change.TryGetProperty("afterValues", out _));
        }
        Assert.DoesNotContain(
            "applyTo:",
            await workspace.ReadTargetAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
        Assert.Contains(
            "- [Before overview](overview.md) - #Before #Memory",
            await workspace.ReadParentAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);

        var afterClear = workspace.SnapshotState();
        var clearNoOp = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--clear-apply-to"]);
        Assert.Equal(0, clearNoOp.ExitCode);
        Assert.Equal(afterClear, workspace.SnapshotState());
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Update comma-separated apply-to replaces the list and refreshes Entries"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task CommaSeparatedApplyToReplacesListAndRefreshesGeneratedEntry()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var initial = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--apply-to", "**/*.cs",
                "--apply-to", "docs/*.md",
            ],
            workspace.ProcessEnvironment);
        Assert.Equal(0, initial.ExitCode);
        Assert.Equal(string.Empty, initial.StandardError);

        var replacement = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--apply-to", "src/**/*.cs,docs/*.md",
                "--format=json",
            ],
            workspace.ProcessEnvironment);
        Assert.Equal(0, replacement.ExitCode);
        Assert.Equal(string.Empty, replacement.StandardError);
        using var document = JsonDocument.Parse(replacement.StandardOutput);
        var change = Assert.Single(document.RootElement.GetProperty("data")
            .GetProperty("changes").EnumerateArray());
        Assert.Equal("applyTo", change.GetProperty("field").GetString());
        Assert.Equal(
            new[] { "**/*.cs", "docs/*.md" },
            change.GetProperty("beforeValues").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.Equal(
            new[] { "src/**/*.cs", "docs/*.md" },
            change.GetProperty("afterValues").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.Contains(
            "applyTo: [\"src/**/*.cs\", \"docs/*.md\"]",
            await workspace.ReadTargetAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
        var parent = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        Assert.Contains(
            "- [Before overview](overview.md) - #Before #Memory - applies to `docs/*.md`, `src/**/*.cs`",
            parent,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "- [Before overview](overview.md) - #Before #Memory - applies to `**/*.cs`, `docs/*.md`",
            parent,
            StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Update refuses an ambiguous target without writes"), Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task AmbiguousTargetRefusesWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        workspace.SeedAmbiguousTarget();
        var result = await RunWithoutWritesAsync(target, workspace,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--description", PublishedRouteUpdateWorkspace.ExpectedDescription, "--dry-run"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains($"Cannot update {PublishedRouteUpdateWorkspace.TargetId}:", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Theory(DisplayName = "Published Route Update creates the workspace form edits authored metadata and converges"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    [InlineData("root", true)]
    [InlineData("scoped", false)]
    [InlineData(null, false)]
    public async Task WorkspaceFormCreationAndAuthoredLocationConverge(string? form, bool root)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        const string body = "# Exact body 🙂\n";
        workspace.SeedTargetText(body);
        if (form is not null)
        {
            workspace.SeedSettingsText($"{{\"schemaVersion\":1,\"frontmatter\":\"{form}\"}}");
        }
        string[] arguments = ["route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", "After overview", "--tag", "Memory", "--format", "json"];
        var preview = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--dry-run"]);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using (var document = JsonDocument.Parse(preview.StandardOutput))
        {
            Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
            Assert.NotEmpty(document.RootElement.GetProperty("effects").EnumerateArray());
        }
        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        var members = root ? "description: After overview\ntags: [Memory]\n"
            : "open-forge:\n  description: After overview\n  tags: [Memory]\n";
        Assert.Equal($"---\n{members}---\n{body}", await workspace.ReadTargetAsync(TestContext.Current.CancellationToken));
        workspace.SeedSettingsText($"{{\"schemaVersion\":1,\"frontmatter\":\"{(root ? "scoped" : "root")}\"}}");
        var edited = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--description", "Final overview", "--format", "json"], workspace.ProcessEnvironment);
        Assert.Equal(0, edited.ExitCode);
        Assert.Equal(string.Empty, edited.StandardError);
        Assert.Equal($"---\n{members.Replace("After overview", "Final overview", StringComparison.Ordinal)}---\n{body}",
            await workspace.ReadTargetAsync(TestContext.Current.CancellationToken));
        var noOp = await RunWithoutWritesAsync(target, workspace,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--description", "Final overview", "--format", "json"]);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var repeated = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(repeated.RootElement.GetProperty("effects").EnumerateArray());
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteUpdateWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
