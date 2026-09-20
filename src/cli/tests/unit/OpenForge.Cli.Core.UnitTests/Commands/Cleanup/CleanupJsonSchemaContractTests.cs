using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Cleanup;
using OpenForge.Cli.Core.Presentation.Cleanup.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupJsonSchemaContractTests
{
    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup JSON preserves native data order and minimal nullable boundaries"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "UnitContract")]
    public void JsonPreservesNativeDataOrder()
    {
        var data = new CleanupData
        {
            Mode = "dry-run",
            Items =
            [
                new CleanupDataItem
                {
                    Path = "recovery.bundle",
                    Kind = "bundle",
                    Outcome = "would-be-removed",
                },
            ],
        };

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(data, CleanupPresentation.Rendering.DataJsonTypeInfo));
        var root = document.RootElement;

        Assert.Equal(
            ["mode", "items"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", root.GetProperty("mode").GetString());
        var item = Assert.Single(root.GetProperty("items").EnumerateArray());
        Assert.Equal(["path", "kind", "outcome"], item.EnumerateObject().Select(property => property.Name));
        Assert.False(item.TryGetProperty("origin", out _));
        Assert.False(item.TryGetProperty("integrity", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup JSON adds standard and full data without the retired nested graph"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "UnitContract")]
    public void JsonAddsStandardAndFullData()
    {
        var data = new CleanupData
        {
            Mode = "apply",
            Items =
            [
                new CleanupDataItem
                {
                    Path = "recovery.bundle",
                    Kind = "bundle",
                    Outcome = "removed",
                    Origin = "index",
                    Integrity = "verified",
                },
            ],
            NotEligible =
            [
                new CleanupDataNotEligible
                {
                    Path = "damaged.bundle",
                    Reason = "not recognized",
                },
            ],
            Lock = "The workspace lock was acquired before removal.",
            FinalCheck = "The recovery store matched the planned items under the workspace lock.",
        };

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(data, CleanupPresentation.Rendering.DataJsonTypeInfo));
        var root = document.RootElement;
        Assert.Equal(
            ["mode", "items", "notEligible", "lock", "finalCheck"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("index", root.GetProperty("items")[0].GetProperty("origin").GetString());
        Assert.Equal("verified", root.GetProperty("items")[0].GetProperty("integrity").GetString());
        Assert.Equal("not recognized", root.GetProperty("notEligible")[0].GetProperty("reason").GetString());
        Assert.False(root.GetProperty("items")[0].TryGetProperty("catalogue", out _));
        Assert.False(root.TryGetProperty("preflight", out _));
        Assert.False(root.TryGetProperty("verification", out _));
    }
}
