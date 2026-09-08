using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Rendering;

namespace OpenForge.Cli.Composition;

internal static class CliLibraryComposer
{
    internal static CliLibraryComposition Compose()
    {
        var group = LibraryBinding.CreateGroup();
        var list = LibraryListBinding.CreateSymbols(group);
        var inspect = LibraryInspectBinding.CreateSymbols(group);
        var attach = LibraryAttachBinding.CreateSymbols(group);
        var sync = LibrarySyncBinding.CreateSymbols(group);
        var detach = LibraryDetachBinding.CreateSymbols(group);
        return new CliLibraryComposition
        {
            Branch = new CliRootBranch(group, LibraryHelpSections.CreateGroup(), []),
            ListBinding = LibraryListBinding.Close(list, new LibraryListBindingComponents
            {
                Help = LibraryListPresentation.CreateHelp(),
                Operation = new LibraryListOperation(),
                Renderers = new CliRendererSet<LibraryListResult>(LibraryListPresentation.RenderHuman, LibraryListPresentation.RenderJson),
            }),
            InspectBinding = LibraryInspectBinding.Close(inspect, new LibraryInspectBindingComponents
            {
                Help = LibraryInspectPresentation.CreateHelp(),
                Operation = new LibraryInspectOperation(),
                Renderers = new CliRendererSet<LibraryInspectResult>(LibraryInspectPresentation.RenderHuman, LibraryInspectPresentation.RenderJson),
            }),
            AttachBinding = LibraryAttachBinding.Close(attach, new LibraryAttachBindingComponents
            {
                Help = LibraryAttachPresentation.CreateHelp(),
                Renderers = new CliRendererSet<LibraryAttachResult>(LibraryAttachPresentation.RenderHuman, LibraryAttachPresentation.RenderJson),
            }),
            SyncBinding = LibrarySyncBinding.Close(sync, new LibrarySyncBindingComponents
            {
                Help = LibrarySyncPresentation.CreateHelp(),
                Renderers = new CliRendererSet<LibrarySyncResult>(LibrarySyncPresentation.RenderHuman, LibrarySyncPresentation.RenderJson),
            }),
            DetachBinding = LibraryDetachBinding.Close(detach, new LibraryDetachBindingComponents
            {
                Help = LibraryDetachPresentation.CreateHelp(),
                Renderers = new CliRendererSet<LibraryDetachResult>(LibraryDetachPresentation.RenderHuman, LibraryDetachPresentation.RenderJson),
            }),
        };
    }
}
