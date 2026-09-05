using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveJsonSchemaContractTests
{
    [Fact(DisplayName = "Route Remove JSON keeps the accepted envelope and result property order"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void JsonKeepsEnvelopeAndResultOrder()
    {
        using var document = JsonDocument.Parse(Serialize(CreateDocument(includeNext: true)));
        var root = document.RootElement;
        var result = root.GetProperty("result");

        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertOrder(
            result,
            "mode", "source", "subject", "ownership", "references", "generatedNavigation",
            "plan", "effects", "unchangedPaths", "recovery", "verification", "findings");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route remove", root.GetProperty("command").GetString());
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal("open-forge doctor", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Route Remove JSON retains every nested coordinate and null boundary"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void JsonRetainsNestedCoordinatesAndNullBoundary()
    {
        using var document = JsonDocument.Parse(Serialize(CreateDocument(includeNext: false)));
        var root = document.RootElement;
        var result = root.GetProperty("result");

        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        AssertOrder(root.GetProperty("workspace"), "path", "selectedBy");
        AssertOrder(result.GetProperty("source"), "requested", "selectedBy", "id", "path", "form");
        AssertOrder(result.GetProperty("subject"), "kind", "layers", "items");
        AssertOrder(
            result.GetProperty("subject").GetProperty("layers")[0],
            "layer", "sourcePath");
        AssertOrder(
            result.GetProperty("subject").GetProperty("items")[0],
            "kind", "layer", "sourceId", "sourcePath", "relativePath");
        AssertOrder(result.GetProperty("ownership"), "state", "framework", "extensions", "claims");
        AssertOrder(
            result.GetProperty("ownership").GetProperty("claims")[0],
            "path", "manager", "owner");
        AssertOrder(
            result.GetProperty("references"),
            "coverage", "scannedSourceCount", "inspectedSourceCount", "occurrenceCount", "detachments");
        var detachment = result.GetProperty("references").GetProperty("detachments")[0];
        AssertOrder(
            detachment,
            "sourcePath", "layer", "location", "before", "expected", "originalDestination", "visibleLabel");
        AssertOrder(detachment.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        AssertOrder(result.GetProperty("generatedNavigation"), "coverage", "regions");
        AssertOrder(
            result.GetProperty("generatedNavigation").GetProperty("regions")[0],
            "path", "reasons", "state");
        AssertOrder(result.GetProperty("plan"), "completeness", "safety");
        var effect = result.GetProperty("effects")[0];
        AssertOrder(effect, "path", "kind", "action", "before", "expected", "outcome", "residual");
        AssertOrder(effect.GetProperty("before"), "kind", "contentSha256");
        AssertOrder(effect.GetProperty("expected"), "kind", "contentSha256");
        AssertOrder(result.GetProperty("recovery"), "state", "protectedPaths", "residualPath");
        AssertOrder(result.GetProperty("findings")[0], "code", "status", "target", "cause");
        Assert.Equal("route-remove.reference-unsafe", result.GetProperty("findings")[0].GetProperty("code").GetString());
    }

    private static RouteRemoveJsonDocument CreateDocument(bool includeNext)
        => new()
        {
            SchemaVersion = 1,
            Command = "route remove",
            Status = "blocked",
            Workspace = new RouteRemoveJsonWorkspace
            {
                Path = "workspace",
                SelectedBy = "explicit-workspace",
            },
            Result = new RouteRemoveJsonResult
            {
                Mode = "apply",
                Source = new RouteRemoveJsonSource
                {
                    Requested = "guidance/old guide",
                    SelectedBy = "source-id",
                    Id = "guidance/old guide",
                    Path = ".agents/guidance/old guide.md",
                    Form = "ordinary-markdown",
                },
                Subject = new RouteRemoveJsonSubject
                {
                    Kind = "leaf",
                    Layers =
                    [
                        new RouteRemoveJsonSubjectLayer
                        {
                            Layer = "base",
                            SourcePath = ".agents/guidance/old guide.md",
                        },
                    ],
                    Items =
                    [
                        new RouteRemoveJsonSubjectItem
                        {
                            Kind = "routed-markdown",
                            Layer = "base",
                            SourceId = "guidance/old guide",
                            SourcePath = ".agents/guidance/old guide.md",
                            RelativePath = "old guide.md",
                        },
                    ],
                },
                Ownership = new RouteRemoveJsonOwnership
                {
                    State = "blocked",
                    Framework = "trusted",
                    Extensions = "trusted",
                    Claims =
                    [
                        new RouteRemoveJsonOwnershipClaim
                        {
                            Path = ".agents/guidance/old guide.md",
                            Manager = "framework",
                            Owner = "route",
                        },
                    ],
                },
                References = new RouteRemoveJsonReferences
                {
                    Coverage = "complete",
                    ScannedSourceCount = 4,
                    InspectedSourceCount = 4,
                    OccurrenceCount = 1,
                    Detachments =
                    [
                        new RouteRemoveJsonReferenceDetachment
                        {
                            SourcePath = "README.md",
                            Layer = null,
                            Location = new RouteRemoveJsonSourceLocation
                            {
                                Line = 3,
                                Column = 5,
                                ByteOffset = 12,
                                ByteLength = 49,
                            },
                            Before = "See [Old guide](target.md).",
                            Expected = "See Old guide.",
                            OriginalDestination = "target.md",
                            VisibleLabel = "Old guide",
                        },
                    ],
                },
                GeneratedNavigation = new RouteRemoveJsonGeneratedNavigation
                {
                    Coverage = "complete",
                    Regions =
                    [
                        new RouteRemoveJsonGeneratedRegion
                        {
                            Path = ".agents/guidance/_guidance.md",
                            Reasons = ["old-parent"],
                            State = "changed",
                        },
                    ],
                },
                Plan = new RouteRemoveJsonPlan
                {
                    Completeness = "complete",
                    Safety = "blocked",
                },
                Effects =
                [
                    new RouteRemoveJsonEffect
                    {
                        Path = ".agents/guidance/old guide.md",
                        Kind = "removed-file",
                        Action = "delete",
                        Before = new RouteRemoveJsonPathState
                        {
                            Kind = "file",
                            ContentSha256 = new string('a', 64),
                        },
                        Expected = new RouteRemoveJsonPathState
                        {
                            Kind = "missing",
                            ContentSha256 = null,
                        },
                        Outcome = "planned",
                        Residual = "none",
                    },
                ],
                UnchangedPaths = [".agents/open-forge.lifecycle.json"],
                Recovery = new RouteRemoveJsonRecovery
                {
                    State = "not-created",
                    ProtectedPaths = ["recovery.zip"],
                    ResidualPath = null,
                },
                Verification = "not-requested",
                Findings =
                [
                    new RouteRemoveJsonFinding
                    {
                        Code = "route-remove.reference-unsafe",
                        Status = "blocked",
                        Target = "README.md",
                        Cause = "The incoming link cannot be detached safely.",
                    },
                ],
            },
            Next = includeNext
                ? new RouteRemoveJsonNext
                {
                    Command = "open-forge doctor",
                    Reason = "Inspect the blocked route.",
                }
                : null,
        };

    private static string Serialize(RouteRemoveJsonDocument document)
        => JsonSerializer.Serialize(
            document,
            RouteRemoveJsonContext.Default.RouteRemoveJsonDocument);

    private static void AssertOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
