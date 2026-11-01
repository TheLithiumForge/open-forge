import { readFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import {
  readReplay,
  REPLAY_MAX_ACTIONS,
  REPLAY_MAX_BYTES,
  REPLAY_VERSION,
  ReplayProblem,
} from '../src/game/replay.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';
import type { ReplayProblemKind } from '../src/game/replay.ts';

export const ReplayExit = { success: 0, failure: 1 } as const;
const MISSING_FILE = 'ENOENT';
const PROBLEMS = {
  [ReplayProblem.tooLarge]: `the file is larger than ${REPLAY_MAX_BYTES} bytes`,
  [ReplayProblem.malformedJson]: 'the file is not valid JSON',
  [ReplayProblem.format]: `it isn't a version ${REPLAY_VERSION} replay with exactly version, boardCode, actions and endMs`,
  [ReplayProblem.boardCode]: 'its board code is invalid',
  [ReplayProblem.actions]: `actions must be a list of at most ${REPLAY_MAX_ACTIONS}`,
  [ReplayProblem.endTime]: 'endMs must be a nonnegative safe integer',
  [ReplayProblem.action]: 'needs exactly atMs and index as nonnegative safe integers',
  [ReplayProblem.order]: 'comes before the previous action or after endMs',
  [ReplayProblem.index]: 'names a card outside the board',
} satisfies Record<ReplayProblemKind, string>;

function fail(file: string, reason: string): number {
  process.stderr.write(`Could not replay ${file}: ${reason}.\n`);
  return ReplayExit.failure;
}

function isMissing(error: unknown): boolean {
  return error instanceof Error && 'code' in error && error.code === MISSING_FILE;
}

// npm runs scripts from the package folder and records the caller's folder in INIT_CWD.
export async function runReplay(
  args: readonly string[],
  callerFolder = process.env.INIT_CWD ?? process.cwd(),
): Promise<number> {
  const [file] = args;
  if (!file || args.length !== 1) {
    process.stderr.write('Usage: npm run --silent replay -- PATH\n');
    return ReplayExit.failure;
  }
  let text: string;
  try {
    text = await readFile(resolve(callerFolder, file), 'utf8');
  } catch (error) {
    return fail(file, isMissing(error) ? 'the file does not exist' : "the file can't be read");
  }
  const result = readReplay(text);
  if ('state' in result) {
    process.stdout.write(`${encodeSnapshot(result.state)}\n`);
    return ReplayExit.success;
  }
  const reason = PROBLEMS[result.problem];
  return fail(file, result.action === undefined ? reason : `action ${result.action + 1} ${reason}`);
}
