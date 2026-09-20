using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Create;

internal static class ExtensionCreateText
{
    // @OpenForgeText extension.create.heading.extension-id-lowercase-digits-and-hyphens
    internal static string HeadingExtensionIdLowercaseDigitsAndHyphens()
        => "Extension ID (lowercase, digits and hyphens):";

    // @OpenForgeText extension.create.message.use-lowercase-letters-digits-and-hyphens
    internal static string MessageUseLowercaseLettersDigitsAndHyphens()
        => "Use lowercase letters, digits and hyphens.";

    // @OpenForgeText extension.create.heading.package-folder
    internal static string HeadingPackageFolder()
        => "Package folder:";

    // @OpenForgeText extension.create.message.enter-an-existing-ordinary-directory
    internal static string MessageEnterAnExistingOrdinaryDirectory()
        => "Enter an existing ordinary directory.";

    // @OpenForgeText extension.create.message.extension-create-was-cancelled-nothing-was-changed
    internal static string MessageExtensionCreateWasCancelledNothingWasChanged()
        => "Extension create was cancelled. Nothing was changed.";

    // @OpenForgeText extension.create.message.edit-extension-json-then-add-files-under-content-agents
    internal static string MessageEditExtensionJsonThenAddFilesUnderContentAgents()
        => "Edit extension.json, then add files under content/.agents/.";

    // @OpenForgeText extension.create.heading.manifest-content
    internal static string HeadingManifestContent()
        => "manifest content:";

    // @OpenForgeText extension.create.label.the-package-folder
    internal static string LabelThePackageFolder()
        => "the package folder";

    // @OpenForgeText extension.create.label.the-destination-changed-after-the-plan-was-made
    internal static string LabelTheDestinationChangedAfterThePlanWasMade()
        => "the destination changed after the plan was made";

    // @OpenForgeText extension.create.message.the-extension-create-result-did-not-contain-a-finding
    internal static string MessageTheExtensionCreateResultDidNotContainAFinding()
        => "The Extension Create result did not contain a finding.";

    // @OpenForgeText extension.create.title.extension-create
    internal static string TitleExtensionCreate()
        => "Extension create";

    // @OpenForgeText extension.create.message.no-id-was-given-and-this-session-cannot-ask
    internal static string MessageNoIdWasGivenAndThisSessionCannotAsk()
        => "no ID was given, and this session cannot ask.";

    // @OpenForgeText extension.create.message.no-path-was-given-and-this-session-cannot-ask
    internal static string MessageNoPathWasGivenAndThisSessionCannotAsk()
        => "no --path was given, and this session cannot ask.";

    // @OpenForgeText extension.create.label.it-is-inside-the-workspace-s-agents
    internal static string LabelItIsInsideTheWorkspaceSAgents()
        => "it is inside the workspace's .agents";

    // @OpenForgeText extension.create.title.create-these-files-y-n
    internal static string TitleCreateTheseFilesYN()
        => "Create these files? [y/N]";

    // @OpenForgeText extension.create.title.catalogue-could-not-be-read
    internal static string TitleCatalogueCouldNotBeRead()
        => "Catalogue could not be read";

    // @OpenForgeText extension.create.title.catalogue-is-unsafe
    internal static string TitleCatalogueIsUnsafe()
        => "Catalogue is unsafe";

    // @OpenForgeText extension.create.title.destination-has-other-content
    internal static string TitleDestinationHasOtherContent()
        => "Destination has other content";

    // @OpenForgeText extension.create.title.destination-changed
    internal static string TitleDestinationChanged()
        => "Destination changed";

    // @OpenForgeText extension.create.title.extension-create-was-cancelled
    internal static string TitleExtensionCreateWasCancelled()
        => "Extension create was cancelled";

    // @OpenForgeText extension.create.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension create [<stable-id>] [--path <catalogue-path>]\n    [--name <text>] [--description <text>] [--package-version <text>]\n    [--dependency <stable-id>]... [--automatic] [--dry-run] [global options]";

    // @OpenForgeText extension.create.help.heading.required-input-and-interaction
    internal static string HelpHeadingRequiredInputAndInteraction()
        => "Required input and interaction";

    // @OpenForgeText extension.create.help.required-input-and-interaction
    internal static string HelpRequiredInputAndInteraction()
        => "Stable ID and --path are required to plan creation. Interactive text requests ask for missing values and let you correct invalid input.\n  Supplying both required values avoids prompts. --automatic, --format json, and redirected requests return invalid if either value is missing.";

    // @OpenForgeText extension.create.help.heading.manifest
    internal static string HelpHeadingManifest()
        => "Manifest";

    // @OpenForgeText extension.create.help.manifest
    internal static string HelpManifest()
        => "The default name splits the stable ID on hyphens and uppercases each first ASCII letter.\n  Description is `Open Forge Extension package <stable-id>.`; package version is `0.1.0`; dependencies default to none.\n  --name, --description, and --package-version are nonblank values, each supplied once.\n  Repeat --dependency for valid distinct non-self IDs; dependencies are written in ordinal order and availability is not resolved.";

    // @OpenForgeText extension.create.help.heading.catalogue-and-scaffold
    internal static string HelpHeadingCatalogueAndScaffold()
        => "Catalogue and scaffold";

    // @OpenForgeText extension.create.help.catalogue-and-scaffold
    internal static string HelpCatalogueAndScaffold()
        => "--path selects an existing local catalogue directory. It needs no marker, and the command never creates this parent. Only <catalogue>/<stable-id>/ is inspected.\n  A missing destination is created. An identical existing scaffold needs no writes.\n  Divergent, partial, additional, unknown, or colliding content blocks. The scaffold contains only extension.json and content/.agents/.";

    // @OpenForgeText extension.create.help.heading.modes-and-global-options
    internal static string HelpHeadingModesAndGlobalOptions()
        => "Modes and global options";

    // @OpenForgeText extension.create.help.modes-and-global-options
    internal static string HelpModesAndGlobalOptions()
        => "Apply is the default. --dry-run previews the same exact plan without writes; --automatic suppresses interaction but adds no inferred input or authority.\n  --workspace is accepted as a no-op. --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version retain their shared meaning; --detail selects detail in text and JSON.";

    // @OpenForgeText extension.create.help.examples
    internal static string HelpExamples()
        => "open-forge extension create\n  open-forge extension create development-toolkit --path D:/packages/open-forge --automatic --dry-run\n  open-forge extension create development-toolkit --path D:/packages/open-forge --name \"Development Toolkit\" --description \"Adds development workflows\"\n  open-forge extension create development-toolkit --path D:/packages/open-forge --package-version 0.2.0 --dependency shared-prompts";

    // @OpenForgeText extension.create.help.heading.workspace-and-recovery-boundary
    internal static string HelpHeadingWorkspaceAndRecoveryBoundary()
        => "Workspace and recovery boundary";

    // @OpenForgeText extension.create.help.workspace-and-recovery-boundary
    internal static string HelpWorkspaceAndRecoveryBoundary()
        => "Extension Create never installs the package, mutates a workspace, writes lifecycle or generated-navigation state, acquires a workspace mutation lock, or creates a recovery bundle.\n  It uses a separate create-only destination path with no Replace or Delete and never restores, rolls back, or compensates for retained partial state.";
}
