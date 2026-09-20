using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

internal static class CleanupJsonAssertions
{
    internal static JsonElement Result(JsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var root = document.RootElement;
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        return root.GetProperty("data");
    }

    internal static JsonElement Result(CleanupIntegrationRun run)
    {
        Assert.NotEqual(string.Empty, run.StandardOutput);
        return Result(run.ParseJson());
    }

    internal static JsonElement Item(JsonElement result, string path)
        => Assert.Single(
            result.GetProperty("items").EnumerateArray(),
            item => string.Equals(
                item.GetProperty("path").GetString(),
                path,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));

    internal static JsonElement NotEligible(JsonElement result, string path)
        => Assert.Single(
            result.GetProperty("notEligible").EnumerateArray(),
            item => string.Equals(
                item.GetProperty("path").GetString(),
                path,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));

    internal static string[] Paths(JsonElement array)
        => [.. array.EnumerateArray()
            .Select(element => element.GetProperty("path").GetString()
                ?? throw new Xunit.Sdk.XunitException("Cleanup JSON path cannot be null."))];

    internal static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));

    internal static void RootPropertyOrder(JsonElement root)
        => PropertyOrder(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");

    internal static void AssertNoPersistentEffect(
        CleanupIntegrationWorkspace workspace,
        IReadOnlyDictionary<string, string> workspaceBefore,
        IReadOnlyDictionary<string, string> recoveryBefore)
    {
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
    }
}
