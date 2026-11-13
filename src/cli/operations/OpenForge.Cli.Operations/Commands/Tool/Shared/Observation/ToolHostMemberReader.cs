using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Shared.Configuration;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal static class ToolHostMemberReader
{
    internal static ToolObservation Compare(ToolMemberReceipt member, byte[] bytes)
    {
        var read = ToolHostConfigReader.ReadMember(member, bytes);
        if (read.Cause is { } cause) return new(ToolEvidenceState.Unavailable, cause);
        if (read.Value is not { } value) return new(ToolEvidenceState.Drifted, "The generated host member is missing.");
        return JsonElement.DeepEquals(value, member.Value) ? new(ToolEvidenceState.Matching) : new(ToolEvidenceState.Drifted, "The generated host member has changed.");
    }
}
