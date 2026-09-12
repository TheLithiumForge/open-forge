using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionSourceObservationReader(ExtensionSourceReader sourceReader)
{
    internal async ValueTask<IReadOnlyList<ExtensionSourceObservation>> ReadAsync(
        CliWorkspace workspace,
        IReadOnlyList<LifecycleInstalledPackage> packages,
        bool includeEmbedded,
        CancellationToken cancellationToken)
    {
        var includesEmbedded = includeEmbedded;
        var recordedSources = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var package in packages)
        {
            if (package.Source is null)
            {
                includesEmbedded = true;
            }
            else
            {
                recordedSources.Add(package.Source);
            }
        }

        var observations = new List<ExtensionSourceObservation>();
        ExtensionSourceReadResult? embedded = null;
        if (includesEmbedded || recordedSources.Contains(EmbeddedExtensionCatalogueReader.Identity))
        {
            embedded = await sourceReader
                .ReadAsync(workspace, explicitSource: null, cancellationToken)
                .ConfigureAwait(false);
            if (includesEmbedded)
            {
                observations.Add(new ExtensionSourceObservation(RecordedSource: null, embedded));
            }
        }

        foreach (var recordedSource in recordedSources)
        {
            var source = string.Equals(recordedSource, EmbeddedExtensionCatalogueReader.Identity, StringComparison.Ordinal)
                ? embedded ?? throw new InvalidOperationException("The recorded embedded source requires its catalogue observation.")
                : await sourceReader
                    .ReadAsync(workspace, recordedSource, cancellationToken)
                    .ConfigureAwait(false);
            observations.Add(new ExtensionSourceObservation(recordedSource, source));
        }

        return observations.ToArray();
    }

    internal static InstalledExtensionObservation Project(
        LifecycleInstalledPackage package,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var source = sources.Single(observation => string.Equals(
            observation.RecordedSource,
            package.Source,
            StringComparison.Ordinal));
        return new InstalledExtensionObservation
        {
            Id = package.Id,
            Version = package.Version,
            Source = package.Source,
            SourceAvailability = ReadAvailability(source.Read),
            Dependencies = package.Dependencies.ToArray(),
            Paths = package.Paths.ToArray(),
        };
    }

    internal static OperationalSourceAvailability ReadAvailability(
        ExtensionSourceReadResult source)
        => source.State switch
        {
            ExtensionSourceReadState.Complete => OperationalSourceAvailability.Available,
            ExtensionSourceReadState.Missing
                or ExtensionSourceReadState.Invalid
                or ExtensionSourceReadState.Blocked
                or ExtensionSourceReadState.Unavailable
                or ExtensionSourceReadState.Cancelled => OperationalSourceAvailability.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source.State,
                "The Extension source read state is not defined."),
        };
}
