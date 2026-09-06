using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedUpdateWorkspace : IDisposable
{
    private const string HistoricalSourcePath = "historical/retired-framework.md";
    private const string PreviousInventoryFingerprint =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HistoricalTargetContents = """
        ---
        open-forge:
          description: Historical Framework guidance retained for Update evidence
          tags: [Guidance, Historical]
        ---

        # Historical Framework Guidance
        """;

    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions LifecycleJsonOptions = new()
    {
        WriteIndented = true,
    };

    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string ManagedPath = ".agents/loader.md";
    internal const string HistoricalTargetPath = ".agents/guidance/retired-framework.md";

    private readonly PublishedInstallWorkspace _installWorkspace;
    private bool _disposed;

    private PublishedUpdateWorkspace(PublishedInstallWorkspace installWorkspace)
    {
        _installWorkspace = installWorkspace;
    }

    internal string Path => _installWorkspace.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment
        => _installWorkspace.ProcessEnvironment;

    internal IReadOnlyDictionary<string, string> SnapshotState()
        => _installWorkspace.SnapshotState();

    internal static PublishedUpdateWorkspace Create()
        => new(PublishedInstallWorkspace.Create());

    internal Task<ProcessRunResult> EstablishTrustedFrameworkAsync(
        PublishedExecutableTarget target)
        => PublishedProcessTestSupport.RunAsync(
            target,
            Path,
            ["install", "--automatic"],
            ProcessEnvironment);

    internal void MutateManagedContent()
    {
        const string installedToken = "Read this after `AGENTS.md` to enter the workspace.";
        const string divergentToken = "Read this after `AGENTS.md` to enter one workspace.";
        var installed = ReadText(ManagedPath);
        var occurrence = installed.IndexOf(installedToken, StringComparison.Ordinal);
        if (occurrence < 0
            || installed.IndexOf(
                installedToken,
                occurrence + installedToken.Length,
                StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                "The published Update loader requires exactly one authored divergence token.");
        }

        _installWorkspace.ReplaceInstalledText(
            ManagedPath,
            installed.Replace(installedToken, divergentToken, StringComparison.Ordinal));
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

        var lifecycle = JsonNode.Parse(ReadText(LifecyclePath))?.AsObject()
            ?? throw new InvalidOperationException("The published Update lifecycle must be a JSON object.");
        var framework = lifecycle["framework"]?.AsObject()
            ?? throw new InvalidOperationException("The published Update lifecycle must contain Framework state.");
        var targets = framework["targets"]?.AsArray()
            ?? throw new InvalidOperationException("The published Update lifecycle must contain Framework targets.");
        var bytes = StrictUtf8NoBom.GetBytes(HistoricalTargetContents);
        targets.Add((JsonNode)new JsonObject
        {
            ["path"] = HistoricalTargetPath,
            ["sourceAssetPath"] = HistoricalSourcePath,
            ["region"] = null,
            ["baselineFingerprint"] = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            ["fingerprintKind"] = "semantic",
        });
        var ordered = targets
            .Select(node => node?.DeepClone()
                ?? throw new InvalidOperationException("Published Update lifecycle targets cannot be null."))
            .OrderBy(node => ReadRequiredString(node.AsObject(), "path"), StringComparer.Ordinal)
            .ThenBy(node => node["region"]?.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();
        targets.Clear();
        foreach (var target in ordered)
        {
            targets.Add(target);
        }

        var source = framework["source"]?.AsObject()
            ?? throw new InvalidOperationException("The published Update lifecycle must contain Framework source state.");
        source["inventoryFingerprint"] = PreviousInventoryFingerprint;
        _installWorkspace.ReplaceInstalledText(
            LifecyclePath,
            lifecycle.ToJsonString(LifecycleJsonOptions) + "\n");
    }

    internal bool HistoricalTargetExists()
        => File.Exists(_installWorkspace.Combine(HistoricalTargetPath));

    internal string ReadText(string relativePath)
        => File.ReadAllText(_installWorkspace.Combine(relativePath), Encoding.UTF8);

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

    private static string ReadRequiredString(JsonObject value, string propertyName)
        => value[propertyName]?.GetValue<string>()
            ?? throw new InvalidOperationException($"The published Update lifecycle requires '{propertyName}'.");

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
            throw new InvalidOperationException("The published Update historical target is not an ordinary file.");
        }

        File.Delete(path);
    }
}
