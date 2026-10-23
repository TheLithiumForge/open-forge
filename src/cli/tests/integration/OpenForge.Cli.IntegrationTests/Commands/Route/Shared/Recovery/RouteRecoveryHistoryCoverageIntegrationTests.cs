using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Recovery;

[Trait("Boundary", "OS"), Trait("Feature", "route-recovery-history"), Trait("Evidence", "Integration")]
public sealed class RouteRecoveryHistoryCoverageIntegrationTests
{
    [Theory(DisplayName = "Route commands warn about every retained path across both recovery stores")]
    [InlineData("include"), InlineData("exclude"), InlineData("create"), InlineData("init")]
    [InlineData("move"), InlineData("update"), InlineData("remove")]
    public async Task AllObservedPathsRemainDistinct(string command)
    {
        using var files = SeedWorkspace(command == "exclude");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var local = RecoveryStoreLocator.Local(workspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(local.WorkspaceDirectory);
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var identity = Guid.NewGuid();
        string[] paths =
        [
            Path.Combine(local.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid())),
            Path.Combine(local.WorkspaceDirectory, RecoveryBundleFormatV1.FinalFileName(identity)),
            Path.Combine(legacy.WorkspaceDirectory, RecoveryBundleFormatV1.FinalFileName(identity)),
        ];
        foreach (var path in paths) await File.WriteAllTextAsync(path, "Retained historical bytes", TestContext.Current.CancellationToken);
        try
        {
            var result = await CliHostCapture.RunAsync(Arguments(command), files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warnings = json.RootElement.GetProperty("findings").EnumerateArray()
                .Where(finding => finding.GetProperty("code").GetString() == $"route-{command}.recovery-history").ToArray();
            Assert.Equal(paths.Order(StringComparer.Ordinal),
                warnings.Select(finding => finding.GetProperty("subject").GetProperty("path").GetString()).Order(StringComparer.Ordinal));
            Assert.All(warnings, finding => Assert.Equal("warning", finding.GetProperty("severity").GetString()));
            Assert.All(paths, path => Assert.Equal("Retained historical bytes", File.ReadAllText(path)));
            AssertApplied(command, files, json.RootElement);
        }
        finally
        {
            foreach (var path in paths) File.Delete(path);
            RemoveCreatedTargets(files);
        }
    }

    [Theory(DisplayName = "Route commands proceed when historical legacy coverage is unavailable")]
    [InlineData("include"), InlineData("exclude"), InlineData("create"), InlineData("init")]
    [InlineData("move"), InlineData("update"), InlineData("remove")]
    public async Task UnavailableLegacyCoverageWarnsWithoutBlocking(string command)
    {
        using var files = SeedWorkspace(command == "exclude");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(legacy.RootPath);
        await File.WriteAllTextAsync(legacy.WorkspaceDirectory, "Unavailable historical directory", TestContext.Current.CancellationToken);
        try
        {
            var result = await CliHostCapture.RunAsync(Arguments(command), files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warning = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString() == $"route-{command}.recovery-coverage-unavailable");
            Assert.Equal("warning", warning.GetProperty("severity").GetString());
            Assert.Equal(legacy.WorkspaceDirectory, warning.GetProperty("subject").GetProperty("path").GetString());
            Assert.Equal("Unavailable historical directory", await File.ReadAllTextAsync(legacy.WorkspaceDirectory, TestContext.Current.CancellationToken));
            AssertApplied(command, files, json.RootElement);
        }
        finally
        {
            File.Delete(legacy.WorkspaceDirectory);
            RemoveCreatedTargets(files);
        }
    }

    [Theory(DisplayName = "Successful Route no-ops retain legacy history warnings without creating runtime state")]
    [InlineData("include"), InlineData("exclude"), InlineData("init"), InlineData("update")]
    public async Task NoOpWarnsWithoutRuntimeCreation(string command)
    {
        using var files = SeedWorkspace(command == "include");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var path = Path.Combine(legacy.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()));
        await File.WriteAllTextAsync(path, "Retained legacy draft", TestContext.Current.CancellationToken);
        string[] arguments = command switch
        {
            "include" => ["route", "include", "guide.md", "--description", "Guide", "--format", "json"],
            "exclude" => ["route", "exclude", "guide.md", "--format", "json"],
            "init" => ["route", "init", "docs", "--format", "json"],
            "update" => ["route", "update", "docs/leaf", "--description", "Leaf", "--format", "json"],
            _ => throw new ArgumentOutOfRangeException(nameof(command)),
        };
        var before = files.SnapshotHashes();
        try
        {
            var result = await CliHostCapture.RunAsync(arguments, files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warning = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString() == $"route-{command}.recovery-history");
            Assert.Equal(path, warning.GetProperty("subject").GetProperty("path").GetString());
            Assert.Equal("warning", warning.GetProperty("severity").GetString());
            Assert.False(Directory.Exists(files.Combine(".open-forge-state")));
            Assert.Equal(before, files.SnapshotHashes());
            Assert.Equal("Retained legacy draft", await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void AssertApplied(string command, TemporaryWorkspace files, JsonElement report)
    {
        if (command == "move")
        {
            Assert.False(File.Exists(files.Combine(".agents/docs/leaf.md")));
            Assert.True(File.Exists(files.Combine(".agents/docs/moved.md")));
        }
        else
        {
            Assert.NotEmpty(report.GetProperty("effects").EnumerateArray());
        }
    }

    private static void RemoveCreatedTargets(TemporaryWorkspace files)
    {
        File.Delete(files.Combine(".agents/docs/new.md"));
        File.Delete(files.Combine(".agents/docs/moved.md"));
        if (Directory.Exists(files.Combine(".agents/docs/new")))
        {
            File.Delete(files.Combine(".agents/docs/new/_new.md"));
            Directory.Delete(files.Combine(".agents/docs/new"));
        }
    }

    private static string[] Arguments(string command) => command switch
    {
        "include" => ["route", "include", "guide.md", "--description", "Guide", "--format", "json"],
        "exclude" => ["route", "exclude", "guide.md", "--format", "json"],
        "create" => ["route", "create", "docs/new", "--description", "New", "--tag", "Docs", "--format", "json"],
        "init" => ["route", "init", "docs/new", "--description", "New", "--tag", "Docs", "--format", "json"],
        "move" => ["route", "move", "docs/leaf", "docs/moved", "--format", "json"],
        "update" => ["route", "update", "docs/leaf", "--description", "Changed", "--format", "json"],
        "remove" => ["route", "remove", "docs/leaf", "--automatic", "--format", "json"],
        _ => throw new ArgumentOutOfRangeException(nameof(command)),
    };

    private static TemporaryWorkspace SeedWorkspace(bool included)
    {
        var files = TemporaryWorkspace.Create("route-history-coverage");
        var guideRow = included ? "\n- [Guide](../guide.md)" : string.Empty;
        files.CreateFile(".agents/loader.md", $"# Loader\n\n## Entries\n\n- [Docs](docs/_docs.md) - #Docs{guideRow}\n");
        files.CreateFile(".agents/open-forge.json", included ? "{\"schemaVersion\":2,\"roots\":[\"guide.md\"]}" : "{\"schemaVersion\":2}");
        files.CreateFile(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("framework", null), [], []),
        }));
        files.CreateFile(".agents/docs/_docs.md", "---\nopen-forge:\n  description: Docs\n  tags: [Docs]\n---\n# Docs\n\n## Entries\n\n- [Leaf](leaf.md) - #Docs\n");
        files.CreateFile(".agents/docs/leaf.md", "---\nopen-forge:\n  description: Leaf\n  tags: [Docs]\n---\n# Leaf\n");
        files.CreateFile("guide.md", "# Guide\n");
        return files;
    }
}
