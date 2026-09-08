using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateCatalogueIntegrationTests
{
    [Fact(DisplayName = "Extension Create accepts empty and populated marker-free catalogue parents while ignoring unrelated siblings"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task EmptyAndPopulatedParentsAreEligibleAndSiblingsArePreserved()
    {
        using var emptyCatalogue = TemporaryWorkspace.Create("extension-create-empty-catalogue");
        using var populatedCatalogue = TemporaryWorkspace.Create("extension-create-populated-catalogue");
        populatedCatalogue.CreateFile("unrelated.txt", "preserve");
        populatedCatalogue.CreateFile("unrelated-package/extension.json", "not a package manifest");

        try
        {
            var emptyResult = await ExecuteAsync(
                cataloguePath: emptyCatalogue.Path,
                stableId: "empty-package",
                cancellationToken: TestContext.Current.CancellationToken);
            var populatedResult = await ExecuteAsync(
                cataloguePath: populatedCatalogue.Path,
                stableId: "populated-package",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, emptyResult.Status);
            Assert.Equal(CliSemanticStatus.Complete, populatedResult.Status);
            Assert.Equal(populatedCatalogue.Combine("populated-package"), populatedResult.Destination);
            Assert.Equal("preserve", File.ReadAllText(populatedCatalogue.Combine("unrelated.txt")));
            Assert.Equal("not a package manifest", File.ReadAllText(populatedCatalogue.Combine("unrelated-package", "extension.json")));
        }
        finally
        {
            DeleteDestination(emptyCatalogue.Combine("empty-package"));
            DeleteDestination(populatedCatalogue.Combine("populated-package"));
        }
    }

    [Theory(DisplayName = "Extension Create rejects a missing or file-valued catalogue parent without writing"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("file")]
    public static async Task InvalidCatalogueParentsAreNoWrite(string parentKind)
    {
        using var owner = TemporaryWorkspace.Create("extension-create-invalid-parent");
        var path = parentKind == "missing"
            ? owner.Combine("missing-catalogue")
            : owner.CreateFile("catalogue-file", "not a directory");
        var before = owner.SnapshotHashes();

        var result = await ExecuteAsync(
            cataloguePath: path,
            stableId: "development-toolkit",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(before, owner.SnapshotHashes());
        Assert.False(Directory.Exists(Path.Combine(path, "development-toolkit")));
    }

    [Fact(DisplayName = "Extension Create accepts a catalogue root alias and reports its resolved destination identity"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CatalogueRootAliasIsAccepted()
    {
        using var external = TemporaryWorkspace.Create("extension-create-external-catalogue");
        using var owner = TemporaryWorkspace.Create("extension-create-linked-catalogue");
        var alias = owner.CreateDirectorySymbolicLink("catalogue-alias", external.Path);
        var destination = Path.Combine(external.Path, "development-toolkit");

        try
        {
            var result = await ExecuteAsync(
                cataloguePath: alias,
                stableId: "development-toolkit",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(Path.Combine(alias, "development-toolkit"), result.Destination);
            Assert.True(File.Exists(Path.Combine(destination, "extension.json")));
            Assert.True(Directory.Exists(Path.Combine(destination, "content", ".agents")));
        }
        finally
        {
            DeleteDestination(destination);
        }
    }

    [Fact(DisplayName = "Extension Create inspects only the exact destination and does not adopt a colliding sibling"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ExactDestinationBoundaryIgnoresSiblingOccupants()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-exact-destination");
        catalogue.CreateFile("other-package/extension.json", "invalid sibling");
        catalogue.CreateFile("other-package/content/unrelated.txt", "preserve");

        try
        {
            var result = await ExecuteAsync(
                cataloguePath: catalogue.Path,
                stableId: "development-toolkit",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal("invalid sibling", File.ReadAllText(catalogue.Combine("other-package", "extension.json")));
            Assert.Equal("preserve", File.ReadAllText(catalogue.Combine("other-package", "content", "unrelated.txt")));
        }
        finally
        {
            DeleteDestination(catalogue.Combine("development-toolkit"));
        }
    }

    private static async Task<ExtensionCreateResult> ExecuteAsync(
        string cataloguePath,
        string stableId,
        ExtensionCreateMode mode = ExtensionCreateMode.Apply,
        bool allowInteraction = false,
        string input = "",
        CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(input);
        using var prompts = new StringWriter();
        var session = new CliInteractiveSession(reader, prompts, canPrompt: allowInteraction);
        var request = new ExtensionCreateRequest
        {
            StableId = stableId,
            CataloguePath = cataloguePath,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
            AllowInteraction = allowInteraction,
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
