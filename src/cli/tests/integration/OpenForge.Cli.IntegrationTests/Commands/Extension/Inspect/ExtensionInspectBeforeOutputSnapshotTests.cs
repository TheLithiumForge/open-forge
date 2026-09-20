using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Inspect;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionInspectBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension inspect output preserves installed and available package relationships without effects")]
    public async Task PackageRelationship()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in new[]
        {
            ("installed-matches", 0),
            ("installed-changed-and-retired", 2),
            ("available-not-installed", 0),
            ("installed-source-missing", 3),
            ("newer-available", 0),
            ("dependency-cycle", 5),
            ("unknown-id", 4),
            ("no-ownership-record", 0),
            ("invalid-input", 4),
            ("ambiguous-source", 5),
        })
        {
            using var workspace = TemporaryWorkspace.Create("extension-inspect-output");
            using var source = ExtensionInstallCatalogue.Create("extension-inspect-output-source");
            source.AddPackage("toolkit", situation == "dependency-cycle" ? ["helper"] : [], (".agents/toolkit.md", "alpha\n"));
            if (situation == "dependency-cycle")
            {
                source.AddPackage("helper", ["toolkit"], (".agents/helper.md", "# Helper\n"));
            }
            if (situation == "ambiguous-source")
            {
                source.AddPackage("duplicate", [], (".agents/toolkit.md", "alpha\n"));
                source.ReplaceManifest("duplicate", File.ReadAllText(Path.Combine(source.PackagePath("toolkit"), "extension.json")));
            }

            if (situation != "no-ownership-record")
            {
                var version = situation == "newer-available" ? "0.9.0" : "1.0.0";
                var paths = situation == "installed-changed-and-retired"
                    ? "\".agents/toolkit.md\",\".agents/retired.md\"" : "\".agents/toolkit.md\"";
                workspace.WriteText(".agents/open-forge.lock.json", situation == "available-not-installed"
                    ? "{\"schemaVersion\":1,\"extensions\":[]}"
                    : $$"""{"schemaVersion":1,"extensions":[{"id":"toolkit","version":"{{version}}","source":"embedded catalogue","dependencies":[],"paths":[{{paths}}],"regions":[]}]}""");
            }

            if (situation != "available-not-installed")
            {
                workspace.WriteText(".agents/toolkit.md", situation == "installed-changed-and-retired" ? "locally changed\n" : "alpha\n");
            }

            if (situation == "installed-changed-and-retired")
            {
                workspace.WriteText(".agents/retired.md", "retired content\n");
            }

            var sourcePath = situation == "installed-source-missing" ? source.PackagePath("missing-source") : source.Path;
            var id = situation == "unknown-id" ? "unknown" : "toolkit";
            string[] arguments = situation == "invalid-input"
                ? ["extension", "inspect", "toolkit", "unexpected-operand"]
                : ["extension", "inspect", id, "--source", sourcePath];
            var before = workspace.SnapshotHashes();
            var sourceBefore = source.Snapshot();
            await new ReadCommandOutputCapture(workspace.Path, source.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = arguments,
                ExitCode = exitCode,
                ShellDiagnostic = situation == "invalid-input",
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(sourceBefore, source.Snapshot());
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(PackageRelationship));
    }
}
