using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle;

internal static class LifecycleStoreIntegrationDocuments
{
    internal const string FrameworkPath = ".agents/loader.md";
    internal const string UpdatedFrameworkPath = ".agents/updated-loader.md";
    internal const string ExtensionPath = ".agents/toolkit.md";
    internal const string FingerprintA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    internal const string FingerprintB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    internal static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static FrameworkLifecycleState Framework(string path = FrameworkPath)
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = "open-forge",
                Version = "1.0.0",
                InventoryFingerprint = FingerprintA,
            },
            Targets =
            [
                new FrameworkLifecycleTarget
                {
                    Path = path,
                    SourceAssetPath = FrameworkPath,
                    Region = null,
                    BaselineFingerprint = FingerprintA,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
            ],
            GeneratedRegions = [],
        };

    internal static ExtensionLifecycleState Extensions(string path = ExtensionPath)
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages =
            [
                new LifecycleExtensionPackageV1
                {
                    Id = "toolkit",
                    Version = "1.0.0",
                    Source = "embedded:toolkit",
                    Dependencies = [],
                    Paths = [path],
                },
            ],
            Paths =
            [
                new LifecycleExtensionPathV1
                {
                    Path = path,
                    Owners = ["toolkit"],
                    BaselineFingerprint = FingerprintB,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
            ],
        };

    internal static LifecycleEnvelopeV1 Envelope(
        TemporaryWorkspace temporary,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions)
        => new()
        {
            SchemaVersion = LifecycleSchema.Version,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(temporary.Path)),
            Framework = framework is null
                ? null
                : JsonSerializer.SerializeToElement(
                    framework,
                    LifecycleJsonContext.Default.FrameworkLifecycleState),
            Extensions = extensions is null
                ? null
                : JsonSerializer.SerializeToElement(
                    extensions,
                    LifecycleJsonContext.Default.ExtensionLifecycleState),
        };

    internal static byte[] Serialize(LifecycleEnvelopeV1 envelope)
        => JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);

    internal static string RawDocument(
        string workspacePath,
        string framework,
        string extensions)
        => $$"""
           {
             "schemaVersion": {{LifecycleSchema.Version}},
             "fingerprintPolicy": "{{LifecycleSchema.FingerprintPolicy}}",
             "workspacePath": "{{JsonEncodedText.Encode(Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspacePath)))}}",
             "framework": {{framework}},
             "extensions": {{extensions}}
           }
           """;

    internal static string FrameworkWithoutSourceProvenanceJson()
        => $$"""
           {
             "coverage": "{{LifecycleSchema.CompleteCoverage}}",
             "source": {
               "id": "open-forge",
               "version": "1.0.0",
               "inventoryFingerprint": "{{FingerprintA}}"
             },
             "targets": [
               {
                 "path": "{{FrameworkPath}}",
                 "region": null,
                 "baselineFingerprint": "{{FingerprintA}}",
                 "fingerprintKind": "{{LifecycleSchema.SemanticFingerprintKind}}"
               }
             ],
             "generatedRegions": []
           }
           """;
}
