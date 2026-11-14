using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

public sealed class ToolClientConfigurationTests
{
    [Theory(DisplayName = "OpenClaw saved workspace has native precedence over environment and includes"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawSavedWorkspace(bool include)
    {
        using var fixture = new ToolHomeFixture();
        var path = fixture.NativePath("chosen-workspace");
        var config = OpenClawConfiguration(path);
        if (include) config[ToolOpenClawConfigurationReader.IncludeProperty] = "synthetic-include.json";
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], config.ToJsonString());
        var before = File.ReadAllBytes(file);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("ignored-workspace");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(path, facts.Clients.Directories[OpenClawWorkspaceRoot]);
        Assert.Equal(before, File.ReadAllBytes(file));
        Assert.False(Directory.Exists(path));
    }

    [Theory(DisplayName = "OpenClaw unavailable config never guesses a workspace or follows includes"),
     InlineData(0), InlineData(1), InlineData(2), InlineData(3),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawUnavailable(int scenario)
    {
        using var fixture = new ToolHomeFixture();
        var text = scenario switch
        {
            0 => new JsonObject { [ToolOpenClawConfigurationReader.IncludeProperty] = "unfollowed.json" }.ToJsonString(),
            1 => "{invalid",
            2 => OpenClawConfiguration("relative-workspace").ToJsonString(),
            3 => "[]",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], text);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("unused-fallback");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
        Assert.False(facts.Clients.Directories.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(text, File.ReadAllText(file));
    }

    [Fact(DisplayName = "OpenClaw profile state and workspace paths honor its internal home and overrides"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawOverrides()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.OpenClawHome] = "~/other-home";
        fixture.Environment[ToolHomeEnvironment.OpenClawProfile] = "synthetic";
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath("other-home/.openclaw-synthetic/" + ToolOpenClawRoots.ConfigurationFile), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
        Assert.Equal(fixture.NativePath("other-home/.openclaw/workspace-synthetic"), facts.Clients.Directories[OpenClawWorkspaceRoot]);
        fixture.Environment[ToolHomeEnvironment.OpenClawState] = "~/other-state";
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = "~/other-workspace";
        facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath("other-home/other-state/" + ToolOpenClawRoots.ConfigurationFile), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
        Assert.Equal(fixture.NativePath("other-home/other-workspace"), facts.Clients.Directories[OpenClawWorkspaceRoot]);
        fixture.Environment[ToolHomeEnvironment.OpenClawConfig] = fixture.File("arbitrary-openclaw.json", "{/* comment */}");
        facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.Environment[ToolHomeEnvironment.OpenClawConfig], facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(OpenClaw)]);
    }

    [Fact(DisplayName = "OpenClaw duplicate path keys are unavailable instead of selecting an arbitrary value"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void OpenClawDuplicate()
    {
        using var fixture = new ToolHomeFixture();
        fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], $"{{\"{ToolOpenClawConfigurationReader.AgentsProperty}\":{{}},\"{ToolOpenClawConfigurationReader.AgentsProperty}\":{{}}}}");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
    }

    [Theory(DisplayName = "Rovo YAML path override is resolved only inside the native client root"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void RovoPath(bool contained)
    {
        using var fixture = new ToolHomeFixture();
        var expected = fixture.NativePath(".rovodev/owned-config.json");
        var value = contained ? "~/.rovodev/owned-config.json" : fixture.NativePath("outside-config.json");
        fixture.File(ToolClientConfigurationReader.RovoConfiguration,
            $"{ToolClientConfigurationReader.RovoMcp}:\n  {ToolClientConfigurationReader.RovoMcpPath}: '{value}'\n");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        var key = ToolHomePathOracle.ConfigurationKey(Rovo);
        if (contained) Assert.Equal(expected, facts.Clients.Files[key]);
        else Assert.True(facts.Clients.Unavailable.ContainsKey(key));
        Assert.False(File.Exists(expected));
    }

    [Theory(DisplayName = "Rovo malformed or aliased configuration leaves its MCP target unavailable"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void RovoUnavailable(bool alias)
    {
        using var fixture = new ToolHomeFixture();
        var value = alias ? "*missing-alias" : "[broken";
        fixture.File(ToolClientConfigurationReader.RovoConfiguration,
            $"{ToolClientConfigurationReader.RovoMcp}:\n  {ToolClientConfigurationReader.RovoMcpPath}: {value}\n");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(ToolHomePathOracle.ConfigurationKey(Rovo)));
    }

    [Fact(DisplayName = "File-valued and directory-valued injected bindings reject the wrong existing destination kind"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void BindingKinds()
    {
        using var fixture = new ToolHomeFixture();
        var key = ToolHomePathOracle.ConfigurationKey(Codex);
        var inputs = fixture.Inputs(new Dictionary<string, string>
        {
            [key] = fixture.Directory("wrong-file-kind"),
            [Claude] = fixture.File("wrong-directory-kind"),
        });
        var facts = fixture.Provider(inputs: inputs).Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(key));
        Assert.True(facts.Clients.Unavailable.ContainsKey(Claude));
    }

    private static JsonObject OpenClawConfiguration(string workspace) => new()
    {
        [ToolOpenClawConfigurationReader.AgentsProperty] = new JsonObject
        {
            [ToolOpenClawConfigurationReader.DefaultsProperty] = new JsonObject { [ToolOpenClawConfigurationReader.WorkspaceProperty] = workspace },
        },
    };

    [Fact(DisplayName = "Empty Rovo YAML keeps the native default filename"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void EmptyRovo()
    {
        using var fixture = new ToolHomeFixture();
        fixture.File(ToolClientConfigurationReader.RovoConfiguration, string.Empty);
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.NativePath(ToolHomePathOracle.FixedConfigurations[Rovo]), facts.Clients.Files[ToolHomePathOracle.ConfigurationKey(Rovo)]);
    }

    [Fact(DisplayName = "Native OpenClaw JSON-like syntax resolves the saved workspace without changing its bytes"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawWorkspace()
    {
        using var fixture = new ToolHomeFixture();
        var path = fixture.NativePath("native-syntax-workspace");
        var text = ToolOpenClawNativeFixtures.WithNativeNumbers(ToolOpenClawNativeFixtures.Document(ToolOpenClawNativeFixtures.StringToken(path)));
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], text);
        var before = File.ReadAllBytes(file);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("unused-fallback");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(path, facts.Clients.Directories[OpenClawWorkspaceRoot]);
        Assert.Equal(before, File.ReadAllBytes(file));
        Assert.False(Directory.Exists(path));
    }

    [Theory(DisplayName = "Missing or zero-byte OpenClaw config uses environment then profile fallback"),
     InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawEmpty(bool exists, bool useEnvironment)
    {
        using var fixture = new ToolHomeFixture();
        var file = fixture.NativePath(ToolHomePathOracle.FixedConfigurations[OpenClaw]);
        if (exists) fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], string.Empty);
        fixture.Environment[ToolHomeEnvironment.OpenClawProfile] = "synthetic";
        fixture.Environment[ToolHomeEnvironment.OpenClawConfig] = file;
        if (useEnvironment) fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("fallback-from-environment");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        var expected = useEnvironment ? fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] : fixture.NativePath(".openclaw/workspace-synthetic");
        Assert.Equal(expected, facts.Clients.Directories[OpenClawWorkspaceRoot]);
        Assert.Equal(exists, File.Exists(file));
        if (exists) Assert.Empty(File.ReadAllBytes(file));
        Assert.False(Directory.Exists(expected));
    }

    [Theory(DisplayName = "Native OpenClaw invalid or empty configured paths block environment fallback"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawInvalidPath(bool empty)
    {
        using var fixture = new ToolHomeFixture();
        fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], ToolOpenClawNativeFixtures.Document(ToolOpenClawNativeFixtures.StringToken(empty ? string.Empty : "relative")));
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("must-not-use");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
    }

    [Fact(DisplayName = "Native OpenClaw include guard never opens an otherwise valid included workspace"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawInclude()
    {
        using var fixture = new ToolHomeFixture();
        var include = fixture.File("unfollowed.json", ToolOpenClawNativeFixtures.Document(ToolOpenClawNativeFixtures.StringToken(fixture.NativePath("included-workspace"))));
        fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], new JsonObject { [ToolOpenClawConfigurationReader.IncludeProperty] = include }.ToJsonString());
        var before = File.ReadAllBytes(include);
        fixture.Environment[ToolHomeEnvironment.OpenClawWorkspace] = fixture.NativePath("must-not-use");
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(before, File.ReadAllBytes(include));
    }

    [Theory(DisplayName = "OpenClaw file reads keep the existing eight MiB bound"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawReadBound(bool oversized)
    {
        using var fixture = new ToolHomeFixture();
        var size = ToolClientConfigurationReader.MaximumConfigurationBytes + (oversized ? 1 : 0);
        var text = "{}" + new string(' ', size - 2);
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw], text);
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(oversized, facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(!oversized, facts.Clients.Directories.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(size, new FileInfo(file).Length);
        Assert.Equal(text, File.ReadAllText(file));
    }

    [Fact(DisplayName = "OpenClaw invalid UTF-8 file bytes stay unavailable and unchanged"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NativeOpenClawUtf8()
    {
        using var fixture = new ToolHomeFixture();
        var file = fixture.File(ToolHomePathOracle.FixedConfigurations[OpenClaw]);
        var bytes = ToolOpenClawNativeFixtures.InvalidUtf8()[0];
        File.WriteAllBytes(file, bytes);
        var facts = fixture.Provider().Resolve(ToolHomeFixture.Profile(Id));
        Assert.True(facts.Clients.Unavailable.ContainsKey(OpenClawWorkspaceRoot));
        Assert.Equal(bytes, File.ReadAllBytes(file));
    }
}

// One external-format oracle shared by the owned-file and pure-reader cases.
// CBM v0.12.1 src/cli/config_json_like.c:138-631, 696-867, 2724-2842 and 3055-3118.
internal static class ToolOpenClawNativeFixtures
{
    internal const string ConfiguredPath = "~/chosen";
    private const int LongTokenLength = 5000;
    private static string Agents => ToolOpenClawConfigurationReader.AgentsProperty;
    private static string Defaults => ToolOpenClawConfigurationReader.DefaultsProperty;
    private static string Workspace => ToolOpenClawConfigurationReader.WorkspaceProperty;
    private static string Include => ToolOpenClawConfigurationReader.IncludeProperty;

    internal static string Document(string token, char quote = default)
    {
        var agents = quote == default ? Agents : $"{quote}{Agents}{quote}";
        var defaults = quote == default ? Defaults : $"{quote}{Defaults}{quote}";
        var workspace = quote == default ? Workspace : $"{quote}{Workspace}{quote}";
        return $$"""
            { {{agents}}: { {{defaults}}: { {{workspace}}: {{token}} } } }
            """;
    }
    internal static string StringToken(string value) => $"\"{JsonEncodedText.Encode(value)}\"";
    internal static string WithRoot(string document, string members) => $"{document[..^1]},{members}}}";
    internal static string WithNativeNumbers(string document) => WithRoot(document, "other:1e9999,other:0xFFFF,");
    internal static string Other(string token) => $"{{other:{token}}}";

    internal static TheoryData<string, string> Configured()
    {
        var data = new TheoryData<string, string>
        {
            { Document($"'{ConfiguredPath}'"), ConfiguredPath },
            { Document(StringToken(ConfiguredPath)), ConfiguredPath },
            { Document($"'{ConfiguredPath}'", '\''), ConfiguredPath },
            { Document(StringToken(ConfiguredPath), '"'), ConfiguredPath },
            { Document("''"), string.Empty },
            { Document(@"'~\x2fch\u006fsen'"), ConfiguredPath },
            { Document(@"'\xE9'"), "é" },
            { Document(@"'~\/chosen'"), ConfiguredPath },
            { Document(@"'\q'"), "q" },
            { Document(@"'\b\f\n\r\t\v'"), "\b\f\n\r\t\v" },
            { Document(@"'\uD83D\uDE00'"), "😀" },
            { Document("'~/é'"), "~/é" },
            { Document("'a\\\nb'"), "ab" },
            { Document("'a\\\rb'"), "ab" },
            { Document("'a\\\r\nb'"), "ab" },
            { Document("'a\\\u2028b'"), "ab" },
            { Document("'a\\\u2029b'"), "ab" },
            { Document($"'{ConfiguredPath}'").Replace(Agents, "ag\\u0065nts", StringComparison.Ordinal), ConfiguredPath },
            { Document($"'{ConfiguredPath}'").Replace(Agents, "\\u0061gents", StringComparison.Ordinal), ConfiguredPath },
            { Document($"'{ConfiguredPath}'").Replace(Agents, "'ag\\ents'", StringComparison.Ordinal), ConfiguredPath },
            { Document($"'{ConfiguredPath}'").Replace(Agents, "'ag\\x65nts'", StringComparison.Ordinal), ConfiguredPath },
            { WithRoot(Document($"'{ConfiguredPath}'"), $"{Include}:null,{Include}:false"), ConfiguredPath },
            { WithRoot(Document($"'{ConfiguredPath}'"), "other:1,other:2"), ConfiguredPath },
            { Document($"'{ConfiguredPath}'") + "//tail", ConfiguredPath },
        };
        return data;
    }

    internal static string[] Missing() =>
    [
        string.Empty, "{}", $"{{{Agents}:{{}}}}", $"{{{Agents}:{{{Defaults}:{{}}}}}}",
        $"{{other:{{{Include}:'unused'}}}}", Other(@"'\u0000\uD800'"),
        Document($"'{ConfiguredPath}'").Replace(Agents, "Agents", StringComparison.Ordinal),
        Document($"'{ConfiguredPath}'").Replace(Agents, "'ag\\\u2028ents'", StringComparison.Ordinal),
        Document($"'{ConfiguredPath}'").Replace(Agents, "'ag\\\u2029ents'", StringComparison.Ordinal),
        Other("'\\é'"), $"{{{new string('a', LongTokenLength)}:1}}", "{é:1}", "{other2:0}", "{other:[true,false,null,{},[],],}",
    ];

    internal static string[] Trivia() =>
    [
        " ", "\t", "\r", "\n", "\f", "\v", "\u00a0", "\u2028", "\u2029", "\ufeff",
        "//comment\n", "//comment\r", "//comment\u2028", "//comment\u2029", "/*comment*/",
    ];

    internal static string[] Numbers() =>
    [
        "1e9999", new string('9', LongTokenLength), "0x" + new string('F', LongTokenLength),
        "+Infinity", "-Infinity", "Infinity", "+NaN", "-NaN", "NaN", ".5", "+.5", "-.5", "1.", "1.e+2", "-0Xf",
    ];

    internal static string[] Invalid()
    {
        var document = Document($"'{ConfiguredPath}'");
        return
        [
            " ", "/*comment*/", "//comment", "[]", "null", "false", "42", "'text'",
            document + "garbage", document + "{}", document + "/*", "{a:1 b:2}", "{a 1}", "{,a:1}", "{a:1,,}",
            "{a:[,1]}", "{a:[1,,2]}", "{a:[1 2]}", "{a:[}", "{a:'unterminated}", "{a:1,",
            "{a:\u2000null}", "{a:'raw\nline'}", "{a:'raw\u2028line'}", "{a:'raw\u2029line'}",
            Other(@"'\xG0'"), Other(@"'\u00G0'"), Other(@"'\u123'"), Other(@"'\x0'"), Other(@"'\1'"), Other(@"'\01'"),
            Document(@"'\0'"), Document(@"'\u0000'"), Document(@"'\uD800'"), Document(@"'\uDC00'"), Document(@"'\uD800\u0001'"), Document("'\\é'"),
            $"{{{Agents}:null}}", $"{{{Agents}:{{{Defaults}:[]}}}}",
            Document("null"), Document("{}"), Document("[]"), Document("true"), Document("false"), Document("0"),
            $"{{{Agents}:{{}},{Agents}:{{}}}}", $"{{{Agents}:{{}},ag\\u0065nts:{{}}}}",
            $"{{{Agents}:{{{Defaults}:{{}},{Defaults}:{{}}}}}}",
            $"{{{Agents}:{{{Defaults}:{{{Workspace}:'a',{Workspace}:'b'}}}}}}",
            WithRoot(document, "other:1e+"), "{1name:1}", "{\\u0030name:1}", "{\\uD800:1}",
        ];
    }

    internal static string[] InvalidNumbers() => ["1e", "1e+", "0x", "+", "-", ".", "01", "0x1g", "NaNx"];
    internal static string IncludeDocument(string token, bool duplicate) => duplicate ? $"{{{Include}:{token},{Include}:{token}}}" : $"{{{Include}:{token}}}";
    internal static string[] IncludeValues() => ["''", "null", "false", "0", "[]", "{}"];
    internal static string Nested(int count, bool objects) => Other(string.Concat(Enumerable.Repeat(objects ? "{other:" : "[", count)) + "0"
        + string.Concat(Enumerable.Repeat(objects ? "}" : "]", count)));
    internal static byte[][] InvalidUtf8() =>
    [
        [.. "{}/*"u8, 0xC0, 0xAF, .. "*/"u8], [.. "{other:'"u8, 0xC2, 0x20, .. "'}"u8],
        [.. "{}//"u8, 0xE2, 0x82], [.. "{}/*"u8, 0xED, 0xA0, 0x80, .. "*/"u8],
        [.. "{}/*"u8, 0xF4, 0x90, 0x80, 0x80, .. "*/"u8],
    ];
}
