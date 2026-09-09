using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Permissions;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Unit")]
public sealed class LibraryPermissionFailureProjectionTests
{
    [Theory]
    [InlineData((int)LibraryPermissionFailure.Required, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)LibraryPermissionFailure.Declined, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)LibraryPermissionFailure.Invalid, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)LibraryPermissionFailure.Unavailable, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)LibraryPermissionFailure.Changed, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)LibraryPermissionFailure.WriteFailed, (int)CliSemanticStatus.Failed)]
    [InlineData((int)LibraryPermissionFailure.Interrupted, (int)CliSemanticStatus.Interrupted)]
    public void AdmissionAndPublicationFailuresKeepTheirSeverity(int failure, int expected)
    {
        var result = LibraryPermissionFailureProjection.Read((LibraryPermissionFailure)failure);

        Assert.Equal((CliSemanticStatus)expected, result.Status);
        Assert.NotEmpty(result.Cause);
    }

    [Fact]
    public void UndefinedFailureCannotBecomeAUserFacingDecision()
        => Assert.Throws<ArgumentOutOfRangeException>(() => LibraryPermissionFailureProjection.Read((LibraryPermissionFailure)int.MaxValue));
}
