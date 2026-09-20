using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Presentation.Update.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Update.Shared.Wording;

public sealed class UpdateWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update wording uses the shared apply sentence when no files are deleted"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUsesApplySentenceWhenNoFilesAreDeleted()
        => Assert.Equal(
            "Apply these changes? [y/N]",
            UpdateWording.Confirmation(new UpdateConfirmationFacts(0)));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update wording names planned deleted files"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationNamesPlannedDeletedFiles()
        => Assert.Equal(
            "Delete the 3 files listed above? [y/N]",
            UpdateWording.Confirmation(new UpdateConfirmationFacts(3)));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update wording uses singular deletion grammar"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUsesSingularDeletedFile()
        => Assert.Equal(
            "Delete the 1 file listed above? [y/N]",
            UpdateWording.Confirmation(new UpdateConfirmationFacts(1)));
}
