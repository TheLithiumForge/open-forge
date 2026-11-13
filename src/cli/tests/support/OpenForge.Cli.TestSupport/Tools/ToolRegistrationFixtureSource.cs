namespace OpenForge.Cli.TestSupport.Tools;

// Authored fixtures model installer output shapes. They contain no upstream source.
public static class ToolRegistrationFixtureSource
{
    public const string Definition = "registration-definition.json";
    public const string Mcp = "registration-mcp.json";
    public const string Hooks = "registration-settings.json";
    public const string Toml = "registration-config.toml";
    public const string Agent = "registration-CLAUDE.md";

    public static byte[] Read(string name)
    {
        using var stream = typeof(ToolRegistrationFixtureSource).Assembly.GetManifestResourceStream($"OpenForge.Tests.ToolFixture/{name}")
            ?? throw new InvalidDataException("The registration fixture is missing.");
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }
}
