using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Presentation.Models;

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
            new CliPresentation(CliFormat.Json, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(Workspace.LexicalRoot, Workspace.LexicalRoot),
            Workspace);

    internal static CliInvalidBindingInput ParserInvalid(CliBindingParse parse)
        => Invalid(parse, new CliInvalidInput("cli.parser.invalid", CliInvalidInputSource.Parser,
            parse.Result.Errors.Select(error => error.Message)));

    internal static CliInvalidBindingInput Invalid(CliBindingParse parse, CliInvalidInput input)
        => new(
            input,
            new CliGlobalInput(
                WorkspaceValue: Workspace.LexicalRoot,
                WorkspaceOccurrences: 1,
                OutputFormat: CliFormat.Json,
                FormatOccurrences: 1,
                Detail: CliDetail.Standard,
                DetailOccurrences: 0,
                Filter: null,
                FilterOccurrences: 0,
                Help: false,
                HelpOccurrences: 0,
                Version: false,
                VersionOccurrences: 0),
            new CliProcessEnvironment(Workspace.LexicalRoot),
            parse);
}
