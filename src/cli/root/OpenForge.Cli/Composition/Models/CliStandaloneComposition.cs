using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliStandaloneComposition
{
    public required ICliCommandBinding FindBinding { get; init; }

    public required ICliCommandBinding IndexBinding { get; init; }

    public required ICliCommandBinding StatusBinding { get; init; }

    public required ICliCommandBinding DoctorBinding { get; init; }

    public required ICliCommandBinding RepairBinding { get; init; }

    public required ICliCommandBinding CleanupBinding { get; init; }

    public required ICliCommandBinding ContextBinding { get; init; }

    public required ICliCommandBinding ReferencesBinding { get; init; }

    public required ICliCommandBinding InstallBinding { get; init; }

    public required ICliCommandBinding UpdateBinding { get; init; }

    public required IReadOnlyList<CliRootLeaf> RootLeaves { get; init; }
}
