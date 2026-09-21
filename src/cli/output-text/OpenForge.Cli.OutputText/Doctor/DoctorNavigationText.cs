namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorNavigationText
{
    // @OpenForgeText doctor.navigation.index-reason
    internal static string IndexReason()
        => "Refresh the generated Entries in this catalogue.";

    // @OpenForgeText doctor.navigation.index-manual
    internal static string IndexManual(string path)
        => $"Run open-forge index with {path} as its source argument, using your shell's quoting rules.";
}
