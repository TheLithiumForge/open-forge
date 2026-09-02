import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import path from "node:path";
import { test } from "node:test";
import { pathToFileURL } from "node:url";

import { configurationEnvironmentVariable, finishDevelopmentLink, runDevelopmentLink } from "./launcher.mjs";

const developmentLinkRelativePath = path.join("src", "cli", "root", "development-link");

test("Linux defaults to the Debug publication and forwards the process contract", (context) => {
  const fixture = createFixture(context, "linux", "Debug");
  const environment = { EXAMPLE: "value" };
  const argumentsToForward = ["route", "list", "value with spaces"];
  const currentDirectory = path.join(fixture.repositoryRoot, "caller");
  let invocation;

  const completion = runDevelopmentLink(
    {
      arguments: argumentsToForward,
      launcherUrl: fixture.launcherUrl,
      platform: "linux",
      environment,
      currentDirectory,
    },
    (executable, argumentsReceived, options) => {
      invocation = { executable, argumentsReceived, options };
      return { status: 23, signal: null };
    },
  );

  assert.deepEqual(completion, { kind: "exit", exitCode: 23 });
  assert.equal(invocation.executable, fixture.executable);
  assert.equal(invocation.argumentsReceived, argumentsToForward);
  assert.equal(invocation.options.cwd, currentDirectory);
  assert.equal(invocation.options.env, environment);
  assert.equal(invocation.options.shell, false);
  assert.equal(invocation.options.stdio, "inherit");
});

test("Windows selects the Release executable when explicitly configured", (context) => {
  const fixture = createFixture(context, "win32", "Release");
  let executableReceived;

  const completion = runDevelopmentLink(
    {
      arguments: [],
      launcherUrl: fixture.launcherUrl,
      platform: "win32",
      environment: { [configurationEnvironmentVariable]: "Release" },
      currentDirectory: fixture.repositoryRoot,
    },
    (executable) => {
      executableReceived = executable;
      return { status: 0, signal: null };
    },
  );

  assert.deepEqual(completion, { kind: "exit", exitCode: 0 });
  assert.equal(executableReceived, fixture.executable);
  assert.match(executableReceived, /open-forge-dev\.exe$/u);
});

test("Invalid configurations fail before process creation", (context) => {
  const fixture = createFixture(context, "linux", "Debug");

  for (const configuration of ["", "debug", "Profile"]) {
    let spawned = false;
    const completion = runDevelopmentLink(
      {
        arguments: [],
        launcherUrl: fixture.launcherUrl,
        platform: "linux",
        environment: { [configurationEnvironmentVariable]: configuration },
        currentDirectory: fixture.repositoryRoot,
      },
      () => {
        spawned = true;
        return { status: 0, signal: null };
      },
    );

    assertFailure(completion, /must be exactly "Debug" or "Release"/u);
    assert.equal(spawned, false);
  }
});

test("Unsupported platforms fail before inspecting a publication", (context) => {
  const fixture = createFixture(context, "linux", "Debug");

  const completion = runDevelopmentLink({
    arguments: [],
    launcherUrl: fixture.launcherUrl,
    platform: "darwin",
    environment: {},
    currentDirectory: fixture.repositoryRoot,
  });

  assertFailure(completion, /supports only Node platforms "linux" and "win32"/u);
});

test("A missing publication fails without spawning", (context) => {
  const fixture = createFixture(context, "linux", "Debug", false);
  let spawned = false;

  const completion = runDevelopmentLink(
    {
      arguments: [],
      launcherUrl: fixture.launcherUrl,
      platform: "linux",
      environment: {},
      currentDirectory: fixture.repositoryRoot,
    },
    () => {
      spawned = true;
      return { status: 0, signal: null };
    },
  );

  assertFailure(completion, /does not exist.*Build the Debug configuration/u);
  assert.equal(spawned, false);
});

test("A non-file publication fails without spawning", (context) => {
  const fixture = createFixture(context, "linux", "Debug", false);
  mkdirSync(fixture.executable, { recursive: true });
  let spawned = false;

  const completion = runDevelopmentLink(
    {
      arguments: [],
      launcherUrl: fixture.launcherUrl,
      platform: "linux",
      environment: {},
      currentDirectory: fixture.repositoryRoot,
    },
    () => {
      spawned = true;
      return { status: 0, signal: null };
    },
  );

  assertFailure(completion, /is not a file.*Build the Debug configuration/u);
  assert.equal(spawned, false);
});

test("Spawn failures are reported through stderr completion", (context) => {
  const fixture = createFixture(context, "linux", "Debug");
  const completion = runDevelopmentLink(
    {
      arguments: [],
      launcherUrl: fixture.launcherUrl,
      platform: "linux",
      environment: {},
      currentDirectory: fixture.repositoryRoot,
    },
    () => ({ status: null, signal: null, error: new Error("spawn denied") }),
  );
  const messages = [];

  const exitCode = finishDevelopmentLink(
    completion,
    (message) => messages.push(message),
    () => assert.fail("A spawn failure must not send a signal."),
  );

  assert.equal(exitCode, 1);
  assert.deepEqual(messages, ["Unable to start managed development executable: spawn denied\n"]);
});

test("Child signals are mirrored without inventing an exit code", (context) => {
  const fixture = createFixture(context, "linux", "Debug");
  const completion = runDevelopmentLink(
    {
      arguments: [],
      launcherUrl: fixture.launcherUrl,
      platform: "linux",
      environment: {},
      currentDirectory: fixture.repositoryRoot,
    },
    () => ({ status: null, signal: "SIGTERM" }),
  );
  const signals = [];

  const exitCode = finishDevelopmentLink(
    completion,
    () => assert.fail("A mirrored signal must not write an error."),
    (signal) => signals.push(signal),
  );

  assert.equal(exitCode, undefined);
  assert.deepEqual(signals, ["SIGTERM"]);
});

test("A signal mirroring limit fails clearly on stderr", () => {
  const messages = [];

  const exitCode = finishDevelopmentLink(
    { kind: "signal", signal: "SIGTERM" },
    (message) => messages.push(message),
    () => {
      throw new Error("signal unavailable");
    },
  );

  assert.equal(exitCode, 1);
  assert.deepEqual(messages, ["Unable to mirror managed development executable signal SIGTERM: signal unavailable\n"]);
});

function createFixture(context, platform, configuration, includeExecutable = true) {
  const repositoryRoot = mkdtempSync(path.join(tmpdir(), "open-forge-development-link-"));
  context.after(() => rmSync(repositoryRoot, { force: true, recursive: true }));

  const launcher = path.join(repositoryRoot, developmentLinkRelativePath, "open-forge-dev.mjs");
  mkdirSync(path.dirname(launcher), { recursive: true });
  writeFileSync(launcher, "");

  const suffix = platform === "win32" ? ".exe" : "";
  const executable = path.join(repositoryRoot, "artifacts", "publish", "open-forge-dev", configuration, `open-forge-dev${suffix}`);

  if (includeExecutable) {
    mkdirSync(path.dirname(executable), { recursive: true });
    writeFileSync(executable, "");
  }

  return {
    repositoryRoot,
    launcherUrl: pathToFileURL(launcher),
    executable,
  };
}

function assertFailure(completion, messagePattern) {
  const messages = [];
  const exitCode = finishDevelopmentLink(
    completion,
    (message) => messages.push(message),
    () => assert.fail("A failure must not send a signal."),
  );

  assert.equal(completion.kind, "failure");
  assert.equal(exitCode, 1);
  assert.equal(messages.length, 1);
  assert.match(messages[0], messagePattern);
  assert.match(messages[0], /\n$/u);
}
