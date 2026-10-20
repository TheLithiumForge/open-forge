namespace OpenForge.Cli.OutputText.Install;

internal static class InstallInputText
{
    // @OpenForgeText install.wording.invalid-frontmatter-value
    internal static string InvalidFrontmatter(string value) => $"Invalid --frontmatter value: {value}. Use root or scoped.";

    // @OpenForgeText install.wording.invalid-preset-value
    internal static string InvalidPreset(string value) => $"Invalid --preset value: {value}. Use minimal, essentials, full-core, or custom.";

    // @OpenForgeText install.wording.invalid-route-value
    internal static string InvalidRoute(string value) => $"Invalid --route value: {value}.";

    // @OpenForgeText install.wording.route-forms
    internal static string RouteForms() => "Use <id>=add, <id>=remove, or <id>=git-ignore.";

    // @OpenForgeText install.wording.route-ids
    internal static string RouteIds(IReadOnlyList<string> ids) => $"Built-in IDs: {string.Join(", ", ids)}.";
}
