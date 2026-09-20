using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Inspect;

internal static class ExtensionInspectText
{
    // @OpenForgeText extension.inspect.title.registered-in
    internal static string TitleRegisteredIn()
        => "Registered in";

    // @OpenForgeText extension.inspect.label.new-in-the-package
    internal static string LabelNewInThePackage()
        => "new in the package";

    // @OpenForgeText extension.inspect.label.could-not-be-compared
    internal static string LabelCouldNotBeCompared()
        => "could not be compared";

    // @OpenForgeText extension.inspect.label.candidate-package
    internal static string LabelCandidatePackage()
        => "candidate package";

    // @OpenForgeText extension.inspect.label.file-unchanged
    internal static string LabelFileUnchanged()
        => "file unchanged";

    // @OpenForgeText extension.inspect.label.file-changed
    internal static string LabelFileChanged()
        => "file changed";

    // @OpenForgeText extension.inspect.label.file-missing
    internal static string LabelFileMissing()
        => "file missing";

    // @OpenForgeText extension.inspect.label.file-new
    internal static string LabelFileNew()
        => "file new";

    // @OpenForgeText extension.inspect.label.files-new
    internal static string LabelFilesNew()
        => "files new";

    // @OpenForgeText extension.inspect.label.file-retired
    internal static string LabelFileRetired()
        => "file retired";

    // @OpenForgeText extension.inspect.message.extension-inspect-was-cancelled
    internal static string MessageExtensionInspectWasCancelled()
        => "Extension inspect was cancelled.";

    // @OpenForgeText extension.inspect.label.bundled-with-this-cli
    internal static string LabelBundledWithThisCli()
        => "bundled with this CLI";

    // @OpenForgeText extension.inspect.label.new-in-the-package-not-installed-yet
    internal static string LabelNewInThePackageNotInstalledYet()
        => "new in the package; not installed yet";

    // @OpenForgeText extension.inspect.title.invalid-extension-id
    internal static string TitleInvalidExtensionId()
        => "Invalid Extension ID";

    // @OpenForgeText extension.inspect.title.extension-source-is-inside-the-workspace
    internal static string TitleExtensionSourceIsInsideTheWorkspace()
        => "Extension source is inside the workspace";

    // @OpenForgeText extension.inspect.title.extension-source-is-ambiguous
    internal static string TitleExtensionSourceIsAmbiguous()
        => "Extension source is ambiguous";

    // @OpenForgeText extension.inspect.title.extension-identity-is-ambiguous
    internal static string TitleExtensionIdentityIsAmbiguous()
        => "Extension identity is ambiguous";

    // @OpenForgeText extension.inspect.title.extension-package-is-unavailable
    internal static string TitleExtensionPackageIsUnavailable()
        => "Extension package is unavailable";

    // @OpenForgeText extension.inspect.title.extension-package-is-invalid
    internal static string TitleExtensionPackageIsInvalid()
        => "Extension package is invalid";

    // @OpenForgeText extension.inspect.title.dependency-could-not-be-resolved
    internal static string TitleDependencyCouldNotBeResolved()
        => "Dependency could not be resolved";

    // @OpenForgeText extension.inspect.title.dependency-cycle
    internal static string TitleDependencyCycle()
        => "Dependency cycle";

    // @OpenForgeText extension.inspect.title.dependency-conflict
    internal static string TitleDependencyConflict()
        => "Dependency conflict";

    // @OpenForgeText extension.inspect.title.file-could-not-be-read
    internal static string TitleFileCouldNotBeRead()
        => "File could not be read";

    // @OpenForgeText extension.inspect.title.file-path-is-invalid
    internal static string TitleFilePathIsInvalid()
        => "File path is invalid";

    // @OpenForgeText extension.inspect.title.file-could-not-be-compared
    internal static string TitleFileCouldNotBeCompared()
        => "File could not be compared";

    // @OpenForgeText extension.inspect.title.file-comparison-used-bytes
    internal static string TitleFileComparisonUsedBytes()
        => "File comparison used bytes";

    // @OpenForgeText extension.inspect.title.generated-region-is-invalid
    internal static string TitleGeneratedRegionIsInvalid()
        => "Generated region is invalid";

    // @OpenForgeText extension.inspect.title.dependencies-changed
    internal static string TitleDependenciesChanged()
        => "Dependencies changed";

    // @OpenForgeText extension.inspect.title.file-changed
    internal static string TitleFileChanged()
        => "File changed";

    // @OpenForgeText extension.inspect.title.file-is-missing
    internal static string TitleFileIsMissing()
        => "File is missing";

    // @OpenForgeText extension.inspect.title.file-is-new
    internal static string TitleFileIsNew()
        => "File is new";

