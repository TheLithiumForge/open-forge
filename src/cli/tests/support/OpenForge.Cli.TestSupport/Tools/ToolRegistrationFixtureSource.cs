using System.Text;

namespace OpenForge.Cli.TestSupport.Tools;

// Authored fixtures model installer output shapes. They contain no upstream source.
public static class ToolRegistrationFixtureSource
{
    public const string Definition = "registration-definition.json";
    public const string Mcp = "registration-mcp.json";
    public const string Hooks = "registration-settings.json";
    public const string Toml = "registration-config.toml";
    public const string Agent = "registration-CLAUDE.md";
    public const string HookDefinition = "registration-hook-definition.json";
    public const string UserHooks = "registration-hooks-user.json";
    public const string CreatedHookGroup = "registration-hooks-created.json";
    public const string SharedHookGroup = "registration-hooks-shared.json";
    public const string UnixHookCommand = "codegraph prompt-hook";
    public const string WindowsHookCommand = "codegraph.cmd prompt-hook";

    public static byte[] HookOutput(string resource, bool windows)
    {
        var bytes = Read(resource);
        if (!windows) return bytes;
        return Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(bytes).Replace("\"" + UnixHookCommand + "\"", "\"" + WindowsHookCommand + "\"", StringComparison.Ordinal));
    }

    public static byte[] Read(string name)
    {
        using var stream = typeof(ToolRegistrationFixtureSource).Assembly.GetManifestResourceStream($"OpenForge.Tests.ToolFixture/{name}")
            ?? throw new InvalidDataException("The registration fixture is missing.");
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }
}
