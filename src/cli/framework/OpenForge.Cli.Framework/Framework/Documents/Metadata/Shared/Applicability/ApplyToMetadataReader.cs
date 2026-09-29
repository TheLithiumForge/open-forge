using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using YamlDotNet.Core;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;

internal static class ApplyToMetadataReader
{
    private const string ApplyToKey = "applyTo";
    private const string OpenForgeKey = "open-forge";

    internal static ApplyToMetadataFacts Read(YamlDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.State != YamlDocumentState.Complete || document.Root?.Mapping is not { } root)
        {
            return ApplyToMetadataFacts.Absent;
        }

        var rootFields = root
            .Where(entry => IsKey(entry, ApplyToKey))
            .ToArray();
        var openForgeFields = root
            .Where(entry => IsKey(entry, OpenForgeKey))
            .ToArray();
        var declarations = new List<ApplyToDeclaration>();
        var failure = (ApplyToMetadataFailure?)null;

        if (rootFields.Length > 1)
        {
            failure = FailureForDuplicate(rootFields[1]);
        }

        foreach (var rootField in rootFields)
        {
            TryReadDeclaration(rootField, ApplyToMetadataLocation.Root, declarations, ref failure);
        }

        var scopedFields = new List<YamlMappingEntry>();
        foreach (var openForgeField in openForgeFields)
        {
            if (openForgeField.Value.Mapping is { } metadata)
            {
                scopedFields.AddRange(metadata.Where(entry => IsKey(entry, ApplyToKey)));
            }
        }

        if (scopedFields.Count > 1)
        {
            failure ??= FailureForDuplicate(scopedFields[1]);
        }

        foreach (var scopedField in scopedFields)
        {
            TryReadDeclaration(scopedField, ApplyToMetadataLocation.OpenForge, declarations, ref failure);
        }

        if (declarations.Count == 0 && failure is null)
        {
            return ApplyToMetadataFacts.Absent;
        }

        var allPatterns = declarations
            .SelectMany(declaration => declaration.Patterns)
            .DistinctBy(pattern => pattern.Text, StringComparer.Ordinal)
            .ToImmutableArray();
        if (failure is not null)
        {
            return ApplyToMetadataFacts.Invalid(allPatterns, declarations, failure);
        }

        if (declarations.Count == 2)
        {
            var first = Normalize(declarations[0].Patterns);
            var second = Normalize(declarations[1].Patterns);
            if (!first.SequenceEqual(second, StringComparer.Ordinal))
            {
                var later = declarations[1];
                return ApplyToMetadataFacts.Invalid(
                    first.Select(text => FindPattern(declarations[0], text)),
                    declarations,
                    new ApplyToMetadataFailure(
                        ApplyToMetadataFailureKind.ConflictingFields,
                        later.KeySpan,
                        null));
            }

            return ApplyToMetadataFacts.Valid(
                first.Select(text => FindPattern(declarations[0], text)),
                declarations);
        }

        return ApplyToMetadataFacts.Valid(
            Normalize(declarations[0].Patterns).Select(text => FindPattern(declarations[0], text)),
            declarations);
    }

    private static bool IsKey(YamlMappingEntry entry, string key)
        => string.Equals(entry.Key.Scalar?.Value, key, StringComparison.Ordinal);

    private static void TryReadDeclaration(
        YamlMappingEntry entry,
        ApplyToMetadataLocation location,
        ICollection<ApplyToDeclaration> declarations,
        ref ApplyToMetadataFailure? failure)
    {
        if (entry.Key.Scalar is not { } key)
        {
            failure ??= FailureForShape(entry.Key, entry.Value.Span);
            return;
        }

        var parsed = new List<ApplyToPattern>();
        if (entry.Value.Scalar is { } scalar)
        {
            if (!IsQuotedString(scalar))
            {
                failure ??= FailureForShape(entry.Key, entry.Value.Span);
                AddDeclaration(entry, location, parsed, declarations);
                return;
            }

            var result = ApplyToPatternExpressionParser.Parse(scalar.Value);
            if (result.Failure is { } patternFailure)
            {
                failure ??= new ApplyToMetadataFailure(
                    ApplyToMetadataFailureKind.InvalidPattern,
                    scalar.Span,
                    patternFailure);
                AddDeclaration(entry, location, parsed, declarations);
                return;
            }

            parsed.AddRange(result.Patterns);
        }
        else if (entry.Value.Sequence is { } sequence)
        {
            if (sequence.Count == 0)
            {
                failure ??= FailureForShape(entry.Key, entry.Value.Span);
                AddDeclaration(entry, location, parsed, declarations);
                return;
            }

            foreach (var item in sequence)
            {
                if (item.Scalar is not { } itemScalar || !IsQuotedString(itemScalar))
                {
                    failure ??= FailureForShape(entry.Key, item.Span);
                    AddDeclaration(entry, location, parsed, declarations);
                    return;
                }

                if (!TryParse(itemScalar, parsed, out var parseFailure))
                {
                    failure ??= parseFailure;
                    AddDeclaration(entry, location, parsed, declarations);
                    return;
                }
            }
        }
        else
        {
            failure ??= FailureForShape(entry.Key, entry.Value.Span);
            AddDeclaration(entry, location, parsed, declarations);
            return;
        }

        AddDeclaration(entry, location, parsed, declarations);
    }

    private static void AddDeclaration(
        YamlMappingEntry entry,
        ApplyToMetadataLocation location,
        IEnumerable<ApplyToPattern> patterns,
        ICollection<ApplyToDeclaration> declarations)
    {
        var key = entry.Key.Scalar
            ?? throw new InvalidOperationException("An applyTo declaration requires a scalar key.");
        declarations.Add(new ApplyToDeclaration(
            location,
            key.Span,
            entry.Value.Span,
            patterns.ToImmutableArray()));
    }

    private static bool IsQuotedString(YamlScalar scalar)
        => scalar.Style is ScalarStyle.SingleQuoted or ScalarStyle.DoubleQuoted
            && scalar.IsQuotedImplicit
            && !scalar.IsPlainImplicit;

    private static bool TryParse(
        YamlScalar scalar,
        ICollection<ApplyToPattern> patterns,
        out ApplyToMetadataFailure? failure)
    {
        var result = ApplyToPatternMatcher.Parse(scalar.Value.Trim());
        if (result.Pattern is { } pattern)
        {
            patterns.Add(pattern);
            failure = null;
            return true;
        }

        failure = new ApplyToMetadataFailure(
            ApplyToMetadataFailureKind.InvalidPattern,
            scalar.Span,
            result.Failure);
        return false;
    }

    private static ApplyToMetadataFailure FailureForDuplicate(YamlMappingEntry duplicate)
        => new(
            ApplyToMetadataFailureKind.DuplicateField,
            duplicate.Key.Scalar?.Span,
            null);

    private static ApplyToMetadataFailure FailureForShape(YamlNode key, YamlTextSpan? valueSpan)
        => new(
            ApplyToMetadataFailureKind.InvalidShape,
            key.Scalar?.Span ?? valueSpan,
            null);

    private static ImmutableArray<string> Normalize(IEnumerable<ApplyToPattern> patterns)
        => patterns.Select(pattern => pattern.Text)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();

    private static ApplyToPattern FindPattern(ApplyToDeclaration declaration, string text)
        => declaration.Patterns.First(pattern => string.Equals(pattern.Text, text, StringComparison.Ordinal));
}
