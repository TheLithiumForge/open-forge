import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { createManifest, verifyManifest } from "../manifest.ts";

const sha = "1234567890abcdef1234567890abcdef12345678";
const version = "0.1.0-beta.1";
const rid = "linux-x64";

test("artifact qualification rejects stale identity, incomplete closures and changed native bytes", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-delivery-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const native = "native/OpenForge.Cli";
  const closure = "managed/unit.dll";
  mkdirSync(join(root, "native"));
  mkdirSync(join(root, "managed"));
  writeFileSync(join(root, native), "native candidate");
  writeFileSync(join(root, closure), "managed suite");
  const manifest = createManifest(root, { sha, version, rid }, [native], [closure]);
  verifyManifest(root, manifest, { sha, version, rid });
  assert.throws(() => verifyManifest(root, manifest, { sha: "a".repeat(40), version, rid }));
  assert.throws(() => verifyManifest(root, manifest, { sha, version: "0.1.0-beta.2", rid }));
  assert.throws(() => verifyManifest(root, manifest, { sha, version, rid: "linux-arm64" }));
  assert.throws(() => createManifest(root, { sha, version, rid }, [native], ["missing.dll"]));
  rmSync(join(root, closure));
  assert.throws(() => verifyManifest(root, manifest, { sha, version, rid }));
  writeFileSync(join(root, closure), "managed suite");
  writeFileSync(join(root, native), "changed candidate");
  assert.throws(() => verifyManifest(root, manifest, { sha, version, rid }));
});
