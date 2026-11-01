import { readFile } from 'node:fs/promises';
import { replayGame } from '../src/game/replay.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';

const file = process.argv[2];
if (!file || process.argv.length !== 3) {
  process.stderr.write('Usage: npm run replay -- PATH\n');
  process.exitCode = 1;
} else {
  try {
    const state = replayGame(await readFile(file, 'utf8'));
    if (!state) throw new Error('Invalid replay.');
    process.stdout.write(`${encodeSnapshot(state)}\n`);
  } catch {
    process.stderr.write('Could not replay the file.\n');
    process.exitCode = 1;
  }
}
