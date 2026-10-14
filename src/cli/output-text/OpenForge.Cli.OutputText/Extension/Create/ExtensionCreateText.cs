using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Create;

internal static class ExtensionCreateText
{
    // @OpenForgeText extension.create.heading.extension-id-lowercase-digits-and-hyphens
    internal static string HeadingExtensionIdLowercaseDigitsAndHyphens()
        => "Extension ID (for example my-tools):";

    // @OpenForgeText extension.create.message.use-lowercase-letters-digits-and-hyphens
    internal static string MessageUseLowercaseLettersDigitsAndHyphens()
        => "Use lowercase letters and digits, with single hyphens between them, up to 128 characters.";

    // @OpenForgeText extension.create.heading.package-folder
    internal static string HeadingPackageFolder()
        => "Existing parent folder for the new Extension:";

    internal static string PackageFolderExplanation(string id)
        => $"Open Forge creates a folder named {id} inside it.";

    // @OpenForgeText extension.create.message.enter-an-existing-ordinary-directory
    internal static string MessageEnterAnExistingOrdinaryDirectory()
        => "Enter an existing folder. Open Forge creates a folder named after the Extension ID inside it.";

    // @OpenForgeText extension.create.message.invalid-parent-folder
    internal static string InvalidParentFolder() => "That folder does not exist. Enter an existing folder.";

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
        => "open-forge extension create [<stable-id>] [options]";

    // @OpenForgeText extension.create.help.heading.required-input-and-interaction
    internal static string HelpHeadingRequiredInputAndInteraction()
        => "Required input and interaction";

    // @OpenForgeText extension.create.help.required-input-and-interaction
    internal static string HelpRequiredInputAndInteraction()
        => "Extension ID and --path are required to plan creation. Interactive text requests ask for missing values and let you correct invalid input.\nSupplying both required values avoids input questions. A writing plan asks for confirmation unless --automatic is supplied. Dry-run and a verified no-op need no write confirmation.\n--automatic, --format json, and redirected requests return invalid if either required value is missing. Non-interactive apply that would create files requires --automatic.";

    // @OpenForgeText extension.create.help.heading.manifest
    internal static string HelpHeadingManifest()
        => "Manifest";

    // @OpenForgeText extension.create.help.manifest
    internal static string HelpManifest()
        => "The default name splits the Extension ID on hyphens and uppercases each first ASCII letter.\n  Description is `Open Forge Extension package <stable-id>.`. Package version is `0.1.0`. Dependencies default to none.\n  --name, --description, and --package-version are nonblank values, each supplied once.\n  Repeat --dependency for valid distinct non-self IDs. Dependencies are written in ordinal order and availability is not resolved.";

    // @OpenForgeText extension.create.help.heading.catalogue-and-scaffold
    internal static string HelpHeadingCatalogueAndScaffold()
        => "Catalogue and scaffold";

    // @OpenForgeText extension.create.help.catalogue-and-scaffold
    internal static string HelpCatalogueAndScaffold()
        => "--path selects an existing local catalogue directory. It needs no marker, and the command never creates this parent. Only <catalogue>/<stable-id>/ is inspected.\n  A missing destination is created. An identical existing scaffold needs no writes.\n  Divergent, partial, additional, unknown, or colliding content blocks. The scaffold contains only extension.json and content/.agents/.";

    // @OpenForgeText extension.create.help.heading.modes-and-global-options
    internal static string HelpHeadingModesAndGlobalOptions()
        => "Creation modes";

    // @OpenForgeText extension.create.help.modes-and-global-options
    internal static string HelpModesAndGlobalOptions()
        => "Apply is the default. --dry-run previews the same exact plan without writes. --automatic suppresses interaction but adds no inferred input or authority. --workspace is accepted as a no-op. Result detail applies to both text and JSON.";

    // @OpenForgeText extension.create.help.examples
    internal static string HelpExamples()
        => "open-forge extension create my-tools --path packages --dry-run\nopen-forge extension create my-tools --path packages\nopen-forge extension create";

    // @OpenForgeText extension.create.help.heading.workspace-and-recovery-boundary
    internal static string HelpHeadingWorkspaceAndRecoveryBoundary()
        => "Workspace and recovery boundary";

    // @OpenForgeText extension.create.help.workspace-and-recovery-boundary
    internal static string HelpWorkspaceAndRecoveryBoundary()
        => "Extension Create never installs the package, mutates a workspace, writes lifecycle or generated-navigation state, acquires a workspace mutation lock, or creates a recovery bundle.\n  It uses a separate create-only destination path with no Replace or Delete and never restores, rolls back, or compensates for retained partial state.";

    // @OpenForgeText extension.create.extensioncreate.help.behavior
    internal static string HelpBehavior()
        => "The catalogue folder must already exist. Supply an Extension ID and --path, or answer the interactive questions.\nApply is the default. --dry-run shows the exact plan and writes nothing.\nA writing plan asks for confirmation unless --automatic is supplied.\nRedirected or JSON requests need both required values and --automatic to write.\nIf creation stops partway through, created files stay in the package folder. Review that folder before retrying. No workspace installation is created.";

    // @OpenForgeText extension.create.extensioncreate.help.relatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("extension inspect --help", "Compare a package with its installation."),
            ("extension install --help", "Install an Extension package."),
        ];
    // @OpenForgeText extension.create.help.details-1
    internal static string HelpInputAndScaffoldDetails()
        => "Interactive input lets you correct invalid values. Supplying the ID and --path avoids input questions. Automatic, JSON, or redirected input fails if either is missing. Dry run and verified no-ops need no write confirmation. Only <catalogue>/<stable-id>/ is inspected. No parent marker is required. A missing destination is created. An identical scaffold needs no writes. Divergent, partial, additional, unknown, or colliding content blocks. The scaffold contains only extension.json and content/.agents/.";

    // @OpenForgeText extension.create.help.details-2
    internal static string HelpManifestDetails()
        => "Name defaults to the ID split on hyphens, with each first ASCII letter uppercased. Description defaults to `Open Forge Extension package <stable-id>.`, version to `0.1.0`, and dependencies to none. Supply nonblank --name, --description, and --package-version at most once. Dependencies must be distinct valid non-self IDs. They are written in ordinal order without resolving availability.";

    // @OpenForgeText extension.create.help.details-3
    internal static string HelpBoundaryDetails()
        => "--automatic infers no input or authority. --workspace is a no-op. Create writes no lifecycle or generated navigation state, takes no workspace lock, and creates no recovery bundle. Its separate create-only destination never uses Replace or Delete, restores, rolls back, or compensates for retained partial state.";
}
