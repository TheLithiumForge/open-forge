using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal static class ExtensionCreateDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension create";
    internal const string ManifestFileName = "extension.json";
    internal const string PayloadDirectoryName = "payload";
    internal const string AgentsDirectoryName = ".agents";
    internal const string DefaultPackageVersion = "0.1.0";

    internal static readonly CliSyntaxDefinition CreateCommand = new(
        name: "create",
        description: "Create one local Extension package scaffold.");

    internal static readonly CliOptionDefinition<string?> Path = new(
        name: "--path",
        description: "Select the exact destination catalogue parent.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "catalogue-path");

    internal static readonly CliOptionDefinition<string?> Name = new(
        name: "--name",
        description: "Override the package display name.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "text");

    internal static readonly CliOptionDefinition<string?> Description = new(
        name: "--description",
        description: "Override the package description.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "text");

    internal static readonly CliOptionDefinition<string?> PackageVersion = new(
        name: "--package-version",
        description: "Override the descriptive package version.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "text");

    internal static readonly CliOptionDefinition<string[]> Dependency = new(
        name: "--dependency",
        description: "Record one stable Extension dependency ID; repeat for additional dependencies.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: [],
        valueName: "stable-id");

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        name: "--automatic",
        description: "Disable prompting after all required inputs are explicit.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        name: "--dry-run",
        description: "Preview the exact scaffold plan without writing.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliNextAction InvalidNext = new(
        "open-forge extension create --help",
        "Correct the named Extension Create input, then rerun the request.");

    internal static readonly CliNextAction IncompleteNext = new(
        "open-forge extension create --verbose",
        "Inspect the unavailable catalogue or filesystem fact before rerunning Extension Create.");

    internal static readonly CliNextAction BlockedNext = new(
        "open-forge extension create --dry-run",
        "Resolve the named catalogue identity or destination collision before rerunning Extension Create.");

    internal static readonly CliNextAction FailedNext = new(
        "open-forge extension create --verbose",
        "Inspect the retained destination state before retrying Extension Create.");

    internal static readonly CliNextAction InterruptedNext = new(
        "open-forge extension create",
        "Inspect the retained destination state, then rerun Extension Create if appropriate.");

    internal static string ReadMachineName(ExtensionCreateMode mode)
        => mode switch
        {
            ExtensionCreateMode.Apply => "apply",
            ExtensionCreateMode.DryRun => "dry-run",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Extension Create mode is not defined."),
        };

    internal static string ReadFindingCode(ExtensionCreateFindingCode code)
        => code switch
        {
            ExtensionCreateFindingCode.InvalidInput => "extension-create.invalid-input",
            ExtensionCreateFindingCode.CatalogueUnavailable => "extension-create.catalogue-unavailable",
            ExtensionCreateFindingCode.CatalogueUnsafe => "extension-create.catalogue-unsafe",
            ExtensionCreateFindingCode.DestinationCollision => "extension-create.destination-collision",
            ExtensionCreateFindingCode.DestinationChanged => "extension-create.destination-changed",
            ExtensionCreateFindingCode.ApplicationFailed => "extension-create.application-failed",
            ExtensionCreateFindingCode.VerificationFailed => "extension-create.verification-failed",
            ExtensionCreateFindingCode.Interrupted => "extension-create.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Create finding code is not defined."),
        };

    internal static string ReadEffectKind(Models.Planning.ExtensionCreateEffectKind kind)
        => kind switch
        {
            Models.Planning.ExtensionCreateEffectKind.ManifestFile => "manifest",
            Models.Planning.ExtensionCreateEffectKind.PayloadAgentsDirectory => "payload-agents-directory",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension Create effect kind is not defined."),
        };

    internal static string ReadVerificationState(ExtensionCreateVerificationState state)
        => state switch
        {
            ExtensionCreateVerificationState.NotStarted => "not-started",
            ExtensionCreateVerificationState.Planned => "planned",
            ExtensionCreateVerificationState.Verified => "verified",
            ExtensionCreateVerificationState.Failed => "failed",
            ExtensionCreateVerificationState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension Create verification state is not defined."),
        };

    internal static CliNextAction? ReadNext(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => IncompleteNext,
            CliSemanticStatus.Invalid => InvalidNext,
            CliSemanticStatus.Blocked => BlockedNext,
            CliSemanticStatus.Failed => FailedNext,
            CliSemanticStatus.Interrupted => InterruptedNext,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Extension Create status is not defined."),
        };
}
