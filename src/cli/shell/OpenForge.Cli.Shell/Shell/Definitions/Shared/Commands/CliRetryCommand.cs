using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

// Task 78's complete-next-command contract requires one quoting and presentation policy.
internal static class CliRetryCommand
{
    internal static void Add(List<string> arguments, string option, string? value)
    {
        if (value is null) return;
        arguments.Add(option);
        arguments.Add(value);
    }

    internal static string Build(IEnumerable<string> arguments)
        => $"open-forge {string.Join(' ', arguments.Select(CliCommandText.Quote))}";

    internal static string BuildInWorkspace(IEnumerable<string> arguments, string? workspace, bool explicitWorkspace)
    {
        var values = arguments.ToList();
        if (explicitWorkspace) Add(values, "--workspace", workspace);
        return Build(values);
    }

    internal static void AddPresentation(List<string> arguments, CliRetryOptions? options, bool diagnostics = false)
    {
        if (options is not null)
        {
            if (options.Format == CliFormat.Json) Add(arguments, "--format", "json");
            if (!diagnostics && options.Detail is { } detail) Add(arguments, "--detail", DetailName(detail));
            if (options.Filter is { } filter)
                foreach (var severity in filter.Order()) Add(arguments, "--detail-filter", SeverityName(severity));
        }
        if (diagnostics) Add(arguments, "--detail", CliPresentationDefinitions.Debug);
    }

    internal static CliNextAction? InWorkspace(CliNextAction? next, string? workspace, bool explicitWorkspace)
        => next is { Kind: CliNextActionKind.Command } && explicitWorkspace && workspace is not null
            && !next.Command.EndsWith(" --help", StringComparison.Ordinal)
            && !next.Command.Contains(" --workspace ", StringComparison.Ordinal)
            && !next.Command.Contains(" --workspace=", StringComparison.Ordinal)
            && !next.Command.Contains(" --workspace:", StringComparison.Ordinal)
            ? new CliNextAction($"{next.Command} --workspace {CliCommandText.Quote(workspace)}", next.Reason) { Kind = next.Kind }
            : next;

    private static string DetailName(CliDetail detail) => detail switch
    {
        CliDetail.Minimal => CliPresentationDefinitions.Minimal,
        CliDetail.Standard => CliPresentationDefinitions.Standard,
        CliDetail.Full => CliPresentationDefinitions.Full,
        CliDetail.Debug => CliPresentationDefinitions.Debug,
        _ => throw new ArgumentOutOfRangeException(nameof(detail)),
    };

    private static string SeverityName(CliSeverity severity) => severity switch
    {
        CliSeverity.Error => "error",
        CliSeverity.Warning => "warning",
        CliSeverity.Info => "info",
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    };
}
