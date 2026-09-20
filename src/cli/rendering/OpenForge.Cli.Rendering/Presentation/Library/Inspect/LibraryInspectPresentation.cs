using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect;

internal static class LibraryInspectPresentation
{
    private static readonly LibraryInspectDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<LibraryInspectResult, LibraryInspectData> Rendering { get; } = new()
    {
        Selector = LibraryInspectReportSelector.Select,
        DataTextRenderer = LibraryInspectDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.LibraryInspectData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with { TextCounts = [] },
    };

    internal static CliHelpContent CreateHelp()
        => LibraryInspectHelpSections.Create();
}
