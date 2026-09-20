using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List.Models;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Help;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.List;

internal static class LibraryListPresentation
{
    private static readonly LibraryListDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<LibraryListResult, LibraryListData> Rendering { get; } = new()
    {
        Selector = LibraryListReportSelector.Select,
        DataTextRenderer = LibraryListDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.LibraryListData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with
        {
            ShowHeadline = !(selected.Report.Status == CliSemanticStatus.Complete
                && selected.Report.Data.Libraries.Count > 0
                && selected.Report.Findings.Count == 0),
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => LibraryListHelpSections.Create();
}

