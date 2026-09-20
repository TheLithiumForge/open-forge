const windowsOnly = new Set([
  "This catalogue enumeration boundary uses an owned Windows directory ACL.",
  "This directory creation boundary uses an owned Windows catalogue ACL.",
  "This deterministic replacement failure requires Windows file sharing.",
  "This deterministic deletion failure requires Windows file sharing.",
  "This replacement failure requires Windows file sharing enforcement.",
  "This source enumeration boundary uses an owned Windows directory ACL.",
  "This deterministic record replacement failure requires Windows file sharing.",
  "This replacement denial requires Windows file sharing.",
  "This evidence requires Windows file sharing.",
]);
const unixOnly = new Set([
  "This copy failure uses Unix directory permissions.",
  "This ordinary link failure uses Unix directory permissions.",
  "This source-enumeration boundary requires Unix directory permissions.",
  "This evidence requires Unix permissions and Unix-domain sockets.",
  "This evidence requires Unix file permissions.",
  "This evidence requires Unix directory permissions.",
  "The unavailable-read case requires Unix permissions.",
  "This read boundary uses Unix file permissions.",
]);

const publicWindowsOnly = new Set([
  "This public-create boundary requires an owned Windows directory ACL.",
  "C07-06 requires the Windows directory-enumeration ACL capability.",
  "F14 terminal and sharing cases require the accepted Windows transport/filesystem capability.",
  "F20 requires the Windows PowerShell and Windows file-sharing capabilities.",
  "F13 terminal cancellation requires the accepted Windows ConPTY transport.",
  "F19 requires the Windows file-sharing capability used by the deterministic replacement failure.",
  "F16 source read denial requires the Windows share boundary.",
  "F21 incomplete find requires the Windows FileShare.None read-denial capability.",
  "C07-07 requires Windows FileShare.Read deletion-denial semantics.",
  "F03 X05 read-denial evidence requires Windows file-sharing semantics.",
  "F07 C09-09/X28 requires Windows FileShare.None read-denial semantics; this is not a product verdict on another OS.",
  "C16-11 requires Windows FileShare.None read denial.",
  "F15 source read denial requires the Windows share boundary.",
]);

// Only explicit OS exclusions in the integration and public suites qualify. Capability failures,
// unknown reasons and skips on the platform that owns the evidence still fail.
export function isPlatformExclusion(test: Record<string, unknown>, platform: NodeJS.Platform): boolean {
  const extra = test["extra"];
  if (typeof extra !== "object" || extra === null || !("type" in extra)) return false;
  if (typeof extra.type !== "string") return false;
  const reason = test["message"];
  if (typeof reason !== "string") return false;
  if (extra.type.startsWith("OpenForge.Cli.EndToEndTests.")) {
    return (platform === "linux" || platform === "darwin") && publicWindowsOnly.has(reason);
  }
  if (!extra.type.startsWith("OpenForge.Cli.IntegrationTests.")) return false;
  if (platform === "win32") return unixOnly.has(reason) || reason === "Required permission evidence targets Linux.";
  if (platform === "linux") return windowsOnly.has(reason);
  if (platform === "darwin") return windowsOnly.has(reason) || reason === "Required permission evidence targets Linux.";
  return false;
}
