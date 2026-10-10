import { spawnSync } from "node:child_process";

export function capture(executable: string, args: readonly string[], root: string, environment: NodeJS.ProcessEnv = process.env) {
  process.stdout.write(`${executable} ${args.join(" ")}\n`);
  const output = spawnSync(executable, args, { cwd: root, env: environment, shell: false, encoding: "utf8" });
  if (output.error) throw output.error;
  process.stdout.write(output.stdout);
  process.stderr.write(output.stderr);
  return { status: output.status, stdout: output.stdout, stderr: output.stderr };
}
