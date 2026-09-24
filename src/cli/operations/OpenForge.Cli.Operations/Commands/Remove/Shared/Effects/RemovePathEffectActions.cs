using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Effects;

internal static class RemovePathEffectActions
{
    internal static string SettingsPersistence(PlannedFileChange change)
        => change.Kind switch
        {
            PlannedFileChangeKind.Create => "create",
            PlannedFileChangeKind.Replace => "persist",
            PlannedFileChangeKind.Delete => throw UnsupportedChange(change),
            PlannedFileChangeKind.ReplaceGeneratedRegion => throw UnsupportedChange(change),
            _ => throw UnsupportedChange(change),
        };

    private static ArgumentOutOfRangeException UnsupportedChange(PlannedFileChange change)
        => new(nameof(change), change.Kind, "A settings removal can only create or replace the settings document.");
}
