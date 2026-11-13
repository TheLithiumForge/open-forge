using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-contracts"), Trait("Evidence", "Unit")]
public sealed class ToolRegistrationContractTests
{
    private const string Entry = "fixture-entry";
    private const string Host = ToolVocabulary.ClaudeCodeHost;
    private const string Component = "fixture-hook";

    [Theory(DisplayName = "Both declared origins round trip through the strict manifest model")]
    [InlineData(ToolOrigin.FirstParty)]
    [InlineData(ToolOrigin.ThirdParty)]
    public void OriginRoundTrips(string origin)
    {
        var json = ToolFixtureSource.Manifest();
        json[Key(nameof(ToolManifest.Origin))] = origin;
        var definition = new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(json.ToJsonString()), ToolFixtureSource.ManifestName, false);
        Assert.Equal(origin, definition.Manifest.Origin);
        Assert.False(definition.Reviewed);
    }

    [Theory(DisplayName = "Missing, unknown and non-string origins fail before admission")]
    [InlineData("missing")]
    [InlineData("unknown")]
    [InlineData("numeric")]
    public void RejectsInvalidOrigin(string kind)
    {
        var json = ToolFixtureSource.Manifest();
        var key = Key(nameof(ToolManifest.Origin));
        if (kind == "missing") json.Remove(key);
        else json[key] = kind == "numeric" ? JsonValue.Create(0) : JsonValue.Create(kind);
        Assert.Throws<JsonException>(() => new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(json.ToJsonString()), ToolFixtureSource.ManifestName, false));
    }

    [Fact(DisplayName = "Registration source generation keeps all four rewrite selectors and exact argument values")]
    public void RegistrationRoundTrips()
    {
        var definition = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest
            with
        { Registration = Registration() };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(definition, ToolJsonContext.Default.ToolManifest);
        var decoded = JsonSerializer.Deserialize(bytes, ToolJsonContext.Default.ToolManifest);
        var registration = Assert.IsType<ToolRegistration>(decoded?.Registration);
        Assert.IsType<ToolJsonCommandRewrite>(registration.Rewrites[0]);
        Assert.IsType<ToolTomlCommandRewrite>(registration.Rewrites[1]);
        Assert.IsType<ToolHookCommandRewrite>(registration.Rewrites[2]);
        Assert.IsType<ToolMarkedLineRewrite>(registration.Rewrites[3]);
        var command = Assert.IsType<ToolJsonCommandRewrite>(registration.Rewrites[0]);
        Assert.Equal(new[] { "--", "space and \"quote\"", "", "--" }, command.Replacement.Args);
        Assert.Equal(Component, Assert.Single(registration.Components).Id);
        Assert.True(registration.Components[0].DefaultEnabled);
        Assert.Equal(Entry, registration.Components[0].DeclinedEntries[0].Expected.GetProperty(Entry).GetString());
        Assert.Equal(ToolVocabulary.CodeNavigationRole, Assert.Single(registration.RoleGuidance).Role);
        Assert.Equal(ToolFixtureSource.ManifestName, command.File);
    }

    [Fact(DisplayName = "Rewrite discriminators can follow normal properties in manifests and locks")]
    public void MetadataOrderDoesNotChangeContract()
    {
        var definition = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest
            with
        { Registration = Registration() };
        var json = Assert.IsType<JsonObject>(JsonNode.Parse(JsonSerializer.SerializeToUtf8Bytes(definition, ToolJsonContext.Default.ToolManifest)));
        var registration = Assert.IsType<JsonObject>(json[Key(nameof(definition.Registration))]);
        var rewrites = Assert.IsType<JsonArray>(registration[Key(nameof(ToolRegistration.Rewrites))]);
        var rewrite = Assert.IsType<JsonObject>(rewrites[0]);
        MoveKindLast(rewrite);
        var decoded = JsonSerializer.Deserialize(json.ToJsonString(), ToolJsonContext.Default.ToolManifest);
        Assert.IsType<ToolJsonCommandRewrite>(decoded?.Registration?.Rewrites[0]);

        var receipt = new ToolRewriteReceipt { Id = Entry, Rewrite = Registration().Rewrites[0], Declined = false };
        var lockJson = Assert.IsType<JsonObject>(JsonNode.Parse(JsonSerializer.SerializeToUtf8Bytes(receipt, WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt)));
        MoveKindLast(Assert.IsType<JsonObject>(lockJson[Key(nameof(receipt.Rewrite))]));
        var decodedReceipt = JsonSerializer.Deserialize(lockJson.ToJsonString(), WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt);
        Assert.IsType<ToolJsonCommandRewrite>(decodedReceipt?.Rewrite);
    }

    [Fact(DisplayName = "Declared launchers, telemetry and removal order survive manifest decoding")]
    public void LifecycleDeclarationRoundTrips()
    {
        var definition = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest;
        var targets = definition.Targets.ToDictionary(pair => pair.Key,
            pair => pair.Value with { Launcher = new(Entry, [ToolFixtureSource.ArtifactName]) }, StringComparer.Ordinal);
        var command = new ToolLifecycleCommand
        {
            Args = [Entry],
            Scope = ToolVocabulary.WorkspaceScope,
            WorkspaceOutputs = [],
            TimeoutSeconds = 1,
        };
        definition = definition with
        {
            Targets = targets,
            Lifecycle = new()
            {
                Install = command,
                Uninstall = command,
                IndexCleanup = command,
                IgnorePaths = [],
                RemovalOrder = [ToolVocabulary.UninstallAction, ToolVocabulary.IndexCleanupAction],
                TelemetryOn = command,
                TelemetryOff = command,
            },
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(definition, ToolJsonContext.Default.ToolManifest);
        var decoded = Assert.IsType<ToolManifest>(JsonSerializer.Deserialize(bytes, ToolJsonContext.Default.ToolManifest));
        Assert.Equal(definition.Lifecycle.RemovalOrder, decoded.Lifecycle?.RemovalOrder);
        Assert.Equal(command.Args, decoded.Lifecycle?.TelemetryOn?.Args);
        Assert.Equal(command.Args, decoded.Lifecycle?.TelemetryOff?.Args);
        Assert.All(decoded.Targets.Values, target => Assert.Equal(new[] { ToolFixtureSource.ArtifactName }, target.Launcher?.Args));
    }

    [Fact(DisplayName = "Passive MCP entry names belong to the tool and need no Open Forge prefix")]
    public void PassiveHostSelectorsAreDeclared()
    {
        var definition = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest;
        var mcp = definition.Mcp with
        {
            Hosts = new(StringComparer.Ordinal)
            {
                [ToolVocabulary.ClaudeCodeHost] = new() { Path = ToolHostTemplate.ClaudeFile, Selector = ToolHostTemplate.ClaudeSelectorPrefix + Entry },
                [ToolVocabulary.CodexHost] = new() { Path = ToolHostTemplate.CodexFile, Selector = ToolHostTemplate.CodexSelectorPrefix + Entry },
            },
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(definition with { Mcp = mcp }, ToolJsonContext.Default.ToolManifest);
        Assert.False(new ToolCatalogueReader().Definition(bytes, ToolFixtureSource.ManifestName, false).Reviewed);
    }

    [Fact(DisplayName = "Unknown rewrite kinds and unknown rewrite fields fail closed in the generated reader")]
    public void RejectsUnknownRewriteShape()
    {
        var definition = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest
            with
        { Registration = Registration() };
        var json = Assert.IsType<JsonObject>(JsonNode.Parse(JsonSerializer.SerializeToUtf8Bytes(definition, ToolJsonContext.Default.ToolManifest)));
        var registration = Assert.IsType<JsonObject>(json[Key(nameof(definition.Registration))]);
        var rewrites = Assert.IsType<JsonArray>(registration[Key(nameof(ToolRegistration.Rewrites))]);
        var rewrite = Assert.IsType<JsonObject>(rewrites[0]);
        rewrite["unexpected"] = true;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json.ToJsonString(), ToolJsonContext.Default.ToolManifest));
        rewrite.Remove("unexpected");
        var kind = rewrite["kind"]?.DeepClone();
        rewrite["kind"] = "unexpected";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json.ToJsonString(), ToolJsonContext.Default.ToolManifest));
        rewrite["kind"] = kind;
        var expected = Assert.IsType<JsonObject>(rewrite[Key(nameof(ToolJsonCommandRewrite.Expected))]);
        expected.Remove(Key(nameof(ToolCommandValue.Args)));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json.ToJsonString(), ToolJsonContext.Default.ToolManifest));
    }

    [Fact(DisplayName = "Saved component choices keep both yes and no without implying role activation")]
    public void ComponentChoicesRoundTrip()
    {
        Dictionary<string, WorkspaceToolSetting> settings = new(StringComparer.Ordinal)
        {
            [ToolFixtureSource.Id] = new()
            {
                Version = ToolFixtureSource.Version,
                Roles = [],
                Components = new(StringComparer.Ordinal) { [Component] = false, [Entry] = true },
            },
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(settings, WorkspaceToolsJsonContext.Default.ToolSettings);
        var decoded = JsonSerializer.Deserialize(bytes, WorkspaceToolsJsonContext.Default.ToolSettings);
        var item = Assert.IsType<WorkspaceToolSetting>(decoded?[ToolFixtureSource.Id]);
        Assert.Empty(item.Roles);
        Assert.False(item.Components?[Component]);
        Assert.True(item.Components?[Entry]);
    }

    [Fact(DisplayName = "Account-local launch records keep pinned executable, fixed arguments and immutable file stamps")]
    public void LaunchRoundTrips()
    {
        var launch = new ToolLaunchBinding
        {
            ToolId = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            ManifestSha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ManifestBytes()),
            StoreId = Entry,
            Executable = Entry,
            PrefixArguments = [ToolFixtureSource.ArtifactName],
            Environment = new(StringComparer.Ordinal) { [Entry] = "" },
            Files = [new(Entry, ToolFixtureSource.ArtifactName, ToolFixtureSource.ArtifactBytes().LongLength, 1)],
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(launch, ToolStoreJsonContext.Default.ToolLaunchBinding);
        var decoded = Assert.IsType<ToolLaunchBinding>(JsonSerializer.Deserialize(bytes, ToolStoreJsonContext.Default.ToolLaunchBinding));
        Assert.Equal(launch.ManifestSha256, decoded.ManifestSha256);
        Assert.Equal(launch.PrefixArguments, decoded.PrefixArguments);
        Assert.Equal(launch.Files, decoded.Files);
        Assert.Equal(launch.Environment, decoded.Environment);
    }

    [Fact(DisplayName = "Lock source generation keeps rewrite expectations, declined choices and required guidance")]
    public void ReceiptRoundTrips()
    {
        var manifest = ToolFixtureSource.ManifestBytes();
        var hash = ToolCatalogueReader.Hash(manifest);
        var receipt = new ToolReceipt
        {
            Id = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            Target = ToolVocabulary.Targets.Order(StringComparer.Ordinal).First(),
            ManifestSha256 = hash,
            ManifestBytes = manifest,
            ManifestFormat = "json",
            AdapterRevision = 1,
            CodeRoot = ".",
            Hosts = [Host],
            Bindings = [],
            Files = [],
            RuntimeDirectories = [],
            NativeProjects = [],
            Components = new(StringComparer.Ordinal) { [Component] = false },
            Rewrites = [new() { Id = Entry, Rewrite = Registration().Rewrites[0], Declined = false }],
            Guidance = [new(ToolVocabulary.CodeNavigationRole, Entry, hash, Entry)],
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(receipt, WorkspaceOwnershipJsonContext.Default.ToolReceipt);
        var decoded = Assert.IsType<ToolReceipt>(JsonSerializer.Deserialize(bytes, WorkspaceOwnershipJsonContext.Default.ToolReceipt));
        Assert.IsType<ToolJsonCommandRewrite>(Assert.Single(Assert.IsType<ToolRewriteReceipt[]>(decoded.Rewrites)).Rewrite);
        Assert.False(decoded.Components?[Component]);
        Assert.Equal(receipt.Guidance, decoded.Guidance);
    }

    [Fact(DisplayName = "A second synthetic provider has a distinct reviewed-tuple identity for future Choose switching")]
    public void SecondProviderIsTestOnly()
    {
        var first = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false);
        var secondJson = ToolFixtureSource.Manifest();
        secondJson[Key(nameof(first.Manifest.Id))] = ToolFixtureSource.Id + "-second";
        var second = new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(secondJson.ToJsonString()), ToolFixtureSource.ManifestName, false);
        Assert.NotEqual(first.Manifest.Id, second.Manifest.Id);
        Assert.NotEqual(first.Sha256, second.Sha256);
        Assert.Equal(first.Manifest.Capabilities, second.Manifest.Capabilities);
        Assert.False(first.Reviewed);
        Assert.False(second.Reviewed);
    }

    private static ToolRegistration Registration() => new()
    {
        Rewrites = [
            new ToolJsonCommandRewrite
            {
                Id = Entry + "-json", Host = Host, File = ToolFixtureSource.ManifestName, Pointer = "/" + Entry,
                Expected = new(Entry, []), Replacement = new(Entry, ["--", "space and \"quote\"", "", "--"]),
            },
            new ToolTomlCommandRewrite
            {
                Id = Entry + "-toml", Host = ToolVocabulary.CodexHost, File = ToolFixtureSource.ManifestName, Table = Entry,
                Expected = new(Entry, []), Replacement = new(Entry, []),
            },
            new ToolHookCommandRewrite
            {
                Id = Entry + "-hook", Host = Host, File = ToolFixtureSource.ManifestName, Component = Component,
                Pointer = "/" + Entry, Expected = Entry, Replacement = Entry,
            },
            new ToolMarkedLineRewrite
            {
                Id = Entry + "-line", Host = Host, File = ToolFixtureSource.ManifestName,
                StartMarker = "<" + Entry + ">", EndMarker = "</" + Entry + ">", Expected = Entry, Replacement = Entry,
            },
        ],
        Components = [new()
        {
            Id = Component, Flag = ToolComponentFlags.PromptHook, Description = Entry, DefaultEnabled = true,
            DeclinedEntries = [new()
            {
                Host = Host, File = ToolFixtureSource.ManifestName, Pointer = "/" + Entry,
                Expected = JsonSerializer.SerializeToElement(new Dictionary<string, string> { [Entry] = Entry }, ToolJsonContext.Default.DictionaryStringString),
            }],
        }],
        RoleGuidance = [new(ToolVocabulary.CodeNavigationRole, Entry)],
        Activate = null,
        Deactivate = null,
    };

    private static string Key(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);

    private static void MoveKindLast(JsonObject rewrite)
    {
        var kind = rewrite["kind"]?.DeepClone();
        rewrite.Remove("kind");
        rewrite["kind"] = kind;
    }
}
