using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Distribution;

public sealed class EmbeddedFrameworkPayloadReaderIntegrationTests
{
    private const string ResourcePrefix = "OpenForge.Framework.Payload/";
    private const string SourceContentDirectory = "FrameworkPayloadSource";

    [Fact(DisplayName = "Embedded Framework direct reader uses complete stable Core assembly resources"), Trait("Feature", "framework-payload"), Trait("Evidence", "Integration")]
    public void DirectReaderUsesCompleteStableCoreAssemblyResources()
    {
        var first = EmbeddedFrameworkPayloadReader.Read();
        var second = EmbeddedFrameworkPayloadReader.Read();
        var payload = AssertAvailable(first);
        var repeated = AssertAvailable(second);
        var assembly = typeof(EmbeddedFrameworkPayloadReader).Assembly;
        var resourcePaths = ReadResourcePaths(assembly);

        Assert.Equal(typeof(EmbeddedFrameworkPayloadReader).Assembly, assembly);
        Assert.NotEmpty(resourcePaths);
        Assert.Equal(resourcePaths, payload.Assets.Select(asset => asset.Path));
        Assert.Equal(payload.InventoryFingerprint, repeated.InventoryFingerprint);
        Assert.Equal(payload.Assets.Select(asset => asset.Path), repeated.Assets.Select(asset => asset.Path));
        Assert.NotNull(payload.Find("AGENTS.md"));
        Assert.NotNull(payload.Find("CLAUDE.md"));
        Assert.NotNull(payload.Find(".agents/loader.md"));
        Assert.All(payload.Assets, AssertCanonicalAsset);
        Assert.All(payload.Assets, asset => Assert.Equal(Hash(asset.Bytes.AsSpan()), asset.Sha256));
        Assert.Equal(InventoryFingerprintOracle(payload.Assets), payload.InventoryFingerprint);
    }

    [Fact(DisplayName = "Embedded Framework payload exactly matches authoritative source paths bytes and hashes"), Trait("Feature", "framework-payload"), Trait("Evidence", "Integration")]
    public void EmbeddedPayloadExactlyMatchesAuthoritativeSource()
    {
        var payload = AssertAvailable(EmbeddedFrameworkPayloadReader.Read());
        var sourceRoot = Path.Combine(AppContext.BaseDirectory, SourceContentDirectory);
        var expected = Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Select(path => ReadSourceAsset(sourceRoot, path))
            .OrderBy(asset => asset.Path, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(expected);
        Assert.Equal(expected.Select(asset => asset.Path), payload.Assets.Select(asset => asset.Path));
        foreach (var expectedAsset in expected)
        {
            var actual = Assert.IsType<FrameworkPayloadAsset>(payload.Find(expectedAsset.Path));
            Assert.Equal(expectedAsset.Bytes, actual.Bytes);
            Assert.Equal(expectedAsset.Bytes.Length, actual.ByteLength);
            Assert.Equal(expectedAsset.Sha256, actual.Sha256);
        }

        Assert.Equal(InventoryFingerprintOracle(expected), payload.InventoryFingerprint);
    }

    private static FrameworkPayload AssertAvailable(FrameworkPayloadReadResult result)
    {
        Assert.Equal(FrameworkPayloadReadState.Available, result.State);
        Assert.Null(result.Cause);
        return Assert.IsType<FrameworkPayload>(result.Payload);
    }

    private static string[] ReadResourcePaths(Assembly assembly)
        => assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Select(name => name[ResourcePrefix.Length..].Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static SourceAsset ReadSourceAsset(string sourceRoot, string path)
    {
        var canonicalPath = Path.GetRelativePath(sourceRoot, path).Replace('\\', '/');
        var bytes = File.ReadAllBytes(path);
        return new SourceAsset(canonicalPath, bytes, Hash(bytes));
    }

    private static void AssertCanonicalAsset(FrameworkPayloadAsset asset)
    {
        Assert.False(string.IsNullOrEmpty(asset.Path));
        Assert.DoesNotContain('\\', asset.Path);
        Assert.DoesNotContain("//", asset.Path, StringComparison.Ordinal);
        Assert.DoesNotContain(asset.Path.Split('/'), segment => segment is "" or "." or "..");
        Assert.DoesNotContain(asset.Path, char.IsControl);
        Assert.True(
            asset.Path is "AGENTS.md" or "CLAUDE.md"
            || asset.Path.StartsWith(".agents/", StringComparison.Ordinal));
        Assert.Equal(asset.Bytes.Length, asset.ByteLength);
        Assert.Matches("^[0-9a-f]{64}$", asset.Sha256);
    }

    private static string InventoryFingerprintOracle(IEnumerable<FrameworkPayloadAsset> assets)
        => InventoryFingerprintOracle(assets.Select(asset => new SourceAsset(
            asset.Path,
            asset.Bytes.ToArray(),
            asset.Sha256)));

    private static string InventoryFingerprintOracle(IEnumerable<SourceAsset> assets)
    {
        var builder = new StringBuilder();
        foreach (var asset in assets.OrderBy(asset => asset.Path, StringComparer.Ordinal))
        {
            builder.Append(CultureInfo.InvariantCulture, $"{asset.Path}\t{asset.Bytes.Length}\t{asset.Sha256}\n");
        }

        return Hash(Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string Hash(ReadOnlySpan<byte> bytes)
        => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private sealed record SourceAsset(string Path, byte[] Bytes, string Sha256);
}
