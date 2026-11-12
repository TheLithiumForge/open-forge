import { randomUUID } from "node:crypto";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import type { TestContext } from "node:test";
import { fileURLToPath } from "node:url";

const run = randomUUID();
const expected = fileURLToPath(new URL("./__snapshots__/", import.meta.url));
const received = fileURLToPath(new URL(`../../../../artifacts/command-output-snapshots/${run}/PyPI/`, import.meta.url));
const serialize = (value: unknown) => JSON.stringify(value, null, 2) + "\n";

export function fileSnapshot(context: TestContext, group: string, value: unknown): void {
  const name = `${group}.txt`;
  const receipt = join(received, group, name);
  mkdirSync(dirname(receipt), { recursive: true });
  mkdirSync(expected, { recursive: true });
  writeFileSync(receipt, serialize(value));
  context.assert.fileSnapshot(value, join(expected, name), { serializers: [serialize] });
}
