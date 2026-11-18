using OpenForge.Cli.Core.Commands.Export;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Uninstall;

// The expected bytes under Fixtures/NeonPairs-6edb7de66 were copied from the eval harness's own output. Its source.txt
// records where they came from. scoped/ is the routed workspace's documents, and each other folder is the exact
// copy one export writes from it. Export shares uninstall's plan, so these bytes cover both commands.
public sealed class UninstallHarnessParityIntegrationTests
{
    private const string Docs = "docs";
    private const string CurrentFolder = "./";
    private const string FixtureName = "NeonPairs-6edb7de66";
    private const string Scoped = "scoped";

    // Each expected folder is named after the parts its export strips.
    private static readonly StripPart[][] PartSets = [[StripPart.Metadata], [StripPart.Metadata, StripPart.Links]];

    public static TheoryData<int> Runs => [.. Enumerable.Range(0, PartSets.Length)];

    [Theory(DisplayName = "Export with stripping and flatten writes the expected bytes from a routed, scoped workspace and keeps the project"), Trait("Feature", "export"), Trait("Evidence", "Integration")]
    [MemberData(nameof(Runs))]
    public async Task UninstallMatchesTheExpectedBytes(int run)
    {
        var parts = PartSets[run];
        var setup = string.Join('-', [StrippingDefinitions.StripOption.TrimStart('-'), .. parts.Select(StrippingDefinitions.PartName)]);
        using var workspace = StripIntegrationWorkspace.Create($"uninstall-harness-{setup}");
        foreach (var (path, bytes) in Fixture(Scoped))
        {
            workspace.WriteBytes(path, bytes);
        }

        await ForgeAsync(workspace);
        var before = workspace.SnapshotHashes();

        var completion = await workspace.RunAsync([
            ExportDefinitions.CommandIdentity,
            workspace.Destination,
            StrippingDefinitions.StripOption,
            string.Join(StrippingDefinitions.PartSeparator, parts.Select(StrippingDefinitions.PartName)),
            StrippingDefinitions.FlattenOption,
            Docs]);

        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        var expected = Fixture(setup);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), workspace.ExportFiles());
        Assert.All(expected, pair => Assert.Equal(pair.Value, workspace.ReadExportBytes(pair.Key)));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    // The harness's forge requests at 6edb7de66.
    private static async Task ForgeAsync(StripIntegrationWorkspace workspace)
    {
        string[][] requests =
        [
            [InstallDefinitions.CommandIdentity, InstallDefinitions.PresetOption, InstallConfigurationChoices.Name(InstallPreset.Minimal),
                InstallDefinitions.Frontmatter.Name, WorkspaceSettingsDefinitions.FrontmatterScopedName, InstallDefinitions.Automatic.Name],
            [RouteDefinitions.RouteGroup.Name, RouteIncludeDefinitions.Command.Name, CurrentFolder + Docs,
                RouteIncludeDefinitions.Tag.Name, SourceLoadingClosureResolver.LoadNowTag],
            [IndexDefinitions.CommandIdentity, $"{CurrentFolder}{Docs}/_{Docs}.md"],
        ];
        foreach (var request in requests)
        {
            Assert.Equal(CliSemanticStatus.Complete, (await workspace.RunAsync(request)).Status);
        }
    }

    private static Dictionary<string, byte[]> Fixture(string setup)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "OpenForge.Cli.slnx")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var folder = Path.Combine(root.FullName, "src", "cli", "tests", "integration", "OpenForge.Cli.IntegrationTests",
            "Commands", "Uninstall", "Fixtures", FixtureName, setup);
        return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).ToDictionary(
            file => Path.GetRelativePath(folder, file).Replace(Path.DirectorySeparatorChar, '/'),
            File.ReadAllBytes,
            StringComparer.Ordinal);
    }
}
