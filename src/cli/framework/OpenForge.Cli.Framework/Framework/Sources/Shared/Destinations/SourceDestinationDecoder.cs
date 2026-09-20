using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

internal static class SourceDestinationDecoder
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static bool TryDecode(
        string destination,
        [NotNullWhen(true)] out string? decoded,
        out SourceDestinationDecodeFailure failure)
    {
        decoded = null;
        failure = SourceDestinationDecodeFailure.None;
        var builder = new StringBuilder(destination.Length);
        for (var index = 0; index < destination.Length;)
        {
            var character = destination[index];
            if (char.IsWhiteSpace(character))
            {
                failure = SourceDestinationDecodeFailure.UnencodedWhitespace;
                return false;
            }

            if (character != '%')
            {
                builder.Append(character);
                index++;
                continue;
            }

            var bytes = new List<byte>();
            while (index < destination.Length && destination[index] == '%')
            {
                if (index + 2 >= destination.Length
                    || !TryHex(destination[index + 1], out var high)
                    || !TryHex(destination[index + 2], out var low))
                {
                    failure = SourceDestinationDecodeFailure.InvalidPercentTriplet;
                    return false;
                }

                bytes.Add((byte)((high << 4) | low));
                index += 3;
            }

            try
            {
                builder.Append(StrictUtf8.GetString(bytes.ToArray()));
            }
            catch (DecoderFallbackException)
            {
                failure = SourceDestinationDecodeFailure.InvalidUtf8;
                return false;
            }
        }

        decoded = builder.ToString();
        return true;
    }

    private static bool TryHex(char character, out int value)
    {
        value = character switch
        {
            >= '0' and <= '9' => character - '0',
            >= 'a' and <= 'f' => character - 'a' + 10,
            >= 'A' and <= 'F' => character - 'A' + 10,
            _ => -1,
        };
        return value >= 0;
    }
}