    // @OpenForgeText extension.inspect.title.file-is-retired
    internal static string TitleFileIsRetired()
        => "File is retired";

    // @OpenForgeText extension.inspect.title.extension-inspect-failed
    internal static string TitleExtensionInspectFailed()
        => "Extension inspect failed";

    // @OpenForgeText extension.inspect.title.extension-inspect-was-cancelled
    internal static string TitleExtensionInspectWasCancelled()
        => "Extension inspect was cancelled";

    // @OpenForgeText extension.inspect.message.no-ownership-record-exists-so-installation-cannot-be-checked
    internal static string MessageNoOwnershipRecordExistsSoInstallationCannotBeChecked()
        => "No ownership record exists, so installation cannot be checked.";

    // @OpenForgeText extension.inspect.message.list-available-extensions
    internal static string MessageListAvailableExtensions()
        => "List available Extensions.";

    // @OpenForgeText extension.inspect.message.inspect-the-recorded-workspace-facts
    internal static string MessageInspectTheRecordedWorkspaceFacts()
        => "Inspect the recorded workspace facts.";

    // @OpenForgeText extension.inspect.message.review-the-retired-files-before-pruning-them
    internal static string MessageReviewTheRetiredFilesBeforePruningThem()
        => "Review the retired files before pruning them.";

    // @OpenForgeText extension.inspect.message.review-the-selected-package-changes-before-applying-them
    internal static string MessageReviewTheSelectedPackageChangesBeforeApplyingThem()
        => "Review the selected package changes before applying them.";

    // @OpenForgeText extension.inspect.title.extension
    internal static string TitleExtension()
        => "Extension";

    // @OpenForgeText extension.inspect.label.some-facts-were-unavailable
    internal static string LabelSomeFactsWereUnavailable()
        => "some facts were unavailable";

    // @OpenForgeText extension.inspect.label.the-request
    internal static string LabelTheRequest()
        => "the request";

    // @OpenForgeText extension.inspect.label.the-request-is-blocked
    internal static string LabelTheRequestIsBlocked()
        => "the request is blocked";

    // @OpenForgeText extension.inspect.label.a-dependency
    internal static string LabelADependency()
        => "a dependency";

    // @OpenForgeText extension.inspect.title.dependencies
    internal static string TitleDependencies()
        => "Dependencies";

    // @OpenForgeText extension.inspect.title.files
    internal static string TitleFiles()
        => "Files";

    // @OpenForgeText extension.inspect.title.cause
    internal static string TitleCause()
        => "Cause";

    // @OpenForgeText extension.inspect.title.package
    internal static string TitlePackage()
        => "Package";

    // @OpenForgeText extension.inspect.title.dependency
    internal static string TitleDependency()
        => "Dependency";

    // @OpenForgeText extension.inspect.title.path
    internal static string TitlePath()
        => "Path";

    // @OpenForgeText extension.inspect.label.dependency
    internal static string LabelDependency()
        => "dependency";

    // @OpenForgeText extension.inspect.label.dependencies
    internal static string LabelDependencies()
        => "dependencies";

    // @OpenForgeText extension.inspect.help.syntax
    internal static string HelpSyntax()
        => "open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global options]";

    // @OpenForgeText extension.inspect.help.heading.subject-and-source
    internal static string HelpHeadingSubjectAndSource()
        => "Subject and source";

    // @OpenForgeText extension.inspect.help.subject-and-source
    internal static string HelpSubjectAndSource()
        => "Supply one exact lowercase stable ID. Available package details come from the bundled catalogue, or only from the separate local package or catalogue named by --source. There is no fallback, network, registry, cache, or approximate ID matching.";

    // @OpenForgeText extension.inspect.help.inspection
    internal static string HelpInspection()
        => "Inspect reports ownership, available package, dependency, path, current-byte, generated-boundary, and current-versus-intended comparison facts. It never writes, mutates, executes package content, or invokes another command.";

    // @OpenForgeText extension.inspect.help.examples
    internal static string HelpExamples()
        => "open-forge extension inspect development-toolkit\n  open-forge extension inspect development-toolkit --source ./packages/toolkit --format json";

    // @OpenForgeText extension.inspect.help.heading.fingerprint-boundary
    internal static string HelpHeadingFingerprintBoundary()
        => "Fingerprint boundary";

    // @OpenForgeText extension.inspect.help.fingerprint-boundary
    internal static string HelpFingerprintBoundary()
        => "open-forge-markdown-v1 uses strict UTF-8, LF-only normalization, heading-bounded Entries exclusion, and exact-byte fallback. Current and intended hashes describe this comparison; no stored baseline is read.";
}
