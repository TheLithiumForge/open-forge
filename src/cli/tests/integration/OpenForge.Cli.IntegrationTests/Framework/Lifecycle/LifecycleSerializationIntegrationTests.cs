using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle;

public sealed class LifecycleSerializationIntegrationTests
{
    private const string Fingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact(DisplayName = "Source-generated lifecycle schema constructs and round trips both isolated sections in memory"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public void SourceGeneratedLifecycleSchemaRoundTripsBothSections()
    {
        var workspacePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lifecycle-serialization"));
        var framework = new FrameworkLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = "embedded-framework",
                Version = "1.0.0",
                InventoryFingerprint = Fingerprint,
            },
            Targets =
            [
                new FrameworkLifecycleTarget
                {
                    Path = ".agents/loader.md",
                    SourceAssetPath = null,
                    Region = "entries",
                    BaselineFingerprint = Fingerprint,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
                new FrameworkLifecycleTarget
                {
                    Path = "AGENTS.md",
                    SourceAssetPath = "AGENTS.md",
                    Region = null,
                    BaselineFingerprint = Fingerprint,
                    FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
                },
            ],
            GeneratedRegions =
            [
                new FrameworkGeneratedRegion
                {
                    Path = ".agents/loader.md",
                    Region = "entries",
                },
            ],
        };
        var extensions = new ExtensionLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages =
            [
                new LifecycleExtensionPackageV1
                {
                    Id = "toolkit",
                    Version = "1.0.0",
                    Source = "embedded catalogue",
                    Dependencies = [],
                    Paths = [".agents/toolkit.md"],
                },
            ],
            Paths =
            [
                new LifecycleExtensionPathV1
                {
                    Path = ".agents/toolkit.md",
                    Owners = ["toolkit"],
                    BaselineFingerprint = Fingerprint,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
            ],
        };
        var envelope = new LifecycleEnvelopeV1
        {
            SchemaVersion = LifecycleSchema.Version,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = workspacePath,
            Framework = JsonSerializer.SerializeToElement(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState),
            Extensions = JsonSerializer.SerializeToElement(
                extensions,
                LifecycleJsonContext.Default.ExtensionLifecycleState),
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        var actualEnvelope = JsonSerializer.Deserialize(
            bytes,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        Assert.NotNull(actualEnvelope);
        var actualFramework = actualEnvelope.Framework?.Deserialize(
            LifecycleJsonContext.Default.FrameworkLifecycleState);
        var actualExtensions = actualEnvelope.Extensions?.Deserialize(
            LifecycleJsonContext.Default.ExtensionLifecycleState);

        Assert.NotNull(actualFramework);
        Assert.NotNull(actualExtensions);
        Assert.Equal("embedded-framework", actualFramework.Source.Id);
        Assert.Collection(
            actualFramework.Targets,
            generatedTarget =>
            {
                Assert.Equal(".agents/loader.md", generatedTarget.Path);
                Assert.Equal("entries", generatedTarget.Region);
                Assert.Null(generatedTarget.SourceAssetPath);
            },
            sourceBackedTarget =>
            {
                Assert.Equal("AGENTS.md", sourceBackedTarget.Path);
                Assert.Null(sourceBackedTarget.Region);
                Assert.Equal("AGENTS.md", sourceBackedTarget.SourceAssetPath);
            });
        Assert.Equal("toolkit", Assert.Single(actualExtensions.Packages).Id);
        Assert.Equal(".agents/toolkit.md", Assert.Single(actualExtensions.Paths).Path);
    }
}
