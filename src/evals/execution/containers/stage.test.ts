import assert from "node:assert/strict";
import { test } from "node:test";
import { ContainerStage, CopyInOwner, DockerNetworkMessages, OverflowSubnets, StageLabel } from "./stage.ts";
import { containerLimits, ContainerTimeouts, ContainerUsers, DefaultContainerLimits, StageKind } from "./settings.ts";
import { evalImageTag } from "./docker-image.ts";
import { assertStrictStage, containerFixture, FakeResult } from "./test-docker.ts";

const TimeoutMs = 10_000;
const TestLimits = { cpus: 1.5, memory: "2g", processes: 64 };

test("a stage's user is root and files are copied in with its owner, so they belong to root from a Linux host too", async () => {
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  try {
    await stage.open(evalImageTag());
    await stage.copyIn(fixture.space.work, "/input");
    await stage.remove();
    const created = fixture.client.calls.find((call) => call.args[0] === "create")?.args ?? [];
    assert.equal(created[created.indexOf("--user") + 1], ContainerUsers.root);
    assert.equal(fixture.client.calls.find((call) => call.args[0] === "cp")?.args.at(-1), CopyInOwner);
  } finally { await fixture.dispose(); }
});

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

test("cleanup counts a container or network that's already gone as removed", async () => {
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  try {
    await stage.open(evalImageTag());
    fixture.client.hook = async (args) => args[0] === "rm" ? { ...FakeResult, code: 1, stderr: "Error response from daemon: No such container: fixture" }
      : args[0] === "network" && args[1] === "rm" ? { ...FakeResult, code: 1, stderr: "Error response from daemon: network fixture-net not found" } : undefined;
    await stage.remove();
    assert.ok(fixture.client.calls.some((call) => call.args[0] === "network" && call.args[1] === "rm"));
  } finally { await fixture.dispose(); }
});

test("a stage that can't open reports that failure, not the cleanup failing after it", async () => {
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  fixture.client.hook = async (args) => args[0] === "network" && ["create", "rm"].includes(args[1] ?? "") ? { ...FakeResult, code: 1 } : undefined;
  try {
    await assert.rejects(stage.open(evalImageTag()), (error) => !(error instanceof AggregateError));
  } finally { await fixture.dispose(); }
});

test("stage networks carry the eval label, and leftover labelled networks are pruned once per process", async () => {
  const fixture = await containerFixture();
  try {
    for (let index = 0; index < 2; index += 1) {
      const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
      await stage.open(evalImageTag());
      await stage.remove();
    }
    const network = (verb: string) => fixture.client.calls.filter((call) => call.args[0] === "network" && call.args[1] === verb);
    for (const call of network("create")) assert.deepEqual(call.args.slice(2, 4), ["--label", StageLabel]);
    assert.ok(network("prune").length <= 1);
  } finally { await fixture.dispose(); }
});

test("once Docker's default pools are full, a stage network takes a free subnet from the overflow range", async () => {
  const fixture = await containerFixture();
  let taken = 0;
  fixture.client.hook = async (args) => {
    if (args[0] !== "network" || args[1] !== "create") return undefined;
    if (!args.includes("--subnet")) return { ...FakeResult, code: 1, stderr: `Error response from daemon: ${DockerNetworkMessages.poolsExhausted}` };
    taken += 1;
    return taken === 1 ? { ...FakeResult, code: 1, stderr: `Error response from daemon: ${DockerNetworkMessages.subnetTaken}` } : undefined;
  };
  try {
    const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
    await stage.open(evalImageTag());
    await stage.remove();
    const subnets = fixture.client.calls.filter((call) => call.args[0] === "network" && call.args[1] === "create").map((call) => call.args[call.args.indexOf("--subnet") + 1]);
    assert.equal(subnets.length, 3);
    const [, first, second] = subnets;
    assert.ok(first?.startsWith(`${OverflowSubnets.prefix}.`) && second?.startsWith(`${OverflowSubnets.prefix}.`));
    assert.notEqual(first, second);
  } finally { await fixture.dispose(); }
});

test("copying a project out of a container gets the copy limit, since it includes installed packages", async () => {
  const fixture = await containerFixture();
  const stage = new ContainerStage({ docker: fixture.client, timeoutMs: TimeoutMs });
  try {
    await stage.open(evalImageTag());
    await stage.copyOut("/work/.", fixture.space.work);
    await stage.remove();
    const copies = fixture.client.calls.filter((call) => call.args[0] === "cp");
    assert.deepEqual(copies.map((call) => call.options.timeoutMs), [ContainerTimeouts.copyMs]);
  } finally { await fixture.dispose(); }
});

test("resource limits default and reject invalid values", () => {
  assert.deepEqual(containerLimits(), DefaultContainerLimits);
  for (const cpus of [0, -1, NaN, Infinity]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, cpus }));
  for (const processes of [0, -1, 0.5]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, processes }));
  for (const memory of ["", "0", "-1g", "2g --privileged"]) assert.throws(() => containerLimits({ ...DefaultContainerLimits, memory }));
});
