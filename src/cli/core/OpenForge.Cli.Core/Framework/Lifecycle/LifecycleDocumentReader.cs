using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class LifecycleDocumentReader(PhysicalPathResolver physicalPathResolver)
{
    internal const string RelativePath = ".agents/open-forge.lifecycle.json";
    internal const string FingerprintPolicy = MarkdownFingerprintPolicy.Name;

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<LifecycleReadResult> ReadExtensionsAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        var path = Path.Combine(workspace.LexicalRoot, ".agents", "open-forge.lifecycle.json");
        var resolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            path);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return Missing();
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return Blocked("The lifecycle document physical boundary is unsafe or unavailable.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        byte[] bytes;
        try
        {
            bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (FileNotFoundException)
        {
            return Missing();
        }
        catch (DirectoryNotFoundException)
        {
            return Missing();
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(exception.Message);
        }
        catch (IOException exception)
        {
            return Unavailable(exception.Message);
        }

        try
        {
            _ = StrictUtf8.GetString(bytes);
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(bytes);
            var document = JsonSerializer.Deserialize(
                bytes,
                LifecycleJsonContext.Default.LifecycleDocumentV1)
                ?? throw new JsonException("The lifecycle document cannot be null.");
            return LifecycleDocumentValidator.Validate(workspace, document);
        }
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException)
        {
            return InvalidJson(exception);
        }
    }

    private static LifecycleReadResult InvalidJson(Exception exception)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: $"The lifecycle document is invalid: {exception.Message}",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Missing()
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Missing,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: "The lifecycle document is missing; installed coverage cannot be proven empty.",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Blocked(string cause)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Blocked,
            packages: [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Blocked,
                WorkspaceBinding = LifecycleWorkspaceBinding.Unavailable,
            });

    private static LifecycleReadResult Cancelled()
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Cancelled,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: "Lifecycle inspection was interrupted.",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Interrupted,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Unavailable(string cause)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Unavailable,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });
}
