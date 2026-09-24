using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

using OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

internal sealed class UpdateIntegrationWorkspace : IDisposable
{
    private const string PreviousAuthoredContent = "Earlier authored fixture content.";

    private static readonly FrameworkContentIdentity ContentIdentity = new();
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions OwnershipJsonOptions = new()
    {
        WriteIndented = true,
    };

    internal const string OwnershipPath = ".agents/open-forge.lock.json";
    internal const string ManagedPath = ".agents/loader.md";
    internal const string GeneratedPath = ".agents/memory/_memory.md";
    internal const string RetiredCandidatePath = ".agents/guidance/_guidance.md";
    internal const string HistoricalTargetPath = ".agents/guidance/retired-framework.md";

    private const string HistoricalTargetContents = """
        ---
        open-forge:
          description: Historical Framework guidance retained for Update evidence
          tags: [Guidance, Historical]
        ---

        # Historical Framework Guidance
        """;

    private readonly InstallOperationWorkspace _installWorkspace;
    private readonly List<string> _testCleanupPaths = [];
    private bool _disposed;

    private UpdateIntegrationWorkspace(InstallOperationWorkspace installWorkspace)
    {
        _installWorkspace = installWorkspace;
        Workspace = installWorkspace.Workspace;
    }

    internal CliWorkspace Workspace { get; }

    internal WorkspaceLockStoreRoot LockStoreRoot => _installWorkspace.LockStoreRoot;

    internal string PhysicalPath => _installWorkspace.PhysicalPath;

    internal static UpdateIntegrationWorkspace Create(string purpose)
        => new(InstallOperationWorkspace.Create(purpose));

