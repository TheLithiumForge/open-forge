namespace OpenForge.Cli.OutputText.Install;

internal static class InstallSetupText
{
    // @OpenForgeText install.setup.frontmatterquestion
    internal static string FrontmatterQuestion() => "How should Open Forge write file metadata?";
    // @OpenForgeText install.setup.rootlabel
    internal static string RootLabel() => "Root keys";
    // @OpenForgeText install.setup.rootexample
    internal static string RootExample() => "description: and tags: at the top of the frontmatter";
    // @OpenForgeText install.setup.scopedlabel
    internal static string ScopedLabel() => "Scoped under open-forge:";
    // @OpenForgeText install.setup.scopedexample
    internal static string ScopedExample() => "open-forge: holds description: and tags:";
    // @OpenForgeText install.setup.frontmatter
    internal static string Frontmatter(string form) => $"Frontmatter: {form}";
    // @OpenForgeText install.setup.frontmatterchange
    internal static string FrontmatterChange(string before, string after) => $"Frontmatter: {before} -> {after}";
    // @OpenForgeText install.setup.frontmatterkept
    internal static string FrontmatterKept(int count)
        => count == 1
            ? "Kept 1 file in its previous form."
            : string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Kept {count} files in their previous form.");
    // @OpenForgeText install.setup.presetquestion
    internal static string PresetQuestion() => "Choose your Open Forge setup";
    // @OpenForgeText install.setup.essentials
    internal static string Essentials() => "Directives, Patterns, Skills and Memory. Working Memory is Git-ignored.";
    // @OpenForgeText install.setup.fullcore
    internal static string FullCore() => "All built-in Core categories and Memory states, without managed Git-ignore patterns.";
    // @OpenForgeText install.setup.custom
    internal static string Custom() => "Start with current choices and choose the routes to change.";
    // @OpenForgeText install.setup.routequestion
    internal static string RouteQuestion() => "Choose a route to change, or finish selection";
    // @OpenForgeText install.setup.actionquestion
    internal static string ActionQuestion(string id) => $"Choose how to configure {id}";
    // @OpenForgeText install.setup.retention
    internal static string Retention() => "Remove omits supplied defaults. Existing files, notes and companions stay and remain routable.";
    // @OpenForgeText install.setup.explicitoverride
    internal static string ExplicitOverride() => "This choice was supplied with --route and is retained.";
    // @OpenForgeText install.setup.add
    internal static string Add() => "Add missing supplied defaults and keep existing content.";
    // @OpenForgeText install.setup.gitignore
    internal static string GitIgnore() => "Add the route, keep its entrypoint shared, and Git-ignore its contents. Tracked files stay tracked.";
    // @OpenForgeText install.setup.help
    internal static string Help() => "Use --configure to change an installed setup. Choose --preset essentials, full-core, or custom. With Custom, repeat --route <id>=<add|remove|git-ignore>. Remove keeps existing files and their routes. Add + Git-ignore shares the entrypoint, ignores route contents, and omits private child Entries.";
}
