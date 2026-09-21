using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;

internal static class ExtensionListResultBuilder
{
    internal static ExtensionListResult Build(
        ExtensionListRequest request,
        ExtensionSourceReadResult source,
        WorkspaceOwnershipRead? lifecycle,
        ExtensionLifecycleDoctorView? doctor = null)
    {
        if (lifecycle is { IsTrustworthy: true }
            && lifecycle.Document.Extensions.GroupBy(package => package.Id, StringComparer.Ordinal).Any(group => group.Count() != 1))
        {
            lifecycle = lifecycle with
            {
                State = WorkspaceOwnershipReadState.Invalid,
                Document = OpenForge.Cli.Core.Framework.Ownership.Models.Document.WorkspaceOwnershipDocument.Empty,
                Cause = "Recorded Extension identities are ambiguous; no installed packages can be established.",
            };
            doctor = null;
        }
        var findings = new List<ExtensionListFinding>();
        AddSourceFinding(request.Selection, source, findings);
        AddLifecycleFinding(lifecycle, findings);
        var installed = ExtensionListRowBuilder.CreateInstalledRows(request.Selection, source, lifecycle, findings, doctor);
        var available = request.Selection.Available && source.State == ExtensionSourceReadState.Complete
            ? ExtensionListRowBuilder.CreateAvailableRows(source.Packages, installed)
            : [];
        var status = ExtensionListStatusPolicy.ReadStatus(findings);
        return new ExtensionListResult(
            status: status,
            workspace: request.Workspace,
            selection: request.Selection,
            source: new ExtensionListSource
            {
                Identity = source.Identity,
                Kind = source.Kind switch
                {
                    null => null,
                    ExtensionSourceKind.EmbeddedCatalogue => ExtensionListSourceKind.EmbeddedCatalogue,
                    ExtensionSourceKind.Package => ExtensionListSourceKind.Package,
                    ExtensionSourceKind.Catalogue => ExtensionListSourceKind.Catalogue,
                    _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "The Extension source kind is not defined."),
                },
                State = source.State switch
                {
                    ExtensionSourceReadState.Complete => ExtensionListSourceState.Complete,
                    ExtensionSourceReadState.Missing => ExtensionListSourceState.Missing,
                    ExtensionSourceReadState.Invalid => ExtensionListSourceState.Invalid,
                    ExtensionSourceReadState.Blocked => ExtensionListSourceState.Blocked,
                    ExtensionSourceReadState.Unavailable => ExtensionListSourceState.Unavailable,
                    ExtensionSourceReadState.Cancelled => ExtensionListSourceState.Interrupted,
                    _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The Extension source state is not defined."),
                },
            },
            lifecycleTrust: ReadTrust(lifecycle),
            installedCoverage: ReadInstalledCoverage(request.Selection, lifecycle),
            availableCoverage: ReadAvailableCoverage(request.Selection, source.State),
            installed: installed,
            available: available,
            findings: findings,
            next: ExtensionListStatusPolicy.ReadNext(status));
    }

    internal static ExtensionListResult Event(
        ExtensionListRequest request,
        CliSemanticStatus status,
        ExtensionListFindingCode code,
        string cause)
        => new(
            status: status,
            workspace: request.Workspace,
            selection: request.Selection,
            source: request.ExplicitSource is null
                ? null
                : new ExtensionListSource
                {
                    Identity = request.ExplicitSource,
                    Kind = null,
                    State = ExtensionListSourceState.Unavailable,
                },
            lifecycleTrust: null,
            installedCoverage: request.Selection.Installed ? ExtensionListCoverage.Incomplete : ExtensionListCoverage.NotRequested,
            availableCoverage: request.Selection.Available ? ExtensionListCoverage.Incomplete : ExtensionListCoverage.NotRequested,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    code: code,
                    status: status,
                    subject: null,
                    cause: cause),
            ],
            next: ExtensionListStatusPolicy.ReadNext(status));

    private static void AddSourceFinding(
        ExtensionListSelection selection,
        ExtensionSourceReadResult source,
        ICollection<ExtensionListFinding> findings)
    {
        var finding = source.State switch
        {
            ExtensionSourceReadState.Complete => null,
            ExtensionSourceReadState.Missing or ExtensionSourceReadState.Unavailable => new ExtensionListFinding(
                code: ExtensionListFindingCode.SourceUnavailable,
                status: selection.Available ? CliSemanticStatus.Incomplete : CliSemanticStatus.Attention,
                subject: source.Identity,
                cause: source.Cause ?? "The selected Extension source is unavailable."),
            ExtensionSourceReadState.Invalid => new ExtensionListFinding(
                code: ExtensionListFindingCode.SourceInvalid,
                status: CliSemanticStatus.Invalid,
                subject: source.Identity,
                cause: source.Cause ?? "The selected Extension source is invalid."),
            ExtensionSourceReadState.Blocked => new ExtensionListFinding(
                code: ExtensionListFindingCode.SourceBlocked,
                status: CliSemanticStatus.Blocked,
                subject: source.Identity,
                cause: source.Cause ?? "The selected Extension source is blocked."),
            ExtensionSourceReadState.Cancelled => new ExtensionListFinding(
                code: ExtensionListFindingCode.Interrupted,
                status: CliSemanticStatus.Interrupted,
                subject: source.Identity,
                cause: source.Cause ?? "Extension source inspection was interrupted."),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The Extension source state is not defined."),
        };
        if (finding is not null)
        {
            findings.Add(finding with { FailureDetail = ProjectFailureDetail(source.FailureDetail) });
        }
    }

    private static ExtensionListSourceFailureDetail? ProjectFailureDetail(ExtensionSourceFailureDetail? detail)
        => detail is null ? null : new(detail.Path, detail.Kind switch
        {
            ExtensionSourceFailureDetailKind.InvalidManifest => ExtensionListSourceFailureDetailKind.InvalidManifest,
            ExtensionSourceFailureDetailKind.InvalidEncoding => ExtensionListSourceFailureDetailKind.InvalidEncoding,
            ExtensionSourceFailureDetailKind.AccessDenied => ExtensionListSourceFailureDetailKind.AccessDenied,
            ExtensionSourceFailureDetailKind.FileInUse => ExtensionListSourceFailureDetailKind.FileInUse,
            ExtensionSourceFailureDetailKind.InputOutput => ExtensionListSourceFailureDetailKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(detail), detail.Kind, "The Extension source failure detail kind is not defined."),
        });

    internal static ExtensionListSource ProjectSource(ExtensionSourceReadResult source)
        => new()
        {
            Identity = source.Identity,
            Kind = source.Kind switch
            {
                null => null,
                ExtensionSourceKind.EmbeddedCatalogue => ExtensionListSourceKind.EmbeddedCatalogue,
                ExtensionSourceKind.Package => ExtensionListSourceKind.Package,
                ExtensionSourceKind.Catalogue => ExtensionListSourceKind.Catalogue,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "The Extension source kind is not defined."),
            },
            State = source.State switch
            {
                ExtensionSourceReadState.Complete => ExtensionListSourceState.Complete,
                ExtensionSourceReadState.Missing => ExtensionListSourceState.Missing,
                ExtensionSourceReadState.Invalid => ExtensionListSourceState.Invalid,
                ExtensionSourceReadState.Blocked => ExtensionListSourceState.Blocked,
                ExtensionSourceReadState.Unavailable => ExtensionListSourceState.Unavailable,
                ExtensionSourceReadState.Cancelled => ExtensionListSourceState.Interrupted,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The Extension source state is not defined."),
            },
        };

    private static void AddLifecycleFinding(
        WorkspaceOwnershipRead? lifecycle,
        ICollection<ExtensionListFinding> findings)
    {
        if (lifecycle is null || lifecycle.IsTrustworthy) return;
        findings.Add(new ExtensionListFinding(
            code: ExtensionListFindingCode.OwnershipObservation,
            status: CliSemanticStatus.Complete,
            subject: WorkspaceOwnershipDefinitions.RelativePath,
            cause: lifecycle.Cause ?? "No Extension ownership is recorded; installed packages cannot be established."));
    }

    private static ExtensionListOwnershipTrust? ReadTrust(WorkspaceOwnershipRead? ownership)
        => ownership is null ? null
            : ownership.State == WorkspaceOwnershipReadState.Absent ? ExtensionListOwnershipTrust.Absent
            : !ownership.IsTrustworthy ? ExtensionListOwnershipTrust.Incomplete
            : ownership.Document.Extensions.IsEmpty ? ExtensionListOwnershipTrust.Absent
            : ExtensionListOwnershipTrust.Trusted;

    private static ExtensionListCoverage ReadInstalledCoverage(
        ExtensionListSelection selection,
        WorkspaceOwnershipRead? lifecycle)
        => !selection.Installed ? ExtensionListCoverage.NotRequested
            : lifecycle is { State: WorkspaceOwnershipReadState.Absent }
                ? ExtensionListCoverage.Complete
            : lifecycle is { IsTrustworthy: true }
                ? ExtensionListCoverage.Complete
            : ExtensionListCoverage.Incomplete;

    private static ExtensionListCoverage ReadAvailableCoverage(
        ExtensionListSelection selection,
        ExtensionSourceReadState sourceState)
    {
        if (!selection.Available)
        {
            return ExtensionListCoverage.NotRequested;
        }

        return sourceState switch
        {
            ExtensionSourceReadState.Complete => ExtensionListCoverage.Complete,
            ExtensionSourceReadState.Blocked => ExtensionListCoverage.Blocked,
            _ => ExtensionListCoverage.Incomplete,
        };
    }
}
