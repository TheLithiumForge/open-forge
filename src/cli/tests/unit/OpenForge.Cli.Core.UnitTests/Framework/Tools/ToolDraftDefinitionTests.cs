using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-draft"), Trait("Evidence", "Unit")]
public sealed class ToolDraftDefinitionTests
{
    [Fact(DisplayName = "CodeGraph pins all six verified assets against published local checksums")]
    public void AssetsMatchPublishedChecksums()
    {
        using var document = Read();
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
            Assert.Equal(manifest.Source.Repository + "/.github/workflows/release.yml@refs/heads/main", proof.GetProperty("signerIdentity").GetString());
        }
    }

    [Fact(DisplayName = "Typed CodeGraph commands remain outside release and native admission")]
    public void TypedDefinitionStaysUnadmitted()
    {
        using var document = Read();
        var manifest = Manifest(document.RootElement);
        Assert.Equal(ToolOrigin.ThirdParty, manifest.Origin);
        Assert.Equal(ToolDraftSource.Blocked, manifest.Admission);
        Assert.Empty(manifest.Qualification.Targets);
        Assert.NotNull(manifest.Lifecycle);
        Assert.NotNull(manifest.Registration);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, ToolJsonContext.Default.ToolManifest);
        var decoded = new ToolCatalogueReader().Definition(bytes, ToolDraftSource.DefinitionExtension, false);
        Assert.False(decoded.Reviewed);
        Assert.NotNull(decoded.Blocker);
        Assert.Throws<JsonException>(() => new ToolCatalogueReader().Definition(
            ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, ToolDraftSource.CodeGraph), ToolDraftSource.DefinitionExtension, true));
    }

    [Fact(DisplayName = "CodeGraph requires packaged guidance and its native MCP names")]
    public void GuidanceAndNamesAreBound()
    {
        using var document = Read();
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
        var registration = Assert.IsType<ToolRegistration>(manifest.Registration);
        var json = Assert.Single(registration.Rewrites.OfType<ToolJsonCommandRewrite>());
        var toml = Assert.Single(registration.Rewrites.OfType<ToolTomlCommandRewrite>());
        Assert.Equal(manifest.Mcp.Hosts[ToolVocabulary.ClaudeCodeHost].Selector, json.Pointer);
        Assert.Equal(manifest.Mcp.Hosts[ToolVocabulary.CodexHost].Selector, toml.Table);
        Assert.Equal(ToolVocabulary.CodeNavigationRole, Assert.Single(registration.RoleGuidance).Role);
        Assert.Empty(manifest.Configuration.Files);
        Assert.Empty(manifest.Configuration.Actions);
    }

    [Fact(DisplayName = "CodeGraph registration round trips all four shared rewrite forms")]
    public void RegistrationUsesSharedTypes()
    {
        using var document = Read();
        var manifest = Manifest(document.RootElement);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, ToolJsonContext.Default.ToolManifest);
        var decoded = Assert.IsType<ToolManifest>(JsonSerializer.Deserialize(bytes, ToolJsonContext.Default.ToolManifest));
        var registration = Assert.IsType<ToolRegistration>(decoded.Registration);
        Assert.Equal(5, registration.Rewrites.Length);
        Assert.Single(registration.Rewrites.OfType<ToolJsonCommandRewrite>());
        Assert.Single(registration.Rewrites.OfType<ToolTomlCommandRewrite>());
        var hook = Assert.Single(registration.Rewrites.OfType<ToolHookCommandRewrite>());
        Assert.Equal(2, registration.Rewrites.OfType<ToolMarkedLineRewrite>().Count());
        var component = Assert.Single(registration.Components);
        Assert.Equal(ToolComponentFlags.PromptHook, component.Flag);
        Assert.True(component.DefaultEnabled);
        Assert.Equal(component.Id, hook.Component);
        var removal = Assert.Single(component.DeclinedEntries);
        Assert.Equal(hook.Expected, removal.Expected.GetProperty("command").GetString());
        Assert.Equal(hook.File, removal.File);
        Assert.NotNull(registration.Activate);
        Assert.NotNull(registration.Deactivate);
    }

    [Fact(DisplayName = "CodeGraph lifecycle commands and rewrite originals match its pinned source fixture")]
    public void MatchesInstallerOriginals()
    {
        using var draft = Read();
        using var fixture = JsonDocument.Parse(ToolFixtureSource.CodeGraphInstallerBytes());
        var original = fixture.RootElement;
        var manifest = Manifest(draft.RootElement);
        var lifecycle = Assert.IsType<ToolLifecycle>(manifest.Lifecycle);
        var commands = new Dictionary<string, string[]>
        {
            ["install"] = lifecycle.Install.Args,
            ["uninstall"] = lifecycle.Uninstall.Args,
            ["indexCleanup"] = lifecycle.IndexCleanup.Args,
            ["telemetryOff"] = Assert.IsType<ToolLifecycleCommand>(lifecycle.TelemetryOff).Args,
            ["telemetryOn"] = Assert.IsType<ToolLifecycleCommand>(lifecycle.TelemetryOn).Args,
        };
        foreach (var (action, arguments) in commands)
            Assert.Equal(original.GetProperty(action).EnumerateArray().Select(value => value.GetString()), arguments);
        Assert.Equal(new[] { ToolVocabulary.UninstallAction, ToolVocabulary.IndexCleanupAction }, lifecycle.RemovalOrder);
        var registration = Assert.IsType<ToolRegistration>(manifest.Registration);
        var json = Assert.Single(registration.Rewrites.OfType<ToolJsonCommandRewrite>());
        Assert.Equal(original.GetProperty("claude").GetProperty("command").GetString(), json.Expected.Command);
        Assert.Equal(original.GetProperty("claude").GetProperty("args").EnumerateArray().Select(value => value.GetString()), json.Expected.Args);
        var runPrefix = original.GetProperty("runPrefix").EnumerateArray().Select(value => value.GetString()).ToArray();
        foreach (var rewrite in registration.Rewrites.OfType<ToolJsonCommandRewrite>())
        {
            Assert.Equal("${" + ToolVocabulary.OpenForgeVariable + "}", rewrite.Replacement.Command);
            Assert.Equal(runPrefix.Concat(rewrite.Expected.Args), rewrite.Replacement.Args);
        }
        foreach (var rewrite in registration.Rewrites.OfType<ToolTomlCommandRewrite>())
        {
            Assert.Equal(json.Expected.Command, rewrite.Expected.Command);
            Assert.Equal(json.Expected.Args, rewrite.Expected.Args);
            Assert.Equal(json.Replacement.Command, rewrite.Replacement.Command);
            Assert.Equal(json.Replacement.Args, rewrite.Replacement.Args);
        }
        foreach (var marked in registration.Rewrites.OfType<ToolMarkedLineRewrite>())
            Assert.Equal(original.GetProperty("markedLine").GetString(), marked.Expected);
        var hook = Assert.Single(registration.Rewrites.OfType<ToolHookCommandRewrite>());
        Assert.Contains(hook.Expected, original.GetProperty("hook").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(lifecycle.Install.Args, registration.Activate?.Args);
        Assert.Equal(lifecycle.Uninstall.Args, registration.Deactivate?.Args);
    }

    [Fact(DisplayName = "Every target runs the verified bundled Node and fixed script with the archive launcher's arguments")]
    public void UsesBundledLauncher()
    {
        using var draft = Read();
        using var fixture = JsonDocument.Parse(ToolFixtureSource.CodeGraphInstallerBytes());
        var expected = fixture.RootElement.GetProperty("launcher");
        foreach (var (rid, target) in Manifest(draft.RootElement).Targets)
        {
            var launcher = Assert.IsType<ToolLauncher>(target.Launcher);
            var platform = rid.StartsWith("win-", StringComparison.Ordinal) ? "windows" : "unix";
            Assert.Equal(expected.GetProperty(platform).GetString(), launcher.Executable);
            Assert.Equal(expected.GetProperty("entry").GetString(), target.Entry);
            Assert.Equal(expected.GetProperty("args").EnumerateArray().Select(value => value.GetString()), launcher.Args);
        }
    }

    private static JsonDocument Read() => JsonDocument.Parse(ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, ToolDraftSource.CodeGraph));

    private static ToolManifest Manifest(JsonElement root) => root.GetProperty("definition").Deserialize(ToolJsonContext.Default.ToolManifest)
        ?? throw new InvalidDataException();
}
