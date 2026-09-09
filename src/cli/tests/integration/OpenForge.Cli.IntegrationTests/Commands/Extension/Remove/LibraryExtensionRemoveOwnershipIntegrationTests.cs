using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class LibraryExtensionRemoveOwnershipIntegrationTests
{
    private const string Target = ".agents/toolkit/note.md";
    private const string SourceTarget = "shared/team-knowledge/.agents/toolkit/note.md";
    private const string Body = "---\nopen-forge:\n  description: Note\n  tags: [Toolkit]\n---\n# Note\n\nPreserve source bytes.\n";
    private const string Record = """
        {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/toolkit/note.md"]}]}
        """;

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true)]
    public static async Task RecordClaimBlocksOrdinaryRemovalPruneAndKeepAsUnmanaged(bool prune, bool changed)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-remove-record");
        using var source = ExtensionInstallCatalogue.Create("library-extension-remove-source");
        await InstallAsync(workspace, source);
        workspace.CreateOccupant(".agents/open-forge.libraries.json", Record);
        using var entries = new LibraryEntries(workspace);
        entries.CreateSource();
        if (changed)
        {
            workspace.ReplaceText(Target, Body + "Changed consumer bytes.\n");
        }

        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var arguments = prune
            ? new[] { "extension", "remove", "toolkit", "--prune", "--automatic", "--json" }
            : ["extension", "remove", "toolkit", "--automatic", "--json"];
        var run = await workspace.RunAsync(arguments);
        Assert.True(run.ExitCode == 5, $"Expected Library ownership refusal: {run.ExitCode}; {run.StandardError}; {run.StandardOutput}");
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Contains(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-remove.ownership-conflict");
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("absent"), InlineData("malformed"), InlineData("registered")]
    public static async Task ActualProjectionLinkIsProtectedIndependentlyOfRecordAuthority(string record)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-remove-link");
        using var source = ExtensionInstallCatalogue.Create("library-extension-remove-link-source");
        await InstallAsync(workspace, source);
        using var entries = new LibraryEntries(workspace);
        entries.CreateSource();
        if (record != "absent")
        {
            workspace.CreateOccupant(".agents/open-forge.libraries.json", record == "registered" ? Record : "{ malformed");
        }

        File.Delete(workspace.Combine(Target));
        entries.CreateProjectionLink("../../shared/team-knowledge/.agents/toolkit/note.md");
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var run = await workspace.RunAsync(["extension", "remove", "toolkit", "--prune", "--automatic", "--json"]);
        Assert.True(run.ExitCode == 5, $"Expected no-follow refusal: {run.ExitCode}; {run.StandardError}; {run.StandardOutput}");
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("effects").EnumerateArray());
        Assert.Equal("../../shared/team-knowledge/.agents/toolkit/note.md", new FileInfo(workspace.Combine(Target)).LinkTarget);
        Assert.Equal(Body, File.ReadAllText(workspace.Combine(SourceTarget)));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    private static async Task InstallAsync(ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (Target, Body));
        var installed = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--json"]);
        Assert.True(installed.ExitCode == 0, $"Extension fixture installation prerequisite: {installed.ExitCode}; {installed.StandardError}; {installed.StandardOutput}");
        var ownership = await new LifecycleOwnershipReader(new PhysicalPathResolver()).ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(LifecycleOwnershipReadState.Trusted, ownership.Framework.State);
        Assert.Equal(LifecycleOwnershipReadState.Trusted, ownership.Extensions.State);
        Assert.Contains(ownership.Claims, claim => claim.Path == Target && claim.Manager == LifecycleOwnershipManager.Extension && claim.Owner == "toolkit");
    }

    private sealed class LibraryEntries(ExtensionInstallIntegrationWorkspace workspace) : IDisposable
    {
        private readonly List<string> _directories = [];
        private bool _sourceCreated;
        private string? _projectionLinkTarget;

        internal void CreateSource()
        {
            string[] directories = ["shared", "shared/team-knowledge", "shared/team-knowledge/.agents", "shared/team-knowledge/.agents/toolkit"];
            foreach (var directory in directories)
            {
                var path = workspace.Combine(directory);
                if (ReadAttributes(path) is not null)
                {
                    throw new InvalidOperationException("Library source setup cannot adopt an existing entry.");
                }

                Directory.CreateDirectory(path);
                _directories.Add(path);
            }

            using var stream = new FileStream(workspace.Combine(SourceTarget), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            _sourceCreated = true;
            using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            writer.Write(Body);
        }

        internal void CreateProjectionLink(string rawTarget)
        {
            File.CreateSymbolicLink(workspace.Combine(Target), rawTarget);
            _projectionLinkTarget = rawTarget;
        }

        public void Dispose()
        {
            try
            {
                if (_projectionLinkTarget is { } expectedTarget)
                {
                    var path = workspace.Combine(Target);
                    if (ReadAttributes(path) is { } attributes)
                    {
                        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != FileAttributes.ReparsePoint
                            || new FileInfo(path).LinkTarget != expectedTarget)
                        {
                            throw new InvalidOperationException("The test-created Library projection was replaced; preserve the unknown entry.");
                        }

                        File.Delete(path);
                    }
                }
            }
            finally
            {
                DeleteSource();
            }
        }

        private void DeleteSource()
        {
            var source = workspace.Combine(SourceTarget);
            if (_sourceCreated && ReadAttributes(source) is { } attributes)
            {
                if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
                {
                    throw new InvalidOperationException("The test-created Library source was replaced; preserve the unknown entry.");
                }

                File.Delete(source);
            }

            for (var index = _directories.Count - 1; index >= 0; index--)
            {
                var directory = _directories[index];
                if (ReadAttributes(directory) is not { } directoryAttributes)
                {
                    continue;
                }

                if ((directoryAttributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != FileAttributes.Directory)
                {
                    throw new InvalidOperationException("A test-created Library source directory was replaced; preserve the unknown entry.");
                }

                Directory.Delete(directory, recursive: false);
            }
        }

        private static FileAttributes? ReadAttributes(string path)
        {
            try
            {
                return File.GetAttributes(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
