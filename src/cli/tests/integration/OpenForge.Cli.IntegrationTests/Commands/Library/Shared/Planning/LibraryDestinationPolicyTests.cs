using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Planning;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Integration")]
public sealed class LibraryDestinationPolicyTests : IDisposable
{
    private const string FixtureName = "library-destination-policy";
    private readonly TemporaryWorkspace _workspace = TemporaryWorkspace.Create(FixtureName);
    private const string ArchiveName = "archive.zip";
    private const string CustomRoot = "docs/forge";
    private const string AliasNote = "notes/team" + SourceFormClassifier.MarkdownExtension;
    private const string Team = "team";
    private const string Other = "other";
    private const string TeamSource = "shared/team";
    private const string OtherSource = "shared/other";
    private const string Leaf = "a" + SourceFormClassifier.MarkdownExtension;
    private const string Child = "child" + SourceFormClassifier.MarkdownExtension;
    private const string Docs = "docs";
    private static readonly string Directives = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Directives;
    private static readonly string Entrypoint = Directives + "/" + SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceCategoryNames.Directives);

    public static TheoryData<string> ProtectedDestinations =>
    [
        WorkspaceLayoutNames.GitMetadataDirectoryName + "/config",
        Entrypoint, Entrypoint + "/" + Child,
        SourceLogicalPath.LoaderPath, WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/" + Leaf,
        WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath,
        WorkspaceSettingsDefinitions.RelativePath + "/" + Child, WorkspaceOwnershipDefinitions.RelativePath + "/" + Child,
        TeamSource + "/" + Leaf,
    ];

    public static TheoryData<string> ExternalDestinations =>
    [
        WorkspaceLayoutNames.HumanGuideFileName,
        Docs + "/" + SourceFormClassifier.CanonicalEntrypointFileName(Docs),
        SourceOverwritePath.ReadAdjacentPath(Docs + "/" + Leaf),
    ];

    public static TheoryData<string> RuntimeDestinations
    {
        get
        {
            var root = Path.GetFullPath(FixtureName);
            var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
            return
            [
                Path.GetRelativePath(root, WorkspaceRuntimePaths.DirectoryPath(workspace)).Replace('\\', '/'),
                Path.GetRelativePath(root, Path.Combine(WorkspaceRuntimePaths.RecoveryRoot(workspace), ArchiveName)).Replace('\\', '/'),
            ];
        }
    }

    [Trait("Boundary", "Filesystem")]
    [Theory]
    [MemberData(nameof(ProtectedDestinations)), MemberData(nameof(RuntimeDestinations))]
    public void ProtectedDestinationsCannotBeAdmittedByMapping(string destination)
    {
        var selected = Record(id: Team, sourceRoot: TeamSource, destination: destination);

        Assert.Equal(destination, LibraryDestinationPolicy.FindConflict(WorkspaceLayout.Create(Workspace(), WorkspaceLayout.DefaultMainRoot), selected, registered: null, generatedTargets: []));
    }

    [Trait("Boundary", "Filesystem")]
    [Theory]
    [MemberData(nameof(ExternalDestinations))]
    public void ExternalContentNamesRemainOpaqueOrdinaryLeaves(string destination)
    {
        var selected = Record(id: Team, sourceRoot: TeamSource, destination: destination);

        Assert.Null(LibraryDestinationPolicy.FindConflict(WorkspaceLayout.Create(Workspace(), WorkspaceLayout.DefaultMainRoot), selected, registered: null, generatedTargets: []));
    }

    [Trait("Boundary", "Filesystem")]
    [Theory]
    [InlineData("docs/a", "DOCS/A/child.md")]
    [InlineData("docs/a/child.md", "DOCS/A")]
    [InlineData("docs/a.md", "DOCS/A.MD")]
    public void RegisteredLeavesReservePortableIdentityAndAncestorUse(string existing, string proposed)
    {
        var registered = LibraryRegistrationSet.Create([Record(id: Other, sourceRoot: OtherSource, destination: existing)]);
        var selected = Record(id: Team, sourceRoot: TeamSource, destination: proposed);

        Assert.Equal(proposed, LibraryDestinationPolicy.FindConflict(WorkspaceLayout.Create(Workspace(), WorkspaceLayout.DefaultMainRoot), selected, registered, generatedTargets: []));
    }

    [Trait("Boundary", "Filesystem")]
    [Fact]
    public void SourceProtectionIncludesOtherLibrariesAndGeneratedRegionTargets()
    {
        var registered = LibraryRegistrationSet.Create([Record(id: Other, sourceRoot: OtherSource, destination: Other + SourceFormClassifier.MarkdownExtension)]);
        var selected = Record(id: Team, sourceRoot: TeamSource, destination: OtherSource + "/" + Leaf);
        Assert.Equal(OtherSource + "/" + Leaf, LibraryDestinationPolicy.FindConflict(WorkspaceLayout.Create(Workspace(), WorkspaceLayout.DefaultMainRoot), selected, registered, generatedTargets: []));

        selected = Record(id: Team, sourceRoot: Directives, destination: Docs + "/" + Leaf);
        Assert.Equal(Entrypoint, LibraryDestinationPolicy.FindConflict(WorkspaceLayout.Create(Workspace(), WorkspaceLayout.DefaultMainRoot), selected, registered,
            generatedTargets: [Entrypoint]));
    }

    [Trait("Boundary", "Filesystem")]
    [Fact]
    public void CustomRootCaseAliasesAreRefused()
    {
        var layout = WorkspaceLayout.Create(Workspace(), CustomRoot);
        var alias = CustomRoot.ToUpperInvariant() + "/" + AliasNote;
        Assert.Equal(alias, LibraryDestinationPolicy.FindConflict(layout, Record(id: Team, sourceRoot: TeamSource, destination: alias),
            registered: null, generatedTargets: []));
        Assert.Null(LibraryDestinationPolicy.FindConflict(layout, Record(id: Team, sourceRoot: TeamSource, destination: CustomRoot + "/" + AliasNote),
            registered: null, generatedTargets: []));
    }

    private static LibraryRegistration Record(string id, string sourceRoot, string destination)
        => LibraryRegistration.Create(LibraryId.Create(id), WorkspaceRelativeDirectory.Create(sourceRoot),
            LibraryDestinationRoot.Create(LibraryDestinationRoot.WorkspaceRootValue), [SourceRelativeEligiblePath.Create(destination)]);

    private CliWorkspace Workspace()
        => new(lexicalRoot: _workspace.Path, physicalRoot: _workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    public void Dispose() => _workspace.Dispose();
}
