using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

public sealed class SourceDocumentSnapshotReaderIntegrationTests
{
    [Fact(DisplayName = "Source document snapshot reader uses exact current bytes instead of a cached source body"), Trait("Feature", "source-document-snapshot"), Trait("Evidence", "Integration")]
    public async Task ExactSourceSnapshotFeedsExpectationAndContentFromOneRead()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        const string canonicalPath = ".agents/memory/_memory.md";
        const string exactText = "exact current source bytes";
        workspace.Write(canonicalPath, exactText);
        var physicalPath = workspace.Absolute(canonicalPath);
        var layer = new SourceLayer(
            canonicalPath,
            physicalPath,
            SourceDocumentForm.CanonicalEntrypoint,
            SourceLayerKind.Base);
        var read = new SourceDocumentReadResult(
            layer,
            new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Verified,
                physicalPath,
                failure: null),
            FileReadResult<string>.Complete(canonicalPath, "earlier cached source text"));

        var snapshot = await new SourceDocumentSnapshotReader().ReadAsync(
            workspace.Workspace,
            read,
            TestContext.Current.CancellationToken);

        Assert.Equal(exactText, System.Text.Encoding.UTF8.GetString(snapshot.Bytes.AsSpan()));
        Assert.Equal(FileExpectation.Hash(snapshot.Bytes.AsSpan()), snapshot.ContentHash);
        Assert.NotEqual(
            FileExpectation.Hash("earlier cached source text"u8),
            snapshot.ContentHash);
    }

    [Fact(DisplayName = "Source document snapshot reader rejects invalid UTF-8 without using the cached body"), Trait("Feature", "source-document-snapshot"), Trait("Evidence", "Integration")]
    public async Task ExactSourceSnapshotRejectsInvalidUtf8()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        const string canonicalPath = ".agents/memory/_memory.md";
        var invalidBytes = new byte[] { 0xC3, 0x28 };
        workspace.Write(canonicalPath, invalidBytes);
        var physicalPath = workspace.Absolute(canonicalPath);
        var layer = new SourceLayer(
            canonicalPath,
            physicalPath,
            SourceDocumentForm.CanonicalEntrypoint,
            SourceLayerKind.Base);
        var read = new SourceDocumentReadResult(
            layer,
            new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Verified,
                physicalPath,
                failure: null),
            FileReadResult<string>.Complete(canonicalPath, "cached source text"));

        await Assert.ThrowsAsync<System.Text.DecoderFallbackException>(async () =>
            await new SourceDocumentSnapshotReader().ReadAsync(
                workspace.Workspace,
                read,
                TestContext.Current.CancellationToken));

        Assert.Equal(invalidBytes, File.ReadAllBytes(physicalPath));
    }
}
