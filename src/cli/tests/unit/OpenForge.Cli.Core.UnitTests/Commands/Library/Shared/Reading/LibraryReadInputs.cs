using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

internal static class LibraryReadInputs
{
    internal static CliWorkspace Workspace { get; } = new(
        lexicalRoot: Path.GetFullPath(Path.Combine(Path.GetTempPath(), "library-read-unit")),
        physicalRoot: Path.GetFullPath(Path.Combine(Path.GetTempPath(), "library-read-unit")),
        selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static CliInvocation Invocation()
        => new(
            new CliProcessIdentity("open-forge", "0.0.0-dev"),
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(Workspace.LexicalRoot, Workspace.LexicalRoot),
            Workspace);

    internal static CliInvalidBindingInput ParserInvalid(CliBindingParse parse)
        => new(
            new CliInvalidInput("cli.parser.invalid", CliInvalidInputSource.Parser, parse.Result.Errors.Select(error => error.Message)),
            new CliGlobalInput(
                WorkspaceValue: Workspace.LexicalRoot,
                WorkspaceOccurrences: 1,
                OutputFormat: CliOutputFormat.Json,
                JsonOccurrences: 1,
                View: CliView.Expanded,
                ViewOccurrences: 0,
                Verbosity: CliVerbosity.Normal,
                VerboseOccurrences: 0,
                Help: false,
                HelpOccurrences: 0,
                Version: false,
                VersionOccurrences: 0),
            new CliProcessEnvironment(Workspace.LexicalRoot),
            parse);
}
