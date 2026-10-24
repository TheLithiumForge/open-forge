using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.TestSupport.Contracts;

// The CLI Architecture keeps published-process evidence product-free through this tracked projection.
// PublishedTokensTests checks every value against its production owner in the Unit suite.
public static class PublishedTokens
{
    public static class Context
    {
        public const string Command = "context";
        public const string Content = "--content";
        public const string FollowLinks = "--follow-links";
        public const string All = "all";
        public const string Frontmatter = "frontmatter";
    }

    public static class Find
    {
        public const string Metadata = "metadata";
        public const string Command = "find";
        public const string Heading = "--heading";
        public const string Content = "--content";
        public const string Exclude = "--exclude";
        public const string Body = "body";
    }

    public static class References
    {
        public const string Command = "references";
    }

    public static class Layout
    {
        public const string IndexFile = "index.md";
        public const string LiteralReferencePrefix = "./";
        public const string LoaderFileName = "loader.md";
        public const string GuideFile = "README.md";
        public const string AgentHost = "AGENTS.md";
        public const string NativeSkillsPath = ".agents/skills";
        public const string SkillsDirectory = "skills";
        public const string NativeSkillFileName = "SKILL.md";
        public const string UnderscoreReferencesFileName = "_references.md";
        public const string MarkdownExtension = ".md";
        public const string OverwriteSuffix = ".overwrite.md";
    }

    public static class Metadata
    {
        public const string EmptyEntry = "- none - No entries - #Empty";
        public const string SkillName = "name";
        public const string Description = "description";
        public const string Tags = "tags";
        public const string Entries = "Entries";
        public const string LoadNow = "LoadNow";
    }

    public static class ContextData
    {
        public const string Sources = "sources";
    }

    public static class FindData
    {
        public const string Matches = "matches";
        public const string Parts = "parts";
    }

    public static class ReferencesData
    {
        public const string Scanned = "scanned";
        public const string Source = "source";
        public const string Incoming = "incoming";
        public const string Outgoing = "outgoing";
        public const string ResolvedPath = "resolvedPath";
    }

    public static class Install
    {
        public const string Configure = "--configure";
        public const string Preset = "--preset";
        public const string CustomPreset = "custom";
        public const string Command = "install";
        public const string Automatic = "--automatic";
        public const string DryRun = "--dry-run";
        public const string Frontmatter = "--frontmatter";
        public const string WorkspaceUnsafeCode = "install.workspace-unsafe";
    }

    public static class Index
    {
        public const string Command = "index";
        public const string DryRun = "--dry-run";
    }

    public static class Status
    {
        public const string Command = "status";
    }

    public static class Doctor
    {
        public const string Command = "doctor";
    }

    public static class Cleanup
    {
        public const string Command = "cleanup";
        public const string DryRun = "--dry-run";
    }

    public static class Remove
    {
        public const string Command = "remove";
        public const string Automatic = "--automatic";
    }

    public static class Route
    {
        public const string Command = "route";
    }

    public static class RouteList
    {
        public const int DefaultDepth = 1;
        public const string Command = "list";
        public const string CommandIdentity = "route list";
        public const string Depth = "--depth";
        public const string AllDepth = "all";
        public const string InvalidDepthCode = "route-list.invalid-depth";
    }

    public static class RouteListData
    {
        public const string Rows = "rows";
        public const string Depth = "depth";
        public const string HasOverwrite = "hasOverwrite";
        public const string AbsoluteDepth = "absoluteDepth";
    }

    public static class RouteInclude
    {
        public const string DryRun = "--dry-run";
        public const string Tag = "--tag";
        public const string ApplyTo = "--apply-to";
        public const string Command = "include";
        public const string Description = "--description";
        public const string RecoveryHistoryCode = "route-include.recovery-history";
    }

    public static class RouteInspect
    {
        public const string Command = "inspect";
    }

    public static class RouteInspectData
    {
        public const string Read = "read";
        public const string AtStart = "atStart";
    }

    public static class RouteExclude
    {
        public const string DryRun = "--dry-run";
        public const string Command = "exclude";
    }

    public static class Update
    {
        public const string Command = "update";
        public const string Automatic = "--automatic";
    }

    public static class RouteCreate
    {
        public const string DryRun = "--dry-run";
        public const string Command = "create";
        public const string Description = "--description";
        public const string Tag = "--tag";
    }

    public static class RouteUpdate
    {
        public const string Command = "update";
        public const string Description = "--description";
    }

