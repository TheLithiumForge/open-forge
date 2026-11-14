using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolSupplyPaths
{
    internal static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    internal static string Tool(string home, string id, string version) => Under(home, ToolRecordDefinitions.ToolsDirectoryName, id, version);
    internal static string Runtime(string home, string version) => Under(home, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName, version);
    internal static string Bin(string home) => Under(home, ToolRecordDefinitions.BinDirectoryName);

    internal static string Under(string home, params string[] segments)
    {
        if (!System.IO.Path.IsPathFullyQualified(home)) throw new IOException("The Open Forge home must be an absolute physical path.");
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine([home, .. segments]));
        if (!PhysicalContainment.Contains(home, path)) throw new IOException("A tool destination leaves its home.");
        CheckOrdinary(path);
        return path;
    }

    internal static void CheckOrdinary(string path)
    {
        for (FileSystemInfo? entry = new FileInfo(path); entry is not null; entry = Directory.GetParent(entry.FullName))
            if (entry.LinkTarget is not null || entry.Exists && entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new IOException("A tool destination passes through a link.");
    }

    internal static string? PreflightFile(string path)
    {
        CheckOrdinary(path);
        if (Directory.Exists(path)) return "A planned tool file is a directory.";
        if (File.Exists(path + ".bak")) return "A planned tool file already has a sibling backup.";
        return null;
    }
}
