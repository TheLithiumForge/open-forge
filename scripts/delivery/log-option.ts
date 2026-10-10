import assert from "node:assert/strict";

export const LogFlag = "--log";
const LogAssignment = `${LogFlag}=`;

export function extractLogOption(args: readonly string[]): { args: string[]; log?: string } {
  const remaining: string[] = [];
  let log: string | undefined;
  for (let index = 0; index < args.length; index++) {
    const argument = args[index];
    assert.ok(argument !== undefined);
    if (argument !== LogFlag && !argument.startsWith(LogAssignment)) {
      remaining.push(argument);
      continue;
    }
    assert.equal(log, undefined, "Supply --log once.");
    log = argument === LogFlag ? args[++index] : argument.slice(LogAssignment.length);
    assert.ok(log && !log.startsWith("--"), "--log requires a file path.");
  }
  return log === undefined ? { args: remaining } : { args: remaining, log };
}
