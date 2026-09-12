import assert from "node:assert/strict";
import { parseTargets } from "./targets.ts";
import type { SupportedRuntime } from "./package-model.ts";

export interface DistOptions {
  sha?: boolean;
  targets?: string;
  offline?: boolean;
  "no-restore"?: boolean;
  "skip-tests"?: boolean;
}

export function distPlan(rid: SupportedRuntime, options: DistOptions) {
  const targets = parseTargets(options.targets);
  assert.ok(targets.includes(rid), "The wrapper target selection must include this host.");
  const target = ["--rid", rid];
  const build = [...target];
  for (const flag of ["sha", "offline", "no-restore"] as const) if (options[flag]) build.push(`--${flag}`);
  return [
    { name: "Build native CLI and test executables", script: "build:native", args: build, skipped: false },
    { name: "Test managed and native suites on this host", script: "test:built", args: target, skipped: options["skip-tests"] === true },
    {
      name: options["skip-tests"] ? "Pack without tests" : "Pack and test npm installation",
      script: "pack",
      args: [...target, ...(options.targets === undefined ? [] : ["--targets", targets.join(",")]), ...(options["skip-tests"] ? ["--skip-tests"] : [])],
      skipped: false,
    },
  ];
}
