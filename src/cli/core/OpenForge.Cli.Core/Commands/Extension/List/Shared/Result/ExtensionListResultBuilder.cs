using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;

internal static class ExtensionListResultBuilder
{
    internal static ExtensionListResult Build(
        ExtensionListRequest request,
        ExtensionSourceReadResult source,
        LifecycleReadResult? lifecycle)
    {
        var findings = new List<ExtensionListFinding>();
        AddSourceFinding(request.Selection, source, findings);
        AddLifecycleFinding(lifecycle, findings);
        var installed = ExtensionListRowBuilder.CreateInstalledRows(request.Selection, source, lifecycle, findings);
        var available = request.Selection.Available && source.State == ExtensionSourceReadState.Complete
            ? ExtensionListRowBuilder.CreateAvailableRows(source.Packages)
            : [];
        var status = ExtensionListStatusPolicy.ReadStatus(findings);
        return new ExtensionListResult(
            status: status,
            workspace: request.Workspace,
            selection: request.Selection,
            source: new ExtensionListSource
            {
                Identity = source.Identity,
                Kind = source.Kind,
                State = source.State,
            },
            lifecycleTrust: lifecycle?.Trust,
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
                    State = ExtensionSourceReadState.Unavailable,
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
            findings.Add(finding);
        }
    }

    private static void AddLifecycleFinding(
        LifecycleReadResult? lifecycle,
        ICollection<ExtensionListFinding> findings)
    {
        if (lifecycle is null || lifecycle.Trust is LifecycleExtensionTrust.Trusted or LifecycleExtensionTrust.Absent)
        {
            return;
        }

        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            findings.Add(new ExtensionListFinding(
                code: ExtensionListFindingCode.Interrupted,
                status: CliSemanticStatus.Interrupted,
                subject: LifecycleSchema.RelativePath,
                cause: lifecycle.Cause ?? "Lifecycle inspection was interrupted."));
            return;
        }

        findings.Add(new ExtensionListFinding(
            code: lifecycle.Trust == LifecycleExtensionTrust.Blocked
                ? ExtensionListFindingCode.LifecycleBlocked
                : ExtensionListFindingCode.LifecycleUnavailable,
            status: lifecycle.Trust == LifecycleExtensionTrust.Blocked
                ? CliSemanticStatus.Blocked
                : CliSemanticStatus.Incomplete,
            subject: LifecycleSchema.RelativePath,
            cause: lifecycle.Cause ?? "Extension lifecycle coverage is unavailable."));
    }

    private static ExtensionListCoverage ReadInstalledCoverage(
        ExtensionListSelection selection,
        LifecycleReadResult? lifecycle)
    {
        if (!selection.Installed)
        {
            return ExtensionListCoverage.NotRequested;
        }

        return lifecycle?.Trust switch
        {
            LifecycleExtensionTrust.Trusted or LifecycleExtensionTrust.Absent => ExtensionListCoverage.Complete,
            LifecycleExtensionTrust.Blocked => ExtensionListCoverage.Blocked,
            _ => ExtensionListCoverage.Incomplete,
        };
    }

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
