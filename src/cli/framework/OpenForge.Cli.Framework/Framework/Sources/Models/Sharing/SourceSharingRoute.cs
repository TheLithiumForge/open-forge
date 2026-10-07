namespace OpenForge.Cli.Core.Framework.Sources.Models.Sharing;

/// <summary>A shared entrypoint inside a private directory. This grants no deletion authority.</summary>
internal sealed record SourceSharingRoute(string Directory, string Entrypoint);
