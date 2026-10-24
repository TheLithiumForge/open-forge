using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.TestSupport.Contracts;

// The CLI Architecture keeps published-process evidence product-free through this tracked projection.
// PublishedTokensTests checks every value against its production owner in the Unit suite.
public static class PublishedTokens
{
    public static class Install
    {
        public const string Command = "install";
        public const string Automatic = "--automatic";
        public const string DryRun = "--dry-run";
        public const string Frontmatter = "--frontmatter";
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

    public static class RouteInclude
    {
        public const string Command = "include";
        public const string Description = "--description";
        public const string RecoveryHistoryCode = "route-include.recovery-history";
    }

    public static class RouteExclude
    {
        public const string Command = "exclude";
    }

    public static class RouteCreate
    {
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
    }

    public static class RouteRemove
    {
        public const string Command = "remove";
        public const string Automatic = "--automatic";
    }

    public static class RouteInit
    {
        public const string Command = "init";
        public const string Description = "--description";
        public const string Tag = "--tag";
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

    public static class Syntax
    {
        public const string FormatOption = "--format";
        public const string DetailOption = "--detail";
        public const string JsonFormat = "json";
    }

    public static class Presentation
    {
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
    }

    public static class Settings
    {
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
