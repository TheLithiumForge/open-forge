using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Shared.Route;

internal static class PublishedRouteWorkspaceSeed
{
    internal const string SourceId = "guidance/old guide";
    internal const string SourcePath = ".agents/guidance/old guide.md";
    internal const string SourceOverwritePath = ".agents/guidance/old guide.overwrite.md";
    internal const string CategoryId = "guidance/topics";
    internal const string CategoryPath = ".agents/guidance/topics/_topics.md";
    internal const string CategoryChildPath = ".agents/guidance/topics/child.md";
    internal const string CategoryNotesPath = ".agents/guidance/topics/notes.md";
    internal const string CategoryResourcePath = ".agents/guidance/topics/image.bin";
    internal const string SourceText = "---\nopen-forge:\n  description: Old guide\n  tags: [Guide]\n---\n# Old guide\n\n[Archive](../archive/_archive.md#entries)\n";
    internal const string OverwriteText = "[Archive override](../archive/_archive.md).\n";

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
            ["route", "create", SourceId, "--description", "Old guide", "--tag=Guide", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "init", CategoryId, "--description", "Topics", "--tag=Topic", "--workspace", temporary.Path]);
        await RunAsync(
            target, temporary, environment,
            ["route", "create", "guidance/topics/child", "--description", "Child", "--tag=Child", "--workspace", temporary.Path]);
        setupLockStore.AssertPersistentZeroByteLock(temporary.Path);
        SeedUserContent(temporary);
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
            temporary.Combine(SourcePath),
            SourceText,
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(SourceOverwritePath),
            OverwriteText,
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(CategoryChildPath),
            OpenForgeDocumentSeed.Metadata(
                "Child",
                ["Child"],
                "# Child\n\n[Old guide](../old%20guide.md#section).\n\n[Notes](notes.md#detail).\n"),
            StrictUtf8NoBom);
        File.WriteAllText(
            temporary.Combine(CategoryNotesPath),
            OpenForgeDocumentSeed.Metadata(
                "Notes",
                ["Note"],
                "# Notes\n\nInternal category target.\n"),
            StrictUtf8NoBom);
        File.WriteAllBytes(
            temporary.Combine(CategoryResourcePath),
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
}
