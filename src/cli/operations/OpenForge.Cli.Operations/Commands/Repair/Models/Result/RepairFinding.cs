using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairFinding
{
    internal RepairFinding(
        RepairFindingCode code,
        string cause,
        string? sourceCanonicalPath = null,
        SourceLocation? occurrence = null,
        RepairTargetSelection? target = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Repair finding code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (sourceCanonicalPath is not null)
        {
            sourceCanonicalPath = RepairPathValidation.ValidateMarkdown(
                sourceCanonicalPath,
                nameof(sourceCanonicalPath));
        }

        Code = code;
        Status = RepairDefinitions.ReadStatus(code);
        Cause = cause;
        SourceCanonicalPath = sourceCanonicalPath;
        Occurrence = occurrence;
        Target = target;
    }

    internal RepairFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string Cause { get; }

    internal string? SourceCanonicalPath { get; }

    internal SourceLocation? Occurrence { get; }

    internal CommandSourceLocation? OccurrenceView
        => Occurrence is { } occurrence ? CommandSourceLocation.From(occurrence) : null;

    internal RepairTargetSelection? Target { get; }

    internal RepairDiagnosisObservation? Observation { get; init; }
}

internal sealed record RepairDiagnosisObservation(string? Path, string? Identifier);
