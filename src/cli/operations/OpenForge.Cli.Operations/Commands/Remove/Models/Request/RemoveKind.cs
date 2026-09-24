namespace OpenForge.Cli.Core.Commands.Remove.Models.Request;

internal enum RemoveKind
{
    Path,
    Route,
    Extension,
    Library,
}

internal enum RemoveMode
{
    Apply,
    DryRun,
}
