import assert from "node:assert/strict";
import { test } from "node:test";
import { GradeWorkflow, githubRepository, triggerGrading, type RunCommand } from "./results-grade.ts";

const Owner = "fixture-owner", Name = "fixture-results", Commit = "0123456789abcdef";

function fakeRun(pushed: boolean, calls: string[][]): RunCommand {
  return async (command, args) => {
    calls.push([command, ...args]);
    if (command === "git" && args[0] === "remote") return { code: 0, stdout: `https://github.com/${Owner}/${Name}.git\n`, stderr: "" };
    if (command === "git" && args[0] === "rev-parse") return { code: 0, stdout: `${Commit}\n`, stderr: "" };
    if (command === "git" && args[0] === "branch") return { code: 0, stdout: pushed ? "  origin/main\n" : "", stderr: "" };
    return { code: 0, stdout: "", stderr: "" };
  };
}

test("grading starts the results repository's workflow with the graders of a pushed commit", async () => {
  const calls: string[][] = [];
  assert.deepEqual(await triggerGrading("root", "results", null, fakeRun(true, calls)), { repository: `${Owner}/${Name}`, commit: Commit });
  const workflow = calls.find((call) => call[0] === "gh");
  assert.ok(workflow?.includes(GradeWorkflow.file));
  assert.ok(workflow?.includes(`${Owner}/${Name}`));
  assert.ok(workflow?.includes(`${GradeWorkflow.refInput}=${Commit}`));
});

test("grading refuses a commit that isn't pushed, since the workflow checks its graders out from GitHub", async () => {
  const calls: string[][] = [];
  await assert.rejects(triggerGrading("root", "results", null, fakeRun(false, calls)));
  assert.equal(calls.some((call) => call[0] === "gh"), false);
});

test("the results repository is read from an HTTPS or SSH GitHub remote", () => {
  for (const remote of [`https://github.com/${Owner}/${Name}.git`, `git@github.com:${Owner}/${Name}.git`, `https://github.com/${Owner}/${Name}`]) {
    assert.equal(githubRepository(remote), `${Owner}/${Name}`);
  }
  assert.throws(() => githubRepository(`https://example.com/${Owner}/${Name}.git`));
});
