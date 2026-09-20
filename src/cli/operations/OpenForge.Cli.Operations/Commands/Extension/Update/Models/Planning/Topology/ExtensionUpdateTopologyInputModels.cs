using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;

internal sealed record ExtensionUpdateTopologyInput(
    ExtensionUpdateRequest Request,
    IReadOnlyList<ExtensionPackageFact> Packages,
    IReadOnlySet<string> RetiredPaths,
    ExtensionUpdateTopologyAdmission Admission);

internal sealed record ExtensionUpdateTopologyAdmission(
    IReadOnlyDictionary<string, byte[]> Overrides,
    IReadOnlySet<string> Exclusions);
