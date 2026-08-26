namespace OpenForge.Cli.Core.Framework.Extensions.Models;

internal abstract record ExtensionManifestFileReadOutcome;

internal sealed record ExtensionManifestFileReadSuccess(
    ExtensionPackageFact Package) : ExtensionManifestFileReadOutcome;

internal sealed record ExtensionManifestFileReadFailure(
    ExtensionSourceReadResult Result) : ExtensionManifestFileReadOutcome;
