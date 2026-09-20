using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusLifecycleFixture
{
    internal sealed record ExtensionSeed(string Id, string? Version, string? Source,
        IReadOnlyList<string> Dependencies, IReadOnlyList<string> Paths);

    internal static void Write(StatusIntegrationWorkspace workspace, FrameworkOwnership? framework,
        ImmutableArray<ExtensionOwnership>? extensions)
    {
        var ownership = WorkspaceOwnershipDocument.Empty with { Framework = framework, Extensions = extensions ?? [] };
        var path = workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath);
        var bytes = WorkspaceOwnershipCodec.Write(ownership);
        if (File.Exists(path)) File.WriteAllBytes(path, bytes);
        else workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, bytes);
    }

    internal static WorkspaceOwnershipDocument Read(StatusIntegrationWorkspace workspace)
        => WorkspaceOwnershipCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath))).Document
            ?? throw new InvalidOperationException("The Status fixture requires readable ownership.");

    internal static ImmutableArray<ExtensionOwnership> Extensions(IEnumerable<ExtensionSeed> packages)
        => [.. packages.Select(package => new ExtensionOwnership(package.Id, package.Version, package.Source,
            [.. package.Dependencies], [.. package.Paths], []))];
}
