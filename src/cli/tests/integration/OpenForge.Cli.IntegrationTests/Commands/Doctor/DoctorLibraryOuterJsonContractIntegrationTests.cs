using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorLibraryOuterJsonContractIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create")]
    [InlineData("link-delete")]
    public static async Task OuterJsonPreservesExactSchemaWithoutRichInternalPayloads(string kind)
    {
        using var workspace = new LibraryResidualWorkspace(dangling: true);
        await workspace.PrepareAsync(kind);

        var run = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.Files.Path);

        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected Doctor result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var category = Assert.Single(
            document.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray(),
            value => value.GetProperty("name").GetString() == "Workspace");
        Assert.Equal("complete", category.GetProperty("coverage").GetString());
        var finding = Assert.Single(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            value => value.GetProperty("code").GetString() == "library.recovery-safe-exact");
        var subject = finding.GetProperty("subject");
        PropertyOrder(subject, "kind", "path", "id", "location");
        Assert.Equal("file", subject.GetProperty("kind").GetString());
        Assert.Equal("safe-exact", finding.GetProperty("resolution").GetString());
        Assert.Contains(
            finding.GetProperty("actions").EnumerateArray(),
            action => action.GetProperty("command").GetString() == "open-forge repair --automatic");
        Assert.Equal(1, category.GetProperty("lanes").GetProperty("safeExact").GetInt32());
    }

    private static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
