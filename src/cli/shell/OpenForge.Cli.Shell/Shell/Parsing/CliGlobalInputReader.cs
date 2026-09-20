using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliGlobalInputReader
{
    internal static CliGlobalInput Read(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var result = parse.Result;
        var options = parse.Options;
        var workspace = CliOptionResultFactsReader.Read(result, options.Workspace);
        var format = CliOptionResultFactsReader.Read(result, options.Format);
        var detail = CliOptionResultFactsReader.Read(result, options.Detail);
        var filter = CliOptionResultFactsReader.Read(result, options.DetailFilter);
        var workspaceValue = result.GetValue(options.Workspace);
        EnsureSingleValue(options.Workspace, workspace);
        EnsureSingleValue(options.Format, format);
        EnsureSingleValue(options.Detail, detail);
        if (filter.IsExplicitWithoutValue)
        {
            throw new ArgumentException(global::OpenForge.Cli.OutputText.Shared.ShellText.OptionRequiresValue(options.DetailFilter.Name));
        }
        if (workspace.IsExplicit && string.IsNullOrEmpty(workspaceValue))
        {
            throw new ArgumentException(global::OpenForge.Cli.OutputText.Shared.ShellText.OptionRequiresNonEmptyValue(options.Workspace.Name));
        }

        return new CliGlobalInput(
            workspaceValue, workspace.IdentifierCount, result.GetValue(options.Format), format.IdentifierCount,
            result.GetValue(options.Detail), detail.IdentifierCount, ReadFilter(result.GetValue(options.DetailFilter) ?? []), filter.IdentifierCount,
            result.GetValue(options.Help), Occurrences(result, options.Help), result.GetValue(options.Version), Occurrences(result, options.Version));
    }

    internal static IReadOnlySet<CliSeverity>? ReadFilter(IReadOnlyList<CliSeverityFilter> values)
    {
        if (values.Count == 0) return null;
        var filter = new HashSet<CliSeverity>();
        foreach (var value in values)
        {
            switch (value)
            {
                case CliSeverityFilter.All: filter.UnionWith([CliSeverity.Error, CliSeverity.Warning, CliSeverity.Info]); break;
                case CliSeverityFilter.Error: filter.Add(CliSeverity.Error); break;
                case CliSeverityFilter.Warning: filter.Add(CliSeverity.Warning); break;
                case CliSeverityFilter.Info: filter.Add(CliSeverity.Info); break;
                default: throw new ArgumentOutOfRangeException(nameof(values), value, "The severity filter is not defined.");
            }
        }

        return filter;
    }

    private static int Occurrences<T>(ParseResult result, Option<T> option) => CliOptionResultFactsReader.Read(result, option).IdentifierCount;
    private static void EnsureSingleValue<T>(Option<T> option, CliOptionResultFacts facts)
    {
        if (facts.IsExplicitWithoutValue) throw new ArgumentException(global::OpenForge.Cli.OutputText.Shared.ShellText.OptionRequiresValue(option.Name));
        if (facts.IdentifierCount > 1) throw new ArgumentException(global::OpenForge.Cli.OutputText.Shared.ShellText.OptionCannotRepeat(option.Name));
    }
}
