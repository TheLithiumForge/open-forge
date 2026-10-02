using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdateAdoptionTarget(
    string Path,
    FileStateSnapshot Snapshot,
    byte[] IntendedDocumentBytes,
    bool OwnsGeneratedEntries);
