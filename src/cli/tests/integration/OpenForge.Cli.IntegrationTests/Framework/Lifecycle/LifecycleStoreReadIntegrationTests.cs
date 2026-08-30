using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle;

public sealed class LifecycleStoreReadIntegrationTests
{
    [Fact(DisplayName = "Lifecycle store distinguishes missing document section and cancellation")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadDistinguishesMissingDocumentSectionAndCancellation()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-store-missing");
        var workspace = LifecycleStoreIntegrationDocuments.Workspace(temporary);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var missingDocument = await store.ReadAsync(
            workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        temporary.WriteBytes(
            LifecycleSchema.RelativePath,
            LifecycleStoreIntegrationDocuments.Serialize(
                LifecycleStoreIntegrationDocuments.Envelope(
                    temporary,
                    framework: null,
                    LifecycleStoreIntegrationDocuments.Extensions())));
        var missingSection = await store.ReadAsync(
            workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        var validExtension = await store.ReadAsync(
            workspace,
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await store.ReadAsync(
            workspace,
            LifecycleSection.Framework,
            cancellation.Token);

        Assert.Equal(LifecycleStoreReadState.DocumentMissing, missingDocument.State);
        Assert.Equal(LifecycleStoreReadState.SectionMissing, missingSection.State);
        Assert.Equal(LifecycleStoreReadState.Available, validExtension.State);
        Assert.Equal(
            LifecycleStoreIntegrationDocuments.ExtensionPath,
            validExtension.Extensions?.Paths[0].Path);
        Assert.Equal(LifecycleStoreReadState.Cancelled, cancelled.State);
    }

    [Fact(DisplayName = "Lifecycle store isolates malformed unrelated section meaning")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadValidFrameworkWithoutAcceptingMalformedExtensions()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-store-isolation");
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        var frameworkJson = System.Text.Encoding.UTF8.GetString(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState));
        temporary.WriteText(
            LifecycleSchema.RelativePath,
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                frameworkJson,
                "{\"coverage\":\"complete\",\"coverage\":\"incomplete\"}"));
        var workspace = LifecycleStoreIntegrationDocuments.Workspace(temporary);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var frameworkRead = await store.ReadAsync(
            workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);
        var extensionRead = await store.ReadAsync(
            workspace,
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleStoreReadState.Available, frameworkRead.State);
        Assert.Equal(LifecycleStoreIntegrationDocuments.FrameworkPath, frameworkRead.Framework?.Targets[0].Path);
        Assert.Equal(LifecycleStoreIntegrationDocuments.FrameworkPath, frameworkRead.Framework?.Targets[0].SourceAssetPath);
        Assert.Equal(LifecycleStoreReadState.Invalid, extensionRead.State);
    }

    [Fact(DisplayName = "Lifecycle store isolates missing required Framework source provenance from the selected Extension section")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadValidExtensionsWithoutAcceptingMissingFrameworkSourceProvenance()
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-store-source-provenance-isolation");
        temporary.WriteText(
            LifecycleSchema.RelativePath,
            LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                LifecycleStoreIntegrationDocuments.FrameworkWithoutSourceProvenanceJson(),
                ExtensionsJson(LifecycleStoreIntegrationDocuments.Extensions())));
        var workspace = LifecycleStoreIntegrationDocuments.Workspace(temporary);
        var store = new LifecycleStore(new PhysicalPathResolver());

        var extensionRead = await store.ReadAsync(
            workspace,
            LifecycleSection.Extensions,
            TestContext.Current.CancellationToken);
        var frameworkRead = await store.ReadAsync(
            workspace,
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleStoreReadState.Available, extensionRead.State);
        Assert.Equal(LifecycleStoreIntegrationDocuments.ExtensionPath, extensionRead.Extensions?.Paths[0].Path);
        Assert.Equal(LifecycleStoreReadState.Invalid, frameworkRead.State);
    }

    [Theory(DisplayName = "Lifecycle store rejects invalid selected Framework facts")]
    [InlineData("unknown-schema")]
    [InlineData("workspace-mismatch")]
    [InlineData("unordered-targets")]
    [InlineData("duplicate-selected")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadRejectsInvalidSelectedFramework(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("lifecycle-store-invalid");
        var document = scenario switch
        {
            "unknown-schema" => LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                FrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                "null").Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2", StringComparison.Ordinal),
            "workspace-mismatch" => LifecycleStoreIntegrationDocuments.RawDocument(
                Path.Combine(temporary.Path, "different"),
                FrameworkJson(LifecycleStoreIntegrationDocuments.Framework()),
                "null"),
            "unordered-targets" => LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                FrameworkJson(UnorderedFramework()),
                "null"),
            "duplicate-selected" => LifecycleStoreIntegrationDocuments.RawDocument(
                temporary.Path,
                "{\"coverage\":\"complete\",\"coverage\":\"complete\"}",
                "null"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The lifecycle scenario is not defined."),
        };
        temporary.WriteText(LifecycleSchema.RelativePath, document);

        var result = await new LifecycleStore(new PhysicalPathResolver()).ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        Assert.Contains(result.State, new[] { LifecycleStoreReadState.Invalid, LifecycleStoreReadState.Blocked });
    }

    [Fact(DisplayName = "Lifecycle store blocks a physically escaping document")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadBlocksExternalLifecycleAlias()
    {
        using var outside = TemporaryWorkspace.Create("lifecycle-store-outside");
        outside.CreateFile("open-forge.lifecycle.json", "{}");
        using var temporary = TemporaryWorkspace.Create("lifecycle-store-alias");
        temporary.CreateDirectorySymbolicLink(".agents", outside.Path);

        var result = await new LifecycleStore(new PhysicalPathResolver()).ReadAsync(
            LifecycleStoreIntegrationDocuments.Workspace(temporary),
            LifecycleSection.Framework,
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleStoreReadState.Blocked, result.State);
    }

    private static string FrameworkJson(FrameworkLifecycleState framework)
        => System.Text.Encoding.UTF8.GetString(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                framework,
                LifecycleJsonContext.Default.FrameworkLifecycleState));

    private static FrameworkLifecycleState UnorderedFramework()
    {
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        return new FrameworkLifecycleState
        {
            Coverage = framework.Coverage,
            Source = framework.Source,
            Targets =
            [
                new FrameworkLifecycleTarget
                {
                    Path = "z.md",
                    SourceAssetPath = LifecycleStoreIntegrationDocuments.FrameworkPath,
                    Region = null,
                    BaselineFingerprint = LifecycleStoreIntegrationDocuments.FingerprintA,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
                framework.Targets[0],
            ],
            GeneratedRegions = [],
        };
    }

    private static string ExtensionsJson(ExtensionLifecycleState extensions)
        => System.Text.Encoding.UTF8.GetString(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                extensions,
                LifecycleJsonContext.Default.ExtensionLifecycleState));

}
