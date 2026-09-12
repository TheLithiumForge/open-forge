using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

public sealed class SourceDocumentReaderIntegrationTests
{
    [Fact(DisplayName = "Neutral document reader verifies a selected physical layer before its first body read")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task VerificationAndRawReadsAreMemoizedAcrossOwnedFixtureMutation()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        workspace.Write(".agents/root.md", "body");
        var layer = Layer(workspace, ".agents/root.md", ".agents/root.md");
        var reader = new SourceDocumentReader(workspace.Workspace);

        var verification = reader.Verify(layer, TestContext.Current.CancellationToken);
        File.Delete(workspace.Absolute(".agents/root.md"));
        var cachedVerification = reader.Verify(layer, TestContext.Current.CancellationToken);
        File.WriteAllBytes(
            workspace.Absolute(".agents/root.md"),
            System.Text.Encoding.UTF8.GetBytes("body"));
        var first = await reader.ReadAsync(layer, TestContext.Current.CancellationToken);
        File.WriteAllBytes(
            workspace.Absolute(".agents/root.md"),
            System.Text.Encoding.UTF8.GetBytes("mutated"));
        var beforeSecondRead = workspace.SnapshotHashes();
        var second = await reader.ReadAsync(layer, TestContext.Current.CancellationToken);

        Assert.Equal(SourceLayerVerificationState.Verified, verification.State);
        Assert.Equal(SourceLayerVerificationState.Verified, cachedVerification.State);
        Assert.Equal(layer.PhysicalPath, cachedVerification.CurrentPhysicalPath);
        Assert.Null(cachedVerification.Failure);
        var firstRead = Assert.IsType<FileReadResult<string>>(first.Read);
        var secondRead = Assert.IsType<FileReadResult<string>>(second.Read);
        Assert.Equal(FileReadState.Complete, firstRead.State);
        Assert.Equal("body", firstRead.Value);
        Assert.Equal(FileReadState.Complete, secondRead.State);
        Assert.Equal("body", secondRead.Value);
        Assert.Equal(beforeSecondRead, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral document reader returns a logically rebound read for contained physical aliases")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task AliasReadsRetainTheRequestedLogicalPath()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        var physical = workspace.Absolute("physical.md");
        workspace.Write("physical.md", "alias body");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/one.md", physical, out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/two.md", physical, out _),
            "This integration case requires real symbolic-link support.");
        var reader = new SourceDocumentReader(workspace.Workspace);
        var firstLayer = Layer(workspace, ".agents/one.md", "physical.md");
        var secondLayer = Layer(workspace, ".agents/two.md", "physical.md");

        var first = await reader.ReadAsync(firstLayer, TestContext.Current.CancellationToken);
        File.WriteAllBytes(
            workspace.Absolute("physical.md"),
            System.Text.Encoding.UTF8.GetBytes("mutated alias body"));
        var second = await reader.ReadAsync(secondLayer, TestContext.Current.CancellationToken);

