using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Remove;

public sealed class RemoveSelectionContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Root Remove defaults to one exact portable path and only strips optional dot slash")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Unit")]
    public void DefaultPathSelectionIsExact()
    {
        var ordinary = RemoveSelectionResolver.Resolve("folder/README.md", null);
        var dotSlash = RemoveSelectionResolver.Resolve("./folder/README.md", "path");
        var idLike = RemoveSelectionResolver.Resolve("guidance/old guide", null);
        var leadingSpace = RemoveSelectionResolver.Resolve(" docs/README.md", "path");

        Assert.Equal(RemoveSelectionState.Path, ordinary.State);
        Assert.Equal("folder/README.md", ordinary.Target);
        Assert.Equal(RemoveSelectionState.Path, dotSlash.State);
        Assert.Equal("folder/README.md", dotSlash.Target);
        Assert.Equal(RemoveSelectionState.Path, idLike.State);
        Assert.Equal("guidance/old guide", idLike.Target);
        Assert.Equal(RemoveSelectionState.Path, leadingSpace.State);
        Assert.Equal(" docs/README.md", leadingSpace.Target);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Root Remove rejects nonportable paths before operation dispatch")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Unit")]
    [InlineData("../outside.md")]
    [InlineData("/rooted.md")]
    [InlineData("C:/rooted.md")]
    [InlineData("folder/*.md")]
    [InlineData("folder/name?.md")]
    public void InvalidPathSelectionIsTyped(string target)
    {
        var selection = RemoveSelectionResolver.Resolve(target, "path");

        Assert.Equal(RemoveSelectionState.Invalid, selection.State);
        Assert.NotNull(selection.Cause);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Root Remove validates typed route, Extension, and Library selectors")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Unit")]
    [InlineData("route", "guide/old page", "Route")]
    [InlineData("extension", "sample-package", "Extension")]
    [InlineData("library", "team-knowledge", "Library")]
    public void ValidTypedSelectorsStayTyped(string kind, string target, string expected)
    {
        var selection = RemoveSelectionResolver.Resolve(target, kind);

        Assert.Equal(expected, selection.State.ToString());
        Assert.Equal(target, selection.Target);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Root Remove rejects malformed typed selectors")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Unit")]
    [InlineData("unknown", "path")]
    [InlineData("extension", "Sample Package")]
    [InlineData("library", "../team")]
    [InlineData("route", "guide/*")]
    public void MalformedTypedSelectorsAreInvalid(string kind, string target)
    {
        var selection = RemoveSelectionResolver.Resolve(target, kind);

        Assert.Equal(RemoveSelectionState.Invalid, selection.State);
        Assert.NotNull(selection.Cause);
    }
}
