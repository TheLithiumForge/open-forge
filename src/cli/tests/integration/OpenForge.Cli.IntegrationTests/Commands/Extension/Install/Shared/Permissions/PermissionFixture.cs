using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

internal static class PermissionFixture
{
    internal const string ExternalPath = ".apm/agents/team.md";
    internal const string PermissionPath = ".agents/open-forge.json";
    internal const string Grants = """{"allowInstallPaths":[".apm/agents/team.md"]}""";

    internal static TemporaryWorkspace CreatePackage(string target = ExternalPath)
    {
        var source = TemporaryWorkspace.Create("permission-content");
        try
        {
            source.CreateFile("extension.json", """{"id":"team","name":"Team","description":"Test package","version":"1.0.0","dependencies":[]}""");
            source.CreateFile($"content/{target}", "content bytes\n");
            return source;
        }
        catch
        {
            source.Dispose();
            throw;
        }
    }

    internal static void DeleteExternalOutput(ExtensionInstallIntegrationWorkspace workspace)
    {
        var path = workspace.Combine(".apm");
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
