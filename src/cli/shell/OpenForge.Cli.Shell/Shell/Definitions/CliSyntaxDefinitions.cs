using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliOptionArity { None, ExactlyOne }

internal static class CliSyntaxDefinitions
{
    internal const string ExecutableName = "open-forge";
    internal static readonly CliSyntaxDefinition Root = new(ExecutableName, global::OpenForge.Cli.OutputText.Shared.SharedText.HelpRootDescription());
    internal static readonly CliOptionDefinition<string?> Workspace = new("--workspace", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpWorkspaceDescription(), CliOptionArity.ExactlyOne, null, "path");
    internal static readonly CliOptionDefinition<CliFormat> Format = new(
        "--format", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpFormatDescription(), CliOptionArity.ExactlyOne, CliFormat.Text, "format",
        new Dictionary<string, CliFormat>(StringComparer.Ordinal) { ["text"] = CliFormat.Text, ["json"] = CliFormat.Json });
    internal static readonly CliOptionDefinition<CliDetail> Detail = new(
        "--detail", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpDetailDescription(), CliOptionArity.ExactlyOne, CliDetail.Minimal, "detail",
        new Dictionary<string, CliDetail>(StringComparer.Ordinal)
        {
            ["minimal"] = CliDetail.Minimal,
            ["standard"] = CliDetail.Standard,
            ["full"] = CliDetail.Full,
            ["debug"] = CliDetail.Debug,
        });
    internal static readonly CliOptionDefinition<CliSeverityFilter> DetailFilter = new(
        "--detail-filter", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpSeverityDescription(), CliOptionArity.ExactlyOne, CliSeverityFilter.All, "severity",
        new Dictionary<string, CliSeverityFilter>(StringComparer.Ordinal)
        {
            ["error"] = CliSeverityFilter.Error,
            ["warning"] = CliSeverityFilter.Warning,
            ["info"] = CliSeverityFilter.Info,
            ["all"] = CliSeverityFilter.All,
        });
    internal static readonly CliOptionDefinition<bool> Help = new("--help", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHelpDescription(), CliOptionArity.None, false);
    internal static readonly CliOptionDefinition<bool> Version = new("--version", global::OpenForge.Cli.OutputText.Shared.SharedText.HelpVersionDescription(), CliOptionArity.None, false);
}
