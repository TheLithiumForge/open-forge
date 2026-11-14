namespace OpenForge.Cli.OutputText.Tool.Inspect;

internal static class ToolInspectText
{
    // @OpenForgeText tool.inspect.headline
    internal static string Headline() => "Tool setup.";
    // @OpenForgeText tool.inspect.finding
    internal static string Finding() => "Check the tool facts.";
    // @OpenForgeText tool.inspect.tool-row
    internal static string ToolRow(string id, string name, string? range, string? state)
        => $"{id}  {name}  {range ?? "undeclared"}  {state ?? "catalogue"}";
    // @OpenForgeText tool.inspect.bin
    internal static string Bin(string? path, string? readiness) => $"Bin: {path ?? "unavailable"} ({readiness ?? "unavailable"})";
    // @OpenForgeText tool.inspect.manager-bin
    internal static string ManagerBin(string? path, string? readiness) => $"Manager global bin: {path ?? "unavailable"} ({readiness ?? "unavailable"})";
    // @OpenForgeText tool.inspect.version
    internal static string Version(string? locked, string? detected) => $"Version: locked {locked ?? "missing"}, detected {detected ?? "unavailable"}";
    // @OpenForgeText tool.inspect.tool-path
    internal static string ToolPath(string? path) => $"Tool path: {path ?? "unavailable"}";
    // @OpenForgeText tool.inspect.runtime-path
    internal static string RuntimePath(string? path) => $"Runtime path: {path ?? "unavailable"}";
    // @OpenForgeText tool.inspect.origin
    internal static string Origin(string value) => $"Origin: {value}";
    // @OpenForgeText tool.inspect.upstream
    internal static string Upstream(string value) => $"Upstream: {value}";
    // @OpenForgeText tool.inspect.licence
    internal static string Licence(string value) => $"Licence: {value}";
    // @OpenForgeText tool.inspect.channel
    internal static string Channel(string id, string kind, string? runtime, string? range, string? client) => $"Channel {id}: {kind}, runtime {string.Join(" ", new[] { runtime ?? "native", range }.Where(value => !string.IsNullOrEmpty(value)))}, required client {client ?? "any"}";
    // @OpenForgeText tool.inspect.target
    internal static string Target(string os, string cpu, string? libc, string? minimumOs, string? minimumLibc) => $"  Target: {os}, {cpu}, libc {libc ?? "not applicable"}, minimum OS {minimumOs ?? "unspecified"}, minimum libc {minimumLibc ?? "unspecified"}";
    // @OpenForgeText tool.inspect.profile
    internal static string Profile(string id, string range, string toolMinimum, string forgeMinimum, string tested) => $"Profile {id}: {range}, minimum tool {toolMinimum}, minimum Open Forge {forgeMinimum}, tested {tested}";
    // @OpenForgeText tool.inspect.setup
    internal static string Setup(string name, IReadOnlyList<string> arguments) => $"{name}: [{string.Join(", ", arguments.Select(argument => $"\"{System.Text.Json.JsonEncodedText.Encode(argument)}\""))}]";
    // @OpenForgeText tool.inspect.effect
    internal static string Effect(string scope, string description, string? path, string? selector)
        => $"  {scope}: {string.Join(" ", new[] { description, path, selector }.Where(value => !string.IsNullOrEmpty(value)))}";
    // @OpenForgeText tool.inspect.addition
    internal static string Addition(string id, string kind, string scope, string description) => $"Addition {id}: {kind}, {scope}. {description}";
    // @OpenForgeText tool.inspect.detail
    internal static string Detail(string name, string? value) => $"  {name}: {value ?? "unavailable"}";
    // @OpenForgeText tool.inspect.machine
    internal static string Machine(string? machine, string? native) => $"Machine records: {machine ?? "unavailable"}, native settings {native ?? "unavailable"}";
    // @OpenForgeText tool.inspect.possible-output
    internal static string PossibleOutput(string path, string scope, string match, string purpose, string condition)
        => $"  Potential native output: {path}, {scope}, {match}, {purpose}, condition {(string.IsNullOrEmpty(condition) ? "all" : condition)}";
    // @OpenForgeText tool.inspect.stored-change
    internal static string StoredChange(string id, string disposition, string? current, string? desired, string? conflict)
        => $"{id}: {disposition}, stored value {current ?? "unavailable"} -> {desired ?? "unavailable"}{(conflict is null ? string.Empty : ", " + conflict)}";
    // @OpenForgeText tool.inspect.retired-addition
    internal static string RetiredAddition(string id, string disposition, string? conflict)
        => $"Retired addition {id}: {disposition}{(conflict is null ? string.Empty : ", " + conflict)}";
    // @OpenForgeText tool.inspect.native-setting
    internal static string NativeSetting(string id, string? previous, string? applied, IReadOnlyList<string> manual)
        => $"{id}: previous stored value {previous}, applied stored value {applied}. Manual setting change: {string.Join(" ", manual)}";
    // @OpenForgeText tool.inspect.recorded-native-home
    internal static string RecordedNativeHome(string? value) => Detail("Recorded native home", value);
    // @OpenForgeText tool.inspect.release-asset-version
    internal static string ReleaseAssetVersion(string? value) => Detail("Release asset version", value);
    // @OpenForgeText tool.inspect.bundled-runtime-path
    internal static string BundledRuntimePath(string? value) => Detail("Bundled runtime path", value);
    // @OpenForgeText tool.inspect.bundled-application-entry
    internal static string BundledApplicationEntry(string? value) => Detail("Bundled application entry", value);
    // @OpenForgeText tool.inspect.native-home-policy
    internal static string NativeHomePolicy(string? value) => Detail("Native home policy", value);
    // @OpenForgeText tool.inspect.required-initial-state
    internal static string RequiredInitialState(string? value) => Detail("Required initial state", value);
    // @OpenForgeText tool.inspect.when-not-ignored
    internal static string WhenNotIgnored(string? value) => Detail("Only when not effectively ignored", value);
    // @OpenForgeText tool.inspect.config-path
    internal static string ConfigPath(string? value) => Detail("Path", value);
    // @OpenForgeText tool.inspect.json-pointer
    internal static string JsonPointer(string? value) => Detail("JSON Pointer", value);
    // @OpenForgeText tool.inspect.json-value
    internal static string JsonValue(string? value) => Detail("Value", value);
    // @OpenForgeText tool.inspect.start-marker
    internal static string StartMarker(string? value) => Detail("Start marker", value);
    // @OpenForgeText tool.inspect.end-marker
    internal static string EndMarker(string? value) => Detail("End marker", value);
    // @OpenForgeText tool.inspect.content
    internal static string Content(string? value) => Detail("Content", value);
    // @OpenForgeText tool.inspect.file-template
    internal static string FileTemplate(string? value) => Detail("File template", value);
    // @OpenForgeText tool.inspect.desired-native-setting
    internal static string DesiredNativeSetting(string? value) => Detail("Desired native setting", value);
    // @OpenForgeText tool.inspect.stored-native-path
    internal static string StoredNativePath(string? value) => Detail("Stored native value path", value);
    // @OpenForgeText tool.inspect.stored-native-parser
    internal static string StoredNativeParser(string? value) => Detail("Stored native value parser", value);
    // @OpenForgeText tool.inspect.missing-stored-native-value
    internal static string MissingStoredNativeValue(string? value) => Detail("Missing stored native value", value);
    // @OpenForgeText tool.inspect.stored-native-limit
    internal static string StoredNativeLimit(string? value) => Detail("Stored native value byte limit", value);
    // @OpenForgeText tool.inspect.native-status-parser
    internal static string NativeStatusParser(string? value) => Detail("Native status parser", value);
    // @OpenForgeText tool.inspect.native-read-timeout
    internal static string NativeReadTimeout(string? value) => Detail("Native read timeout seconds", value);
    // @OpenForgeText tool.inspect.native-read-limit
    internal static string NativeReadLimit(string? value) => Detail("Native read output byte limit", value);
    // @OpenForgeText tool.inspect.minimum-linux-kernel
    internal static string MinimumLinuxKernel(string? value) => Detail("Minimum Linux kernel", value ?? "unspecified");
    // @OpenForgeText tool.inspect.minimum-lib-std-cpp
    internal static string MinimumLibStdCpp(string? value) => Detail("Minimum libstdc++", value ?? "unspecified");
    // @OpenForgeText tool.inspect.native-timeout
    internal static string NativeTimeout(string? value) => Detail("Native timeout seconds", value);
    // @OpenForgeText tool.inspect.native-output-limit
    internal static string NativeOutputLimit(string? value) => Detail("Native output byte limit", value);
    // @OpenForgeText tool.inspect.native-inventory-parser
    internal static string NativeInventoryParser(string? value) => Detail("Native inventory parser", value);
    // @OpenForgeText tool.inspect.native-inventory-timeout
    internal static string NativeInventoryTimeout(string? value) => Detail("Native inventory timeout seconds", value);
    // @OpenForgeText tool.inspect.native-inventory-limit
    internal static string NativeInventoryLimit(string? value) => Detail("Native inventory output byte limit", value);
    // @OpenForgeText tool.inspect.bundled-runtime-arguments
    internal static string BundledRuntimeArguments(IReadOnlyList<string> arguments) => Setup("Bundled runtime arguments", arguments);
    // @OpenForgeText tool.inspect.apply
    internal static string Apply(IReadOnlyList<string> arguments) => Setup("Apply", arguments);
    // @OpenForgeText tool.inspect.read
    internal static string Read(IReadOnlyList<string> arguments) => Setup("Read", arguments);
    // @OpenForgeText tool.inspect.manual-setting-change
    internal static string ManualSettingChange(IReadOnlyList<string> arguments) => Setup("Manual setting change", arguments);
    // @OpenForgeText tool.inspect.native-inventory
    internal static string NativeInventory(IReadOnlyList<string> arguments) => Setup("Native inventory", arguments);
    // @OpenForgeText tool.inspect.native-preview
    internal static string NativePreview(IReadOnlyList<string> arguments) => Setup("Native preview", arguments);
    // @OpenForgeText tool.inspect.native-setup
    internal static string NativeSetup(IReadOnlyList<string> arguments) => Setup("Native setup", arguments);
    // @OpenForgeText tool.inspect.native-uninstall
    internal static string NativeUninstall(IReadOnlyList<string> arguments) => Setup("Native uninstall", arguments);
    // @OpenForgeText tool.inspect.bundled-runtime
    internal static string BundledRuntime(string runtime, string version) => Detail("Bundled runtime", $"{runtime} {version}");
    // @OpenForgeText tool.inspect.native-override-unavailable
    internal static string NativeOverrideUnavailable() => Detail("Native environment override", "unchecked or unavailable");
    // @OpenForgeText tool.inspect.native-override-none
    internal static string NativeOverrideNone() => Detail("Native environment override", "none");
    // @OpenForgeText tool.inspect.native-override
    internal static string NativeOverride(string variable, string effective) => Detail($"Effective native value from {variable}", effective);
    // @OpenForgeText tool.inspect.environment
    internal static string Environment(string variable, string? value) => Detail(variable, value ?? "<removed for this command>");
}