    internal async Task EstablishTrustedFrameworkAsync(CancellationToken cancellationToken)
    {
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                LockStoreRoot)
            .ExecuteAsync(
                new InstallRequest(
                    Workspace,
                    InstallMode.Apply,
                    force: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                cancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
    }

    internal UpdateRequest Request(
        UpdateMode mode = UpdateMode.Apply,
        bool force = false,
        bool prune = false,
        bool automatic = true,
        bool allowsInteractiveConfirmation = false)
        => new(
            Workspace,
            mode,
            force,
            prune,
            automatic,
            allowsInteractiveConfirmation);

    internal ValueTask<UpdatePlanBuild> BuildAsync(UpdateRequest request)
        => UpdatePlanBuilder.Create().BuildAsync(
            request,
            TestContext.Current.CancellationToken);

    internal async ValueTask<UpdateResult> ExecuteAsync(
        UpdateRequest request,
        bool canPrompt = false,
        string input = "")
    {
        return await UpdateOperationFactory.Create(
                UpdateInteractionTestSupport.ScriptedConfirmation(input, canPrompt),
                LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    internal async ValueTask<UpdateResult> ExecutePromptedAsync(
        UpdateRequest request,
        Action mutation)
    {
        return await UpdateOperationFactory.Create(
                UpdateInteractionTestSupport.Confirmation(
                    observe: (_, _) => mutation()),
                LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    internal async Task<CliProcessCompletion> RunRootAsync(IReadOnlyList<string> arguments)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "update-integration"),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = LockStoreRoot,
            });
        return await application.RunAsync(
            arguments.ToArray(),
            new CliProcessEnvironment(Workspace.LexicalRoot),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _installWorkspace.SnapshotHashes();

    internal string ReadText(string relativePath)
        => File.ReadAllText(_installWorkspace.Combine(relativePath), Encoding.UTF8);

    internal byte[] ReadBytes(string relativePath)
        => File.ReadAllBytes(_installWorkspace.Combine(relativePath));

    internal void WriteText(string relativePath, string contents)
        => _installWorkspace.WriteText(relativePath, contents);

    internal void CreateDirectory(string relativePath)
        => _installWorkspace.CreateDirectory(relativePath);

    internal void ReplaceText(string relativePath, string contents)
        => _installWorkspace.ReplaceInstalledText(relativePath, contents);

    internal void RemoveFile(string relativePath)
        => File.Delete(_installWorkspace.Combine(relativePath));

    internal bool Exists(string relativePath)
        => _installWorkspace.Exists(relativePath);

    internal void RegisterTestCleanupPath(string relativePath)
        => _testCleanupPaths.Add(_installWorkspace.Combine(relativePath));

    internal bool RecoveryDirectoryExists()
        => _installWorkspace.RecoveryDirectoryExists();

    internal void MutateManagedContent()
        => ReplaceUniqueManagedText(
            "It defines how to select context, follow applicable rules, and maintain the workspace.",
            "It defines how to select context, follow applicable rules, and maintain one workspace.");

    internal string MutateManagedContentAgain()
        => ReplaceUniqueManagedText(
            "# Open Forge Loader",
            "# Open Forge Router");

    internal void RemoveManagedContent()
        => RemoveFile(ManagedPath);

    internal void RemoveRetiredCandidate()
        => RemoveFile(RetiredCandidatePath);

    internal void SeedPreviousInventoryIdentity()
    {
        var lifecycle = ReadOwnership();
        SetPreviousInventory(lifecycle);
        WriteOwnership(lifecycle);
    }

    internal (string Id, string? Version) ReadOwnershipSourceIdentity()
    {
        var source = ReadFramework(ReadOwnership())["source"]?.AsObject()
            ?? throw new InvalidOperationException(
                "The installed lifecycle fixture must contain Framework source state.");
        return (
            ReadRequiredString(source, "id"),
            source["version"]?.GetValue<string>());
    }

    internal void SeedSafePreviousSourceVersion()
    {
        var current = ReadText(RetiredCandidatePath);
        var previous = $"{current}\n{PreviousAuthoredContent}\n";

        ReplaceText(RetiredCandidatePath, previous);
    }

    internal void SeedGenuinelyNewSourceTarget()
    {
        RemoveRetiredCandidate();
        var lifecycle = ReadOwnership();
        var paths = ReadFramework(lifecycle)["paths"]!.AsArray();
        var target = paths.Single(path => path!.GetValue<string>() == RetiredCandidatePath);
        paths.Remove(target);
        WriteOwnership(lifecycle);
    }

    internal byte[] SeedCoalescedAuthoredAndGeneratedChange()
    {
        var current = ReadText(GeneratedPath);
        var document = new MarkdownDocumentParser().Parse(current);
        if (document.GeneratedRegion.ContentSpan is not { } content || document.BodySpan is not { } body)
        {
            throw new InvalidOperationException("The coalesced-change fixture requires a generated Entries region.");
        }

        var emptyEntries = $"\n{MarkdownEntriesSectionReader.EmptyEntry}\n";
        if (string.Equals(current[content.Start..content.End], emptyEntries, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The coalesced-change fixture requires populated Entries.");
        }

        var previous = $"{current[..body.Start]}\n{PreviousAuthoredContent}\n\n{current[body.Start..content.Start]}{emptyEntries}{current[content.End..]}";
        ReplaceText(GeneratedPath, previous);
        return Encoding.UTF8.GetBytes(current);
    }

    internal void SeedHistoricalRetiredTarget()
    {
        var targetPath = _installWorkspace.Combine(HistoricalTargetPath);
        using (var stream = new FileStream(
                   targetPath,
                   FileMode.CreateNew,
                   FileAccess.Write,
                   FileShare.None))
        using (var writer = new StreamWriter(stream, StrictUtf8NoBom))
        {
            writer.Write(HistoricalTargetContents);
        }

        var lifecycle = ReadOwnership();
        ReadFramework(lifecycle)["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create(HistoricalTargetPath));
        WriteOwnership(lifecycle);
    }

    internal void DivergeHistoricalRetiredTarget()
        => ReplaceText(HistoricalTargetPath, "# Divergent historical Framework guidance\n");

    internal void BreakGeneratedBoundary()
    {
        var current = ReadText(GeneratedPath);
        ReplaceText(
            GeneratedPath,
            current.Replace(
                "## Entries",
                "## Entries\n\n## Entries",
                StringComparison.Ordinal));
    }

    internal void RemoveGeneratedBoundary()
    {
        var current = ReadText(GeneratedPath);
        const string heading = "## Entries";
        var start = current.IndexOf(heading, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("The generated-boundary fixture requires its Entries heading.");
        }

        ReplaceText(GeneratedPath, current.Remove(start));
    }

    internal void CorruptGeneratedUtf8()
    {
        var bytes = ReadBytes(GeneratedPath);
        var marker = Encoding.UTF8.GetBytes("## Entries");
        var markerOffset = bytes.AsSpan().IndexOf(marker);
        if (markerOffset < 0)
        {
            throw new InvalidOperationException(
                "The invalid-UTF8 fixture requires one generated Entries heading.");
        }

        var bodyOffset = markerOffset + marker.Length;
        while (bodyOffset < bytes.Length && bytes[bodyOffset] is (byte)'\r' or (byte)'\n')
        {
            bodyOffset++;
        }
        if (bodyOffset >= bytes.Length)
        {
            throw new InvalidOperationException(
                "The invalid-UTF8 fixture requires generated body bytes.");
        }

        bytes[bodyOffset] = 0xff;
        File.WriteAllBytes(_installWorkspace.Combine(GeneratedPath), bytes);
    }

    internal FileStream HoldExternalLock()
        => _installWorkspace.HoldExternalLock();

    internal void SeedInvalidSourceProvenance()
    {
        var lifecycle = ReadOwnership();
        ReadFramework(lifecycle)["regions"]!.AsArray().Add((JsonNode)new JsonObject
        { ["path"] = GeneratedPath, ["region"] = "authored" });
        WriteOwnership(lifecycle);
    }

    internal void SeedExplicitManagedHostRegion(string path)
    {
        var lifecycle = ReadOwnership();
        Assert.Contains(ReadFramework(lifecycle)["regions"]!.AsArray(),
            region => region!["path"]!.GetValue<string>() == path
                && region["region"]!.GetValue<string>() == "open-forge");
        WriteOwnership(lifecycle);
    }

    internal void SeedMalformedLifecycle()
        => ReplaceText(OwnershipPath, "{\"framework\": {\"coverage\": \"broken\"}}\n");

    private JsonObject ReadOwnership()
        => JsonNode.Parse(ReadText(OwnershipPath))?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must be a JSON object.");

    private void WriteOwnership(JsonObject lifecycle)
        => ReplaceText(
            OwnershipPath,
            lifecycle.ToJsonString(OwnershipJsonOptions));

    private static JsonObject ReadFramework(JsonObject lifecycle)
        => lifecycle["framework"]?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must contain Framework state.");

    private static void SetPreviousInventory(JsonObject lifecycle)
    {
        var source = ReadFramework(lifecycle)["source"]?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must contain Framework source state.");
        source["version"] = "previous-release";
    }

    private static string ReadRequiredString(JsonObject value, string propertyName)
        => value[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"The lifecycle fixture requires '{propertyName}'.");

    private string ReplaceUniqueManagedText(string before, string after)
    {
        var current = ReadText(ManagedPath);
        var first = current.IndexOf(before, StringComparison.Ordinal);
        if (first < 0
            || current.IndexOf(before, first + before.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                "The managed-divergence fixture requires one exact authored token.");
        }

        var updated = current.Replace(before, after, StringComparison.Ordinal);
        ReplaceText(ManagedPath, updated);
        return updated;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _testCleanupPaths)
        {
            RemoveTestEntry(path);
        }

        DeleteHistoricalTargetIfPresent();
        _installWorkspace.Dispose();
        _disposed = true;
    }

    private void DeleteHistoricalTargetIfPresent()
    {
        var path = _installWorkspace.Combine(HistoricalTargetPath);
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The historical Update target is not an ordinary file.");
        }

        File.Delete(path);
    }

    private static void RemoveTestEntry(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            if ((attributes & FileAttributes.Directory) != 0)
            {
                Directory.Delete(path, recursive: false);
            }
            else
            {
                File.Delete(path);
            }

            return;
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path).ToArray())
            {
                RemoveTestEntry(child);
            }

            Directory.Delete(path, recursive: false);
            return;
        }

        File.Delete(path);
    }
}
