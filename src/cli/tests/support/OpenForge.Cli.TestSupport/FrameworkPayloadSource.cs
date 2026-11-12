using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.TestSupport;

public static class FrameworkPayloadSource
{
    private const string ResourcePrefix = "OpenForge.Tests.FrameworkPayload/";

    public static IReadOnlyList<string> InstalledPaths { get; } = ReadInstalledPaths();

    public static IReadOnlyList<string> CliSkillPaths { get; } = Array.AsReadOnly(InstalledPaths
        .Where(path => path.StartsWith($"{PublishedTokens.Layout.CliSkillDirectory}/", StringComparison.Ordinal))
        .ToArray());

    public static IReadOnlyList<string> CliSkillResourcePaths { get; } = Array.AsReadOnly(CliSkillPaths
        .Where(path => path != $"{PublishedTokens.Layout.CliSkillDirectory}/{PublishedTokens.Layout.NativeSkillFileName}")
        .ToArray());

    private static IReadOnlyList<string> ReadInstalledPaths()
    {
        var paths = typeof(FrameworkPayloadSource).Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Select(name => name[ResourcePrefix.Length..].Replace('\\', '/'))
            .Where(path => path is not PublishedTokens.Layout.AgentHost and not PublishedTokens.Layout.ClaudeHost)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (paths.Length == 0 || paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw new InvalidDataException("The source Framework fixture requires a nonempty set of distinct installed paths.");
        }

        return Array.AsReadOnly(paths);
    }
}
