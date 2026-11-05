# ADR 015: Keep replay in the developer command

This record explains why replay is a command-line diagnostic and how it shares the browser’s rules.

## Decision

Replay is available in the game core and through `npm run --silent replay -- PATH`. The playable UI has board-code and snapshot controls, but no replay controls. This keeps timed action traces available to engineers without giving the reproduction panel a second editor, timeline and execution interface.

A replay describes one deal with a board code, timed card indices and an end time. It is especially useful for an error that depends on the order of a timeout and an activation. A snapshot answers what the state was; a replay can answer how the stated actions reached a state. [Replay format](replay-format.md) documents the input.

## Command behavior

`scripts/replay.ts` passes arguments to `runReplay` and sets the process exit code. The wrapper reads UTF-8 text, calls `readReplay` and encodes a successful state with `encodeSnapshot`. Valid input prints only the final JSON to stdout and exits successfully. Invalid input leaves stdout empty, writes an explanation to stderr and exits nonzero.

The silent npm option matters because the output can be redirected or consumed by another tool. An npm script banner mixed with JSON would make otherwise valid output unusable. Error messages still go to stderr. This is a data command, so a friendly progress log on stdout would be a functional regression.

The wrapper resolves a supplied path from `INIT_CWD` when npm provides it, otherwise from the current process directory. That handles invocation from a caller's directory even though npm runs package scripts from the package folder. Missing arguments, extra arguments, unreadable files and invalid replay contents have separate test cases.

## Reuse rather than a simulator fork

`readReplay` constructs the starting game, advances logical time to each action, applies `activateCard`, and finally advances to `endMs`. It does not emulate clicks, mount React or wait for timers. The same rule functions that browser play uses decide whether an activation is ignored, matches a pair or creates a mismatch.

This makes replay fast and deterministic. It also limits what replay claims to show. It does not reproduce a browser's focus ring, scroll position or spoken output. A browser artifact is the right evidence for those observations. The replay timeline omits hidden-page and imported-pause intervals rather than attempting to record document visibility events.

## Alternatives and costs

A browser replay UI could have supplied a step button and visual debugging. It would also have added interaction design, import messages and state ownership for a feature primarily used by engineers. Replaying through Playwright alone would have been possible, but every logical investigation would then require a browser and generated input events.

A separate Node simulation was rejected as unnecessary duplication. Importing the pure core gives exact rule reuse while the CLI wrapper owns filesystem access and process output. That split is an example of the architectural boundary rather than an exception to it.

The parser bounds byte size and action count, validates exact fields and reports an action position for per-action failures. The CLI turns those problem kinds into readable messages. It does not try to repair a malformed trace. [ADR 048](adr-048-replay-order.md) explains equal-time behavior, and [ADR 010](adr-010-snapshot-json.md) explains the final output representation.

## The command’s execution path

The wrapper owns IO and diagnostics. On success it uses the public snapshot encoder, so callers receive the same state representation as the browser export.

```ts
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
```
