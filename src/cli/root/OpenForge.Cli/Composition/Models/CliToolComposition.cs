using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliToolComposition(CliRootBranch Branch, ICliCommandBinding ListBinding,
    ICliCommandBinding InspectBinding, ICliCommandBinding StatusBinding);
