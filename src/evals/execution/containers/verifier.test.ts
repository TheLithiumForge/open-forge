import assert from "node:assert/strict";
import { test } from "node:test";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { AuthVariables, CodexAuthFile } from "../../config/auth.ts";
import { Isolation } from "../../config/choices.ts";
import { buildProject } from "../../grading/build.ts";
import { verifyInContainer, VerifierCommands, VerifierEnvironment } from "./verifier.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "./settings.ts";
import { assertStrictStage, containerFixture, FakeResult, FixtureFiles, FixtureText, readTree } from "./test-docker.ts";

const TimeoutMs = 10_000;

test("verifier installs without scripts, disconnects, builds as a separate account and copies output", async () => {
  const fixture = await containerFixture();
  const { client, space } = fixture;
  try {
    await writeFile(join(space.work, FixtureFiles.manifest), JSON.stringify({ scripts: { build: "build" } }));
    await writeFile(join(space.work, FixtureFiles.lock), JSON.stringify({ packages: { "": {}, example: {} } }));
    await writeFile(join(space.work, FixtureFiles.config), JSON.stringify({ include: [] }));
    for (const folder of [".codex", ".claude", "node_modules"]) {
      await mkdir(join(space.work, folder));
      await writeFile(join(space.work, folder, CodexAuthFile), FixtureText.before);
    }
    for (const name of [".env.local", ".npmrc"]) await writeFile(join(space.work, name), FixtureText.before);
    client.hook = async (args) => {
      if (args.includes("ci")) {
        const name = client.names[0];
        assert.ok(name);
        const files = Object.values(await readTree(client.path(name, ContainerPaths.work)));
        assert.equal(files.includes(FixtureText.before), false);
      }
      if (args.includes("build")) {
        const name = client.names[0];
        assert.ok(name);
        const page = client.path(name, `${ContainerPaths.work}/${FixtureFiles.built}`);
        await mkdir(dirname(page), { recursive: true });
        await writeFile(page, FixtureText.built);
      }
      return undefined;
    };
    const result = await buildProject(Isolation.docker, space, TimeoutMs, { docker: client });
    assert.equal(result.passed, true);
    assert.equal(result.typecheck, true);
    assert.equal(result.packages, 1);
    const name = assertStrictStage(client, StageKind.verifier);
    const execs = client.calls.filter((call) => call.args[0] === "exec" && !call.args.includes("node"));
    assert.deepEqual(execs.map((call) => call.args), [
      ["exec", "-i", "--user", ContainerUsers.root, "--workdir", ContainerPaths.work, "--env", `HOME=${ContainerPaths.installHome}`, name, ...VerifierCommands.ci],
      ...[VerifierCommands.build, [...VerifierCommands.typecheck, FixtureFiles.config]].map((command) => ["exec", "-i", "--user", ContainerUsers.build, "--workdir", ContainerPaths.work,
        ...Object.entries(VerifierEnvironment).flatMap(([key, value]) => ["--env", `${key}=${value}`]), name, ...command]),
    ]);
    const disconnect = client.calls.findIndex((call) => call.args[0] === "network" && call.args[1] === "disconnect");
    assert.ok(disconnect > client.calls.indexOf(execs[0]!));
    assert.ok(disconnect < client.calls.indexOf(execs[1]!));
    assert.equal(await readFile(join(space.work, FixtureFiles.built), "utf8"), FixtureText.built);
    for (const call of client.calls) {
      assert.equal(call.options.env, undefined);
      for (const variable of Object.values(AuthVariables)) assert.equal(call.args.some((arg) => arg.includes(variable)), false);
      assert.equal(call.args.some((arg) => arg.endsWith(CodexAuthFile)), false);
    }
  } finally { await fixture.dispose(); }
});

for (const failedCommand of ["install", "build"]) {
  test(`verifier removes its container after ${failedCommand} fails`, async () => {
    const fixture = await containerFixture();
    try {
      fixture.client.hook = async (args) => args.includes(failedCommand) ? { ...FakeResult, code: 1 } : undefined;
      if (failedCommand === "install") await assert.rejects(verifyInContainer(fixture.space, { docker: fixture.client, timeoutMs: TimeoutMs }));
      else assert.equal((await verifyInContainer(fixture.space, { docker: fixture.client, timeoutMs: TimeoutMs })).build.code, 1);
      assertStrictStage(fixture.client, StageKind.verifier);
    } finally { await fixture.dispose(); }
  });
}
