using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Shared.Resolution;

internal sealed class ToolRunPinResolver(ToolCatalogueReader catalogue, string target)
{
    internal ToolRunResolution Resolve(ToolRunRequest request, CliWorkspace workspace, ToolRunPinEvidence evidence)
    {
        var receipt = evidence.Receipt;
        var setting = evidence.Setting;
        ToolRunRefused Refuse(ToolRunFailureCode code, string cause, bool offer = false)
            => new(new(code, request.Id, cause, offer) { Workspace = workspace });
        if (receipt.Id != request.Id || receipt.Version != setting.Version || receipt.Target != target
            || receipt.CodeRoot != (setting.CodeRoot ?? ".") || receipt.Telemetry != setting.Telemetry
            || !(setting.Hosts ?? [.. ToolVocabulary.Hosts]).Order(StringComparer.Ordinal).SequenceEqual(receipt.Hosts.Order(StringComparer.Ordinal))
            || !(setting.Components ?? []).OrderBy(pair => pair.Key, StringComparer.Ordinal).SequenceEqual((receipt.Components ?? []).OrderBy(pair => pair.Key, StringComparer.Ordinal)))
            return Refuse(ToolRunFailureCode.PinMismatch, "The declared pin or configuration differs from its verification receipt.");
        var definition = catalogue.Definition(receipt.ManifestBytes, "receipt." + receipt.ManifestFormat, false);
        if (definition.Sha256 != receipt.ManifestSha256 || definition.Manifest.Id != request.Id || definition.Manifest.Version != setting.Version
            || definition.Manifest.Adapter.Revision != receipt.AdapterRevision || !definition.Reviewed)
            return Refuse(ToolRunFailureCode.PinMismatch, "The receipt doesn't identify an exact reviewed tool pin.");
        var manifest = definition.Manifest;
        if (manifest.Admission != "enabled" || !manifest.Targets.TryGetValue(target, out var selected) || selected.State != "available"
            || selected.Entry is null || !manifest.Qualification.Targets.Contains(target, StringComparer.Ordinal))
            return Refuse(ToolRunFailureCode.VerificationUnavailable, "The recorded tool pin isn't qualified for this target.");
        if (selected.Launcher is { } launcher && (launcher.Args is null || launcher.Args.Any(argument => argument is null || argument.Contains('\0'))))
            return Refuse(ToolRunFailureCode.VerificationUnavailable, "The declared launcher arguments are unavailable.");
        var key = ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Install, ToolStoreIdentity.InstallPath(manifest, target));
        if (!evidence.Registry.Units.TryGetValue(key, out var unit))
            return Refuse(ToolRunFailureCode.DeclaredToolMissing, "The declared tool pin isn't installed for this account.", true);
        if (!unit.Verified || unit.StagingPath is not null || unit.RecipeSha256 != receipt.ManifestSha256)
            return Refuse(ToolRunFailureCode.PinMismatch, "The account-local install isn't the verified workspace pin.");
        if (!unit.Workspaces.TryGetValue(WorkspaceIdentity.Key(workspace.PhysicalRoot), out var local))
            return Refuse(ToolRunFailureCode.DeclaredToolMissing, "The declared tool has no local installation for this workspace.", true);
        var launch = local.Launch;
        if (launch is null) return Refuse(ToolRunFailureCode.VerificationUnavailable, "The local installation has no verified launch evidence.");
        if (WorkspaceIdentity.Key(local.PhysicalWorkspace) != WorkspaceIdentity.Key(workspace.PhysicalRoot) || local.CodeRoot != receipt.CodeRoot || launch.ToolId != request.Id
            || launch.Version != setting.Version || launch.ManifestSha256 != receipt.ManifestSha256 || launch.StoreId != unit.Binding.Id)
            return Refuse(ToolRunFailureCode.PinMismatch, "The account-local launch differs from the workspace pin.");
        var closure = Closure(evidence.Registry, key);
        if (closure is null) return Refuse(ToolRunFailureCode.VerificationUnavailable, "The install dependency closure isn't verified.");
        foreach (var member in closure)
        {
            if (!receipt.Bindings.Any(binding => binding.Id == member.Binding.Id && binding.Kind == member.Binding.Kind
                && binding.IdentitySha256 == member.Binding.IdentitySha256))
                return Refuse(ToolRunFailureCode.PinMismatch, "The lock and account-local store bindings differ.");
            var locked = receipt.Files.Where(file => file.StoreId == member.Binding.Id).OrderBy(file => file.Path, StringComparer.Ordinal);
            if (!locked.SequenceEqual(member.Files.OrderBy(file => file.Path, StringComparer.Ordinal)))
                return Refuse(ToolRunFailureCode.PinMismatch, "The lock and immutable member receipts differ.");
        }
        var files = closure.SelectMany(member => member.Files).ToArray();
        if (launch.Files is null || launch.PrefixArguments is null || launch.Environment is null || !Path.IsPathFullyQualified(launch.Executable)
            || launch.Files.Any(file => file is null) || launch.PrefixArguments.Any(argument => argument is null || argument.Contains('\0'))
            || launch.Environment.Any(pair => string.IsNullOrEmpty(pair.Key) || pair.Key.ContainsAny('=', '\0') || pair.Value is null || pair.Value.Contains('\0'))
            || files.Length == 0 || launch.Files.Length != files.Length
            || launch.Files.DistinctBy(file => (file.StoreId, file.Path)).Count() != files.Length
            || launch.Files.Any(stamp => !ToolIdentity.IsRelativePath(stamp.Path) || stamp.Bytes < 0 || stamp.LastWriteTimeUtcTicks <= 0
                || !files.Any(file => file.StoreId == stamp.StoreId && file.Path == stamp.Path)))
            return Refuse(ToolRunFailureCode.VerificationUnavailable, "The immutable launch member stamps are incomplete.");
        if (launch.McpArguments is { } mcp && mcp.Any(arguments => arguments is null || arguments.Any(argument => argument is null || argument.Contains('\0'))))
            return Refuse(ToolRunFailureCode.VerificationUnavailable, "The recorded MCP argument vectors are unavailable.");
        var executable = selected.Launcher?.Executable ?? selected.Entry;
        if (!ToolIdentity.IsRelativePath(executable)) return Refuse(ToolRunFailureCode.PinMismatch, "The declared launcher executable isn't contained in its install.");
        var expected = Path.Combine(unit.Binding.PhysicalRoot, executable.Replace('/', Path.DirectorySeparatorChar));
        if (!SamePath(launch.Executable, expected) || !unit.Files.Any(file => file.Path == executable && file.Executable))
            return Refuse(ToolRunFailureCode.PinMismatch, "The launch executable isn't the verified target entry.");
        // Writers resolves every template during full verification. Run uses that account-local
        // argument vector without trying to reconstruct state, cache or native-project values.
        if (launch.PrefixArguments.Length != (selected.Launcher?.Args.Length ?? 0))
            return Refuse(ToolRunFailureCode.PinMismatch, "The recorded fixed arguments don't cover the declared launcher.");
        return new ToolRunReady(new(launch, request.CurrentDirectory, request.Arguments)) { Workspace = workspace };
    }

    internal static ToolStoredUnit[]? Closure(ToolStoreRegistryDocument registry, string key)
    {
        var units = new List<ToolStoredUnit>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<string>();
        pending.Enqueue(key);
        while (pending.TryDequeue(out var next))
        {
            if (!seen.Add(next)) continue;
            if (!registry.Units.TryGetValue(next, out var unit) || !unit.Verified || unit.StagingPath is not null) return null;
            units.Add(unit);
            foreach (var dependency in unit.Dependencies) pending.Enqueue(dependency);
        }
        return [.. units];
    }

    private static bool SamePath(string left, string right)
        => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
