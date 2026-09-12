using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

public sealed class ExtensionSourceAndLifecycleIntegrationTests
{
    [Fact(DisplayName = "Real Extension catalogue resolves exact manifests dependencies and stable ordering"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealCatalogueResolvesManifestsDependenciesAndOrdering()
    {
        using var workspace = TemporaryWorkspace.Create("extension-target");
        using var source = TemporaryWorkspace.Create("extension-source");
        WriteManifest(source, "z-toolkit", "z-toolkit", ["a-base"]);
        WriteManifest(source, "a-base", "a-base", []);
        var beforeWorkspace = workspace.SnapshotHashes();
        var beforeSource = source.SnapshotHashes();
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var result = await reader.ReadAsync(Workspace(workspace), source.Path, CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        Assert.Equal(ExtensionSourceKind.Catalogue, result.Kind);
        Assert.Equal(["a-base", "z-toolkit"], result.Packages.Select(package => package.Id));
        Assert.Equal(["a-base"], result.Packages[1].Dependencies);
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.Equal(beforeSource, source.SnapshotHashes());
    }

    [Fact(DisplayName = "Real Extension source rejects duplicate IDs incomplete closure and workspace overlap"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealSourceRejectsUnsafeAndIncompleteUniverses()
    {
        using var workspace = TemporaryWorkspace.Create("extension-target");
        using var duplicate = TemporaryWorkspace.Create("extension-duplicate");
        WriteManifest(duplicate, "first", "same-id", []);
        WriteManifest(duplicate, "second", "same-id", []);
        using var incomplete = TemporaryWorkspace.Create("extension-incomplete");
        WriteManifest(incomplete, "toolkit", "toolkit", ["missing"]);
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var duplicateResult = await reader.ReadAsync(Workspace(workspace), duplicate.Path, CancellationToken.None);
        var incompleteResult = await reader.ReadAsync(Workspace(workspace), incomplete.Path, CancellationToken.None);
        var overlapResult = await reader.ReadAsync(Workspace(workspace), workspace.CreateDirectory("catalogue"), CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Blocked, duplicateResult.State);
        Assert.Equal(ExtensionSourceReadState.Invalid, incompleteResult.State);
        Assert.Equal(ExtensionSourceReadState.Blocked, overlapResult.State);
    }

    [Fact(DisplayName = "Real Extension source blocks a physical alias of the selected workspace"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public async Task RealSourceBlocksPhysicalWorkspaceAlias()
    {
        using var workspace = TemporaryWorkspace.Create("extension-alias-target");
        using var aliasParent = TemporaryWorkspace.Create("extension-alias-parent");
        var alias = aliasParent.CreateDirectorySymbolicLink("source-alias", workspace.Path);
        var reader = new ExtensionSourceReader(new PhysicalPathResolver());

        var result = await reader.ReadAsync(Workspace(workspace), alias, CancellationToken.None);

        Assert.Equal(ExtensionSourceReadState.Blocked, result.State);
    }

    [Theory(DisplayName = "Real Extension source overlap takes precedence over missing and file classification"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    [InlineData("missing-child")]
    [InlineData("file-child")]
    public async Task RealSourceBlocksLexicalWorkspaceChildrenBeforeClassification(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("extension-overlap-precedence");
        var source = scenario switch
        {
            "missing-child" => workspace.Combine("missing-catalogue"),
            "file-child" => workspace.CreateFile("catalogue.json"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The overlap scenario is not defined."),
        };

        var result = await new ExtensionSourceReader(new PhysicalPathResolver()).ReadAsync(
            Workspace(workspace),
            source,
            TestContext.Current.CancellationToken);

        Assert.Equal(ExtensionSourceReadState.Blocked, result.State);
    }

    [Fact(DisplayName = "Lifecycle v1 reader accepts complete reciprocal Extension ownership and empty absence"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    public async Task LifecycleReaderAcceptsTrustedAndAbsentSections()
    {
        using var trustedWorkspace = TemporaryWorkspace.Create("lifecycle-trusted");
        trustedWorkspace.WriteText(
            LifecycleSchema.RelativePath,
            Lifecycle(
                trustedWorkspace.Path,
                """
                [{
                  "id": "toolkit",
                  "version": "1.0.0",
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": [".agents/toolkit.md"]
                }]
                """,
                """
                [{
                  "path": ".agents/toolkit.md",
                  "owners": ["toolkit"],
                  "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "fingerprintKind": "semantic"
                }]
                """));
        using var absentWorkspace = TemporaryWorkspace.Create("lifecycle-absent");
        absentWorkspace.WriteText(LifecycleSchema.RelativePath, Lifecycle(absentWorkspace.Path, "[]", "[]"));
        var reader = LifecycleReader();

        var trusted = await reader.ReadExtensionsAsync(Workspace(trustedWorkspace), CancellationToken.None);
        var absent = await reader.ReadExtensionsAsync(Workspace(absentWorkspace), CancellationToken.None);

        Assert.Equal(LifecycleReadState.Complete, trusted.State);
        Assert.Equal(LifecycleExtensionTrust.Trusted, trusted.Trust);
        Assert.Equal("toolkit", Assert.Single(trusted.Packages).Id);
        Assert.Equal(LifecycleReadState.Complete, absent.State);
        Assert.Equal(LifecycleExtensionTrust.Absent, absent.Trust);
        Assert.Empty(absent.Packages);
    }

    [Fact(DisplayName = "Lifecycle v1 reader skips duplicate properties inside the opaque framework subtree"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    public async Task LifecycleReaderSkipsOpaqueFrameworkDuplicateProperties()
    {
        using var workspace = TemporaryWorkspace.Create("lifecycle-opaque-framework");
        var document = Lifecycle(
            workspace.Path,
            """
            [{
              "id": "toolkit",
              "version": "1.0.0",
              "source": "embedded catalogue",
              "dependencies": [],
              "paths": []
            }]
            """,
            "[]").Replace(
                "\"framework\": null",
                "\"framework\": { \"settings\": { \"enabled\": true, \"enabled\": false }, \"settings\": null }",
                StringComparison.Ordinal);
        workspace.WriteText(LifecycleSchema.RelativePath, document);

        var result = await LifecycleReader().ReadExtensionsAsync(
            Workspace(workspace),
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleReadState.Complete, result.State);
        Assert.Equal(LifecycleExtensionTrust.Trusted, result.Trust);
        Assert.Equal("toolkit", Assert.Single(result.Packages).Id);
    }

    [Fact(DisplayName = "Lifecycle v1 reader retains structurally safe rows from incomplete Extension coverage"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    public async Task LifecycleReaderRetainsSafeUntrustedRows()
    {
        using var workspace = TemporaryWorkspace.Create("lifecycle-untrusted");
        var document = Lifecycle(
            workspace.Path,
            """
            [{
              "id": "toolkit",
              "version": "1.0.0",
              "source": "embedded catalogue",
              "dependencies": [],
              "paths": [".agents/toolkit.md"]
            }]
            """,
            """
            [{
              "path": ".agents/toolkit.md",
              "owners": ["toolkit"],
              "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
              "fingerprintKind": "semantic"
            }]
            """).Replace("\"coverage\": \"complete\"", "\"coverage\": \"incomplete\"", StringComparison.Ordinal);
        workspace.WriteText(LifecycleSchema.RelativePath, document);

        var result = await LifecycleReader().ReadExtensionsAsync(Workspace(workspace), CancellationToken.None);

        Assert.Equal(LifecycleReadState.Invalid, result.State);
        Assert.Equal(LifecycleExtensionTrust.Untrusted, result.Trust);
        Assert.Equal("toolkit", Assert.Single(result.Packages).Id);
    }

    [Theory(DisplayName = "Lifecycle v1 reader rejects duplicate unknown mismatched and nonreciprocal facts"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    [InlineData("duplicate-property")]
    [InlineData("duplicate-extension-property")]
    [InlineData("duplicate-package-property")]
    [InlineData("duplicate-path-property")]
    [InlineData("unknown-property")]
    [InlineData("workspace-mismatch")]
    [InlineData("workspace-alias")]
    [InlineData("path-case-alias")]
    [InlineData("self-dependency")]
    [InlineData("dependency-cycle")]
    [InlineData("nonreciprocal-owner")]
    public async Task LifecycleReaderRejectsStrictInvalidFacts(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("lifecycle-invalid");
        var document = scenario switch
        {
            "duplicate-property" => Lifecycle(workspace.Path, "[]", "[]").Replace(
                "\"schemaVersion\": 1,",
                "\"schemaVersion\": 1, \"schemaVersion\": 1,",
                StringComparison.Ordinal),
            "duplicate-extension-property" => Lifecycle(workspace.Path, "[]", "[]").Replace(
                "\"coverage\": \"complete\",",
                "\"coverage\": \"complete\", \"coverage\": \"complete\",",
                StringComparison.Ordinal),
            "duplicate-package-property" => Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "toolkit",
                  "id": "toolkit",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": []
                }]
                """,
                "[]"),
            "duplicate-path-property" => Lifecycle(
                workspace.Path,
                "[]",
                """
                [{
                  "path": ".agents/toolkit.md",
                  "path": ".agents/toolkit.md",
                  "owners": [],
                  "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "fingerprintKind": "semantic"
                }]
                """),
            "unknown-property" => Lifecycle(workspace.Path, "[]", "[]").Replace(
                "\"schemaVersion\": 1,",
                "\"schemaVersion\": 1, \"unknown\": true,",
                StringComparison.Ordinal),
            "workspace-mismatch" => Lifecycle(workspace.Combine("other"), "[]", "[]"),
            "workspace-alias" => Lifecycle(workspace.Path, "[]", "[]").Replace(
                JsonString(Path.GetFullPath(workspace.Path)),
                JsonString($"{workspace.Path}{Path.DirectorySeparatorChar}."),
                StringComparison.Ordinal),
            "path-case-alias" => Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "toolkit",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": [".agents/Toolkit.md"]
                }]
                """,
                """
                [{
                  "path": ".agents/toolkit.md",
                  "owners": ["toolkit"],
                  "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "fingerprintKind": "semantic"
                }]
                """),
            "self-dependency" => Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "toolkit",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": ["toolkit"],
                  "paths": []
                }]
                """,
                "[]"),
            "dependency-cycle" => Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "base",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": ["toolkit"],
                  "paths": []
                }, {
                  "id": "toolkit",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": ["base"],
                  "paths": []
                }]
                """,
                "[]"),
            "nonreciprocal-owner" => Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "toolkit",
                  "version": null,
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": []
                }]
                """,
                """
                [{
                  "path": ".agents/toolkit.md",
                  "owners": ["toolkit"],
                  "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "fingerprintKind": "semantic"
                }]
                """),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The lifecycle invalid scenario is not defined."),
        };
        workspace.WriteText(LifecycleSchema.RelativePath, document);

        var result = await LifecycleReader().ReadExtensionsAsync(Workspace(workspace), CancellationToken.None);

        Assert.Equal(LifecycleReadState.Invalid, result.State);
        Assert.Contains(result.Trust, new[] { LifecycleExtensionTrust.Incomplete, LifecycleExtensionTrust.Blocked });
    }

    [Fact(DisplayName = "Lifecycle reader never treats the legacy Extension receipt as replacement lifecycle input"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    public async Task LegacyReceiptRemainsOrdinaryContent()
    {
        using var workspace = TemporaryWorkspace.Create("lifecycle-legacy");
        workspace.WriteText("open-forge.extensions.json", "{ \"schema\": 2 }");

        var result = await LifecycleReader().ReadExtensionsAsync(Workspace(workspace), CancellationToken.None);

        Assert.Equal(LifecycleReadState.Missing, result.State);
        Assert.Equal(LifecycleExtensionTrust.Incomplete, result.Trust);
    }

    [Theory(DisplayName = "Lifecycle reader blocks linked ancestor and file escapes before reading"), Trait("Feature", "extension-lifecycle-read"), Trait("Evidence", "Integration")]
    [InlineData("linked-ancestor")]
    [InlineData("linked-file")]
    public async Task LifecycleReaderBlocksPhysicalEscapes(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("lifecycle-physical-workspace");
        using var outside = TemporaryWorkspace.Create("lifecycle-physical-outside");
        outside.WriteText("open-forge.lifecycle.json", Lifecycle(workspace.Path, "[]", "[]"));
        if (scenario == "linked-ancestor")
        {
            _ = workspace.CreateDirectorySymbolicLink(".agents", outside.Path);
        }
        else
        {
            _ = workspace.CreateDirectory(".agents");
            _ = workspace.CreateFileSymbolicLink(
                LifecycleSchema.RelativePath,
                outside.Combine("open-forge.lifecycle.json"));
        }

        var result = await LifecycleReader().ReadExtensionsAsync(
            Workspace(workspace),
            TestContext.Current.CancellationToken);

        Assert.Equal(LifecycleReadState.Invalid, result.State);
        Assert.Equal(LifecycleExtensionTrust.Blocked, result.Trust);
    }

    internal static string Lifecycle(string workspacePath, string packages, string paths)
        => $$"""
            {
              "schemaVersion": 1,
              "fingerprintPolicy": "open-forge-markdown-v1",
              "workspacePath": {{JsonString(Path.GetFullPath(workspacePath))}},
              "framework": null,
              "extensions": {
                "coverage": "complete",
                "packages": {{packages}},
                "paths": {{paths}}
              }
            }
            """;

    private static CliWorkspace Workspace(TemporaryWorkspace workspace)
        => new(
            lexicalRoot: workspace.Path,
            physicalRoot: workspace.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static LifecycleDocumentReader LifecycleReader()
        => new(new PhysicalPathResolver());

    private static void WriteManifest(
        TemporaryWorkspace source,
        string directory,
        string id,
        string[] dependencies)
    {
        source.WriteText(
            $"{directory}/extension.json",
            $$"""
            {
              "id": {{JsonString(id)}},
              "name": {{JsonString(id)}},
              "description": {{JsonString($"Package {id}.")}},
              "version": "1.0.0",
              "dependencies": [{{string.Join(", ", dependencies.Select(JsonString))}}]
            }
            """);
    }

    private static string JsonString(string value)
        => $"\"{JsonEncodedText.Encode(value)}\"";
}
