import { spawnSync } from "node:child_process";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

interface Step {
  name: string;
  executable: string;
  args: readonly string[];
}

const PlanFlag = "--plan";
const Failure = 1;

/** Installs the locked APM packages, then applies Open Forge model policy to the generated Codex agents. */
export function installSteps(node: string = process.execPath): Step[] {
  return [
    { name: "Install locked APM packages", executable: "apm", args: ["install", "--frozen", "--target", "codex"] },
    { name: "Apply Codex agent model policy", executable: node, args: [fileURLToPath(new URL("patch-codex-agent-models.ts", import.meta.url))] },
  ];
}

export function describeStep(step: Step): string {
  return `${step.name}: ${step.executable} ${step.args.join(" ")}`;
}

/** Runs each step in order and stops at the first failure with its exit status. */
function run(steps: readonly Step[]): number {
  for (const step of steps) {
    process.stdout.write(`${describeStep(step)}\n`);
    const result = spawnSync(step.executable, step.args, { stdio: "inherit", shell: false });
    if (result.error) {
      process.stderr.write(`${step.name} could not start: ${result.error.message}\n`);
      return Failure;
    }
    if (result.status !== 0) return result.status ?? Failure;
  }
  return 0;
}

if (process.argv[1] !== undefined && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const steps = installSteps();
  if (process.argv.includes(PlanFlag)) for (const step of steps) process.stdout.write(`${describeStep(step)}\n`);
  else process.exitCode = run(steps);
}
