using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class RouteInspectMatchingFilesOutputTests
{
    [Fact(DisplayName = "Matching files retain their complete presentation at every detail level")]
    public async Task MatchingFiles()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var scenario in new[]
        {
            "matches", "zero", "capped", "unavailable", "files-unavailable", "unconditioned", "with-for",
            "missing-source", "multiple-sources", "invalid-working-path", "invalid-workspace",
            "pending-selected", "pending-startup", "pending-resolved",
        })
        {
            using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
            var pendingScenario = scenario.StartsWith("pending-", StringComparison.Ordinal);
            var loadingTag = pendingScenario ? "LoadNow" : "Docs";
            if (pendingScenario)
            {
                workspace.WriteLoader(
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("Documents", "docs/_docs.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Maps", "maps/_maps.md", "Maps"),
                ]);
                workspace.WriteEntrypoint(new()
                {
                    RelativePath = ".agents/maps/_maps.md",
                    Description = "Maps",
                    Tags = ["Maps"],
                    Entries = [],
                });
            }
            else
            {
                workspace.WriteLoader([RouteInspectProfileIntegrationWorkspace.Entry("Documents", "docs/_docs.md", loadingTag)]);
            }

            workspace.WriteEntrypoint(new()
            {
                RelativePath = ".agents/docs/_docs.md",
                Description = "Documents",
                Tags = ["Docs"],
                Entries = [RouteInspectProfileIntegrationWorkspace.Entry("Guide", "guide.md", loadingTag)],
            });
            workspace.WriteRoutedMarkdown(".agents/docs/guide.md", "Guide", [loadingTag], "# Guide\n",
                scenario == "unconditioned" ? null : ["docs/*.md"]);
            if (scenario != "zero")
            {
                workspace.Write("docs/guide.md", "Guide\n");
                workspace.Write("docs/reference.md", "Reference\n");
            }

            if (scenario == "capped")
            {
                for (var index = 0; index < 141; index++)
                {
                    workspace.Write(FormattableString.Invariant($"docs/item-{index:D3}.md"), "Item\n");
                }
            }

            var before = workspace.Snapshot();
            if (scenario == "unconditioned")
            {
                var operation = RouteInspectOperationFactory.Create(null, (_, _) =>
                    throw new InvalidOperationException("An unrestricted source must not enumerate files."));
                var result = await operation(new(workspace.Workspace, "docs/guide", false, matchingFiles: true),
                    TestContext.Current.CancellationToken);
                Assert.Equal(CliSemanticStatus.Complete, result.Status);
                var files = Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles);
                Assert.Equal(RouteInspectMatchingFilesScope.AllFiles, files.Scope);
                Assert.Null(files.Count);
                Assert.Empty(files.Paths);
                CommandOutputRenderers<RouteInspectResult>.From(RouteInspectPresentation.Rendering)
                    .MatchDetails(result, scenario, snapshotCollector: snapshots);
            }
            else if (pendingScenario)
            {
                var reference = scenario == "pending-selected" ? "docs/guide" : "maps";
                string[] workingPaths = scenario == "pending-resolved" ? ["--for", "docs/planned.md"] : [];
                var result = await workspace.InspectAsync(reference, TestContext.Current.CancellationToken,
                    scenario == "pending-resolved" ? ["docs/planned.md"] : null);
                Assert.Equal(scenario == "pending-resolved" ? CliSemanticStatus.Complete : CliSemanticStatus.Incomplete, result.Status);
                if (scenario != "pending-resolved")
                {
                    Assert.Contains("--for <path>", Assert.IsType<OpenForge.Cli.Core.Shell.Pipeline.Models.Operation.CliNextAction>(result.Next).Command,
                        StringComparison.Ordinal);
                }
                else
                {
                    Assert.Null(result.Next);
                }

                if (scenario == "pending-startup")
                {
                    Assert.Null(result.Applicability);
                }

                await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = scenario,
                    Arguments = ["route", "inspect", reference, .. workingPaths],
                    ExitCode = scenario == "pending-resolved" ? 0 : 3,
                }, snapshotCollector: snapshots);
            }
            else if (scenario is "missing-source" or "multiple-sources" or "invalid-working-path" or "invalid-workspace")
            {
                string[] arguments = scenario switch
                {
                    "missing-source" => [],
                    "multiple-sources" => ["docs", "docs/guide"],
                    "invalid-working-path" => ["docs/guide", "--for", "../outside.cs"],
                    "invalid-workspace" => ["docs/guide", "--workspace", "missing"],
                    _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
                };
                await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = scenario,
                    Arguments = ["route", "inspect", .. arguments, "--matching-files"],
                    ExitCode = 4,
                }, snapshotCollector: snapshots);
            }
            else if (scenario is "unavailable" or "files-unavailable")
            {
                // C1's enumeration seam keeps a confirmed Git failure deterministic across hosts.
                var operation = RouteInspectOperationFactory.Create(null, (_, _) => ValueTask.FromResult(
                    RouteInspectFileEnumeration.Unavailable(RouteInspectMatchingFilesScope.GitTrackedAndUntracked,
                        scenario == "files-unavailable"
                            ? RouteInspectMatchingFilesReason.FilesUnavailable
                            : RouteInspectMatchingFilesReason.GitUnavailable)));
                var result = await operation(new(workspace.Workspace, "docs/guide", false, matchingFiles: true),
                    TestContext.Current.CancellationToken);
                Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
                Assert.False(Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles).Complete);
                CommandOutputRenderers<RouteInspectResult>.From(RouteInspectPresentation.Rendering)
                    .MatchDetails(result, scenario, snapshotCollector: snapshots);
            }
            else
            {
                string[] workingPaths = scenario == "with-for" ? ["--for", "docs/planned.md", "--for", "src/other.cs"] : [];
                await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
                {
                    Situation = scenario,
                    Arguments = ["route", "inspect", "docs/guide", "--matching-files", .. workingPaths],
                    ExitCode = 0,
                }, snapshotCollector: snapshots);
            }

            RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
