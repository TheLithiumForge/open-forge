using System.Text.Json.Nodes;

namespace OpenForge.Cli.TestSupport.Tools;

public static class ToolFixtureSource
{
    public const string Id = "fixture-tool";
    public const string Version = "1.0.0";
    public const string ManifestName = "manifest.json";
    public const string ArtifactName = "artifact.bin";

    public static byte[] ManifestBytes() => Read(ManifestName);
    public static byte[] ArtifactBytes() => Read(ArtifactName);
    public static JsonObject Manifest() => JsonNode.Parse(ManifestBytes())?.AsObject() ?? throw new InvalidDataException("The fixture manifest is missing.");

    private static byte[] Read(string name)
    {
        using var source = typeof(ToolFixtureSource).Assembly.GetManifestResourceStream($"OpenForge.Tests.ToolFixture/{name}")
            ?? throw new InvalidDataException("The Tool fixture resource is missing.");
        using var output = new MemoryStream();
        source.CopyTo(output);
        return output.ToArray();
    }
}
