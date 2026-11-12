using System.Collections.Frozen;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

namespace OpenForge.Cli.Core.Framework.Tools;

// The manifest contract requires exact release admission. Fixtures inject their own pins and revisions.
internal sealed class ToolReleaseSupport
{
    internal static readonly ToolReleaseSupport Production = new([], []);
    private readonly FrozenSet<ToolReviewedPin> _pins;
    private readonly FrozenSet<ToolAdapterRevision> _adapters;

    internal ToolReleaseSupport(IEnumerable<ToolReviewedPin> pins, IEnumerable<ToolAdapterRevision> adapters)
    {
        ArgumentNullException.ThrowIfNull(pins);
        ArgumentNullException.ThrowIfNull(adapters);
        _pins = pins.ToFrozenSet();
        _adapters = adapters.ToFrozenSet();
    }

    internal string? Blocker(ToolManifest manifest, string sha256)
    {
        if (!_pins.Contains(new(manifest.Id, manifest.Version, sha256, manifest.Adapter.Revision))) return "The exact definition is not reviewed by this CLI release.";
        if (!_adapters.Contains(new(manifest.Adapter.Id, manifest.Adapter.Revision))) return "The required compiled adapter revision is unavailable.";
        if (manifest.Adapter.Id == "qmd-2.8" || manifest.Capabilities.Any(capability => capability != ToolVocabulary.CodeGraphCapability)) return "The definition requires a reserved V1 capability.";
        foreach (var (key, value) in manifest.Processes.Environment)
            if (manifest.Mcp.Environment.TryGetValue(key, out var other) && other != value) return "The process and MCP environment declarations conflict.";
        return null;
    }
}
