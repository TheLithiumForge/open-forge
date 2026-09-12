using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Presentation;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesJsonProjection
{
    internal static ReferencesJsonDocument Create(ReferencesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);
        return new ReferencesJsonDocument
        {
            SchemaVersion = ReferencesDefinitions.SchemaVersion,
            Command = result.Command,
            Status = Status(result.Status),
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new ReferencesJsonResult
            {
                Source = result.Source is null ? null : Source(result.Source),
                RequestedDirection = result.RequestedDirection is null
                    ? null
                    : Direction(result.RequestedDirection.Value),
                IncomingSelection = result.IncomingSelection is null
                    ? null
                    : IncomingSelection(result.IncomingSelection),
                Incoming = result.Incoming is null ? null : Section(result.Incoming),
                Outgoing = result.Outgoing is null ? null : Section(result.Outgoing),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new ReferencesJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static ReferencesJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static ReferencesJsonSource Source(ReferencesSource source)
        => new()
        {
            Id = source.Id,
            Path = source.Path,
            Layers = source.Layers.Select(layer => Layer(layer.Kind)).ToArray(),
        };

    private static ReferencesJsonIncomingSelection IncomingSelection(
        ReferencesIncomingSelection selection)
        => new()
        {
            Mode = selection.Mode switch
            {
                ReferencesSelectionMode.Default => "default",
                ReferencesSelectionMode.Filtered => "filtered",
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Mode, "The selection mode is not defined."),
            },
            Supplied = selection.Supplied.Select(value => new ReferencesJsonSelectorOccurrence
            {
                Role = Role(value.Role),
                Value = value.Value,
            }).ToArray(),
            Resolved = selection.Resolved.Select(SelectorResolution).ToArray(),
            EffectiveSources = selection.EffectiveSources.Select(Identity).ToArray(),
            InspectedSources = selection.InspectedSources.Select(value => new ReferencesJsonSourceLayerEvidence
            {
                Source = Identity(value.Source),
                Layer = Layer(value.Layer),
                Path = value.Path,
            }).ToArray(),
        };

    private static ReferencesJsonSelectorResolution SelectorResolution(
        ReferencesSelectorResolution resolution)
        => new()
        {
            Role = Role(resolution.Role),
            Occurrence = resolution.Occurrence,
            Supplied = resolution.Supplied,
            Form = resolution.Form switch
            {
                SourceReferenceKind.SourceId => "source-id",
                SourceReferenceKind.SourcePath => "source-path",
                _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.Form, "The selector form is not defined."),
            },
            Resolution = resolution.Resolution switch
            {
                SourceReferenceResolutionState.Resolved => "resolved",
                SourceReferenceResolutionState.Invalid => "invalid",
                SourceReferenceResolutionState.Unknown => "unknown",
                SourceReferenceResolutionState.Ambiguous => "ambiguous",
                SourceReferenceResolutionState.Unsupported => "unsupported",
                SourceReferenceResolutionState.Unsafe => "unsafe",
                _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.Resolution, "The selector resolution is not defined."),
            },
            Source = resolution.Source is null ? null : Identity(resolution.Source),
            Expansion = resolution.Expansion is null ? null : Expansion(resolution.Expansion.Value),
            Candidates = resolution.Candidates.Select(Identity).ToArray(),
        };

    private static ReferencesJsonSection Section(ReferencesSection section)
        => new()
        {
            Coverage = section.Coverage switch
            {
                ReferencesCoverage.Complete => "complete",
                ReferencesCoverage.Incomplete => "incomplete",
                ReferencesCoverage.Blocked => "blocked",
                _ => throw new ArgumentOutOfRangeException(nameof(section), section.Coverage, "The section coverage is not defined."),
            },
            Status = Status(section.Status),
            OccurrenceCount = section.OccurrenceCount,
            Occurrences = section.Occurrences.Select(Occurrence).ToArray(),
        };

    private static ReferencesJsonOccurrence Occurrence(ReferencesOccurrence occurrence)
        => new()
        {
            Direction = Direction(occurrence.Direction),
            Level = occurrence.Level,
            Source = new ReferencesJsonOccurrenceSource
            {
                Id = occurrence.Source.Id,
                Path = occurrence.Source.Path,
                Layer = Layer(occurrence.Source.Layer),
            },
            Location = Location(occurrence.Location),
            DestinationLocation = occurrence.DestinationLocation is null
                ? null
                : Location(occurrence.DestinationLocation),
            RawDestination = occurrence.RawDestination,
            Fragment = occurrence.Fragment,
            Target = new ReferencesJsonTarget
            {
                Kind = occurrence.Target.Kind switch
                {
                    ReferencesTargetKind.Local => "local",
                    ReferencesTargetKind.External => "external",
                    ReferencesTargetKind.Unsupported => "unsupported",
                    _ => throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence.Target.Kind, "The target kind is not defined."),
                },
                Id = occurrence.Target.Id,
                Path = occurrence.Target.Path,
                Layer = occurrence.Target.Layer is null ? null : Layer(occurrence.Target.Layer.Value),
                Resolution = Resolution(occurrence.Target.Resolution),
                Network = occurrence.Target.Network is null ? null : "network-not-attempted",
            },
            Provenance = occurrence.Provenance switch
            {
                ReferencesProvenance.SelectedSource => "selected-source",
                ReferencesProvenance.DefaultIncomingScan => "default-incoming-scan",
                ReferencesProvenance.FilteredIncomingScan => "filtered-incoming-scan",
                _ => throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence.Provenance, "The occurrence provenance is not defined."),
            },
        };

}
