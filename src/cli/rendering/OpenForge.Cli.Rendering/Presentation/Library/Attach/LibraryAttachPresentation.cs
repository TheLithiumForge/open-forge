using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Attach.Models;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Help;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Attach;

internal static class LibraryAttachPresentation
{
    private static readonly LibraryAttachDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<LibraryAttachResult, LibraryAttachData> Rendering { get; } = new()
    {
        Selector = LibraryAttachReportSelector.Select,
        DataTextRenderer = LibraryAttachDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.LibraryAttachData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => LibraryAttachHelpSections.Create();
}
