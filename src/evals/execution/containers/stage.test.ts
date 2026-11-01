import assert from "node:assert/strict";
import { test } from "node:test";
import { ContainerStage } from "./stage.ts";
import { containerLimits, DefaultContainerLimits, StageKind } from "./settings.ts";
import { evalImageTag } from "./docker-image.ts";
import { assertStrictStage, containerFixture, FakeResult } from "./test-docker.ts";

const TimeoutMs = 10_000;
const TestLimits = { cpus: 1.5, memory: "2g", processes: 64 };

for (const command of ["create", "start", "cp", "exec"]) {
  test(`container is removed when Docker ${command} fails`, async () => {
    const fixture = await containerFixture();
    const { client } = fixture;
    const stage = new ContainerStage({ docker: client, timeoutMs: TimeoutMs });
    client.hook = async (args) => args[0] === command ? { ...FakeResult, code: 1 } : undefined;
    try {
      await assert.rejects(async () => {
        try {
          await stage.open(evalImageTag());
          await stage.copyIn(fixture.space.work, "/input");
          await stage.prepare();
        } finally { await stage.remove(); }
      });
      assertStrictStage(client, StageKind.agent);
    } finally { await fixture.dispose(); }
  });
}

test("abort kills the container and removal does not inherit the aborted signal", async () => {
  const fixture = await containerFixture();
  const controller = new AbortController();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs, signal: controller.signal, limits: TestLimits });
  try {
    await stage.open(evalImageTag());
    controller.abort();
    await assert.rejects(stage.required(["exec"]));
    await stage.remove();
    assertStrictStage(fixture.client, StageKind.agent, TestLimits);
    for (const call of fixture.client.calls.filter((call) => ["kill", "rm"].includes(call.args[0] ?? ""))) assert.equal(call.options.signal, undefined);
    assert.equal(fixture.client.calls.filter((call) => call.args[0] === "kill").length, 1);
  } finally { await fixture.dispose(); }
});

test("stage deadline kills the container without real sleeps", async (context) => {
  context.mock.timers.enable({ apis: ["setTimeout", "Date"] });
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  try {
    await stage.open(evalImageTag());
    context.mock.timers.tick(TimeoutMs);
    await assert.rejects(stage.required(["exec"]));
    await stage.remove();
    assertStrictStage(fixture.client, StageKind.agent);
    assert.ok(fixture.client.calls.some((call) => call.args[0] === "kill"));
  } finally { context.mock.timers.reset(); await fixture.dispose(); }
});

test("cleanup attempts the network even when container removal fails", async () => {
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  try {
    await stage.open(evalImageTag());
    fixture.client.hook = async (args) => args[0] === "rm" ? { ...FakeResult, code: 1 } : undefined;
    await assert.rejects(stage.remove(), AggregateError);
    assertStrictStage(fixture.client, StageKind.agent);
  } finally { await fixture.dispose(); }
});

test("resource limits default and reject invalid values", () => {
  assert.deepEqual(containerLimits(), DefaultContainerLimits);
  for (const cpus of [0, -1, NaN, Infinity]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, cpus }));
  for (const processes of [0, -1, 0.5]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, processes }));
  for (const memory of ["", "0", "-1g", "2g --privileged"]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, memory }));
});
