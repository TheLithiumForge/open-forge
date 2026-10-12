import { spawn } from "node:child_process";
import { writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

// Reports a hung test, then either exits with the requested status or stalls with a child process.
const [pidFile, status] = process.argv.slice(2);
process.stdout.write("passed First test\n[Long Running Test] 'Stalled test', Elapsed: 00:02:00\n");
if (status !== undefined) process.exit(Number(status));
const idle = spawn(process.execPath, [fileURLToPath(new URL("idle-process.ts", import.meta.url))], { stdio: "ignore" });
writeFileSync(pidFile ?? "", String(idle.pid));
setInterval(() => undefined, 1_000);
