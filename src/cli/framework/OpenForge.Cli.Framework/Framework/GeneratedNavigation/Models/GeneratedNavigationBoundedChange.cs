using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal enum GeneratedNavigationChangeKind
{
    Unchanged,
    Update,
}

internal sealed record GeneratedNavigationBoundedChange
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal GeneratedNavigationBoundedChange(GeneratedNavigationBoundedChangeInput input)
    {
        var kind = string.Equals(input.BeforeBody, input.ExpectedBody, StringComparison.Ordinal)
            ? GeneratedNavigationChangeKind.Unchanged
            : GeneratedNavigationChangeKind.Update;

        ContentLocation = input.ContentLocation;
        BeforeBody = input.BeforeBody;
        ExpectedBody = input.ExpectedBody;
        Prefix = input.Prefix;
        Suffix = input.Suffix;
        BeforeBodyBytes = ToBytes(input.BeforeBody);
        ExpectedBodyBytes = ToBytes(input.ExpectedBody);
        PrefixBytes = ToBytes(input.Prefix);
        SuffixBytes = ToBytes(input.Suffix);
        BeforeDocumentBytes = Combine(PrefixBytes, BeforeBodyBytes, SuffixBytes);
        ExpectedDocumentBytes = Combine(PrefixBytes, ExpectedBodyBytes, SuffixBytes);
        Kind = kind;
    }

    internal GeneratedNavigationChangeKind Kind { get; }

    internal SourceLocation ContentLocation { get; }

    internal string BeforeBody { get; }

    internal string ExpectedBody { get; }

    internal string Prefix { get; }

    internal string Suffix { get; }

    internal ImmutableArray<byte> BeforeBodyBytes { get; }

    internal ImmutableArray<byte> ExpectedBodyBytes { get; }

    internal ImmutableArray<byte> PrefixBytes { get; }

    internal ImmutableArray<byte> SuffixBytes { get; }

    internal ImmutableArray<byte> BeforeDocumentBytes { get; }

    internal ImmutableArray<byte> ExpectedDocumentBytes { get; }

    internal bool IsUnchanged => Kind == GeneratedNavigationChangeKind.Unchanged;

    internal bool RequiresUpdate => Kind == GeneratedNavigationChangeKind.Update;

    internal bool PrefixPreserved => HasPrefix(PrefixBytes, ExpectedDocumentBytes);

    internal bool SuffixPreserved => HasSuffix(SuffixBytes, ExpectedDocumentBytes);

    private static ImmutableArray<byte> ToBytes(string value)
        => ImmutableArray.CreateRange(StrictUtf8.GetBytes(value));

    private static ImmutableArray<byte> Combine(params ImmutableArray<byte>[] parts)
    {
        var length = parts.Sum(part => part.Length);
        var builder = ImmutableArray.CreateBuilder<byte>(length);
        foreach (var part in parts)
        {
            builder.AddRange(part);
        }

        return builder.MoveToImmutable();
    }

    private static bool HasPrefix(
        ImmutableArray<byte> prefix,
        ImmutableArray<byte> value)
    {
        return prefix.AsSpan().SequenceEqual(value.AsSpan()[..prefix.Length]);
    }

    private static bool HasSuffix(
        ImmutableArray<byte> suffix,
        ImmutableArray<byte> value)
    {
        return suffix.AsSpan().SequenceEqual(value.AsSpan()[^suffix.Length..]);
    }
}
