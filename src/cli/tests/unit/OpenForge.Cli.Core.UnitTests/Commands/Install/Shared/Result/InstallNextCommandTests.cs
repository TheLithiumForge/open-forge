using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Result;

public sealed class InstallNextCommandTests
{
    [Theory(DisplayName = "Resolved Install setup takes precedence over original flags in an occupied-target preview")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ResolvedSetupSurvivesRetry(bool custom)
    {
        var routes = new[] { new InstallRouteSelection("guidance", InstallRouteAction.GitIgnore), new InstallRouteSelection("maps", InstallRouteAction.Remove) };
        var input = new InstallBindingInput(false, false, InstallMode.Apply,
            Configuration: new(false, custom ? InstallPreset.Custom : InstallPreset.Essentials, [.. routes]))
        { ResolvedFrontmatter = FrontmatterForm.Root };
        var result = InstallResult.Invalid(input, null, [new(InstallFindingCode.TargetOccupied, "An existing file occupies the target.")]);
        var command = Assert.IsType<CliNextAction>(InstallReportSelector.Select(result, new(CliDetail.Minimal)).Next).Command;
        Assert.Equal(custom
            ? "open-forge install --preset custom --route guidance=git-ignore --route maps=remove --frontmatter root --force --dry-run"
            : "open-forge install --preset essentials --frontmatter root --force --dry-run", command);
        Assert.Equal(result.Next?.Command, command);
    }

    [Theory(DisplayName = "Install retry retains explicit setup and output choices in text and JSON next facts")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void RetryRetainsTypedRequest(bool automatic)
    {
        var code = automatic ? InstallFindingCode.ConfirmationRequired : InstallFindingCode.Interrupted;
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "selected workspace"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var input = new InstallBindingInput(true, false, InstallMode.Apply,
            new(true, InstallPreset.Custom, [new("guidance", InstallRouteAction.GitIgnore), new("maps", InstallRouteAction.Remove)])
            { Frontmatter = FrontmatterForm.Scoped })
        { RetryOptions = new(CliFormat.Json, CliDetail.Standard, new HashSet<CliSeverity> { CliSeverity.Warning, CliSeverity.Error }) };
        var result = InstallResult.Invalid(input, workspace, [new(code, "Stopped before changes.")]);
        var next = Assert.IsType<CliNextAction>(InstallReportSelector.Select(result, new(CliDetail.Minimal)).Next);
        Assert.Equal(result.Next?.Command, next.Command);
        Assert.Contains($"--workspace '{root}'", next.Command, StringComparison.Ordinal);
        Assert.Contains("--configure --preset custom --route guidance=git-ignore --route maps=remove --frontmatter scoped --force", next.Command, StringComparison.Ordinal);
        Assert.Contains("--format json --detail standard --detail-filter error --detail-filter warning", next.Command, StringComparison.Ordinal);
        Assert.Equal(automatic, next.Command.Contains("--automatic", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Install retry preserves each supported detail level")]
    [InlineData(0, "minimal"), InlineData(1, "standard"), InlineData(2, "full"), InlineData(3, "debug")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void DetailIsPreserved(int detail, string name)
    {
        var input = new InstallBindingInput(false, false, InstallMode.Apply) { RetryOptions = new(CliFormat.Text, (CliDetail)detail, null) };
        Assert.Equal($"open-forge install --detail {name}", InstallNextCommand.Create(input, null));
    }

    [Fact(DisplayName = "Install retry rejects undefined detail and severity values")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void UndefinedOutputValuesAreRejected()
    {
        var input = new InstallBindingInput(false, false, InstallMode.Apply);
        Assert.Throws<ArgumentOutOfRangeException>(() => InstallNextCommand.Create(input with { RetryOptions = new(CliFormat.Text, (CliDetail)99, null) }, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => InstallNextCommand.Create(input with
        { RetryOptions = new(CliFormat.Text, null, new HashSet<CliSeverity> { (CliSeverity)99 }) }, null));
    }

    [Theory(DisplayName = "Install retry preserves every supported finding filter")]
    [InlineData(0, "error"), InlineData(1, "warning"), InlineData(2, "info")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void SeverityIsPreserved(int severity, string name)
    {
        var input = new InstallBindingInput(false, false, InstallMode.Apply)
        { RetryOptions = new(CliFormat.Text, null, new HashSet<CliSeverity> { (CliSeverity)severity }) };
        Assert.Equal($"open-forge install --detail-filter {name}", InstallNextCommand.Create(input, null));
    }

    [Fact(DisplayName = "A cancelled Configure resumes the Configure wizard without automatic mode")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ConfigureCancellationResumesWizard()
    {
        var result = InstallResult.Invalid(new(false, false, InstallMode.Apply, new(true, null, [])), null,
            [new(InstallFindingCode.Interrupted, "Setup was cancelled.")]);
        Assert.Equal("open-forge install --configure", result.Next?.Command);
    }

    [Fact(DisplayName = "An occupied-target preview keeps setup choices and adds no automatic confirmation")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void OccupiedPreviewRetainsSetup()
    {
        var result = InstallResult.Invalid(new(false, false, InstallMode.Apply, new(false, InstallPreset.Essentials, [])
        { Frontmatter = FrontmatterForm.Scoped }), null, [new(InstallFindingCode.TargetOccupied, "An existing file occupies the target.")]);
        var next = InstallReportSelector.Select(result, new(CliDetail.Minimal)).Next;
        Assert.Equal("open-forge install --preset essentials --frontmatter scoped --force --dry-run", next?.Command);
        Assert.Equal(result.Next?.Command, next?.Command);
    }
}
