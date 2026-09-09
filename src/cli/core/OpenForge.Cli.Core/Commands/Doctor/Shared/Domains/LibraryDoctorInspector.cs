using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LibraryDoctorInspector
{
    internal static DoctorDomainReport Inspect(
        CliWorkspace workspace,
        DoctorDomainReport workspaceEntry,
        LibraryDoctorView libraries,
        RecoveryResidualDoctorView recovery,
        ImmutableArray<LibraryResidualEvidence> residuals)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(workspaceEntry);
        ArgumentNullException.ThrowIfNull(libraries);
        ArgumentNullException.ThrowIfNull(recovery);
        if (residuals.IsDefault)
        {
            throw new ArgumentException("Doctor requires an observed Library residual attribution set.", nameof(residuals));
        }

        if (workspaceEntry.Domain != DoctorDomainKind.WorkspaceEntry)
        {
            throw new ArgumentException("Library findings extend the existing workspace-entry domain.", nameof(workspaceEntry));
        }

        var findings = workspaceEntry.Findings.ToList();
        var limitations = workspaceEntry.Limitations.ToList();
        var coverage = DoctorDomainSupport.Combine(
            workspaceEntry.Coverage,
            DoctorDomainSupport.Coverage(libraries.State));
        InspectRecord(libraries.Record, findings);
        foreach (var record in libraries.Record.Record?.Libraries ?? [])
        {
            var inventory = libraries.Inventories.FirstOrDefault(value => value.Source.Request.SourceRoot == record.SourceRoot);
            if (inventory is not null)
            {
                InspectInventory(libraries, record, inventory, findings);
            }
        }

        foreach (var mapping in libraries.Mappings)
        {
            InspectMapping(libraries, mapping, findings);
        }

        InspectOwnership(libraries, findings);
        if (libraries.LinkCapability?.State == LibraryLinkCapabilityState.Unsupported)
        {
            Add(
                findings,
                DoctorFindingKind.LibraryLinkCapabilityUnsupported,
                DoctorResolutionLane.BlockedRepair,
                libraries.LinkCapability.Evidence,
                Subject(libraries, DoctorSubjectKind.Library),
                new DoctorStateEvidence(DoctorObservedState.Unsupported));
        }

        foreach (var residual in residuals)
        {
            var subject = Subject(libraries, DoctorSubjectKind.LibraryResidual, residual);
            var proposal = new DoctorExactProposal(
                DoctorProposalKind.LibraryResidualRecovery,
                reference: null,
                libraryRecovery: residual)
            {
                Subject = subject,
                Boundary = workspaceEntry.Boundary,
                Verification = DoctorProposalVerificationKind.NoFollowPriorState,
                Recovery = DoctorProposalRecoveryKind.VerifiedLibraryResidual,
            };
            var finding = DoctorDomainSupport.CreateWithProposal(
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.LibraryRecoverySafeExact,
                    "A verified Library residual has one exact no-follow recovery proposal.",
                    DoctorResolutionLane.SafeExact),
                subject,
                DoctorDomainSupport.Provenance(
                    DoctorDomainKind.RecoveryResiduals,
                    DoctorProvenanceSource.RecoveryResiduals,
                    residual.Residual.Candidate.Path),
                [new DoctorStateEvidence(DoctorObservedState.Verified)],
                proposal);
            findings.Add(finding);
        }

        if (libraries.State != OperationalViewState.Complete)
        {
            limitations.Add(DoctorDomainSupport.Limitation(
                DoctorDomainSupport.Coverage(libraries.State),
                "Library observation did not establish complete source and projection coverage."));
        }

        var ordered = DoctorFindingAggregation.Order(findings);
        return workspaceEntry with
        {
            Libraries = libraries,
            Coverage = coverage,
            Limitations = limitations,
            Findings = ordered,
            Counts = DoctorFindingAggregation.Count(ordered),
            Actions = DoctorFindingAggregation.Actions(ordered),
        };
    }

    private static void InspectRecord(LibrariesRecordRead record, ICollection<DoctorFinding> findings)
    {
        if (record.State == LibrariesRecordReadState.Malformed)
        {
            Add(findings, DoctorFindingKind.LibraryRecordMalformed, DoctorResolutionLane.BlockedRepair,
                record.Cause ?? "The Library record is malformed.", PlainSubject(DoctorSubjectKind.Library, ".agents/open-forge.libraries.json"),
                new DoctorStateEvidence(DoctorObservedState.Malformed));
        }
        else if (record.State is LibrariesRecordReadState.Unavailable or LibrariesRecordReadState.Blocked)
        {
            Add(findings, DoctorFindingKind.LibraryRecordUnavailable, DoctorResolutionLane.Informational,
                record.Cause ?? "The Library record is unavailable.", PlainSubject(DoctorSubjectKind.Library, ".agents/open-forge.libraries.json"),
                new DoctorStateEvidence(record.State == LibrariesRecordReadState.Blocked
                    ? DoctorObservedState.Blocked
                    : DoctorObservedState.Unavailable));
        }
    }

    private static void InspectInventory(
        LibraryDoctorView libraries,
        LibraryRecord record,
        LibraryInventoryRead inventory,
        ICollection<DoctorFinding> findings)
    {
        if (inventory.Source.State == LibrarySourceRootState.Invalid)
        {
            Add(findings, DoctorFindingKind.LibrarySourceRootInvalid, DoctorResolutionLane.BlockedRepair,
                inventory.Source.Cause ?? "The Library source root is invalid.", Subject(record, DoctorSubjectKind.LibrarySourceRoot, inventory.Source),
                new DoctorStateEvidence(DoctorObservedState.Invalid));

            return;
        }

        if (inventory.Source.State == LibrarySourceRootState.Blocked)
        {
            Add(findings, DoctorFindingKind.LibrarySourceRootAliased, DoctorResolutionLane.BlockedRepair,
                inventory.Source.Cause ?? "The Library source root is physically aliased or unsafe.", Subject(record, DoctorSubjectKind.LibrarySourceRoot, inventory.Source),
                new DoctorStateEvidence(DoctorObservedState.Blocked));
            return;
        }

        if (inventory.Inventory is null || inventory.Inventory.State != LibraryInventoryState.Complete)
        {
            Add(findings, DoctorFindingKind.LibraryInventoryIncomplete, DoctorResolutionLane.Informational,
                inventory.Inventory?.Cause ?? inventory.UnavailablePaths.FirstOrDefault()?.Cause ?? "The Library inventory is incomplete.",
                Subject(record, DoctorSubjectKind.LibrarySourceRoot, inventory.Source),
                new DoctorStateEvidence(DoctorObservedState.Incomplete));
            return;
        }

        var eligible = inventory.Inventory.Entries.Select(entry => entry.SourcePath.Value).ToHashSet(StringComparer.Ordinal);
        foreach (var path in record.Paths.Where(path => !eligible.Contains(path.Value)))
        {
            var mapping = libraries.Mappings.SingleOrDefault(value => value.Mapping == LibraryPathIdentity.Map(record.SourceRoot, record.DestinationRoot, path));
            if (mapping?.State == LibraryMappingObservationState.Current)
            {
                Add(findings, DoctorFindingKind.LibraryProjectionDangling, DoctorResolutionLane.BlockedRepair,
                    "A registered current Library projection no longer has an eligible source inventory entry.",
                    Subject(record, DoctorSubjectKind.LibraryProjection, mapping),
                    new DoctorStateEvidence(DoctorObservedState.Changed));
            }
        }
    }

    private static void InspectMapping(
        LibraryDoctorView libraries,
        LibraryMappingObservation mapping,
        ICollection<DoctorFinding> findings)
    {
        var record = libraries.Record.Record?.Libraries.SingleOrDefault(value =>
            LibraryPathIdentity.Mappings(value).Any(registered => registered == mapping.Mapping));
        if (record is null)
        {
            return;
        }

        var descriptor = mapping.State switch
        {
            LibraryMappingObservationState.Current => ((DoctorFindingKind?)null, DoctorResolutionLane.Informational, DoctorObservedState.Current),
            LibraryMappingObservationState.Missing => (DoctorFindingKind.LibraryProjectionMissing, DoctorResolutionLane.ManualDecision, DoctorObservedState.Missing),
            LibraryMappingObservationState.Changed => (DoctorFindingKind.LibraryProjectionRetargeted, DoctorResolutionLane.BlockedRepair, DoctorObservedState.Changed),
            LibraryMappingObservationState.Blocked => (DoctorFindingKind.LibraryProjectionRetargeted, DoctorResolutionLane.BlockedRepair, DoctorObservedState.Blocked),
            LibraryMappingObservationState.Unavailable => (DoctorFindingKind.LibraryInventoryIncomplete, DoctorResolutionLane.Informational, DoctorObservedState.Unavailable),
            _ => throw new ArgumentOutOfRangeException(nameof(mapping), mapping.State, "The Library mapping state is not defined."),
        };
        if (descriptor.Item1 is { } kind)
        {
            Add(findings, kind, descriptor.Item2,
                mapping.Cause ?? "The registered Library projection is not current.",
                Subject(record, DoctorSubjectKind.LibraryProjection, mapping),
                new DoctorStateEvidence(descriptor.Item3));
        }
    }

    private static void InspectOwnership(LibraryDoctorView libraries, ICollection<DoctorFinding> findings)
    {
        if (libraries.Ownership is not { } ownership || libraries.Record.Record is not { } record)
        {
            return;
        }

        var recordsByPath = record.Libraries.SelectMany(library => LibraryPathIdentity.Mappings(library).Select(mapping => (library, mapping)))
            .ToDictionary(value => PortableWorkspacePath.CreatePortableKey(value.mapping.DestinationPath.Value), value => value.library, StringComparer.Ordinal);
        foreach (var claim in ownership.Claims)
        {
            if (!recordsByPath.TryGetValue(PortableWorkspacePath.CreatePortableKey(claim.Path), out var library))
            {
                continue;
            }

            Add(findings,
                claim.Manager == LifecycleOwnershipManager.Extension
                    ? DoctorFindingKind.LibraryExtensionCollision
                    : DoctorFindingKind.LibraryPathCollision,
                DoctorResolutionLane.ManualDecision,
                "A registered Library projection is also claimed by another managed domain.",
                Subject(library, DoctorSubjectKind.Library, library),
                new DoctorStateEvidence(DoctorObservedState.Invalid));
        }
    }

    private static DoctorSubject Subject(
        LibraryDoctorView libraries,
        DoctorSubjectKind kind,
        LibraryResidualEvidence? residual = null)
    {
        var record = residual is null
            ? libraries.Record.Record?.Libraries.FirstOrDefault()
            : libraries.Record.Record?.Libraries.FirstOrDefault(value => value.Id == residual.LibraryId);
        return residual is not null
            ? Subject(record, kind, residual)
            : record is not null
                ? Subject(record, kind, record)
                : PlainSubject(kind, ".agents/open-forge.libraries.json");
    }

    private static DoctorSubject Subject(
        LibraryRecord? record,
        DoctorSubjectKind kind,
        object fact)
    {
        if (record is null)
        {
            return PlainSubject(kind, ".agents/open-forge.libraries.json");
        }

        var library = fact switch
        {
            LibraryRecord registration => new DoctorLibrarySubject(kind, record.Id, registration, null, null, null, null),
            LibrarySourceRootObservation source => new DoctorLibrarySubject(kind, record.Id, null, source, null, null, null),
            LibraryMapping mapping => new DoctorLibrarySubject(kind, record.Id, null, null, mapping, null, null),
            LibraryMappingObservation projection => new DoctorLibrarySubject(kind, record.Id, null, null, null, projection, null),
            LibraryResidualEvidence residual => new DoctorLibrarySubject(kind, residual.LibraryId, null, null, null, null, residual),
            _ => throw new ArgumentOutOfRangeException(nameof(fact)),
        };
        return new DoctorSubject
        {
            Kind = kind,
            Path = kind == DoctorSubjectKind.LibraryResidual
                ? library.Residual?.Residual.Candidate.Path
                : kind == DoctorSubjectKind.LibrarySourceRoot
                    ? library.Source?.Request.SourceRoot.Value
                    : library.Projection?.LogicalDestinationPath,
            Identifier = library.LibraryId.Value,
            Location = null,
            Library = library,
        };
    }

    private static DoctorSubject PlainSubject(DoctorSubjectKind kind, string path)
        => new() { Kind = kind, Path = path, Identifier = null, Location = null };

    private static void Add(
        ICollection<DoctorFinding> findings,
        DoctorFindingKind kind,
        DoctorResolutionLane resolution,
        string message,
        DoctorSubject subject,
        DoctorEvidence evidence)
        => findings.Add(DoctorDomainSupport.Create(
            resolution == DoctorResolutionLane.Informational
                ? DoctorDomainSupport.Information(kind, message)
                : resolution == DoctorResolutionLane.BlockedRepair
                    ? DoctorDomainSupport.Error(kind, message, resolution)
                    : DoctorDomainSupport.Warning(kind, message, resolution),
            subject,
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.WorkspaceEntry,
                DoctorProvenanceSource.WorkspaceEntry,
                subject.Path),
            [evidence]));
}
