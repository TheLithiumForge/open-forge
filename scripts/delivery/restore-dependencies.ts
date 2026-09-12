import { mkdirSync } from "node:fs";
import { join } from "node:path";
import { run } from "./process.ts";

export function restoreDependencies(root: string, offline = false): void {
  const feed = join(root, "artifacts/delivery/offline-feed");
  if (offline) mkdirSync(feed, { recursive: true });
  const flags = offline ? ["--source", feed, "-p:NuGetAudit=false"] : [];
  if (offline) process.stdout.write("Offline restore: cached dependencies only; vulnerability audit is unavailable.\n");
  run("dotnet", ["restore", "OpenForge.Cli.slnx", ...flags], root);
}
