using System.Net;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm.Models;

internal sealed record ToolNpmMetadataConnection(Uri Registry, IWebProxy? Proxy);
