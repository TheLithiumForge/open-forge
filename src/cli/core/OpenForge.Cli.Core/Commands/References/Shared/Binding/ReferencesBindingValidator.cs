using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;

namespace OpenForge.Cli.Core.Commands.References.Shared.Binding;

internal static class ReferencesBindingValidator
{
    internal static IReadOnlyList<ReferencesFinding> ReadInvalidFindings(
        ReferencesBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var findings = new List<ReferencesFinding>();
        if (input.DirectionWasSupplied && input.Direction is null)
        {
            findings.Add(CreateFinding(
                ReferencesFindingCode.InvalidDirection,
                null,
                input.DirectionSpelling,
                input.DirectionCause ?? "Direction must be exactly in, out, or both."));
            return findings;
        }

        var applicableDirection = input.Direction is ReferencesDirection.In or ReferencesDirection.Out
            ? input.Direction
            : null;
        if (!input.SourceWasSupplied || string.IsNullOrWhiteSpace(input.SourceReference))
        {
            findings.Add(CreateFinding(
                ReferencesFindingCode.InvalidSource,
                applicableDirection,
                input.SourceReference,
                "A source-reference operand is required."));
        }

        if (input.FilterCause is not null)
        {
            findings.Add(CreateFinding(
                ReferencesFindingCode.InvalidFilter,
                applicableDirection,
                null,
                input.FilterCause));
        }

        if (input.Direction == ReferencesDirection.Out && input.FilterWasSupplied)
        {
            findings.Add(CreateFinding(
                ReferencesFindingCode.InvalidFilter,
                ReferencesDirection.Out,
                null,
                "Include and exclude filters apply only when incoming work is requested."));
        }

        return findings;
    }

    private static ReferencesFinding CreateFinding(
        ReferencesFindingCode code,
        ReferencesDirection? direction,
        string? subject,
        string cause)
        => new(
            code,
            direction,
            subject,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
}
