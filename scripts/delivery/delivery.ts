import assert from "node:assert/strict";
import { mkdirSync } from "node:fs";
import { join } from "node:path";
import { parseArgs } from "node:util";
import { fileURLToPath } from "node:url";
import { managedBuild, nativeBuild } from "./build.ts";
import { npm, run } from "./commands.ts";
import { hostRuntime } from "./layout.ts";
import { pack } from "./pack.ts";
import { sourceIdentity } from "./source.ts";
import { testBuilt, testManaged } from "./test-built.ts";
import { candidateVersion } from "./version.ts";
import { committedVersion } from "./version-sync.ts";

const root = fileURLToPath(new URL("../../", import.meta.url));
const [command, ...args] = process.argv.slice(2);

function main(): void {
  if (command === "version:bump") {
    assert.ok(args.length > 0, "Supply patch, minor, major, prerelease or an explicit version.");
    npm(["version", ...args, "--no-git-tag-version"], root);
    return;
  }
  const { values } = parseArgs({ args, options: { rid: { type: "string" }, sha: { type: "boolean" }, offline: { type: "boolean" } } });
  if (command === "restore") {
    const flags = values.offline ? ["--source", join(root, "artifacts/delivery/offline-feed"), "-p:NuGetAudit=false"] : [];
    if (values.offline) {
      mkdirSync(join(root, "artifacts/delivery/offline-feed"), { recursive: true });
      process.stdout.write("Offline restore: cached dependencies only; vulnerability audit is unavailable.\n");
    }
    run("dotnet", ["restore", "OpenForge.Cli.slnx", ...flags], root);
    return;
  }
  assert.ok(!values.offline, "--offline applies to restore only; builds never restore.");
  const version = committedVersion(root);
  switch (command) {
    case "build":
      managedBuild(root, version);
      return;
    case "test":
      managedBuild(root, version);
      testManaged(root);
      return;
    case "build:native":
      nativeBuild(root, hostRuntime(values.rid), candidateVersion(version, values.sha ? sourceIdentity(root).sha : undefined));
      return;
    case "test:built":
      assert.ok(!values.sha);
      testBuilt(root, hostRuntime(values.rid));
      return;
    case "pack":
      assert.ok(!values.sha);
      pack(root, hostRuntime(values.rid));
      return;
    default:
      throw new Error(`Unknown delivery command: ${command}`);
  }
}

try {
  main();
} catch (error) {
  process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`);
  process.exitCode ||= 1;
}
