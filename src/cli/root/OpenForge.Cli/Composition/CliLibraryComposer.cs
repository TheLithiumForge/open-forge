using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Commands.Library.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Serialization;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Presentation.Library.Attach.Models;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Presentation.Legacy.Library.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Interaction;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliLibraryComposer
{
    internal static CliLibraryComposition Compose(CliInteractionComposition interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var group = LibraryBinding.CreateGroup();
        var list = LibraryListBinding.CreateSymbols(group);
        var inspect = LibraryInspectBinding.CreateSymbols(group);
        var attach = LibraryAttachBinding.CreateSymbols(group);
        var sync = LibrarySyncBinding.CreateSymbols(group);
        var detach = LibraryDetachBinding.CreateSymbols(group);
        return new CliLibraryComposition
        {
            Branch = new CliRootBranch(group, LibraryHelpSections.CreateGroup()),
            ListBinding = CliReportBinding.Close(LibraryListBinding.CreateRequestBinding(list, new LibraryListBindingComponents
            {
                Help = LibraryListPresentation.CreateHelp(),
                Operation = new LibraryListOperation(),
            }), LibraryListPresentation.Rendering),
            InspectBinding = CliReportBinding.Close(LibraryInspectBinding.CreateRequestBinding(inspect, new LibraryInspectBindingComponents
            {
                Help = LibraryInspectPresentation.CreateHelp(),
                Operation = new LibraryInspectOperation(),
            }), LibraryInspectPresentation.Rendering),
            AttachBinding = CliReportBinding.Close(LibraryAttachBinding.CreateRequestBinding(attach, new LibraryAttachBindingComponents
            {
                Help = LibraryAttachPresentation.CreateHelp(),
                Operation = new LibraryAttachOperation(
                    new LibraryPermissionOperation(interaction.Prompts.PermissionAsync),
                    interaction.Prompts.PlanConfirmation<LibraryAttachResult, LibraryAttachData, LibraryAttachPlan>(
                        LibraryAttachPresentation.Rendering,
                        static (LibraryAttachPlan plan) => LibraryAttachInteractionPresentation.CreateConfirmationQuestion(plan))),
            }), LibraryAttachPresentation.Rendering),
            SyncBinding = CliReportBinding.Close(LibrarySyncBinding.CreateRequestBinding(sync, new LibrarySyncBindingComponents
            {
                Help = LibrarySyncPresentation.CreateHelp(),
                Operation = new LibrarySyncOperation(
                    new LibraryPermissionOperation(interaction.Prompts.PermissionAsync),
                    interaction.Prompts.PlanConfirmation<LibrarySyncResult, LibrarySyncData, LibrarySyncPlan>(
                        LibrarySyncPresentation.Rendering,
                        static (LibrarySyncPlan plan) => LibrarySyncInteractionPresentation.CreateConfirmationQuestion(plan))),
            }), LibrarySyncPresentation.Rendering),
            DetachBinding = CliReportBinding.Close(LibraryDetachBinding.CreateRequestBinding(detach, new LibraryDetachBindingComponents
            {
                Help = LibraryDetachPresentation.CreateHelp(),
                Operation = new LibraryDetachOperation(
                    new LibraryPermissionOperation(interaction.Prompts.PermissionAsync),
                    interaction.Prompts.PlanConfirmation<LibraryDetachResult, LibraryDetachData, LibraryDetachPlan>(
                        LibraryDetachPresentation.Rendering,
                        static (LibraryDetachPlan plan) => LibraryDetachInteractionPresentation.CreateConfirmationQuestion(plan))),
            }), LibraryDetachPresentation.Rendering),
        };
    }
}
