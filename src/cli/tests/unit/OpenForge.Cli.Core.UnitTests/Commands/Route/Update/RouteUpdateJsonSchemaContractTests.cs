using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateJsonSchemaContractTests
{
    [Fact(DisplayName = "Route Update schema-v1 serializes exact order arrays and required nulls"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void SchemaV1SerializesExactOrderArraysAndRequiredNulls()
    {
        var document = new RouteUpdateJsonDocument
        {
            SchemaVersion = 1,
            Command = "route update",
            Status = "incomplete",
            Workspace = null,
            Result = new RouteUpdateJsonResult
            {
                Mode = "dry-run",
                Target = new RouteUpdateJsonTarget
                {
                    Requested = "memory/topic",
                    SelectedBy = null,
                    Id = null,
                    Path = null,
                    Form = null,
                    OverwritePaths = [],
                },
                Patch = new RouteUpdateJsonPatch
                {
                    Description = new RouteUpdateJsonDescriptionPatch
                    {
                        Requested = true,
                        Before = null,
                        Expected = "After",
                        State = "unresolved",
                    },
                    Responsibility = new RouteUpdateJsonResponsibilityPatch
                    {
                        Requested = false,
                        Operation = "not-requested",
                        Before = null,
                        Expected = null,
                        State = "not-requested",
                    },
                    Tags = new RouteUpdateJsonTagsPatch
                    {
                        Requested = false,
                        Before = null,
                        Expected = null,
                        State = "not-requested",
                    },
                },
                Template = new RouteUpdateJsonTemplate
                {
                    Requested = "templates/topic",
                    Id = null,
                    Path = null,
                    Classification = null,
                    BodyByteLength = null,
                    Decision = "unresolved",
                },
                Plan = new RouteUpdateJsonPlan
                {
                    Completeness = "incomplete",
                    Safety = "not-established",
                    Body = "not-established",
                },
                Effects =
                [
                    new RouteUpdateJsonEffect
                    {
                        Path = ".agents/memory/topic.md",
                        Kind = "routed-file",
                        Action = "replace",
                        Change = new RouteUpdateJsonChange
                        {
                            Before = "before-hash",
                            Expected = "expected-hash",
                        },
                        Preview =
                        [
                            new RouteUpdateJsonPreviewHunk
                            {
                                Kind = "metadata-field",
                                Before = "description: Before",
                                Expected = "description: After",
                            },
                        ],
                        Outcome = "planned",
                        Residual = "none",
                    },
                ],
                UnchangedPaths = [],
                Recovery = new RouteUpdateJsonRecovery
                {
                    State = "not-required",
                    ResidualPath = null,
                },
                Verification = "not-requested",
                Findings =
                [
                    new RouteUpdateJsonFinding
                    {
                        Code = "route-update.template-unavailable",
                        Status = "incomplete",
                        Target = null,
                        Cause = "The Template was unavailable.",
                    },
                ],
            },
            Next = null,
        };

        var json = JsonSerializer.Serialize(
            document,
            RouteUpdateJsonContext.Default.RouteUpdateJsonDocument);
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        var result = root.GetProperty("result");
        var target = result.GetProperty("target");
        var patch = result.GetProperty("patch");
        var template = result.GetProperty("template");
        var effect = Assert.Single(result.GetProperty("effects").EnumerateArray());
        var preview = Assert.Single(effect.GetProperty("preview").EnumerateArray());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());

        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(result, "mode", "target", "patch", "template", "plan", "effects", "unchangedPaths", "recovery", "verification", "findings");
        AssertPropertyOrder(target, "requested", "selectedBy", "id", "path", "form", "overwritePaths");
        AssertPropertyOrder(patch, "description", "responsibility", "tags");
        AssertPropertyOrder(patch.GetProperty("description"), "requested", "before", "expected", "state");
        AssertPropertyOrder(patch.GetProperty("responsibility"), "requested", "operation", "before", "expected", "state");
        AssertPropertyOrder(patch.GetProperty("tags"), "requested", "before", "expected", "state");
        AssertPropertyOrder(template, "requested", "id", "path", "classification", "bodyByteLength", "decision");
        AssertPropertyOrder(result.GetProperty("plan"), "completeness", "safety", "body");
        AssertPropertyOrder(effect, "path", "kind", "action", "change", "preview", "outcome", "residual");
        AssertPropertyOrder(effect.GetProperty("change"), "before", "expected");
        AssertPropertyOrder(preview, "kind", "before", "expected");
        AssertPropertyOrder(result.GetProperty("recovery"), "state", "residualPath");
        AssertPropertyOrder(finding, "code", "status", "target", "cause");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal(JsonValueKind.Null, target.GetProperty("selectedBy").ValueKind);
        Assert.Equal(JsonValueKind.Null, target.GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, target.GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Null, target.GetProperty("form").ValueKind);
        Assert.Equal(JsonValueKind.Null, template.GetProperty("classification").ValueKind);
        Assert.Equal(JsonValueKind.Null, template.GetProperty("bodyByteLength").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("target").ValueKind);
        Assert.Empty(target.GetProperty("overwritePaths").EnumerateArray());
        Assert.Empty(result.GetProperty("unchangedPaths").EnumerateArray());
    }

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
