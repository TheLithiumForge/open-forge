using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Storage;

internal sealed record ToolStoreRead(ToolStoreRegistryDocument Document, FileStateSnapshot Snapshot);
