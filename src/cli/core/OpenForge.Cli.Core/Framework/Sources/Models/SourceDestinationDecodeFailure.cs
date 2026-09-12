namespace OpenForge.Cli.Core.Framework.Sources.Models;

internal enum SourceDestinationDecodeFailure
{
    None,
    UnencodedWhitespace,
    InvalidPercentTriplet,
    InvalidUtf8,
}
