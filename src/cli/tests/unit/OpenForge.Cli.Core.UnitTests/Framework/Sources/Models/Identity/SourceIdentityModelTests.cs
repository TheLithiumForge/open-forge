using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Identity;

public sealed class SourceIdentityModelTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source document forms retain the exact finite vocabulary")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DocumentFormsAreStable()
    {
        Assert.Equal(
            [
                nameof(SourceDocumentForm.Loader),
                nameof(SourceDocumentForm.CanonicalEntrypoint),
                nameof(SourceDocumentForm.IndexEntrypoint),
                nameof(SourceDocumentForm.UnderscoreIndexEntrypoint),
                nameof(SourceDocumentForm.ReferencesEntrypoint),
                nameof(SourceDocumentForm.UnderscoreReferencesEntrypoint),
                nameof(SourceDocumentForm.Skill),
                nameof(SourceDocumentForm.Markdown),
                nameof(SourceDocumentForm.OverwriteCompanion),
            ],
            Enum.GetNames<SourceDocumentForm>());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source-reference result factories preserve kind, attempt, and cause independently")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ReferenceFactoriesRetainTheirTypedAttempt()
    {
        var validId = SourceReferenceParseResult.ValidId("route/id");
        Assert.Equal(SourceReferenceParseState.Valid, validId.State);
        Assert.Equal(SourceReferenceKind.SourceId, validId.Kind);
        Assert.Equal("route/id", validId.AttemptedId);
        Assert.Null(validId.AttemptedPath);
        Assert.Null(validId.Cause);

        var validPath = SourceReferenceParseResult.ValidPath(".agents/root/root.md");
        Assert.Equal(SourceReferenceParseState.Valid, validPath.State);
        Assert.Equal(SourceReferenceKind.SourcePath, validPath.Kind);
        Assert.Equal(".agents/root/root.md", validPath.AttemptedPath);

        var invalidId = SourceReferenceParseResult.InvalidId("bad/id", "invalid ID");
        Assert.Equal(SourceReferenceParseState.Invalid, invalidId.State);
        Assert.Equal(SourceReferenceKind.SourceId, invalidId.Kind);
        Assert.Equal("bad/id", invalidId.AttemptedId);
        Assert.Equal("invalid ID", invalidId.Cause);

        var invalidPath = SourceReferenceParseResult.InvalidPath(".agents/../root.md", "unsafe path");
        Assert.Equal(SourceReferenceParseState.Invalid, invalidPath.State);
        Assert.Equal(SourceReferenceKind.SourcePath, invalidPath.Kind);
        Assert.Equal(".agents/../root.md", invalidPath.AttemptedPath);
        Assert.Equal("unsafe path", invalidPath.Cause);
    }

}
