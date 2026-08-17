using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Markdig;
using Markdig.Syntax;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenForge.Cli.Foundation;

internal static class FoundationProbe
{
    private const string ProbeFileName = "foundation-probe.md";
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder().Build();

    internal static Task<FoundationResult> RunAsync(FoundationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.WorkspacePath)
            || !Path.IsPathFullyQualified(request.WorkspacePath)
            || request.Markdown is null
            || request.Yaml is null)
        {
            return Task.FromResult<FoundationResult>(RejectInvalidInput());
        }

        FoundationYamlDocument yamlDocument;
        try
        {
            var yamlContext = new FoundationYamlContext();
            var deserializer = new StaticDeserializerBuilder(yamlContext)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            yamlDocument = deserializer.Deserialize<FoundationYamlDocument>(request.Yaml);
        }
        catch (YamlException)
        {
            return Task.FromResult<FoundationResult>(RejectInvalidInput());
        }

        if (yamlDocument is null || string.IsNullOrWhiteSpace(yamlDocument.Name))
        {
            return Task.FromResult<FoundationResult>(RejectInvalidInput());
        }

        var markdownDocument = Markdown.Parse(request.Markdown, MarkdownPipeline);
        var headingCount = markdownDocument.Descendants<HeadingBlock>().Count();

        if (!Directory.Exists(request.WorkspacePath))
        {
            return Task.FromResult<FoundationResult>(RejectFilesystem());
        }

        return RunFilesystemProbeAsync(request.WorkspacePath, request.Markdown, yamlDocument.Name, headingCount, cancellationToken);
    }

    private static async Task<FoundationResult> RunFilesystemProbeAsync(string workspacePath, string markdown, string yamlName, int headingCount, CancellationToken cancellationToken)
    {
        try
        {
            var probePath = Path.Combine(workspacePath, ProbeFileName);
            var expectedBytes = Encoding.UTF8.GetBytes(markdown);
            await File.WriteAllBytesAsync(probePath, expectedBytes, cancellationToken);
            var actualBytes = await File.ReadAllBytesAsync(probePath, cancellationToken);

            if (!actualBytes.AsSpan().SequenceEqual(expectedBytes))
            {
                return RejectFilesystem();
            }

            var exclusiveLockObserved = ObserveExclusiveLock(probePath);
            var snapshot = new FoundationSnapshot(
                SchemaVersion: 1,
                MarkdownHeadingCount: headingCount,
                YamlName: yamlName,
                FileByteCount: actualBytes.Length,
                FileSha256: Convert.ToHexString(SHA256.HashData(actualBytes)).ToLowerInvariant(),
                ExclusiveLockObserved: exclusiveLockObserved);
            var json = JsonSerializer.Serialize(snapshot, FoundationJsonContext.Default.FoundationSnapshot);

            return new FoundationSucceeded(new FoundationReport(snapshot, json));
        }
        catch (IOException)
        {
            return RejectFilesystem();
        }
        catch (UnauthorizedAccessException)
        {
            return RejectFilesystem();
        }
    }

    private static bool ObserveExclusiveLock(string path)
    {
        using var owner = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        try
        {
            using var contender = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private static FoundationRejected RejectInvalidInput()
    {
        return new FoundationRejected(new FoundationFailure(FoundationFailureKind.InvalidInput, "Foundation probe input is invalid."));
    }

    private static FoundationRejected RejectFilesystem()
    {
        return new FoundationRejected(new FoundationFailure(FoundationFailureKind.Filesystem, "Foundation probe filesystem access failed."));
    }
}
