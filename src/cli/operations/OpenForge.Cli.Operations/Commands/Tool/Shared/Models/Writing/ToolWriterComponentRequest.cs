using OpenForge.Cli.Core.Commands.Tool.Models.Interaction;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterComponentRequest(
    ToolOptionalComponent[] Declarations,
    ToolComponentChoice[] Supplied,
    IReadOnlyDictionary<string, bool>? Saved,
    ToolWriterDisclosure Disclosure,
    ToolWriteOptions Options);

internal sealed record ToolWriterComponentResolution(Dictionary<string, bool> Choices, ToolWriteIssue[] Issues);
