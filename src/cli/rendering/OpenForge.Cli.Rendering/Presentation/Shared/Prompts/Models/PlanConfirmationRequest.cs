using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;

internal sealed record PlanConfirmationRequest<TResult, TData, TQuestion>(
    CliReportRendering<TResult, TData> Rendering,
    Func<TQuestion, CliConfirmQuestion> Question,
    TResult Preview,
    TQuestion Facts,
    CliPromptPolicy Policy)
    where TResult : ICliCommandResult
    where TData : class
    where TQuestion : notnull;
