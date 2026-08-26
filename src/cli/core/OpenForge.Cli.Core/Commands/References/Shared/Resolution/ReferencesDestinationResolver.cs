using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Operation;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References.Shared.Resolution;

internal sealed record ReferencesDestinationInput
{
    internal ReferencesDestinationInput(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        SourceLogicalSource source,
        SourceLayer layer,
        string rawDestination)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(rawDestination);
        Workspace = workspace;
        Catalogue = catalogue;
        Source = source;
        Layer = layer;
        RawDestination = rawDestination;
    }

    internal CliWorkspace Workspace { get; }

    internal SourceCatalogue Catalogue { get; }

    internal SourceLogicalSource Source { get; }

    internal SourceLayer Layer { get; }

    internal string RawDestination { get; }
}

internal sealed record ReferencesDestinationFinding(
    ReferencesFindingCode Code,
    string Cause,
    IReadOnlyList<ReferencesSourceIdentity> Candidates);

internal sealed record ReferencesDestinationFacts(
    string? Fragment,
    ReferencesTarget Target,
    ReferencesDestinationFinding? Finding);

internal sealed class ReferencesDestinationResolver
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly ReferencesPhysicalPathResolver _physicalPathResolver;
    private readonly ReferencesStrictUtf8Reader _strictUtf8Reader;
    private readonly ReferencesMarkdownParser _markdownParser;

    internal ReferencesDestinationResolver(
        ReferencesPhysicalPathResolver physicalPathResolver,
        ReferencesStrictUtf8Reader strictUtf8Reader,
        ReferencesMarkdownParser markdownParser)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(strictUtf8Reader);
        ArgumentNullException.ThrowIfNull(markdownParser);
        _physicalPathResolver = physicalPathResolver;
        _strictUtf8Reader = strictUtf8Reader;
        _markdownParser = markdownParser;
    }

    internal async ValueTask<ReferencesDestinationFacts> ResolveAsync(
        ReferencesDestinationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();
        var raw = input.RawDestination;
        if (TryReadAbsoluteUri(raw, out var uri))
        {
            if (uri.Scheme is "http" or "https")
            {
                return new ReferencesDestinationFacts(
                    ReadFragment(raw),
                    new ReferencesTarget(
                        ReferencesTargetKind.External,
                        null,
                        null,
                        null,
                        ReferencesTargetResolution.ExternalUnchecked,
                        ReferencesNetworkState.NetworkNotAttempted),
                    null);
            }

            return Unsupported(raw);
        }

        var hash = raw.IndexOf('#');
        var rawPath = hash >= 0 ? raw[..hash] : raw;
        var rawFragment = hash >= 0 ? raw[(hash + 1)..] : null;
        if (raw.Length == 0 || (hash < 0 && rawPath.Length == 0))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Malformed, "The local destination is empty.");
        }

        if (rawPath.Contains('?'))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Query, "A local destination cannot contain an unencoded query.");
        }

        if (rawPath.Contains('\\'))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Malformed, "A local destination cannot use backslash separators.");
        }

        if (IsDriveRooted(rawPath) || rawPath.StartsWith("/", StringComparison.Ordinal))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Absolute, "A local destination cannot be an absolute or rooted path.");
        }

        string decodedPath;
        string? decodedFragment = null;
        try
        {
            decodedPath = DecodePercent(rawPath);
            if (rawFragment is not null)
            {
                decodedFragment = DecodePercent(rawFragment);
            }
        }
        catch (FormatException exception)
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    null,
                    null,
                    null,
                    ReferencesTargetResolution.EncodingUnsupported,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.InvalidEncoding,
                    exception.Message,
                    []));
        }

        if (decodedPath.StartsWith("/", StringComparison.Ordinal)
            || IsDriveRooted(decodedPath))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Absolute, "A local destination cannot be an absolute or rooted path.");
        }

        if (decodedPath.Contains('\\'))
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Malformed, "A local destination cannot use backslash separators.");
        }

        var sourceLexicalPath = SourceLogicalPath.ToLexicalPath(
            input.Workspace.LexicalRoot,
            input.Layer.CanonicalPath);
        var sourceDirectory = Path.GetDirectoryName(sourceLexicalPath);
        if (sourceDirectory is null)
        {
            return Unsafe(rawFragment, ReferencesTargetResolution.PhysicalEscape, "The source layer has no containing directory.");
        }

        string lexicalTarget;
        try
        {
            lexicalTarget = Path.GetFullPath(
                string.IsNullOrEmpty(decodedPath)
                    ? sourceLexicalPath
                    : Path.Combine(sourceDirectory, decodedPath));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Malformed, "The local destination path is malformed.");
        }

        if (!PhysicalContainment.Contains(input.Workspace.LexicalRoot, lexicalTarget))
        {
            return Unsafe(rawFragment, ReferencesTargetResolution.OutsideWorkspace, "The local destination leaves the selected workspace.");
        }

        var canonicalPath = ReadCanonicalPath(input.Workspace.LexicalRoot, lexicalTarget);
        PhysicalPathResolution physical;
        try
        {
            physical = _physicalPathResolver(input.Workspace, lexicalTarget);
        }
        catch (Exception)
        {
            return Unsafe(rawFragment, ReferencesTargetResolution.PhysicalEscape, "The local destination physical boundary could not be established.");
        }

        if (physical.State == PhysicalPathState.Missing)
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    null,
                    canonicalPath,
                    null,
                    ReferencesTargetResolution.Missing,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.TargetMissing,
                    "The local destination path does not exist.",
                    []));
        }

        if (physical.State != PhysicalPathState.Contained)
        {
            var (resolution, code, cause) = physical.State switch
            {
                PhysicalPathState.Invalid => (
                    ReferencesTargetResolution.Malformed,
                    ReferencesFindingCode.DestinationMalformed,
                    "The local destination path is malformed."),
                PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => (
                    ReferencesTargetResolution.Unreadable,
                    ReferencesFindingCode.TargetUnreadable,
                    "The local destination target could not be read."),
                _ => (
                    ReferencesTargetResolution.PhysicalEscape,
                    ReferencesFindingCode.TargetUnsafe,
                    "The local destination physical boundary is unsafe."),
            };
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(ReferencesTargetKind.Local, null, canonicalPath, null, resolution, null),
                new ReferencesDestinationFinding(
                    code,
                    cause,
                    []));
        }

        var physicalTargetPath = physical.GetContainedPhysicalPath();
        var component = LinkTargetReader.Read(physicalTargetPath);
        if (component.State != PathComponentState.Ordinary)
        {
            if (component.State is PathComponentState.Inaccessible or PathComponentState.InputOutputFailure)
            {
                return new ReferencesDestinationFacts(
                    rawFragment,
                    new ReferencesTarget(
                        ReferencesTargetKind.Local,
                        null,
                        canonicalPath,
                        null,
                        ReferencesTargetResolution.Unreadable,
                        null),
                    new ReferencesDestinationFinding(
                        ReferencesFindingCode.TargetUnreadable,
                        "The local destination target could not be read.",
                        []));
            }

            return Unsafe(rawFragment, ReferencesTargetResolution.PhysicalEscape, "The local destination physical target is unavailable or unsafe.");
        }

        if (component.Attributes is not { } attributes)
        {
            return Unsafe(rawFragment, ReferencesTargetResolution.PhysicalEscape, "The local destination attributes could not be established.");
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            return Malformed(rawFragment, ReferencesTargetResolution.Malformed, "A local destination must identify a file, not a directory.");
        }

        var layerMatches = ReadTargetLayers(input.Catalogue, physicalTargetPath);
        if (layerMatches.Count > 1)
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    null,
                    canonicalPath,
                    null,
                    ReferencesTargetResolution.Ambiguous,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.TargetAmbiguous,
                    "More than one logical source or layer has the destination physical identity.",
                    layerMatches.Select(match => ToIdentity(match.Source)).Distinct().ToArray()));
        }

        var canonicalSource = input.Catalogue.FindByPath(canonicalPath);
        var matchedLayer = canonicalSource is null
            ? null
            : ReadLayers(canonicalSource)
                .FirstOrDefault(layer => string.Equals(layer.CanonicalPath, canonicalPath, StringComparison.Ordinal));
        if (canonicalSource is not null
            && matchedLayer is not null
            && !PhysicalIdentityTracker.PathComparer.Equals(matchedLayer.PhysicalPath, physicalTargetPath))
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    null,
                    canonicalPath,
                    null,
                    ReferencesTargetResolution.Ambiguous,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.TargetAmbiguous,
                    "The authored target path and its current physical source identity disagree.",
                    [ToIdentity(canonicalSource)]));
        }

        var identityCandidates = canonicalSource is null
            ? Array.Empty<SourceLogicalSource>()
            : input.Catalogue.FindAllById(canonicalSource.Identity.AutomaticId).ToArray();
        var targetId = identityCandidates.Length > 1
            ? null
            : canonicalSource?.Identity.AutomaticId;
        var target = new ReferencesTarget(
            ReferencesTargetKind.Local,
            targetId,
            canonicalPath,
            matchedLayer?.Kind,
            ReferencesTargetResolution.Complete,
            null);
        if (rawFragment is null)
        {
            return identityCandidates.Length > 1
                ? IdentityCollision(target, identityCandidates)
                : new ReferencesDestinationFacts(null, target, null);
        }

        string decodedTargetFragment;
        try
        {
            decodedTargetFragment = decodedFragment ?? string.Empty;
        }
        catch (Exception)
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    target.Id,
                    target.Path,
                    target.Layer,
                    ReferencesTargetResolution.EncodingUnsupported,
                    null),
                new ReferencesDestinationFinding(ReferencesFindingCode.InvalidEncoding, "The authored fragment encoding is invalid.", []));
        }

        var read = await _strictUtf8Reader(physicalTargetPath, canonicalPath, cancellationToken).ConfigureAwait(false);
        if (read.State == FileReadState.Cancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (read.State != FileReadState.Complete || read.Value is null)
        {
            var resolution = read.State == FileReadState.InvalidEncoding
                ? ReferencesTargetResolution.EncodingUnsupported
                : ReferencesTargetResolution.Unreadable;
            var code = read.State == FileReadState.InvalidEncoding
                ? ReferencesFindingCode.InvalidEncoding
                : ReferencesFindingCode.TargetUnreadable;
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    target.Id,
                    target.Path,
                    target.Layer,
                    resolution,
                    null),
                new ReferencesDestinationFinding(
                    code,
                    "The target bytes could not be read.",
                    []));
        }

        MarkdownDocumentFacts targetDocument;
        try
        {
            targetDocument = _markdownParser(read.Value);
        }
        catch (Exception)
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    target.Id,
                    target.Path,
                    target.Layer,
                    ReferencesTargetResolution.Unreadable,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.TargetUnreadable,
                    "The target Markdown fragment facts could not be established.",
                    []));
        }

        if (targetDocument.Headings.Any(heading =>
                heading.IsCanonical
                && heading.FragmentIdentifier is not null
                && string.Equals(heading.FragmentIdentifier, decodedTargetFragment, StringComparison.Ordinal)))
        {
            return identityCandidates.Length > 1
                ? IdentityCollision(target, identityCandidates, rawFragment)
                : new ReferencesDestinationFacts(rawFragment, target, null);
        }

        if (targetDocument.Headings.Any(heading => heading.IsCanonical && heading.FragmentIdentifier is null))
        {
            return new ReferencesDestinationFacts(
                rawFragment,
                new ReferencesTarget(
                    ReferencesTargetKind.Local,
                    target.Id,
                    target.Path,
                    target.Layer,
                    ReferencesTargetResolution.Unreadable,
                    null),
                new ReferencesDestinationFinding(
                    ReferencesFindingCode.TargetUnreadable,
                    "A canonical target heading has no available fragment identifier.",
                    []));
        }

        return new ReferencesDestinationFacts(
            rawFragment,
            new ReferencesTarget(
                ReferencesTargetKind.Local,
                target.Id,
                target.Path,
                target.Layer,
                ReferencesTargetResolution.FragmentMissing,
                null),
            new ReferencesDestinationFinding(
                ReferencesFindingCode.FragmentMissing,
                "The authored fragment is not present on the target.",
                []));
    }

    private static bool TryReadAbsoluteUri(string raw, [NotNullWhen(true)] out Uri? uri)
    {
        uri = null;
        if (IsDriveRooted(raw) || raw.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var parsed) || parsed is null)
        {
            return false;
        }

        uri = parsed;
        return !string.IsNullOrEmpty(parsed.Scheme);
    }

    private static string? ReadFragment(string raw)
    {
        var hash = raw.IndexOf('#');
        return hash < 0 ? null : raw[(hash + 1)..];
    }

    private static bool IsDriveRooted(string value)
        => value.Length >= 3
            && char.IsLetter(value[0])
            && value[1] == ':'
            && (value[2] == '/' || value[2] == '\\');

    private static string ReadCanonicalPath(string lexicalRoot, string target)
    {
        var relative = Path.GetRelativePath(lexicalRoot, target);
        if (relative == ".")
        {
            return ".";
        }

        return relative.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string DecodePercent(string value)
    {
        var bytes = new List<byte>(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%')
            {
                var scalar = char.ConvertToUtf32(value, index);
                var scalarText = char.ConvertFromUtf32(scalar);
                bytes.AddRange(StrictUtf8.GetBytes(scalarText));
                if (scalar > 0xFFFF)
                {
                    index++;
                }

                continue;
            }

            if (index + 2 >= value.Length
                || !TryHex(value[index + 1], out var high)
                || !TryHex(value[index + 2], out var low))
            {
                throw new FormatException("A percent-encoded destination contains an invalid triplet.");
            }

            bytes.Add((byte)((high << 4) | low));
            index += 2;
        }

        try
        {
            return StrictUtf8.GetString(bytes.ToArray());
        }
        catch (DecoderFallbackException exception)
        {
            throw new FormatException("A percent-encoded destination is not valid UTF-8.", exception);
        }
    }

    private static bool TryHex(char value, out int result)
    {
        result = value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1,
        };
        return result >= 0;
    }

    private static ReferencesDestinationFacts Unsupported(string raw)
        => new(
            ReadFragment(raw),
            new ReferencesTarget(
                ReferencesTargetKind.Unsupported,
                null,
                null,
                null,
                ReferencesTargetResolution.Unsupported,
                null),
            new ReferencesDestinationFinding(
                ReferencesFindingCode.DestinationUnsupported,
                "The destination uses an unsupported URI scheme.",
                []));

    private static ReferencesDestinationFacts Malformed(
        string? fragment,
        ReferencesTargetResolution resolution,
        string cause)
        => new(
            fragment,
            new ReferencesTarget(ReferencesTargetKind.Local, null, null, null, resolution, null),
            new ReferencesDestinationFinding(ReferencesFindingCode.DestinationMalformed, cause, []));

    private static ReferencesDestinationFacts Unsafe(
        string? fragment,
        ReferencesTargetResolution resolution,
        string cause)
        => new(
            fragment,
            new ReferencesTarget(ReferencesTargetKind.Local, null, null, null, resolution, null),
            new ReferencesDestinationFinding(ReferencesFindingCode.TargetUnsafe, cause, []));

    private static IReadOnlyList<(SourceLogicalSource Source, SourceLayer Layer)> ReadTargetLayers(
        SourceCatalogue catalogue,
        string physicalTargetPath)
        => catalogue.Sources
            .SelectMany(source => ReadLayers(source).Select(layer => (Source: source, Layer: layer)))
            .Where(candidate => PhysicalIdentityTracker.PathComparer.Equals(
                candidate.Layer.PhysicalPath,
                physicalTargetPath))
            .OrderBy(candidate => candidate.Source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Layer.Kind)
            .ToArray();

    private static IReadOnlyList<SourceLayer> ReadLayers(SourceLogicalSource source)
        => source.Overwrite is { } overwrite
            ? [source.Base, overwrite]
            : [source.Base];

    private static ReferencesSourceIdentity ToIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);

    private static ReferencesDestinationFacts IdentityCollision(
        ReferencesTarget target,
        IEnumerable<SourceLogicalSource> candidates,
        string? fragment = null)
        => new(
            fragment,
            target,
            new ReferencesDestinationFinding(
                ReferencesFindingCode.IdentityCollision,
                "The target path is exact, but its automatic source ID is shared by more than one logical source.",
                candidates.Select(ToIdentity).ToArray()));
}
