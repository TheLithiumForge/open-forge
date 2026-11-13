using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.References;

[Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceLinkCodeFragmentTests
{
    [Theory(DisplayName = "File fragment classification checks Markdown before line and symbol syntax"),
        InlineData("File.cs", null, SourceLinkFragmentKind.None),
        InlineData("guide.md", "L1", SourceLinkFragmentKind.Heading),
        InlineData("guide.MD", "Type.Member", SourceLinkFragmentKind.Heading),
        InlineData("File.cs", "L1", SourceLinkFragmentKind.LineRange),
        InlineData("File.ts", "L1-L3", SourceLinkFragmentKind.LineRange),
        InlineData("File.cs", "L99999999999999999999", SourceLinkFragmentKind.LineRange),
        InlineData("File.cs", "Type.Member", SourceLinkFragmentKind.Symbol),
        InlineData("File.cs", "L1-Lx", SourceLinkFragmentKind.Symbol),
        InlineData("File.cs", "", SourceLinkFragmentKind.Symbol)]
    public void Classification(string path, string? fragment, object expected)
        => Assert.Equal((SourceLinkFragmentKind)expected, SourceLinkFileFragmentReader.Classify(path, fragment));

    [Theory(DisplayName = "File line fragments require ordered existing endpoints"),
        InlineData("one\ntwo\nthree\n", "L1", true),
        InlineData("one\r\ntwo\rthree", "L1-L3", true),
        InlineData("one", "L1-L1", true),
        InlineData("one\ntwo\nthree\n", "L3", true),
        InlineData("one\ntwo\nthree\n", "L4", false),
        InlineData("one\ntwo\nthree", "L2-L4", false),
        InlineData("one\ntwo\nthree", "L3-L2", false),
        InlineData("one", "L0", false),
        InlineData("", "L1", false),
        InlineData("one", "L99999999999999999999", false)]
    public void LineRanges(string source, string fragment, bool valid)
        => Check(source, fragment, valid, SourceLinkDestinationFindingCode.LineOutOfRange, SourceLinkTargetResolution.LineOutOfRange);

    [Theory(DisplayName = "Symbol fragments match complete case-sensitive identifier tokens for every dotted part"),
        InlineData("class Type { void Member() {} }", "Type.Member", true),
        InlineData("class Type { void Other() {} }", "Type.Member", false),
        InlineData("class TypeExtra { void Member() {} }", "Type.Member", false),
        InlineData("class Type { void MemberExtra() {} }", "Type.Member", false),
        InlineData("class Type { void Member() {} }", "type.Member", false),
        InlineData("// Type\nvar text = \"Member\";", "Type.Member", true),
        InlineData("const $name = Élément;", "$name.Élément", true),
        InlineData("class Type {}", "Type.", false),
        InlineData("class Type {}", "", false),
        InlineData("123Type", "Type", false)]
    public void Symbols(string source, string fragment, bool valid)
        => Check(source, fragment, valid, SourceLinkDestinationFindingCode.SymbolMissing, SourceLinkTargetResolution.SymbolMissing);

    private static void Check(string source, string fragment, bool valid,
        SourceLinkDestinationFindingCode code, SourceLinkTargetResolution resolution)
    {
        var target = new SourceLinkTarget
        {
            Kind = SourceLinkTargetKind.Local,
            Id = null,
            Path = "File.cs",
            PhysicalPath = null,
            Layer = null,
            Resolution = SourceLinkTargetResolution.Complete,
            Network = null,
        };
        var result = SourceLinkFileFragmentReader.ReadFailure(source, target, fragment, fragment);
        if (valid) Assert.Null(result);
        else
        {
            Assert.NotNull(result);
            Assert.Equal(code, result.Finding?.Code);
            Assert.Equal(resolution, result.Target.Resolution);
            Assert.Equal(target.Path, result.Target.Path);
            Assert.Null(result.CanonicalFragment);
        }
    }
}
