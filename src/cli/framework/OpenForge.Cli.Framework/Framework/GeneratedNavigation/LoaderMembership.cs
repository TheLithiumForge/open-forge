namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

/// <summary>
/// The Loader's Entries are the user's list of what is switched on, so maintenance keeps that list and only
/// adds the roots an operation turns on: roots whose catalogue the operation creates, and roots the user
/// explicitly selects after leaving them out.
/// </summary>
internal static class LoaderMembership
{
    internal static IReadOnlySet<string> ReadTurnedOn(
        IEnumerable<string> structuralRoots,
        Func<string, bool> existedBefore,
        IEnumerable<string>? selectedRoots = null)
    {
        ArgumentNullException.ThrowIfNull(structuralRoots);
        ArgumentNullException.ThrowIfNull(existedBefore);
        var turnedOn = new HashSet<string>(selectedRoots ?? [], StringComparer.Ordinal);
        foreach (var root in structuralRoots)
        {
            if (!existedBefore(root))
            {
                turnedOn.Add(root);
            }
        }

        return turnedOn;
    }
}
