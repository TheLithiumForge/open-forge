using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteInspectBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route inspect output preserves exact source selection from an ambiguous ID prompt")]
    public async Task AmbiguousIdPrompt()
    {
        using var workspace = new RoutedOutputWorkspace();
        workspace.CollidingGuideId();
        using var input = new StringReader("1\n");
        using var prompts = new StringWriter();
        var before = workspace.Snapshot();
        var terminal = new CliTerminal(
            new CliTerminalCapabilities(canPrompt: true, canReadKeys: false, canRedraw: false),
            (content, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                prompts.Write(content.Span);
                return ValueTask.CompletedTask;
            },
            cancellationToken => input.ReadLineAsync(cancellationToken),
            _ => ValueTask.FromResult<CliKeyStroke?>(null));
        var operation = RouteInspectOperationFactory.Create(
            RouteInspectSourceSelectionPrompt.Create(new CliPrompts(terminal)));
        var result = await operation(new RouteInspectRequest(
            new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory),
            "docs/guide", allowInteractiveSourceSelection: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Contains("docs/guide matches 2 sources. Which one?", prompts.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        CommandOutputRenderers<RouteInspectResult>.From(RouteInspectPresentation.Rendering)
            .MatchDetails(result, "ambiguous-id-prompt");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route inspect output preserves each source identity and its loading behavior")]
    public async Task Source()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in Scenarios())
        {
            using var workspace = new RoutedOutputWorkspace();
            var source = situation switch
            {
                "entrypoint" => "docs",
                "compatibility-entrypoint" => "legacy",
                "not-routed-file" => ".agents/unlisted.md",
                "id-not-unique-exact-path" => ".agents/docs/guide.md",
                "unknown-source" => "missing/source",
                "loader-subject" => ".agents/loader.md",
                "orphan-overwrite" => ".agents/orphan.overwrite.md",
                _ => "docs/guide",
            };
            switch (situation)
            {
                case "load-now-child":
                case "keep-in-mind":
                    var tag = situation == "load-now-child" ? "LoadNow" : "KeepInMind";
                    workspace.Replace(".agents/docs/guide.md", OpenForgeDocumentSeed.Metadata(description: "Guide", tags: ["Docs", tag], body: "# Guide\n"));
                    workspace.Replace(".agents/docs/_docs.md", OpenForgeDocumentSeed.Metadata(description: "Documents", tags: ["Docs"],
                        body: $"# Documents\n\n## Entries\n\n- [Guide](guide.md) - #Docs #{tag}\n- [Reference](reference.md) - #Docs\n"));
                    break;
                case "overwrite-pair":
                    workspace.Write(".agents/docs/guide.overwrite.md", OpenForgeDocumentSeed.Metadata(description: "Local guide", tags: ["Docs"], body: "# Local guide\n"));
                    break;
                case "compatibility-entrypoint":
                    workspace.Write(".agents/legacy/index.md", OpenForgeDocumentSeed.Metadata(description: "Legacy", tags: ["Docs"], body: "# Legacy\n\n## Entries\n\n- none - No entries - #Empty\n"));
                    workspace.Replace(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Documents](docs/_docs.md) - #Docs\n- [Legacy](legacy/index.md) - #Docs"));
                    break;
                case "not-routed-file":
                    workspace.Write(".agents/unlisted.md", OpenForgeDocumentSeed.Metadata(description: "Unlisted", tags: ["Docs"], body: "# Unlisted\n"));
                    break;
                case "id-not-unique-exact-path":
                    workspace.Write(".agents/docs/guide/_guide.md", OpenForgeDocumentSeed.Metadata(
                        description: "Guide category", tags: ["Docs"], body: "# Guide category\n\n## Entries\n\n- none - No entries - #Empty\n"));
                    break;
                case "unreadable-source":
                    workspace.ReplaceBytes(".agents/docs/guide.md", [0xff, 0xfe, 0xfd]);
                    break;
                case "orphan-overwrite":
                    workspace.Write(".agents/orphan.overwrite.md", "# Orphan\n");
                    break;
            }

            var before = workspace.Snapshot();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = ["route", "inspect", source],
                ExitCode = exitCode,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.Snapshot());
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    private static IEnumerable<(string Situation, int ExitCode)> Scenarios()
        =>
        [
            ("entrypoint", 0),
            ("routed-file", 0),
            ("load-now-child", 0),
            ("keep-in-mind", 0),
            ("overwrite-pair", 0),
            ("compatibility-entrypoint", 0),
            ("not-routed-file", 0),
            ("id-not-unique-exact-path", 2),
            ("unknown-source", 4),
            ("loader-subject", 4),
            ("unreadable-source", 3),
            ("orphan-overwrite", 5),
        ];
}
