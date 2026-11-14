using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Storage;

internal sealed record ToolFileWrite(FileExpectation Expectation, ReadOnlyMemory<byte> Bytes, UnixFileMode? Mode = null);
