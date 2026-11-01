import { readdir, stat } from "node:fs/promises";

const uid = process.getuid?.();
if (uid === undefined || uid === 0) throw new Error("Tool cleanup needs the tool account.");
const processes: number[] = [];
for (const name of await readdir("/proc")) {
  if (!/^\d+$/.test(name) || Number(name) === process.pid) continue;
  try {
    if ((await stat(`/proc/${name}`)).uid === uid) processes.push(Number(name));
  } catch { /* Processes can exit while the list is read. */ }
}
for (const pid of processes) {
  try { process.kill(pid, "SIGKILL"); }
  catch { /* A process can exit before its turn. */ }
}
