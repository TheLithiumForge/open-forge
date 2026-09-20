using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Validation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRelativeFileLinkValidatorTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Relative link validation matches only exact missing create and exact raw-target delete states")]
    [InlineData(false), InlineData(true)]
    public void MatchesExactState(bool delete)
    {
        var path = Path.GetFullPath(".agents/a.md");
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
        var effect = delete
            ? RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/a.md"), link)
            : RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/a.md"), link);
        var expected = delete ? NoFollowLeafObservation.CreateRelativeFileLink(path, link) : NoFollowLeafObservation.Missing(path);

        var result = RelativeFileLinkValidator.Validate(effect, expected, expected);

        Assert.Equal(RelativeFileLinkValidationState.Matched, result.State);
        Assert.Equal(expected, result.Actual);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Relative link create rejects every occupied ordinary or linked leaf without adoption")]
    [InlineData("file"), InlineData("directory"), InlineData("relative"), InlineData("absolute"), InlineData("reparse"), InlineData("special")]
    public void RejectsOccupiedCreate(string occupant)
    {
        var path = Path.GetFullPath(".agents/a.md");
        var effect = RelativeFileLinkEffect.Create(CanonicalRelativePath.Create(".agents/a.md"),
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md"));
        var actual = Occupant(path, occupant);

        var result = RelativeFileLinkValidator.Validate(effect, NoFollowLeafObservation.Missing(path), actual);

        Assert.Contains(result.State, new[] { RelativeFileLinkValidationState.Mismatched, RelativeFileLinkValidationState.Blocked });
        Assert.Equal(actual, result.Actual);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Relative link delete rejects missing changed ordinary generic and special leaves")]
    [InlineData("missing"), InlineData("file"), InlineData("directory"), InlineData("relative"), InlineData("absolute"), InlineData("reparse"), InlineData("special")]
    public void RejectsChangedDelete(string occupant)
    {
        var path = Path.GetFullPath(".agents/a.md");
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
        var effect = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/a.md"), link);
        var actual = Occupant(path, occupant);

        var result = RelativeFileLinkValidator.Validate(effect, NoFollowLeafObservation.CreateRelativeFileLink(path, link), actual);

        Assert.Contains(result.State, new[] { RelativeFileLinkValidationState.Mismatched, RelativeFileLinkValidationState.Blocked });
        Assert.Equal(actual, result.Actual);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Relative link identity compares raw text even when normalized destinations would agree")]
    public void RejectsEquivalentButDifferentRawTarget()
    {
        var path = Path.GetFullPath(".agents/a.md");
        var expectedLink = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
        var actualLink = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/./.agents/a.md");
        var effect = RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(".agents/a.md"), expectedLink);

        var result = RelativeFileLinkValidator.Validate(effect,
            NoFollowLeafObservation.CreateRelativeFileLink(path, expectedLink),
            NoFollowLeafObservation.CreateRelativeFileLink(path, actualLink));

        Assert.Equal(RelativeFileLinkValidationState.Mismatched, result.State);
    }

    private static NoFollowLeafObservation Occupant(string path, string occupant)
        => occupant switch
        {
            "missing" => NoFollowLeafObservation.Missing(path),
            "file" => NoFollowLeafObservation.OrdinaryFile(path),
            "directory" => NoFollowLeafObservation.Directory(path),
            "relative" => NoFollowLeafObservation.CreateRelativeFileLink(path,
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "different.md")),
            "absolute" => NoFollowLeafObservation.CreateLink(path,
                NoFollowLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, Path.GetFullPath("target.md"), NoFollowLinkTargetForm.Absolute)),
            "reparse" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.ReparsePoint),
            "special" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.Special),
            _ => throw new ArgumentOutOfRangeException(nameof(occupant)),
        };
}
