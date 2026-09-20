using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Help;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Detach;

internal static class LibraryDetachPresentation
{
    private static readonly LibraryDetachDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<LibraryDetachResult, LibraryDetachData> Rendering { get; } = new()
    {
        Selector = LibraryDetachReportSelector.Select,
        DataTextRenderer = LibraryDetachDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.LibraryDetachData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => LibraryDetachHelpSections.Create();
}
