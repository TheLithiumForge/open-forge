using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

internal static class CleanupJsonAssertions
{
    internal static JsonElement Result(JsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var root = document.RootElement;
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        return root.GetProperty("result");
    }

    internal static JsonElement Result(CleanupIntegrationRun run)
    {
        Assert.NotEqual(string.Empty, run.StandardOutput);
        return Result(run.ParseJson());
    }

    internal static JsonElement Candidate(
        JsonElement result,
        string path)
        => Assert.Single(
            result.GetProperty("catalogue").GetProperty("candidates").EnumerateArray(),
            candidate => string.Equals(
                candidate.GetProperty("path").GetString(),
                path,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));

    internal static JsonElement PlanEntry(
        JsonElement result,
        string path)
        => Assert.Single(
            result.GetProperty("plan").GetProperty("entries").EnumerateArray(),
            entry => string.Equals(
                entry.GetProperty("path").GetString(),
                path,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));

    internal static string[] Paths(
        JsonElement array)
        => [.. array.EnumerateArray()
            .Select(element => element.GetProperty("path").GetString()
                ?? throw new Xunit.Sdk.XunitException("Cleanup JSON path cannot be null."))];

    internal static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));

    internal static void RootPropertyOrder(JsonElement root)
        => PropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");

    internal static void ResultPropertyOrder(JsonElement result)
        => PropertyOrder(
            result,
            "mode",
            "catalogue",
            "plan",
            "preflight",
            "lease",
            "revalidation",
            "effects",
            "residuals",
            "verification",
            "findings");

    internal static void AssertCandidate(
        JsonElement candidate,
        string kind,
        string integrity,
        string fileKind,
        string eligibility,
        string action)
    {
        PropertyOrder(
            candidate,
            "path",
            "kind",
            "integrity",
            "fileKind",
            "workspaceAssociation",
            "leaseBoundary",
            "provenance",
            "verification",
            "eligibility",
            "action",
            "cause");
        Assert.Equal(kind, candidate.GetProperty("kind").GetString());
        Assert.Equal(integrity, candidate.GetProperty("integrity").GetString());
        Assert.Equal(fileKind, candidate.GetProperty("fileKind").GetString());
        Assert.Equal(eligibility, candidate.GetProperty("eligibility").GetString());
        Assert.Equal(action, candidate.GetProperty("action").GetString());
    }

    internal static void AssertDryRunApplicationParity(
        JsonElement dryResult,
        JsonElement appliedResult)
    {
        var normalizedDryRun = NormalizeForApplicationParity(dryResult);
        var normalizedApplication = NormalizeForApplicationParity(appliedResult);
        Assert.True(
            JsonNode.DeepEquals(normalizedDryRun, normalizedApplication),
            "Dry-run and application differ in a contract-owned planning fact.");
    }

    internal static void AssertNoPersistentEffect(
        CleanupIntegrationWorkspace workspace,
        IReadOnlyDictionary<string, string> workspaceBefore,
        IReadOnlyDictionary<string, string> recoveryBefore)
    {
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
    }

    private static JsonObject NormalizeForApplicationParity(JsonElement result)
    {
        var normalized = JsonNode.Parse(result.GetRawText())?.AsObject()
            ?? throw new Xunit.Sdk.XunitException(
                "Cleanup result parity requires a JSON object.");
        foreach (var propertyName in new[]
        {
            "mode",
            "lease",
            "revalidation",
            "effects",
            "residuals",
            "verification",
        })
        {
            normalized.Remove(propertyName);
        }

        var entries = normalized["plan"]?.AsObject()?["entries"]?.AsArray();
        if (entries is not null)
        {
            foreach (var entry in entries)
            {
                entry?.AsObject()?.Remove("resultEffect");
            }
        }

        return normalized;
    }
}
