using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Rendering;

public sealed class WorkspaceSelectionWireVocabularyTests
{
    [Theory(DisplayName = "Workspace selection retains both exact wire names"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliWorkspaceSelectionMethod.CurrentDirectory, "current-directory")]
    [InlineData((int)CliWorkspaceSelectionMethod.ExplicitWorkspace, "explicit-workspace")]
    public void RendersNamedSelections(int value, string expected)
    {
        var selection = (CliWorkspaceSelectionMethod)value;

        Assert.Equal(expected, WorkspaceSelectionWireVocabulary.Read(selection));
    }

    [Fact(DisplayName = "Workspace selection rejects an undefined enum with its exact argument facts"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void RejectsUndefinedSelection()
    {
        var selection = (CliWorkspaceSelectionMethod)int.MaxValue;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => WorkspaceSelectionWireVocabulary.Read(selection));

        Assert.Equal("workspace", exception.ParamName);
        Assert.Equal(selection, exception.ActualValue);
        Assert.Equal(
            new ArgumentOutOfRangeException(
                paramName: "workspace",
                actualValue: selection,
                message: "The workspace selection method is not defined.").Message,
            exception.Message);
    }
}
