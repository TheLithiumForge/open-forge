using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F16LibraryLifecycleJourneyTests
{
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string LibraryId = "team";
    private const string SourceRoot = "shared-guides";
    private const string DestinationRoot = ".agents/guidance/team";
    private const string RoutePath = DestinationRoot + "/_team.md";

    private const string RoutePrefix = "---\nopen-forge:\n  description: Team route authored description\n  tags: [Guidance]\n---\n# Team route authored heading\n\nF16 independently authored prefix.\n\n## Entries\n";
    private const string RouteSuffix = "## F16 authored suffix\n\nF16 independently authored suffix.\n";
    private const string RouteBody = RoutePrefix + "\n" + RouteSuffix;

    private const string AlphaDescription = "Alpha source guidance";
    private const string BetaDescription = "Beta source guidance";
    private const string GammaDescription = "Gamma source guidance";
    private const string DeltaDescription = "Delta source guidance";

    [Fact(DisplayName = "F16 carries Library membership through attach, sync, and detach"), Trait("Feature", "library-membership-lifecycle"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F16")]
    public async Task CarriesMembershipThroughChangingSourceAndDetach()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f16-library-lifecycle");
        await InstallFrameworkAsync(workspace);
        PrepareWorkspace(workspace, ["a", "b", "c", "d"], ["a", "b"]);
        Assert.Equal(RouteBody, File.ReadAllText(workspace.Combine(RoutePath)));

        var sourceBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["a.md"] = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md")),
            ["b.md"] = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/b.md")),
        };

        var attach = await workspace.RunAsync(
            "library", "attach", LibraryId, SourceRoot,
            "--to", DestinationRoot,
            "--automatic");
        AssertCompleted(attach, LibraryId);
        AssertLibraryRegistration(workspace, ["a.md", "b.md"]);
        AssertLinkedMemberSet(workspace, "a", "b");
        AssertRouteEntries(workspace, "a", "b");
        AssertRelativeFileLink(workspace, "a");
        AssertRelativeFileLink(workspace, "b");
        AssertOrdinaryDestinationParents(workspace);
        Assert.Equal(sourceBytes["a.md"], File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md")));
        Assert.Equal(sourceBytes["b.md"], File.ReadAllBytes(workspace.Combine($"{SourceRoot}/b.md")));

        var inspect = await RunReadOnlyAsync(workspace, "library", "inspect", LibraryId);
        Assert.Equal(0, inspect.ExitCode);
        Assert.Equal(string.Empty, inspect.StandardError);
        Assert.Contains(LibraryId, inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertLibraryRegistration(workspace, ["a.md", "b.md"]);
        AssertLinkedMemberSet(workspace, "a", "b");
        AssertRouteEntries(workspace, "a", "b");
        AssertRelativeFileLink(workspace, "a");
        AssertRelativeFileLink(workspace, "b");

        workspace.WriteText($"{SourceRoot}/c.md", TeamSource("c"));
        var inspectAfterAddition = await RunReadOnlyAsync(workspace, "library", "inspect", LibraryId);
        Assert.Equal(2, inspectAfterAddition.ExitCode);
        Assert.Equal(string.Empty, inspectAfterAddition.StandardError);
        Assert.Contains("c.md", inspectAfterAddition.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertNoEntry(workspace, "c");
        AssertLibraryRegistration(workspace, ["a.md", "b.md"]);
        AssertLinkedMemberSet(workspace, "a", "b");
        AssertRouteEntries(workspace, "a", "b");

        var firstSync = await workspace.RunAsync(
            "library", "sync", LibraryId, "--automatic");
        AssertCompleted(firstSync, LibraryId);
        AssertLibraryRegistration(workspace, ["a.md", "b.md", "c.md"]);
        AssertLinkedMemberSet(workspace, "a", "b", "c");
        AssertRouteEntries(workspace, "a", "b", "c");
        AssertRelativeFileLink(workspace, "a");
        AssertRelativeFileLink(workspace, "b");
        AssertRelativeFileLink(workspace, "c");
        Assert.Equal(TeamSource("c"), File.ReadAllText(workspace.Combine($"{SourceRoot}/c.md")));

        File.Delete(workspace.Combine($"{SourceRoot}/b.md"));
        workspace.WriteText($"{SourceRoot}/d.md", TeamSource("d"));
        var secondSync = await workspace.RunAsync(
            "library", "sync", LibraryId, "--automatic");
        AssertCompleted(secondSync, LibraryId);
        AssertNoEntry(workspace, "b");
        AssertLinkedMemberSet(workspace, "a", "c", "d");
        AssertRouteEntries(workspace, "a", "c", "d");
        AssertRelativeFileLink(workspace, "a");
        AssertRelativeFileLink(workspace, "c");
        AssertRelativeFileLink(workspace, "d");
        AssertLibraryRegistration(workspace, ["a.md", "c.md", "d.md"]);
        Assert.Equal(TeamSource("a"), File.ReadAllText(workspace.Combine($"{SourceRoot}/a.md")));
        Assert.Equal(TeamSource("c"), File.ReadAllText(workspace.Combine($"{SourceRoot}/c.md")));
        Assert.Equal(TeamSource("d"), File.ReadAllText(workspace.Combine($"{SourceRoot}/d.md")));

        var detach = await workspace.RunAsync(
            "library", "detach", LibraryId, "--automatic");
        AssertCompleted(detach, LibraryId);
        Assert.Contains("source", detach.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ReadLibraryPaths(workspace));
        AssertLinkedMemberSet(workspace);
        AssertRouteEntries(workspace);
        foreach (var member in new[] { "a", "c", "d" })
        {
            AssertNoEntry(workspace, member);
            Assert.True(File.Exists(workspace.Combine($"{SourceRoot}/{member}.md")));
        }

        Assert.Equal(TeamSource("a"), File.ReadAllText(workspace.Combine($"{SourceRoot}/a.md")));
        Assert.Equal(TeamSource("c"), File.ReadAllText(workspace.Combine($"{SourceRoot}/c.md")));
        Assert.Equal(TeamSource("d"), File.ReadAllText(workspace.Combine($"{SourceRoot}/d.md")));
        Assert.True(File.Exists(workspace.Combine(RoutePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F16 records an empty Library before its first source member exists"), Trait("Feature", "library-membership-lifecycle"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F16")]
    public async Task EmptySourceRegistersThenSyncsTheFirstMember()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f16-empty-library");
        await InstallFrameworkAsync(workspace);
        PrepareWorkspace(workspace, ["a"], []);
        Directory.CreateDirectory(workspace.Combine(SourceRoot));

        var attach = await workspace.RunAsync(
            "library", "attach", LibraryId, SourceRoot,
            "--to", DestinationRoot,
            "--automatic");
        AssertCompleted(attach, LibraryId);
        AssertLibraryRegistration(workspace, []);
        AssertNoEntry(workspace, "a");

        workspace.WriteText($"{SourceRoot}/a.md", TeamSource("a"));
        var sync = await workspace.RunAsync(
            "library", "sync", LibraryId, "--automatic");
        AssertCompleted(sync, LibraryId);
        AssertLibraryRegistration(workspace, ["a.md"]);
        AssertRelativeFileLink(workspace, "a");
        Assert.Equal(TeamSource("a"), File.ReadAllText(workspace.Combine($"{SourceRoot}/a.md")));
    }

    [Fact(DisplayName = "F16 preserves registered links when the source inventory is incomplete"), Trait("Feature", "library-membership-safety"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F16")]
    public async Task IncompleteSourceInventoryDoesNotInferRetirement()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F16 source read denial requires the Windows share boundary.");
            return;
        }

        using var workspace = PublishedJourneyWorkspace.Create("f16-incomplete-library");
        await InstallFrameworkAsync(workspace);
        PrepareWorkspace(workspace, ["a", "b"], ["a", "b"]);

        var attach = await workspace.RunAsync(
            "library", "attach", LibraryId, SourceRoot,
            "--to", DestinationRoot,
            "--automatic");
        AssertCompleted(attach, LibraryId);
        var aTarget = LinkTarget(workspace, "a");
        var bTarget = LinkTarget(workspace, "b");
        var registrationBefore = File.ReadAllBytes(workspace.Combine(OwnershipPath));
        var aBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md"));
        var bPath = workspace.Combine($"{SourceRoot}/b.md");

        using (var sourceLock = new FileStream(bPath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            AssertReadDenied(bPath);
            var sync = await workspace.RunAsync(
                "library", "sync", LibraryId, "--automatic");
            Assert.Equal(3, sync.ExitCode);
            Assert.Equal(string.Empty, sync.StandardError);
            Assert.Contains("source", sync.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(aTarget, LinkTarget(workspace, "a"));
            Assert.Equal(bTarget, LinkTarget(workspace, "b"));
            Assert.Equal(registrationBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        }

        Assert.Equal(aBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/a.md")));
        Assert.Equal(TeamSource("b"), File.ReadAllText(bPath));
        AssertLibraryRegistration(workspace, ["a.md", "b.md"]);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static async Task InstallFrameworkAsync(PublishedJourneyWorkspace workspace)
    {
        workspace.ExpectCoreInstall();
        var install = await workspace.RunAsync("install", "--automatic");
        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
    }

    private static void PrepareWorkspace(
        PublishedJourneyWorkspace workspace,
        IReadOnlyList<string> reservedSourceMembers,
        IReadOnlyList<string> initialSourceMembers)
    {
        var reserved = new List<string>
        {
            RoutePath,
        };
        reserved.AddRange(reservedSourceMembers.Select(member => $"{SourceRoot}/{member}.md"));
        reserved.AddRange(new[] { "a", "b", "c", "d" }.Select(member => $"{DestinationRoot}/{member}.md"));
        workspace.ExpectFiles([.. reserved]);
        workspace.WriteText(RoutePath, RouteBody);
        foreach (var member in initialSourceMembers)
        {
            workspace.WriteText($"{SourceRoot}/{member}.md", TeamSource(member));
        }
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

    private static void AssertCompleted(ProcessRunResult result, string subject)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(subject, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static string TeamSource(string member)
        => member switch
        {
            "a" => $"---\nopen-forge:\n  description: {AlphaDescription}\n  tags: [Guidance]\n---\n# Alpha authored member\n\nF16 authored source bytes for member a.\n",
            "b" => $"---\nopen-forge:\n  description: {BetaDescription}\n  tags: [Guidance]\n---\n# Beta authored member\n\nF16 authored source bytes for member b.\n",
            "c" => $"---\nopen-forge:\n  description: {GammaDescription}\n  tags: [Guidance]\n---\n# Gamma authored member\n\nF16 authored source bytes for member c.\n",
            "d" => $"---\nopen-forge:\n  description: {DeltaDescription}\n  tags: [Guidance]\n---\n# Delta authored member\n\nF16 authored source bytes for member d.\n",
            _ => throw new ArgumentOutOfRangeException(nameof(member), member, "Unknown F16 fixture member."),
        };

    private static void AssertLibraryRegistration(
        PublishedJourneyWorkspace workspace,
        IReadOnlyList<string> expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var library = Assert.Single(
            document.RootElement.GetProperty("libraries").EnumerateArray(),
            value => string.Equals(value.GetProperty("id").GetString(), LibraryId, StringComparison.Ordinal));
        Assert.Equal(SourceRoot, library.GetProperty("sourceRoot").GetString());
        Assert.Equal(DestinationRoot, library.GetProperty("destinationRoot").GetString());
        Assert.Equal(expectedPaths, library.GetProperty("paths").EnumerateArray().Select(value => value.GetString()!).ToArray());
    }

    private static IReadOnlyList<string> ReadLibraryPaths(PublishedJourneyWorkspace workspace)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var libraries = document.RootElement.GetProperty("libraries").EnumerateArray().ToArray();
        return libraries.Length == 0
            ? []
            : Assert.Single(
                    libraries,
                    value => string.Equals(value.GetProperty("id").GetString(), LibraryId, StringComparison.Ordinal))
                .GetProperty("paths")
                .EnumerateArray()
                .Select(value => value.GetString()!)
                .ToArray();
    }

    private static void AssertRouteEntries(
        PublishedJourneyWorkspace workspace,
        params string[] expectedMembers)
    {
        Assert.Equal(
            RouteWithEntries(expectedMembers),
            File.ReadAllText(workspace.Combine(RoutePath)));
    }

    private static string RouteWithEntries(IReadOnlyList<string> members)
    {
        var entries = members.Count == 0
            ? "- none - No entries - #Empty"
            : string.Join("\n", members.Select(ExpectedEntry));
        return RoutePrefix + "\n" + entries + "\n\n" + RouteSuffix;
    }

    private static string ExpectedEntry(string member)
        => member switch
        {
            "a" => $"- [{AlphaDescription}](a.md) - #Guidance",
            "b" => $"- [{BetaDescription}](b.md) - #Guidance",
            "c" => $"- [{GammaDescription}](c.md) - #Guidance",
            "d" => $"- [{DeltaDescription}](d.md) - #Guidance",
            _ => throw new ArgumentOutOfRangeException(nameof(member), member, "Unknown F16 fixture member."),
        };

    private static void AssertLinkedMemberSet(
        PublishedJourneyWorkspace workspace,
        params string[] expectedMembers)
    {
        var actual = Directory.EnumerateFileSystemEntries(workspace.Combine(DestinationRoot))
            .Select(path => System.IO.Path.GetFileName(path))
            .Where(name => !string.Equals(name, "_team.md", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = expectedMembers
            .Select(member => $"{member}.md")
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected, actual);
    }

    private static string LinkTarget(PublishedJourneyWorkspace workspace, string member)
    {
        var linkPath = workspace.Combine($"{DestinationRoot}/{member}.md");
        var info = new FileInfo(linkPath);
        var attributes = File.GetAttributes(linkPath);
        Assert.True((attributes & FileAttributes.ReparsePoint) != 0);
        Assert.True((attributes & FileAttributes.Directory) == 0);
        return info.LinkTarget
            ?? throw new Xunit.Sdk.XunitException($"Expected a raw relative link at {linkPath}.");
    }

    private static void AssertNoEntry(PublishedJourneyWorkspace workspace, string member)
    {
        var path = workspace.Combine($"{DestinationRoot}/{member}.md");
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(path));
        Assert.Null(new FileInfo(path).LinkTarget);
    }

    private static void AssertRelativeFileLink(PublishedJourneyWorkspace workspace, string member)
    {
        Assert.Equal(
            System.IO.Path.GetRelativePath(
                workspace.Combine(DestinationRoot),
                workspace.Combine($"{SourceRoot}/{member}.md")).Replace('\\', '/'),
            LinkTarget(workspace, member));
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
}
