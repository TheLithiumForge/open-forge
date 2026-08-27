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
        { "Valid generated interior", Bytes("## Entries\n\n<!-- open-forge:generated-index:start -->\n- generated\n<!-- open-forge:generated-index:end -->\n"), true, (int)MarkdownFingerprintState.Semantic, "2d253cbfbaea22ccffbc10d37e25d5ed52db632b6accf1698896b8d135c4e0e9", (int)MarkdownFingerprintRegionState.Valid, 54, 66, 12 },
        { "Valid generated interior before trailing empty line", Bytes("## Entries\n\n<!-- open-forge:generated-index:start -->\n- generated\n<!-- open-forge:generated-index:end -->\n\n"), true, (int)MarkdownFingerprintState.Semantic, "8e435cdf7735acb286d6354d4f422ffc31b380977f981703206a8f07214b4b68", (int)MarkdownFingerprintRegionState.Valid, 54, 66, 12 },
        { "Valid generated interior with lone CR", Bytes("## Entries\r\r<!-- open-forge:generated-index:start -->\r- generated\r<!-- open-forge:generated-index:end -->\r"), true, (int)MarkdownFingerprintState.Semantic, "2d253cbfbaea22ccffbc10d37e25d5ed52db632b6accf1698896b8d135c4e0e9", (int)MarkdownFingerprintRegionState.Valid, 54, 66, 12 },
        { "Valid empty generated interior", Bytes("## Entries\n\n<!-- open-forge:generated-index:start -->\n<!-- open-forge:generated-index:end -->\n"), true, (int)MarkdownFingerprintState.Semantic, "2d253cbfbaea22ccffbc10d37e25d5ed52db632b6accf1698896b8d135c4e0e9", (int)MarkdownFingerprintRegionState.Valid, 54, 54, 0 },
        { "Valid generated interior with non-ASCII prefix", Bytes("## Café 😀\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- generated\n<!-- open-forge:generated-index:end -->\n"), true, (int)MarkdownFingerprintState.Semantic, "7acf7ddfb41fa3bdb5722815140fdd57209ff91b5ddf90c243094f356f37ed20", (int)MarkdownFingerprintRegionState.Valid, 69, 81, 12 },
        { "Generated markers in inline code", Bytes("`<!-- open-forge:generated-index:start -->`\n`<!-- open-forge:generated-index:end -->`\n"), true, (int)MarkdownFingerprintState.Semantic, "ea6736d0f129d6453a00aaf2da87e564860193c2de4921f1526e478e7c70ba92", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Generated markers in fenced code", Bytes("```\n<!-- open-forge:generated-index:start -->\n<!-- open-forge:generated-index:end -->\n```\n"), true, (int)MarkdownFingerprintState.Semantic, "4b30cbba625d60044686ebd2fc51263bb1947b421b8729625bb9478f640ed2ba", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Malformed generated marker", Bytes("## Entries\n<!-- open-forge:generated-index:bogus -->\n"), true, (int)MarkdownFingerprintState.ExactBytes, "cb0e5ff1bf333033c40d3aaa320fcc1ef9771cfc93ebc24be26a433e761d81d7", (int)MarkdownFingerprintRegionState.Invalid, null, null, null },
        { "Reversed boundary", Bytes("## Entries\n<!-- open-forge:generated-index:end -->\n- route\n<!-- open-forge:generated-index:start -->\n"), true, (int)MarkdownFingerprintState.ExactBytes, "fa690b96fc34ae0c7fd05e3d4318d947b1d7eef4c33cc745ed8cc5cf4fae10bb", (int)MarkdownFingerprintRegionState.Invalid, null, null, null },
        { "Unsupported bytes", Bytes("{}\r"), false, (int)MarkdownFingerprintState.ExactBytes, "f545623b541a21d6b8b415ee1793b91001a50ca985d26fad253c3c68aba5ffe9", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Binary NUL", new byte[] { 0x00, (byte)'a', 0x0D, 0x0A }, true, (int)MarkdownFingerprintState.ExactBytes, "c4cbb7cbfda0feb8dde8cd2e8abfb0771fc2c04fe50720acc3b83971937b8ac3", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Invalid UTF-8", new byte[] { 0xFF, 0x0D, 0x0A }, true, (int)MarkdownFingerprintState.ExactBytes, "1320b5dc13aa91dbac6eabc346cb655592aef8244a8ed04b8c4b3bdd59b8af4c", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Unterminated frontmatter", Bytes("---\nopen-forge:\n  tags: [One]\n# Body\n"), true, (int)MarkdownFingerprintState.ExactBytes, "07e98622c59f111bd94e991a3b29135ed72f09e309416e091ba0eea218912a23", (int)MarkdownFingerprintRegionState.Unavailable, null, null, null },
        { "Deterministic repeat, first run", Bytes("repeat\nvalue"), true, (int)MarkdownFingerprintState.Semantic, "d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
        { "Deterministic repeat, second run", Bytes("repeat\nvalue"), true, (int)MarkdownFingerprintState.Semantic, "d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38", (int)MarkdownFingerprintRegionState.Absent, null, null, 0 },
    };

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
        Assert.Equal((MarkdownFingerprintRegionState)expectedRegion == MarkdownFingerprintRegionState.Valid, facts.Region.MarkerLinesRetained);
    }

    [Fact(DisplayName = "open-forge-markdown-v1 repeat reads are deterministic and do not share mutable output"), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void RepeatedReadsRemainEqual()
    {
        var bytes = Bytes("## Entries\r\n\r\n<!-- open-forge:generated-index:start -->\r\n- generated\r\n<!-- open-forge:generated-index:end -->\r\n");
        var reader = new MarkdownFingerprintReader();

        var first = reader.Read(bytes);
        var second = reader.Read(bytes);

        Assert.Equal(first, second);
        Assert.Equal(54, first.Region.StartByteOffset);
        Assert.Equal(66, first.Region.EndByteOffset);
        Assert.Equal(12, first.Region.ExcludedInteriorByteLength);
        Assert.True(first.Region.MarkerLinesRetained);
    }

    [Fact(DisplayName = "open-forge-markdown-v1 maps parser candidate states without a second grammar")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void CandidateStatesMapToFingerprintStates()
    {
        var cases = new[]
        {
            ("## Entries\n", MarkdownFingerprintState.Semantic, MarkdownFingerprintRegionState.Absent),
            ("<!-- open-forge:generated-index:bogus -->\n", MarkdownFingerprintState.ExactBytes, MarkdownFingerprintRegionState.Invalid),
            ("## Entries   \n<!-- open-forge:generated-index:start -->\nbody\n<!-- open-forge:generated-index:end -->\n", MarkdownFingerprintState.ExactBytes, MarkdownFingerprintRegionState.Invalid),
            ("## Entries\n  <!-- open-forge:generated-index:start -->\nbody\n<!-- open-forge:generated-index:end -->\n", MarkdownFingerprintState.ExactBytes, MarkdownFingerprintRegionState.Invalid),
            ("`<!-- open-forge:generated-index:start -->`\n```\n<!-- open-forge:generated-index:end -->\n```\n", MarkdownFingerprintState.Semantic, MarkdownFingerprintRegionState.Absent),
        };

        foreach (var (source, expectedState, expectedRegion) in cases)
        {
            var facts = new MarkdownFingerprintReader().Read(Bytes(source));

            Assert.Equal(expectedState, facts.State);
            Assert.Equal(expectedRegion, facts.Region.State);
        }
    }

    [Fact(DisplayName = "open-forge-markdown-v1 retains authored content before generated markers")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void AuthoredContentBeforeMarkerRemainsSemanticWithUtf8Offsets()
    {
        const string start = "<!-- open-forge:generated-index:start -->";
        const string end = "<!-- open-forge:generated-index:end -->";
        const string authored = "α authored route\n";
        const string generated = "- generated\n";
        var source = $"## Entries\n{authored}{start}\n{generated}{end}\n";
        var bytes = Bytes(source);

        var facts = new MarkdownFingerprintReader().Read(bytes);

        Assert.Equal(MarkdownFingerprintState.Semantic, facts.State);
        Assert.Equal(MarkdownFingerprintRegionState.Valid, facts.Region.State);
        var omissionStart = source.IndexOf(generated, StringComparison.Ordinal);
        var omissionEnd = source.IndexOf(end, StringComparison.Ordinal);
        Assert.True(omissionStart >= 0);
        Assert.True(omissionEnd > omissionStart);
        Assert.Equal(Encoding.UTF8.GetByteCount(source[..omissionStart]), facts.Region.StartByteOffset);
        Assert.Equal(Encoding.UTF8.GetByteCount(source[..omissionEnd]), facts.Region.EndByteOffset);
        Assert.Equal(Encoding.UTF8.GetByteCount(generated), facts.Region.ExcludedInteriorByteLength);

        var retained = $"## Entries\n{authored}{start}\n{end}\n";
        var expectedHash = Convert.ToHexStringLower(SHA256.HashData(Bytes(retained)));
        Assert.Equal(expectedHash, facts.Sha256);
    }

    [Theory(DisplayName = "open-forge-markdown-v1 falls back exactly for horizontal whitespace after the end marker")]
    [InlineData(" ")]
    [InlineData("\t")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void HorizontalWhitespaceAfterEndMarkerUsesExactFallback(string trailing)
    {
        var source =
            "## Entries\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "body\n"
            + "<!-- open-forge:generated-index:end -->\n"
            + trailing;
        var bytes = Bytes(source);

        var facts = new MarkdownFingerprintReader().Read(bytes);

        Assert.Equal(MarkdownFingerprintState.ExactBytes, facts.State);
        Assert.Equal(MarkdownFingerprintRegionState.Invalid, facts.Region.State);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), facts.Sha256);
    }

    [Fact(DisplayName = "open-forge-markdown-v1 accepts extra empty lines after a valid generated region")]
    [Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void ExtraEmptyLinesAfterEndMarkerRemainValid()
    {
        var facts = new MarkdownFingerprintReader().Read(Bytes(
            "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "body\n"
            + "<!-- open-forge:generated-index:end -->\n\n"));

        Assert.Equal(MarkdownFingerprintState.Semantic, facts.State);
        Assert.Equal(MarkdownFingerprintRegionState.Valid, facts.Region.State);
    }

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
