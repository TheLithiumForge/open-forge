using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class LifecycleUtf8AdmissionCompatibilityTests
{
    [Theory(DisplayName = "Lifecycle readers preserve full UTF-8 and JSON rejection causes"), InlineData("isolated"), InlineData("truncated"), InlineData("surrogate")]
    [InlineData("continuation"), InlineData("long-prefix"), InlineData("duplicate-property"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void InvalidDocumentsPreserveExposedCausesAndSnapshot(string scenario)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var bytes = InvalidDocument(scenario);
            var snapshot = Snapshot(bytes);
            var file = Assert.IsType<FileStateSnapshot>(snapshot.File);
            Assert.Equal(LifecycleDocumentSnapshotState.Available, snapshot.State);
            Assert.Equal(FileExpectationKind.File, file.Kind);
            Assert.True(file.HasBytes);
            Assert.Equal(bytes, file.Bytes.ToArray());
            var store = new LifecycleStore(new PhysicalPathResolver());
            var framework = store.Read(snapshot, LifecycleSection.Framework);
            var extensions = store.Read(snapshot, LifecycleSection.Extensions);
            var document = new LifecycleDocumentReader(new PhysicalPathResolver()).ReadExtensions(snapshot);

            Assert.Equal(LifecycleSection.Framework, framework.SelectedSection);
            Assert.Equal(LifecycleSection.Extensions, extensions.SelectedSection);
            foreach (var result in new[] { framework, extensions })
            {
                Assert.Equal(LifecycleStoreReadState.Invalid, result.State);
                Assert.Same(snapshot.Workspace, result.Workspace);
                Assert.Same(file, result.File);
                Assert.Null(result.Envelope);
                Assert.Null(result.Framework);
                Assert.Null(result.Extensions);
                Assert.Null(result.Failure);
            }

            Assert.Equal(LifecycleReadState.Invalid, document.State);
            Assert.Equal(LifecycleExtensionTrust.Incomplete, document.Trust);
            Assert.Equal(LifecycleCoverageState.Incomplete, document.Coverage);
            Assert.Equal(LifecycleWorkspaceBinding.NotChecked, document.WorkspaceBinding);
            Assert.Empty(document.Packages);
            Assert.Empty(document.Paths);
            Assert.Null(document.FingerprintPolicy);
            Assert.Same(file, snapshot.File);
            Assert.Null(snapshot.Failure);
            Assert.Null(snapshot.FailureStage);
            Assert.Null(snapshot.Cause);
            Assert.Equal(bytes, file.Bytes.ToArray());
            var expectedCause = ExpectedCause(scenario);
            var expectedStoreCause = scenario == "duplicate-property"
                ? "The lifecycle document is invalid: The JSON property 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                    "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                    "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
                : expectedCause;
            Assert.Equal(expectedStoreCause, framework.Cause);
            Assert.Equal(expectedStoreCause, extensions.Cause);
            Assert.Equal(expectedCause, document.Cause);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact(DisplayName = "Valid non-ASCII lifecycle bytes retain selected and trusted state"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void NonAsciiDocumentRetainsSelectedSectionsAndTrustedExtensions()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lifecycle-utf8-compatibility"));
            var json = $$"""
                {
                  "schemaVersion": 1,
                  "fingerprintPolicy": "{{LifecycleSchema.FingerprintPolicy}}",
                  "workspacePath": "{{JsonEncodedText.Encode(root)}}",
                  "framework": {
                    "coverage": "complete",
                    "source": { "id": "open-forge", "version": "1.0.0-é", "inventoryFingerprint": "{{new string('a', 64)}}" },
                    "targets": [{
                      "path": ".agents/loader.md", "sourceAssetPath": ".agents/loader.md", "region": null,
                      "baselineFingerprint": "{{new string('a', 64)}}", "fingerprintKind": "semantic"
                    }],
                    "generatedRegions": []
                  },
                  "extensions": {
                    "coverage": "complete",
                    "packages": [{ "id": "toolkit", "version": "1.0.0-é", "source": "embedded:toolkit", "dependencies": [], "paths": [] }],
                    "paths": []
                  }
                }
                """;
            var bytes = Encoding.UTF8.GetBytes(json);
            Assert.Contains((byte)0xc3, bytes);
            Assert.Contains((byte)0xa9, bytes);
            var snapshot = Snapshot(bytes);
            var store = new LifecycleStore(new PhysicalPathResolver());
            var framework = store.Read(snapshot, LifecycleSection.Framework);
            var extensions = store.Read(snapshot, LifecycleSection.Extensions);
            var document = new LifecycleDocumentReader(new PhysicalPathResolver()).ReadExtensions(snapshot);

            Assert.Equal(LifecycleSection.Framework, framework.SelectedSection);
            Assert.Equal(LifecycleSection.Extensions, extensions.SelectedSection);
            foreach (var result in new[] { framework, extensions })
            {
                Assert.Equal(LifecycleStoreReadState.Available, result.State);
                Assert.Same(snapshot.Workspace, result.Workspace);
                Assert.Same(snapshot.File, result.File);
                Assert.NotNull(result.Envelope);
                Assert.Null(result.Failure);
                Assert.Null(result.Cause);
            }

            Assert.NotNull(framework.Framework);
            Assert.Equal("1.0.0-é", framework.Framework.Source.Version);
            Assert.Null(framework.Extensions);
            Assert.NotNull(extensions.Extensions);
            Assert.Equal("1.0.0-é", Assert.Single(extensions.Extensions.Packages).Version);
            Assert.Null(extensions.Framework);
            Assert.Equal(LifecycleReadState.Complete, document.State);
            Assert.Equal(LifecycleExtensionTrust.Trusted, document.Trust);
            Assert.Equal(LifecycleCoverageState.Complete, document.Coverage);
            Assert.Equal(LifecycleWorkspaceBinding.Matched, document.WorkspaceBinding);
            Assert.Equal("1.0.0-é", Assert.Single(document.Packages).Version);
            Assert.Empty(document.Paths);
            Assert.Equal(LifecycleSchema.FingerprintPolicy, document.FingerprintPolicy);
            Assert.Null(document.Cause);
            Assert.Equal(bytes, Assert.IsType<FileStateSnapshot>(snapshot.File).Bytes.ToArray());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static LifecycleDocumentSnapshot Snapshot(byte[] bytes)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lifecycle-utf8-compatibility"));
        var workspace = new CliWorkspace(lexicalRoot: root, physicalRoot: root, selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var lifecyclePath = Path.Combine(root, LifecycleSchema.RelativePath);
        var file = FileStateSnapshot.File(logicalPath: lifecyclePath, physicalPath: lifecyclePath, bytes: bytes);
        return LifecycleDocumentSnapshot.Available(workspace, file);
    }

    private static string ExpectedCause(string scenario)
        => scenario switch
        {
            "isolated" => "The lifecycle document is invalid: Unable to translate bytes [FF] at index 0 from specified code page to Unicode.",
            "truncated" => "The lifecycle document is invalid: Unable to translate bytes [E2][82] at index 0 from specified code page to Unicode.",
            "surrogate" => "The lifecycle document is invalid: Unable to translate bytes [ED] at index 0 from specified code page to Unicode.",
            "continuation" => "The lifecycle document is invalid: Unable to translate bytes [E2] at index 3 from specified code page to Unicode.",
            "long-prefix" => "The lifecycle document is invalid: Unable to translate bytes [FF] at index 128 from specified code page to Unicode.",
            "duplicate-property" => "The lifecycle document is invalid: The JSON property 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxx' is duplicated.",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The UTF-8 admission scenario is not defined."),
        };

    private static byte[] InvalidDocument(string scenario)
        => scenario switch
        {
            "isolated" => [0xff],
            "truncated" => [0xe2, 0x82],
            "surrogate" => [0xed, 0xa0, 0x80],
            "continuation" => [0x22, 0xc3, 0xa9, 0xe2, 0x28, 0xa1],
            "long-prefix" => [.. Encoding.UTF8.GetBytes(new string(' ', 128)), 0xff],
            "duplicate-property" => Encoding.UTF8.GetBytes($"{{\"{new string('x', 300)}\":0,\"{new string('x', 300)}\":1}}"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The UTF-8 admission scenario is not defined."),
        };
}
