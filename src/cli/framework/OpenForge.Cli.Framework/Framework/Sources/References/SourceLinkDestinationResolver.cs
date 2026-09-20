using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.References;

internal delegate PhysicalPathResolution SourceLinkPhysicalPathResolver(
    CliWorkspace workspace,
    string lexicalPath);

internal delegate ValueTask<FileReadResult<string>> SourceLinkStrictUtf8Reader(
    string physicalPath,
    string logicalPath,
    CancellationToken cancellationToken);

internal delegate MarkdownDocumentFacts SourceLinkMarkdownParser(string source);

internal sealed class SourceLinkDestinationResolver
{
    private const string UndefinedLexicalPathStateMessage = "The source-link lexical path state is not defined.";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly SourceLinkPhysicalPathResolver _physicalPathResolver;
    private readonly SourceLinkStrictUtf8Reader _strictUtf8Reader;
    private readonly SourceLinkMarkdownParser _markdownParser;

    internal SourceLinkDestinationResolver(
        SourceLinkPhysicalPathResolver physicalPathResolver,
        SourceLinkStrictUtf8Reader strictUtf8Reader,
        SourceLinkMarkdownParser markdownParser)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(strictUtf8Reader);
        ArgumentNullException.ThrowIfNull(markdownParser);
        _physicalPathResolver = physicalPathResolver;
        _strictUtf8Reader = strictUtf8Reader;
        _markdownParser = markdownParser;
    }

    internal async ValueTask<SourceLinkDestinationFacts> ResolveAsync(
        SourceLinkDestinationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();
        var raw = input.RawDestination;
        if (TryReadAbsoluteUri(raw, out var uri))
        {
            return uri.Scheme is "http" or "https"
                ? new SourceLinkDestinationFacts
                {
                    Fragment = ReadFragment(raw),
                    Target = new SourceLinkTarget
                    {
                        Kind = SourceLinkTargetKind.External,
                        Id = null,
                        Path = null,
                        PhysicalPath = null,
                        Layer = null,
                        Resolution = SourceLinkTargetResolution.ExternalUnchecked,
                        Network = SourceLinkNetworkState.NetworkNotAttempted,
                    },
                    Finding = null,
                }
                : Unsupported(raw);
        }

        var hash = raw.IndexOf('#');
        var rawPath = hash >= 0 ? raw[..hash] : raw;
        var rawFragment = hash >= 0 ? raw[(hash + 1)..] : null;
        if (raw.Length == 0 || (hash < 0 && rawPath.Length == 0))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Malformed, "The local destination is empty.");
        }

        if (rawPath.Contains('?'))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Query, "A local destination cannot contain an unencoded query.");
        }

        if (rawPath.Contains('\\'))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Malformed, "A local destination cannot use backslash separators.");
        }

        if (IsDriveRooted(rawPath) || rawPath.StartsWith("/", StringComparison.Ordinal))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Absolute, "A local destination cannot be an absolute or rooted path.");
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
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = null,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.EncodingUnsupported,
                    Network = null,
                },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.InvalidEncoding,
                    Cause = exception.Message,
                    Candidates = [],
                },
            };
        }

        if (decodedPath.StartsWith("/", StringComparison.Ordinal) || IsDriveRooted(decodedPath))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Absolute, "A local destination cannot be an absolute or rooted path.");
        }

        if (decodedPath.Contains('\\'))
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Malformed, "A local destination cannot use backslash separators.");
        }

        var lexical = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: input.Workspace.LexicalRoot,
            sourceCanonicalPath: input.SourceCanonicalPath,
            decodedPath: decodedPath);
        if (lexical is not
            { State: SourceLinkLexicalPathState.Complete, LexicalTarget: { } lexicalTarget, CanonicalPath: { } canonicalPath })
        {
            return lexical.State switch
            {
                SourceLinkLexicalPathState.SourceDirectoryMissing =>
                    Unsafe(rawFragment, SourceLinkTargetResolution.PhysicalEscape, "The source layer has no containing directory."),
                SourceLinkLexicalPathState.Malformed =>
                    Malformed(rawFragment, SourceLinkTargetResolution.Malformed, "The local destination path is malformed."),
                SourceLinkLexicalPathState.OutsideWorkspace =>
                    Unsafe(rawFragment, SourceLinkTargetResolution.OutsideWorkspace, "The local destination leaves the selected workspace."),
                SourceLinkLexicalPathState.Complete => throw new InvalidOperationException("A complete source-link lexical path requires both coordinates."),
                _ => throw new ArgumentOutOfRangeException(nameof(lexical), lexical.State, UndefinedLexicalPathStateMessage),
            };
        }

        PhysicalPathResolution physical;
        try
        {
            physical = _physicalPathResolver(input.Workspace, lexicalTarget);
        }
        catch (Exception)
        {
            return Unsafe(rawFragment, SourceLinkTargetResolution.PhysicalEscape, "The local destination physical boundary could not be established.");
        }

        if (physical.State == PhysicalPathState.Missing)
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = canonicalPath,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Missing,
                    Network = null,
                },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetMissing,
                    Cause = "The local destination path does not exist.",
                    Candidates = [],
                },
            };
        }

        if (physical.State != PhysicalPathState.Contained)
        {
            var (resolution, code, cause) = physical.State switch
            {
                PhysicalPathState.Invalid => (
                    SourceLinkTargetResolution.Malformed,
                    SourceLinkDestinationFindingCode.DestinationMalformed,
                    "The local destination path is malformed."),
                PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => (
                    SourceLinkTargetResolution.Unreadable,
                    SourceLinkDestinationFindingCode.TargetUnreadable,
                    "The local destination target could not be read."),
                PhysicalPathState.Contained
                    or PhysicalPathState.Missing
                    or PhysicalPathState.Dangling
                    or PhysicalPathState.External
                    or PhysicalPathState.Cycle
                    or PhysicalPathState.Unsupported => (
                        SourceLinkTargetResolution.PhysicalEscape,
                        SourceLinkDestinationFindingCode.TargetUnsafe,
                        "The local destination physical boundary is unsafe."),
                _ => throw new ArgumentOutOfRangeException(nameof(physical), physical.State, "The physical path state is not defined."),
            };
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = canonicalPath,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = resolution,
                    Network = null,
                },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = code,
                    Cause = cause,
                    Candidates = [],
                },
            };
        }

        var physicalTargetPath = physical.GetContainedPhysicalPath();
        var component = LinkTargetReader.Read(physicalTargetPath);
        if (component.State != PathComponentState.Ordinary)
        {
            if (component.State is PathComponentState.Inaccessible or PathComponentState.InputOutputFailure)
            {
                return new SourceLinkDestinationFacts
                {
                    Fragment = rawFragment,
                    Target = new SourceLinkTarget
                    {
                        Kind = SourceLinkTargetKind.Local,
                        Id = null,
                        Path = canonicalPath,
                        PhysicalPath = physicalTargetPath,
                        Layer = null,
                        Resolution = SourceLinkTargetResolution.Unreadable,
                        Network = null,
                    },
                    Finding = new SourceLinkDestinationFinding
                    {
                        Code = SourceLinkDestinationFindingCode.TargetUnreadable,
                        Cause = "The local destination target could not be read.",
                        Candidates = [],
                    },
                };
            }

            return Unsafe(rawFragment, SourceLinkTargetResolution.PhysicalEscape, "The local destination physical target is unavailable or unsafe.");
        }

        if (component.Attributes is not { } attributes)
        {
            return Unsafe(rawFragment, SourceLinkTargetResolution.PhysicalEscape, "The local destination attributes could not be established.");
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            return Malformed(rawFragment, SourceLinkTargetResolution.Malformed, "A local destination must identify a file, not a directory.");
        }

        var layerMatches = SourceLinkTargetIdentityReader.ReadTargetLayers(input.Catalogue.Sources, physicalTargetPath);
        if (layerMatches.Count > 1)
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = canonicalPath,
                    PhysicalPath = physicalTargetPath,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Ambiguous,
                    Network = null,
                },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetAmbiguous,
                    Cause = "More than one logical source or layer has the destination physical identity.",
                    Candidates = layerMatches.Select(match => SourceLinkTargetIdentityReader.ToIdentity(match.Source)).Distinct().ToArray(),
                },
            };
        }

        var canonicalSource = input.Catalogue.FindByPath(canonicalPath);
        var matchedLayer = canonicalSource is null
            ? null
            : SourceLinkTargetIdentityReader.FindLayer(canonicalSource, canonicalPath);
        if (canonicalSource is not null
            && matchedLayer is not null
            && !PhysicalIdentityTracker.PathComparer.Equals(matchedLayer.PhysicalPath, physicalTargetPath))
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = canonicalPath,
                    PhysicalPath = physicalTargetPath,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Ambiguous,
                    Network = null,
                },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetAmbiguous,
                    Cause = "The authored target path and its current physical source identity disagree.",
                    Candidates = [SourceLinkTargetIdentityReader.ToIdentity(canonicalSource)],
                },
            };
        }

        var identityCandidates = canonicalSource is null
            ? Array.Empty<SourceLogicalSource>()
            : input.Catalogue.FindAllById(canonicalSource.Identity.AutomaticId).ToArray();
        var targetId = identityCandidates.Length > 1 ? null : canonicalSource?.Identity.AutomaticId;
        var target = new SourceLinkTarget
        {
            Kind = SourceLinkTargetKind.Local,
            Id = targetId,
            Path = canonicalPath,
            PhysicalPath = physicalTargetPath,
            Layer = matchedLayer?.Kind,
            Resolution = SourceLinkTargetResolution.Complete,
            Network = null,
        };
        if (rawFragment is null)
        {
            return identityCandidates.Length > 1
                ? SourceLinkTargetIdentityReader.ReadCollision(target, identityCandidates)
                : new SourceLinkDestinationFacts
                {
                    Fragment = null,
                    Target = target,
                    Finding = null,
                };
        }

        var read = await _strictUtf8Reader(physicalTargetPath, canonicalPath, cancellationToken).ConfigureAwait(false);
        if (read.State == FileReadState.Cancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (read.State != FileReadState.Complete || read.Value is null)
        {
            var resolution = read.State == FileReadState.InvalidEncoding
                ? SourceLinkTargetResolution.EncodingUnsupported
                : SourceLinkTargetResolution.Unreadable;
            var code = read.State == FileReadState.InvalidEncoding
                ? SourceLinkDestinationFindingCode.InvalidEncoding
                : SourceLinkDestinationFindingCode.TargetUnreadable;
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = target with { Resolution = resolution },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = code,
                    Cause = "The target bytes could not be read.",
                    Candidates = [],
                },
            };
        }

        MarkdownDocumentFacts targetDocument;
        try
        {
            targetDocument = _markdownParser(read.Value);
        }
        catch (Exception)
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = target with { Resolution = SourceLinkTargetResolution.Unreadable },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetUnreadable,
                    Cause = "The target Markdown fragment facts could not be established.",
                    Candidates = [],
                },
            };
        }

        var fragmentFailure = SourceLinkFragmentProjection.ReadFailure(
            headings: targetDocument.Headings,
            target: target,
            rawFragment: rawFragment,
            decodedFragment: decodedFragment);
        if (fragmentFailure is not null)
        {
            return fragmentFailure;
        }

        return identityCandidates.Length > 1
            ? SourceLinkTargetIdentityReader.ReadCollision(target, identityCandidates, rawFragment)
            : new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = target,
                Finding = null,
            };
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

    internal static bool ResolvesToCanonicalPath(
        CliWorkspace workspace,
        string sourceCanonicalPath,
        string rawDestination,
        string expectedCanonicalPath)
    {
        if (!TryDecodeDestinationPath(rawDestination, out var decodedPath))
        {
            return false;
        }

        var lexical = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: workspace.LexicalRoot,
            sourceCanonicalPath: sourceCanonicalPath,
            decodedPath: decodedPath);
        return lexical switch
        {
            { State: SourceLinkLexicalPathState.Complete, CanonicalPath: { } canonicalPath } =>
                string.Equals(canonicalPath, expectedCanonicalPath, StringComparison.Ordinal),
            { State: SourceLinkLexicalPathState.SourceDirectoryMissing or SourceLinkLexicalPathState.Malformed or SourceLinkLexicalPathState.OutsideWorkspace } => false,
            _ => throw new ArgumentOutOfRangeException(nameof(lexical), lexical.State, UndefinedLexicalPathStateMessage),
        };
    }

    private static bool TryDecodeDestinationPath(
        string rawDestination,
        out string decodedPath)
    {
        var hash = rawDestination.IndexOf('#');
        var rawPath = hash >= 0 ? rawDestination[..hash] : rawDestination;
        decodedPath = string.Empty;
        if (rawPath.Contains('?')
            || rawPath.Contains('\\')
            || IsDriveRooted(rawPath)
            || rawPath.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            decodedPath = DecodePercent(rawPath);
        }
        catch (FormatException)
        {
            return false;
        }

        return !decodedPath.Contains('\\')
            && !IsDriveRooted(decodedPath)
            && !decodedPath.StartsWith("/", StringComparison.Ordinal);
    }

    private static bool IsDriveRooted(string value)
        => value.Length >= 3
            && char.IsLetter(value[0])
            && value[1] == ':'
            && (value[2] == '/' || value[2] == '\\');

    private static string DecodePercent(string value)
    {
        var bytes = new List<byte>(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%')
            {
                var scalar = char.ConvertToUtf32(value, index);
                bytes.AddRange(StrictUtf8.GetBytes(char.ConvertFromUtf32(scalar)));
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

    private static SourceLinkDestinationFacts Unsupported(string raw)
        => new()
        {
            Fragment = ReadFragment(raw),
            Target = new SourceLinkTarget
            {
                Kind = SourceLinkTargetKind.Unsupported,
                Id = null,
                Path = null,
                PhysicalPath = null,
                Layer = null,
                Resolution = SourceLinkTargetResolution.Unsupported,
                Network = null,
            },
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.DestinationUnsupported,
                Cause = "The destination uses an unsupported URI scheme.",
                Candidates = [],
            },
        };

    private static SourceLinkDestinationFacts Malformed(
        string? fragment,
        SourceLinkTargetResolution resolution,
        string cause)
        => new()
        {
            Fragment = fragment,
            Target = new SourceLinkTarget
            {
                Kind = SourceLinkTargetKind.Local,
                Id = null,
                Path = null,
                PhysicalPath = null,
                Layer = null,
                Resolution = resolution,
                Network = null,
            },
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.DestinationMalformed,
                Cause = cause,
                Candidates = [],
            },
        };

    private static SourceLinkDestinationFacts Unsafe(
        string? fragment,
        SourceLinkTargetResolution resolution,
        string cause)
        => new()
        {
            Fragment = fragment,
            Target = new SourceLinkTarget
            {
                Kind = SourceLinkTargetKind.Local,
                Id = null,
                Path = null,
                PhysicalPath = null,
                Layer = null,
                Resolution = resolution,
                Network = null,
            },
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.TargetUnsafe,
                Cause = cause,
                Candidates = [],
            },
        };
}
