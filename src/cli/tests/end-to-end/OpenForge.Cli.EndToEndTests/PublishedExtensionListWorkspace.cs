using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedExtensionListWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly TemporaryWorkspace _source;

    private PublishedExtensionListWorkspace(
        TemporaryWorkspace workspace,
        TemporaryWorkspace source)
    {
        _workspace = workspace;
        _source = source;
    }

    internal string Path => _workspace.Path;

    internal string SourcePath => _source.Path;

    internal string MissingSourcePath => _source.Combine("missing-source");

    internal IReadOnlyDictionary<string, string> SnapshotState() => PublishedWorkspaceTreeSnapshot.Capture(_workspace.Path);

    internal IReadOnlyDictionary<string, string> SnapshotSource() => PublishedWorkspaceTreeSnapshot.Capture(_source.Path);

    internal static PublishedExtensionListWorkspace Create(
        bool trustedInstalled = false,
        bool opaqueFrameworkDuplicates = false)
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-list");
        var source = TemporaryWorkspace.Create("e2e-extension-source");
        try
        {
            workspace.WriteText(
                ".agents/open-forge.lifecycle.json",
                Lifecycle(workspace.Path, trustedInstalled, opaqueFrameworkDuplicates));
            source.WriteText(
                "extension.json",
                """
                {
                  "id": "local-toolkit",
                  "name": "Local Toolkit",
                  "description": "A local Extension package.",
                  "version": "1.2.3",
                  "dependencies": []
                }
                """);
            return new PublishedExtensionListWorkspace(
                workspace: workspace,
                source: source);
        }
        catch
        {
            source.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal string CreateMalformedSource()
    {
        _source.WriteText("malformed/extension.json", "{ \"id\": \"invalid\", \"id\": \"duplicate\" }");
        return _source.Combine("malformed");
    }

    public void Dispose()
    {
        _source.Dispose();
        _workspace.Dispose();
    }

    private static string Lifecycle(
        string workspacePath,
        bool trustedInstalled,
        bool opaqueFrameworkDuplicates)
    {
        var packages = trustedInstalled
            ? """
              [{
                "id": "development-toolkit",
                "version": "0.1.0",
                "source": "embedded catalogue",
                "dependencies": [],
                "paths": [".agents/workflows/architecture.md"]
              }]
              """
            : "[]";
        var paths = trustedInstalled
            ? """
              [{
                "path": ".agents/workflows/architecture.md",
                "owners": ["development-toolkit"],
                "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                "fingerprintKind": "semantic"
              }]
              """
            : "[]";
        var framework = opaqueFrameworkDuplicates
            ? """
              {
                "settings": { "enabled": true, "enabled": false },
                "settings": null
              }
              """
            : "null";
        return $$"""
            {
              "schemaVersion": 1,
              "fingerprintPolicy": "open-forge-markdown-v1",
              "workspacePath": "{{JsonEncodedText.Encode(System.IO.Path.GetFullPath(workspacePath))}}",
              "framework": {{framework}},
              "extensions": {
                "coverage": "complete",
                "packages": {{packages}},
                "paths": {{paths}}
              }
            }
            """;
    }

}
