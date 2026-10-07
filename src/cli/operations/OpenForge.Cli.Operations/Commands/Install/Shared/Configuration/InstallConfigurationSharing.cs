using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallConfigurationSharing
{
    internal static InstallConfigurationPlan Complete(InstallConfigurationPlan plan,
        InstallConfiguration configuration, IReadOnlyCollection<SourceLogicalSource> sources)
    {
        var routes = configuration.Routes.Where(row => row.Action == InstallRouteAction.GitIgnore)
            .Select(row =>
            {
                var directory = InstallConfigurationChoices.Directory(row.Id);
                var entrypoint = sources.Single(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                    && SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath) == directory);
                return new SourceSharingRoute(directory, entrypoint.Identity.CanonicalBasePath);
            }).ToImmutableArray();
        var snapshot = plan.Ignore.Snapshot ?? throw new InvalidDataException("The Git-ignore file is unavailable.");
        var bytes = InstallIgnoreSection.Rewrite(snapshot.Bytes.AsSpan(), configuration.Routes, routes);
        PlannedFileChange? change = null;
        if (!snapshot.Bytes.AsSpan().SequenceEqual(bytes))
            change = plan.Ignore.State == InstallTargetReadState.Missing
                ? PlannedFileChange.Create(snapshot.Expectation, bytes)
                : PlannedFileChange.ReplaceGeneratedRegion(snapshot.Expectation, bytes);
        return plan with { IgnoreChange = change, GitIgnoredRoutes = routes };
    }
}
