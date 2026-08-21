using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListFilesystemFinding
{
    private const int MaximumCauseLength = 256;

    internal RouteListFilesystemFinding(
        RouteListFindingCode code,
        CliSemanticStatus status,
        string canonicalLogicalSubject,
        string cause)
    {
        _ = RouteListDefinitions.ReadFindingCode(code);
        _ = CliStatusDefinitions.Read(status);
        if (!IsAllowed(code, status))
        {
            throw new ArgumentException("The status does not match the filesystem finding code.", nameof(status));
        }

        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalSubject))
        {
            throw new ArgumentException("The filesystem finding subject is not canonical.", nameof(canonicalLogicalSubject));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (cause.Any(char.IsControl))
        {
            throw new ArgumentException("A filesystem finding cause cannot contain control characters.", nameof(cause));
        }

        Code = code;
        Status = status;
        CanonicalLogicalSubject = canonicalLogicalSubject;
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal RouteListFindingCode Code { get; }

    internal string MachineCode => RouteListDefinitions.ReadFindingCode(Code);

    internal CliSemanticStatus Status { get; }

    internal string CanonicalLogicalSubject { get; }

    internal string Cause { get; }

    private static bool IsAllowed(RouteListFindingCode code, CliSemanticStatus status)
    {
        return code switch
        {
            RouteListFindingCode.PhysicalBoundary => status == CliSemanticStatus.Blocked,
            RouteListFindingCode.ReadUnavailable
                or RouteListFindingCode.MetadataMissing
                or RouteListFindingCode.MetadataMalformed => status == CliSemanticStatus.Incomplete,
            RouteListFindingCode.AuthoredForm
                or RouteListFindingCode.IdentityCollision => status == CliSemanticStatus.Attention,
            RouteListFindingCode.Interrupted => status == CliSemanticStatus.Interrupted,
            _ => false,
        };
    }
}
