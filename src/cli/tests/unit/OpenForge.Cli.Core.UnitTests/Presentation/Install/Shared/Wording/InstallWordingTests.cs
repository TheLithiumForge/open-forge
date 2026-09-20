using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Install.Shared.Wording;

public sealed class InstallWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install wording uses the shared apply sentence when no files are replaced"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUsesApplySentenceWhenNoFilesAreReplaced()
        => Assert.Equal(
            "Apply these changes? [y/N]",
            InstallWording.Confirmation(new InstallConfirmationFacts(0)));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install wording names distinct replacement files"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationNamesDistinctReplacementFiles()
        => Assert.Equal(
            "Replace the 2 existing files listed above? [y/N]",
            InstallWording.Confirmation(new InstallConfirmationFacts(2)));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install wording uses singular replacement grammar"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUsesSingularReplacementFile()
        => Assert.Equal(
            "Replace the 1 existing file listed above? [y/N]",
            InstallWording.Confirmation(new InstallConfirmationFacts(1)));
}
