using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateScaffoldIntegrationTests
{
    [Fact(DisplayName = "Extension Create writes only the exact manifest and empty payload agents scaffold"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ApplyCreatesExactScaffoldAndStrictManifest()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-scaffold");
        var destination = catalogue.Combine("development-toolkit");
        try
        {
            var result = await ExecuteAsync(
                cataloguePath: catalogue.Path,
                stableId: "development-toolkit",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.True(Directory.Exists(destination));
            Assert.True(File.Exists(Path.Combine(destination, "extension.json")));
            Assert.True(Directory.Exists(Path.Combine(destination, "payload", ".agents")));
            Assert.Equal(
                ["extension.json", "payload"],
                Directory.EnumerateFileSystemEntries(destination).Select(Path.GetFileName).Order(StringComparer.Ordinal));
            Assert.Empty(Directory.EnumerateFileSystemEntries(Path.Combine(destination, "payload", ".agents")));
            Assert.False(File.Exists(Path.Combine(destination, "README.md")));
            Assert.False(File.Exists(Path.Combine(destination, "payload", "content.md")));

            var manifestBytes = await File.ReadAllBytesAsync(
                Path.Combine(destination, "extension.json"),
                TestContext.Current.CancellationToken);
            using var json = JsonDocument.Parse(manifestBytes);
            Assert.Equal(
                ["id", "name", "description", "version", "dependencies"],
                json.RootElement.EnumerateObject().Select(property => property.Name));
            var read = ExtensionManifestReader.Read(manifestBytes, "extension.json", []);
            Assert.Equal("development-toolkit", read.Id);
            Assert.Equal("Development Toolkit", read.Name);
            Assert.Equal("Open Forge Extension package development-toolkit.", read.Description);
            Assert.Equal("0.1.0", read.Version);
            Assert.Empty(read.Dependencies);
        }
        finally
        {
            DeleteDestination(destination);
        }
    }

    [Fact(DisplayName = "Extension Create dry-run forms the apply plan without creating destination state"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task DryRunDoesNotWrite()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-scaffold-dry-run");
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            cataloguePath: catalogue.Path,
            stableId: "development-toolkit",
            mode: ExtensionCreateMode.DryRun,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ExtensionCreateMode.DryRun, result.Mode);
        Assert.Equal(2, result.IntendedEffects.Count);
        Assert.Empty(result.AppliedEffects);
        Assert.Equal(before, catalogue.SnapshotHashes());
        Assert.False(Directory.Exists(catalogue.Combine("development-toolkit")));
    }

    [Fact(DisplayName = "Extension Create treats an exact scaffold as a verified no-op and remains byte-stable on the second apply"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ExactScaffoldIsVerifiedNoOpAndRepeatIsStable()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-no-op");
        var destination = catalogue.Combine("development-toolkit");
        var manifest = "{\"id\":\"development-toolkit\",\"name\":\"Development Toolkit\",\"description\":\"Open Forge Extension package development-toolkit.\",\"version\":\"0.1.0\",\"dependencies\":[]}";
        catalogue.CreateFile("development-toolkit/extension.json", manifest);
        catalogue.CreateDirectory("development-toolkit/payload/.agents");
        var before = catalogue.SnapshotHashes();
        try
        {
            var first = await ExecuteAsync(
                cataloguePath: catalogue.Path,
                stableId: "development-toolkit",
                cancellationToken: TestContext.Current.CancellationToken);
            var afterFirst = catalogue.SnapshotHashes();
            var second = await ExecuteAsync(
                cataloguePath: catalogue.Path,
                stableId: "development-toolkit",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, first.Status);
            Assert.Equal(CliSemanticStatus.Complete, second.Status);
            Assert.Empty(first.AppliedEffects);
            Assert.Empty(second.AppliedEffects);
            Assert.Equal(before, afterFirst);
            Assert.Equal(afterFirst, catalogue.SnapshotHashes());
            Assert.Equal(manifest, File.ReadAllText(Path.Combine(destination, "extension.json")));
        }
        finally
        {
            DeleteDestination(destination);
        }
    }

    [Theory(DisplayName = "Extension Create blocks every divergent or colliding destination without overwriting existing bytes"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("divergent-manifest")]
    [InlineData("partial-scaffold")]
    [InlineData("additional-entry")]
    [InlineData("payload-content")]
    [InlineData("destination-file")]
    [InlineData("manifest-directory")]
    [InlineData("case-alias")]
    [InlineData("unicode-lookalike")]
    public async Task DivergentDestinationsAreBlocked(string scenario)
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-collisions");
        var destination = catalogue.Combine("development-toolkit");
        switch (scenario)
        {
            case "divergent-manifest":
                catalogue.CreateFile("development-toolkit/extension.json", "{\"id\":\"different\"}");
                catalogue.CreateDirectory("development-toolkit/payload/.agents");
                break;
            case "partial-scaffold":
                catalogue.CreateFile("development-toolkit/extension.json", "{}");
                break;
            case "additional-entry":
                CreateExactScaffold(catalogue);
                catalogue.CreateFile("development-toolkit/extra.txt", "retain");
                break;
            case "payload-content":
                CreateExactScaffold(catalogue);
                catalogue.CreateFile("development-toolkit/payload/.agents/content.md", "retain");
                break;
            case "destination-file":
                catalogue.CreateFile("development-toolkit", "retain");
                break;
            case "manifest-directory":
                catalogue.CreateDirectory("development-toolkit/extension.json");
                break;
            case "case-alias":
                catalogue.CreateFile("development-toolkit/Extension.json", "retain");
                catalogue.CreateDirectory("development-toolkit/payload/.agents");
                break;
            case "unicode-lookalike":
                catalogue.CreateFile("development-toolkit/extensiоn.json", "retain");
                catalogue.CreateDirectory("development-toolkit/payload/.agents");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The collision scenario is not defined.");
        }

        var before = catalogue.SnapshotHashes();
        var retained = Directory.Exists(destination)
            ? Directory.EnumerateFileSystemEntries(destination).ToArray()
            : [];
        var result = await ExecuteAsync(
            cataloguePath: catalogue.Path,
            stableId: "development-toolkit",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, catalogue.SnapshotHashes());
        if (retained.Length > 0)
        {
            Assert.Equal(retained.Order(StringComparer.Ordinal), Directory.EnumerateFileSystemEntries(destination).Order(StringComparer.Ordinal));
        }

        DeleteDestination(destination);
    }

    [Fact(DisplayName = "Extension Create rejects an external destination link without adopting the linked package"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ExternalDestinationLinkIsBlocked()
    {
        using var external = TemporaryWorkspace.Create("extension-create-external-destination");
        using var catalogue = TemporaryWorkspace.Create("extension-create-linked-destination");
        var destinationLink = catalogue.CreateDirectorySymbolicLink("development-toolkit", external.Path);
        var beforeExternal = external.SnapshotHashes();
        var beforeCatalogue = catalogue.SnapshotHashes();

        var result = await ExecuteAsync(
            cataloguePath: catalogue.Path,
            stableId: "development-toolkit",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(beforeExternal, external.SnapshotHashes());
        Assert.Equal(beforeCatalogue, catalogue.SnapshotHashes());
        Assert.Equal(external.Path, new DirectoryInfo(destinationLink).LinkTarget);
        Assert.Empty(external.SnapshotHashes());
    }

    private static void CreateExactScaffold(TemporaryWorkspace catalogue)
    {
        catalogue.CreateFile(
            "development-toolkit/extension.json",
            "{\"id\":\"development-toolkit\",\"name\":\"Development Toolkit\",\"description\":\"Open Forge Extension package development-toolkit.\",\"version\":\"0.1.0\",\"dependencies\":[]}");
        catalogue.CreateDirectory("development-toolkit/payload/.agents");
    }

    private static async Task<ExtensionCreateResult> ExecuteAsync(
        string cataloguePath,
        string stableId,
        ExtensionCreateMode mode = ExtensionCreateMode.Apply,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(string.Empty);
        using var prompts = new StringWriter();
        var session = new CliInteractiveSession(reader, prompts, canPrompt: false);
        var request = new ExtensionCreateRequest
        {
            StableId = stableId,
            CataloguePath = cataloguePath,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
            AllowInteraction = false,
            Mode = mode,
        };
        return await ExtensionCreateOperationFactory.Create(session).ExecuteAsync(request, cancellationToken);
    }

    private static void DeleteDestination(string destination)
    {
        if (Directory.Exists(destination))
        {
            Directory.Delete(destination, recursive: true);
        }
        else if (File.Exists(destination))
        {
            File.Delete(destination);
        }
    }
}
