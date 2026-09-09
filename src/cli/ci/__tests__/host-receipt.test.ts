import assert from "node:assert/strict";
import { test } from "node:test";

import { qualifyHost } from "../host-receipt.ts";

const host = {
  rid: "linux-x64",
  platform: "linux",
  architecture: "x64",
  node: "v24.19.0",
  bun: "1.3.14",
  bunArchitecture: "x64",
  sdk: "10.0.111",
  sdkArchitecture: "x64",
  dotnetRoot: "/owned/dotnet",
  dotnetExecutable: "/owned/dotnet/dotnet",
  installationDirectory: "/owned/dotnet",
};

test("Unit: producer host accepts the exact native isolated tools", () => {
  assert.deepEqual(qualifyHost(host), host);
});

test("Unit: producer host rejects a foreign native architecture", () => {
  assert.throws(() => qualifyHost({ ...host, architecture: "arm64" }), { message: "host-target-mismatch" });
});

test("Unit: producer host rejects changed SDK and architecture", () => {
  for (const changed of [{ sdk: "10.0.200" }, { sdkArchitecture: "arm64" }, { bunArchitecture: "arm64" }, { node: "v24.20.0" }]) {
    assert.throws(() => qualifyHost({ ...host, ...changed }), { message: "host-tool-mismatch" });
  }
});

test("Unit: producer host rejects a global SDK selection", () => {
  assert.throws(() => qualifyHost({ ...host, dotnetExecutable: "/global/dotnet/dotnet" }), { message: "host-tool-mismatch" });
});
