using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-manifest"), Trait("Evidence", "Unit")]
public sealed class ToolManifestTests
{
    [Fact(DisplayName = "Synthetic definitions validate without production admission")]
    public void SyntheticDefinitionIsNotAdmitted()
    {
        var definition = Read(ToolFixtureSource.Manifest());
        Assert.Equal(ToolFixtureSource.Id, definition.Manifest.Id);
        Assert.Equal(ToolFixtureSource.Version, definition.Manifest.Version);
        Assert.False(definition.Reviewed);
        Assert.Equal(ToolVocabulary.Targets.Count, definition.Manifest.Targets.Count);
    }

    [Fact(DisplayName = "Tool JSON and block YAML use the same typed model")]
    public void JsonAndYamlAgree()
    {
        var json = ToolFixtureSource.Manifest();
        var yaml = string.Join('\n', json.Select(pair => $"{pair.Key}: {pair.Value?.ToJsonString() ?? "null"}"));
        var definition = new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(yaml), "fixture.yaml", true);
        Assert.Equal(ToolFixtureSource.Id, definition.Manifest.Id);
        Assert.Equal(ToolFixtureSource.Version, definition.Manifest.Version);
        Assert.Equal(Read(json).Manifest.Artifacts["package"].Integrity, definition.Manifest.Artifacts["package"].Integrity);
    }

    [Theory(DisplayName = "Strict Tool decoding rejects unknown keys and forbidden YAML constructs")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("alias")]
    [InlineData("tag")]
    public void RejectsMalformed(string kind)
    {
        var bytes = ToolFixtureSource.ManifestBytes();
        var text = Encoding.UTF8.GetString(bytes);
        var path = ToolFixtureSource.ManifestName;
        switch (kind)
        {
            case "unknown": text = text.Replace("\"license\":", "\"unexpected\": 1, \"license\":", StringComparison.Ordinal); break;
            case "duplicate": text = text.Replace("\"schema\": 1", "\"schema\": 1, \"schema\": 1", StringComparison.Ordinal); break;
            case "alias": text = "schema: &s 1\nother: *s"; path = "fixture.yaml"; break;
            case "tag": text = "!!map {schema: 1}"; path = "fixture.yaml"; break;
        }
        Assert.Throws<JsonException>(() => new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(text), path, true));
    }

    [Theory(DisplayName = "Tool semantic validation rejects malformed required fields")]
    [InlineData("target")]
    [InlineData("version")]
    [InlineData("artifact")]
    [InlineData("skill")]
    [InlineData("optional")]
    [InlineData("disabled")]
    [InlineData("token")]
    [InlineData("template")]
    [InlineData("null-element")]
    [InlineData("graph-integrity")]
    [InlineData("enabled-without-target")]
    public void RejectsSemanticFailures(string kind)
    {
        var json = ToolFixtureSource.Manifest();
        switch (kind)
        {
            case "target": json["targets"]?.AsObject().Remove("linux-arm64"); break;
            case "version": json["version"] = "v1.0.0"; break;
            case "artifact": json["artifacts"]!["package"]!["file"] = "../escape.bin"; break;
            case "skill": json["skill"] = null; break;
            case "optional": json["skill"]!["optional"] = true; break;
            case "disabled": json["skill"]!["defaultEnabled"] = false; break;
            case "token": json["adapter"]!["id"] = "imported-code"; break;
            case "template": json["mcp"]!["args"] = new JsonArray("${unknown}"); break;
            case "null-element": json["configuration"]!["files"] = new JsonArray((JsonNode?)null); break;
            case "graph-integrity": json["artifacts"]!["package"]!["integrity"] = "sha512-" + Convert.ToBase64String(new byte[64]); break;
            case "enabled-without-target":
                json["admission"] = "enabled";
                json["targets"]!["win-x64"]!["state"] = "unsupported";
                json["targets"]!["win-x64"]!["reason"] = "Test-only unsupported target";
                break;
        }
        Assert.Throws<JsonException>(() => Read(json));
    }

    [Theory(DisplayName = "Exact versions follow full SemVer without coercion")]
    [InlineData("1.0.0", true)]
    [InlineData("1.0.0-alpha.1+build.01", true)]
    [InlineData("01.0.0", false)]
    [InlineData("1.0.0-01", false)]
    [InlineData("1.0", false)]
    [InlineData("1.0.0\n", false)]
    public void ExactVersion(string version, bool valid) => Assert.Equal(valid, ToolIdentity.IsVersion(version));

    [Fact(DisplayName = "Tool settings keep inactive pins, reject duplicate roles and preserve unrelated keys")]
    public void SettingsSplit()
    {
        var json = "{\"future\":7,\"tools\":{\"fixture-tool\":{\"version\":\"1.0.0\",\"roles\":[]}}}";
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));
        Assert.NotNull(decoded.Document);
        Assert.Empty(decoded.Document.Tools[ToolFixtureSource.Id].Roles);
        var changed = WorkspaceSettingsCodec.AddAllowInstallPath(Encoding.UTF8.GetBytes(json), "host.json");
        Assert.NotNull(changed);
        using var result = JsonDocument.Parse(changed);
        Assert.Equal(7, result.RootElement.GetProperty("future").GetInt32());
        Assert.True(result.RootElement.GetProperty("tools").TryGetProperty(ToolFixtureSource.Id, out _));
        var duplicate = "{\"tools\":{\"first\":{\"version\":\"1.0.0\",\"roles\":[\"code-navigation\"]},\"second\":{\"version\":\"1.0.0\",\"roles\":[\"code-navigation\"]}}}";
        Assert.Null(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(duplicate)).Document);
    }

    private static ToolDefinition Read(JsonObject value)
        => new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(value.ToJsonString()), ToolFixtureSource.ManifestName, true);
}
