import assert from "node:assert/strict";
import { test } from "node:test";
import { posix, win32 } from "node:path";
import { assignmentLines, environmentAssignments, PreparedVariable, RunnerOs, DotnetFolder } from "../environment.ts";

const RunnerPaths = { unix: "/runner/temp", physical: "/private/runner/temp", windows: "D:\\runner\\temp" } as const;

for (const os of Object.values(RunnerOs)) {
  test(`runner preparation assigns SDK paths on ${os} and resolves macOS TMPDIR`, () => {
    const temporary = os === RunnerOs.windows ? RunnerPaths.windows : RunnerPaths.unix;
    const path = os === RunnerOs.windows ? win32 : posix;
    const expected = {
      [PreparedVariable.install]: path.join(temporary, DotnetFolder),
      [PreparedVariable.root]: path.join(temporary, DotnetFolder),
      ...(os === RunnerOs.mac ? { [PreparedVariable.temporary]: `${RunnerPaths.physical}/` } : {}),
    };
    assert.deepEqual(environmentAssignments({ os, temporary, physicalTemporary: `${RunnerPaths.physical}/` }), expected);
    assert.equal(
      assignmentLines(expected),
      Object.entries(expected)
        .map(([name, value]) => `${name}=${value}\n`)
        .join(""),
    );
  });
}

test("environment lines reject multiline path values", () => {
  assert.throws(() => assignmentLines({ [PreparedVariable.root]: "first\nsecond" }), /one environment line/u);
});
