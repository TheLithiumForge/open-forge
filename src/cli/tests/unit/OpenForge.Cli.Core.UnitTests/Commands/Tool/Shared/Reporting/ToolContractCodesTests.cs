using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Shared.Reporting;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Reporting;

[Trait("Feature", "tool-contracts"), Trait("Evidence", "Unit")]
public sealed class ToolContractCodesTests
{
    public static TheoryData<int, int> RunFailures => new()
    {
        { (int)ToolRunFailureCode.InvalidInput, 4 },
        { (int)ToolRunFailureCode.ToolUndeclared, 4 },
        { (int)ToolRunFailureCode.DeclaredToolMissing, 5 },
        { (int)ToolRunFailureCode.PinMismatch, 5 },
        { (int)ToolRunFailureCode.WorkspaceUnavailable, 3 },
        { (int)ToolRunFailureCode.VerificationUnavailable, 3 },
        { (int)ToolRunFailureCode.LaunchIntegrityMismatch, 5 },
        { (int)ToolRunFailureCode.StartFailed, 1 },
        { (int)ToolRunFailureCode.Cancelled, 130 },
    };

    [Theory(DisplayName = "Each pre-launch Run failure has its contract exit"), MemberData(nameof(RunFailures))]
    public void RunFailureHasExit(int code, int expected)
    {
        var failure = new ToolRunFailure((ToolRunFailureCode)code, ToolFixtureSource.Id, ToolFixtureSource.ManifestName, false) { Workspace = null };
        Assert.Equal(expected, failure.ExitCode);
    }

    [Fact(DisplayName = "Run failure cases and machine suffixes are closed and distinct")]
    public void RunCodesAreClosed()
    {
        ToolRunFailureCode[] expected = [
            ToolRunFailureCode.InvalidInput, ToolRunFailureCode.ToolUndeclared, ToolRunFailureCode.DeclaredToolMissing,
            ToolRunFailureCode.PinMismatch, ToolRunFailureCode.WorkspaceUnavailable, ToolRunFailureCode.VerificationUnavailable,
            ToolRunFailureCode.LaunchIntegrityMismatch, ToolRunFailureCode.StartFailed, ToolRunFailureCode.Cancelled,
        ];
        Assert.Equal(expected, Enum.GetValues<ToolRunFailureCode>());
        Assert.Equal(expected.Length, expected.Select(ToolRunFailureCodes.Suffix).Distinct(StringComparer.Ordinal).Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolRunFailureCodes.Suffix((ToolRunFailureCode)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolRunFailureCodes.ExitCode((ToolRunFailureCode)(-1)));
    }

    [Fact(DisplayName = "Writer machine findings are closed, distinct and reject undefined values")]
    public void WriterCodesAreClosed()
    {
        ToolWriteIssueCode[] expected = [
            ToolWriteIssueCode.InvalidInput, ToolWriteIssueCode.DeclarationMissing, ToolWriteIssueCode.LockMismatch,
            ToolWriteIssueCode.PinNotSupported, ToolWriteIssueCode.TargetUnavailable, ToolWriteIssueCode.DownloadConsentMissing,
            ToolWriteIssueCode.ProcessConsentMissing, ToolWriteIssueCode.PermissionMissing, ToolWriteIssueCode.PathExcluded,
            ToolWriteIssueCode.ComponentChoiceMissing, ToolWriteIssueCode.EntryShapeUnexpected, ToolWriteIssueCode.EntryRewriteDrift,
            ToolWriteIssueCode.RoleGuidanceDrift, ToolWriteIssueCode.StoreUnavailable, ToolWriteIssueCode.CommandFailed,
            ToolWriteIssueCode.VerificationFailed, ToolWriteIssueCode.BackupPresent, ToolWriteIssueCode.PublicationFailed,
            ToolWriteIssueCode.Interrupted, ToolWriteIssueCode.OperationFailed,
        ];
        Assert.Equal(expected, Enum.GetValues<ToolWriteIssueCode>());
        Assert.Equal(expected.Length, expected.Select(ToolContractCodes.Suffix).Distinct(StringComparer.Ordinal).Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolContractCodes.Suffix((ToolWriteIssueCode)(-1)));
    }
}
