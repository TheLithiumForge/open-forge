using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

internal static class ToolHookCase
{
    internal static ToolRegistration Definition() => JsonSerializer.Deserialize(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.HookDefinition),
        ToolJsonContext.Default.ToolRegistration) ?? throw new InvalidDataException();
    internal static ToolHookCommandRewrite Hook => Assert.IsType<ToolHookCommandRewrite>(Assert.Single(Definition().Rewrites));

    internal static ToolEntryRewriteRequest Request(string target, bool decline = false) => new()
    {
        WorkspaceRoot = Path.GetFullPath("fixture-workspace"),
        Rewrites = Definition().Rewrites,
        DeclinedComponents = decline ? Definition().Components : [],
        Values = new Dictionary<string, string>(StringComparer.Ordinal)
        { [ToolVocabulary.OpenForgeVariable] = ToolEntryDeclarations.PathCommand, [ToolVocabulary.TargetVariable] = target },
        BeforeInstallerFiles = new Dictionary<string, byte[]?> { [Hook.File] = ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.UserHooks) },
    };

    internal static byte[] Output(string resource, string target)
        => ToolRegistrationFixtureSource.HookOutput(resource, target.StartsWith("win-", StringComparison.Ordinal));

    internal static Dictionary<string, byte[]?> Files(byte[] bytes) => new(StringComparer.Ordinal) { [Hook.File] = bytes };

    internal static ToolEntryRewriteRequest Removal(ToolRewriteReceipt receipt, string target)
    {
        var hook = Assert.IsType<ToolHookCommandRewrite>(receipt.Rewrite);
        var seed = Definition().Components[0];
        var removal = seed.DeclinedEntries[0];
        var expected = JsonNode.Parse(removal.Expected.GetRawText())?.AsObject() ?? throw new InvalidDataException();
        expected[ToolJsonEntries.Command] = hook.Replacement;
        using var json = JsonDocument.Parse(expected.ToJsonString());
        return Request(target) with
        {
            Rewrites = [],
            BeforeInstallerFiles = null,
            CreatedHookGroups = receipt.CreatedHookGroup is { } group ? [group] : [],
            DeclinedComponents = [seed with { DeclinedEntries = [removal with { Expected = json.RootElement.Clone(), PlatformOverrides = null }] }],
        };
    }
}
