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

// Only explicit OS exclusions in the integration suite qualify. Capability failures,
// unknown reasons and skips on the platform that owns the evidence still fail.
export function isPlatformExclusion(test: Record<string, unknown>, platform: NodeJS.Platform): boolean {
  const extra = test["extra"];
  if (typeof extra !== "object" || extra === null || !("type" in extra)) return false;
  if (typeof extra.type !== "string" || !extra.type.startsWith("OpenForge.Cli.IntegrationTests.")) return false;
  const reason = test["message"];
  if (typeof reason !== "string") return false;
  if (platform === "win32") return unixOnly.has(reason) || reason === "Required permission evidence targets Linux.";
  if (platform === "linux") return windowsOnly.has(reason);
  if (platform === "darwin") return windowsOnly.has(reason) || reason === "Required permission evidence targets Linux.";
  return false;
}
