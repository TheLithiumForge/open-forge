using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Workspace.Shared.Runtime;

[Trait("Feature", "workspace-runtime-reservation"), Trait("Evidence", "Unit")]
public sealed class WorkspaceRuntimePathsTests
{
    private const string WorkspaceName = "runtime-case-workspace";
    private const string NotesName = "notes.md";
    private const string FileName = "file.md";
    private const string NewName = "new.md";
    private const string ChildName = "child";
    private const string NestedName = "nested";
    private const string NewDirectoryName = "new";
    private const string SiblingSuffix = "-sibling";
    private static readonly string Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), WorkspaceName));
    private static readonly CliWorkspace Workspace = new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    private static readonly string RuntimeName = Path.GetFileName(WorkspaceRuntimePaths.DirectoryPath(Workspace));

    public static TheoryData<string, bool> RuntimeCases =>
    [
        (RuntimeName.ToUpperInvariant(), true),
        (Path.Combine(AlternatingCase(RuntimeName), NotesName), true),
        (Path.Combine(AlternatingCase(RuntimeName), NewDirectoryName, FileName), true),
        (Path.Combine(RuntimeName.ToUpperInvariant(), ChildName, "..", NotesName), true),
        (Path.Combine(RuntimeName.ToUpperInvariant(), "..", NotesName), false),
        (Path.Combine(RuntimeName.ToUpperInvariant() + SiblingSuffix, NotesName), false),
        (Path.Combine(NestedName, RuntimeName.ToUpperInvariant(), NotesName), false),
        (".", false),
    ];

    [Theory(DisplayName = "Runtime reservation ignores segment case and preserves directory boundaries")]
    [MemberData(nameof(RuntimeCases))]
    public void ContainsReservesOnlyTheRootRuntimeSegment(string relative, bool reserved)
    {
        Assert.Equal(reserved, WorkspaceRuntimePaths.Contains(Workspace, Path.GetFullPath(relative, Root)));
    }

    [Fact(DisplayName = "Case-independent runtime reservation also works at a volume root")]
    public void ContainsHandlesVolumeRoot()
    {
        var root = Path.GetPathRoot(Path.GetFullPath(Path.GetTempPath()));
        Assert.NotNull(root);
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        Assert.True(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(root, RuntimeName.ToUpperInvariant(), NewName)));
        Assert.False(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(root, RuntimeName.ToUpperInvariant() + SiblingSuffix, NewName)));
    }

    private static string AlternatingCase(string value)
        => new(value.Select((character, index) => index % 2 == 0 ? char.ToUpperInvariant(character) : char.ToLowerInvariant(character)).ToArray());
}
