using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.List;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionListBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension list output preserves installed ownership and selected catalogue coverage")]
    public async Task OwnershipSelection()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[]
        {
            "none-installed",
            "one-installed",
            "installed-only",
            "available-only",
            "no-ownership-record",
        })
        {
            if (situation is "one-installed" or "installed-only")
            {
                using var installedWorkspace = ExtensionInstallIntegrationWorkspace.Create("extension-list-output");
                await installedWorkspace.SeedFrameworkAsync();
                var installed = await installedWorkspace.RunAsync(["extension", "install", "development-toolkit", "--automatic"]);
                Assert.Equal(0, installed.ExitCode);
                var installedBefore = installedWorkspace.Snapshot();
                string[] installedSelection = situation == "installed-only" ? ["--installed"] : [];
                await new ReadCommandOutputCapture(installedWorkspace.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = situation,
                    Arguments = ["extension", "list", .. installedSelection],
                    ExitCode = 0,
                }, snapshotCollector: snapshots);
                Assert.Equal(installedBefore, installedWorkspace.Snapshot());
                continue;
            }

            using var workspace = TemporaryWorkspace.Create("extension-list-output");
            if (situation != "no-ownership-record")
            {
                workspace.WriteText(".agents/open-forge.lock.json", situation == "none-installed"
                    ? "{\"schemaVersion\":1,\"extensions\":[]}"
                    : "{\"schemaVersion\":1,\"extensions\":[{\"id\":\"development-toolkit\",\"version\":\"0.1.0\",\"source\":\"embedded catalogue\",\"dependencies\":[],\"paths\":[\".agents/workflows/architecture.md\"],\"regions\":[]}]}");
            }

            string[] selection = situation switch
            {
                "installed-only" => ["--installed"],
                "available-only" => ["--available"],
                _ => [],
            };
            var before = workspace.SnapshotHashes();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = ["extension", "list", .. selection],
                ExitCode = 0,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(OwnershipSelection));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension list output preserves an explicit external catalogue without effects")]
    public async Task ExplicitSource()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-output-source");
        using var source = ExtensionInstallCatalogue.Create("extension-list-output-catalogue");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", "# Toolkit\n"));
        var before = workspace.SnapshotHashes();
        var sourceBefore = source.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path, source.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "explicit-source",
            Arguments = ["extension", "list", "--source", source.Path],
            ExitCode = 0,
        }, testName: nameof(ExplicitSource));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension list output preserves unavailable and invalid source observations")]
    public async Task SourceBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var scenario in new[]
        {
            ("source-unreadable", 3),
            ("source-invalid", 4),
            ("installed-source-missing", 2),
        })
        {
            var (situation, exitCode) = scenario;
            using var workspace = TemporaryWorkspace.Create("extension-list-output-boundary");
            using var source = ExtensionInstallCatalogue.Create("extension-list-output-boundary-source");
            source.AddPackage("toolkit", [], (".agents/toolkit.md", "# Toolkit\n"));
            if (situation == "source-invalid") source.ReplaceManifest("toolkit", "{ malformed");
            if (situation == "installed-source-missing")
            {
                var missingSource = Path.Combine(source.Path, "missing-source")
                    .Replace("\\", "\\\\", StringComparison.Ordinal);
                workspace.WriteText(".agents/open-forge.lock.json", $$"""{"schemaVersion":1,"extensions":[{"id":"missing-package","version":"1.0.0","source":"{{missingSource}}","dependencies":[],"paths":[],"regions":[]}] }""");
            }
            var before = workspace.SnapshotHashes();
            var sourceBefore = source.Snapshot();
            using (var held = situation == "source-unreadable"
                       ? File.Open(Path.Combine(source.PackagePath("toolkit"), "extension.json"), FileMode.Open, FileAccess.Read, FileShare.None)
                       : null)
            {
                await new ReadCommandOutputCapture(workspace.Path, source.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = situation,
                    Arguments = ["extension", "list", "--source", source.Path],
                    ExitCode = exitCode,
                }, snapshotCollector: snapshots);
            }
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(sourceBefore, source.Snapshot());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(SourceBoundary));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension list output preserves invalid input and blocked overlapping sources")]
    public async Task InvalidBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var scenario in new[]
        {
            ("source-blocked", 5),
            ("invalid-input", 4),
        })
        {
            var (situation, exitCode) = scenario;
            using var workspace = TemporaryWorkspace.Create("extension-list-output-invalid");
            string[] arguments = situation == "source-blocked"
                ? ["extension", "list", "--source", workspace.Path]
                : ["extension", "list", "unexpected-operand"];
            var before = workspace.SnapshotHashes();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = arguments,
                ExitCode = exitCode,
                ShellDiagnostic = situation == "invalid-input",
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(InvalidBoundary));
    }

}
