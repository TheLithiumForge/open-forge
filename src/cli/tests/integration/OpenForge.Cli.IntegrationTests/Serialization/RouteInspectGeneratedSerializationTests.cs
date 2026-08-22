using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class RouteInspectGeneratedSerializationTests
{
    [Fact(DisplayName = "Route Inspect JSON document uses generated metadata for the complete presentation graph")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public void InspectDocumentUsesGeneratedMetadata()
    {
        var document = new RouteInspectJsonDocument
        {
            SchemaVersion = 1,
            Command = "route inspect",
            Status = "incomplete",
            Workspace = new RouteInspectJsonWorkspace
            {
                Path = "C:/workspace",
                SelectedBy = "explicit-workspace",
            },
            Result = new RouteInspectJsonResult
            {
                Selection = new RouteInspectJsonSelection
                {
                    ReferenceKind = "source-id",
                    SelectionMethod = "automatic-id",
                    RequestedReference = "root",
                    CandidatePaths = [],
                },
                Identity = new RouteInspectJsonIdentity
                {
                    Id = "root",
                    Path = ".agents/root/_root.md",
                    SourceKind = "entrypoint",
                    SourceForm = "canonical",
                    RouteState = "routed",
                    PhysicalLayers =
                    [
                        new RouteInspectJsonPhysicalLayer
                        {
                            WorkspaceRelativePath = ".agents/root/_root.md",
                            PhysicalPath = "C:/workspace/.agents/root/_root.md",
                            Role = "base",
                        },
                        new RouteInspectJsonPhysicalLayer
                        {
                            WorkspaceRelativePath = ".agents/root/_root.overwrite.md",
                            PhysicalPath = "C:/workspace/.agents/root/_root.overwrite.md",
                            Role = "overwrite",
                        },
                    ],
                },
                Profile = new RouteInspectJsonProfile
                {
                    Reading = new RouteInspectJsonReading
                    {
                        TaskStart = new RouteInspectJsonBooleanFact
                        {
                            State = "value",
                            Value = true,
                            Reason = null,
                        },
                        Automatic = new RouteInspectJsonFact<RouteInspectJsonAutomaticReadings>
                        {
                            State = "value",
                            Value = new RouteInspectJsonAutomaticReadings
                            {
                                Reasons =
                                [
                                    new RouteInspectJsonAutomaticReading
                                    {
                                        Kind = "parent-load-now",
                                        RelatedSourceId = "loader",
                                        Events = ["exposing-parent-read"],
                                    },
                                ],
                            },
                            Reason = null,
                        },
                        Later = new RouteInspectJsonFact<RouteInspectJsonLaterReading>
                        {
                            State = "value",
                            Value = new RouteInspectJsonLaterReading
                            {
                                MayBeReadAgain = true,
                                Occasions = ["context-restoration", "handoff", "closeout"],
                            },
                            Reason = null,
                        },
                    },
                    Measurements = new RouteInspectJsonMeasurements
                    {
                        OwnSource = UnavailableMeasurementFact(),
                        SelectedClosure = MeasurementFact(),
                        TaskStartOverlap = MeasurementFact(),
                        SelectionAddition = MeasurementFact(),
                        LoadNowDescendants = MeasurementFact(),
                    },
                    Topology = new RouteInspectJsonFact<RouteInspectJsonTopology>
                    {
                        State = "value",
                        Value = new RouteInspectJsonTopology
                        {
                            RootRoute = "root",
                            RouteChain = ["root"],
                            ParentId = null,
                            Depth = 1,
                            Counts = new RouteInspectJsonFact<RouteInspectJsonTopologyCounts>
                            {
                                State = "value",
                                Value = new RouteInspectJsonTopologyCounts
                                {
                                    DirectRoutedFileCount = 0,
                                    DirectEntrypointCount = 0,
                                    DescendantRoutedFileCount = 0,
                                    DescendantEntrypointCount = 0,
                                },
                                Reason = null,
                            },
                        },
                        Reason = null,
                    },
                    Axioms = new RouteInspectJsonFact<RouteInspectJsonAxioms>
                    {
                        State = "value",
                        Value = new RouteInspectJsonAxioms
                        {
                            Inherited = new RouteInspectJsonFact<RouteInspectJsonAxiomsSources>
                            {
                                State = "value",
                                Value = new RouteInspectJsonAxiomsSources { SourceIds = ["loader"] },
                                Reason = null,
                            },
                            Local = new RouteInspectJsonFact<string>
                            {
                                State = "value",
                                Value = "substantive",
                                Reason = null,
                            },
                        },
                        Reason = null,
                    },
                    Completeness = "incomplete",
                    Safety = "safe",
                },
                Observations =
                [
                    new RouteInspectJsonObservation
                    {
                        Code = "route-inspect.valid-overwrite",
                        Subject = ".agents/root/_root.md",
                        Message = "A valid overwrite accompanies the base source.",
                        Paths =
                        [
                            ".agents/root/_root.md",
                            ".agents/root/_root.overwrite.md",
                        ],
                    },
                ],
                Conditions =
                [
                    new RouteInspectJsonCondition
                    {
                        Code = "route-inspect.unavailable-fact",
                        Status = "incomplete",
                        Subject = ".agents/root/_root.md",
                        Message = "The own-source measurement is unavailable.",
                        Paths = [".agents/root/_root.md"],
                    },
                ],
            },
            Next = new RouteInspectJsonNext
            {
                Command = "open-forge doctor",
                Reason = "Review the unavailable route fact, then rerun route inspect.",
            },
        };

        var metadata = CliJsonContext.Default.RouteInspectJsonDocument;
        Assert.Equal(typeof(RouteInspectJsonDocument), metadata.Type);
        var json = JsonSerializer.Serialize(document, metadata);

        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            parsed.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("route inspect", parsed.RootElement.GetProperty("command").GetString());
        Assert.Equal("incomplete", parsed.RootElement.GetProperty("status").GetString());
        Assert.Equal("root", parsed.RootElement.GetProperty("result").GetProperty("selection")
            .GetProperty("requestedReference").GetString());
        var layers = parsed.RootElement.GetProperty("result").GetProperty("identity")
            .GetProperty("physicalLayers").EnumerateArray().ToArray();
        Assert.Equal(2, layers.Length);
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            layers.Select(layer => layer.GetProperty("workspaceRelativePath").GetString()));
        Assert.Equal(
            [
                "C:/workspace/.agents/root/_root.md",
                "C:/workspace/.agents/root/_root.overwrite.md",
            ],
            layers.Select(layer => layer.GetProperty("physicalPath").GetString()));
        Assert.Equal(
            ["base", "overwrite"],
            layers.Select(layer => layer.GetProperty("role").GetString()));
        var observation = Assert.Single(
            parsed.RootElement.GetProperty("result").GetProperty("observations").EnumerateArray());
        Assert.Equal("route-inspect.valid-overwrite", observation.GetProperty("code").GetString());
        Assert.Equal(
            ".agents/root/_root.overwrite.md",
            observation.GetProperty("paths")[1].GetString());
        var condition = Assert.Single(
            parsed.RootElement.GetProperty("result").GetProperty("conditions").EnumerateArray());
        Assert.Equal("route-inspect.unavailable-fact", condition.GetProperty("code").GetString());
        Assert.Equal("The own-source measurement is unavailable.", condition.GetProperty("message").GetString());
        var next = parsed.RootElement.GetProperty("next");
        Assert.Equal("open-forge doctor", next.GetProperty("command").GetString());
        Assert.Equal(
            "Review the unavailable route fact, then rerun route inspect.",
            next.GetProperty("reason").GetString());
        Assert.Equal(
            "parent-load-now",
            parsed.RootElement.GetProperty("result").GetProperty("profile").GetProperty("reading")
                .GetProperty("automatic").GetProperty("value").GetProperty("reasons")[0]
                .GetProperty("kind").GetString());
    }

    private static RouteInspectJsonFact<RouteInspectJsonMeasurement> MeasurementFact()
    {
        return new RouteInspectJsonFact<RouteInspectJsonMeasurement>
        {
            State = "value",
            Value = new RouteInspectJsonMeasurement
            {
                PhysicalFileCount = 1,
                UnicodeScalarCount = 4,
                Utf8ByteCount = 4,
                EstimatedTokens = 1,
            },
            Reason = null,
        };
    }

    private static RouteInspectJsonFact<RouteInspectJsonMeasurement> UnavailableMeasurementFact()
    {
        return new RouteInspectJsonFact<RouteInspectJsonMeasurement>
        {
            State = "unavailable",
            Value = null,
            Reason = "The own-source measurement is unavailable.",
        };
    }
}
