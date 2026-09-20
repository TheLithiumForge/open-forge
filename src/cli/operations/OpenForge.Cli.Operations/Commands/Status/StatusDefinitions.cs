using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Status;

internal static class StatusDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "status";
    internal const string TokenEstimator = "ceiling-characters-divided-by-four";

    internal static CliSyntaxDefinition StatusCommand { get; } = new(
        CommandIdentity,
        "Show workspace status and recovery data.");

    internal static CliNextAction AttentionNextAction { get; } = new(
        CommandLines.Doctor,
        "Inspect the reported operational findings before relying on this Status result.");

    internal static CliNextAction IncompleteNextAction { get; } = new(
        CommandLines.Doctor,
        "Inspect the unavailable operational facts before relying on this Status result.");

    internal static CliNextAction InvalidNextAction { get; } = new(
        "open-forge status --help",
        "Correct the Status input, then rerun the request.");

    internal static CliNextAction BlockedNextAction { get; } = new(
        CommandLines.Status,
        "Resolve the blocked workspace boundary, then rerun the same Status request.");

    internal static CliNextAction FailedNextAction { get; } = new(
        "open-forge status --detail debug",
        "Report the failure and retry Status with bounded diagnostics.");

    internal static CliNextAction InterruptedNextAction { get; } = new(
        CommandLines.Status,
        "Rerun the same Status request.");

    internal static string ReadFindingCode(StatusFindingCode code) => StatusFindingVocabulary.ReadMachineName(code);

    internal static CliSemanticStatus ReadFindingStatus(StatusFindingCode code) => StatusFindingVocabulary.ReadStatus(code);
}
