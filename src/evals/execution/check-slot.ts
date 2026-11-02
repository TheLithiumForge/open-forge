import { mkdir, readFile, rm, stat, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { setTimeout } from "node:timers/promises";

/**
 * Browser checks are timed, and a crowded machine makes them run past their limit. Every eval process on this
 * machine shares these slots, so agents still run in parallel while check stages wait their turn.
 */
export const CheckSlots = { environment: "EVALS_CHECK_SLOTS", rootEnvironment: "EVALS_CHECK_SLOT_ROOT", count: 2, pollMs: 5_000, claimGraceMs: 60_000, maxAgeMs: 6 * 60 * 60_000 } as const;
// The test runner points this at a private folder, so tests never wait on a real evaluation's slots.
export const CheckSlotRoot = process.env[CheckSlots.rootEnvironment] ?? join(tmpdir(), "open-forge-evals-check-slots");
const OwnerFile = "owner.json";

interface Owner { readonly pid: number; readonly at: string }
export interface CheckSlotOptions { readonly root?: string; readonly count?: number; readonly pollMs?: number }

export function checkSlotCount(value = process.env[CheckSlots.environment]): number {
  const count = Number(value ?? CheckSlots.count);
  return Number.isInteger(count) && count >= 1 ? count : CheckSlots.count;
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
    if (await stale(slot).catch(() => false)) await rm(slot, { recursive: true, force: true });
    return false;
  }
  await writeFile(join(slot, OwnerFile), JSON.stringify({ pid: process.pid, at: new Date().toISOString() } satisfies Owner));
  return true;
}

/** Runs one check stage in a machine-wide slot, waiting while every slot is taken. */
export async function withCheckSlot<T>(work: () => Promise<T>, options: CheckSlotOptions = {}): Promise<T> {
  const root = options.root ?? CheckSlotRoot, count = options.count ?? checkSlotCount();
  await mkdir(root, { recursive: true });
  for (;;) {
    for (let index = 0; index < count; index += 1) {
      const slot = join(root, `slot-${index}`);
      if (!await claim(slot)) continue;
      try { return await work(); }
      finally { await rm(slot, { recursive: true, force: true }); }
    }
    await setTimeout(options.pollMs ?? CheckSlots.pollMs);
  }
}
