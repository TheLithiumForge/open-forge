namespace OpenForge.Cli.OutputText.Install;

internal static class InstallSetupText
{
    // @OpenForgeText install.setup.frontmatterquestion
    internal static string FrontmatterQuestion() => "How should Open Forge write file metadata?";
    // @OpenForgeText install.setup.rootlabel
    internal static string RootLabel() => "Root keys";
    // @OpenForgeText install.setup.rootexample
    internal static string RootExample() => "description: and tags: at the top level";
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
    internal static string Essentials() => "Directives, Patterns, Skills, and Emerging and Crystallized Memory, plus Working Memory with its contents Git-ignored.";
    // @OpenForgeText install.setup.fullcore
    internal static string FullCore() => "Directives, Guidance, Maps, Patterns, Skills, Templates and all four Memory states.";
    // @OpenForgeText install.setup.custom
    internal static string Custom() => "Starts from your current choices, or Essentials in a new workspace, then shows one list to change.";
    // @OpenForgeText install.setup.routequestion
    internal static string RouteQuestion() => "Choose what Open Forge sets up";
    // @OpenForgeText install.setup.retention
    internal static string Retention() => "Existing files and notes stay and remain routable. Open Forge stops managing its defaults.";
    // @OpenForgeText install.setup.explicitoverride
    internal static string ExplicitOverride(string id, string action) => $"{id} is set by --route {id}={action}. Change that option to change this row.";
    // @OpenForgeText install.setup.add
    internal static string Add() => "Adds missing defaults. Your existing files stay.";
    // @OpenForgeText install.setup.gitignore
    internal static string GitIgnore() => "New files stay on this machine. The entrypoint is still shared through Git.";
    // @OpenForgeText install.setup.freshremove
    internal static string FreshRemove() => "Open Forge does not add this route.";
    // @OpenForgeText install.setup.essentialssummary
    internal static string EssentialsSummary() => "Rules, patterns, skills and memory";
    // @OpenForgeText install.setup.fullcoresummary
    internal static string FullCoreSummary() => "Every built-in folder, all shared through Git";
    // @OpenForgeText install.setup.customsummary
    internal static string CustomSummary() => "Choose each folder on the next screen";
    // @OpenForgeText install.setup.addlegend
    internal static string AddLegend() => "add";
    // @OpenForgeText install.setup.gitignorelegend
    internal static string GitIgnoreLegend() => "add, keep contents out of Git";
    // @OpenForgeText install.setup.removelegend
    internal static string RemoveLegend() => "leave out";
    // @OpenForgeText install.setup.directivessummary
    internal static string DirectivesSummary() => "Rules agents must follow";
    // @OpenForgeText install.setup.guidancesummary
    internal static string GuidanceSummary() => "Advice for recurring choices";
    // @OpenForgeText install.setup.mapssummary
    internal static string MapsSummary() => "Pointers to important sources";
    // @OpenForgeText install.setup.patternssummary
    internal static string PatternsSummary() => "Reusable shapes for code and documents";
    // @OpenForgeText install.setup.skillssummary
    internal static string SkillsSummary() => "Packaged agent capabilities";
    // @OpenForgeText install.setup.templatessummary
    internal static string TemplatesSummary() => "Copy-ready starter files";
    // @OpenForgeText install.setup.workingsummary
    internal static string WorkingSummary() => "Notes for active work";
    // @OpenForgeText install.setup.emergingsummary
    internal static string EmergingSummary() => "Findings not accepted yet";
    // @OpenForgeText install.setup.crystallizedsummary
    internal static string CrystallizedSummary() => "Accepted knowledge";
    // @OpenForgeText install.setup.archivedsummary
    internal static string ArchivedSummary() => "Completed and historical records";
    // @OpenForgeText install.setup.help
    internal static string Help() => "Use --configure to change an installed setup. Choose --preset essentials, full-core, or custom. With Custom, repeat --route <id>=<add|remove|git-ignore>. Remove keeps existing files and their routes. Add + Git-ignore shares the entrypoint, ignores route contents, and omits private child Entries.";
}
