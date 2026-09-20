using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Observation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryEntrySetObserverIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Real recovery entry-set observation retains complete ordered ordinary and missing-link facts including third or unavailable entries")]
    [InlineData("intended"), InlineData("third"), InlineData("unavailable")]
    public static async Task ObservesEveryVerifiedEntry(string firstState)
    {
        if (OperatingSystem.IsWindows() && firstState == "unavailable")
        {
            Assert.Skip("The unavailable-read case requires Unix permissions.");
            return;
        }
        using var fixture = await LibraryRecoveryWorkspace.CreateOrderedSetAsync();
        UnixFileMode? mode = null;
        if (firstState == "third")
        {
            File.WriteAllText(fixture.TargetPath, "third");
        }
        if (!OperatingSystem.IsWindows() && firstState == "unavailable")
        {
            mode = File.GetUnixFileMode(fixture.TargetPath);
            File.SetUnixFileMode(fixture.TargetPath, UnixFileMode.None);
        }
        try
        {
            var result = await RecoveryEntrySetObserver.ReadAsync(new PhysicalPathResolver(), fixture.Workspace,
                fixture.Candidate, TestContext.Current.CancellationToken);

            Assert.Same(fixture.Candidate, result.Candidate);
            Assert.Equal(fixture.Preparation.Entries, result.Entries.Select(entry => entry.Input.Context.Entry));
            Assert.Equal(2, result.Entries.Length);
            var first = result.Entries[0];
            var expected = firstState switch
            {
                "intended" => RecoveryBundleTargetComparisonState.Intended,
                "third" => RecoveryBundleTargetComparisonState.Third,
                "unavailable" => RecoveryBundleTargetComparisonState.Unavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(firstState)),
            };
            Assert.Equal(expected, first.State);
            Assert.NotNull(first.Input.OrdinaryContent);
            Assert.Equal(RecoveryBundleTargetComparisonState.Prior, result.Entries[1].State);
            Assert.Equal(RecoveryEntryState.Missing, result.Entries[1].Observed);
            Assert.Null(result.Entries[1].Input.OrdinaryContent);
            Assert.True(File.Exists(fixture.Preparation.BundlePath));
            fixture.AssertUnrelatedPreserved();
        }
        finally
        {
            if (!OperatingSystem.IsWindows() && mode is { } original)
            {
                File.SetUnixFileMode(fixture.TargetPath, original);
            }
        }
    }
}
