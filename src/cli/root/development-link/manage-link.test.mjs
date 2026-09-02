import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";

import { finishLinkManager, runLinkManager } from "./manage-link.mjs";

const packageName = "@thelithiumforge/open-forge-development-link";
const packageDirectoryArgument = "./src/cli/root/development-link";
const stateProtectionArguments = ["--no-save", "--no-package-lock"];

test("Link appends invariant protection after caller flags from the repository root", () => {
  const forwardedArguments = ["--dry-run", "--save", "--package-lock"];
  const runtime = createRuntime("link", forwardedArguments);
  const invocations = [];

  const completion = runLinkManager(runtime, capture(invocations, [{ status: 0, signal: null }]));

  assert.deepEqual(completion, { kind: "exit", exitCode: 0 });
  assert.deepEqual(invocations, [
    {
      executable: runtime.nodeExecutable,
      argumentsReceived: [runtime.npmCli, "link", packageDirectoryArgument, ...forwardedArguments, ...stateProtectionArguments],
      options: expectedOptions(runtime),
    },
  ]);
});

test("The root development script invokes the repository-local npm bin", () => {
  const rootPackage = JSON.parse(readFileSync(new URL("../../../../package.json", import.meta.url), "utf8"));

  assert.equal(rootPackage.scripts["cli:dev"], "open-forge-dev");
});

test("Unlink removes the local link before the global package link", () => {
  const forwardedArguments = ["--dry-run", "--save", "--package-lock"];
  const runtime = createRuntime("unlink", forwardedArguments);
  const invocations = [];

  const completion = runLinkManager(
    runtime,
    capture(invocations, [
      { status: 0, signal: null },
      { status: 0, signal: null },
    ]),
  );

  assert.deepEqual(completion, { kind: "exit", exitCode: 0 });
  assert.deepEqual(
    invocations.map((invocation) => invocation.argumentsReceived),
    [
      [runtime.npmCli, "unlink", packageName, ...forwardedArguments, ...stateProtectionArguments],
      [runtime.npmCli, "unlink", "--global", packageName, ...forwardedArguments, ...stateProtectionArguments],
    ],
  );
  assert.deepEqual(
    invocations.map((invocation) => invocation.options),
    [expectedOptions(runtime), expectedOptions(runtime)],
  );
});

test("A failed local unlink stops before global cleanup and preserves the exact exit", () => {
  const runtime = createRuntime("unlink");
  const invocations = [];

  const completion = runLinkManager(runtime, capture(invocations, [{ status: 17, signal: null }]));

  assert.deepEqual(completion, { kind: "exit", exitCode: 17 });
  assert.equal(invocations.length, 1);
});

test("A failed global unlink preserves its exact exit after local cleanup", () => {
  const runtime = createRuntime("unlink");
  const invocations = [];

  const completion = runLinkManager(
    runtime,
    capture(invocations, [
      { status: 0, signal: null },
      { status: 29, signal: null },
    ]),
  );

  assert.deepEqual(completion, { kind: "exit", exitCode: 29 });
  assert.equal(invocations.length, 2);
});

test("A spawn error fails clearly through stderr completion", () => {
  const completion = runLinkManager(createRuntime("link"), () => ({
    status: null,
    signal: null,
    error: new Error("spawn denied"),
  }));
  const messages = [];

  const exitCode = finishLinkManager(
    completion,
    (message) => messages.push(message),
    () => assert.fail("A spawn error must not send a signal."),
  );

  assert.equal(exitCode, 1);
  assert.deepEqual(messages, ["Unable to run npm link: spawn denied\n"]);
});

test("An npm signal is mirrored without starting a later command", () => {
  const runtime = createRuntime("unlink");
  const invocations = [];
  const completion = runLinkManager(runtime, capture(invocations, [{ status: null, signal: "SIGTERM" }]));
  const signals = [];

  const exitCode = finishLinkManager(
    completion,
    () => assert.fail("A mirrored signal must not write an error."),
    (signal) => signals.push(signal),
  );

  assert.equal(exitCode, undefined);
  assert.deepEqual(signals, ["SIGTERM"]);
  assert.equal(invocations.length, 1);
});

function createRuntime(action, forwardedArguments = []) {
  return {
    action,
    forwardedArguments,
    nodeExecutable: "/runtime/node",
    npmCli: "/runtime/npm-cli.js",
    repositoryRoot: "/workspace/open-forge",
    environment: { EXAMPLE: "value" },
  };
}

function capture(invocations, results) {
  return (executable, argumentsReceived, options) => {
    invocations.push({ executable, argumentsReceived, options });
    return results[invocations.length - 1];
  };
}

function expectedOptions(runtime) {
  return {
    cwd: runtime.repositoryRoot,
    env: runtime.environment,
    shell: false,
    stdio: "inherit",
  };
}
