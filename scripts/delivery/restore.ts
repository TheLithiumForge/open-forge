import { mkdirSync } from "node:fs";
import { join } from "node:path";
import { readDeliveryOptions } from "./options.ts";
import { reportFailure, run } from "./process.ts";
import { repositoryRoot } from "./repository.ts";

try {
  const values = readDeliveryOptions();
  const offlineFeed = join(repositoryRoot, "artifacts/delivery/offline-feed");
  const flags = values.offline ? ["--source", offlineFeed, "-p:NuGetAudit=false"] : [];
  if (values.offline) {
    mkdirSync(offlineFeed, { recursive: true });
    process.stdout.write("Offline restore: cached dependencies only; vulnerability audit is unavailable.\n");
  }
  run("dotnet", ["restore", "OpenForge.Cli.slnx", ...flags], repositoryRoot);
} catch (error) {
  reportFailure(error);
}
