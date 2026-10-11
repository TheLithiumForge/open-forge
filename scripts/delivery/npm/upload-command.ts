import { spawn } from "node:child_process";
import { npmInvocation, ProcessExit } from "../process.ts";

// Yield while npm runs so the release boundary can observe cancellation signals.
export async function uploadCommand(root: string, args: readonly string[]): Promise<void> {
  const invocation = npmInvocation(args);
  process.stdout.write(`npm ${args.join(" ")}\n`);
  const child = spawn(invocation.executable, invocation.args, { cwd: root, shell: false, stdio: "inherit" });
  await new Promise<void>((resolve, reject) => {
    child.once("error", reject);
    child.once("close", (code, signal) => {
      if (code === ProcessExit.success) resolve();
      else reject(new Error(`npm ${args[0]} failed (${code ?? signal}).`));
    });
  });
}
