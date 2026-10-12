import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { runWatched } from "../../watched-process.ts";

// Runs the stalled suite fixture under runWatched and prints the result, so a test can signal this process.
const [root] = process.argv.slice(2);
const result = await runWatched({
  executable: process.execPath,
  args: [fileURLToPath(new URL("stalled-suite.ts", import.meta.url)), join(root ?? "", "idle.pid")],
  cwd: root ?? "",
  log: join(root ?? "", "execution.log"),
  deadlineMilliseconds: 120_000,
  heartbeatMilliseconds: 60_000,
  onOutput: () => undefined,
  onHeartbeat: () => undefined,
  beforeStop: () => undefined,
});
process.stdout.write(`${JSON.stringify(result)}\n`);
