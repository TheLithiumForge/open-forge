using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;

internal sealed record ToolConfigEdit(byte[]? Bytes, ToolAppliedChange? Addition, ToolChangeDisposition Disposition);
