using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;

internal static class ExtensionCreateWording
{
    internal static string ExtensionId() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HeadingExtensionIdLowercaseDigitsAndHyphens();

    internal static string ExtensionIdRule() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageUseLowercaseLettersDigitsAndHyphens();

    internal static string InvalidExtensionId(string value)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.InvalidExtensionId(value);

    internal static string PackageFolder() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HeadingPackageFolder();

    internal static string PackageFolderRule() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageEnterAnExistingOrdinaryDirectory();

    internal static string ConfirmCreate() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleCreateTheseFilesYN();

    internal static string Created(string id, string folder)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.Created(id, folder);

    internal static string WouldCreate(string id, string folder)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.WouldCreate(id, folder);

    internal static string AlreadyMatches(string id, string folder)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.AlreadyMatches(id, folder);

    internal static string Incomplete(string limitation)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreatePhrases.FormatTheScaffoldCouldNotBeCreatedNothingWasChanged($"{Sentence(limitation)}");

    internal static string Invalid(string problem)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreatePhrases.FormatCannotCreateTheExtension($"{Sentence(problem)}");

    internal static string Blocked(string id, string folder, string reason)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreatePhrases.FormatCannotCreateAt($"{id}", $"{folder}", $"{Sentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageExtensionCreateWasCancelledNothingWasChanged();

    internal static string WouldCreateEffect(string path) => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.WouldCreateEffect(path);

    internal static string CreatedEffect() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreated();

    internal static string NotStartedEffect() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();

    internal static string EditNext() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageEditExtensionJsonThenAddFilesUnderContentAgents();

    internal static string ManifestName(string value) => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.ManifestName(value);

    internal static string ManifestDescription(string value) => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.ManifestDescription(value);

    internal static string ManifestVersion(string value) => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.ManifestVersion(value);

    internal static string ManifestDependencies(IReadOnlyList<string> values)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreatePhrases.FormatDependencies($"{(values.Count == 0 ? "none" : string.Join(", ", values))}");

    internal static string ManifestContent() => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.HeadingManifestContent();

    internal static string NoChanges() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string CatalogueUnavailable(string folder)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.CatalogueUnavailable(folder);

    internal static string CatalogueUnsafe(string folder, string reason)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.CatalogueUnsafe(folder, reason);

    internal static string DestinationCollision(string destination)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.DestinationCollision(destination);

    internal static string DestinationChanged(string destination)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.DestinationChanged(destination);

    internal static string ApplicationFailed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.ApplicationFailed(completed, total);

    internal static string VerificationFailed(string destination)
        => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateWording.VerificationFailed(destination);

    internal static string FindingCode(ExtensionCreateFindingCode code)
        => $"extension-create.{Name(code)}";

    internal static string FindingTitle(ExtensionCreateFindingCode code) => code switch
    {
        ExtensionCreateFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        ExtensionCreateFindingCode.CatalogueUnavailable => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleCatalogueCouldNotBeRead(),
        ExtensionCreateFindingCode.CatalogueUnsafe => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleCatalogueIsUnsafe(),
        ExtensionCreateFindingCode.DestinationCollision => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleDestinationHasOtherContent(),
        ExtensionCreateFindingCode.DestinationChanged => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleDestinationChanged(),
        ExtensionCreateFindingCode.ApplicationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWritingFailed(),
        ExtensionCreateFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        ExtensionCreateFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        ExtensionCreateFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleExtensionCreateWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Create finding code is not defined."),
    };

    private static string Sentence(string value) => value.Trim().TrimEnd('.');

    private static string Name<T>(T value) where T : struct, Enum
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
}
