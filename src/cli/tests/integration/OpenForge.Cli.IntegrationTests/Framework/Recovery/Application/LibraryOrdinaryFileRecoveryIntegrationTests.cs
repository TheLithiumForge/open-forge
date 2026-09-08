using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryOrdinaryFileRecoveryIntegrationTests
{
    [Theory(DisplayName = "Explicit ordinary recovery deletes exact prior-missing creates and restores verified prior bytes including deleted targets")]
    [InlineData("create"), InlineData("replace"), InlineData("generated"), InlineData("delete")]
    public static async Task RestoresExactPriorOrdinaryState(string kind)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync(kind);
        await using var lease = await fixture.AcquireLaterLeaseAsync();
        var entry = Assert.Single(fixture.Preparation.Entries);
        var bundleBytes = File.ReadAllBytes(fixture.Preparation.BundlePath);
        Assert.NotEqual(fixture.Preparation.OperationId, lease.Request.OperationId);
        Assert.NotEqual(fixture.Preparation.Command, lease.Request.Command);

        var result = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
            fixture.Preparation, entry, TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.Applied, result.Effect);
        Assert.Equal(FilesystemVerificationState.Verified, result.Verification);
        Assert.Equal(RecoveryBundleTargetComparisonState.Intended, result.Before.State);
        var after = Assert.IsType<RecoveryEntryComparison>(result.After);
        Assert.Equal(RecoveryBundleTargetComparisonState.Prior, after.State);
        Assert.Equal(entry.Prior, after.Observed);
        Assert.Null(new FileInfo(fixture.TargetPath).LinkTarget);
        if (kind == "create")
        {
            Assert.False(File.Exists(fixture.TargetPath));
        }
        else
        {
            Assert.Equal("prior", File.ReadAllText(fixture.TargetPath));
        }
        Assert.Equal(bundleBytes, File.ReadAllBytes(fixture.Preparation.BundlePath));
        fixture.AssertUnrelatedPreserved();
    }

    [Fact(DisplayName = "Explicit ordinary recovery cancellation before observation performs no effect")]
    public static async Task CancelsBeforeObservation()
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync("replace");
        await using var lease = await fixture.AcquireLaterLeaseAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                fixture.Preparation, Assert.Single(fixture.Preparation.Entries), cancellation.Token);
        });

        Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        Assert.True(File.Exists(fixture.Preparation.BundlePath));
        fixture.AssertUnrelatedPreserved();
    }
}
