import { spawn } from "node:child_process";
import { closeSync, mkdirSync, openSync, writeSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { ProcessExit } from "./process.ts";

export async function runLogged(executable: string, args: readonly string[], root: string, log: string): Promise<void> {
  const path = resolve(root, log);
  mkdirSync(dirname(path), { recursive: true });
  const descriptor = openSync(path, "w");
  try {
    const command = `${executable} ${args.join(" ")}\n`;
    process.stdout.write(command);
    writeSync(descriptor, command);
    const child = spawn(executable, args, { cwd: root, shell: false, stdio: ["inherit", "pipe", "pipe"] });
    child.stdout.on("data", (chunk: Buffer) => {
      process.stdout.write(chunk);
      writeSync(descriptor, chunk);
    });
    child.stderr.on("data", (chunk: Buffer) => {
      process.stderr.write(chunk);
      writeSync(descriptor, chunk);
    });
    const status = await new Promise<number>((resolve, reject) => {
      child.once("error", reject);
      child.once("close", (code) => resolve(code ?? ProcessExit.failure));
    });
    process.exitCode = status;
  } catch (error) {
    writeSync(descriptor, `${error instanceof Error ? error.message : String(error)}\n`);
    throw error;
  } finally {
    closeSync(descriptor);
  }
}
