using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

using OpenForge.Cli.Core.Commands.References.Models.Result;

namespace OpenForge.Cli.Core.Commands.References;

/// <summary>Command-local identity and finite finding vocabulary for References.</summary>
internal static class ReferencesDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "references";

    internal const string In = "in";
    internal const string Out = "out";
    internal const string Both = "both";

    internal static readonly CliSyntaxDefinition ReferencesCommand = new(
        CommandIdentity,
        "Show direct incoming and outgoing references.");

    internal static readonly CliSyntaxDefinition Source = new(
        "source-reference",
        "Select one source ID or exact .agents/... path.");

    internal static readonly CliOptionDefinition<string?> Direction = new(
        "--direction",
        "Select incoming, outgoing, or both direct reference sections.",
        CliOptionArity.ExactlyOne,
        null,
        "in|out|both");

    internal static readonly CliOptionDefinition<string[]> Include = new(
        "--include",
        "Include one source reference in the incoming scan.",
        CliOptionArity.ExactlyOne,
        [],
        "source-reference");

    internal static readonly CliOptionDefinition<string[]> Exclude = new(
        "--exclude",
        "Exclude one source reference from the incoming scan.",
        CliOptionArity.ExactlyOne,
        [],
        "source-reference");

    internal static readonly IReadOnlyList<ReferencesFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<ReferencesFindingCode>());

    internal static ReferencesFindingDefinition Read(ReferencesFindingCode code)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The References finding code is not defined.");
        }

        return code switch
        {
            ReferencesFindingCode.InvalidInput => Definition(code, "references.invalid-input", CliSemanticStatus.Invalid),
            ReferencesFindingCode.InvalidSource => Definition(code, "references.invalid-source", CliSemanticStatus.Invalid),
            ReferencesFindingCode.InvalidDirection => Definition(code, "references.invalid-direction", CliSemanticStatus.Invalid),
            ReferencesFindingCode.InvalidFilter => Definition(code, "references.invalid-filter", CliSemanticStatus.Invalid),
            ReferencesFindingCode.WorkspaceUnavailable => Definition(code, "references.workspace-unavailable", CliSemanticStatus.Blocked),
            ReferencesFindingCode.WorkspaceUnsafe => Definition(code, "references.workspace-unsafe", CliSemanticStatus.Blocked),
            ReferencesFindingCode.SourceAmbiguous => Definition(code, "references.source-ambiguous", CliSemanticStatus.Blocked),
            ReferencesFindingCode.SourceUnsafe => Definition(code, "references.source-unsafe", CliSemanticStatus.Blocked),
            ReferencesFindingCode.SelectorAmbiguous => Definition(code, "references.selector-ambiguous", CliSemanticStatus.Blocked),
            ReferencesFindingCode.SelectorUnsafe => Definition(code, "references.selector-unsafe", CliSemanticStatus.Blocked),
            ReferencesFindingCode.IdentityCollision => Definition(code, "references.identity-collision", CliSemanticStatus.Attention),
            ReferencesFindingCode.PhysicalAlias => Definition(code, "references.physical-alias", CliSemanticStatus.Blocked),
            ReferencesFindingCode.IdentityUnavailable => Definition(code, "references.identity-unavailable", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.CandidateUnsafe => Definition(code, "references.candidate-unsafe", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.LayerUnresolved => Definition(code, "references.layer-unresolved", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.InspectionUnavailable => Definition(code, "references.inspection-unavailable", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.InvalidEncoding => Definition(code, "references.invalid-encoding", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.LinkEncodingInvalid => Definition(code, "references.link-encoding-invalid", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.GeneratedRegionUnavailable => Definition(code, "references.generated-region-unavailable", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.DestinationMalformed => Definition(code, "references.destination-malformed", CliSemanticStatus.Attention),
            ReferencesFindingCode.DestinationUnsupported => Definition(code, "references.destination-unsupported", CliSemanticStatus.Attention),
            ReferencesFindingCode.TargetMissing => Definition(code, "references.target-missing", CliSemanticStatus.Attention),
            ReferencesFindingCode.FragmentMissing => Definition(code, "references.fragment-missing", CliSemanticStatus.Attention),
            ReferencesFindingCode.TargetUnsafe => Definition(code, "references.target-unsafe", CliSemanticStatus.Blocked),
            ReferencesFindingCode.TargetAmbiguous => Definition(code, "references.target-ambiguous", CliSemanticStatus.Blocked),
            ReferencesFindingCode.TargetUnreadable => Definition(code, "references.target-unreadable", CliSemanticStatus.Incomplete),
            ReferencesFindingCode.OperationFailed => Definition(code, "references.operation-failed", CliSemanticStatus.Failed),
            ReferencesFindingCode.Interrupted => Definition(code, "references.interrupted", CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The References finding code is not defined."),
        };
    }

    internal static string ReadMachineName(ReferencesFindingCode code) => Read(code).MachineName;

    internal static CliSemanticStatus ReadStatus(ReferencesFindingCode code) => Read(code).Status;

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<Models.Result.ReferencesFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        if (status == CliSemanticStatus.Complete)
        {
            return null;
        }

        return status switch
        {
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge references --help",
                "Correct the named References input, then rerun the request."),
            CliSemanticStatus.Blocked when findings.Any(finding =>
                    finding.Code == ReferencesFindingCode.SourceAmbiguous
                    || finding.Code == ReferencesFindingCode.SelectorAmbiguous)
                && findings.First(finding => IsBlocked(finding.Code)).Code is
                    ReferencesFindingCode.SourceAmbiguous or ReferencesFindingCode.SelectorAmbiguous
                => new CliNextAction(
                    CommandLines.References,
                    "Replace the ambiguous source or selector with one listed exact path, then rerun the request."),
            CliSemanticStatus.Blocked => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the blocked workspace, source, selector, generated-region, or target boundary before rerunning References."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the unavailable source, generated-region, or target facts before relying on this References result."),
            CliSemanticStatus.Attention => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the reported reference or identity findings before relying on this References result."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge references --detail debug",
                "Report the failure and retry the same References request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                CommandLines.References,
                "Rerun the same References request."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The References status is not defined."),
        };
    }

    private static bool IsBlocked(ReferencesFindingCode code)
        => ReadStatus(code) == CliSemanticStatus.Blocked;

    private static ReferencesFindingDefinition Definition(
        ReferencesFindingCode code,
        string machineName,
        CliSemanticStatus status)
        => new(code, machineName, status);
}

internal sealed record ReferencesFindingDefinition(
    ReferencesFindingCode Code,
    string MachineName,
    CliSemanticStatus Status);
