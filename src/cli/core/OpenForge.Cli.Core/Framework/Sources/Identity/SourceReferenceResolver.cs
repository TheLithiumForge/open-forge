using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal delegate PhysicalPathResolution SourcePhysicalPathResolver(
    CliWorkspace workspace,
    string canonicalPath);

internal sealed class SourceReferenceResolver
{
    private const string UndefinedPhysicalPathStateMessage = "The physical path state is not defined.";
    private const string UnsafePhysicalPathCause = "The exact source path is outside an established safe physical boundary.";

    private readonly SourcePhysicalPathResolver _physicalPathResolver;

    internal SourceReferenceResolver(SourcePhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal SourceReferenceResolution Resolve(string value, SourceCatalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(catalogue);

        var parsed = SourceReferenceParser.Parse(value);
        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            return Unresolved(
                value: value,
                form: parsed.Kind,
                state: SourceReferenceResolutionState.Invalid,
                canonicalPath: ReadCanonicalPath(parsed.AttemptedPath),
                cause: parsed.Cause);
        }

        return parsed.Kind switch
        {
            SourceReferenceKind.SourceId => ResolveId(value, parsed, catalogue),
            SourceReferenceKind.SourcePath => ResolvePath(value, parsed, catalogue),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source-reference kind is not defined."),
        };
    }

    private static SourceReferenceResolution ResolveId(
        string value,
        SourceReferenceParseResult parsed,
        SourceCatalogue catalogue)
    {
        var id = parsed.AttemptedId
            ?? throw new InvalidOperationException("A valid source ID reference requires an attempted ID.");
        var sources = catalogue.FindAllById(id);
        if (sources.Count == 1)
        {
            return Resolved(value, parsed.Kind, sources[0]);
        }

        if (sources.Count > 1)
        {
            return new SourceReferenceResolution(
                value: value,
                form: parsed.Kind,
                state: SourceReferenceResolutionState.Ambiguous,
                source: null,
                candidates: sources,
                canonicalPath: null,
                cause: "The source ID resolves to more than one logical source.");
        }

        var candidates = catalogue.FindAllCandidatesById(id);
        if (candidates.Any(candidate => IsUnsafe(candidate.PhysicalState)))
        {
            return Unresolved(
                value: value,
                form: parsed.Kind,
                state: SourceReferenceResolutionState.Unsafe,
                canonicalPath: null,
                cause: "The source ID has a candidate outside an established safe physical boundary.");
        }

        return Unresolved(
            value: value,
            form: parsed.Kind,
            state: SourceReferenceResolutionState.Unknown,
            canonicalPath: null,
            cause: "The source ID does not identify a retained logical source.");
    }

    private SourceReferenceResolution ResolvePath(
        string value,
        SourceReferenceParseResult parsed,
        SourceCatalogue catalogue)
    {
        var path = parsed.AttemptedPath
            ?? throw new InvalidOperationException("A valid source path reference requires an attempted path.");
        var source = catalogue.FindByPath(path);
        if (source is not null)
        {
            return Resolved(value, parsed.Kind, source);
        }

        var candidate = catalogue.FindCandidateByPath(path);
        if (candidate is not null)
        {
            return Unresolved(
                value: value,
                form: parsed.Kind,
                state: ReadState(candidate.PhysicalState),
                canonicalPath: path,
                cause: ReadCandidateCause(candidate.PhysicalState));
        }

        var physical = _physicalPathResolver(catalogue.Workspace, path);
        return Unresolved(
            value: value,
            form: parsed.Kind,
            state: ReadState(physical.State),
            canonicalPath: path,
            cause: ReadPhysicalCause(physical.State));
    }

    private static SourceReferenceResolution Resolved(
        string value,
        SourceReferenceKind form,
        SourceLogicalSource source)
        => new(
            value: value,
            form: form,
            state: SourceReferenceResolutionState.Resolved,
            source: source,
            candidates: [],
            canonicalPath: source.Identity.CanonicalBasePath,
            cause: null);

    private static SourceReferenceResolution Unresolved(
        string value,
        SourceReferenceKind form,
        SourceReferenceResolutionState state,
        string? canonicalPath,
        string? cause)
        => new(
            value: value,
            form: form,
            state: state,
            source: null,
            candidates: [],
            canonicalPath: canonicalPath,
            cause: cause ?? ReadCause(state));

    private static SourceReferenceResolutionState ReadState(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => SourceReferenceResolutionState.Unsupported,
            PhysicalPathState.Missing => SourceReferenceResolutionState.Unknown,
            PhysicalPathState.Dangling
                or PhysicalPathState.Inaccessible
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                or PhysicalPathState.InputOutputFailure => SourceReferenceResolutionState.Unsafe,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, UndefinedPhysicalPathStateMessage),
        };

    private static bool IsUnsafe(PhysicalPathState state)
        => state is not (PhysicalPathState.Contained or PhysicalPathState.Missing);

    private static string ReadCandidateCause(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => "The exact path is not an admitted logical source.",
            PhysicalPathState.Missing => "The exact source path is no longer present.",
            PhysicalPathState.Dangling
                or PhysicalPathState.Inaccessible
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                or PhysicalPathState.InputOutputFailure => UnsafePhysicalPathCause,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, UndefinedPhysicalPathStateMessage),
        };

    private static string ReadPhysicalCause(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => "The exact path is not an admitted logical source.",
            PhysicalPathState.Missing => "The exact source path does not exist.",
            PhysicalPathState.Dangling
                or PhysicalPathState.Inaccessible
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                or PhysicalPathState.InputOutputFailure => UnsafePhysicalPathCause,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, UndefinedPhysicalPathStateMessage),
        };

    private static string ReadCause(SourceReferenceResolutionState state)
        => state switch
        {
            SourceReferenceResolutionState.Invalid => "The value is not a valid shared source reference.",
            SourceReferenceResolutionState.Unknown => "The source reference does not identify a known source.",
            SourceReferenceResolutionState.Unsupported => "The source reference does not identify an admitted source.",
            SourceReferenceResolutionState.Ambiguous => "The source reference identifies more than one source.",
            SourceReferenceResolutionState.Unsafe => "The source reference cannot establish a safe physical boundary.",
            SourceReferenceResolutionState.Resolved => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A resolved source reference does not have an unresolved cause."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The source-reference resolution state is not defined."),
        };

    private static string? ReadCanonicalPath(string? path)
        => path is not null && SourceLogicalPath.IsCanonicalRoot(path) ? path : null;
}
