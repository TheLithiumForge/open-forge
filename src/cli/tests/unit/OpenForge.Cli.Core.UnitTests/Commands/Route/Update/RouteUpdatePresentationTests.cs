using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdatePresentationTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Update native reports retain status disposition"),
     InlineData((int)RouteUpdateFindingCode.InvalidPatch, "Cannot update memory/topic", 4, (int)CliOutputTarget.StandardError),
     InlineData((int)RouteUpdateFindingCode.IdentityCollision, "Cannot update memory/topic", 5, (int)CliOutputTarget.StandardError),
     InlineData((int)RouteUpdateFindingCode.WorkspaceUnavailable, "memory/topic could not be updated", 3, (int)CliOutputTarget.StandardOutput),
     Trait("Feature", "route-update"), Trait("Evidence", "Unit")]
    public void NativeReportsRetainStatusDisposition(
        int findingValue,
        string expectedHeadline,
        int expectedExit,
        int expectedTarget)
    {
        var result = RouteUpdateTestData.Result(RouteUpdateTestData.VerifiedNoOpFormation(
            findings: [RouteUpdateTestData.Finding((RouteUpdateFindingCode)findingValue)]));
        var rendered = CliRenderingStage.Render(
            Presentation(result, CliFormat.Text),
            RouteUpdatePresentation.Rendering);

        Assert.Contains(expectedHeadline, rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal((CliOutputTarget)expectedTarget, rendered.PrimaryTarget);
        Assert.Equal(expectedExit, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update native minimal output lists changed fields and the parent entry"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void NativeMinimalOutputListsChangedFieldsAndParentEntry()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation() with
        {
            Patch = ChangedPatch(),
            Effects =
            [
                RouteUpdateTestData.Effect(),
                RouteUpdateTestData.Effect(RouteUpdateTestData.ParentPath, RouteUpdateEffectKind.GeneratedRegion),
            ],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.Removed(),
        };

        var text = Render(RouteUpdateTestData.Result(formation), CliFormat.Text, CliDetail.Minimal);

        AssertInOrder(
            text,
            $"Updated {RouteUpdateTestData.TargetId}",
            "description: \"Before\" -> \"After\"",
            "responsibility: \"Before responsibility\" -> \"After responsibility\"",
            "tags: #Before -> #After",
            $"Entry updated in {RouteUpdateTestData.ParentPath}");
        Assert.DoesNotContain(RouteUpdateTestData.TargetPath + "  frontmatter rewritten", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update native standard and full output add paths, effect hashes, and frontmatter"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void NativeStandardAndFullOutputAddDetail()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation() with
        {
            Patch = ChangedPatch(),
            Effects =
            [
                RouteUpdateTestData.Effect() with
                {
                    Preview =
                    [
                        new RouteUpdatePreviewHunk
                        {
                            Kind = RouteUpdatePreviewKind.MetadataField,
                            Before = "description: Before",
                            Expected = "description: After",
                        },
                    ],
                },
            ],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.Removed(),
        };
        var result = RouteUpdateTestData.Result(formation);

        var standard = Render(result, CliFormat.Text, CliDetail.Standard);
        Assert.Contains($"Path: {RouteUpdateTestData.TargetPath}", standard, StringComparison.Ordinal);
        Assert.Contains($"{RouteUpdateTestData.TargetPath}  frontmatter rewritten", standard, StringComparison.Ordinal);

        var full = Render(result, CliFormat.Text, CliDetail.Full);
        Assert.Contains("Before: before-hash", full, StringComparison.Ordinal);
        Assert.Contains("After: expected-hash", full, StringComparison.Ordinal);
        Assert.Contains("Frontmatter before:", full, StringComparison.Ordinal);
        Assert.Contains("description: Before", full, StringComparison.Ordinal);
        Assert.Contains("description: After", full, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update native states protected Template body without a legacy status line"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void NativeProtectedTemplateBodyUsesCatalogueWording()
    {
        var text = Render(ProtectedBodyResult(), CliFormat.Text, CliDetail.Minimal);

        Assert.StartsWith(
            "Updated memory/topic, but the Template body was not copied.",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            "The file already has content, which was kept. The Template templates/topic was not copied.",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update native JSON uses the report envelope and catalogue data shape"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void NativeJsonUsesCatalogueDataShape()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation() with
        {
            Patch = ChangedPatch(),
            Effects =
            [
                RouteUpdateTestData.Effect(),
                RouteUpdateTestData.Effect(RouteUpdateTestData.ParentPath, RouteUpdateEffectKind.GeneratedRegion),
            ],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.Removed(),
        };
        var json = Render(RouteUpdateTestData.Result(formation), CliFormat.Json, CliDetail.Full);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var data = root.GetProperty("data");
        var change = Assert.Single(data.GetProperty("changes").EnumerateArray(), item =>
            item.GetProperty("field").GetString() == "description");

        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route update", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.Equal(RouteUpdateTestData.TargetId, data.GetProperty("target").GetProperty("id").GetString());
        Assert.Equal(RouteUpdateTestData.TargetPath, data.GetProperty("target").GetProperty("path").GetString());
        Assert.Equal("Before", change.GetProperty("before").GetString());
        Assert.Equal("After", change.GetProperty("after").GetString());
        Assert.Equal(RouteUpdateTestData.ParentPath, data.GetProperty("listedIn").GetString());
        Assert.Equal("before-hash", root.GetProperty("effects").EnumerateArray().First().GetProperty("before").GetString());
        Assert.Equal(3, root.GetProperty("counts").GetProperty("fieldsChanged").GetInt32());
        Assert.Equal(1, root.GetProperty("counts").GetProperty("sectionsUpdated").GetInt32());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update native diagnostics retain bounded facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void NativeDiagnosticsRetainBoundedFacts()
    {
        var result = ProtectedBodyResult();
        var rendered = CliRenderingStage.Render(
            Presentation(result, CliFormat.Text, CliDetail.Debug),
            RouteUpdatePresentation.Rendering);

        Assert.NotNull(rendered.DiagnosticContent);
        Assert.Contains("status=completed-with-warnings", rendered.DiagnosticContent!, StringComparison.Ordinal);
        Assert.Contains("mode=apply", rendered.DiagnosticContent!, StringComparison.Ordinal);
        Assert.Contains("body=authored-body-protected", rendered.DiagnosticContent!, StringComparison.Ordinal);
        Assert.Contains("The authored body is protected.", rendered.DiagnosticContent!, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Update help exposes exactly the accepted public surface"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void HelpExposesExactlyAcceptedPublicSurface()
    {
        var help = RouteUpdatePresentation.CreateHelp();

        Assert.Equal(
        [
            "Syntax",
            "Target",
            "Metadata",
            "Template",
            "Write policy",
            "Examples",
            "Notes",
        ],
        help.Sections.Select(section => section.Heading));
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));
        Assert.Contains("open-forge route update <source-reference>", text, StringComparison.Ordinal);
        Assert.Contains("--description <text>", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility <text>", text, StringComparison.Ordinal);
        Assert.Contains("--tag <tag>", text, StringComparison.Ordinal);
        Assert.Contains("--template <template-reference>", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.Contains("exact empty responsibility removes", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("authored body", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--body", text, StringComparison.Ordinal);
    }

    private static RouteUpdateResult ProtectedBodyResult()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            RouteUpdateTestData.ProtectedTemplate(),
            RouteUpdateBodyState.AuthoredBodyProtected,
            findings:
            [
                RouteUpdateTestData.Finding(
                    RouteUpdateFindingCode.TemplateBodyProtected,
                    cause: "The authored body is protected."),
            ]) with
        {
            Patch = ChangedPatch(),
        };
        return RouteUpdateTestData.Result(formation);
    }

    private static RouteUpdatePatch ChangedPatch()
        => new()
        {
            Description = new RouteUpdateDescriptionPatch
            {
                Requested = true,
                Before = "Before",
                Expected = "After",
                State = RouteUpdatePatchState.Changed,
            },
            Responsibility = new RouteUpdateResponsibilityPatch
            {
                Requested = true,
                Operation = RouteUpdateResponsibilityOperation.Set,
                Before = "Before responsibility",
                Expected = "After responsibility",
                State = RouteUpdatePatchState.Changed,
            },
            Tags = new RouteUpdateTagsPatch
            {
                Requested = true,
                Before = ["Before"],
                Expected = ["After"],
                State = RouteUpdatePatchState.Changed,
            },
        };

    private static string Render(RouteUpdateResult result, CliFormat format, CliDetail detail)
        => CliRenderingStage.Render(Presentation(result, format, detail), RouteUpdatePresentation.Rendering).PrimaryContent;

    private static CliPresentationRequest<RouteUpdateResult> Presentation(
        RouteUpdateResult result,
        CliFormat format,
        CliDetail detail = CliDetail.Standard)
        => new(result, new CliPresentation(format, detail, null));

    private static void AssertInOrder(string text, params string[] expected)
    {
        var offset = 0;
        foreach (var item in expected)
        {
            var next = text.IndexOf(item, offset, StringComparison.Ordinal);
            Assert.True(next >= offset, $"Expected '{item}' after offset {offset}.{Environment.NewLine}{text}");
            offset = next + item.Length;
        }
    }
}
