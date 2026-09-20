namespace OpenForge.Cli.Core.Framework.Ownership.Models.Document;

internal enum OwnedPathManager
{
    Framework,
    Extension,
}

internal sealed record OwnedPath(string Path, OwnedPathManager Manager, string Owner);
