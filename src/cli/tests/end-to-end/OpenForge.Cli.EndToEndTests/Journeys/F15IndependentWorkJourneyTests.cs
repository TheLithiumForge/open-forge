using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F15IndependentWorkJourneyTests
{
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string OldNotePath = ".agents/patterns/old-note.md";
    private const string ToolkitTargetPath = ".agents/guidance/toolkit.md";
    private const string ToolkitSourcePath = "toolkit/content/.agents/guidance/toolkit.md";
    private const string ToolkitId = "toolkit";
    private const string SourceRoot = "shared-guides";
    private const string DestinationRoot = ".agents/guidance/team";
    private const string TeamRoutePath = DestinationRoot + "/_team.md";

    private static readonly byte[] MalformedRoutedNote = Encoding.UTF8.GetBytes(
        "---\nopen-forge:\n  description: [unterminated\n---\n# Unrelated F15 note\n\nDistinctive malformed routed bytes.\n");

    private static readonly byte[] ExistingAllegedToolkit = Encoding.UTF8.GetBytes(
        "---\nopen-forge:\n  description: Existing alleged toolkit\n  tags: [Guidance]\n---\n# Existing alleged toolkit\n\nNot authorized by the malformed lock.\n");

    private const string TeamRoute = "---\nopen-forge:\n  description: Team\n  tags: [Guidance]\n---\n# Team\n\n## Entries\n";

    [Fact(DisplayName = "F15 continues the selected install beside unrelated routed corruption"), Trait("Feature", "independent-work-safety"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F15")]
    public async Task ContinuesSelectedInstallAndCarriesKnownFactsThroughReadOnlyCommands()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f15-independent-install");
        await InstallFrameworkAsync(workspace);

        using var catalogue = ToolkitCatalogue.Create("f15-independent-catalogue");
        workspace.ExpectFiles(OldNotePath, ToolkitTargetPath);
        workspace.WriteBytes(OldNotePath, MalformedRoutedNote);

        var sourceBytes = File.ReadAllBytes(catalogue.Combine(ToolkitSourcePath));
        var sourceSnapshot = catalogue.SnapshotTree();
        var install = await workspace.RunAsync(
            "extension", "install", ToolkitId,
            "--source", catalogue.Path,
            "--automatic");

        Assert.Equal(2, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.Contains(OldNotePath, install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(ToolkitId, install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(MalformedRoutedNote, File.ReadAllBytes(workspace.Combine(OldNotePath)));
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(ToolkitTargetPath)));
        Assert.Equal(sourceSnapshot, catalogue.SnapshotTree());
        AssertExtensionRegistration(workspace, ToolkitId, ToolkitTargetPath, catalogue.Path);

        var inspect = await RunReadOnlyAsync(
            workspace,
            catalogue,
            "extension", "inspect", ToolkitId,
            "--source", catalogue.Path);
        Assert.Equal(0, inspect.ExitCode);
        Assert.Equal(string.Empty, inspect.StandardError);
        Assert.Contains(ToolkitId, inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);

        var status = await RunReadOnlyAsync(workspace, "status");
        Assert.Equal(2, status.ExitCode);
        Assert.Equal(string.Empty, status.StandardError);
        Assert.Contains(OldNotePath, status.StandardOutput, StringComparison.OrdinalIgnoreCase);

        var update = await RunReadOnlyAsync(
            workspace,
            catalogue,
            "extension", "update", ToolkitId,
            "--source", catalogue.Path,
            "--dry-run");
        Assert.Equal(0, update.ExitCode);
        Assert.Equal(string.Empty, update.StandardError);
        Assert.Contains(ToolkitId, update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(ToolkitTargetPath)));
        Assert.Equal(MalformedRoutedNote, File.ReadAllBytes(workspace.Combine(OldNotePath)));
    }

    [Fact(DisplayName = "F15 stops only the install that needs an unreadable source"), Trait("Feature", "independent-work-safety"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F15")]
    public async Task UnreadableRequiredSourceDoesNotCreateTargetOrClaim()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F15 source read denial requires the Windows share boundary.");
            return;
        }

        using var workspace = PublishedJourneyWorkspace.Create("f15-source-unreadable");
        await InstallFrameworkAsync(workspace);

        using var catalogue = ToolkitCatalogue.Create("f15-source-unreadable-catalogue");
        workspace.ExpectFiles(OldNotePath, ToolkitTargetPath);
        workspace.WriteBytes(OldNotePath, MalformedRoutedNote);
        var ownershipBefore = File.ReadAllBytes(workspace.Combine(OwnershipPath));
        var workspaceBefore = workspace.SnapshotState();
        var sourceSnapshot = catalogue.SnapshotTree();
        var sourcePath = catalogue.Combine(ToolkitSourcePath);

        using (var sourceLock = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            AssertReadDenied(sourcePath);
            var install = await workspace.RunAsync(
                "extension", "install", ToolkitId,
                "--source", catalogue.Path,
                "--automatic");

            Assert.Equal(3, install.ExitCode);
            Assert.Equal(string.Empty, install.StandardError);
            Assert.Contains("source", install.StandardOutput, StringComparison.OrdinalIgnoreCase);
            AssertNoEntry(workspace, ToolkitTargetPath);
            Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
            Assert.Equal(workspaceBefore, workspace.SnapshotState());
        }

        Assert.Equal(sourceSnapshot, catalogue.SnapshotTree());
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F15 does not use malformed ownership to update or remove an alleged package"), Trait("Feature", "ownership-authority"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F15")]
    public async Task MalformedOwnershipDoesNotAuthorizeUpdateOrRemove()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f15-malformed-ownership");
        await InstallFrameworkAsync(workspace);

        using var catalogue = ToolkitCatalogue.Create("f15-malformed-ownership-catalogue");
        workspace.ExpectFiles(ToolkitTargetPath);
        workspace.WriteBytes(ToolkitTargetPath, ExistingAllegedToolkit);
        workspace.WriteBytes(OwnershipPath, Encoding.UTF8.GetBytes("{ malformed ownership record"));
        var targetBefore = File.ReadAllBytes(workspace.Combine(ToolkitTargetPath));
        var ownershipBefore = File.ReadAllBytes(workspace.Combine(OwnershipPath));
        var sourceSnapshot = catalogue.SnapshotTree();

        var update = await RunReadOnlyAsync(
            workspace,
            catalogue,
            "extension", "update", ToolkitId,
            "--source", catalogue.Path,
            "--dry-run");
        Assert.Equal(2, update.ExitCode);
        Assert.Equal(string.Empty, update.StandardError);
        Assert.Contains("ownership", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(targetBefore, File.ReadAllBytes(workspace.Combine(ToolkitTargetPath)));
        Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        Assert.Equal(sourceSnapshot, catalogue.SnapshotTree());

        var remove = await workspace.RunAsync(
            "extension", "remove", ToolkitId,
            "--automatic", "--dry-run");
        Assert.Equal(3, remove.ExitCode);
        Assert.Equal(string.Empty, remove.StandardError);
        Assert.Contains("record", remove.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(targetBefore, File.ReadAllBytes(workspace.Combine(ToolkitTargetPath)));
        Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
    }

    [Fact(DisplayName = "F15 permits a fresh install from a known empty ownership section"), Trait("Feature", "ownership-authority"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F15")]
    public async Task KnownEmptyOwnershipPermitsFreshInstall()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f15-known-empty-ownership");
        await InstallFrameworkAsync(workspace);

        using var catalogue = ToolkitCatalogue.Create("f15-known-empty-ownership-catalogue");
        workspace.ExpectFiles(ToolkitTargetPath);
        WriteKnownEmptyOwnership(workspace);
        var sourceBytes = File.ReadAllBytes(catalogue.Combine(ToolkitSourcePath));
        var sourceSnapshot = catalogue.SnapshotTree();

        var install = await workspace.RunAsync(
            "extension", "install", ToolkitId,
            "--source", catalogue.Path,
            "--automatic");

        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.Contains(ToolkitId, install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(ToolkitTargetPath)));
        Assert.Equal(sourceSnapshot, catalogue.SnapshotTree());
        AssertExtensionRegistration(workspace, ToolkitId, ToolkitTargetPath, catalogue.Path);
    }

    [Fact(DisplayName = "F15 blocks attach without effects when ownership is malformed"), Trait("Feature", "ownership-authority"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F15")]
    public async Task MalformedOwnershipBlocksAttachWithoutEffects()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f15-malformed-attach");
        await InstallFrameworkAsync(workspace);

        workspace.ExpectFiles(
            TeamRoutePath,
            $"{SourceRoot}/a.md",
            $"{SourceRoot}/b.md",
            $"{DestinationRoot}/a.md",
            $"{DestinationRoot}/b.md");
        workspace.WriteText(TeamRoutePath, TeamRoute);
        workspace.WriteText($"{SourceRoot}/a.md", TeamSource("a"));
        workspace.WriteText($"{SourceRoot}/b.md", TeamSource("b"));
        var malformedOwnership = Encoding.UTF8.GetBytes("{ malformed ownership record");
        workspace.WriteBytes(OwnershipPath, malformedOwnership);
        var workspaceBefore = workspace.SnapshotState();
        var sourceBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["a.md"] = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md")),
            ["b.md"] = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/b.md")),
        };

        var attach = await workspace.RunAsync(
            "library", "attach", "team", SourceRoot,
            "--to", DestinationRoot,
            "--automatic");

        Assert.Equal(5, attach.ExitCode);
        Assert.Equal(string.Empty, attach.StandardOutput);
        Assert.Contains(OwnershipPath, attach.StandardError, StringComparison.Ordinal);
        Assert.Contains("invalid", attach.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(malformedOwnership, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        AssertNoEntry(workspace, $"{DestinationRoot}/a.md");
        AssertNoEntry(workspace, $"{DestinationRoot}/b.md");
        Assert.Equal(sourceBytes["a.md"], File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md")));
        Assert.Equal(sourceBytes["b.md"], File.ReadAllBytes(workspace.Combine($"{SourceRoot}/b.md")));
        Assert.Equal(workspaceBefore, workspace.SnapshotState());
    }

    private static async Task InstallFrameworkAsync(PublishedJourneyWorkspace workspace)
    {
        workspace.ExpectCoreInstall();
        var install = await workspace.RunAsync("install", "--automatic");
        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
    }

    private static async Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
    {
        var before = workspace.SnapshotState();
        var result = await workspace.RunAsync(arguments);
        Assert.Equal(before, workspace.SnapshotState());
        return result;
    }

    private static async Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        ToolkitCatalogue catalogue,
        params string[] arguments)
    {
        var workspaceBefore = workspace.SnapshotState();
        var catalogueBefore = catalogue.SnapshotTree();
        var result = await workspace.RunAsync(arguments);
        Assert.Equal(workspaceBefore, workspace.SnapshotState());
        Assert.Equal(catalogueBefore, catalogue.SnapshotTree());
        return result;
    }

    private static void AssertReadDenied(string path)
    {
        var denied = false;
        try
        {
            _ = File.ReadAllBytes(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            denied = true;
        }

        Assert.True(denied, "The locked source remained readable; FileShare.None was not independently proven.");
    }

    private static void AssertExtensionRegistration(
        PublishedJourneyWorkspace workspace,
        string id,
        string expectedPath,
        string expectedSource)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var extension = Assert.Single(
            document.RootElement.GetProperty("extensions").EnumerateArray(),
            value => string.Equals(value.GetProperty("id").GetString(), id, StringComparison.Ordinal));
        Assert.Equal("1.0.0", extension.GetProperty("version").GetString());
        Assert.Equal(expectedSource, extension.GetProperty("source").GetString());
        Assert.Equal(
            new[] { expectedPath },
            extension.GetProperty("paths").EnumerateArray().Select(value => value.GetString()!).ToArray());
        Assert.Empty(extension.GetProperty("dependencies").EnumerateArray());
        Assert.Empty(extension.GetProperty("regions").EnumerateArray());
    }

    private static void WriteKnownEmptyOwnership(PublishedJourneyWorkspace workspace)
    {
        var ownershipPath = workspace.Combine(OwnershipPath);
        using var source = JsonDocument.Parse(File.ReadAllBytes(ownershipPath));
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            foreach (var property in source.RootElement.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);
                if (property.NameEquals("extensions") || property.NameEquals("libraries"))
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
                else
                {
                    property.Value.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        workspace.WriteBytes(OwnershipPath, stream.ToArray());
    }

    private static string TeamSource(string id)
        => $"---\nopen-forge:\n  description: Team source {id}\n  tags: [Guidance]\n---\n# Team source {id}\n\nF15 source bytes for {id}.\n";

    private static void AssertLibraryRegistration(
        PublishedJourneyWorkspace workspace,
        string id,
        string sourceRoot,
        string destinationRoot,
        IReadOnlyList<string> expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var library = Assert.Single(
            document.RootElement.GetProperty("libraries").EnumerateArray(),
            value => string.Equals(value.GetProperty("id").GetString(), id, StringComparison.Ordinal));
        Assert.Equal(sourceRoot, library.GetProperty("sourceRoot").GetString());
        Assert.Equal(destinationRoot, library.GetProperty("destinationRoot").GetString());
        Assert.Equal(expectedPaths, library.GetProperty("paths").EnumerateArray().Select(value => value.GetString()!).ToArray());
    }

    private static void AssertRelativeFileLink(PublishedJourneyWorkspace workspace, string relativePath)
    {
        var linkPath = workspace.Combine($"{DestinationRoot}/{relativePath}");
        var sourcePath = workspace.Combine($"{SourceRoot}/{relativePath}");
        var info = new FileInfo(linkPath);
        var attributes = File.GetAttributes(linkPath);
        Assert.True((attributes & FileAttributes.ReparsePoint) != 0);
        Assert.True((attributes & FileAttributes.Directory) == 0);
        Assert.Equal(
            System.IO.Path.GetRelativePath(
                workspace.Combine(DestinationRoot),
                sourcePath).Replace('\\', '/'),
            info.LinkTarget);
    }

    private static void AssertOrdinaryDestinationParents(PublishedJourneyWorkspace workspace)
    {
        foreach (var relativePath in new[] { ".agents", ".agents/guidance", DestinationRoot })
        {
            var attributes = File.GetAttributes(workspace.Combine(relativePath));
            Assert.True((attributes & FileAttributes.Directory) != 0);
            Assert.True((attributes & FileAttributes.ReparsePoint) == 0);
        }
    }

    private static void AssertNoEntry(PublishedJourneyWorkspace workspace, string relativePath)
    {
        var path = workspace.Combine(relativePath);
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(path));
        Assert.Null(new FileInfo(path).LinkTarget);
    }

    private sealed class ToolkitCatalogue : IDisposable
    {
        private readonly TemporaryWorkspace _workspace;

        private ToolkitCatalogue(TemporaryWorkspace workspace)
        {
            _workspace = workspace;
        }

        internal string Path => _workspace.Path;

        internal static ToolkitCatalogue Create(string purpose)
        {
            var workspace = TemporaryWorkspace.Create(purpose);
            workspace.WriteText(
                "toolkit/extension.json",
                """
                {
                  "id": "toolkit",
                  "name": "toolkit",
                  "description": "F15 toolkit source.",
                  "version": "1.0.0",
                  "dependencies": []
                }
                """);
            workspace.WriteText(
                ToolkitSourcePath,
                "---\nopen-forge:\n  description: Toolkit source\n  tags: [Guidance]\n---\n# Toolkit source\n\nF15 source bytes.\n");
            return new ToolkitCatalogue(workspace);
        }

        internal string Combine(string relativePath) => _workspace.Combine(relativePath);

        internal IReadOnlyDictionary<string, string> SnapshotTree()
        {
            var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
            CaptureTree(new DirectoryInfo(_workspace.Path), _workspace.Path, snapshot);
            return snapshot;
        }

        private static void CaptureTree(
            DirectoryInfo directory,
            string rootPath,
            IDictionary<string, string> snapshot)
        {
            foreach (var entry in directory
                         .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                         .OrderBy(entry => entry.Name, StringComparer.Ordinal))
            {
                var relativePath = System.IO.Path.GetRelativePath(rootPath, entry.FullName).Replace('\\', '/');
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    snapshot[relativePath] = $"link:{entry.LinkTarget}";
                    continue;
                }

                if ((entry.Attributes & FileAttributes.Directory) != 0)
                {
                    snapshot[$"{relativePath}/"] = "directory";
                    CaptureTree((DirectoryInfo)entry, rootPath, snapshot);
                    continue;
                }

                if (entry is FileInfo && (entry.Attributes & FileAttributes.Device) == 0)
                {
                    snapshot[relativePath] =
                        $"file:{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(entry.FullName)))}";
                }
            }
        }

        public void Dispose() => _workspace.Dispose();
    }
}
