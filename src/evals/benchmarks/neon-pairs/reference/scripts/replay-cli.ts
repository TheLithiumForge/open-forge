import { readFile } from 'node:fs/promises';
import { replayGame } from '../src/game/replay.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';

export const ReplayExit = { success: 0, failure: 1 } as const;

export async function runReplay(args: readonly string[]): Promise<number> {
  const [file] = args;
  if (!file || args.length !== 1) {
    process.stderr.write('Usage: npm run --silent replay -- PATH\n');
    return ReplayExit.failure;
  }
  try {
    const state = replayGame(await readFile(file, 'utf8'));
    if (!state) throw new Error('Invalid replay.');
    process.stdout.write(`${encodeSnapshot(state)}\n`);
    return ReplayExit.success;
  } catch {
    process.stderr.write('Could not replay the file.\n');
    return ReplayExit.failure;
  }
}
