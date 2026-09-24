using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Mutation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Planning;

internal sealed record RemovePathPlan(
    RemovePathRequest Request,
    string Target,
    bool TargetIsDirectory,
    bool IsMissing,
    WorkspaceSettingsRead Settings,
    WorkspaceOwnershipRead Ownership,
    RemovePathMetadataChange? SettingsChange,
    RemovePathMetadataChange? OwnershipChange,
    PlannedDirectoryCreation? AgentsCreation,
    ImmutableArray<FileStateSnapshot> Files,
    ImmutableArray<RemovePathLibraryLink> Links,
    ImmutableArray<FileStateSnapshot> Directories,
    RemoveNavigationPlan Navigation)
{
    internal bool IsNoOp
        => IsMissing
            && Files.IsEmpty
            && Links.IsEmpty
            && Directories.IsEmpty
            && SettingsChange is null
            && OwnershipChange is null
            && Navigation.Changes.IsEmpty;

    internal int FileCount => Files.Length + Links.Length;

    internal int DirectoryCount => Directories.Length;

    internal bool Matches(RemovePathPlan actual)
    {
        ArgumentNullException.ThrowIfNull(actual);
        return Target == actual.Target
            && TargetIsDirectory == actual.TargetIsDirectory
            && IsMissing == actual.IsMissing
            && Settings.MatchesObservation(actual.Settings)
            && Ownership.State == actual.Ownership.State
            && MatchesSnapshot(Ownership.Snapshot, actual.Ownership.Snapshot)
            && Files.Select(file => file.Expectation).SequenceEqual(actual.Files.Select(file => file.Expectation))
            && Files.Zip(actual.Files).All(pair => pair.First.Bytes.AsSpan().SequenceEqual(pair.Second.Bytes.AsSpan()))
            && Links.Select(link => (link.Path, link.Effect, link.Before, link.Release))
                .SequenceEqual(actual.Links.Select(link => (link.Path, link.Effect, link.Before, link.Release)))
            && Navigation.Matches(actual.Navigation)
            && Directories.Select(directory => directory.Expectation).SequenceEqual(actual.Directories.Select(directory => directory.Expectation))
            && MatchesChange(SettingsChange, actual.SettingsChange)
            && MatchesChange(OwnershipChange, actual.OwnershipChange)
            && (AgentsCreation?.Expectation == actual.AgentsCreation?.Expectation);
    }

    private static bool MatchesSnapshot(FileStateSnapshot? left, FileStateSnapshot? right)
        => left is null ? right is null
            : right is not null
                && left.Expectation == right.Expectation
                && left.Bytes.AsSpan().SequenceEqual(right.Bytes.AsSpan());

    private static bool MatchesChange(RemovePathMetadataChange? left, RemovePathMetadataChange? right)
        => left is null ? right is null
            : right is not null
                && left.Change.Kind == right.Change.Kind
                && left.Change.Expectation == right.Change.Expectation
                && left.Change.IntendedBytes.AsSpan().SequenceEqual(right.Change.IntendedBytes.AsSpan())
                && MatchesSnapshot(left.Before, right.Before);
}

internal sealed record RemovePathMetadataChange(
    PlannedFileChange Change,
    FileStateSnapshot Before);

internal sealed record RemovePathLibraryLink(
    string Path,
    RelativeFileLinkEffect Effect,
    NoFollowLeafObservation Before,
    LibraryPathRelease Release);
