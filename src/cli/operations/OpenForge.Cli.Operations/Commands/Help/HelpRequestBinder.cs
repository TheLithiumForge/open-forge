using OpenForge.Cli.Core.Commands.Help.Models.Binding;
using OpenForge.Cli.Core.Commands.Help.Models.Request;
using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Help;

internal sealed class HelpRequestBinder(HelpSymbols symbols)
{
    internal CliBindResult<HelpRequest, HelpResult> Bind(CliBindingParse parse, CliInvocation invocation)
    {
        var topic = parse.Result.GetValue(symbols.Topic);
        var namesCommand = parse.Result.RootCommandResult.Command.Subcommands
            .Any(command => string.Equals(command.Name, topic, StringComparison.Ordinal));
        return CliBindResult<HelpRequest, HelpResult>.Bound(new HelpRequest(topic, namesCommand));
    }
}
