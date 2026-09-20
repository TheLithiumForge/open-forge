using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Install;

internal static class ExtensionInstallPresentation
{
    private static readonly ExtensionInstallDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<ExtensionInstallResult, ExtensionInstallData> Rendering { get; } = new()
    {
        Selector = ExtensionInstallReportSelector.Select,
        DataTextRenderer = ExtensionInstallDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionInstallData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard
                || selected.Report.Status is CliSemanticStatus.Blocked
                    or CliSemanticStatus.Failed
                    or CliSemanticStatus.Interrupted,
            TextEffects = [],
            TextCounts = [],
            ShowNext = selected.Report.Data.TextNextLines.Count == 0,
        },
    };

    internal static CliHelpContent CreateHelp()
        => ExtensionInstallHelpSections.Create();
}
