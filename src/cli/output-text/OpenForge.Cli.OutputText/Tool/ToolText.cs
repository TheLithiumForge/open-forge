namespace OpenForge.Cli.OutputText.Tool;

public static class ToolText
{
    public static string GroupUsage() => "open-forge tool <command> [options]";
    public static string CommandHelp() => "Run open-forge tool <command> --help for syntax and examples.";
    public static string ListUsage() => "open-forge tool list [--role <role>] [--source <path>] [global flags]";
    public static string InspectUsage() => "open-forge tool inspect <id> [--source <path>] [--tool-version <exact>] [global flags]";
    public static string StatusUsage() => "open-forge tool status [<id>] [global flags]";
    public static string[] ListExamples() => ["open-forge tool list", "open-forge tool list --role code-navigation --format json"];
    public static string[] InspectExamples() => ["open-forge tool inspect codegraph --source ./reviewed-manifest.json --tool-version 1.6.2",
        "open-forge tool inspect codegraph --detail full --format json"];
    public static string[] StatusExamples() => ["open-forge tool status --format json", "open-forge tool status example-tool"];
    public static string[] ReadBehavior() => ["Reads local facts without downloads, child processes or writes.", "Unavailable observations keep their cause and independently readable facts.", "Tool-owned installers handle agent configuration."];
    public static string SupplyHeading() => "Tool supply";
    public static string SupplyDetails() => """
        Open Forge ships reviewed recipes and never ships, mirrors or rehosts tool bytes.
        Native assets come from the project's official GitHub release with pinned version, URL, size, SHA-256, source commit and build-attestation evidence where published.
        Node tools use a private official portable runtime from nodejs.org/dist, pinned from signed SHASUMS256.txt.
        Its bundled npm runs npm ci from the pinned registry lock with integrity checks and package scripts disabled.
        Every command uses absolute private runtime and package paths. Your Node and PATH stay unchanged. musl/Alpine has no official Node build.
        Missing pins or dependencies, unsafe archives, mismatched size or hashes and failed required signatures or attestations block installation.
        Upstream license conditions still apply.
        """;
    public static string StoreHeading() => "Stores and settings";
    public static string StoreDetails() => """
        Settings record one pin per tool and active roles. The lock keeps recipe snapshots, hashes, files, references and command results.
        Physical store bindings and process consent stay account-local. Versioned tool and private Node bytes stay while any workspace references them.
        Inspect shows resolved paths or why they are unavailable. Status diagnoses without repairing.
        Tools never self-update. The tools.<id>.telemetry boolean defaults to false. Inspect shows the saved choice or unavailable.
        Status reports drift for an active tool when that choice differs from its configured receipt. Pending writers apply the choice.
        """;
    public static string WriterHeading() => "Planned writes";
    public static string WriterDetails() => """
        The five write commands are pending. Install downloads verified official artifacts and runs the pinned tool's installer.
        Repeating Install verifies and repairs bytes and reruns that installer. Verified local artifacts can avoid another download.
        Update selects reviewed catalogue pins only, including exact older pins for downgrades. Previously reviewed pins stay in the catalogue.
        Tool installers register MCP. Open Forge supplies the required short usage Skill and updates its records, managed ignore section, declared command rewrites and conditional role lines.
        Definitions declare first-party or third-party origin. Consent names third-party upstream software, its repository, licence, official release and authors.
        After installation, only declared command parts are rewritten. Unexpected shapes stop the write. No independent host entry or TOML parser is added.
        Optional prompt hooks use an interactive choice or --prompt-hook yes|no. Declining removes only that entry.
        Bare open-forge tool install restores every declared lock-verified pin with the same consent. Choose switches role integrations and guidance while keeping both installs and indexes.
        Remove runs its stored commands in definition-declared order, removes unused Tool ignore rules and releases the workspace's store references.
        Preview starts no transfers or processes. Downloads, session processes and destinations need separate consent or grants. Removal exclusions still apply.
        Git workspaces create no sibling backup. Without Git, existing managed files use sibling .bak files through write validation and restoration on failure.
        There is no separate Tool repair or open-path command.
        """;
    public static string RunHeading() => "Tool Run";
    public static string RunDetails() => "V1 Tool Run is pending. It runs only a declared, installed and verified workspace pin by ID, forwards arguments and streams without a shell and adds no success output. Missing tools require open-forge tool install.";
    public static string ListHeadline() => "Read available definitions and recorded tool installs.";
    public static string InspectHeadline() => "Read the selected tool definition and its requirements.";
    public static string StatusHeadline() => "Read passive tool installation and configuration evidence.";
    public static string FindingTitle() => "Tool observation";
    public static string ReadNext() => "Resolve the reported cause, then rerun the same read command.";
    public static string BackupTitle() => "Sibling backup is kept";
    public static string BackupUnavailableTitle() => "Sibling backup cannot be checked";
    public static string BackupMessage(string path) => $"A sibling backup is present at {path}. Preview restoration before changing its target.";
    public static string BackupNext() => "Review the saved file and preview its restoration before changing the target.";
    public static string Backups() => "Tool control-file backups";
    public static string BackupRestoreHeadline(bool preview) => preview ? "Preview restoring Tool control files from sibling backups." : "Read Tool control-file restoration results.";
    public static string BackupRestoreRow(string backup, bool verified) => verified ? $"Restored and verified from {backup}." : $"Restore from {backup}.";
    public static string BackupRestoreNext() => "Read the restored files, then rerun Repair for other workspace repairs.";
    public static string BackupApplyNext() => "Review the preview, then apply the shown backup restoration.";
    public static string BackupBlockedNext() => "Resolve the reported cause, then preview the backup restoration again.";
    public static string BackupRestoreFailedTitle() => "Backup restoration failed";
    public static string Origin() => "Origin";
    public static string UnknownOrigin() => "unavailable";
    public static string[] AvailableColumns() => ["ID", "Version", Origin(), "Target", "Details"];
    public static string[] InstalledColumns() => ["ID", "Version", Origin(), "Verification"];
    public static string OriginValue(string origin) => $"{Origin()}: {origin}";
    public static string Available() => "Available";
    public static string Installed() => "Installed";
    public static string Empty() => "None recorded.";
    public static string Coverage() => "Coverage";
    public static string Reviewed(bool reviewed) => reviewed ? "reviewed" : "unreviewed";
    public static string Roles() => "Roles";
    public static string Policy() => "Declared process policy";
    public static string Limits() => "Limits";
    public static string DefinitionUnavailable() => "The selected definition is unavailable.";
    public static string InstructionsUnavailable(string cause) => "External instructions unavailable: " + cause;
    public static string TelemetryChoice(bool? enabled) => enabled switch
    {
        true => "Telemetry setting: on",
        false => "Telemetry setting: off",
        null => "Telemetry setting: unavailable",
    };
}
