using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownFingerprintReaderTests
{
    public static TheoryData<string, byte[], bool, int, string, int, int?, int?, int?> FingerprintVectors => new()
    {
        { "LF baseline", Bytes("alpha\n"), true, (int)MarkdownFingerprintState.Semantic, "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "CRLF equals LF", Bytes("alpha\r\n"), true, (int)MarkdownFingerprintState.Semantic, "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Lone CR equals LF", Bytes("alpha\r"), true, (int)MarkdownFingerprintState.Semantic, "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Authored whitespace", Bytes("alpha  \n"), true, (int)MarkdownFingerprintState.Semantic, "a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Authored final-newline difference", Bytes("alpha"), true, (int)MarkdownFingerprintState.Semantic, "8ed3f6ad685b959ead7022518e1af76cd816f8e8ec7ccdda1ed4018e8f2223f8", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Unicode", Bytes("café\n"), true, (int)MarkdownFingerprintState.Semantic, "7b49b9e063bd91a4f9252b413261f5557b9c570aa61516989499f64a62dbcdd6", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Leading BOM retained", new byte[] { 0xEF, 0xBB, 0xBF, (byte)'a', (byte)'l', (byte)'p', (byte)'h', (byte)'a', (byte)'\n' }, true, (int)MarkdownFingerprintState.Semantic, "9eff3bdb19b9bef9372b96a1244c0f0f939acdb8f539551e0a099a5b20a3862e", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Generated heading body", Bytes("## Entries\n\n- generated\n"), true, (int)MarkdownFingerprintState.Semantic, "e4d94a79f3beb0b14c191662483cbe8bef1152bd29247ef36331f691aabbf332", (int)MarkdownFingerprintRegionState.Valid, 12, 24, 12 },
        { "Generated CRLF body", Bytes("## Entries\r\n\r\n- generated\r\n"), true, (int)MarkdownFingerprintState.Semantic, "e4d94a79f3beb0b14c191662483cbe8bef1152bd29247ef36331f691aabbf332", (int)MarkdownFingerprintRegionState.Valid, 12, 24, 12 },
        { "Empty generated body", Bytes("## Entries"), true, (int)MarkdownFingerprintState.Semantic, "2a38f622e9c1d12f9d062fa1b6062b6f3c2ceaa0de30b6687c77a9ee66039b71", (int)MarkdownFingerprintRegionState.Valid, 10, 10, 0 },
        { "Unicode prefix", Bytes("## Café 😀\n\n## Entries\n\n- generated\n"), true, (int)MarkdownFingerprintState.Semantic, "1a03685a71ef484c7b37f40c5ae64e059d3793d490f4d396a7593596602acfac", (int)MarkdownFingerprintRegionState.Valid, 27, 39, 12 },
        { "Following authored section", Bytes("## Entries\n\n- generated\n\n## Following\nAuthored.\n"), true, (int)MarkdownFingerprintState.Semantic, "f1df4eff51202e2b477e556ee897c265a6883483e647b5291bd994ae42416b4d", (int)MarkdownFingerprintRegionState.Valid, 12, 24, 12 },
        { "Retired guard migration input", Bytes("## Entries\n\n<!-- open-forge:generated-index:start -->\n- generated\n<!-- open-forge:generated-index:end -->\n"), true, (int)MarkdownFingerprintState.Semantic, "2d253cbfbaea22ccffbc10d37e25d5ed52db632b6accf1698896b8d135c4e0e9", (int)MarkdownFingerprintRegionState.Valid, 54, 66, 12 },
        { "Duplicate sections", Bytes("## Entries\n\n## Entries\n"), true, (int)MarkdownFingerprintState.ExactBytes, "7ab5dc3f8dbe26ecf1a0580e88774bf5e5c175bf05276a5033de2ea9d14aa1f7", (int)MarkdownFingerprintRegionState.Invalid, null, null, null },
        { "Unsupported bytes", Bytes("{}\r"), false, (int)MarkdownFingerprintState.ExactBytes, "f545623b541a21d6b8b415ee1793b91001a50ca985d26fad253c3c68aba5ffe9", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Binary NUL", new byte[] { 0x00, (byte)'a', 0x0D, 0x0A }, true, (int)MarkdownFingerprintState.ExactBytes, "c4cbb7cbfda0feb8dde8cd2e8abfb0771fc2c04fe50720acc3b83971937b8ac3", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Invalid UTF-8", new byte[] { 0xFF, 0x0D, 0x0A }, true, (int)MarkdownFingerprintState.ExactBytes, "1320b5dc13aa91dbac6eabc346cb655592aef8244a8ed04b8c4b3bdd59b8af4c", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Unterminated frontmatter", Bytes("---\nopen-forge:\n  tags: [One]\n# Body\n"), true, (int)MarkdownFingerprintState.ExactBytes, "07e98622c59f111bd94e991a3b29135ed72f09e309416e091ba0eea218912a23", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Deterministic repeat, first run", Bytes("repeat\nvalue"), true, (int)MarkdownFingerprintState.Semantic, "d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Deterministic repeat, second run", Bytes("repeat\nvalue"), true, (int)MarkdownFingerprintState.Semantic, "d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
    };

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "open-forge-markdown-v1 reproduces every frozen byte vector"), MemberData(nameof(FingerprintVectors)), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void FrozenVectorsRemainByteExact(
        string name,
        byte[] input,
        bool supportedMarkdown,
        int expectedState,
        string expectedHash,
        int expectedRegion,
        int? expectedStart,
        int? expectedEnd,
        int? expectedExcluded)
    {
        Assert.False(string.IsNullOrWhiteSpace(name));
        var facts = new MarkdownFingerprintReader().Read(input, supportedMarkdown);

        Assert.Equal((MarkdownFingerprintState)expectedState, facts.State);
        Assert.Equal(MarkdownFingerprintReader.Policy, facts.Policy);
        Assert.Equal(expectedHash, facts.Sha256);
        Assert.Equal((MarkdownFingerprintRegionState)expectedRegion, facts.Region.State);
        Assert.Equal(expectedStart, facts.Region.StartByteOffset);
        Assert.Equal(expectedEnd, facts.Region.EndByteOffset);
        Assert.Equal(expectedExcluded, facts.Region.ExcludedInteriorByteLength);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Semantic fingerprints ignore only the first Entries list and retain surrounding authored bytes")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void GeneratedChangesDoNotChangeAuthoredFingerprint()
    {
        var reader = new MarkdownFingerprintReader();
        const string prefix = "# Café 😀\n\nAuthored.\n\n## Entries";
        const string suffix = "## Following\nKeep this.\n";
        var first = reader.Read(Bytes(prefix + "\n\n- one\n\n" + suffix));
        var second = reader.Read(Bytes(prefix + "\n\n- two\n- three\n\n" + suffix));
        Assert.Equal(first.Sha256, second.Sha256);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes(prefix + "\n\n\n" + suffix))), first.Sha256);
        Assert.Equal(Encoding.UTF8.GetByteCount(prefix + "\n\n"), first.Region.StartByteOffset);
        Assert.NotEqual(first.Sha256, reader.Read(Bytes(prefix + "\n\n- one\n\n" + suffix + "Changed.")).Sha256);
        Assert.Equal(first, reader.Read(Bytes(prefix + "\n\n- one\n\n" + suffix)));
        Assert.NotEqual(first.Sha256, reader.Read(Bytes(prefix + "\n\nBefore.\n- one\n\n" + suffix)).Sha256);
        Assert.NotEqual(first.Sha256, reader.Read(Bytes(prefix + "\n\n- one\n\nAfter.\n" + suffix)).Sha256);
        Assert.NotEqual(first.Sha256, reader.Read(Bytes(prefix + "\n\n- one\n\n- Authored second list\n" + suffix)).Sha256);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "open-forge-markdown-v1 keeps an unavailable body boundary distinct from invalid grammar")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void UnterminatedFrontmatterUsesUnavailableRegionFallback()
    {
        var bytes = Bytes("---\nopen-forge:\n  tags: [One]\n# Body\n");

        var facts = new MarkdownFingerprintReader().Read(bytes);

        Assert.Equal(MarkdownFingerprintState.ExactBytes, facts.State);
        Assert.Equal(MarkdownFingerprintRegionState.Unavailable, facts.Region.State);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), facts.Sha256);
    }

    private static byte[] Bytes(string value) => Encoding.UTF8.GetBytes(value);
}
