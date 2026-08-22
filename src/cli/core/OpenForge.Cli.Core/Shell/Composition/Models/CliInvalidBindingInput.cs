using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

internal sealed record CliInvalidBindingInput(
    CliInvalidInput InvalidInput,
    CliGlobalInput GlobalInput,
    CliProcessEnvironment ProcessEnvironment,
    OpenForge.Cli.Core.Shell.Composition.CliBindingParse BindingParse);
