using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

internal sealed record CliRetryOptions(CliFormat Format, CliDetail? Detail, IReadOnlySet<CliSeverity>? Filter)
{
    internal static CliRetryOptions From(CliInvocation invocation)
        => new(invocation.Presentation.Format, invocation.SuppliedDetail, invocation.Presentation.Filter);
}
