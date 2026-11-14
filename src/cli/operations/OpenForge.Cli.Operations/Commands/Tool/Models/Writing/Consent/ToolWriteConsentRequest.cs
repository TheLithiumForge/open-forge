using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;

internal enum ToolWriteConsentKind { Plan, AccountUninstall, SharedFileDeletion, PathChange }
internal enum ToolWriteConsentDecision { Accepted, Preview, MissingConsent, Skipped, Declined, Cancelled }

internal sealed record ToolWriteConsentQuestion
{
    public required ToolWriteConsentKind Kind { get; init; }
    public string? ToolId { get; init; }
    public required IReadOnlyList<ToolWriteEffectView> Effects { get; init; }
    public IReadOnlyList<string> Paths { get; init; } = [];
}

internal sealed record ToolWriteConsentRequest
{
    public required ToolConsent Consent { get; init; }
    public required CliFormat Format { get; init; }
    public required bool Preview { get; init; }
    public required ToolWriteConsentQuestion Question { get; init; }
}
