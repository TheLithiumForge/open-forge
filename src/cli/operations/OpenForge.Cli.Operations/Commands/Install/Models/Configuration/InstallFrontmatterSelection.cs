using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal sealed record InstallFrontmatterSelection
{
    internal InstallFrontmatterSelection(
        WorkspaceSettingsRead settings,
        FrontmatterForm form,
        FrontmatterForm? previousForm,
        bool persist,
        bool convertOwnedFiles)
    {
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The frontmatter form is not defined.");
        }

        if (previousForm is { } previous && !Enum.IsDefined(previous))
        {
            throw new ArgumentOutOfRangeException(nameof(previousForm), previous, "The previous frontmatter form is not defined.");
        }

        if (convertOwnedFiles && previousForm is null)
        {
            throw new ArgumentException(
                "Only an installed workspace can convert owned files to another frontmatter form.",
                nameof(convertOwnedFiles));
        }

        Settings = settings;
        Form = form;
        PreviousForm = previousForm;
        Persist = persist;
        ConvertOwnedFiles = convertOwnedFiles;
    }

    internal WorkspaceSettingsRead Settings { get; }

    internal FrontmatterForm Form { get; }

    internal FrontmatterForm? PreviousForm { get; }

    internal bool Persist { get; }

    internal bool ConvertOwnedFiles { get; }
}
