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
        bool leftoverState = false)
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-list");
        var source = TemporaryWorkspace.Create("e2e-extension-source");
        try
        {
            workspace.WriteText(
                ".agents/open-forge.lock.json",
                Ownership(trustedInstalled));
            if (leftoverState) workspace.WriteText(".agents/open-forge.lifecycle.json", "{ obsolete and malformed }");
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

    private static string Ownership(bool trustedInstalled)
    {
        var packages = trustedInstalled
            ? """
              [{"id":"development-toolkit","version":"0.1.0","source":"embedded catalogue","dependencies":[],"paths":[],"regions":[]}]
              """ : "[]";
        return $$"""
            {"schemaVersion":1,"extensions":{{packages}}}
            """;
    }
}
