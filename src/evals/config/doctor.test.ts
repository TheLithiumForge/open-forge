import assert from "node:assert/strict";
import { test } from "node:test";
import { doctor, MinimumNode, ToolPackages, type DoctorServices } from "./doctor.ts";
import { ImageVersions } from "../execution/containers/docker-image.ts";

const services: DoctorServices = { node: `${MinimumNode}.0.0`, docker: async () => true, signIns: () => ({ claude: true, codex: true }), digests: () => [],
  version: async (name) => name === ToolPackages.claude ? ImageVersions.claude : name === ToolPackages.codex ? ImageVersions.codex : ImageVersions.openForge };

test("doctor checks prerequisites through fakes without invoking Docker or model tools", async () => {
  assert.ok((await doctor(services)).every((check) => check.passed));
  const missing = await doctor({ ...services, node: `${MinimumNode - 1}.0.0`, docker: async () => false, digests: () => { throw new Error(); } });
  assert.equal(missing.filter((check) => !check.passed).length, 3);
});
