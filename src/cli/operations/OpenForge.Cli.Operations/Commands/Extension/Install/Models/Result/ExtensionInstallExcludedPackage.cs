namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;

internal sealed record ExtensionInstallExcludedPackage(
    string Id,
    string Name,
    int FileCount,
    IReadOnlyList<string> Categories,
    bool EntirePayloadExcluded);
