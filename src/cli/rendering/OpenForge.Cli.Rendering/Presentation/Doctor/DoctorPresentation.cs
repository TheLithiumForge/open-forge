using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor.Models;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Help;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Doctor;

internal static class DoctorPresentation
{
    private static readonly DoctorDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<DoctorResult, DoctorData> Rendering { get; } = new()
    {
        Selector = DoctorReportSelector.Select,
        DataTextRenderer = DoctorDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.DoctorData,
        Shape = CliCommandShape.Diagnosis,
        SelectText = static selected => selected with
        {
            TextCounts = [],
            TextLimitations = [],
            ShowNext = selected.Selection.Detail != CliDetail.Minimal
                || selected.Report.Status != CliSemanticStatus.Attention,
        },
    };

    internal static CliHelpContent CreateHelp()
        => DoctorHelpSections.Create();
}
