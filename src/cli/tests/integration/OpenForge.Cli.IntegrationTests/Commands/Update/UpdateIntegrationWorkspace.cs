using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

internal sealed class UpdateIntegrationWorkspace : IDisposable
{
    private const string PreviousInventoryFingerprint =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private static readonly FrameworkContentIdentity ContentIdentity = new();
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions LifecycleJsonOptions = new()
    {
        WriteIndented = true,
    };

    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string ManagedPath = ".agents/loader.md";
    internal const string GeneratedPath = ".agents/memory/_memory.md";
    internal const string RetiredCandidatePath = ".agents/guidance/adaptive-collaboration.md";
    internal const string HistoricalTargetPath = ".agents/guidance/retired-framework.md";

    private const string HistoricalSourcePath = "historical/retired-framework.md";
    private const string HistoricalTargetContents = """
        ---
        open-forge:
          description: Historical Framework guidance retained for Update evidence
          tags: [Guidance, Historical]
        ---

        # Historical Framework Guidance
        """;

    private readonly InstallOperationWorkspace _installWorkspace;
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
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var result = await InstallOperationFactory.Create(
                new CliInteractiveSession(standardInput, promptOutput, canPrompt: false),
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
        using var standardInput = new StringReader(input);
        using var promptOutput = new StringWriter();
        return await UpdateOperationFactory.Create(
                new CliInteractiveSession(standardInput, promptOutput, canPrompt),
                LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    internal async ValueTask<UpdateResult> ExecutePromptedAsync(
        UpdateRequest request,
        Action mutation)
    {
        using var standardInput = new MutatingConfirmationReader(mutation);
        using var promptOutput = new StringWriter();
        return await UpdateOperationFactory.Create(
                new CliInteractiveSession(standardInput, promptOutput, canPrompt: true),
                LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _installWorkspace.SnapshotHashes();

    internal string ReadText(string relativePath)
        => File.ReadAllText(_installWorkspace.Combine(relativePath), Encoding.UTF8);

    internal byte[] ReadBytes(string relativePath)
        => File.ReadAllBytes(_installWorkspace.Combine(relativePath));

    internal void ReplaceText(string relativePath, string contents)
        => _installWorkspace.ReplaceInstalledText(relativePath, contents);

    internal void RemoveFile(string relativePath)
        => File.Delete(_installWorkspace.Combine(relativePath));

    internal bool Exists(string relativePath)
        => _installWorkspace.Exists(relativePath);

    internal bool RecoveryDirectoryExists()
        => _installWorkspace.RecoveryDirectoryExists();

    internal void MutateManagedContent()
        => ReplaceUniqueManagedText(
            "Read this after `AGENTS.md` to enter the workspace.",
            "Read this after `AGENTS.md` to enter one workspace.");

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
        var lifecycle = ReadLifecycle();
        SetPreviousInventory(lifecycle);
        WriteLifecycle(lifecycle);
    }

    internal (string Id, string? Version, string InventoryFingerprint) ReadLifecycleSourceIdentity()
    {
        var source = ReadFramework(ReadLifecycle())["source"]?.AsObject()
            ?? throw new InvalidOperationException(
                "The installed lifecycle fixture must contain Framework source state.");
        return (
            ReadRequiredString(source, "id"),
            source["version"]?.GetValue<string>(),
            ReadRequiredString(source, "inventoryFingerprint"));
    }

    internal void SeedSafePreviousSourceVersion()
    {
        var current = ReadText(RetiredCandidatePath);
        var previous = current.Replace(
            "or finish broad work.",
            "or finish focused work.",
            StringComparison.Ordinal);
        if (string.Equals(current, previous, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The previous source fixture did not change the intended target.");
        }

        ReplaceText(RetiredCandidatePath, previous);
        var lifecycle = ReadLifecycle();
        var target = ReadSourceTarget(lifecycle, RetiredCandidatePath);
        SetSourceBaseline(target, Encoding.UTF8.GetBytes(previous));
        SetPreviousInventory(lifecycle);
        WriteLifecycle(lifecycle);
    }

    internal void SeedGenuinelyNewSourceTarget()
    {
        RemoveRetiredCandidate();
        var lifecycle = ReadLifecycle();
        var targets = ReadTargets(lifecycle);
        var matches = targets
            .Select((node, index) => (Node: node?.AsObject(), Index: index))
            .Where(value => string.Equals(
                value.Node?["sourceAssetPath"]?.GetValue<string>(),
                RetiredCandidatePath,
                StringComparison.Ordinal))
            .ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidOperationException("The new-target fixture requires one installed lifecycle target.");
        }

        targets.RemoveAt(matches[0].Index);
        SetPreviousInventory(lifecycle);
        WriteLifecycle(lifecycle);
    }

    internal byte[] SeedCoalescedAuthoredAndGeneratedChange()
    {
        var current = ReadText(GeneratedPath);
        var previous = current
            .Replace(
                "Memory is self-growing Markdown state",
                "Memory was previously maintained Markdown state",
                StringComparison.Ordinal)
            .Replace(
                "- [Useful history that no longer controls current work](archived/_archived.md) - #Memory #Archived #Contextual #Historical\n",
                string.Empty,
                StringComparison.Ordinal);
        if (string.Equals(current, previous, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The coalesced-change fixture did not change the intended target.");
        }

        ReplaceText(GeneratedPath, previous);
        var lifecycle = ReadLifecycle();
        SetSourceBaseline(
            ReadSourceTarget(lifecycle, GeneratedPath),
            Encoding.UTF8.GetBytes(previous));
        var generatedTarget = ReadGeneratedTarget(lifecycle, GeneratedPath);
        generatedTarget["baselineFingerprint"] = ContentIdentity.ReadGeneratedEntriesFingerprint(
            Encoding.UTF8.GetBytes(previous),
            ReadRequiredString(generatedTarget, "fingerprintKind"));
        SetPreviousInventory(lifecycle);
        WriteLifecycle(lifecycle);
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

        var lifecycle = ReadLifecycle();
        var bytes = Encoding.UTF8.GetBytes(HistoricalTargetContents);
        var identity = ContentIdentity.ReadSourceFingerprint(bytes);
        if (!identity.IsSemantic || identity.Sha256 is null)
        {
            throw new InvalidOperationException("The historical target fixture must have semantic identity.");
        }

        var targets = ReadTargets(lifecycle);
        targets.Add((JsonNode)new JsonObject
        {
            ["path"] = HistoricalTargetPath,
            ["sourceAssetPath"] = HistoricalSourcePath,
            ["region"] = null,
            ["baselineFingerprint"] = identity.Sha256,
            ["fingerprintKind"] = LifecycleSchema.SemanticFingerprintKind,
        });
        SortTargets(targets);
        SetPreviousInventory(lifecycle);
        WriteLifecycle(lifecycle);
    }

    internal void DivergeHistoricalRetiredTarget()
        => ReplaceText(HistoricalTargetPath, "# Divergent historical Framework guidance\n");

    internal void BreakGeneratedBoundary()
    {
        var current = ReadText(GeneratedPath);
        ReplaceText(
            GeneratedPath,
            current.Replace(
                "<!-- open-forge:generated-index:start -->",
                "<!-- open-forge:generated-index:broken -->",
                StringComparison.Ordinal));
    }

    internal void RemoveGeneratedBoundary()
    {
        const string startMarker = "<!-- open-forge:generated-index:start -->";
        const string endMarker = "<!-- open-forge:generated-index:end -->";
        var current = ReadText(GeneratedPath);
        var start = current.IndexOf(startMarker, StringComparison.Ordinal);
        var end = current.IndexOf(endMarker, StringComparison.Ordinal);
        if (start < 0
            || end < start
            || current.IndexOf(startMarker, start + startMarker.Length, StringComparison.Ordinal) >= 0
            || current.IndexOf(endMarker, end + endMarker.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                "The generated-boundary fixture requires one complete marker pair.");
        }

        var endExclusive = end + endMarker.Length;
        if (endExclusive < current.Length && current[endExclusive] == '\r')
        {
            endExclusive++;
        }
        if (endExclusive < current.Length && current[endExclusive] == '\n')
        {
            endExclusive++;
        }

        ReplaceText(GeneratedPath, current.Remove(start, endExclusive - start));
    }

    internal void CorruptGeneratedUtf8()
    {
        var bytes = ReadBytes(GeneratedPath);
        var marker = Encoding.UTF8.GetBytes("<!-- open-forge:generated-index:start -->");
        var markerOffset = bytes.AsSpan().IndexOf(marker);
        if (markerOffset < 0)
        {
            throw new InvalidOperationException(
                "The invalid-UTF8 fixture requires one generated start marker.");
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
        var lifecycle = ReadLifecycle();
        ReadSourceTarget(lifecycle, GeneratedPath)["region"] = "authored";
        SortTargets(ReadTargets(lifecycle));
        WriteLifecycle(lifecycle);
    }

    internal void SeedMalformedLifecycle()
        => ReplaceText(LifecyclePath, "{\"framework\": {\"coverage\": \"broken\"}}\n");

    private JsonObject ReadLifecycle()
        => JsonNode.Parse(ReadText(LifecyclePath))?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must be a JSON object.");

    private void WriteLifecycle(JsonObject lifecycle)
        => ReplaceText(
            LifecyclePath,
            lifecycle.ToJsonString(LifecycleJsonOptions) + "\n");

    private static JsonArray ReadTargets(JsonObject lifecycle)
        => ReadFramework(lifecycle)["targets"]?.AsArray()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must contain Framework targets.");

    private static JsonObject ReadSourceTarget(JsonObject lifecycle, string sourceAssetPath)
    {
        var matches = ReadTargets(lifecycle)
            .Select(node => node?.AsObject())
            .Where(target => string.Equals(
                target?["sourceAssetPath"]?.GetValue<string>(),
                sourceAssetPath,
                StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1 && matches[0] is { } target
            ? target
            : throw new InvalidOperationException("The source fixture requires one installed lifecycle target.");
    }

    private static JsonObject ReadGeneratedTarget(JsonObject lifecycle, string path)
    {
        var matches = ReadTargets(lifecycle)
            .Select(node => node?.AsObject())
            .Where(target => string.Equals(
                    target?["path"]?.GetValue<string>(),
                    path,
                    StringComparison.Ordinal)
                && string.Equals(
                    target?["region"]?.GetValue<string>(),
                    LifecycleSchema.GeneratedEntriesRegion,
                    StringComparison.Ordinal)
                && target?["sourceAssetPath"] is null)
            .ToArray();
        return matches.Length == 1 && matches[0] is { } target
            ? target
            : throw new InvalidOperationException("The generated fixture requires one installed lifecycle target.");
    }

    private static JsonObject ReadFramework(JsonObject lifecycle)
        => lifecycle["framework"]?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must contain Framework state.");

    private static void SetSourceBaseline(JsonObject target, byte[] bytes)
    {
        target["baselineFingerprint"] = ContentIdentity.ReadSourceFingerprint(
            bytes,
            ReadRequiredString(target, "fingerprintKind"));
    }

    private static void SetPreviousInventory(JsonObject lifecycle)
    {
        var source = ReadFramework(lifecycle)["source"]?.AsObject()
            ?? throw new InvalidOperationException("The installed lifecycle fixture must contain Framework source state.");
        source["inventoryFingerprint"] = PreviousInventoryFingerprint;
    }

    private static void SortTargets(JsonArray targets)
    {
        var ordered = targets
            .Select(node => node?.DeepClone()
                ?? throw new InvalidOperationException("Lifecycle targets cannot contain null members."))
            .OrderBy(node => ReadRequiredString(node.AsObject(), "path"), StringComparer.Ordinal)
            .ThenBy(node => node["region"]?.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();
        targets.Clear();
        foreach (var target in ordered)
        {
            targets.Add(target);
        }
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

    private sealed class MutatingConfirmationReader(Action mutation) : TextReader
    {
        private readonly Action _mutation = mutation;
        private bool _read;

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_read)
            {
                return ValueTask.FromResult<string?>(null);
            }

            _read = true;
            _mutation();
            return ValueTask.FromResult<string?>("y");
        }
    }
}
