import { mkdir, readFile, rm, stat, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { setTimeout } from "node:timers/promises";

/**
 * Browser checks are timed, and a crowded machine makes them run past their limit. Every eval process on this
 * machine shares these slots, so agents still run in parallel while check stages wait their turn.
 */
export const CheckSlots = { count: 1, pollMs: 5_000, claimGraceMs: 60_000, maxAgeMs: 6 * 60 * 60_000 } as const;
export const CheckSlotRoot = join(tmpdir(), "open-forge-evals-check-slots");
const OwnerFile = "owner.json";
// Windows refuses to remove a folder while another process reads its owner file, so removal retries briefly.
const RemoveOptions = { recursive: true, force: true, maxRetries: 10, retryDelay: 200 } as const;

interface Owner { readonly pid: number; readonly at: string }
export interface CheckSlotOptions { readonly root: string; readonly count: number; readonly pollMs?: number }
// Commands that run checks turn the slots on. Library callers and tests run their checks directly.
let configured: CheckSlotOptions | null = null;

export function useCheckSlots(count: number = CheckSlots.count, root = CheckSlotRoot): void {
  configured = { root, count };
}

function alive(pid: number): boolean {
  try { process.kill(pid, 0); return true; }
  catch (error) { return error instanceof Error && "code" in error && error.code === "EPERM"; }
}

async function stale(slot: string): Promise<boolean> {
  const owner = await readFile(join(slot, OwnerFile), "utf8").then((text): Owner => JSON.parse(text), () => null);
  const age = Date.now() - (owner === null ? (await stat(slot)).mtimeMs : Date.parse(owner.at));
  // A slot just made may not have its owner yet, and a slot can outlive its owner after a crash.
  if (owner === null) return age > CheckSlots.claimGraceMs;
  return !alive(owner.pid) || age > CheckSlots.maxAgeMs;
}

async function claim(slot: string): Promise<boolean> {
  try { await mkdir(slot); }
  catch (error) {
    if (!(error instanceof Error && "code" in error && error.code === "EEXIST")) throw error;
    if (await stale(slot).catch(() => false)) await rm(slot, RemoveOptions).catch(() => undefined);
    return false;
  }
  await writeFile(join(slot, OwnerFile), JSON.stringify({ pid: process.pid, at: new Date().toISOString() } satisfies Owner));
  return true;
}

/** Runs one check stage in a machine-wide slot, waiting while every slot is taken. */
export async function withCheckSlot<T>(work: () => Promise<T>, options: CheckSlotOptions | null = configured): Promise<T> {
  if (options === null) return work();
  const { root, count } = options;
  await mkdir(root, { recursive: true });
  for (;;) {
    for (let index = 0; index < count; index += 1) {
      const slot = join(root, `slot-${index}`);
      if (!await claim(slot)) continue;
      try { return await work(); }
      // A slot that can't be removed yet must not turn a finished check into a failure.
      finally { await rm(slot, RemoveOptions).catch(() => undefined); }
    }
    await setTimeout(options.pollMs ?? CheckSlots.pollMs);
  }
}
