import { setTimeout } from "node:timers/promises";
import { parseArgs } from "node:util";
import { repositoryRoot } from "../repository.ts";
import { sourceIdentity } from "../source.ts";
import { committedVersion } from "../version.ts";
import { reportFailure } from "../process.ts";
import { releaseChannel } from "./release-selection.ts";
import { checkpointSeconds, MillisecondsPerSecond, publicationPlan, readReleaseTarget, ReleaseDirectory } from "./release-plan.ts";
import { readReleasePlan } from "./read-release-plan.ts";

try {
  const { values } = parseArgs({ options: { from: { type: "string", default: ReleaseDirectory }, target: { type: "string" }, tag: { type: "string" }, wait: { type: "string" } } });
  const version = committedVersion(repositoryRoot);
  const target = readReleaseTarget(values.target ?? process.env["RELEASE_TARGET"]);
  const channel = values.tag ?? releaseChannel(version);
  const seconds = checkpointSeconds(values.wait);
  const plan = readReleasePlan(repositoryRoot, values.from, sourceIdentity(repositoryRoot), version, target, channel);
  process.stdout.write(`${publicationPlan(plan)}\nWaiting ${seconds} seconds before publication. Cancel now to publish nothing.\n`);
  await setTimeout(seconds * MillisecondsPerSecond);
} catch (error) {
  reportFailure(error);
}
