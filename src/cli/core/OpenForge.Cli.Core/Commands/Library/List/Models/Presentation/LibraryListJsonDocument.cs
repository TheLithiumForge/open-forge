using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Presentation;

internal sealed record LibraryListJsonDocument
{
    public required int SchemaVersion { get; init; }
    public required string Command { get; init; }
    [JsonConverter(typeof(LibrarySemanticStatusConverter))]
    public required CliSemanticStatus Status { get; init; }
    public required LibraryJsonWorkspace? Workspace { get; init; }
    public required LibraryListPayload Result { get; init; }
    public required LibraryJsonNext? Next { get; init; }
}
