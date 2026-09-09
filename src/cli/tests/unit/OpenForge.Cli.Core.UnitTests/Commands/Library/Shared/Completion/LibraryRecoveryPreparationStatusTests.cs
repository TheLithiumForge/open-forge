using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Completion;

public sealed class LibraryRecoveryPreparationStatusTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(null, null)]
    [InlineData((int)RecoveryBundlePreparationState.NotNeeded, null)]
    [InlineData((int)RecoveryBundlePreparationState.Prepared, null)]
    [InlineData((int)RecoveryBundlePreparationState.Incomplete, (int)CliSemanticStatus.Incomplete)]
    [InlineData((int)RecoveryBundlePreparationState.Blocked, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)RecoveryBundlePreparationState.Cancelled, (int)CliSemanticStatus.Interrupted)]
    public void EveryPreparationStateHasItsFiniteStatus(int? state, int? status)
        => Assert.Equal((CliSemanticStatus?)status, LibraryMutationCompletionProjection.PreparationStatus((RecoveryBundlePreparationState?)state));

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UndefinedPreparationStateFailsExplicitly()
        => Assert.Throws<ArgumentOutOfRangeException>(() => LibraryMutationCompletionProjection.PreparationStatus((RecoveryBundlePreparationState)int.MaxValue));
}
