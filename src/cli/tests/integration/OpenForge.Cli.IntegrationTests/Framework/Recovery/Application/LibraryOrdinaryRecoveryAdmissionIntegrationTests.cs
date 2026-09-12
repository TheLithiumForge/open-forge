using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryOrdinaryRecoveryAdmissionIntegrationTests
{
    [Theory(DisplayName = "Ordinary recovery admission requires a held same-workspace lease exact original member and ordinary effect kind")]
    [InlineData("released"), InlineData("foreign-workspace"), InlineData("foreign-entry"), InlineData("link-kind")]
    public static async Task RejectsInvalidApplicationAuthority(string defect)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync(defect == "link-kind" ? "link-create" : "replace");
        using var foreign = await LibraryRecoveryWorkspace.CreateAsync("replace");
        var selected = defect == "foreign-workspace" ? foreign : fixture;
        await using var lease = await selected.AcquireLaterLeaseAsync();
        var entry = Assert.Single(fixture.Preparation.Entries);
        if (defect == "released")
        {
            await lease.DisposeAsync();
        }
        if (defect == "foreign-entry")
        {
            entry = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/other.md"),
                kind: RecoveryEntryKind.OrdinaryCreate, prior: RecoveryEntryState.Missing,
                intended: RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes("intended"u8)));
        }

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                fixture.Preparation, entry, TestContext.Current.CancellationToken);
        });

        if (defect == "link-kind")
        {
            Assert.Equal(LibraryRecoveryWorkspace.RelativeTarget, new FileInfo(fixture.TargetPath).LinkTarget);
        }
        else
        {
            Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        }
        Assert.True(File.Exists(fixture.Preparation.BundlePath));
        fixture.AssertUnrelatedPreserved();
        foreign.AssertUnrelatedPreserved();
    }
}
