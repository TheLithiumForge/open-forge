using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Shared.Acquisition;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Reading;

internal sealed class SourceDocumentReader
{
    private readonly PhysicalPathResolver _physicalPathResolver;
    internal SourceFileAcquisition Acquisition { get; } = new();
    internal MarkdownDocumentCache Documents => Acquisition.Documents;

    internal MarkdownDocumentFacts ParseDocument(string source)
        => Acquisition.ParseDocument(source);

    internal SourceDocumentReader(CliWorkspace workspace, PhysicalPathResolver? physicalPaths = null)
    {
        Workspace = workspace;
        _physicalPathResolver = physicalPaths ?? new PhysicalPathResolver().CreateReadSession();
    }

    internal CliWorkspace Workspace { get; }

    internal PhysicalPathResolver PhysicalPaths => _physicalPathResolver;

    internal SourceLayerVerification Verify(
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var verification = cancellationToken.IsCancellationRequested
            ? new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Cancelled,
                null,
                null)
            : FormVerification(layer, cancellationToken);
        return verification;
    }

    internal async ValueTask<SourceDocumentReadResult> ReadAsync(
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var verification = Verify(layer, cancellationToken);
        if (verification.State == SourceLayerVerificationState.Missing)
        {
            return new SourceDocumentReadResult(
                layer,
                verification,
                FileReadResult<string>.Missing(layer.CanonicalPath));
        }

        if (verification.State != SourceLayerVerificationState.Verified)
        {
            return new SourceDocumentReadResult(layer, verification, null);
        }

        var physicalPath = verification.CurrentPhysicalPath
            ?? throw new InvalidOperationException("A verified source layer requires its current physical path.");
        var rawRead = await Acquisition.ReadAsync(physicalPath, layer.CanonicalPath, cancellationToken).ConfigureAwait(false);
        rawRead = NormalizeRead(rawRead, layer.CanonicalPath);

        return new SourceDocumentReadResult(
            layer,
            verification,
            RebindRead(rawRead, layer.CanonicalPath));
    }

    private SourceLayerVerification FormVerification(
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var resolution = _physicalPathResolver.ResolveCandidate(
            Workspace.LexicalRoot,
            Workspace.PhysicalRoot,
            Path.Combine(Workspace.LexicalRoot, layer.CanonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        if (cancellationToken.IsCancellationRequested)
        {
            return new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Cancelled,
                null,
                null);
        }

        return resolution.State switch
        {
            PhysicalPathState.Missing => new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Missing,
                null,
                null),
            PhysicalPathState.Contained => FormContainedVerification(layer, resolution),
            PhysicalPathState.External
                or PhysicalPathState.Dangling
                or PhysicalPathState.Cycle => new SourceLayerVerification(
                    layer,
                    SourceLayerVerificationState.Unsafe,
                    null,
                    null),
            PhysicalPathState.Inaccessible
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                or PhysicalPathState.InputOutputFailure => new SourceLayerVerification(
                    layer,
                    SourceLayerVerificationState.Unavailable,
                    null,
                    resolution.Failure ?? new FilesystemFailure(
                        FilesystemFailureKind.InputOutput,
                        "Physical source-layer resolution was unavailable.")),
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.State, "The physical path state is not defined."),
        };
    }

    private static SourceLayerVerification FormContainedVerification(
        SourceLayer layer,
        PhysicalPathResolution resolution)
    {
        var currentPhysicalPath = resolution.GetContainedPhysicalPath();
        var state = PhysicalIdentityTracker.PathComparer.Equals(
            currentPhysicalPath,
            layer.PhysicalPath)
            ? SourceLayerVerificationState.Verified
            : SourceLayerVerificationState.Changed;
        return new SourceLayerVerification(layer, state, currentPhysicalPath, null);
    }

    private static FileReadResult<string> RebindRead(
        FileReadResult<string> read,
        string logicalPath)
    {
        return read.State switch
        {
            FileReadState.Complete => FileReadResult<string>.Complete(
                logicalPath,
                read.Value
                    ?? throw new InvalidOperationException("A complete file read requires a value.")),
            FileReadState.Missing => FileReadResult<string>.Missing(logicalPath),
            FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax
                or FileReadState.AccessDenied
                or FileReadState.InputOutputFailure => FileReadResult<string>.Failed(
                    read.State,
                    logicalPath,
                    read.Failure
                        ?? throw new InvalidOperationException("A failed file read requires a failure.")),
            FileReadState.Cancelled => FileReadResult<string>.Cancelled(logicalPath),
            _ => throw new ArgumentOutOfRangeException(nameof(read), read.State, "The file read state is not defined."),
        };
    }

    private static FileReadResult<string> NormalizeRead(
        FileReadResult<string> read,
        string logicalPath)
    {
        if (read.State != FileReadState.InputOutputFailure
            || read.Failure is null
            || !IsAccessDeniedSharingFailure(read.Failure.DirectCause))
        {
            return read;
        }

        return FileReadResult<string>.Failed(
            FileReadState.AccessDenied,
            logicalPath,
            new FilesystemFailure(
                FilesystemFailureKind.AccessDenied,
                "Filesystem access was denied."));
    }

    private static bool IsAccessDeniedSharingFailure(string directCause)
    {
        return directCause.Contains("0x80070005", StringComparison.OrdinalIgnoreCase)
            || directCause.Contains("0x80070020", StringComparison.OrdinalIgnoreCase)
            || (OperatingSystem.IsLinux()
                && directCause.Contains("0x0000000B", StringComparison.OrdinalIgnoreCase))
            || (OperatingSystem.IsMacOS()
                && directCause.Contains("0x00000023", StringComparison.OrdinalIgnoreCase));
    }
}
