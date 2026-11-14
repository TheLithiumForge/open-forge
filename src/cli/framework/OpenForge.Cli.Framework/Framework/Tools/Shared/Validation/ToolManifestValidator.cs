using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

internal static partial class ToolManifestValidator
{
    internal static void Validate(ToolManifest manifest)
    {
        Require(manifest.Schema == ToolVocabulary.ManifestSchema, "schema must be 1");
        Require(ToolIdentity.IsToolId(manifest.Id) && ToolIdentity.IsVersion(manifest.Version), "invalid tool ID or exact version");
        Require(!string.IsNullOrWhiteSpace(manifest.Name) && !string.IsNullOrWhiteSpace(manifest.License), "name and license are required");
        Require(Https(manifest.Source.Repository) && !string.IsNullOrWhiteSpace(manifest.Source.Tag), "invalid source repository or tag");
        Choice(manifest.Admission, "admission", "candidate", "enabled", "blocked");
        Require(manifest.Source.Commit is { Length: 40 } commit && commit.All(IsLowerHex)
            || manifest.Admission == "blocked" && manifest.Source.Commit is null, "source commit must contain 40 lowercase hex characters");
        Require(ToolVocabulary.Adapters.Contains(manifest.Adapter.Id) && manifest.Adapter.Revision > 0, "unknown adapter or revision");
        Tokens(manifest.Capabilities, ToolVocabulary.Capabilities, "capabilities");
        Require(manifest.Capabilities.Length > 0 && Https(manifest.DownloadBase), "capabilities and an HTTPS download base are required");
        Require(manifest.Targets.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(ToolVocabulary.Targets), "all six native targets are required");
        Require(manifest.Artifacts.Count > 0, "at least one artifact is required");
        foreach (var (id, artifact) in manifest.Artifacts)
        {
            Require(artifact is not null, "artifact entries cannot be null");
            Require(ToolIdentity.IsId(id), "invalid artifact ID");
            Require(artifact.Url is null || Https(artifact.Url), "artifact url must be exact HTTPS without credentials, query or fragment");
            Require(!artifact.File.Contains('?') && !artifact.File.Contains('#')
                && ToolIdentity.IsRelativePath(Uri.UnescapeDataString(artifact.File)), "artifact file must be a contained relative URL path");
            Require(artifact.Bytes > 0 || manifest.Admission == "blocked" && artifact.Bytes is null, "artifact bytes must be positive");
            Require(ToolArtifactIntegrity.IsValid(artifact.Integrity), "invalid artifact integrity");
            if (manifest.Runtime.Kind != "locked-package" && manifest.Capabilities.Contains(ToolVocabulary.CodeGraphCapability, StringComparer.Ordinal))
                Require(artifact.Integrity.StartsWith(ToolArtifactIntegrity.Sha256Prefix, StringComparison.Ordinal), "graph distributions require SHA-256");
            Choice(artifact.Archive, "archive", "zip", "tar.gz", "tgz", "file");
            Require(artifact.StripComponents >= 0 && (artifact.Archive != "file" || artifact.StripComponents == 0), "invalid stripComponents");
        }
        foreach (var (id, target) in manifest.Targets)
        {
            Require(target is not null, "target entries cannot be null");
            Choice(target.State, "target state", "available", "unsupported", "blocked");
            References(target.Artifacts, manifest, "target artifacts");
            if (target.State == "available")
                Require(target.Artifacts.Length > 0 && ToolIdentity.IsRelativePath(target.Entry), "an available target needs artifacts and an entry");
            else Require(!string.IsNullOrWhiteSpace(target.Reason), "a blocked or unsupported target needs a reason");
            if (target.Entry is not null) Require(ToolIdentity.IsRelativePath(target.Entry), "invalid target entry");
            if (target.Lock is { } closure)
            {
                References(closure.Artifacts, manifest, "dependency lock");
                Require(closure.ClosureComplete && target.Artifacts.Concat(manifest.Runtime.ExtraDownloads).All(closure.Artifacts.Contains), "dependency closure is incomplete");
            }
            if (manifest.Runtime.Kind == "locked-package" && target.State == "available")
                Require(target.Lock is not null, "locked packages need a complete dependency lock");
            if (manifest.Admission == "enabled" && target.State == "available")
                Require(manifest.Qualification.Targets.Contains(id, StringComparer.Ordinal), "enabled targets require native qualification");
        }
        Choice(manifest.Runtime.Kind, "runtime kind", "native", "bundled", "locked-package");
        Require(manifest.Runtime.InstallScripts == "forbidden", "install scripts are forbidden");
        References(manifest.Runtime.ExtraDownloads, manifest, "extra downloads");
        Require(ToolVocabulary.Templates.Contains(manifest.Configuration.Template), "unknown configuration template");
        Choice(manifest.Configuration.CodeRoot, "code root", "workspace-relative", "none");
        Choice(manifest.Configuration.Documents, "documents", "none", "open-forge-source-inventory");
        Tokens(manifest.Configuration.Actions, ToolVocabulary.Actions, "actions");
        foreach (var value in manifest.Configuration.Settings.Values)
            Require(value.ValueKind is JsonValueKind.String or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) && double.IsFinite(number), "settings must contain finite scalars");
        foreach (var file in manifest.Configuration.Files)
        {
            Require(file is not null, "configuration files cannot be null");
            Template(file.Path);
            Choice(file.Format, "configuration format", "json-members", "native-settings", "native-collections", "adapter-helper");
            Distinct(file.Members, "member selectors");
        }
        Require(manifest.Configuration.Files.Select(file => file.Path).Distinct(StringComparer.Ordinal).Count() == manifest.Configuration.Files.Length, "duplicate configuration file");
        ValidateExecution(manifest);
        if (manifest.Lifecycle is { } lifecycle) ToolLifecycleValidator.Validate(lifecycle);
        if (manifest.Skill is { } skill) Require(ToolVocabulary.Skills.Contains(skill.Template), "unknown usage Skill");
        if (manifest.Capabilities.Contains(ToolVocabulary.CodeGraphCapability, StringComparer.Ordinal))
            Require(manifest.Skill is { Optional: false, DefaultEnabled: true }, "V1 graph definitions require enabled mandatory guidance");
        Tokens(manifest.Checks, ToolVocabulary.Checks, "checks");
        Choice(manifest.Qualification.Attestation, "attestation", "release-evidence-required", "required-at-install", "unavailable");
        Tokens(manifest.Qualification.Targets, ToolVocabulary.Targets, "qualification targets");
        Choice(manifest.Qualification.Accuracy, "accuracy", "three-languages-2026-11-12", "pending");
        if (manifest.Admission == "enabled")
            Require(manifest.Targets.Values.Any(target => target.State == "available")
                && manifest.Qualification.Attestation != "unavailable"
                && manifest.Checks.Contains("artifact-hash", StringComparer.Ordinal)
                && manifest.Checks.Contains("installed-files", StringComparer.Ordinal), "enabled admission requires an available verified target");
    }

    private static void ValidateExecution(ToolManifest manifest)
    {
        var process = manifest.Processes;
        Choice(process.Telemetry, "telemetry", "off", "absent", "unverified");
        Choice(process.UpdateChecks, "update checks", "off", "absent", "unverified");
        Choice(process.Daemon, "daemon", "off", "session-required", "unverified");
        Require(process.Watcher == "off" && process.AutoIndex == "off" && process.Hooks == "blocked", "watching, automatic indexing and hooks must be disabled");
        Choice(process.Consent, "process consent", "none", "session-daemon");
        Choice(process.Coordination, "coordination", "worktree", "account-build-cache");
        Environment(process.Environment);
        Choice(manifest.Index.Location, "index location", "project-directory", "native-account-cache", "external");
        Template(manifest.Index.Path);
        Require(manifest.Index.PerWorktree, "indexes must have worktree identity");
        Choice(manifest.Index.SharedContentCache, "content cache", "none", "native-content-keyed");
        Choice(manifest.Index.Freshness, "freshness", "manual-sync", "full-rebuild-after-delete", "manual-update");
        Require(manifest.Mcp.Transport == "stdio" && manifest.Mcp.Command == "target-entry" && manifest.Mcp.Cwd == "worktree", "MCP requires stdio, the target entry and worktree CWD");
        foreach (var argument in manifest.Mcp.Args) Template(argument);
        Distinct(manifest.Mcp.Args, "MCP arguments");
        Environment(manifest.Mcp.Environment);
        Require(manifest.Mcp.Hosts.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(ToolVocabulary.Hosts), "both host templates are required");
        Require(manifest.Mcp.Hosts["claude-code"].Path == ".mcp.json" && manifest.Mcp.Hosts["codex"].Path == ".codex/config.toml", "host templates cannot widen destinations");
        Require(manifest.Mcp.Hosts["claude-code"].Selector == $"/mcpServers/open-forge-{manifest.Id}"
            && manifest.Mcp.Hosts["codex"].Selector == $"mcp_servers.open-forge-{manifest.Id}", "host selectors must identify the tool member");
    }

    internal static bool Https(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps && uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0;

    internal static void Template(string value)
    {
        Require(value is not null && !value.Contains('\0'), "invalid template string");
        foreach (Match match in Variables().Matches(value)) Require(ToolVocabulary.Variables.Contains(match.Groups[1].Value), "unknown template variable");
        Require(!Variables().Replace(value, "").Contains("${", StringComparison.Ordinal), "unfinished template variable");
    }

    private static void Environment(Dictionary<string, string> values)
    {
        foreach (var (key, value) in values)
        {
            Require(key.Length > 0 && !key.Contains('=') && !key.Contains('\0'), "invalid environment name");
            Template(value);
        }
    }

    private static bool IsLowerHex(char value) => value is >= '0' and <= '9' or >= 'a' and <= 'f';
    private static void References(string[] values, ToolManifest manifest, string name)
    {
        Distinct(values, name);
        Require(values.All(manifest.Artifacts.ContainsKey), $"{name} references an unknown artifact");
    }
    internal static void Distinct(string[] values, string name) => Require(values is not null && values.All(value => value is not null)
        && values.Distinct(StringComparer.Ordinal).Count() == values.Length, $"{name} must be distinct non-null values");
    private static void Tokens(string[] values, IReadOnlySet<string> allowed, string name)
    {
        Distinct(values, name);
        Require(values.All(allowed.Contains), $"unknown {name} token");
    }
    private static void Choice(string value, string name, params string[] allowed) => Require(allowed.Contains(value, StringComparer.Ordinal), $"unknown {name}");
    internal static void Require([DoesNotReturnIf(false)] bool condition, string cause)
    {
        if (!condition) throw new JsonException($"Invalid tool definition: {cause}.");
    }
    [GeneratedRegex(@"\$\{([^}]*)\}", RegexOptions.CultureInvariant)] private static partial Regex Variables();
}
