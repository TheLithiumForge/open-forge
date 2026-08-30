using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class LifecycleSchemaContractTests
{
    private const string Fingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact(DisplayName = "Lifecycle schema v1 serializes the exact isolated Framework graph through source generation"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void LifecycleSchemaSerializesExactFrameworkGraph()
    {
        var workspace = Workspace();
        var framework = FrameworkState();
        var frameworkValue = JsonSerializer.SerializeToElement(
            framework,
            LifecycleJsonContext.Default.FrameworkLifecycleState);
        var envelope = new LifecycleEnvelopeV1
        {
            SchemaVersion = LifecycleSchema.Version,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = workspace.LexicalRoot,
            Framework = frameworkValue,
            Extensions = null,
        };

        var json = JsonSerializer.Serialize(envelope, LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var serializedFramework = root.GetProperty("framework");
        var serializedTargets = serializedFramework.GetProperty("targets");

        Assert.Equal(LifecycleSchema.Version, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(LifecycleSchema.FingerprintPolicy, root.GetProperty("fingerprintPolicy").GetString());
        Assert.Equal(workspace.LexicalRoot, root.GetProperty("workspacePath").GetString());
        Assert.Equal("embedded-framework", serializedFramework.GetProperty("source").GetProperty("id").GetString());
        Assert.Equal(".agents/loader.md", serializedTargets[0].GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, serializedTargets[0].GetProperty("sourceAssetPath").ValueKind);
        Assert.Equal("AGENTS.md", serializedTargets[1].GetProperty("sourceAssetPath").GetString());
        Assert.False(serializedFramework.TryGetProperty("trust", out _));
        Assert.False(root.TryGetProperty("plan", out _));
        Assert.False(root.TryGetProperty("recovery", out _));
    }

    [Fact(DisplayName = "Lifecycle selected-section duplicate validation isolates the unrelated section"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void LifecycleDuplicateValidationIsolatesUnrelatedSection()
    {
        var json = Encoding.UTF8.GetBytes("""
            {
              "schemaVersion": 1,
              "fingerprintPolicy": "open-forge-markdown-v1",
              "workspacePath": "/workspace",
              "framework": { "source": { "id": "one", "id": "two" } },
              "extensions": { "coverage": "complete", "packages": [], "paths": [] }
            }
            """);

        LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(json, LifecycleSection.Extensions);
        Assert.Throws<JsonException>(() =>
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(json, LifecycleSection.Framework));
    }

    [Fact(DisplayName = "Lifecycle selected Framework schema rejects unknown members without reading Extension meaning"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FrameworkSchemaRejectsUnknownMembers()
    {
        var json = """
            {
              "coverage": "complete",
              "source": {
                "id": "embedded-framework",
                "version": null,
                "inventoryFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
              },
              "targets": [],
              "generatedRegions": [],
              "unknown": true
            }
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
            json,
            LifecycleJsonContext.Default.FrameworkLifecycleState));
    }

    [Fact(DisplayName = "Lifecycle Framework schema rejects a target that omits required nullable source provenance"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FrameworkSchemaRejectsTargetWithoutSourceProvenance()
    {
        var json = $$"""
            {
              "coverage": "complete",
              "source": {
                "id": "embedded-framework",
                "version": null,
                "inventoryFingerprint": "{{Fingerprint}}"
              },
              "targets": [
                {
                  "path": "AGENTS.md",
                  "region": null,
                  "baselineFingerprint": "{{Fingerprint}}",
                  "fingerprintKind": "semantic"
                }
              ],
              "generatedRegions": []
            }
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
            json,
            LifecycleJsonContext.Default.FrameworkLifecycleState));
    }

    [Fact(DisplayName = "Lifecycle Extension validation rejects unknown schema versions before section trust"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void LifecycleValidationRejectsUnknownVersion()
    {
        var workspace = Workspace();
        var envelope = new LifecycleEnvelopeV1
        {
            SchemaVersion = LifecycleSchema.Version + 1,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = workspace.LexicalRoot,
            Framework = null,
            Extensions = null,
        };
        var extensions = new ExtensionLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages = [],
            Paths = [],
        };

        var result = LifecycleDocumentValidator.ValidateExtensions(workspace, envelope, extensions);

        Assert.Equal(LifecycleReadState.Invalid, result.State);
        Assert.Equal(LifecycleExtensionTrust.Incomplete, result.Trust);
        Assert.Contains("unsupported", result.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Lifecycle Framework source-generated round trip retains every accepted schema field"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FrameworkSchemaRoundTripRetainsAcceptedFields()
    {
        var expected = FrameworkState();
        var json = JsonSerializer.Serialize(
            expected,
            LifecycleJsonContext.Default.FrameworkLifecycleState);

        var actual = JsonSerializer.Deserialize(
            json,
            LifecycleJsonContext.Default.FrameworkLifecycleState);

        Assert.NotNull(actual);
        Assert.Equal(LifecycleSchema.CompleteCoverage, actual.Coverage);
        Assert.Equal(expected.Source.Id, actual.Source.Id);
        Assert.Equal(expected.Source.Version, actual.Source.Version);
        Assert.Equal(expected.Source.InventoryFingerprint, actual.Source.InventoryFingerprint);
        Assert.Equal(expected.Targets[0].Path, actual.Targets[0].Path);
        Assert.Equal(expected.Targets[0].SourceAssetPath, actual.Targets[0].SourceAssetPath);
        Assert.Equal(expected.Targets[0].Region, actual.Targets[0].Region);
        Assert.Equal(expected.Targets[0].BaselineFingerprint, actual.Targets[0].BaselineFingerprint);
        Assert.Equal(expected.Targets[0].FingerprintKind, actual.Targets[0].FingerprintKind);
        Assert.Equal(expected.Targets[1].Path, actual.Targets[1].Path);
        Assert.Equal(expected.Targets[1].SourceAssetPath, actual.Targets[1].SourceAssetPath);
        Assert.Equal(expected.Targets[1].Region, actual.Targets[1].Region);
        Assert.Equal(expected.Targets[1].BaselineFingerprint, actual.Targets[1].BaselineFingerprint);
        Assert.Equal(expected.Targets[1].FingerprintKind, actual.Targets[1].FingerprintKind);
        Assert.Equal(expected.GeneratedRegions[0].Path, actual.GeneratedRegions[0].Path);
        Assert.Equal(expected.GeneratedRegions[0].Region, actual.GeneratedRegions[0].Region);
    }

    [Fact(DisplayName = "Lifecycle read factories accept truthful missing, section-missing, Framework, and Extension facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void LifecycleReadFactoriesAcceptValidFacts()
    {
        var workspace = Workspace();
        var file = FileSnapshot(workspace);
        var framework = FrameworkState();
        var extensions = ExtensionsState();

        var documentMissing = LifecycleStoreReadResult.DocumentMissing(
            workspace,
            LifecycleSection.Framework,
            FileStateSnapshot.Missing(LifecyclePath(workspace)));
        var sectionMissing = LifecycleStoreReadResult.SectionMissing(
            workspace,
            LifecycleSection.Framework,
            file,
            Envelope(workspace, framework: null, extensions: extensions));
        var frameworkAvailable = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            file,
            Envelope(workspace, framework, extensions: null),
            framework,
            extensions: null);
        var extensionAvailable = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Extensions,
            file,
            Envelope(workspace, framework: null, extensions: extensions),
            framework: null,
            extensions: extensions);
        var unavailable = LifecycleStoreReadResult.Unavailable(
            workspace,
            LifecycleSection.Framework,
            new FilesystemFailure(
                FilesystemFailureKind.InputOutput,
                "The lifecycle read failed."));
        var cancelled = LifecycleStoreReadResult.Cancelled(
            workspace,
            LifecycleSection.Framework);

        Assert.Equal(LifecycleStoreReadState.DocumentMissing, documentMissing.State);
        Assert.Equal(LifecycleStoreReadState.SectionMissing, sectionMissing.State);
        Assert.Equal(LifecycleStoreReadState.Available, frameworkAvailable.State);
        Assert.Equal(LifecycleStoreReadState.Available, extensionAvailable.State);
        Assert.Equal(LifecycleStoreReadState.Unavailable, unavailable.State);
        Assert.Equal(LifecycleStoreReadState.Cancelled, cancelled.State);
    }

    [Fact(DisplayName = "Lifecycle Framework planning creates a canonical envelope with a complete empty Extension section"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanFrameworkUpdateCreatesCanonicalEnvelopeWithEmptyExtensions()
    {
        var workspace = Workspace();
        var current = LifecycleStoreReadResult.DocumentMissing(
            workspace,
            LifecycleSection.Framework,
            FileStateSnapshot.Missing(LifecyclePath(workspace)));
        var result = new LifecycleStore(new PhysicalPathResolver()).PlanFrameworkUpdate(
            current,
            FrameworkState());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        using var document = JsonDocument.Parse(change.IntendedBytes.ToArray());
        var root = document.RootElement;

        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(
            new[] { "schemaVersion", "fingerprintPolicy", "workspacePath", "framework", "extensions" },
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("open-forge-markdown-v1", root.GetProperty("fingerprintPolicy").GetString());

        var extensions = root.GetProperty("extensions");
        Assert.Equal("complete", extensions.GetProperty("coverage").GetString());
        Assert.Empty(extensions.GetProperty("packages").EnumerateArray());
        Assert.Empty(extensions.GetProperty("paths").EnumerateArray());
    }

    [Fact(DisplayName = "Lifecycle Framework planning does not repair an existing missing Extension section"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanFrameworkUpdateBlocksExistingMissingExtensions()
    {
        var workspace = Workspace();
        var framework = FrameworkState();
        var originalBytes = Serialize(Envelope(workspace, framework, extensions: null));
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            FileSnapshot(workspace, originalBytes),
            Envelope(workspace, framework, extensions: null),
            framework,
            extensions: null);

        var result = new LifecycleStore(new PhysicalPathResolver()).PlanFrameworkUpdate(
            current,
            FrameworkStateWithVersion("2.0.0"));

        Assert.Equal(LifecycleWritePlanState.Blocked, result.State);
        Assert.Null(result.Change);
        Assert.Equal(originalBytes, current.File?.Bytes.ToArray());
    }

    [Fact(DisplayName = "Lifecycle Extension planning does not repair an existing missing Framework section"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanExtensionUpdateBlocksExistingMissingFramework()
    {
        var workspace = Workspace();
        var extensions = ExtensionsState(".agents/toolkit.md");
        var originalBytes = Serialize(Envelope(workspace, framework: null, extensions: extensions));
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Extensions,
            FileSnapshot(workspace, originalBytes),
            Envelope(workspace, framework: null, extensions: extensions),
            framework: null,
            extensions: extensions);

        var result = new LifecycleStore(new PhysicalPathResolver()).PlanExtensionUpdate(
            current,
            ExtensionsState(".agents/updated-toolkit.md"));

        Assert.Equal(LifecycleWritePlanState.Blocked, result.State);
        Assert.Null(result.Change);
        Assert.Equal(originalBytes, current.File?.Bytes.ToArray());
    }

    [Fact(DisplayName = "Lifecycle read factories reject forged path, selected presence, common mismatch, and typed/raw mismatch facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void LifecycleReadFactoriesRejectContradictoryFacts()
    {
        var workspace = Workspace();
        var file = FileSnapshot(workspace);
        var framework = FrameworkState();
        var canonicalEnvelope = Envelope(workspace, framework, extensions: null);

        var forgedFile = FileStateSnapshot.File(
            Path.Combine(workspace.LexicalRoot, "forged.json"),
            Path.Combine(workspace.PhysicalRoot, "forged.json"),
            "bytes"u8);
        Assert.Throws<ArgumentException>(() => LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            forgedFile,
            canonicalEnvelope,
            framework,
            extensions: null));

        Assert.Throws<ArgumentException>(() => LifecycleStoreReadResult.SectionMissing(
            workspace,
            LifecycleSection.Framework,
            file,
            canonicalEnvelope));

        Assert.Throws<ArgumentException>(() => LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            file,
            Envelope(workspace, framework: null, extensions: null),
            framework,
            extensions: null));

        Assert.Throws<ArgumentException>(() => LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            file,
            Envelope(
                workspace,
                FrameworkState(".agents/other-loader.md"),
                extensions: null),
            framework,
            extensions: null));

        Assert.Throws<ArgumentException>(() => LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            file,
            new LifecycleEnvelopeV1
            {
                SchemaVersion = LifecycleSchema.Version,
                FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
                WorkspacePath = Path.Combine(workspace.LexicalRoot, "different"),
                Framework = canonicalEnvelope.Framework,
                Extensions = null,
            },
            framework,
            extensions: null));
    }

    [Fact(DisplayName = "Lifecycle Framework planning emits exact canonical envelope bytes for a semantic change"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanFrameworkUpdateProducesCanonicalEnvelopeForSemanticChange()
    {
        var workspace = Workspace();
        var originalFramework = FrameworkState();
        var updatedFramework = FrameworkStateWithVersion("2.0.0");
        var extensions = ExtensionsState();
        var originalEnvelope = Envelope(workspace, originalFramework, extensions);
        var originalBytes = Serialize(originalEnvelope);
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            FileSnapshot(workspace, originalBytes),
            originalEnvelope,
            originalFramework,
            extensions: null);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var result = store.PlanFrameworkUpdate(current, updatedFramework);
        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var expectedBytes = Serialize(Envelope(workspace, updatedFramework, extensions));

        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(expectedBytes, change.IntendedBytes.ToArray());
        Assert.NotEqual(originalBytes, change.IntendedBytes.ToArray());

        var repeated = store.PlanFrameworkUpdate(current, updatedFramework);
        var repeatedChange = Assert.IsType<PlannedFileChange>(repeated.Change);
        Assert.Equal(expectedBytes, repeatedChange.IntendedBytes.ToArray());
    }

    [Fact(DisplayName = "Lifecycle Framework planning performs no write for a semantic no-op and retains original bytes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanFrameworkUpdateDoesNotWriteForSemanticNoOp()
    {
        var workspace = Workspace();
        var framework = FrameworkState();
        var extensions = ExtensionsState();
        var originalEnvelope = Envelope(workspace, framework, extensions);
        var originalBytes = Serialize(originalEnvelope);
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Framework,
            FileSnapshot(workspace, originalBytes),
            originalEnvelope,
            framework,
            extensions: null);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var result = store.PlanFrameworkUpdate(current, framework);
        var retainedBytes = current.File?.Bytes.ToArray()
            ?? throw new InvalidOperationException("The current lifecycle read requires original bytes.");

        Assert.Equal(LifecycleWritePlanState.Unchanged, result.State);
        Assert.Null(result.Change);
        Assert.Equal(originalBytes, retainedBytes);
    }

    [Fact(DisplayName = "Lifecycle Extension planning emits exact canonical envelope bytes for a semantic change"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanExtensionUpdateProducesCanonicalEnvelopeForSemanticChange()
    {
        var workspace = Workspace();
        var framework = FrameworkState();
        var originalExtensions = ExtensionsState(".agents/toolkit.md");
        var updatedExtensions = ExtensionsState(".agents/updated-toolkit.md");
        var originalEnvelope = Envelope(workspace, framework, originalExtensions);
        var originalBytes = Serialize(originalEnvelope);
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Extensions,
            FileSnapshot(workspace, originalBytes),
            originalEnvelope,
            framework: null,
            extensions: originalExtensions);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var result = store.PlanExtensionUpdate(current, updatedExtensions);
        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var expectedBytes = Serialize(Envelope(workspace, framework, updatedExtensions));

        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(expectedBytes, change.IntendedBytes.ToArray());
        Assert.NotEqual(originalBytes, change.IntendedBytes.ToArray());

        var repeated = store.PlanExtensionUpdate(current, updatedExtensions);
        var repeatedChange = Assert.IsType<PlannedFileChange>(repeated.Change);
        Assert.Equal(expectedBytes, repeatedChange.IntendedBytes.ToArray());
    }

    [Fact(DisplayName = "Lifecycle Extension planning performs no write for a semantic no-op and retains original bytes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlanExtensionUpdateDoesNotWriteForSemanticNoOp()
    {
        var workspace = Workspace();
        var framework = FrameworkState();
        var extensions = ExtensionsState(".agents/toolkit.md");
        var originalEnvelope = Envelope(workspace, framework, extensions);
        var originalBytes = Serialize(originalEnvelope);
        var current = LifecycleStoreReadResult.Available(
            workspace,
            LifecycleSection.Extensions,
            FileSnapshot(workspace, originalBytes),
            originalEnvelope,
            framework: null,
            extensions: extensions);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var result = store.PlanExtensionUpdate(current, extensions);
        var retainedBytes = current.File?.Bytes.ToArray()
            ?? throw new InvalidOperationException("The current lifecycle read requires original bytes.");

        Assert.Equal(LifecycleWritePlanState.Unchanged, result.State);
        Assert.Null(result.Change);
        Assert.Equal(originalBytes, retainedBytes);
    }

    private static FrameworkLifecycleState FrameworkState()
        => new()
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

    private static FrameworkLifecycleState FrameworkState(string path)
    {
        var framework = FrameworkState();
        return new FrameworkLifecycleState
        {
            Coverage = framework.Coverage,
            Source = framework.Source,
            Targets =
            [
                framework.Targets[0],
                new FrameworkLifecycleTarget
                {
                    Path = path,
                    SourceAssetPath = framework.Targets[1].SourceAssetPath,
                    Region = framework.Targets[1].Region,
                    BaselineFingerprint = framework.Targets[1].BaselineFingerprint,
                    FingerprintKind = framework.Targets[1].FingerprintKind,
                },
            ],
            GeneratedRegions = framework.GeneratedRegions,
        };
    }

    private static FrameworkLifecycleState FrameworkStateWithVersion(string version)
    {
        var framework = FrameworkState();
        return new FrameworkLifecycleState
        {
            Coverage = framework.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = framework.Source.Id,
                Version = version,
                InventoryFingerprint = framework.Source.InventoryFingerprint,
            },
            Targets = framework.Targets,
            GeneratedRegions = framework.GeneratedRegions,
        };
    }

    private static ExtensionLifecycleState ExtensionsState()
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages = [],
            Paths = [],
        };

    private static ExtensionLifecycleState ExtensionsState(string path)
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
                    BaselineFingerprint = Fingerprint,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
            ],
        };

    private static LifecycleEnvelopeV1 Envelope(
        CliWorkspace workspace,
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions)
        => new()
        {
            SchemaVersion = LifecycleSchema.Version,
            FingerprintPolicy = LifecycleSchema.FingerprintPolicy,
            WorkspacePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspace.LexicalRoot)),
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

    private static byte[] Serialize(LifecycleEnvelopeV1 envelope)
        => JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);

    private static FileStateSnapshot FileSnapshot(CliWorkspace workspace)
        => FileSnapshot(workspace, "bytes"u8);

    private static FileStateSnapshot FileSnapshot(
        CliWorkspace workspace,
        ReadOnlySpan<byte> bytes)
    {
        var path = LifecyclePath(workspace);
        return FileStateSnapshot.File(path, path, bytes);
    }

    private static string LifecyclePath(CliWorkspace workspace)
        => Path.Combine(workspace.LexicalRoot, LifecycleSchema.RelativePath);

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lifecycle-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
