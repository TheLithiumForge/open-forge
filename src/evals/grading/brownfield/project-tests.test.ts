import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { containerFixture, FakeResult } from "../../execution/containers/test-docker.ts";
import { ContainerPaths, ContainerUsers } from "../../execution/containers/settings.ts";
import { writeNew } from "../../materialize/files.ts";
import { ProjectTestCommands, projectTestPassed, testProject } from "./project-tests.ts";

test("project tests run as the build account offline and collect their ignored output through the verifier", async (t) => {
  const fixture = await containerFixture(); t.after(fixture.dispose);
  const generated = ".art/report.txt";
  fixture.client.hook = async (args) => {
    if (args[0] !== "exec") return undefined;
    if (args.slice(-ProjectTestCommands.unit.length).join(" ") === ProjectTestCommands.unit.join(" ")) return { ...FakeResult, code: 1, stdout: "1 failed" };
    if (args.slice(-ProjectTestCommands.browser.length).join(" ") === ProjectTestCommands.browser.join(" ")) {
      const name = fixture.client.names.at(-1); assert.ok(name);
      await writeNew(fixture.client.path(name, ContainerPaths.work), generated, "Fixture ignored browser output.");
      return { ...FakeResult, stdout: "1 passed" };
    }
    return undefined;
  };
  const results = await testProject(fixture.space, { docker: fixture.client, timeoutMs: 10_000 });
  assert.equal(projectTestPassed(results.unit), false);
  assert.equal(projectTestPassed(results.browser), true);
  assert.ok(await readFile(join(fixture.space.work, generated)));
  const calls = fixture.client.calls.map((call) => call.args);
  const offline = calls.findIndex((args) => args[0] === "network" && args[1] === "disconnect");
  for (const command of Object.values(ProjectTestCommands)) {
    const index = calls.findIndex((args) => args.slice(-command.length).join(" ") === command.join(" "));
    assert.ok(index > offline);
    assert.ok(calls[index]?.includes(ContainerUsers.build));
  }
  assert.equal(projectTestPassed({ ...FakeResult, stdout: "No tests" }), false);
  assert.equal(projectTestPassed({ ...FakeResult, stdout: "1 passed", timedOut: true }), false);
});
