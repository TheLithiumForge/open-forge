using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Shared.Observation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryMappingObserverIntegrationTests
{
    [Theory(DisplayName = "Library mapping observations distinguish exact dangling links missing links changed text and unsafe occupants")]
    [InlineData("exact", "Current"), InlineData("dangling", "Current"), InlineData("missing", "Missing")]
    [InlineData("changed", "Changed"), InlineData("file", "Changed"), InlineData("absolute", "Blocked"), InlineData("parent", "Blocked")]
    public static void ObservesExactLogicalMapping(string scenario, string expected)
    {
        using var temporary = TemporaryWorkspace.Create("library-mapping");
        var target = temporary.CreateFile("shared/team/.agents/a.md", "source");
        const string rawTarget = "../shared/team/.agents/a.md";
        temporary.CreateDirectory("consumer-parent");
        if (scenario == "parent")
        {
            temporary.CreateDirectorySymbolicLink(".agents", "consumer-parent");
        }
        else
        {
            temporary.CreateDirectory(".agents");
            switch (scenario)
            {
                case "exact": temporary.CreateFileSymbolicLink(".agents/a.md", rawTarget); break;
                case "dangling":
                    temporary.CreateFileSymbolicLink(".agents/a.md", rawTarget);
                    File.Delete(target);
                    break;
                case "changed": temporary.CreateFileSymbolicLink(".agents/a.md", "../shared/team/./.agents/a.md"); break;
                case "file": temporary.CreateFile(".agents/a.md", "local"); break;
                case "absolute": temporary.CreateFileSymbolicLink(".agents/a.md", target); break;
            }
        }
        var request = new LibraryMappingObservationRequest
        {
            Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Mapping = LibraryMapping.Create(WorkspaceRelativeDirectory.Create("shared/team"), SourceRelativeEligiblePath.Create(".agents/a.md")),
        };

        var result = LibraryMappingObserver.Observe(new PhysicalPathResolver(), request, TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.State.ToString());
        Assert.Equal(".agents/a.md", result.LogicalDestinationPath);
        Assert.Equal(rawTarget, result.ExpectedLink.RawRelativeTarget);
        if (scenario == "dangling")
        {
            Assert.False(File.Exists(target));
        }
        else
        {
            Assert.Equal("source", File.ReadAllText(target));
        }
    }
}
