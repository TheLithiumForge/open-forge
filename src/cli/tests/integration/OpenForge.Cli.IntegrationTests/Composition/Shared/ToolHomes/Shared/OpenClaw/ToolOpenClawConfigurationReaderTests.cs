using System.Text;
using OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.Shared.OpenClaw;

public sealed class ToolOpenClawConfigurationReaderTests
{
    public static TheoryData<string, string> ConfiguredCases() => ToolOpenClawNativeFixtures.Configured();
    public static TheoryData<string> MissingCases() => Cases(ToolOpenClawNativeFixtures.Missing());
    public static TheoryData<string> TriviaCases() => Cases(ToolOpenClawNativeFixtures.Trivia());
    public static TheoryData<string> NumericCases() => Cases(ToolOpenClawNativeFixtures.Numbers());
    public static TheoryData<string> InvalidCases() => Cases(ToolOpenClawNativeFixtures.Invalid().Concat(ToolOpenClawNativeFixtures.InvalidNumbers().Select(ToolOpenClawNativeFixtures.Other)));
    public static TheoryData<byte[]> Utf8Cases()
    {
        var data = new TheoryData<byte[]>();
        foreach (var value in ToolOpenClawNativeFixtures.InvalidUtf8()) data.Add(value);
        return data;
    }
    public static TheoryData<string, bool> IncludeCases()
    {
        var data = new TheoryData<string, bool>();
        foreach (var value in ToolOpenClawNativeFixtures.IncludeValues()) { data.Add(value, false); data.Add(value, true); }
        return data;
    }

    [Theory(DisplayName = "Native OpenClaw selected strings decode while input bytes stay unchanged"), MemberData(nameof(ConfiguredCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Configured(string text, string expected)
    {
        var source = Encoding.UTF8.GetBytes(text);
        var before = source.ToArray();
        Assert.Equal(expected, ToolOpenClawConfigurationReader.ReadWorkspace(source));
        Assert.Equal(before, source);
    }

    [Theory(DisplayName = "Native OpenClaw absent selected paths return missing"), MemberData(nameof(MissingCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Missing(string text) => Assert.Null(ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(text)));

    [Theory(DisplayName = "Native trivia is accepted at document and member positions"), MemberData(nameof(TriviaCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Trivia(string trivia)
    {
        var document = ToolOpenClawNativeFixtures.Document($"'{ToolOpenClawNativeFixtures.ConfiguredPath}'", '"');
        document = trivia + document.Replace(":", trivia + ":" + trivia, StringComparison.Ordinal) + trivia;
        Assert.Equal(ToolOpenClawNativeFixtures.ConfiguredPath, ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(document)));
    }

    [Theory(DisplayName = "Native unrelated numbers are skipped without value conversion"), MemberData(nameof(NumericCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NumericSkip(string token) => Assert.Null(ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(ToolOpenClawNativeFixtures.Other(token))));

    [Theory(DisplayName = "Native complete-document validation rejects syntax shape decoding and ambiguity failures"), MemberData(nameof(InvalidCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Invalid(string text) => Assert.Throws<IOException>(() => ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(text)));

    [Theory(DisplayName = "Native root include presence prevents fallback only when the selected workspace is missing"), MemberData(nameof(IncludeCases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Include(string token, bool duplicate)
        => Assert.Throws<IOException>(() => ToolOpenClawConfigurationReader.ReadWorkspace(Encoding.UTF8.GetBytes(ToolOpenClawNativeFixtures.IncludeDocument(token, duplicate))));

    [Theory(DisplayName = "Native UTF-8 validation covers unrelated values and comments"), MemberData(nameof(Utf8Cases)),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Utf8(byte[] source)
    {
        var before = source.ToArray();
        Assert.Throws<IOException>(() => ToolOpenClawConfigurationReader.ReadWorkspace(source));
        Assert.Equal(before, source);
    }

    [Theory(DisplayName = "Native depth permits containers at 63 and scalar values at 64 but rejects containers at 64"),
     InlineData(ToolOpenClawJsonSyntax.MaximumDepth - 1, false), InlineData(ToolOpenClawJsonSyntax.MaximumDepth, false),
     InlineData(ToolOpenClawJsonSyntax.MaximumDepth - 1, true), InlineData(ToolOpenClawJsonSyntax.MaximumDepth, true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void Depth(int containers, bool objects)
    {
        var source = Encoding.UTF8.GetBytes(ToolOpenClawNativeFixtures.Nested(containers, objects));
        if (containers == ToolOpenClawJsonSyntax.MaximumDepth) Assert.Throws<IOException>(() => ToolOpenClawConfigurationReader.ReadWorkspace(source));
        else Assert.Null(ToolOpenClawConfigurationReader.ReadWorkspace(source));
    }

    private static TheoryData<string> Cases(IEnumerable<string> values)
    {
        var data = new TheoryData<string>();
        foreach (var value in values) data.Add(value);
        return data;
    }
}
