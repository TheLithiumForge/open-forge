using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorJsonProjection
{
    internal static DoctorJsonDocument Create(DoctorResult result)
        => new()
        {
            SchemaVersion = DoctorDefinitions.SchemaVersion,
            Command = result.Command,
            Status = DoctorWireVocabulary.Status(result.Status),
            Workspace = result.Workspace is { } workspace
                ? new DoctorJsonWorkspace
                {
                    Path = workspace.LexicalRoot,
                    SelectedBy = DoctorWireVocabulary.WorkspaceSelection(workspace.SelectedBy),
                }
                : null,
            Result = new DoctorJsonResult
            {
                ReadOnly = result.Diagnosis.ReadOnly,
                ChangesMade = result.Diagnosis.ChangesMade,
                Coverage = DoctorWireVocabulary.Coverage(result.Diagnosis.Coverage),
                Counts = Counts(result.Diagnosis.Counts),
                Actions = result.Diagnosis.Actions.Select(Action).ToArray(),
                Domains = result.Diagnosis.Domains.Select(Domain).ToArray(),
            },
            Next = result.Next is { } next
                ? new DoctorJsonEnvelopeNext { Command = next.Command, Reason = next.Reason }
                : null,
        };

    internal static DoctorJsonAction Action(DoctorNextAction action)
        => new()
        {
            Kind = DoctorFindingWireVocabulary.Action(action.Kind),
            Operation = action.Operation is { } operation
                ? DoctorFindingWireVocabulary.Operation(operation)
                : null,
            Command = action.Command,
            Reason = action.Reason,
        };

    internal static DoctorJsonBoundary Boundary(DoctorBoundary boundary)
        => new()
        {
            Kind = DoctorWireVocabulary.Boundary(boundary.Kind),
            Path = boundary.Path,
        };

    internal static DoctorJsonLocation? Location(SourceLocation? location)
        => location is null
            ? null
            : new DoctorJsonLocation
            {
                Line = location.Line,
                Column = location.Column,
                ByteOffset = location.ByteOffset,
                ByteLength = location.ByteLength,
            };

    private static DoctorJsonDomain Domain(DoctorDomainReport domain)
        => new()
        {
            Domain = DoctorWireVocabulary.Domain(domain.Domain),
            Boundary = Boundary(domain.Boundary),
            Coverage = DoctorWireVocabulary.Coverage(domain.Coverage),
            Lifecycle = domain.Lifecycle is { } lifecycle
                ? DoctorWireVocabulary.Lifecycle(lifecycle)
                : null,
            SourceAvailability = domain.SourceAvailability is { } sourceAvailability
                ? DoctorWireVocabulary.SourceAvailability(sourceAvailability)
                : null,
            Limitations = domain.Limitations.Select(limitation => new DoctorJsonLimitation
            {
                Kind = DoctorWireVocabulary.Limitation(limitation.Kind),
                Message = limitation.Message,
            }).ToArray(),
            Counts = Counts(domain.Counts),
            Findings = domain.Findings.Select(DoctorJsonFindingProjection.Finding).ToArray(),
            Actions = domain.Actions.Select(Action).ToArray(),
        };

    private static DoctorJsonCounts Counts(DoctorFindingCounts counts)
        => new()
        {
            Resolution = new DoctorJsonResolutionCounts
            {
                SafeExact = Count(counts.Resolution.SafeExact),
                GuidedChoice = Count(counts.Resolution.GuidedChoice),
                TargetedOperation = Count(counts.Resolution.TargetedOperation),
                ManualDecision = Count(counts.Resolution.ManualDecision),
                BlockedRepair = Count(counts.Resolution.BlockedRepair),
                Informational = Count(counts.Resolution.Informational),
            },
            Severity = new DoctorJsonSeverityCounts
            {
                Information = Count(counts.Severity.Information),
                Warning = Count(counts.Severity.Warning),
                Error = Count(counts.Severity.Error),
            },
        };

    private static DoctorJsonCount Count(DoctorCount count)
        => new()
        {
            State = DoctorWireVocabulary.ValueState(count.State),
            Value = count.Value,
        };
}
