using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Shared;

/// <summary>
/// The command line a reader types to run a command, for the next actions one
/// command suggests about another.
///
/// Every command already owns its identity and the executable name is invariant,
/// so the full line is composed from the two rather than restated at each of the
/// 88 places one was written out.
/// </summary>
internal static class CommandLines
{
    private const string Run = CliSyntaxDefinitions.ExecutableName + " ";

    internal const string Cleanup = Run + CleanupDefinitions.CommandIdentity;
    internal const string Context = Run + ContextDefinitions.CommandIdentity;
    internal const string Doctor = Run + DoctorDefinitions.CommandIdentity;
    internal const string ExtensionCreate = Run + ExtensionCreateDefinitions.CommandIdentity;
    internal const string ExtensionInspect = Run + ExtensionInspectDefinitions.CommandIdentity;
    internal const string ExtensionList = Run + ExtensionListDefinitions.CommandIdentity;
    internal const string ExtensionUpdate = Run + ExtensionUpdateDefinitions.CommandIdentity;
    internal const string Find = Run + FindDefinitions.CommandIdentity;
    internal const string Index = Run + IndexDefinitions.CommandIdentity;
    internal const string Install = Run + InstallDefinitions.CommandIdentity;
    internal const string References = Run + ReferencesDefinitions.CommandIdentity;
    internal const string Repair = Run + RepairDefinitions.CommandIdentity;
    internal const string RouteCreate = Run + RouteCreateDefinitions.CommandIdentity;
    internal const string RouteInit = Run + RouteInitDefinitions.CommandIdentity;
    internal const string RouteInspect = Run + RouteInspectDefinitions.CommandIdentity;
    internal const string RouteList = Run + RouteListDefinitions.CommandIdentity;
    internal const string RouteMove = Run + RouteMoveDefinitions.CommandIdentity;
    internal const string RouteRemove = Run + RouteRemoveDefinitions.CommandIdentity;
    internal const string RouteUpdate = Run + RouteUpdateDefinitions.CommandIdentity;
    internal const string Status = Run + StatusDefinitions.CommandIdentity;
    internal const string Update = Run + UpdateDefinitions.CommandIdentity;
}
