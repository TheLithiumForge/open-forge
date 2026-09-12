using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal sealed class FrameworkLifecycleTargetStateReader(
    LifecycleManagedTargetReader managedTargetReader)
{
    private readonly FrameworkLifecycleTargetIdentity _targetIdentity = new();

    internal async ValueTask<FrameworkManagedTargetDoctorRead> ReadAsync(
        CliWorkspace workspace,
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated,
        CancellationToken cancellationToken)
    {
        var read = await managedTargetReader
            .ReadAsync(workspace, target.Path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != LifecycleManagedTargetReadState.Available)
        {
            return FrameworkManagedTargetDoctorRead.Boundary(read.State, cancellationToken);
        }

        try
        {
            var fingerprint = _targetIdentity.ReadFingerprint(target, generated, read.Bytes.Span);
            var state = string.Equals(
                fingerprint,
                target.BaselineFingerprint,
                StringComparison.Ordinal)
                ? OperationalTargetState.Current
                : OperationalTargetState.Changed;
            return FrameworkManagedTargetDoctorRead.Observed(state, fingerprint);
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return FrameworkManagedTargetDoctorRead.Blocked(exception.Message);
        }
    }
}
