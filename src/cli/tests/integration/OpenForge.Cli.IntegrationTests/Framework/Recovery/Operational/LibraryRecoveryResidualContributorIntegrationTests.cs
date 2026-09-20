using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Operational;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryResidualContributorIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library residual Status producer reports verified metadata without entry-content comparison or mutation")]
    public static async Task StatusDoesNotEnterEntryComparison()
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateOrderedSetAsync();

        var result = await RecoveryResidualOperationalContributor.ReadStatusAsync(fixture.Workspace, TestContext.Current.CancellationToken);

        Assert.Equal(OperationalViewState.Complete, result.State);
        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(fixture.Preparation.BundlePath, candidate.Path);
        Assert.Equal(RecoveryBundleIntegrity.Verified, candidate.Integrity);
        Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        fixture.AssertUnrelatedPreserved();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Residual Doctor producer routes Library finals through neutral entry sets while retaining Framework-only bundle comparison")]
    [InlineData(false), InlineData(true)]
    public static async Task PreservesSeparateProducerComparisonPolicy(bool library)
    {
        using var fixture = library ? await LibraryRecoveryWorkspace.CreateOrderedSetAsync()
            : await LibraryRecoveryWorkspace.CreateAsync("replace", framework: true);
        var resolver = new PhysicalPathResolver();
        var contributor = new RecoveryResidualOperationalContributor(new RecoveryBundleTargetStateReader(resolver), resolver);

        var result = await contributor.ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken);

        Assert.Equal(OperationalViewState.Complete, result.State);
        var candidate = Assert.Single(result.Candidates);
        Assert.Equal(fixture.Preparation.BundlePath, candidate.Path);
        if (library)
        {
            Assert.Null(candidate.Comparison);
            var entries = Assert.IsType<RecoveryEntrySetObservation>(candidate.EntryComparisons);
            Assert.Equal([RecoveryBundleTargetComparisonState.Intended, RecoveryBundleTargetComparisonState.Prior], entries.Entries.Select(entry => entry.State));
        }
        else
        {
            Assert.Null(candidate.EntryComparisons);
            var comparison = Assert.IsType<RecoveryBundleComparison>(candidate.Comparison);
            Assert.Equal(RecoveryBundleTargetComparisonState.Intended, Assert.Single(comparison.Targets).State);
        }
        Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        fixture.AssertUnrelatedPreserved();
    }
}
