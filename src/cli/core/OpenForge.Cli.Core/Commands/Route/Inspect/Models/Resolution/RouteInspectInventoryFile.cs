using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectInventoryFile
{
    internal RouteInspectInventoryFile(
        string canonicalPath,
        string physicalPath,
        RouteSourceForm form,
        FileReadState readState,
        string? body)
    {
        CanonicalPath = canonicalPath;
        PhysicalPath = physicalPath;
        Form = form;
        ReadState = readState;
        Body = body;
    }

    internal string CanonicalPath { get; }

    internal string PhysicalPath { get; }

    internal RouteSourceForm Form { get; }

    internal FileReadState ReadState { get; }

    internal string? Body { get; }
}
