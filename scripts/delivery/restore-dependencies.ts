import { mkdirSync } from "node:fs";
import { join } from "node:path";
import { run } from "./process.ts";
import { Configuration, hostRuntime, Projects } from "./layout.ts";

export function restoreDependencies(root: string, offline = false): void {
  const feed = join(root, "artifacts/delivery/offline-feed");
  if (offline) mkdirSync(feed, { recursive: true });
  const flags = offline ? ["--source", feed, "-p:NuGetAudit=false"] : [];
  if (offline) process.stdout.write("Offline restore: cached dependencies only; vulnerability audit is unavailable.\n");
  run("dotnet", ["restore", "OpenForge.Cli.slnx", ...flags], root);
  // Resolve the self-contained Native AOT pack with the same inputs as publish.
  // A solution restore alone leaves that runtime pack unresolved.
  const rid = hostRuntime();
  for (const project of [Projects.cli, Projects.integration, Projects.public]) {
    run("dotnet", ["restore", project, "--runtime", rid, `-p:Configuration=${Configuration}`, "-p:SelfContained=true", ...flags], root);
  }
}
