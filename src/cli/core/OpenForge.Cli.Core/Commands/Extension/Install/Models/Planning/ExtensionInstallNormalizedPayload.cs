using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;

internal sealed record ExtensionInstallNormalizedPayload(
        string PackageId,
        ExtensionPackageFileFact File,
        string NormalizedPath,
        string PortableKey);
