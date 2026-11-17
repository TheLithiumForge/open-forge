using System.Text;

namespace OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;

// Derived from CBM v0.12.1 src/cli/config_json_like.c, jl_key_equals and jl_decode_string_value.
// Copyright (c) 2025 DeusData. MIT licence, full Codebase Memory notice in root LICENSE.
internal static class ToolOpenClawJsonStrings
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static void ValidateUtf8(ReadOnlySpan<byte> source)
    {
        try { StrictUtf8.GetCharCount(source); }
        catch (DecoderFallbackException) { throw ToolOpenClawConfigurationReader.Invalid(); }
    }

    // The only targets are the reader's four fixed ASCII names. Non-ASCII output cannot match them.
    internal static bool KeyEquals(ReadOnlySpan<byte> key, string target)
    {
        if (key[0] is (byte)'"' or (byte)'\'') key = key[1..^1];
        var position = 0;
        var targetPosition = 0;
        while (position < key.Length)
        {
            uint value = key[position++];
            if (value == '\\')
            {
                var escaped = key[position++];
                if (escaped is (byte)'\n' or (byte)'\r')
                {
                    if (escaped == '\r' && position < key.Length && key[position] == '\n') position++;
                    continue;
                }
                // Native keys don't take the selected value decoder's Unicode line-continuation branch.
                value = KeyEscape(key, ref position, escaped);
            }
            if (value > 0x7F || targetPosition >= target.Length || value != target[targetPosition++]) return false;
        }
        return targetPosition == target.Length;
    }

    internal static string DecodeValue(ReadOnlySpan<byte> token)
    {
        if (token.Length < 2 || token[0] is not ((byte)'"' or (byte)'\'') || token[^1] != token[0])
            throw ToolOpenClawConfigurationReader.Invalid();
        var body = token[1..^1];
        var output = new byte[body.Length];
        var written = 0;
        var position = 0;
        while (position < body.Length)
        {
            var current = body[position++];
            if (current != '\\') { output[written++] = current; continue; }
            var escaped = body[position++];
            if (escaped is (byte)'\n' or (byte)'\r')
            {
                if (escaped == '\r' && position < body.Length && body[position] == '\n') position++;
                continue;
            }
            if (escaped == 0xE2 && ToolOpenClawJsonSyntax.LineTerminatorWidth(body[(position - 1)..]) == 3)
            { position += 2; continue; }
            var value = ValueEscape(body, ref position, escaped);
            if (value == 0 || !Rune.TryCreate((int)value, out var rune)) throw ToolOpenClawConfigurationReader.Invalid();
            written += rune.EncodeToUtf8(output.AsSpan(written));
        }
        try { return StrictUtf8.GetString(output.AsSpan(0, written)); }
        catch (DecoderFallbackException) { throw ToolOpenClawConfigurationReader.Invalid(); }
    }

    private static uint KeyEscape(ReadOnlySpan<byte> source, ref int position, byte escaped)
    {
        switch (escaped)
        {
            case (byte)'b': return '\b';
            case (byte)'f': return '\f';
            case (byte)'n': return '\n';
            case (byte)'r': return '\r';
            case (byte)'t': return '\t';
            case (byte)'v': return '\v';
            case (byte)'0': return 0;
            case (byte)'x':
                var hex = ToolOpenClawJsonSyntax.Hex(source.Slice(position, 2));
                position += 2;
                return hex;
            case (byte)'u':
                var value = ToolOpenClawJsonSyntax.Hex(source.Slice(position, 4));
                position += 4;
                if (value is >= 0xD800 and <= 0xDBFF && source.Length - position >= 6 && source[position] == '\\' && source[position + 1] == 'u')
                {
                    var low = ToolOpenClawJsonSyntax.Hex(source.Slice(position + 2, 4));
                    if (low is >= 0xDC00 and <= 0xDFFF)
                    {
                        position += 6;
                        return 0x10000 + ((value - 0xD800) << 10) + low - 0xDC00;
                    }
                }
                return value;
            default: return escaped;
        }
    }

    // The native selected decoder requires a valid surrogate pair. Key matching does not.
    private static uint ValueEscape(ReadOnlySpan<byte> source, ref int position, byte escaped)
    {
        switch (escaped)
        {
            case (byte)'b': return '\b';
            case (byte)'f': return '\f';
            case (byte)'n': return '\n';
            case (byte)'r': return '\r';
            case (byte)'t': return '\t';
            case (byte)'v': return '\v';
            case (byte)'0': return 0;
            case (byte)'x':
                var hex = ToolOpenClawJsonSyntax.Hex(source.Slice(position, 2));
                position += 2;
                return hex;
            case (byte)'u':
                var value = ToolOpenClawJsonSyntax.Hex(source.Slice(position, 4));
                position += 4;
                if (value is >= 0xD800 and <= 0xDBFF)
                {
                    if (source.Length - position < 6 || source[position] != '\\' || source[position + 1] != 'u')
                        throw ToolOpenClawConfigurationReader.Invalid();
                    var low = ToolOpenClawJsonSyntax.Hex(source.Slice(position + 2, 4));
                    if (low is < 0xDC00 or > 0xDFFF) throw ToolOpenClawConfigurationReader.Invalid();
                    position += 6;
                    return 0x10000 + ((value - 0xD800) << 10) + low - 0xDC00;
                }
                return value;
            default: return escaped;
        }
    }
}
