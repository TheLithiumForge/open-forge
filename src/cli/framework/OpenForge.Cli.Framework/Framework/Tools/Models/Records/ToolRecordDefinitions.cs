namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal static class ToolRecordDefinitions
{
    internal const int ReleasedSettingsSchemaVersion = 5;
    internal const int ReleasedLockSchemaVersion = 1;
    internal const int SettingsSchemaVersion = 6;
    internal const int LockSchemaVersion = 2;
    internal const int LocalSchemaVersion = 1;
    internal const string ToolsProperty = "tools";
    internal const string VersionProperty = "version";
    internal const string ProfileDigestProperty = "profileDigest";
    internal const string ChannelProperty = "channel";
    internal const string ChangesProperty = "changes";
    internal const string KindProperty = "kind";
    internal const string IdProperty = "id";
    internal const string PathProperty = "path";
    internal const string PointerProperty = "pointer";
    internal const string ItemsProperty = "items";
    internal const string ValueProperty = "value";
    internal const string StartMarkerProperty = "startMarker";
    internal const string EndMarkerProperty = "endMarker";
    internal const string ContentSha256Property = "contentSha256";
    internal const string Sha256Property = "sha256";
    internal const string AllowListIdProperty = "allowListId";
    internal const string JsonUnionKind = "json-union";
    internal const string JsonValueKind = "json-value";
    internal const string MarkedBlockKind = "marked-block";
    internal const string ManagedFileKind = "managed-file";
    internal const string NativeCommandKind = "native-command";
    internal const string WorkspacesDirectoryName = "workspaces";
    internal const string ToolsDirectoryName = "tools";
    internal const string HomeDirectoryName = ".open-forge";
    internal const string BinDirectoryName = "bin";
    internal const string RuntimesDirectoryName = "runtimes";
    internal const string NodeDirectoryName = "node";
    internal const string MachineStateDirectoryName = "state";
}
