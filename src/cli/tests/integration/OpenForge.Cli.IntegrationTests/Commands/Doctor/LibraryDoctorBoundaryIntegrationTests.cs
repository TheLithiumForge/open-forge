using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class LibraryDoctorBoundaryIntegrationTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordHasNoLibraryFindingOrOwnershipInference()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Link();
        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["doctor", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var domain = WorkspaceDomain(document);
        Assert.DoesNotContain(domain.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("kind").GetString()?.StartsWith("library.", StringComparison.Ordinal) == true);
        Assert.Equal("complete", domain.GetProperty("coverage").GetString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRegisteredSourceHasIncompleteDeclaredInventoryCoverage()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Link();
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["doctor", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var domain = WorkspaceDomain(document);
        Assert.Equal("incomplete", domain.GetProperty("coverage").GetString());
        Assert.Contains(domain.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("kind").GetString() == "library.inventory-incomplete");
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingProjectionIsManualDriftWithoutRepairEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["doctor", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var finding = Assert.Single(WorkspaceDomain(document).GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("kind").GetString() == "library.projection-missing");
        Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        Assert.Equal("library", finding.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create"), InlineData("record-replace"), InlineData("record-delete")]
    [InlineData("link-create"), InlineData("link-delete")]
    public static async Task TypedResidualFlowsThroughDoctorWithoutApplyingRecovery(string kind)
    {
        using var workspace = new LibraryResidualWorkspace(dangling: true);
        await workspace.PrepareAsync(kind);
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var run = await CliHostCapture.RunAsync(["doctor", "--json"], workspace.Files.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected Doctor result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        Assert.Equal(6, document.RootElement.GetProperty("result").GetProperty("domains").GetArrayLength());
        var finding = Assert.Single(WorkspaceDomain(document).GetProperty("findings").EnumerateArray(),
            value => value.GetProperty("kind").GetString() == "library.recovery-safe-exact");
        Assert.Equal("safe-exact", finding.GetProperty("resolution").GetString());
        Assert.Equal("library-residual-recovery", finding.GetProperty("proposal").GetProperty("kind").GetString());
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }

    private static JsonElement WorkspaceDomain(JsonDocument document)
        => Assert.Single(document.RootElement.GetProperty("result").GetProperty("domains").EnumerateArray(),
            domain => domain.GetProperty("domain").GetString() == "workspace-entry");
}
