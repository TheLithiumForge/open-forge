using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Parsing;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run;

// Registration point: parse the composed tree before its report pipeline sees child tokens.
internal static class ToolRunRequestParser
{
    private const string Delimiter = "--";

    internal static ToolRunInput Parse(string[] arguments, ToolRunEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(environment);
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var id = new Argument<string>("tool") { Arity = ArgumentArity.ExactlyOne };
        tree.Root.Add(id);
        return Parse(tree, id, arguments, environment);
    }

    internal static ToolRunInput Parse(CliCommandTree tree, Argument<string> id, string[] arguments, ToolRunEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(environment);

        // Parse prefixes through the library until it assigns the first operand.
        // Child tokens never enter the Open Forge parser, including help and response-file syntax.
        CliParseOutcome parsed = tree.Parse([]);
        var consumed = 0;
        for (; consumed < arguments.Length; consumed++)
        {
            parsed = tree.Parse(arguments[..(consumed + 1)]);
            if (parsed.Result.GetResult(id) is { Tokens.Count: > 0 }) { consumed++; break; }
        }
        try
        {
            if (parsed.Result.GetValue(tree.Options.Help) || parsed.Result.GetValue(tree.Options.Version))
                return new(null, CliGlobalInputReader.Read(parsed), []);
            if (parsed.Result.Errors.Count != 0)
                return new(null, null, [.. parsed.Result.Errors.Select(error => error.Message)]);
            var global = CliGlobalInputReader.Read(parsed);
            var tool = parsed.Result.GetValue(id);
            if (!ToolIdentity.IsToolId(tool)) return new(null, global, ["A declared tool ID is required."]);
            if (consumed < arguments.Length && arguments[consumed] == Delimiter) consumed++;
            return new(new()
            {
                WorkspacePath = global.WorkspaceValue,
                CurrentDirectory = environment.CurrentDirectory,
                HostProjectDirectory = environment.HostProjectDirectory,
                Interactive = environment.Interactive && global.OutputFormat != CliFormat.Json,
                Id = tool ?? throw new InvalidOperationException("The validated tool ID is unavailable."),
                Arguments = arguments[consumed..],
            }, global, []);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException) { return new(null, null, [exception.Message]); }
    }
}
