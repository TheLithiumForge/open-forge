using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Reading;

public sealed class SourceReadingModelTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral layer verification retains the exact finite verification states")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void VerificationStatesAreStable()
    {
        Assert.Equal(
            [
                nameof(SourceLayerVerificationState.Verified),
                nameof(SourceLayerVerificationState.Missing),
                nameof(SourceLayerVerificationState.Unsafe),
                nameof(SourceLayerVerificationState.Unavailable),
                nameof(SourceLayerVerificationState.Changed),
                nameof(SourceLayerVerificationState.Cancelled),
            ],
            Enum.GetNames<SourceLayerVerificationState>());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral verified document reads retain the requested layer and logical path")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void VerifiedReadRetainsTheLayerAssociation()
    {
        var layer = Layer(".agents/root.md", SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var verification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Verified,
            layer.PhysicalPath,
            null);
        var read = FileReadResult<string>.Complete(layer.CanonicalPath, "body");
        var result = new SourceDocumentReadResult(layer, verification, read);

        Assert.Same(layer, result.Layer);
        Assert.Same(verification, result.Verification);
        Assert.Same(read, result.Read);
        var completeRead = Assert.IsType<FileReadResult<string>>(result.Read);
        Assert.Equal(layer.CanonicalPath, completeRead.LogicalPath);
        Assert.Equal("body", completeRead.Value);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral missing unavailable and cancelled document reads carry no body while retaining typed state")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void MissingUnavailableAndCancelledReadsHaveDistinctNoBodyShapes()
    {
        var layer = Layer(".agents/root.md", SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var missingVerification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Missing,
            null,
            null);
        var missing = new SourceDocumentReadResult(
            layer,
            missingVerification,
            FileReadResult<string>.Missing(layer.CanonicalPath));
        var missingRead = Assert.IsType<FileReadResult<string>>(missing.Read);
        Assert.Equal(FileReadState.Missing, missingRead.State);
        Assert.Null(missingRead.Value);

        var unavailableFailure = new FilesystemFailure(
            FilesystemFailureKind.AccessDenied,
            "Physical resolution was unavailable.");
        var unavailableVerification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Unavailable,
            null,
            unavailableFailure);
        var unavailable = new SourceDocumentReadResult(layer, unavailableVerification, null);
        Assert.Same(unavailableFailure, unavailable.Verification.Failure);
        Assert.Null(unavailable.Verification.CurrentPhysicalPath);
        Assert.Null(unavailable.Read);

        var cancelledVerification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Cancelled,
            null,
            null);
        var cancelled = new SourceDocumentReadResult(layer, cancelledVerification, null);
        Assert.Equal(SourceLayerVerificationState.Cancelled, cancelled.Verification.State);
        Assert.Null(cancelled.Read);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral document read model rejects a body for unsafe verification and a mismatched logical layer")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ReadStateAndLayerInvariantsAreStrict()
    {
        var layer = Layer(".agents/root.md", SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var unsafeVerification = new SourceLayerVerification(
            layer,
            SourceLayerVerificationState.Unsafe,
            null,
            null);
        Assert.Throws<ArgumentException>(() => new SourceDocumentReadResult(
            layer,
            unsafeVerification,
            FileReadResult<string>.Complete(layer.CanonicalPath, "must not be read")));

        var otherLayer = Layer(".agents/other.md", SourceDocumentForm.Markdown, SourceLayerKind.Base);
        var otherVerification = new SourceLayerVerification(
            otherLayer,
            SourceLayerVerificationState.Verified,
            otherLayer.PhysicalPath,
            null);
        Assert.Throws<ArgumentException>(() => new SourceDocumentReadResult(
            layer,
            otherVerification,
            FileReadResult<string>.Complete(layer.CanonicalPath, "wrong layer")));
    }

    private static SourceLayer Layer(
        string canonicalPath,
        SourceDocumentForm form,
        SourceLayerKind kind)
    {
        return new SourceLayer(
            canonicalPath,
            Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                "source-reading-model-unit",
                canonicalPath.Replace('/', Path.DirectorySeparatorChar))),
            form,
            kind);
    }
}
