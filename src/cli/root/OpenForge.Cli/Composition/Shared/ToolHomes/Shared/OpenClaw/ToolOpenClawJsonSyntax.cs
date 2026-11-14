namespace OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;

// Derived from CBM v0.12.1 src/cli/config_json_like.c, jl_parse_* and jl_skip_trivia.
// Copyright (c) 2025 DeusData. MIT licence, full Codebase Memory notice in root LICENSE.
// Contract: .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md#native-config-reader (I1).
internal ref struct ToolOpenClawJsonSyntax(ReadOnlySpan<byte> source)
{
    internal const int MaximumDepth = 64;
    private readonly ReadOnlySpan<byte> _source = source;
    internal int Position { get; set; }
    private byte Current => Position < _source.Length ? _source[Position] : throw ToolOpenClawConfigurationReader.Invalid();

    internal int ValidateDocument()
    {
        ToolOpenClawJsonStrings.ValidateUtf8(_source);
        SkipTrivia();
        var root = Position;
        if (Current != '{') throw ToolOpenClawConfigurationReader.Invalid();
        ReadValue(0);
        SkipTrivia();
        if (Position != _source.Length) throw ToolOpenClawConfigurationReader.Invalid();
        return root;
    }

    internal void SkipTrivia()
    {
        while (Position < _source.Length)
        {
            var width = WhitespaceWidth();
            if (width > 0) { Position += width; continue; }
            if (Current != '/' || _source.Length - Position < 2) return;
            var next = _source[Position + 1];
            if (next == '/')
            {
                Position += 2;
                while (Position < _source.Length && LineTerminatorWidth(_source[Position..]) == 0) Position++;
                continue;
            }
            if (next != '*') return;
            Position += 2;
            while (_source.Length - Position >= 2 && !(_source[Position] == '*' && _source[Position + 1] == '/')) Position++;
            if (_source.Length - Position < 2) throw ToolOpenClawConfigurationReader.Invalid();
            Position += 2;
        }
    }

    internal Range ReadKey()
    {
        var start = Position;
        if (Current is (byte)'\'' or (byte)'"') ReadString();
        else ReadIdentifier();
        return start..Position;
    }

    internal Range ReadValue(int depth)
    {
        if (depth > MaximumDepth) throw ToolOpenClawConfigurationReader.Invalid();
        var start = Position;
        switch (Current)
        {
            case (byte)'{': ReadContainer(depth, isObject: true); break;
            case (byte)'[': ReadContainer(depth, isObject: false); break;
            case (byte)'\'':
            case (byte)'"': ReadString(); break;
            default:
                if (!TakeWord("true"u8) && !TakeWord("false"u8) && !TakeWord("null"u8)) ReadNumber();
                break;
        }
        return start..Position;
    }

    internal bool Take(byte value)
    {
        if (Position >= _source.Length || _source[Position] != value) return false;
        Position++;
        return true;
    }

    internal void Require(byte value)
    {
        if (!Take(value)) throw ToolOpenClawConfigurationReader.Invalid();
    }

    private void ReadContainer(int depth, bool isObject)
    {
        if (depth >= MaximumDepth) throw ToolOpenClawConfigurationReader.Invalid();
        var close = isObject ? (byte)'}' : (byte)']';
        Position++;
        SkipTrivia();
        if (Take(close)) return;
        while (true)
        {
            if (isObject) { ReadKey(); SkipTrivia(); Require((byte)':'); SkipTrivia(); }
            ReadValue(depth + 1);
            SkipTrivia();
            if (Take(close)) return;
            Require((byte)',');
            SkipTrivia();
            if (Take(close)) return;
        }
    }

    private void ReadString()
    {
        var quote = Current;
        Position++;
        while (Position < _source.Length)
        {
            var current = _source[Position++];
            if (current == quote) return;
            if (current < 0x20) throw ToolOpenClawConfigurationReader.Invalid();
            if (current != '\\')
            {
                if (current == 0xE2 && LineTerminatorWidth(_source[(Position - 1)..]) == 3) throw ToolOpenClawConfigurationReader.Invalid();
                continue;
            }
            var escaped = Current;
            Position++;
            if (escaped == 'u') { SkipHex(4); continue; }
            if (escaped == 'x') { SkipHex(2); continue; }
            if (escaped is (byte)'\n' or (byte)'\r')
            {
                if (escaped == '\r') Take((byte)'\n');
                continue;
            }
            if (escaped == 0xE2 && LineTerminatorWidth(_source[(Position - 1)..]) == 3) { Position += 2; continue; }
            if (escaped < 0x20 || escaped is >= (byte)'1' and <= (byte)'9'
                || escaped == '0' && Position < _source.Length && IsDigit(_source[Position])) throw ToolOpenClawConfigurationReader.Invalid();
        }
        throw ToolOpenClawConfigurationReader.Invalid();
    }

    private void ReadIdentifier()
    {
        var first = true;
        while (Position < _source.Length)
        {
            var current = Current;
            if (IsIdentifierStart(current) || !first && IsDigit(current)) { Position++; first = false; continue; }
            if (current != '\\' || _source.Length - Position < 6 || _source[Position + 1] != 'u') break;
            var digits = _source.Slice(Position + 2, 4);
            foreach (var digit in digits)
                if (!IsHex(digit)) throw ToolOpenClawConfigurationReader.Invalid();
            var value = Hex(digits);
            if (value is >= 0xD800 and <= 0xDFFF || value < 0x80 && !(IsIdentifierStart((byte)value) || !first && IsDigit((byte)value)))
                throw ToolOpenClawConfigurationReader.Invalid();
            Position += 6;
            first = false;
        }
        if (first) throw ToolOpenClawConfigurationReader.Invalid();
    }

    private void ReadNumber()
    {
        if (!Take((byte)'-')) Take((byte)'+');
        if (TakeWord("Infinity"u8) || TakeWord("NaN"u8)) return;
        if (_source.Length - Position >= 2 && _source[Position] == '0' && _source[Position + 1] is (byte)'x' or (byte)'X')
        {
            Position += 2;
            var digits = Position;
            while (Position < _source.Length && IsHex(_source[Position])) Position++;
            if (Position == digits) throw ToolOpenClawConfigurationReader.Invalid();
            return;
        }
        var integer = Take((byte)'0');
        if (!integer) integer = SkipDigits();
        var fraction = false;
        if (Take((byte)'.')) fraction = SkipDigits();
        if (!integer && !fraction) throw ToolOpenClawConfigurationReader.Invalid();
        if (!Take((byte)'e') && !Take((byte)'E')) return;
        if (!Take((byte)'+')) Take((byte)'-');
        if (!SkipDigits()) throw ToolOpenClawConfigurationReader.Invalid();
    }

    private bool SkipDigits()
    {
        var start = Position;
        while (Position < _source.Length && IsDigit(_source[Position])) Position++;
        return Position > start;
    }

    private void SkipHex(int count)
    {
        if (_source.Length - Position < count) throw ToolOpenClawConfigurationReader.Invalid();
        for (var i = 0; i < count; i++)
            if (!IsHex(_source[Position++])) throw ToolOpenClawConfigurationReader.Invalid();
    }

    private bool TakeWord(ReadOnlySpan<byte> word)
    {
        if (!_source[Position..].StartsWith(word)) return false;
        Position += word.Length;
        return true;
    }

    private int WhitespaceWidth()
    {
        if (Current is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n' or (byte)'\f' or (byte)'\v') return 1;
        var rest = _source[Position..];
        if (rest.StartsWith("\u00a0"u8)) return 2;
        if (rest.StartsWith("\ufeff"u8)) return 3;
        return LineTerminatorWidth(rest);
    }

    internal static int LineTerminatorWidth(ReadOnlySpan<byte> rest)
    {
        if (rest.IsEmpty) return 0;
        if (rest[0] is (byte)'\n' or (byte)'\r') return 1;
        return rest.Length >= 3 && rest[0] == 0xE2 && rest[1] == 0x80 && rest[2] is 0xA8 or 0xA9 ? 3 : 0;
    }

    private static bool IsDigit(byte value) => value is >= (byte)'0' and <= (byte)'9';
    private static bool IsIdentifierStart(byte value) => value is >= (byte)'a' and <= (byte)'z' or >= (byte)'A' and <= (byte)'Z' or (byte)'_' or (byte)'$' or >= 0x80;
    private static bool IsHex(byte value) => IsDigit(value) || value is >= (byte)'a' and <= (byte)'f' or >= (byte)'A' and <= (byte)'F';
    internal static uint Hex(ReadOnlySpan<byte> digits)
    {
        uint value = 0;
        foreach (var digit in digits)
        {
            var hex = digit - '0';
            if (digit is >= (byte)'a' and <= (byte)'f') hex = digit - 'a' + 10;
            else if (digit is >= (byte)'A' and <= (byte)'F') hex = digit - 'A' + 10;
            value = (value << 4) | (uint)hex;
        }
        return value;
    }
}
