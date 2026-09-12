using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePresentationTests
{
    [Fact(DisplayName = "Extension Remove help preserves the accepted syntax and safety sections"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void HelpPreservesAcceptedSectionsAndSyntax()
    {
        var help = ExtensionRemovePresentation.CreateHelp();

        Assert.Equal(
            ["Command", "Selection and dependencies", "Ownership and changed content", "Results and streams"],
            help.Sections.Select(section => section.Heading));
        Assert.Contains(
            "open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global flags]",
            help.Sections[0].Body,
            StringComparison.Ordinal);
        Assert.Contains("Retained dependents block dependency removal", help.Sections[1].Body, StringComparison.Ordinal);
        Assert.Contains("unless this request includes --prune", help.Sections[2].Body, StringComparison.Ordinal);
        Assert.Contains("Dry-run writes nothing.", help.Sections[3].Body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Remove JSON preserves one ordered schema-v1 result envelope"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesOrderedSchemaV1Envelope()
    {
        var result = ExtensionRemoveResult.Empty(
            workspace: null,
            mode: ExtensionRemoveMode.DryRun,
            prune: false,
            automatic: true,
            findings:
            [
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.InvalidInput,
                    "Correct the selected input."),
            ]);
        var json = ExtensionRemoveJsonProjection.RenderJson(
            new CliPresentationRequest<ExtensionRemoveResult>(
                result,
                new CliPresentation(
                    CliOutputFormat.Json,
                    CliView.Expanded,
                    CliVerbosity.Normal)));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);

        var commandResult = root.GetProperty("result");
        Assert.Equal(
        [
            "mode", "prune", "automatic", "selection", "dependencies", "paths",
            "generatedNavigation", "effects", "permissions", "lifecycle", "recovery", "verification",
            "packageSourceUnchanged", "findings",
        ],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", commandResult.GetProperty("mode").GetString());
        Assert.False(commandResult.GetProperty("prune").GetBoolean());
        Assert.True(commandResult.GetProperty("automatic").GetBoolean());
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("selection").ValueKind);
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("dependencies").ValueKind);
        Assert.Empty(commandResult.GetProperty("paths").EnumerateArray());
        Assert.Empty(commandResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-requested", commandResult.GetProperty("lifecycle").GetProperty("trust").GetString());
        Assert.Equal("not-required", commandResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.True(commandResult.GetProperty("packageSourceUnchanged").GetBoolean());
        Assert.Equal(
            ["code", "status", "target", "cause"],
            commandResult.GetProperty("findings").EnumerateArray()
                .Single()
                .EnumerateObject()
                .Select(property => property.Name));
        Assert.Equal(
            "extension-remove.invalid-input",
            commandResult.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(
            "open-forge extension remove --help",
            root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Extension Remove human presentation exposes workspace, policy, safety facts, status, and next action"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void HumanPresentationPreservesSafetyFacts()
    {
        var result = ExtensionRemoveResult.Empty(
            workspace: null,
            mode: ExtensionRemoveMode.DryRun,
            prune: false,
            automatic: true,
            findings:
            [
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.InvalidInput,
                    "Correct the selected input."),
            ]);

        var rendered = ExtensionRemovePresentation.RenderHuman(
            new CliPresentationRequest<ExtensionRemoveResult>(
                result,
                new CliPresentation(
                    CliOutputFormat.Human,
                    CliView.Expanded,
                    CliVerbosity.Normal)));

        Assert.Contains("Open Forge extension remove", rendered, StringComparison.Ordinal);
        Assert.Contains("Workspace: unavailable", rendered, StringComparison.Ordinal);
        Assert.Contains("Mode: dry-run", rendered, StringComparison.Ordinal);
        Assert.Contains("Prune: false", rendered, StringComparison.Ordinal);
        Assert.Contains("Automatic: true", rendered, StringComparison.Ordinal);
        Assert.Contains("Selection: unavailable", rendered, StringComparison.Ordinal);
        Assert.Contains("Dependencies: unavailable", rendered, StringComparison.Ordinal);
        Assert.Contains("Recovery: not-required", rendered, StringComparison.Ordinal);
        Assert.Contains("Package source unchanged: true", rendered, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", rendered, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge extension remove --help", rendered, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Remove diagnostic presentation stays bounded to status and finding count"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void DiagnosticPresentationIsBounded()
    {
        var result = ExtensionRemoveResult.Empty(
            workspace: null,
            mode: ExtensionRemoveMode.Apply,
            prune: false,
            automatic: false,
            findings:
            [
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.InvalidInput,
                    "Correct the selected input."),
            ]);

        var diagnostic = ExtensionRemovePresentation.RenderDiagnostic(
            new CliPresentationRequest<ExtensionRemoveResult>(
                result,
                new CliPresentation(
                    CliOutputFormat.Human,
                    CliView.Expanded,
                    CliVerbosity.Verbose)));

        Assert.Equal("status=invalid; findings=1", diagnostic);
    }
}
