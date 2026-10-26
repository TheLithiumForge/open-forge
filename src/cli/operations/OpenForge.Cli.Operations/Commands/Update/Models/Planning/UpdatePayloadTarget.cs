using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdatePayloadTarget(FrameworkPayloadAsset Asset, string Path);
