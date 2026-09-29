using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;

internal enum ApplyToMetadataState
{
    Absent,
    Valid,
    Invalid,
}

internal enum ApplyToMetadataFailureKind
{
    InvalidShape,
    DuplicateField,
    ConflictingFields,
    InvalidPattern,
}

internal enum ApplyToMetadataLocation
{
    Root,
    OpenForge,
}

internal sealed record ApplyToDeclaration(
    ApplyToMetadataLocation Location,
    YamlTextSpan KeySpan,
    YamlTextSpan ValueSpan,
    ImmutableArray<ApplyToPattern> Patterns);

internal sealed record ApplyToMetadataFailure(
    ApplyToMetadataFailureKind Kind,
    YamlTextSpan? Span,
    ApplyToPatternFailure? Cause);

internal sealed record ApplyToMetadataFacts
{
    private ApplyToMetadataFacts(
        ApplyToMetadataState state,
        ImmutableArray<ApplyToPattern> patterns,
        ImmutableArray<ApplyToDeclaration> declarations,
        ApplyToMetadataFailure? failure)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The applyTo metadata state is not defined.");
        }

        if (patterns.IsDefault || declarations.IsDefault)
        {
            throw new ArgumentException("applyTo metadata collections must be initialized.");
        }

        if ((state == ApplyToMetadataState.Invalid) != (failure is not null)
            || state == ApplyToMetadataState.Absent && (!patterns.IsEmpty || !declarations.IsEmpty)
            || state == ApplyToMetadataState.Valid && (patterns.IsEmpty || declarations.IsEmpty))
        {
            throw new ArgumentException("applyTo facts must carry values and failures consistent with their state.");
        }

        State = state;
        Patterns = patterns;
        Declarations = declarations;
        Failure = failure;
    }

    internal ApplyToMetadataState State { get; }

    internal ImmutableArray<ApplyToPattern> Patterns { get; }

    internal ImmutableArray<ApplyToDeclaration> Declarations { get; }

    internal ApplyToMetadataFailure? Failure { get; }

    internal static ApplyToMetadataFacts Absent { get; } = new(
        ApplyToMetadataState.Absent,
        [],
        [],
        null);

    internal static ApplyToMetadataFacts Valid(
        IEnumerable<ApplyToPattern> patterns,
        IEnumerable<ApplyToDeclaration> declarations)
        => new(ApplyToMetadataState.Valid, patterns.ToImmutableArray(), declarations.ToImmutableArray(), null);

    internal static ApplyToMetadataFacts Invalid(
        IEnumerable<ApplyToPattern> patterns,
        IEnumerable<ApplyToDeclaration> declarations,
        ApplyToMetadataFailure failure)
        => new(ApplyToMetadataState.Invalid, patterns.ToImmutableArray(), declarations.ToImmutableArray(), failure);
}
