using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;

internal static class ExtensionInstallWording
{
    internal static string Selection() => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.PromptWhichExtensionsDoYouWantToInstall();

    internal static string ReplaceExisting(IReadOnlyList<string> paths)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatReplaceTheExistingFileListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{paths.Count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(paths.Count == 1 ? string.Empty : "s")}"));

    internal static string Apply() => CliPromptWording.Confirm();

    internal static string Installed(
        IReadOnlyList<string> rootIds,
        IReadOnlyList<ExtensionInstallPackage> packages,
        bool preview)
    {
        ArgumentNullException.ThrowIfNull(rootIds);
        ArgumentNullException.ThrowIfNull(packages);
        var verb = preview ? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleWouldInstall() : global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInstalled();
        if (rootIds.Count > 1)
        {
            return global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatExtensions(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{verb}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{rootIds.Count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{string.Join(", ", rootIds)}"));
        }

        var rootId = rootIds.FirstOrDefault()
            ?? packages.FirstOrDefault(package => package.SelectedRoot)?.Id
            ?? packages.FirstOrDefault()?.Id
            ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelected();
        var dependencies = packages
            .Where(package => !package.SelectedRoot)
            .Select(package => package.Id)
            .ToArray();
        return dependencies.Length == 0
            ? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatTheExtension($"{verb}", $"{rootId}")
            : global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatTheExtensionAndPackagesItRequires(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{verb}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{rootId}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{dependencies.Length}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{string.Join(", ", dependencies)}"));
    }

    internal static string AlreadyInstalled(string id)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.AlreadyInstalled(id);

    internal static string NothingInstalled(string source)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.NothingInstalled(source);

    internal static string NoContentRow(string source)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.NoContentRow(source);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatTheExtensionCouldNotBeInstalledNothingWasChanged($"{id}", $"{Sentence(limitation)}");

    internal static string CannotInstall(string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInstall($"{Sentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatCannotInstall($"{id}", $"{Sentence(reason)}");

    internal static string InitialForceRequired(int count)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatAlreadyWhereThePackageWouldWrite(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? "exists" : "exist")}"));

    internal static string PermissionRequired(string id)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.PermissionRequired(id);

    internal static string PermissionNext(string id, string path)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.PermissionNext(id, path);

    internal static string ForcePreviewNext(string id)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.ForcePreviewNext(id);

    internal static string PermissionAlternative(string path)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.PermissionAlternative(path);

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.MessageExtensionInstallWasCancelledNothingWasChanged();

    internal static string CreatedFile(string package, bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatWouldCreate($"{package}") : global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatCreated($"{package}");

    internal static string ReplacedFile(bool preview)
        => preview
            ? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleWouldReplaceYourPreviousFileIsInTheRecoveryBundle()
            : global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.LabelReplacedYourPreviousFileIsInTheRecoveryBundle();

    internal static string UpdatedSection(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWouldUpdate() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdated();

    internal static string CreatedFiles(int count, string directories, bool preview)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatUnder(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(preview ? "Would create" : "Created")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{directories}"));

    internal static string UpdatedSections(int count, bool preview)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatTheEntriesSectionOf(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(preview ? "Would update" : "Updated")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"));

    internal static string SavedGrant(string path, bool preview)
        => preview
            ? global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatWouldSaveAGrantForToAgentsOpenForgeJson($"{path}")
            : global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatSavedAGrantForToAgentsOpenForgeJson($"{path}");

    internal static string Lock(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWouldUpdate() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdated();

    internal static string EntriesUnchanged(IReadOnlyList<string> paths)
        => paths.Count == 0
            ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleEntriesSectionsUnchangedNone()
            : global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatEntriesSectionsUnchanged($"{string.Join(", ", paths)}");

    internal static string Verification(ExtensionInstallVerification verification)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallPhrases.FormatVerificationTargetsEntriesExtensionRecordFrameworkRecord(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{HumanName(verification.Targets)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{HumanName(verification.Topology)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{HumanName(verification.ExtensionsLifecycle)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{HumanName(verification.FrameworkLifecycle)}"));

    internal static string RecoveryFacts(ExtensionInstallRecovery recovery)
        => recovery.ResidualPath is { } path
            ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatRecoveryAtProtected($"{Name(recovery.State)}", $"{path}", $"{recovery.ProtectedPaths.Count}", $"{Plural(recovery.ProtectedPaths.Count, "path")}")
            : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecovery($"{Name(recovery.State)}");

    internal static string PackageContentsChanged(string id)
        => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.PackageContentsChanged(id);

    internal static string SourceUnavailable(string path) => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallWording.SourceUnavailable(path);

    internal static string SourceInvalid(string path, string reason)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatIsNotAValidPackageOrPackageFolder($"{path}", $"{Sentence(reason)}");

    internal static string FindingCode(ExtensionInstallFindingCode code)
        => $"extension-install.{Name(code)}";

    internal static string FindingTitle(ExtensionInstallFindingCode code) => code switch
    {
        ExtensionInstallFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        ExtensionInstallFindingCode.SelectionRequired => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSelectionIsRequired(),
        ExtensionInstallFindingCode.InteractionEnded => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInteractionEnded(),
        ExtensionInstallFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        ExtensionInstallFindingCode.SourceUnavailable => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSourceIsUnavailable(),
        ExtensionInstallFindingCode.SourceInvalid => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSourceIsInvalid(),
        ExtensionInstallFindingCode.FrameworkUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkIsUnavailable(),
        ExtensionInstallFindingCode.FrameworkUnsafe => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleFrameworkIsUnsafe(),
        ExtensionInstallFindingCode.LifecycleUnavailable => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordIsUnavailable(),
        ExtensionInstallFindingCode.LifecycleBlocked => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionRecordIsInvalid(),
        ExtensionInstallFindingCode.LifecycleObservation => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleOwnershipObservationIsIncomplete(),
        ExtensionInstallFindingCode.ManagedDivergence => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleManagedContentChanged(),
        ExtensionInstallFindingCode.PackageContentsChanged => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleRecordedPackageDiffers(),
        ExtensionInstallFindingCode.InitialForceRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsOccupied(),
        ExtensionInstallFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflicts(),
        ExtensionInstallFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        ExtensionInstallFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        ExtensionInstallFindingCode.PermissionsInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreInvalid(),
        ExtensionInstallFindingCode.PermissionsUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreUnavailable(),
        ExtensionInstallFindingCode.PermissionsChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsChanged(),
        ExtensionInstallFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionCouldNotBeSaved(),
        ExtensionInstallFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        ExtensionInstallFindingCode.MetadataProjectionSkipped => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        ExtensionInstallFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        ExtensionInstallFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        ExtensionInstallFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        ExtensionInstallFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        ExtensionInstallFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleRecoveryBundleBlocksInstallation(),
        ExtensionInstallFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsUnavailable(),
        ExtensionInstallFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        ExtensionInstallFindingCode.PackageContentMissing => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitlePackageContentIsMissing(),
        ExtensionInstallFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWritingFailed(),
        ExtensionInstallFindingCode.TopologyVerificationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleEntriesVerificationFailed(),
        ExtensionInstallFindingCode.LifecyclePublicationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordCouldNotBeWritten(),
        ExtensionInstallFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleInstallationVerificationFailed(),
        ExtensionInstallFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryFailed(),
        ExtensionInstallFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstallFailed(),
        ExtensionInstallFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.TitleExtensionInstallWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Install finding code is not defined."),
    };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpSyntax());
    internal static string HelpSelection() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpSelection());
    internal static string HelpLayout() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpLayout());
    internal static string HelpInteraction() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpInteraction());
    internal static string HelpForce() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpForce());
    internal static string HelpResults() => ("  " + global::OpenForge.Cli.OutputText.Extension.Install.ExtensionInstallText.HelpResults());

    private static string Sentence(string value) => CliFindingWording.PlainCause(value);

    private static string Plural(int count, string singular) => count == 1 ? singular : singular + "s";

    private static string Name<T>(T value) where T : struct, Enum
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    private static string HumanName<T>(T value) where T : struct, Enum
        => Name(value) switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => Name(value).Replace('-', ' '),
        };
}
