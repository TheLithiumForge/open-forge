using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal static class PublishedRouteMoveSetup
{
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static async Task SeedAsync(
        PublishedExecutableTarget target,
        TemporaryWorkspace temporary)
    {
        using var setupLockStore = PublishedWorkspaceLockStore.Create(
            "e2e-route-move-setup-lock-store");
        _ = setupLockStore.Track(temporary.Path);
        var environment = setupLockStore.EnvironmentVariables;
        await RunAsync(
            target, temporary, environment,
            ["install", "--automatic", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "init", "archive", "--description", "Archive", "--tag=Route", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "create", PublishedRouteMoveWorkspace.SourceId, "--description", "Old guide", "--tag=Guide", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "init", "guidance/topics", "--description", "Topics", "--tag=Topic", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "create", "guidance/topics/child", "--description", "Child", "--tag=Child", "--workspace", temporary.Path]);
        setupLockStore.AssertPersistentZeroByteLock(temporary.Path);
        SeedUserContent(temporary);
    }

    internal static void DeleteArtifacts(TemporaryWorkspace temporary)
    {
        string[] routePaths =
        [
            PublishedRouteMoveWorkspace.SourcePath,
            PublishedRouteMoveWorkspace.SourceOverwritePath,
            PublishedRouteMoveWorkspace.DestinationPath,
            PublishedRouteMoveWorkspace.DestinationOverwritePath,
            PublishedRouteMoveWorkspace.CategoryPath,
            ".agents/guidance/topics/child.md",
            ".agents/guidance/topics/notes.md",
            ".agents/guidance/topics/image.bin",
            ".agents/archive/_archive.md",
            "invalid.md",
            "README.md",
            "definitions.md",
            ".agents/open-forge.lifecycle.json",
            "AGENTS.md",
            "CLAUDE.md",
        ];
        foreach (var path in PublishedInstallWorkspace.EmbeddedPayloadPaths
                     .Concat(routePaths)
                     .Distinct(StringComparer.Ordinal))
        {
            DeleteOrdinaryFileIfPresent(temporary.Combine(path));
        }

        foreach (var directory in PublishedInstallWorkspace.EmbeddedPayloadPaths
                     .Select(path => Path.GetDirectoryName(temporary.Combine(path)))
                     .Append(temporary.Combine(".agents/guidance/topics"))
                     .Append(temporary.Combine(".agents/archive"))
                     .Append(temporary.Combine(".agents"))
                     .OfType<string>()
                     .Distinct(StringComparer.Ordinal)
                     .OrderByDescending(path => path.Length))
        {
            DeleteEmptyOrdinaryDirectoryIfPresent(directory);
        }
    }

    private static async Task RunAsync(
        PublishedExecutableTarget target,
        TemporaryWorkspace temporary,
        IReadOnlyDictionary<string, string> environment,
        string[] arguments)
    {
        var result = await PublishedProcessTestSupport.RunAsync(
            target,
            temporary.Path,
            arguments,
            environment);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static void SeedUserContent(TemporaryWorkspace temporary)
    {
        File.WriteAllText(
            temporary.Combine(PublishedRouteMoveWorkspace.SourcePath),
            PublishedRouteMoveWorkspace.SourceText,
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(PublishedRouteMoveWorkspace.SourceOverwritePath),
            PublishedRouteMoveWorkspace.OverwriteText,
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(".agents/guidance/topics/child.md"),
            OpenForgeDocumentSeed.Metadata(
                "Child",
                ["Child"],
                "# Child\n\n[Old guide](../old%20guide.md#section).\n\n[Notes](notes.md#detail).\n"),
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(".agents/guidance/topics/notes.md"),
            OpenForgeDocumentSeed.Metadata(
                "Notes",
                ["Note"],
                "# Notes\n\nInternal category target.\n"),
            StrictUtf8NoBom);
        File.WriteAllBytes(
            temporary.Combine(".agents/guidance/topics/image.bin"),
            [0x00, 0x01, 0xfe, 0xff]);
        File.WriteAllText(
            temporary.Combine("README.md"),
            "# Outside\n\n[Old guide](.agents/guidance/old%20guide.md#section)\n",
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine("definitions.md"),
            "[Guide][target]\n\n[target]: .agents/guidance/old%20guide.md#caf%C3%A9\n",
            StrictUtf8NoBom);
    }

    private static void DeleteOrdinaryFileIfPresent(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException(
                "The published Route Move cleanup target is not an ordinary file.");
        }

        File.Delete(path);
    }

    private static void DeleteEmptyOrdinaryDirectoryIfPresent(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException(
                "The published Route Move cleanup target is not an ordinary directory.");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
