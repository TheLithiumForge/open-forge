using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

internal static class ToolEntryCase
{
    internal static ToolRegistration Definition() => JsonSerializer.Deserialize(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.Definition),
        ToolJsonContext.Default.ToolRegistration) ?? throw new InvalidDataException();

    internal static ToolEntryRewriteRequest Request(params ToolEntryRewrite[] rewrites) => new()
    {
        WorkspaceRoot = Path.GetFullPath("fixture-workspace"),
        Rewrites = rewrites,
        DeclinedComponents = [],
        Values = new Dictionary<string, string>(StringComparer.Ordinal) { [ToolVocabulary.OpenForgeVariable] = ToolEntryDeclarations.PathCommand },
    };

    internal static byte[] Original(ToolEntryRewrite rewrite) => ToolRegistrationFixtureSource.Read(rewrite switch
    {
        ToolJsonCommandRewrite => ToolRegistrationFixtureSource.Mcp,
        ToolTomlCommandRewrite => ToolRegistrationFixtureSource.Toml,
        ToolHookCommandRewrite => ToolRegistrationFixtureSource.Hooks,
        ToolMarkedLineRewrite => ToolRegistrationFixtureSource.Agent,
        _ => throw new ArgumentOutOfRangeException(nameof(rewrite)),
    });

    internal static Dictionary<string, byte[]?> Files(ToolEntryRewrite rewrite, byte[]? bytes) => new(StringComparer.Ordinal) { [rewrite.File] = bytes };
    internal static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
    internal static string Text(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
