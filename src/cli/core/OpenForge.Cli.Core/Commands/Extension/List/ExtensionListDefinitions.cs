using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal static class ExtensionListDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension list";

    internal static readonly CliSyntaxDefinition ListCommand = new(
        name: "list",
        description: "List installed and available Extension packages.");

    internal static readonly CliOptionDefinition<bool> Installed = new(
        name: "--installed",
        description: "Show installed packages; combine with --available to show both.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> Available = new(
        name: "--available",
        description: "Show available packages; combine with --installed to show both.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<string?> Source = new(
        name: "--source",
        description: "Read one exact local package or catalogue source.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "package-or-catalogue-path");

    internal static string ReadFindingCode(ExtensionListFindingCode code)
        => code switch
        {
            ExtensionListFindingCode.InvalidInput => "extension-list.invalid-input",
            ExtensionListFindingCode.WorkspaceUnavailable => "extension-list.workspace-unavailable",
            ExtensionListFindingCode.SourceUnavailable => "extension-list.source-unavailable",
            ExtensionListFindingCode.SourceInvalid => "extension-list.source-invalid",
            ExtensionListFindingCode.SourceBlocked => "extension-list.source-blocked",
            ExtensionListFindingCode.LifecycleUnavailable => "extension-list.lifecycle-unavailable",
            ExtensionListFindingCode.LifecycleBlocked => "extension-list.lifecycle-blocked",
            ExtensionListFindingCode.OperationFailed => "extension-list.operation-failed",
            ExtensionListFindingCode.Interrupted => "extension-list.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension List finding code is not defined."),
        };

    internal static CliNextAction InvalidNext { get; } = new(
        command: "open-forge extension list --help",
        reason: "Correct the named Extension List input, then rerun the request.");

    internal static CliNextAction IncompleteNext { get; } = new(
        command: "open-forge doctor",
        reason: "Inspect the unavailable package-source or lifecycle facts before relying on this list.");

    internal static CliNextAction BlockedNext { get; } = new(
        command: "open-forge doctor",
        reason: "Resolve the blocked source, workspace, identity, or ownership boundary before rerunning Extension List.");

    internal static CliNextAction FailedNext { get; } = new(
        command: "open-forge extension list --verbose",
        reason: "Report the failure and retry Extension List with bounded diagnostics.");

    internal static CliNextAction InterruptedNext { get; } = new(
        command: "open-forge extension list",
        reason: "Rerun the same Extension List request.");
}
