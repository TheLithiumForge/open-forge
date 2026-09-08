using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorLibraryPresentation
{
    internal static DoctorJsonLibrary Project(LibraryDoctorView libraries)
    {
        ArgumentNullException.ThrowIfNull(libraries);
        return new DoctorJsonLibrary
        {
            State = State(libraries.State),
            RecordPath = ".agents/open-forge.libraries.json",
            RecordState = RecordState(libraries.Record.State),
            Record = libraries.Record.Record is { } record ? Document(record) : null,
            Roots = [.. libraries.Inventories.Select(Root)],
            Mappings = [.. libraries.Mappings.Select(Mapping)],
            Ownership = libraries.Ownership is { } ownership
                ? [.. ownership.Claims.Select(Ownership)]
                : [],
            LinkCapability = libraries.LinkCapability is { } capability
                ? LinkCapability(capability.State)
                : null,
            LinkCapabilityEvidence = libraries.LinkCapability?.Evidence,
        };
    }

    internal static DoctorJsonLibrarySubject Subject(DoctorLibrarySubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return subject.Kind switch
        {
            DoctorSubjectKind.Library => CreateSubject(
                subject,
                sourceRoot: subject.Registration!.SourceRoot.Value),
            DoctorSubjectKind.LibrarySourceRoot => CreateSubject(
                subject,
                sourceRoot: subject.Source!.Request.SourceRoot.Value),
            DoctorSubjectKind.LibraryMapping => CreateSubject(
                subject,
                sourceRoot: SourceRoot(subject.Mapping!),
                mapping: subject.Mapping),
            DoctorSubjectKind.LibraryProjection => CreateSubject(
                subject,
                sourceRoot: SourceRoot(subject.Projection!.Mapping),
                mapping: subject.Projection.Mapping),
            DoctorSubjectKind.LibraryResidual => CreateSubject(
                subject,
                sourceRoot: ResidualSourceRoot(subject.Residual!),
                residual: subject.Residual),
            _ => throw new ArgumentOutOfRangeException(nameof(subject), "The Library subject kind is not defined."),
        };
    }

    internal static void Append(StringBuilder builder, LibraryDoctorView libraries)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(libraries);
        builder.AppendLine($"  Libraries: {State(libraries.State)}");
        builder.AppendLine($"    record: .agents/open-forge.libraries.json ({RecordState(libraries.Record.State)})");
        foreach (var inventory in libraries.Inventories)
        {
            builder.AppendLine(
                $"    source root: {DoctorHumanRenderer.Text(inventory.Source.Request.SourceRoot.Value)} ({SourceState(inventory.Source.State)})");
            builder.AppendLine(
                $"      inventory: {InventoryState(inventory)} ({inventory.Inventory?.Entries.Length ?? 0} eligible paths)");
        }

        foreach (var mapping in libraries.Mappings)
        {
            builder.AppendLine(
                $"    projection: {DoctorHumanRenderer.Text(
                    mapping.Mapping.DestinationPath.Value)} ({MappingState(mapping.State)}; expected={DoctorHumanRenderer.Text(
                        mapping.Mapping.ExpectedRelativeLink.Value)})");
        }

        if (libraries.LinkCapability is { } capability)
        {
            builder.AppendLine($"    link capability: {LinkCapability(capability.State)}");
        }
    }

    private static DoctorJsonLibraryRoot Root(LibraryInventoryRead inventory)
        => new()
        {
            SourceRoot = inventory.Source.Request.SourceRoot.Value,
            State = SourceState(inventory.Source.State),
            LexicalPath = inventory.Source.LexicalSourceRoot,
            PhysicalPath = inventory.Source.PhysicalSourceRoot,
            LexicallyContained = inventory.Source.LexicallyContained,
            PhysicallyContained = inventory.Source.PhysicallyContained,
            PhysicallyDisjoint = inventory.Source.PhysicallyDisjoint,
            PhysicalAgentsDirectory = inventory.Source.PhysicalAgentsDirectory,
            InventoryPaths = inventory.Inventory is { } value
                ? [.. value.Entries.Select(entry => entry.SourcePath.Value)]
                : null,
            ExcludedPaths = [.. inventory.ExcludedPaths.Select(exclusion => new DoctorJsonEvidence
            {
                Kind = "state",
                Basis = null,
                State = "not-applicable",
                Expected = null,
                Actual = null,
                Value = Exclusion(exclusion.Kind),
                Path = exclusion.Path,
                Location = null,
            })],
            UnavailablePaths = [.. inventory.UnavailablePaths.Select(unavailable => new DoctorJsonEvidence
            {
                Kind = "availability",
                Basis = null,
                State = "unavailable",
                Expected = null,
                Actual = null,
                Value = unavailable.Cause,
                Path = unavailable.Path,
                Location = null,
            })],
            Cause = inventory.Inventory?.Cause ?? inventory.Source.Cause,
        };

    private static DoctorJsonLibraryMapping Mapping(LibraryMappingObservation observation)
        => new()
        {
            SourcePath = observation.Mapping.SourcePath.Value,
            DestinationPath = observation.Mapping.DestinationPath.Value,
            ExpectedRelativeLink = observation.Mapping.ExpectedRelativeLink.Value,
            State = MappingState(observation.State),
            LeafState = LeafState(observation.Leaf.State),
            LinkKind = observation.Leaf.RelativeFileLink is { } relative
                ? LinkKind(relative.LinkKind)
                : observation.Leaf.Link is { } link ? LinkKind(link.LinkKind) : null,
            RawLinkTarget = observation.Leaf.RelativeFileLink?.RawRelativeTarget
                ?? observation.Leaf.Link?.RawTarget,
            LinkTargetForm = observation.Leaf.RelativeFileLink is not null
                ? "relative"
                : observation.Leaf.Link is { } target ? TargetForm(target.TargetForm) : null,
            Cause = observation.Cause ?? observation.Leaf.Failure?.DirectCause,
        };

    private static DoctorJsonEvidence Ownership(LifecycleOwnershipClaim claim)
        => new()
        {
            Kind = "state",
            Basis = null,
            State = "valid",
            Expected = null,
            Actual = null,
            Value = $"{OwnershipManager(claim.Manager)}:{claim.Owner}",
            Path = claim.Path,
            Location = null,
        };

    private static DoctorJsonLibrarySubject CreateSubject(
        DoctorLibrarySubject subject,
        string? sourceRoot,
        LibraryMapping? mapping = null,
        LibraryResidualEvidence? residual = null)
        => new()
        {
            LibraryId = subject.LibraryId.Value,
            SourceRoot = sourceRoot,
            SourcePath = mapping?.SourcePath.Value,
            DestinationPath = mapping?.DestinationPath.Value,
            ExpectedRelativeLink = mapping?.ExpectedRelativeLink.Value,
            BundlePath = residual?.Residual.Candidate.Path,
            EntryOrdinal = residual?.Entry.Input.Context.Entry.Ordinal,
        };

    private static string? ResidualSourceRoot(LibraryResidualEvidence residual)
        => residual.CurrentRecord.Record?.Libraries.FirstOrDefault(record => record.Id == residual.LibraryId)?.SourceRoot.Value
            ?? residual.VerifiedPriorRecord?.Record.Libraries.FirstOrDefault(record => record.Id == residual.LibraryId)?.SourceRoot.Value;

    private static string SourceRoot(LibraryMapping mapping)
    {
        var destinationParent = mapping.DestinationPath.Value.Split('/').SkipLast(1);
        var segments = new List<string>(destinationParent);
        foreach (var segment in mapping.ExpectedRelativeLink.Value.Split('/'))
        {
            if (segment == "..")
            {
                if (segments.Count == 0)
                {
                    throw new ArgumentException("A Library mapping target escapes its portable workspace identity.", nameof(mapping));
                }

                segments.RemoveAt(segments.Count - 1);
            }
            else if (segment != ".")
            {
                segments.Add(segment);
            }
        }

        var sourceSegments = mapping.SourcePath.Value.Split('/');
        if (segments.Count <= sourceSegments.Length
            || !segments.TakeLast(sourceSegments.Length).SequenceEqual(sourceSegments, StringComparer.Ordinal))
        {
            throw new ArgumentException("A Library mapping does not retain its source-root identity.", nameof(mapping));
        }

        return string.Join('/', segments.Take(segments.Count - sourceSegments.Length));
    }

    private static LibrariesRecordDocument Document(LibrariesRecord record)
        => new()
        {
            SchemaVersion = record.SchemaVersion,
            Libraries = [.. record.Libraries.Select(library => new LibraryRecordDocument
            {
                Id = library.Id.Value,
                SourceRoot = library.SourceRoot.Value,
                Paths = [.. library.Paths.Select(path => path.Value)],
            })],
        };

    private static string State(OperationalViewState state)
        => state switch
        {
            OperationalViewState.Complete => "complete",
            OperationalViewState.Incomplete => "incomplete",
            OperationalViewState.Blocked => "blocked",
            OperationalViewState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library operational state is not defined."),
        };

    private static string RecordState(LibrariesRecordReadState state)
        => state switch
        {
            LibrariesRecordReadState.Missing => "missing",
            LibrariesRecordReadState.Complete => "complete",
            LibrariesRecordReadState.Malformed => "malformed",
            LibrariesRecordReadState.Unavailable => "unavailable",
            LibrariesRecordReadState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library record state is not defined."),
        };

    private static string SourceState(LibrarySourceRootState state)
        => state switch
        {
            LibrarySourceRootState.Available => "available",
            LibrarySourceRootState.Missing => "missing",
            LibrarySourceRootState.Invalid => "invalid",
            LibrarySourceRootState.Inaccessible => "inaccessible",
            LibrarySourceRootState.Blocked => "blocked",
            LibrarySourceRootState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library source-root state is not defined."),
        };

    private static string InventoryState(LibraryInventoryRead inventory)
        => inventory.Inventory?.State switch
        {
            LibraryInventoryState.Complete => "complete",
            LibraryInventoryState.Incomplete => "incomplete",
            LibraryInventoryState.Blocked => "blocked",
            LibraryInventoryState.Unavailable => "unavailable",
            null => "not-observed",
            _ => throw new ArgumentOutOfRangeException(nameof(inventory), inventory.Inventory.State, "The Library inventory state is not defined."),
        };

    private static string MappingState(LibraryMappingObservationState state)
        => state switch
        {
            LibraryMappingObservationState.Current => "current",
            LibraryMappingObservationState.Missing => "missing",
            LibraryMappingObservationState.Changed => "changed",
            LibraryMappingObservationState.Blocked => "blocked",
            LibraryMappingObservationState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library mapping state is not defined."),
        };

    private static string LeafState(NoFollowLeafState state)
        => state switch
        {
            NoFollowLeafState.Missing => "missing",
            NoFollowLeafState.OrdinaryFile => "ordinary-file",
            NoFollowLeafState.Directory => "directory",
            NoFollowLeafState.RelativeFileLink => "relative-file-link",
            NoFollowLeafState.Link => "link",
            NoFollowLeafState.ReparsePoint => "reparse-point",
            NoFollowLeafState.Special => "special",
            NoFollowLeafState.Inaccessible => "inaccessible",
            NoFollowLeafState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The no-follow leaf state is not defined."),
        };

    private static string LinkKind(NoFollowLinkKind kind)
        => kind switch
        {
            NoFollowLinkKind.SymbolicLink => "symbolic-link",
            NoFollowLinkKind.Junction => "junction",
            NoFollowLinkKind.Other => "other",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The no-follow link kind is not defined."),
        };

    private static string TargetForm(NoFollowLinkTargetForm form)
        => form switch
        {
            NoFollowLinkTargetForm.Relative => "relative",
            NoFollowLinkTargetForm.Absolute => "absolute",
            NoFollowLinkTargetForm.Unsupported => "unsupported",
            NoFollowLinkTargetForm.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The link target form is not defined."),
        };

    private static string Exclusion(LibraryInventoryExclusionKind kind)
        => kind switch
        {
            LibraryInventoryExclusionKind.Loader => "loader",
            LibraryInventoryExclusionKind.Entrypoint => "entrypoint",
            LibraryInventoryExclusionKind.Overwrite => "overwrite",
            LibraryInventoryExclusionKind.ManagerControl => "manager-control",
            LibraryInventoryExclusionKind.Link => "link",
            LibraryInventoryExclusionKind.ReparsePoint => "reparse-point",
            LibraryInventoryExclusionKind.Special => "special",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library inventory exclusion kind is not defined."),
        };

    private static string LinkCapability(LibraryLinkCapabilityState state)
        => state switch
        {
            LibraryLinkCapabilityState.Supported => "supported",
            LibraryLinkCapabilityState.Unsupported => "unsupported",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library link capability state is not defined."),
        };

    private static string OwnershipManager(LifecycleOwnershipManager manager)
        => manager switch
        {
            LifecycleOwnershipManager.Framework => "framework",
            LifecycleOwnershipManager.Extension => "extension",
            _ => throw new ArgumentOutOfRangeException(nameof(manager), manager, "The lifecycle manager is not defined."),
        };
}
