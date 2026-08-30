using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle;

public sealed class LifecycleStorePlanningIntegrationTests
{
    [Fact(DisplayName = "Lifecycle store plans canonical creation without writing")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanFrameworkUpdateCreatesCanonicalEnvelopeWithoutWriting()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-create");
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        var result = store.PlanFrameworkUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Framework());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Create, change.Kind);
        Assert.False(File.Exists(temporary.Combine(LifecycleSchema.RelativePath)));
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.Serialize(
                LifecycleStoreIntegrationDocuments.Envelope(
                    temporary,
                    LifecycleStoreIntegrationDocuments.Framework(),
                    LifecycleStoreIntegrationDocuments.EmptyExtensions())),
            change.IntendedBytes.ToArray());
        var envelope = JsonSerializer.Deserialize(
            change.IntendedBytes.AsSpan(),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1);
        Assert.NotNull(envelope?.Framework);
        Assert.NotNull(envelope?.Extensions);

        using var document = JsonDocument.Parse(change.IntendedBytes.ToArray());
        Assert.Equal(
            new[] { "schemaVersion", "fingerprintPolicy", "workspacePath", "framework", "extensions" },
            document.RootElement.EnumerateObject().Select(property => property.Name));
        var extensions = document.RootElement.GetProperty("extensions");
        Assert.Equal("complete", extensions.GetProperty("coverage").GetString());
        Assert.Empty(extensions.GetProperty("packages").EnumerateArray());
        Assert.Empty(extensions.GetProperty("paths").EnumerateArray());
    }

    [Fact(DisplayName = "Lifecycle store canonicalizes a selected change and preserves Extension meaning")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanFrameworkUpdatePreservesUnrelatedMeaningAndCanonicalBytes()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-framework");
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                NonCanonicalFrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                NonCanonicalExtensionsJson(LifecycleStoreIntegrationDocuments.Extensions())));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        var result = store.PlanFrameworkUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Framework(
                LifecycleStoreIntegrationDocuments.UpdatedFrameworkPath));

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var repeated = store.PlanFrameworkUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Framework(
                LifecycleStoreIntegrationDocuments.UpdatedFrameworkPath));
        var repeatedChange = Assert.IsType<PlannedFileChange>(repeated.Change);
        var originalEnvelope = Assert.IsType<LifecycleEnvelopeV1>(JsonSerializer.Deserialize(
            originalBytes.AsSpan(),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1));
        var planned = Assert.IsType<LifecycleEnvelopeV1>(JsonSerializer.Deserialize(
            change.IntendedBytes.AsSpan(),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1));
        var originalExtensions = originalEnvelope.Extensions
            ?? throw new InvalidOperationException("The original lifecycle document requires Extensions.");
        var plannedExtensions = planned.Extensions
            ?? throw new InvalidOperationException("The planned lifecycle document requires Extensions.");
        var plannedFramework = planned.Framework?.Deserialize(
            LifecycleJsonContext.Default.FrameworkLifecycleState)
            ?? throw new InvalidOperationException("The planned lifecycle document requires Framework.");
        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.FrameworkPath,
            Assert.Single(plannedFramework.Targets).SourceAssetPath);
        Assert.True(JsonElement.DeepEquals(
            originalExtensions,
            plannedExtensions));
        Assert.Equal(change.IntendedBytes, repeatedChange.IntendedBytes);
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.Serialize(
                LifecycleStoreIntegrationDocuments.Envelope(
                    temporary,
                    LifecycleStoreIntegrationDocuments.Framework(
                        LifecycleStoreIntegrationDocuments.UpdatedFrameworkPath),
                    LifecycleStoreIntegrationDocuments.Extensions())),
            change.IntendedBytes.ToArray());
        Assert.NotEqual(originalBytes, change.IntendedBytes);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Lifecycle store returns unchanged for equal selected meaning")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanFrameworkUpdateDoesNotManufactureARewrite()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-unchanged");
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                NonCanonicalFrameworkJson(framework),
                "{\"paths\":[],\"packages\":[],\"coverage\":\"complete\"}"));
        temporary.WriteBytes(
            LifecycleSchema.RelativePath,
            originalBytes);
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        var result = store.PlanFrameworkUpdate(current, framework);

        Assert.Equal(LifecycleWritePlanState.Unchanged, result.State);
        Assert.Null(result.Change);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Lifecycle store blocks unrelated invalidity and cross-section collision")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanFrameworkUpdateBlocksInvalidUnrelatedSectionAndCollision()
    {
        using var invalidTemporary = TemporaryWorkspace.Create("lifecycle-plan-invalid-other");
        invalidTemporary.WriteText(
            LifecycleSchema.RelativePath,
            LifecycleStoreIntegrationDocuments.RawDocument(
                invalidTemporary.Path,
                FrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                "{\"coverage\":\"complete\",\"coverage\":\"incomplete\"}"));
        var invalidStore = new LifecycleStore(new PhysicalPathResolver());
        var invalidCurrent = await invalidStore.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(invalidTemporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        var invalid = invalidStore.PlanFrameworkUpdate(
            invalidCurrent,
            LifecycleStoreIntegrationDocuments.Framework(
                LifecycleStoreIntegrationDocuments.UpdatedFrameworkPath));

        using var collisionTemporary = TemporaryWorkspace.Create("lifecycle-plan-collision");
        var collidingFramework = LifecycleStoreIntegrationDocuments.Framework(
            LifecycleStoreIntegrationDocuments.ExtensionPath);
        collisionTemporary.WriteBytes(
            LifecycleSchema.RelativePath,
            LifecycleStoreIntegrationDocuments.Serialize(
                LifecycleStoreIntegrationDocuments.Envelope(
                    collisionTemporary,
                    collidingFramework,
                    LifecycleStoreIntegrationDocuments.Extensions())));
        var collisionStore = new LifecycleStore(new PhysicalPathResolver());
        var collisionCurrent = await collisionStore.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(collisionTemporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        var collision = collisionStore.PlanFrameworkUpdate(
            collisionCurrent,
            collidingFramework);

        Assert.Equal(LifecycleWritePlanState.Blocked, invalid.State);
        Assert.Equal(LifecycleWritePlanState.Blocked, collision.State);
    }

    [Theory(DisplayName = "Lifecycle Framework planning blocks existing untrusted sections without writing")]
    [InlineData("selected-section-missing")]
    [InlineData("selected-explicit-null")]
    [InlineData("selected-malformed")]
    [InlineData("selected-incomplete")]
    [InlineData("unrelated-section-missing")]
    [InlineData("unrelated-explicit-null")]
    [InlineData("unrelated-malformed")]
    [InlineData("unrelated-incomplete")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanFrameworkUpdateBlocksExistingUntrustedSections(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create($"lifecycle-plan-framework-untrusted-{scenario}");
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        var extensions = LifecycleStoreIntegrationDocuments.EmptyExtensions();
        var frameworkJson = scenario switch
        {
            "selected-section-missing" => null,
            "selected-explicit-null" => "null",
            "selected-malformed" => "{\"coverage\":\"complete\"}",
            "selected-incomplete" => FrameworkJson(framework)
                .Replace("\"coverage\":\"complete\"", "\"coverage\":\"incomplete\"", StringComparison.Ordinal),
            _ => FrameworkJson(framework),
        };
        var extensionsJson = scenario switch
        {
            "unrelated-section-missing" => null,
            "unrelated-explicit-null" => "null",
            "unrelated-malformed" => "{\"coverage\":\"complete\",\"packages\":[]}",
            "unrelated-incomplete" => ExtensionsJson(extensions)
                .Replace("\"coverage\":\"complete\"", "\"coverage\":\"incomplete\"", StringComparison.Ordinal),
            _ => ExtensionsJson(extensions),
        };
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocumentWithSectionPresence(
                temporary.Path,
                frameworkJson,
                extensionsJson,
                includeFramework: scenario != "selected-section-missing",
                includeExtensions: scenario != "unrelated-section-missing"));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);

        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        var result = store.PlanFrameworkUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Framework(
                LifecycleStoreIntegrationDocuments.UpdatedFrameworkPath));

        Assert.Equal(LifecycleWritePlanState.Blocked, result.State);
        Assert.Null(result.Change);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Lifecycle store Extension plan preserves valid Framework value")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanExtensionUpdatePreservesFrameworkValue()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-extension");
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                NonCanonicalFrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                NonCanonicalExtensionsJson(LifecycleStoreIntegrationDocuments.Extensions())));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);

        var result = store.PlanExtensionUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Extensions(".agents/updated-toolkit.md"));

        var planned = Assert.IsType<LifecycleEnvelopeV1>(JsonSerializer.Deserialize(
            Assert.IsType<PlannedFileChange>(result.Change).IntendedBytes.AsSpan(),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1));
        var originalEnvelope = Assert.IsType<LifecycleEnvelopeV1>(JsonSerializer.Deserialize(
            originalBytes.AsSpan(),
            LifecycleJsonContext.Default.LifecycleEnvelopeV1));
        var originalFramework = originalEnvelope.Framework
            ?? throw new InvalidOperationException("The original lifecycle document requires Framework.");
        var plannedFramework = planned.Framework
            ?? throw new InvalidOperationException("The planned lifecycle document requires Framework.");
        Assert.Equal(LifecycleWritePlanState.Planned, result.State);
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.Serialize(
                LifecycleStoreIntegrationDocuments.Envelope(
                    temporary,
                    LifecycleStoreIntegrationDocuments.Framework(),
                    LifecycleStoreIntegrationDocuments.Extensions(
                        ".agents/updated-toolkit.md"))),
            Assert.IsType<PlannedFileChange>(result.Change).IntendedBytes.ToArray());
        Assert.True(JsonElement.DeepEquals(
            originalFramework,
            plannedFramework));
    }

    [Theory(DisplayName = "Lifecycle Extension planning blocks existing untrusted sections without writing")]
    [InlineData("selected-section-missing")]
    [InlineData("selected-explicit-null")]
    [InlineData("selected-malformed")]
    [InlineData("selected-incomplete")]
    [InlineData("unrelated-section-missing")]
    [InlineData("unrelated-explicit-null")]
    [InlineData("unrelated-malformed")]
    [InlineData("unrelated-incomplete")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanExtensionUpdateBlocksExistingUntrustedSections(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create($"lifecycle-plan-extension-untrusted-{scenario}");
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        var extensions = LifecycleStoreIntegrationDocuments.EmptyExtensions();
        var frameworkJson = scenario switch
        {
            "unrelated-section-missing" => null,
            "unrelated-explicit-null" => "null",
            "unrelated-malformed" => "{\"coverage\":\"complete\"}",
            "unrelated-incomplete" => FrameworkJson(framework)
                .Replace("\"coverage\":\"complete\"", "\"coverage\":\"incomplete\"", StringComparison.Ordinal),
            _ => FrameworkJson(framework),
        };
        var extensionsJson = scenario switch
        {
            "selected-section-missing" => null,
            "selected-explicit-null" => "null",
            "selected-malformed" => "{\"coverage\":\"complete\"}",
            "selected-incomplete" => ExtensionsJson(extensions)
                .Replace("\"coverage\":\"complete\"", "\"coverage\":\"incomplete\"", StringComparison.Ordinal),
            _ => ExtensionsJson(extensions),
        };
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocumentWithSectionPresence(
                temporary.Path,
                frameworkJson,
                extensionsJson,
                includeFramework: scenario != "unrelated-section-missing",
                includeExtensions: scenario != "selected-section-missing"));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);

        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);
        var result = store.PlanExtensionUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Extensions(
                ".agents/updated-toolkit.md"));

        Assert.Equal(LifecycleWritePlanState.Blocked, result.State);
        Assert.Null(result.Change);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Lifecycle store blocks Extension planning when Framework omits required source provenance")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanExtensionUpdateBlocksFrameworkWithoutRequiredSourceProvenance()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-extension-invalid-framework");
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                LifecycleStoreIntegrationDocuments.FrameworkWithoutSourceProvenanceJson(),
                NonCanonicalExtensionsJson(LifecycleStoreIntegrationDocuments.Extensions())));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleStoreReadState.Available, current.State);
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.ExtensionPath,
            current.Extensions?.Paths[0].Path);

        var result = store.PlanExtensionUpdate(
            current,
            LifecycleStoreIntegrationDocuments.Extensions(".agents/updated-toolkit.md"));

        Assert.Equal(LifecycleWritePlanState.Blocked, result.State);
        Assert.Null(result.Change);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Lifecycle store Extension semantic no-op preserves original bytes without writing")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PlanExtensionUpdateDoesNotManufactureARewrite()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-plan-extension-unchanged");
        var extensions = LifecycleStoreIntegrationDocuments.Extensions();
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                NonCanonicalFrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                NonCanonicalExtensionsJson(extensions)));
        temporary.WriteBytes(LifecycleSchema.RelativePath, originalBytes);
        var store = new LifecycleStore(new PhysicalPathResolver());
        var current = await store.ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);

        var result = store.PlanExtensionUpdate(current, extensions);

        Assert.Equal(LifecycleWritePlanState.Unchanged, result.State);
        Assert.Null(result.Change);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(
                temporary.Combine(LifecycleSchema.RelativePath),
                TestContext.Current.CancellationToken));
    }

    private static string FrameworkJson(FrameworkLifecycleState framework)
        => System.Text.Encoding.UTF8.GetString(
            JsonSerializer.SerializeToUtf8Bytes(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState));

    private static string ExtensionsJson(ExtensionLifecycleState extensions)
        => System.Text.Encoding.UTF8.GetString(
            JsonSerializer.SerializeToUtf8Bytes(
                extensions,
                LifecycleJsonContext.Default.ExtensionLifecycleState));

    private static string NonCanonicalFrameworkJson(FrameworkLifecycleState framework)
        => $$"""
           {
             "targets": [
               {
                 "fingerprintKind": "{{framework.Targets[0].FingerprintKind}}",
                 "baselineFingerprint": "{{framework.Targets[0].BaselineFingerprint}}",
                 "region": null,
                 "sourceAssetPath": "{{framework.Targets[0].SourceAssetPath}}",
                 "path": "{{framework.Targets[0].Path}}"
               }
             ],
             "generatedRegions": [],
             "source": {
               "inventoryFingerprint": "{{framework.Source.InventoryFingerprint}}",
               "version": "{{framework.Source.Version}}",
               "id": "{{framework.Source.Id}}"
             },
             "coverage": "{{framework.Coverage}}"
           }
           """;

    private static string NonCanonicalExtensionsJson(ExtensionLifecycleState extensions)
        => $$"""
           {
             "paths": [
               {
                 "fingerprintKind": "{{extensions.Paths[0].FingerprintKind}}",
                 "baselineFingerprint": "{{extensions.Paths[0].BaselineFingerprint}}",
                 "owners": ["{{extensions.Paths[0].Owners[0]}}"],
                 "path": "{{extensions.Paths[0].Path}}"
               }
             ],
             "packages": [
               {
                 "paths": ["{{extensions.Packages[0].Paths[0]}}"],
                 "dependencies": [],
                 "source": "{{extensions.Packages[0].Source}}",
                 "version": "{{extensions.Packages[0].Version}}",
                 "id": "{{extensions.Packages[0].Id}}"
               }
             ],
             "coverage": "{{extensions.Coverage}}"
           }
           """;
}
