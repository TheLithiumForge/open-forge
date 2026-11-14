using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue;

public sealed class ToolCatalogueTests
{
    [Fact(DisplayName = "The V1 catalogue contains only the two reviewed third-party entries"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void ContainsReviewedEntries()
    {
        var catalogue = ToolCatalogueFactory.Create([]);
        Assert.Equal(new[] { CodebaseMemoryDefinitions.Id, CodeGraphDefinitions.Id }, catalogue.Entries.Keys.Order(StringComparer.Ordinal));
        Assert.All(catalogue.Entries, pair => Assert.Equal(pair.Key, pair.Value.Identity.Id));
        Assert.Empty(catalogue.NodeRuntimes);
    }

    [Fact(DisplayName = "The catalogue keeps only supplied private Node pins and copies their inventory"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void KeepsSuppliedRuntimeInventory()
    {
        var runtime = SyntheticRuntime();
        var supplied = new List<ToolPrivateNodeSpec> { runtime };
        var catalogue = ToolCatalogueFactory.Create(supplied);
        supplied.Clear();
        Assert.Same(runtime, Assert.Single(catalogue.NodeRuntimes));
        Assert.Equal(2, catalogue.Entries.Count);
    }

    [Fact(DisplayName = "The catalogue rejects a missing runtime inventory at composition"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit")]
    public void RejectsMissingRuntimeInventory() => Assert.Throws<ArgumentNullException>(() => ToolCatalogueFactory.Create(null!));

    private static ToolPrivateNodeSpec SyntheticRuntime() => new()
    {
        Version = "99.0.0",
        Archive = new()
        {
            Version = "99.0.0",
            Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
            Url = new("https://nodejs.org/dist/v99.0.0/synthetic.zip"),
            Sha256 = new string('a', 64),
            Archive = ToolArchiveKind.Zip,
            StripComponents = 1,
            EntryRelativePath = "node.exe",
            MaximumBytes = 1024,
        },
        SignedChecksums = new("https://nodejs.org/dist/v99.0.0/SHASUMS256.txt.sig"),
        VerifiedSigner = "synthetic-reviewer",
        NpmRelativePath = "node_modules/npm/bin/npm-cli.js",
    };
}
