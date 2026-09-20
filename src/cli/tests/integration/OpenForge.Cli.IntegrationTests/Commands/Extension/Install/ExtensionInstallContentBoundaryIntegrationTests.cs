using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallContentBoundaryIntegrationTests
{
    private const string RoutedSource = """
                                        ---
                                        open-forge:
                                          description: A routed guidance source
                                          tags: [Guidance]
                                        ---

                                        # Placed

                                        """;

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "A package that delivers no files reports attention naming the content directory"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("--automatic")]
    [InlineData("--dry-run")]
    public async Task PayloadOutsideContentReportsAttention(string mode)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-install-payload-outside-content{mode}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-install-payload-outside-content-source{mode}");
        source.AddPackage("misplaced", []);
        source.AddMisplacedPayload("misplaced", ".agents/guidance/misplaced.md", RoutedSource);
        var ownershipBefore = File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath));

        var result = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            mode, "--format", "json",
        ]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-install.package-content-missing", finding.GetProperty("code").GetString());
        Assert.Contains("content/.agents/", finding.GetProperty("message").GetString());
        Assert.Equal(ownershipBefore,
            File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath)));
        Assert.DoesNotContain(
            workspace.ReadExtensionOwnership().EnumerateArray(),
            package => package.GetProperty("id").GetString() == "misplaced");
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "A package whose payload sits under content installs and stays complete"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task PayloadUnderContentInstalls()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-payload-under-content");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-payload-under-content-source");
        source.AddPackage("placed", [], (".agents/guidance/placed.md", RoutedSource));

        var result = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Contains(
            workspace.ReadExtensionOwnership().EnumerateArray(),
            package => package.GetProperty("id").GetString() == "placed");
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "A dependency-only package retains an ownership record while its dependency carries content"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task DependencyOnlyAggregatorRetainsOwnershipRecord()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-dependency-only-content");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-dependency-only-content-source");
        source.AddPackage("placed", [], (".agents/guidance/placed.md", RoutedSource));
        source.AddPackage("aggregate", ["placed"]);

        var result = await workspace.RunAsync(
        [
            "extension", "install", "aggregate",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        var receipts = workspace.ReadExtensionOwnership().EnumerateArray().ToArray();
        Assert.Equal(["aggregate", "placed"], receipts.Select(receipt => receipt.GetProperty("id").GetString()));
        var aggregate = Assert.Single(receipts, receipt => receipt.GetProperty("id").GetString() == "aggregate");
        Assert.Empty(aggregate.GetProperty("paths").EnumerateArray());
        Assert.Equal(
            ["placed"],
            aggregate.GetProperty("dependencies").EnumerateArray().Select(dependency => dependency.GetString()));
    }
}
