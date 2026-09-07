using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupJsonSchemaContractTests
{
    [Fact(DisplayName = "Cleanup JSON preserves the schema-v1 envelope and complete nested graph order"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void JsonPreservesEnvelopeAndNestedGraphOrder()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(
            CleanupTestData.JsonSample(),
            CleanupJsonContext.Default.CleanupJsonDocument));
        var root = document.RootElement;
        var result = root.GetProperty("result");
        var catalogue = result.GetProperty("catalogue");
        var candidate = catalogue.GetProperty("candidates").EnumerateArray().Single();
        var plan = result.GetProperty("plan");
        var planEntry = plan.GetProperty("entries").EnumerateArray().Single();
        var effect = result.GetProperty("effects").EnumerateArray().Single();
        var residual = result.GetProperty("residuals").EnumerateArray().Single();

        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertOrder(
            result,
            "mode", "catalogue", "plan", "preflight", "lease", "revalidation", "effects",
            "residuals", "verification", "findings");
        AssertOrder(catalogue, "coverage", "candidates");
        AssertOrder(
            candidate,
            "path", "kind", "integrity", "fileKind", "workspaceAssociation", "leaseBoundary",
            "provenance", "verification", "eligibility", "action", "cause");
        AssertOrder(
            plan,
            "safety", "entries");
        AssertOrder(
            planEntry,
            "ordinal", "path", "kind", "integrity", "fileKind", "workspaceAssociation",
            "leaseBoundary", "provenance", "verification", "eligibility", "action",
            "resultEffect", "cause");
        AssertOrder(
            result.GetProperty("preflight"),
            "state", "cause");
        AssertOrder(
            result.GetProperty("lease"),
            "state", "cause");
        AssertOrder(
            result.GetProperty("revalidation"),
            "state", "planned", "observed", "cause");
        AssertOrder(
            effect,
            "path", "kind", "integrity", "fileKind", "workspaceAssociation", "leaseBoundary",
            "provenance", "verification", "action", "outcome", "residual", "cause");
        AssertOrder(
            residual,
            "path", "kind", "integrity", "fileKind", "workspaceAssociation", "leaseBoundary",
            "provenance", "verification", "action", "outcome", "residual", "cause");
        AssertOrder(
            result.GetProperty("verification"),
            "state", "cause");
        AssertOrder(
            result.GetProperty("findings").EnumerateArray().Single(),
            "code", "status", "subject", "cause");
        AssertOrder(
            candidate.GetProperty("workspaceAssociation"),
            "state", "selectedPhysicalPath", "candidatePhysicalPath", "selectedWorkspaceKey",
            "candidateWorkspaceKey", "cause");
        AssertOrder(
            candidate.GetProperty("leaseBoundary"),
            "state", "workspaceKey", "command", "operationId", "cause");
        AssertOrder(
            candidate.GetProperty("provenance"),
            "producer", "operation", "subject", "command", "workspacePhysicalPath", "workspaceKey",
            "operationId");
        AssertOrder(candidate.GetProperty("provenance").GetProperty("subject"), "kind", "identity");
        AssertOrder(
            candidate.GetProperty("verification"),
            "state", "expectedPath", "expectedFileKind", "expectedIntegrity", "cause");
        AssertOrder(planEntry.GetProperty("resultEffect"), "outcome", "residual");
        AssertOrder(root.GetProperty("workspace"), "path", "selectedBy");
        AssertOrder(root.GetProperty("next"), "command", "reason");

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("complete", catalogue.GetProperty("coverage").GetString());
        Assert.Equal("eligible", candidate.GetProperty("eligibility").GetString());
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        Assert.Equal("open-forge cleanup", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Cleanup JSON keeps required nullable coordinates present as null"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void JsonKeepsRequiredNullableCoordinates()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(
            CleanupTestData.JsonSample(
                includeNext: false,
                includeWorkspace: false,
                includeProvenance: false),
            CleanupJsonContext.Default.CleanupJsonDocument));
        var root = document.RootElement;
        var result = root.GetProperty("result");
        var candidate = result
            .GetProperty("catalogue")
            .GetProperty("candidates")
            .EnumerateArray()
            .Single();
        var planEntry = result
            .GetProperty("plan")
            .GetProperty("entries")
            .EnumerateArray()
            .Single();
        var effect = result.GetProperty("effects").EnumerateArray().Single();
        var residual = result.GetProperty("residuals").EnumerateArray().Single();

        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal(JsonValueKind.Null, candidate.GetProperty("provenance").ValueKind);
        Assert.Equal(JsonValueKind.Null, planEntry.GetProperty("provenance").ValueKind);
        Assert.Equal(JsonValueKind.Null, effect.GetProperty("provenance").ValueKind);
        Assert.Equal(JsonValueKind.Null, residual.GetProperty("provenance").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("revalidation").GetProperty("planned").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("revalidation").GetProperty("observed").ValueKind);
        Assert.Equal(JsonValueKind.Null, candidate.GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, candidate.GetProperty("workspaceAssociation").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, candidate.GetProperty("leaseBoundary").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, candidate.GetProperty("verification").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("preflight").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("lease").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("revalidation").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("verification").GetProperty("cause").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("findings").EnumerateArray().Single().GetProperty("subject").ValueKind);
    }

    private static void AssertOrder(JsonElement element, params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
