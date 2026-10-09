namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliChangeWording
{
    internal static string Created(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.Created(preview);

    internal static string Entries(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.Entries(preview);

    internal static string OpenForgeSection(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.OpenForgeSection(preview);

    internal static string AddedOpenForgeSection(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.AddedOpenForgeSection(preview);

    internal static string Settings(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.Settings(preview);

    internal static string OwnershipRecord(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.OwnershipRecord(preview);

    internal static string OwnershipRecordUnchanged()
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.OwnershipRecordUnchanged();

    internal static string OwnershipRecordNotUpdated()
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.OwnershipRecordNotUpdated();

    internal static string OwnershipRecordUnconfirmed()
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.OwnershipRecordUnconfirmed();

    internal static string Updated(bool preview)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.Updated(preview);

    internal static string Replaced(bool preview, bool recoveryKept)
        => global::OpenForge.Cli.OutputText.Shared.ChangeLabels.Replaced(preview, recoveryKept);
}
