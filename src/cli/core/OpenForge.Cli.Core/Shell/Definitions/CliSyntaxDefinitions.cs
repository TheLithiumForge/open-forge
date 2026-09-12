using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliOptionArity
{
    None,
    ExactlyOne,
}

internal static class CliSyntaxDefinitions
{
    internal const string ExecutableName = "open-forge";

    internal static readonly CliSyntaxDefinition Root = new(
        ExecutableName,
        "Inspect and maintain an Open Forge workspace.");

    internal static readonly CliOptionDefinition<string?> Workspace = new(
        "--workspace",
        "Select the workspace explicitly.",
        CliOptionArity.ExactlyOne,
        null,
        "path");

    internal static readonly CliOptionDefinition<bool> Json = new(
        "--json",
        "Write the complete structured result as JSON.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<CliView> View = new(
        "--view",
        "Select compact or expanded human output.",
        CliOptionArity.ExactlyOne,
        CliView.Expanded,
        "view",
        new Dictionary<string, CliView>(StringComparer.Ordinal)
        {
            [CliPresentationDefinitions.Compact] = CliView.Compact,
            [CliPresentationDefinitions.Expanded] = CliView.Expanded,
        });

    internal static readonly CliOptionDefinition<bool> Verbose = new(
        "--verbose",
        "Write bounded diagnostics to stderr.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Help = new(
        "--help",
        "Show help and exit.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Version = new(
        "--version",
        "Show the executable version and exit.",
        CliOptionArity.None,
        false);
}
