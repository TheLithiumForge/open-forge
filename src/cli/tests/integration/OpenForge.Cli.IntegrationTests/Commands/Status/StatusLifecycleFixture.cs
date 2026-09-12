using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusLifecycleFixture
{
    internal sealed record ExtensionSeed(
        string Id,
        string? Version,
        string? Source,
        IReadOnlyList<string> Dependencies,
        IReadOnlyList<string> Paths);

    internal sealed record PathSeed(
        string Path,
        IReadOnlyList<string> Owners,
        string BaselineFingerprint);

    internal static void Write(
        StatusIntegrationWorkspace workspace,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions)
        => WriteSections(
            workspace,
            framework is null
                ? null
                : JsonSerializer.SerializeToElement(
                    framework,
                    LifecycleJsonContext.Default.FrameworkLifecycleState),
            extensions is null
                ? null
                : JsonSerializer.SerializeToElement(
                    extensions,
                    LifecycleJsonContext.Default.ExtensionLifecycleState));

    internal static void WriteSections(
        StatusIntegrationWorkspace workspace,
        JsonElement? framework,
        JsonElement? extensions)
    {
        var envelope = new LifecycleEnvelopeV1
        {
            SchemaVersion = LifecycleSchema.Version,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = Normalize(workspace.Path),
            Framework = framework,
            Extensions = extensions,
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        var path = workspace.Combine(StatusIntegrationWorkspace.LifecyclePath);
        if (File.Exists(path))
        {
            File.WriteAllBytes(path, bytes);
            return;
        }

        workspace.WriteBytes(StatusIntegrationWorkspace.LifecyclePath, bytes);
    }

    internal static LifecycleEnvelopeV1 Read(StatusIntegrationWorkspace workspace)
        => JsonSerializer.Deserialize(
            File.ReadAllBytes(workspace.Combine(StatusIntegrationWorkspace.LifecyclePath)),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1)
            ?? throw new InvalidOperationException("The Status lifecycle fixture envelope cannot be null.");

    internal static JsonElement ExtensionSection(ExtensionLifecycleState extensions)
        => JsonSerializer.SerializeToElement(
            extensions,
            LifecycleJsonContext.Default.ExtensionLifecycleState);

    internal static JsonElement MalformedSection()
    {
        using var document = JsonDocument.Parse("""
            { "coverage": "complete", "unexpected": true }
            """);
        return document.RootElement.Clone();
    }

    internal static ExtensionLifecycleState Extensions(
        IEnumerable<ExtensionSeed> packages,
        IEnumerable<PathSeed> paths)
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages = packages
                .Select(package => new LifecycleExtensionPackageV1
                {
                    Id = package.Id,
                    Version = package.Version,
                    Source = package.Source,
                    Dependencies = package.Dependencies.ToArray(),
                    Paths = package.Paths.ToArray(),
                })
                .ToArray(),
            Paths = paths
                .Select(path => new LifecycleExtensionPathV1
                {
                    Path = path.Path,
                    Owners = path.Owners.ToArray(),
                    BaselineFingerprint = path.BaselineFingerprint,
                    FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
                })
                .ToArray(),
        };

    internal static string Hash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Normalize(string path)
        => System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(path));
}