    public static class RouteMove
    {
        public const string Command = "move";
        public const string DryRun = "--dry-run";
    }

    public static class RouteRemove
    {
        public const string DryRun = "--dry-run";
        public const string Command = "remove";
        public const string Automatic = "--automatic";
    }

    public static class RouteInit
    {
        public const string Command = "init";
        public const string Description = "--description";
        public const string Tag = "--tag";
        public const string DryRun = "--dry-run";
        public const string Framework = "--framework";
    }

    public static class Extension
    {
        public const string Command = "extension";
    }

    public static class ExtensionInstall
    {
        public const string Command = "install";
        public const string Automatic = "--automatic";
        public const string Source = "--source";
    }

    public static class ExtensionUpdate
    {
        public const string Command = "update";
        public const string Automatic = "--automatic";
        public const string Source = "--source";
    }

    public static class ExtensionRemove
    {
        public const string Command = "remove";
        public const string Automatic = "--automatic";
    }

    public static class ExtensionInspect
    {
        public const string Command = "inspect";
        public const string Source = "--source";
        public const string Files = "files";
        public const string Matches = "matches";
    }

    public static class ExtensionList
    {
        public const string Command = "list";
        public const string Installed = "--installed";
        public const string InstalledData = "installed";
    }

    public static class Syntax
    {
        public const string TextFormat = "text";
        public const string FormatOption = "--format";
        public const string DetailOption = "--detail";
        public const string JsonFormat = "json";
    }

    public static class Presentation
    {
        public const string MinimalDetail = "minimal";
        public const string FullDetail = "full";
    }

    public static class DataHome
    {
        public const string Variable = TestDataHome.Variable;
    }

    public static class Runtime
    {
        public const string IgnoreContent = "*\n";
    }

    public static class Sources
    {
        public const string AgentsRoot = ".agents";
        public const string LoaderPath = ".agents/loader.md";
    }

    public static class Ownership
    {
        public const string RelativePath = ".agents/open-forge.lock.json";
        public const string Framework = "framework";
        public const string Source = "source";
        public const string Id = "id";
    }

    public static class Settings
    {
        public const string RemovedCategoriesProperty = "removedCategories";
        public const string RemovedDirectoriesProperty = "removedDirectories";
        public const string RootsProperty = "roots";
        public const string RelativePath = ".agents/open-forge.json";
        public const int SchemaVersion = 4;
        public const string SchemaVersionProperty = "schemaVersion";
        public const string MainRootProperty = "mainRoot";
        public const string FrontmatterProperty = "frontmatter";
        public const string RootFrontmatter = "root";
        public const string ScopedFrontmatter = "scoped";
    }

    public static class ExtensionPackage
    {
        public const string ContentDirectory = "content";
    }

    public static class RecoveryFormat
    {
        public const string FinalFilePrefix = "operation-";
        public const string FinalFileExtension = ".zip";
        public const string DraftFileExtension = ".draft";
        public const string OperationIdFormat = "N";
    }

    public static class Recovery
    {
        public const string WorkspaceStore = "workspace";
        public const string LegacyStore = "legacy";
        public const string VerifiedIntegrity = "verified";
    }

    public static class Statuses
    {
        public const int CompleteExitCode = 0;
        public const int InvalidExitCode = 4;
        public const string Invalid = "invalid-input";
        public const string Complete = "completed";
        public const string Attention = "completed-with-warnings";
        public const string Incomplete = "incomplete";
    }

    public static class Report
    {
        public const string WarningSeverity = "warning";
    }

    public static class Json
    {
        public const string Effects = "effects";
        public const string After = "after";
        public const string Id = "id";
        public const string Data = "data";
        public const string Command = "command";
        public const string Status = "status";
        public const string Findings = "findings";
        public const string Code = "code";
        public const string Severity = "severity";
        public const string Subject = "subject";
        public const string Path = "path";
    }

    public static class StatusRecovery
    {
        public const string Recovery = "recovery";
        public const string Stores = "stores";
        public const string Candidates = "candidates";
        public const string Kind = "kind";
        public const string Path = "path";
        public const string Integrity = "integrity";
    }

    public static class DoctorData
    {
        public const string RecoveryStores = "recoveryStores";
        public const string Kind = "kind";
        public const string Path = "path";
        public const string Candidates = "candidates";
    }

    public static class CleanupData
    {
        public const string Stores = "stores";
        public const string Kind = "kind";
        public const string Path = "path";
        public const string Candidates = "candidates";
        public const string Cause = "cause";
    }
}
