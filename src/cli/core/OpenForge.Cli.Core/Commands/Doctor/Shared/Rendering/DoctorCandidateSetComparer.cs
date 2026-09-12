using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal sealed class DoctorCandidateSetComparer : IEqualityComparer<DoctorCandidateSet>
{
    internal static DoctorCandidateSetComparer Instance { get; } = new();

    public bool Equals(DoctorCandidateSet? left, DoctorCandidateSet? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Cardinality != right.Cardinality || left.Items.Count != right.Items.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Items.Count; index++)
        {
            var first = left.Items[index];
            var second = right.Items[index];
            if (first.Subject != second.Subject || first.Provenance != second.Provenance || !first.Evidence.SequenceEqual(second.Evidence))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(DoctorCandidateSet value)
    {
        var hash = new HashCode();
        hash.Add(value.Cardinality);
        foreach (var candidate in value.Items)
        {
            hash.Add(candidate.Subject);
            hash.Add(candidate.Provenance);
            foreach (var evidence in candidate.Evidence)
            {
                hash.Add(evidence);
            }
        }

        return hash.ToHashCode();
    }
}
