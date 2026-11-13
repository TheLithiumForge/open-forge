using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;

internal sealed record ToolRunPinEvidence(WorkspaceToolSetting Setting, ToolReceipt Receipt, ToolStoreRegistryDocument Registry);
