using System.Collections.Immutable;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Distribution.Models;

internal sealed class FrameworkPayloadAsset
{
    internal const string RootAgentPath = SourceLogicalPath.WorkspaceEntryPath;
    internal const string RootClaudePath = "CLAUDE.md";
    internal const string LoaderPath = SourceLogicalPath.LoaderPath;

    private FrameworkPayloadAsset(
        string path,
        ImmutableArray<byte> bytes,
        string sha256)
    {
        Path = path;
        Bytes = bytes;
        Sha256 = sha256;
    }

    internal string Path { get; }

    internal ImmutableArray<byte> Bytes { get; }

    internal int ByteLength => Bytes.Length;

    internal string Sha256 { get; }

    internal static FrameworkPayloadAsset Create(
        string path,
        ReadOnlySpan<byte> bytes)
    {
        ValidatePath(path);
        var values = ImmutableArray.CreateRange(bytes.ToArray());
        var sha256 = Convert.ToHexStringLower(SHA256.HashData(values.AsSpan()));
        return new FrameworkPayloadAsset(path, values, sha256);
    }

    private static void ValidatePath(string path)
    {
        if (path is RootAgentPath or RootClaudePath)
        {
            return;
        }

        if (!SourceLogicalPath.IsCanonical(path))
        {
            throw new ArgumentException("A Framework payload asset path must be canonical.", nameof(path));
        }
    }
}
