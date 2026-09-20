
namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;

internal enum LibraryRecordEffect
{
    None,
    Create,
    Replace,
    Delete,
}

internal enum LibraryLinkEffectKind
{
    Create,
    Delete,
}

internal enum LibraryExpectedStateKind
{
    Missing,
    OrdinaryFile,
    RelativeFileLink,
}
