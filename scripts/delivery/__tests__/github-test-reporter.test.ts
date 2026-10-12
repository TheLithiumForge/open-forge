import assert from "node:assert/strict";
import { join } from "node:path";
import { test } from "node:test";
import githubTestReporter, { failureMessage, suiteName } from "../github-test-reporter.ts";

async function* events(...items: readonly { type: string; data: Record<string, unknown> }[]) {
  for (const item of items) yield item;
}

async function collect(source: AsyncGenerator<string>): Promise<string[]> {
  const lines: string[] = [];
  for await (const line of source) lines.push(line);
  return lines;
}

const Failure = {
  type: "test:fail",
  data: {
    name: "runner stops",
    file: join(process.cwd(), "scripts", "delivery", "x.test.ts"),
    line: 7,
    details: { error: new Error("wrapper", { cause: new Error("expected 1\nactual 2") }) },
  },
};
const Pass = { type: "test:pass", data: { name: "fine" } };

test("failure messages unwrap node:test's wrapper", () => {
  assert.equal(failureMessage(new Error("wrapper", { cause: new Error("expected 1\nactual 2") })), "expected 1\nactual 2");
  assert.equal(failureMessage(new Error("plain")), "plain");
  assert.equal(failureMessage(undefined), "");
  assert.equal(suiteName("test:delivery"), "delivery");
  assert.equal(suiteName("verify"), "verify");
  assert.equal(suiteName(undefined), "node");
});

test("failed tests become annotations only in GitHub Actions", async (context) => {
  const previous = { actions: process.env["GITHUB_ACTIONS"], script: process.env["npm_lifecycle_event"] };
  context.after(() => {
    for (const [name, value] of [
      ["GITHUB_ACTIONS", previous.actions],
      ["npm_lifecycle_event", previous.script],
    ] as const)
      if (value === undefined) delete process.env[name];
      else process.env[name] = value;
  });
  process.env["npm_lifecycle_event"] = "test:delivery";
  process.env["GITHUB_ACTIONS"] = "true";
  assert.deepEqual(await collect(githubTestReporter(events(Pass, Failure))), [
    "::error file=scripts/delivery/x.test.ts,line=7,title=Test delivery failed::runner stops: expected 1%0Aactual 2\n",
  ]);
  delete process.env["GITHUB_ACTIONS"];
  assert.deepEqual(await collect(githubTestReporter(events(Pass, Failure))), []);
});
