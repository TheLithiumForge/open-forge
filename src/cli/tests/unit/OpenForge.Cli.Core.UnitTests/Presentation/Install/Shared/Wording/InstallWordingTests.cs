using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Install.Shared.Wording;

public sealed class InstallWordingTests
{
    [Theory(DisplayName = "Shared change confirmations name the destructive part of the whole plan")]
    [InlineData(1, "Apply these changes, including deleting 1 file? [y/N]", "Apply these changes, including removing 1 link? [y/N]")]
    [InlineData(3, "Apply these changes, including deleting 3 files? [y/N]", "Apply these changes, including removing 3 links? [y/N]")]
    [Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void SharedConfirmationsUseSingularAndPlural(int count, string deleting, string removing)
    {
        Assert.Equal(deleting, CliPromptWording.ConfirmDeleting(count));
        Assert.Equal(removing, CliPromptWording.ConfirmRemoving(count));
    }

    [Theory(DisplayName = "Install summaries distinguish creations, edits and whole-file replacements")]
    [InlineData(2, 2, 5, 0, true, "Would create 2 files and 2 directories under .agents, and update 5 existing files.")]
    [InlineData(1, 1, 1, 0, false, "Created 1 file and 1 directory under .agents, and updated 1 existing file.")]
    [InlineData(0, 0, 1, 0, true, "Would update 1 existing file.")]
    [InlineData(0, 0, 2, 0, false, "Updated 2 existing files.")]
    [InlineData(17, 12, 0, 1, true, "Would create 17 files and 12 directories under .agents, and replace 1 existing file.")]
    [InlineData(17, 12, 0, 1, false, "Created 17 files and 12 directories under .agents, and replaced 1 existing file.")]
    [InlineData(1, 1, 2, 1, true, "Would create 1 file and 1 directory under .agents, and update 2 existing files and replace 1 existing file.")]
    [InlineData(1, 1, 2, 1, false, "Created 1 file and 1 directory under .agents, and updated 2 existing files and replaced 1 existing file.")]
    [InlineData(0, 0, 0, 2, true, "Would replace 2 existing files.")]
    [InlineData(0, 0, 0, 1, false, "Replaced 1 existing file.")]
    [InlineData(0, 0, 1, 2, true, "Would update 1 existing file and replace 2 existing files.")]
    [InlineData(0, 0, 1, 2, false, "Updated 1 existing file and replaced 2 existing files.")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ChangeSummaryCountsFilesAndDirectories(int files, int directories, int updatedFiles, int replacedFiles, bool preview, string expected)
    {
        string? creationSummary = null;
        if (files > 0 || directories > 0)
            creationSummary = preview
                ? InstallWording.WouldCreateSummary(files: files, directories: directories, bothHostFiles: false)
                : InstallWording.CreatedSummary(files: files, directories: directories, lockVerified: false);
        Assert.Equal(expected, InstallWording.ChangeSummary(creationSummary: creationSummary,
            updatedFiles: updatedFiles, replacedFiles: replacedFiles, preview: preview));
    }

    [Fact(DisplayName = "Install ownership reference belongs to the creation clause before existing-file edits")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void OwnershipReferenceStaysWithCreations()
        => Assert.Equal(
            "Created 17 files and 13 directories under .agents (listed in .agents/open-forge.lock.json), and updated 1 existing file.",
            InstallWording.ChangeSummary(creationSummary: InstallWording.CreatedSummary(files: 17, directories: 13, lockVerified: true),
                updatedFiles: 1, replacedFiles: 0, preview: false));

    [Theory(DisplayName = "Configure conversion labels name the selected metadata form")]
    [InlineData("root", false, "metadata moved to root keys")]
    [InlineData("root", true, "metadata would move to root keys")]
    [InlineData("scoped", false, "metadata moved under open-forge:")]
    [InlineData("scoped", true, "metadata would move under open-forge:")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ConversionNamesSelectedForm(string form, bool preview, string expected)
        => Assert.Equal(expected, InstallWording.FrontmatterConversion(form, preview));

    [Fact(DisplayName = "Configure conversion wording rejects an undefined metadata form")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ConversionRejectsUndefinedForm()
        => Assert.Throws<ArgumentOutOfRangeException>(() => InstallWording.FrontmatterConversion("undefined", true));

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
            "Apply these changes, including replacing 2 existing files? [y/N]",
            InstallWording.Confirmation(new InstallConfirmationFacts(2)));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install wording uses singular replacement grammar"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUsesSingularReplacementFile()
        => Assert.Equal(
            "Apply these changes, including replacing 1 existing file? [y/N]",
            InstallWording.Confirmation(new InstallConfirmationFacts(1)));
}
