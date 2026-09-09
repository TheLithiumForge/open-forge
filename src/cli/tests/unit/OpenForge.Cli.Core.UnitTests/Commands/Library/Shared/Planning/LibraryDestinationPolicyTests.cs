using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Planning;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Unit")]
public sealed class LibraryDestinationPolicyTests
{
    [Theory]
    [InlineData(".git/config"), InlineData(".agents/open-forge.permissions.json/child.md")]
    [InlineData(".agents/directives/_directives.md"), InlineData(".agents/directives/_directives.md/child.md")]
    [InlineData(".agents/loader.md"), InlineData(".AGENTS/anything.md")]
    [InlineData("shared/team/a.md")]
    public void ProtectedDestinationsCannotBeAdmittedByMapping(string destination)
    {
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: destination);

        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered: null, generatedTargets: []));
    }

    [Theory]
    [InlineData("README.md"), InlineData("docs/_docs.md"), InlineData("docs/a.overwrite.md")]
    public void ExternalContentNamesRemainOpaqueOrdinaryLeaves(string destination)
    {
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: destination);

        Assert.Null(LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered: null, generatedTargets: []));
    }

    [Theory]
    [InlineData("docs/a", "DOCS/A/child.md")]
    [InlineData("docs/a/child.md", "DOCS/A")]
    [InlineData("docs/a.md", "DOCS/A.MD")]
    public void RegisteredLeavesReservePortableIdentityAndAncestorUse(string existing, string proposed)
    {
        var registered = LibrariesRecord.Create([Record(id: "other", sourceRoot: "shared/other", destination: existing)]);
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: proposed);

        Assert.Equal(proposed, LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered, generatedTargets: []));
    }

    [Fact]
    public void SourceProtectionIncludesOtherLibrariesAndGeneratedRegionTargets()
    {
        var registered = LibrariesRecord.Create([Record(id: "other", sourceRoot: "shared/other", destination: "other.md")]);
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: "shared/other/a.md");
        Assert.Equal("shared/other/a.md", LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered, generatedTargets: []));

        selected = Record(id: "team", sourceRoot: ".agents/directives", destination: "docs/a.md");
        Assert.Equal(".agents/directives/_directives.md", LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered,
            generatedTargets: [".agents/directives/_directives.md"]));
    }

    private static LibraryRecord Record(string id, string sourceRoot, string destination)
        => LibraryRecord.Create(LibraryId.Create(id), WorkspaceRelativeDirectory.Create(sourceRoot),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);

    private static CliWorkspace Workspace()
    {
        var path = Path.GetFullPath("library-policy-unit");
        return new(lexicalRoot: path, physicalRoot: path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
