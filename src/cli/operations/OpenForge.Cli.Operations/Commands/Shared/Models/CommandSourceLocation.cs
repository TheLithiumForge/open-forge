using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Shared.Models;

internal readonly record struct CommandSourceLocation(int Line, int Column, long ByteOffset, long ByteLength)
{
    internal static CommandSourceLocation From(SourceLocation location)
        => new(location.Line, location.Column, location.ByteOffset, location.ByteLength);
}
