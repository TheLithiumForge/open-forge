using System.Text.Json;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusJsonRenderingTests
{
    [Fact(DisplayName = "Status schema-v1 serializes the exact graph order presence and nullability"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void SchemaV1SerializesExactStatusGraphOrderPresenceAndNullability()
    {
        var result = StatusResultSeeds.Representative(CliSemanticStatus.Incomplete, StatusDefinitions.IncompleteNextAction);
        var extensions = result.Facts.Lifecycle.Extensions;
        var installed = extensions.Installed[0] with
        {
            Version = null,
            Source = null,
            SourceAvailability = OperationalSourceAvailability.Unavailable,
        };
        result = result with
        {
            Facts = result.Facts with
            {
                Lifecycle = result.Facts.Lifecycle with
                {
                    Extensions = extensions with
                    {
                        SourceAvailability = OperationalSourceAvailability.Unavailable,
                        Installed = [installed],
                    },
                },
                Recovery = result.Facts.Recovery with
                {
                    IncompleteDrafts = StatusResultSeeds.Available(1),
                    Candidates =
                    [
                        new StatusRecoveryCandidate(
                            "/recovery/draft.tmp",
                            RecoveryBundleCandidateKind.Draft,
                            RecoveryBundleIntegrity.Incomplete),
                    ],
                },
            },
            Findings =
            [
                new StatusFinding
                {
                    Code = StatusFindingCode.ExtensionSourceUnavailable,
                    Status = CliSemanticStatus.Incomplete,
                    Subject = null,
                    Cause = "The installed Extension source is unavailable.",
                },
                new StatusFinding
                {
                    Code = StatusFindingCode.RecoveryDraftIncomplete,
                    Status = CliSemanticStatus.Incomplete,
                    Subject = null,
                    Cause = "The exact recovery draft is incomplete.",
                },
            ],
        };
        var json = StatusJsonRenderer.Render(new CliPresentationRequest<StatusResult>(
            result,
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        var commandResult = root.GetProperty("result");
        var context = commandResult.GetProperty("context");
        var startup = context.GetProperty("startup");
        var lifecycle = commandResult.GetProperty("lifecycle");
        var framework = lifecycle.GetProperty("framework");
        var extensionsJson = lifecycle.GetProperty("extensions");
        var managedFiles = extensionsJson.GetProperty("managedFiles");
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        AssertPropertyOrder(commandResult, "installation", "context", "structure", "lifecycle", "library", "recovery", "findings");
        AssertPropertyOrder(commandResult.GetProperty("installation"), "state", "entryPath", "loaderPath");
        AssertPropertyOrder(
            context,
            "tokenEstimator",
            "startup",
            "totalAvailable",
            "startupPercentage",
            "continuity",
            "continuitySources");
        AssertPropertyOrder(startup, "initial", "current", "difference");
        AssertMeasurementOrder(startup.GetProperty("initial"));
        AssertMeasurementOrder(startup.GetProperty("current"));
        AssertMeasurementOrder(startup.GetProperty("difference"));
        AssertMeasurementOrder(context.GetProperty("totalAvailable"));
        AssertPropertyOrder(context.GetProperty("startupPercentage"), "state", "value");
        AssertMeasurementOrder(context.GetProperty("continuity"));
        var continuitySources = context.GetProperty("continuitySources").EnumerateArray().ToArray();
        Assert.Equal(2, continuitySources.Length);
        AssertPropertyOrder(continuitySources[0], "sourceId", "utf8Bytes", "layers");
        AssertPropertyOrder(continuitySources[0].GetProperty("layers")[0], "path", "utf8Bytes");
        var structure = commandResult.GetProperty("structure");
        AssertPropertyOrder(structure, "rootCategories", "generatedNavigation");
        AssertPropertyOrder(structure.GetProperty("rootCategories"), "count", "added", "removed");
        AssertPropertyOrder(structure.GetProperty("generatedNavigation")[0], "path", "state");
        AssertPropertyOrder(lifecycle, "framework", "extensions");
        AssertPropertyOrder(framework, "state", "sourceAvailability", "targets");
        AssertPropertyOrder(
            framework.GetProperty("targets")[0],
            "path",
            "kind",
            "sourceAssetPath",
            "region",
            "baselineFingerprint",
            "fingerprintKind",
            "state");
        AssertPropertyOrder(extensionsJson, "state", "sourceAvailability", "installed", "managedFiles");
        AssertPropertyOrder(
            extensionsJson.GetProperty("installed")[0],
            "id",
            "version",
            "source",
            "sourceAvailability",
            "dependencies",
            "paths");
        AssertPropertyOrder(managedFiles, "counts", "targets");
        AssertPropertyOrder(managedFiles.GetProperty("counts"), "current", "changed", "missing", "unavailable", "blocked");
        AssertPropertyOrder(
            managedFiles.GetProperty("targets")[0],
            "path",
            "owners",
            "baselineFingerprint",
            "fingerprintKind",
            "state");
        var library = commandResult.GetProperty("library");
        AssertPropertyOrder(library, "state", "record", "records", "counts");
        AssertPropertyOrder(library.GetProperty("record"), "path", "state");
        AssertPropertyOrder(
            library.GetProperty("counts"),
            "registered",
            "current",
            "missing",
            "changed",
            "blocked",
            "unavailable");
        var recovery = commandResult.GetProperty("recovery");
        AssertPropertyOrder(recovery, "verifiedFinals", "incompleteDrafts", "candidates");
        AssertPropertyOrder(recovery.GetProperty("candidates")[0], "path", "kind", "integrity");
        AssertPropertyOrder(commandResult.GetProperty("findings")[0], "code", "status", "subject", "cause");
        AssertPropertyOrder(root.GetProperty("next"), "command", "reason");
        Assert.Equal(JsonValueKind.Null, framework.GetProperty("targets")[0].GetProperty("region").ValueKind);
        Assert.Equal(JsonValueKind.Null, extensionsJson.GetProperty("installed")[0].GetProperty("version").ValueKind);
        Assert.Equal(JsonValueKind.Null, extensionsJson.GetProperty("installed")[0].GetProperty("source").ValueKind);
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("findings")[0].GetProperty("subject").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, commandResult.ValueKind);
        Assert.NotEqual(JsonValueKind.Null, commandResult.GetProperty("findings").ValueKind);
    }

    [Fact(DisplayName = "Status JSON preserves finite numeric and collection facts without substitution"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesFiniteNumericAndCollectionFactsWithoutSubstitution()
    {
        var result = StatusResultSeeds.Representative(CliSemanticStatus.Attention, StatusDefinitions.AttentionNextAction);
        var difference = new StatusMeasurement(
            new StatusIntegerValue(OperationalValueState.Available, 0),
            new StatusIntegerValue(OperationalValueState.Available, -12),
            new StatusIntegerValue(OperationalValueState.Unavailable, null),
            new StatusIntegerValue(OperationalValueState.NotApplicable, null));
        result = result with
        {
            Facts = result.Facts with
            {
                Context = result.Facts.Context with
                {
                    Startup = result.Facts.Context.Startup with { Difference = difference },
                },
            },
        };
        var document = StatusJsonProjection.Create(result);
        var projected = document.Result.Context.Startup.Difference;
        Assert.Equal("attention", document.Status);
        Assert.Equal("available", projected.Files.State);
        Assert.Equal(0, projected.Files.Value);
        Assert.Equal("available", projected.Characters.State);
        Assert.Equal(-12, projected.Characters.Value);
        Assert.Equal("unavailable", projected.Utf8Bytes.State);
        Assert.Null(projected.Utf8Bytes.Value);
        Assert.Equal("not-applicable", projected.EstimatedTokens.State);
        Assert.Null(projected.EstimatedTokens.Value);
        Assert.NotNull(document.Result.Context.ContinuitySources);
        Assert.NotNull(document.Result.Structure.GeneratedNavigation);
        Assert.NotNull(document.Result.Lifecycle.Framework.Targets);
        Assert.NotNull(document.Result.Lifecycle.Extensions.Installed);
        Assert.NotNull(document.Result.Lifecycle.Extensions.ManagedFiles.Targets);
        var candidate = Assert.Single(document.Result.Recovery.Candidates);
        Assert.Equal("/recovery/final.zip", candidate.Path);
        Assert.Equal("final", candidate.Kind);
        Assert.Equal("verified", candidate.Integrity);
    }

    [Theory, Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    [InlineData((int)StatusFindingCode.InvalidInput, "invalid")]
    [InlineData((int)StatusFindingCode.OperationFailed, "failed")]
    [InlineData((int)StatusFindingCode.Interrupted, "interrupted")]
    public void EventJsonRetainsAnHonestUnavailableLibraryGraph(int findingCode, string expectedStatus)
    {
        var result = StatusResultBuilder.Event(
            workspace: null,
            (StatusFindingCode)findingCode,
            subject: null,
            "A bounded Status event occurred.");
        var document = StatusJsonProjection.Create(result);
        var library = document.Result.Library;

        Assert.Equal(expectedStatus, document.Status);
        Assert.NotNull(result.Facts.Library);
        Assert.Equal("incomplete", library.State);
        Assert.Equal("unavailable", library.Record.State);
        Assert.Empty(library.Records);
        foreach (var count in new[]
        {
            library.Counts.Registered,
            library.Counts.Current,
            library.Counts.Missing,
            library.Counts.Changed,
            library.Counts.Blocked,
            library.Counts.Unavailable,
        })
        {
            Assert.Equal("unavailable", count.State);
            Assert.Null(count.Value);
        }
    }

    private static void AssertMeasurementOrder(JsonElement measurement)
    {
        AssertPropertyOrder(measurement, "files", "characters", "utf8Bytes", "estimatedTokens");
        foreach (var property in measurement.EnumerateObject())
        {
            AssertPropertyOrder(property.Value, "state", "value");
        }
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
