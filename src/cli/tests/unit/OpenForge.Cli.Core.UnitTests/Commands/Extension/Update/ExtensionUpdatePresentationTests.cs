using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdatePresentationTests
{
    [Fact(DisplayName = "Extension Update help preserves the accepted command sections and syntax"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void HelpPreservesAcceptedSectionsAndSyntax()
    {
        var help = ExtensionUpdatePresentation.CreateHelp();

        Assert.Equal(
            ["Command", "Selection and dependencies", "Authority", "Results and streams"],
            help.Sections.Select(section => section.Heading));
        Assert.Contains(
            "open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]",
            help.Sections[0].Body,
            StringComparison.Ordinal);
        Assert.Contains("Dependencies are resolved transitively and dependency-first.", help.Sections[1].Body, StringComparison.Ordinal);
        Assert.Contains("--force", help.Sections[2].Body, StringComparison.Ordinal);
        Assert.Contains("--prune", help.Sections[2].Body, StringComparison.Ordinal);
        Assert.Contains("Dry-run writes nothing.", help.Sections[3].Body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Update JSON projection preserves one ordered schema-v1 result envelope"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesOrderedSchemaV1Envelope()
    {
        var result = ExtensionUpdateResult.Empty(
            workspace: null,
            mode: ExtensionUpdateMode.DryRun,
            force: false,
            prune: false,
            automatic: true,
            findings:
            [
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.InvalidInput,
                    "Correct the selected input."),
            ]);
        var json = ExtensionUpdateJsonProjection.RenderJson(
            new CliPresentationRequest<ExtensionUpdateResult>(
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
        Assert.Equal("extension update", root.GetProperty("command").GetString());
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(
        [
            "mode", "force", "prune", "automatic", "selection", "source", "packages",
            "comparisons", "generatedNavigation", "effects", "permissions", "lifecycle", "recovery",
            "verification", "findings",
        ],
            root.GetProperty("result").EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", root.GetProperty("result").GetProperty("mode").GetString());
        Assert.True(root.GetProperty("result").GetProperty("automatic").GetBoolean());
        Assert.Empty(root.GetProperty("result").GetProperty("packages").EnumerateArray());
        Assert.Empty(root.GetProperty("result").GetProperty("comparisons").EnumerateArray());
        Assert.Empty(root.GetProperty("result").GetProperty("effects").EnumerateArray());
        Assert.Equal(
            ["code", "status", "target", "cause"],
            root.GetProperty("result").GetProperty("findings").EnumerateArray()
                .Single()
                .EnumerateObject()
                .Select(property => property.Name));
        Assert.Equal("extension-update.invalid-input", root.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal("open-forge extension update --help", root.GetProperty("next").GetProperty("command").GetString());
    }
}
