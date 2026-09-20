using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class StatusBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Status output observes a genuinely installed bundled Extension without changing bytes")]
    public async Task HealthyWithExtension()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("status-output-extension");
        await workspace.SeedFrameworkAsync();
        var installed = await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "healthy-with-extension",
            Arguments = ["status"],
            ExitCode = 0,
        }, testName: nameof(HealthyWithExtension));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Status output preserves observed installed workspace conditions without effects")]
    public async Task InstalledWorkspace()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in new[]
        {
            ("healthy", 0),
            ("changed-managed-file", 2),
            ("missing-managed-file", 2),
            ("stale-entries", 2),
            ("no-ownership-record", 0),
            ("unreadable-entry-file", 3),
            ("library-link-missing", 2),
        })
        {
            using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-output");
            switch (situation)
            {
                case "library-link-missing":
                    workspace.WriteText("shared/team-knowledge/.agents/resources/data.json", "{\"value\":1}\n");
                    OpenForge.Cli.IntegrationTests.Framework.Ownership.OwnershipFixture.Libraries(workspace.Path,
                        new("team-knowledge", "shared/team-knowledge", ".", [".agents/resources/data.json"]));
                    break;
                case "changed-managed-file":
                    workspace.OverwriteInstalledText(".agents/guidance/_guidance.md",
                        File.ReadAllText(workspace.Combine(".agents/guidance/_guidance.md")) + "\nAdditional authored instructions.\n");
                    break;
                case "missing-managed-file":
                    workspace.DeleteInstalledTarget("CLAUDE.md");
                    break;
                case "stale-entries":
                    workspace.OverwriteInstalledText(".agents/maps/_maps.md", "---\nopen-forge:\n  description: Maps\n  tags: [Map]\n---\n# Maps\n\n## Entries\n\n- [Missing](missing.md) - #Map\n");
                    break;
                case "no-ownership-record":
                    workspace.DeleteInstalledTarget(StatusIntegrationWorkspace.OwnershipPath);
                    break;
            }

            var before = workspace.SnapshotHashes();
            using (var unavailable = situation == "unreadable-entry-file"
                       ? File.Open(workspace.Combine(StatusIntegrationWorkspace.GeneratedTargetPath), FileMode.Open, FileAccess.Read, FileShare.None)
                       : null)
            {
                await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = situation,
                    Arguments = ["status"],
                    ExitCode = exitCode,
                }, snapshotCollector: snapshots);
            }

            Assert.Equal(before, workspace.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(InstalledWorkspace));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Status output observes an owned recovery bundle without altering its bytes")]
    public async Task RecoveryBundlePresent()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-output-recovery");
        using var recovery = StatusRecoveryFixture.Create(workspace);
        var path = await recovery.AddVerifiedFinalAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        Assert.Equal(recovery.WorkspaceDirectory, Path.GetDirectoryName(path));
        Assert.True(File.Exists(path));
        var before = workspace.SnapshotHashes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(recovery.WorkspaceDirectory);
        await new ReadCommandOutputCapture(workspace.Path, recoveryBundlePath: path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "recovery-bundle-present",
            Arguments = ["status"],
            ExitCode = 2,
        }, testName: nameof(RecoveryBundlePresent));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(recovery.WorkspaceDirectory));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Status output preserves absent workspaces and invalid input without effects")]
    public async Task WorkspaceBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in new[]
        {
            ("not-installed", 0),
            ("blocked-workspace", 5),
            ("invalid-input", 4),
        })
        {
            using var workspace = StatusIntegrationWorkspace.Create("status-output-boundary");
            string[] arguments = situation switch
            {
                "not-installed" => ["status"],
                "blocked-workspace" => ["status", "--workspace", workspace.Combine("missing-workspace")],
                "invalid-input" => ["status", "unexpected-operand"],
                _ => throw new ArgumentOutOfRangeException(nameof(situation)),
            };
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
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(WorkspaceBoundary));
    }
}
