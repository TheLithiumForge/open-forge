using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Planning;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Unit")]
public sealed class LibraryDestinationPolicyTests
{
    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData(".git/config")]
    [InlineData(".agents/directives/_directives.md"), InlineData(".agents/directives/_directives.md/child.md")]
    [InlineData(".agents/loader.md"), InlineData(".AGENTS/anything.md")]
    [InlineData(".agents/open-forge.json"), InlineData(".agents/open-forge.lock.json")]
    [InlineData(".agents/open-forge.json/child.md"), InlineData(".agents/open-forge.lock.json/child.md")]
    [InlineData("shared/team/a.md")]
    public void ProtectedDestinationsCannotBeAdmittedByMapping(string destination)
    {
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: destination);

        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered: null, generatedTargets: []));
    }

    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData("README.md"), InlineData("docs/_docs.md"), InlineData("docs/a.overwrite.md")]
    public void ExternalContentNamesRemainOpaqueOrdinaryLeaves(string destination)
    {
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: destination);

        Assert.Null(LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered: null, generatedTargets: []));
    }

    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData("docs/a", "DOCS/A/child.md")]
    [InlineData("docs/a/child.md", "DOCS/A")]
    [InlineData("docs/a.md", "DOCS/A.MD")]
    public void RegisteredLeavesReservePortableIdentityAndAncestorUse(string existing, string proposed)
    {
        var registered = LibraryRegistrationSet.Create([Record(id: "other", sourceRoot: "shared/other", destination: existing)]);
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: proposed);

        Assert.Equal(proposed, LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered, generatedTargets: []));
    }

    [Trait("Boundary", "Processing")]
    [Fact]
    public void SourceProtectionIncludesOtherLibrariesAndGeneratedRegionTargets()
    {
        var registered = LibraryRegistrationSet.Create([Record(id: "other", sourceRoot: "shared/other", destination: "other.md")]);
        var selected = Record(id: "team", sourceRoot: "shared/team", destination: "shared/other/a.md");
        Assert.Equal("shared/other/a.md", LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered, generatedTargets: []));

        selected = Record(id: "team", sourceRoot: ".agents/directives", destination: "docs/a.md");
        Assert.Equal(".agents/directives/_directives.md", LibraryDestinationPolicy.FindConflict(Workspace(), selected, registered,
            generatedTargets: [".agents/directives/_directives.md"]));
    }

    private static LibraryRegistration Record(string id, string sourceRoot, string destination)
        => LibraryRegistration.Create(LibraryId.Create(id), WorkspaceRelativeDirectory.Create(sourceRoot),
            LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(destination)]);

    private static CliWorkspace Workspace()
    {
        var path = Path.GetFullPath("library-policy-unit");
        return new(lexicalRoot: path, physicalRoot: path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
