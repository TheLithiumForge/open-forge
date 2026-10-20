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
    internal static string Essentials() => "Rules, reusable patterns, skills and project memory. New notes for active work stay out of Git.";
    // @OpenForgeText install.setup.fullcore
    internal static string FullCore() => "Everything in Essentials, plus advice, pointers to sources, starter files and archived records. All contents, including notes for active work, are shared through Git.";
    // @OpenForgeText install.setup.custom
    internal static string Custom() => "Starts from your current choices, or Essentials in a new workspace, then shows one list to change.";
    // @OpenForgeText install.setup.minimal
    internal static string Minimal() => "Only the loader and the Open Forge sections in AGENTS.md and CLAUDE.md. Nothing is on until you list it, for example a link to a document you already have.";
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
    internal static string FreshRemove(string id) => $"Leave out: no {id} folder is created. You can add it later with open-forge install --configure.";
    // @OpenForgeText install.setup.essentialssummary
    internal static string EssentialsSummary() => "Recommended. Rules, patterns, skills and memory";
    // @OpenForgeText install.setup.fullcoresummary
    internal static string FullCoreSummary() => "Every built-in folder, all shared through Git";
    // @OpenForgeText install.setup.customsummary
    internal static string CustomSummary() => "Choose each folder on the next screen";
    // @OpenForgeText install.setup.minimalsummary
    internal static string MinimalSummary() => "Just the loader. Link the documents you already have";
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
    // @OpenForgeText install.setup.reviewcaption
    internal static string ReviewCaption() => "Review changes";
    // @OpenForgeText install.setup.currentmetadata
    internal static string CurrentMetadata(string form) => $"Current metadata form: {form}";
    // @OpenForgeText install.setup.rootdetail
    internal static string RootDetail() => "Recommended. Metadata sits at the top of each file's frontmatter.";
    // @OpenForgeText install.setup.scopeddetail
    internal static string ScopedDetail() => "Choose this if other tools already use top-level frontmatter keys in your files.";
    // @OpenForgeText install.setup.linegitignore
    internal static string LineGitIgnore() => "~ keeps the entrypoint shared through Git. New contents are Git-ignored.";
    // @OpenForgeText install.setup.linetracked
    internal static string LineTracked() => "Files already tracked by Git stay tracked.";
    // @OpenForgeText install.setup.lineleaveout
    internal static string LineLeaveOut() => "- leaves out supplied defaults. Existing files stay and remain routable.";
    // @OpenForgeText install.setup.linestopmanaging
    internal static string LineStopManaging() => "Open Forge stops managing those defaults.";
    // @OpenForgeText install.setup.essentialslabel
    internal static string EssentialsLabel() => "Essentials";
    // @OpenForgeText install.setup.fullcorelabel
    internal static string FullCoreLabel() => "Full Core";
    // @OpenForgeText install.setup.customlabel
    internal static string CustomLabel() => "Custom";
    // @OpenForgeText install.setup.minimallabel
    internal static string MinimalLabel() => "Minimal";
    // @OpenForgeText install.setup.recap
    internal static string Setup(string preset) => $"Setup: {preset}";
    // @OpenForgeText install.setup.addroutes
    internal static string AddRoutes(string routes) => $"Add: {routes}";
    // @OpenForgeText install.setup.ignoredroutes
    internal static string IgnoredRoutes(string routes) => $"Git-ignored contents: {routes}";
    // @OpenForgeText install.setup.leftoutroutes
    internal static string LeftOutRoutes(string routes) => $"Leave out: {routes}";
    // @OpenForgeText install.setup.routechange
    internal static string RouteChange(string id, string before, string after) => $"{id}: {before} -> {after}";
    // @OpenForgeText install.setup.existingfilesstay
    internal static string ExistingFilesStay() => "Existing files stay and remain routable.";
    // @OpenForgeText install.setup.none
    internal static string None() => "none";
    // @OpenForgeText install.setup.ignoredaction
    internal static string IgnoredAction() => "add with Git-ignored contents";
    // @OpenForgeText install.setup.firsttask
    internal static string FirstTask() => """
        If your agent tool does not read AGENTS.md on its own, ask it to read AGENTS.md first.
        Next: review the new files with git status and git diff, commit them, then give your agent a normal task.
        """;
    // @OpenForgeText install.setup.help
    internal static string Help() => "Use --configure to change an installed setup. Choose --preset essentials, full-core, or custom. With Custom, repeat --route <id>=<add|remove|git-ignore>. Built-in IDs: directives, guidance, maps, patterns, skills, templates, memory/working, memory/emerging, memory/crystallized, memory/archived.\nRemove keeps existing files and their routes. Add + Git-ignore shares the entrypoint, ignores route contents, and omits private child Entries. Already tracked files remain tracked.";
}
