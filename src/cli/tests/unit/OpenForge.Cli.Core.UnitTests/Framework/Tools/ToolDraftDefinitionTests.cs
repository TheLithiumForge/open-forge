using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-draft"), Trait("Evidence", "Unit")]
public sealed class ToolDraftDefinitionTests
{
    [Theory(DisplayName = "Graph drafts pin all six assets against the published local checksum fixture")]
    [InlineData(ToolDraftSource.CodeGraph)]
    [InlineData(ToolDraftSource.CodebaseMemory)]
    public void AssetsMatchPublishedChecksums(string pin)
    {
        using var document = Read(pin);
        var root = document.RootElement;
        var manifest = Manifest(root);
        var evidence = root.GetProperty("releaseEvidence");
        var checksumBytes = ToolDraftSource.ReadChecksums(typeof(ToolCatalogueReader).Assembly, manifest.Id);
        Assert.Equal(evidence.GetProperty("checksumSha256").GetString(), ToolCatalogueReader.Hash(checksumBytes));
        var checksums = Encoding.UTF8.GetString(checksumBytes).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(parts => parts[1], parts => parts[0], StringComparer.Ordinal);
        Assert.True(ToolVocabulary.Targets.SetEquals(manifest.Targets.Keys));
        Assert.Matches(ToolDraftSource.SourceCommitPattern, Assert.IsType<string>(manifest.Source.Commit));
        Assert.Equal("v" + manifest.Version, manifest.Source.Tag);
        foreach (var (targetId, target) in manifest.Targets)
        {
            Assert.Equal(ToolDraftSource.Blocked, target.State);
            Assert.False(string.IsNullOrWhiteSpace(target.Reason));
            var artifact = manifest.Artifacts[Assert.Single(target.Artifacts)];
            var proof = evidence.GetProperty("assets").GetProperty(targetId);
            Assert.Equal(checksums[artifact.File], proof.GetProperty("sha256").GetString());
            Assert.Equal("sha256:" + checksums[artifact.File], artifact.Integrity);
            Assert.Equal(artifact.Bytes, proof.GetProperty("bytes").GetInt64());
            Assert.Equal(manifest.DownloadBase + artifact.File, proof.GetProperty("url").GetString());
            Assert.True(proof.GetProperty("verified").GetBoolean());
            Assert.Equal(manifest.Source.Repository + "/.github/workflows/release.yml@refs/heads/main",
                proof.GetProperty("signerIdentity").GetString());
        }
    }

    [Theory(DisplayName = "Draft commands cannot invent pending arguments or qualify a native install")]
    [InlineData(ToolDraftSource.CodeGraph)]
    [InlineData(ToolDraftSource.CodebaseMemory)]
    public void PendingCommandsStayUnlaunchable(string pin)
    {
        using var document = Read(pin);
        var root = document.RootElement;
        var manifest = Manifest(root);
        Assert.Equal(ToolDraftSource.Blocked, manifest.Admission);
        Assert.Empty(manifest.Qualification.Targets);
        Assert.Null(manifest.Lifecycle);
        Assert.NotNull(new ToolReleaseSupport([], []).Blocker(manifest, ToolCatalogueReader.Hash(ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, pin))));
        foreach (var action in ToolVocabulary.LifecycleActions)
        {
            var draftKey = action == ToolVocabulary.IndexCleanupAction ? "indexCleanup" : action;
            var command = root.GetProperty("lifecycleDraft").GetProperty(draftKey);
            if (command.GetProperty("state").GetString() == ToolDraftSource.Pending)
                Assert.Equal(JsonValueKind.Null, command.GetProperty("args").ValueKind);
            else Assert.NotEmpty(command.GetProperty("args").EnumerateArray());
            Assert.Equal(JsonValueKind.Null, command.GetProperty("timeoutSeconds").ValueKind);
        }
        Assert.Throws<JsonException>(() => new ToolCatalogueReader().Definition(
            ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, pin), pin + ToolDraftSource.DefinitionExtension, true));
    }

    [Theory(DisplayName = "Graph drafts require packaged guidance and keep upstream server names")]
    [InlineData(ToolDraftSource.CodeGraph)]
    [InlineData(ToolDraftSource.CodebaseMemory)]
    public void GuidanceAndNamesAreBound(string pin)
    {
        using var document = Read(pin);
        var root = document.RootElement;
        var manifest = Manifest(root);
        Assert.NotNull(manifest.Skill);
        Assert.False(manifest.Skill.Optional);
        Assert.True(manifest.Skill.DefaultEnabled);
        var guidance = root.GetProperty("guidance");
        Assert.True(guidance.GetProperty("required").GetBoolean());
        var skill = ToolDraftSource.Read(typeof(ToolCatalogueReader).Assembly,
            guidance.GetProperty("path").GetString() ?? throw new InvalidDataException());
        Assert.Equal(guidance.GetProperty("sha256").GetString(), ToolCatalogueReader.Hash(skill));
        var server = root.GetProperty("passiveObservationProposal").GetProperty("serverName").GetString();
        Assert.Equal("/mcpServers/" + server, manifest.Mcp.Hosts["claude-code"].Selector);
        Assert.Equal("mcp_servers." + server, manifest.Mcp.Hosts["codex"].Selector);
        Assert.Empty(manifest.Configuration.Files);
        Assert.Empty(manifest.Configuration.Actions);
    }

    private static JsonDocument Read(string pin) => JsonDocument.Parse(ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, pin));

    private static ToolManifest Manifest(JsonElement root) => root.GetProperty("definition").Deserialize(ToolJsonContext.Default.ToolManifest)
        ?? throw new InvalidDataException();
}
