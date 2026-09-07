using System.Runtime.CompilerServices;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class ReferenceIdentityComparer<T> : IEqualityComparer<T>
    where T : class
{
    internal static ReferenceIdentityComparer<T> Instance { get; } = new();

    public bool Equals(T? left, T? right)
        => ReferenceEquals(left, right);

    public int GetHashCode(T value)
        => RuntimeHelpers.GetHashCode(value);
}
