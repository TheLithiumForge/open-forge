using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;

internal static class ToolReadIssueCodes
{
    internal static string Suffix(ToolReadIssueCode code) => code switch
    {
        ToolReadIssueCode.InvalidInput => "invalid-input",
        ToolReadIssueCode.WorkspaceUnavailable => "workspace-unavailable",
        ToolReadIssueCode.CatalogueInvalid => "catalogue-invalid",
        ToolReadIssueCode.CatalogueUnavailable => "catalogue-unavailable",
        ToolReadIssueCode.DefinitionInvalid => "definition-invalid",
        ToolReadIssueCode.PinNotSupported => "pin-not-supported",
        ToolReadIssueCode.SettingsUnavailable => "settings-unavailable",
        ToolReadIssueCode.LockUnavailable => "lock-unavailable",
        ToolReadIssueCode.ReceiptMissing => "receipt-missing",
        ToolReadIssueCode.ReceiptMismatch => "receipt-mismatch",
        ToolReadIssueCode.StoreUnavailable => "store-unavailable",
        ToolReadIssueCode.InstalledFileMissing => "installed-file-missing",
        ToolReadIssueCode.InstalledFileChanged => "installed-file-changed",
        ToolReadIssueCode.MemberChanged => "member-changed",
        ToolReadIssueCode.HostUnavailable => "host-unavailable",
        ToolReadIssueCode.IndexMissing => "index-missing",
        ToolReadIssueCode.IndexUnavailable => "index-unavailable",
        ToolReadIssueCode.UsageSkillMissing => "usage-skill-missing",
        ToolReadIssueCode.UsageSkillChanged => "usage-skill-changed",
        ToolReadIssueCode.PathExcluded => "path-excluded",
        ToolReadIssueCode.UsageSkillUnavailable => "usage-skill-unavailable",
        ToolReadIssueCode.ToolNotInstalled => "tool-not-installed",
        ToolReadIssueCode.NativeLimits => "native-limits",
        ToolReadIssueCode.Cancelled => "cancelled",
        ToolReadIssueCode.ReadUnavailable => "read-unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
