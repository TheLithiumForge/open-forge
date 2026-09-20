using System.CommandLine;
using OpenForge.Cli.Core.Shell.Composition;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Results;

internal enum CliBindingSelectionState
{
    Root,
    Group,
    Leaf,
    NoLeaf,
}

internal sealed record CliBindingSelection(
    CliBindingSelectionState State,
    Command Command,
    ICliCommandBinding? Binding);
