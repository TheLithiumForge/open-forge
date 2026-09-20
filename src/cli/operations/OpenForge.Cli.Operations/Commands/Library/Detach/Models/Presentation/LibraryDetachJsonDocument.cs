using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Presentation;

internal sealed record LibraryDetachJsonDocument
{
    public required int SchemaVersion { get; init; }
    public required string Command { get; init; }
    [JsonConverter(typeof(LibrarySemanticStatusConverter))]
    public required CliSemanticStatus Status { get; init; }
    public required LibraryJsonWorkspace? Workspace { get; init; }
    public required LibraryDetachPayload Result { get; init; }
    public required LibraryJsonNext? Next { get; init; }
}