        var firstRead = Assert.IsType<FileReadResult<string>>(first.Read);
        var secondRead = Assert.IsType<FileReadResult<string>>(second.Read);
        Assert.Equal(".agents/one.md", firstRead.LogicalPath);
        Assert.Equal(".agents/two.md", secondRead.LogicalPath);
        Assert.Equal("alias body", firstRead.Value);
        Assert.Equal("alias body", secondRead.Value);
    }

    [Fact(DisplayName = "Neutral document reader distinguishes missing unsafe unavailable and changed verification states without retrying")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task VerificationOutcomesAreTypedAndSingleAttempt()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("source-document-reader-outside");
        outside.WriteText("outside.md", "outside");
        workspace.Write("changed-before.md", "before");
        workspace.Write("changed-after.md", "after");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/unsafe.md",
                outside.Combine("outside.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/changed.md",
                workspace.Absolute("changed-before.md"),
                out var changedPath),
            "This integration case requires real symbolic-link support.");
        var reader = new SourceDocumentReader(workspace.Workspace);
        var missing = Layer(workspace, ".agents/missing.md", ".agents/missing.md");
        var unsafeLayer = Layer(workspace, ".agents/unsafe.md", ".agents/unsafe.md");
        var changed = Layer(workspace, ".agents/changed.md", "changed-before.md");
        var changedLinkPath = changedPath
            ?? throw new InvalidOperationException("The changed fixture link was not created.");
        File.Delete(changedLinkPath);
        File.CreateSymbolicLink(changedLinkPath, workspace.Absolute("changed-after.md"));

        var missingResult = await reader.ReadAsync(missing, TestContext.Current.CancellationToken);
        var unsafeResult = await reader.ReadAsync(unsafeLayer, TestContext.Current.CancellationToken);
        var changedResult = await reader.ReadAsync(changed, TestContext.Current.CancellationToken);

        Assert.Equal(SourceLayerVerificationState.Missing, missingResult.Verification.State);
        var missingRead = Assert.IsType<FileReadResult<string>>(missingResult.Read);
        Assert.Equal(FileReadState.Missing, missingRead.State);
        Assert.Equal(missing.CanonicalPath, missingRead.LogicalPath);
        Assert.Null(missingRead.Value);
        Assert.Null(missingRead.Failure);
        Assert.Null(missingResult.Verification.CurrentPhysicalPath);
        Assert.Null(missingResult.Verification.Failure);
        Assert.Equal(SourceLayerVerificationState.Unsafe, unsafeResult.Verification.State);
        Assert.Equal(SourceLayerVerificationState.Changed, changedResult.Verification.State);
        Assert.Null(unsafeResult.Read);
        Assert.Null(changedResult.Read);
    }

    [Fact(DisplayName = "Neutral document reader preserves strict UTF-8 failures and does not open an invalid body twice")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task StrictUtf8AndMemoizedFailureRemainDeterministic()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        workspace.Write(".agents/invalid.md", [0xC3, 0x28]);
        var layer = Layer(workspace, ".agents/invalid.md", ".agents/invalid.md");
        var reader = new SourceDocumentReader(workspace.Workspace);

        var first = await reader.ReadAsync(layer, TestContext.Current.CancellationToken);
        File.WriteAllBytes(
            workspace.Absolute(".agents/invalid.md"),
            System.Text.Encoding.UTF8.GetBytes("now valid"));
        var beforeSecondRead = workspace.SnapshotHashes();
        var second = await reader.ReadAsync(layer, TestContext.Current.CancellationToken);

        Assert.Equal(SourceLayerVerificationState.Verified, first.Verification.State);
        var firstRead = Assert.IsType<FileReadResult<string>>(first.Read);
        var secondRead = Assert.IsType<FileReadResult<string>>(second.Read);
        var firstFailure = Assert.IsType<FilesystemFailure>(firstRead.Failure);
        var secondFailure = Assert.IsType<FilesystemFailure>(secondRead.Failure);
        Assert.Equal(FileReadState.InvalidEncoding, firstRead.State);
        Assert.Equal(firstRead.State, secondRead.State);
        Assert.Equal(firstFailure.Kind, secondFailure.Kind);
        Assert.Equal(beforeSecondRead, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral document reader retains cancellation as a terminal verification and read state")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task CancellationIsNotRetried()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        workspace.Write(".agents/cancelled.md", "body");
        var layer = Layer(workspace, ".agents/cancelled.md", ".agents/cancelled.md");
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await new SourceDocumentReader(workspace.Workspace)
            .ReadAsync(layer, cancellation.Token);

        Assert.Same(layer, result.Layer);
        Assert.Same(layer, result.Verification.Layer);
        Assert.Equal(SourceLayerVerificationState.Cancelled, result.Verification.State);
        Assert.Null(result.Verification.CurrentPhysicalPath);
        Assert.Null(result.Verification.Failure);
        Assert.Null(result.Read);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral document reader retains verified identity and a typed access-denied body result for a locked file")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task LockedFileProducesTypedReadAccessFailure()
    {
        using var workspace = SourceDocumentReaderIntegrationWorkspace.Create();
        workspace.Write(".agents/locked.md", "locked");
        var layer = Layer(workspace, ".agents/locked.md", ".agents/locked.md");
        var before = workspace.SnapshotHashes();
        var reader = new SourceDocumentReader(workspace.Workspace);
        SourceDocumentReadResult result;
        using (new FileStream(
                   workspace.Absolute(".agents/locked.md"),
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        {
            result = await reader.ReadAsync(layer, TestContext.Current.CancellationToken);
        }

        Assert.Equal(SourceLayerVerificationState.Verified, result.Verification.State);
        var read = Assert.IsType<FileReadResult<string>>(result.Read);
        Assert.Equal(FileReadState.AccessDenied, read.State);
        var failure = Assert.IsType<FilesystemFailure>(read.Failure);
        Assert.Equal(FilesystemFailureKind.AccessDenied, failure.Kind);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static SourceLayer Layer(
        SourceDocumentReaderIntegrationWorkspace workspace,
        string logicalPath,
        string physicalRelativePath)
    {
        return new SourceLayer(
            logicalPath,
            workspace.Absolute(physicalRelativePath),
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
    }
}
